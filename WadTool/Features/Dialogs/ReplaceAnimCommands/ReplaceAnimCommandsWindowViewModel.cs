#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TombLib.Graphics;
using TombLib.Wad;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.ReplaceAnimCommands;

/// <summary>
/// A single search hit shown in the results grid (one matching anim command).
/// </summary>
public partial class ResultRow : ObservableObject
{
    [ObservableProperty] private bool _selected;

    public int AnimIndex { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// View model of the "Find &amp; replace anim commands" dialog
/// (WPF port of the legacy <c>FormReplaceAnimCommands</c>).
/// </summary>
public partial class ReplaceAnimCommandsWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly AnimationEditor _editor;
    private readonly ILocalizationService _localizationService;

    // Snapshot of the search command taken when the search was run, so later edits in the
    // "Search for" editor don't desync the results grid (mirrors the legacy _backupCommand).
    private WadAnimCommand? _backupCommand;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _statusText = string.Empty;

    /// <summary>The animation editor this dialog operates on. Used by the view to initialize the WPF editors.</summary>
    public AnimationEditor Editor => _editor;

    /// <summary>
    /// Command edited by the hosted "Search for" <see cref="AnimCommandEditor"/>. The previous
    /// control mutates this instance in place, so it is always current when searching.
    /// </summary>
    public WadAnimCommand FindAnimCommand { get; }

    /// <summary>
    /// Command edited by the hosted "Replace with" <see cref="AnimCommandEditor"/> (mutated in place, see <see cref="FindAnimCommand"/>).
    /// </summary>
    public WadAnimCommand ReplaceWithAnimCommand { get; }

    /// <summary>True once a replacement or deletion was made; the caller marks the wad as unsaved.</summary>
    public bool EditingWasDone { get; private set; }

    public ObservableCollection<ResultRow> Rows { get; } = new();

    public ReplaceAnimCommandsWindowViewModel(
        AnimationEditor editor,
        WadAnimCommand? refCommand = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        FindAnimCommand = refCommand ?? new WadAnimCommand { Type = WadAnimCommandType.SetPosition };
        ReplaceWithAnimCommand = new WadAnimCommand { Type = WadAnimCommandType.SetPosition };
    }

    private bool HasResults() => Rows.Count > 0;
    private bool HasSelectedRows() => Rows.Any(row => row.Selected);

    [RelayCommand]
    private void Find() => RunSearch(true);

    [RelayCommand(CanExecute = nameof(HasSelectedRows))]
    private void Replace() => ReplaceOrDelete(false);

    [RelayCommand(CanExecute = nameof(HasSelectedRows))]
    private void Delete() => ReplaceOrDelete(true);

    [RelayCommand(CanExecute = nameof(HasResults))]
    private void SelectAll() => SelectOrDeselectAll(true);

    [RelayCommand(CanExecute = nameof(HasResults))]
    private void DeselectAll() => SelectOrDeselectAll(false);

    [RelayCommand]
    private void Close() => DialogResult = false;

    private void SelectOrDeselectAll(bool select)
    {
        foreach (ResultRow row in Rows)
            row.Selected = select;
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResultRow.Selected))
            UpdateCommandStates();
    }

    // Equivalent of the legacy UpdateUI(): button enabled state follows search results / checked rows.
    private void UpdateCommandStates()
    {
        SelectAllCommand.NotifyCanExecuteChanged();
        DeselectAllCommand.NotifyCanExecuteChanged();
        ReplaceCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    private void RunSearch(bool newSearch)
    {
        // Reset previous search's state...
        Rows.Clear();

        // Store the search command in case user later changes it in the editor.
        if (newSearch)
            _backupCommand = FindAnimCommand.Clone();

        var collectedAnims = new List<int>();

        for (int i = 0; i < _editor.Animations.Count; i++)
        {
            AnimationNode anim = _editor.Animations[i];

            foreach (WadAnimCommand ac in anim.WadAnimation.AnimCommands)
                if (WadAnimCommand.DistinctiveEquals(ac, _backupCommand, false))
                {
                    if (!collectedAnims.Contains(i))
                        collectedAnims.Add(i);

                    var row = new ResultRow
                    {
                        AnimIndex = anim.Index,
                        Description = _localizationService.Format("ResultDescription", anim.WadAnimation.Name, ac)
                    };

                    row.PropertyChanged += OnRowPropertyChanged;
                    Rows.Add(row);
                }
        }

        UpdateCommandStates();

        if (newSearch)
            StatusText = _localizationService.Format("StatusSearchFinished",
                FormatCount(Rows.Count, "MatchSingular", "MatchPlural"),
                FormatCount(collectedAnims.Count, "AnimationSingular", "AnimationPlural", pluralAboveOneOnly: true));
    }

    private void ReplaceOrDelete(bool delete)
    {
        // Enlist all animations which are pending for replacement
        var animsToUndo = new List<AnimationNode>();
        foreach (ResultRow row in Rows)
            if (row.Selected && !animsToUndo.Any(anim => anim.Index == row.AnimIndex))
                animsToUndo.Add(_editor.Animations[row.AnimIndex]);

        // Undo
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, animsToUndo);

        int count = 0;
        int animCount = 0;
        int actionCount = 0;
        bool alreadyFound;

        for (int i = 0; i < _editor.Animations.Count; i++)
        {
            // Collect animcommand indices to remove later in reverse order with RemoveAt.
            var indicesToDelete = new List<int>();

            for (int j = 0; j < _editor.Animations[i].WadAnimation.AnimCommands.Count; j++)
            {
                // NOTE: resetting the flag per command mirrors the legacy form exactly
                // (there animCount ends up equal to actionCount).
                alreadyFound = false;

                WadAnimCommand ac = _editor.Animations[i].WadAnimation.AnimCommands[j];
                if (WadAnimCommand.DistinctiveEquals(ac, _backupCommand, false))
                {
                    if (Rows[count].Selected)
                    {
                        if (!alreadyFound) // Increase counter for statistics
                        {
                            animCount++;
                            alreadyFound = true;
                        }

                        if (delete)
                            indicesToDelete.Add(j);
                        else
                        {
                            WadAnimCommand preparedCommand = ReplaceWithAnimCommand.Clone();

                            // Preserve frame number in frame-based animcommands
                            if (preparedCommand.FrameBased && ac.FrameBased)
                                preparedCommand.Parameter1 = ac.Parameter1;

                            _editor.Animations[i].WadAnimation.AnimCommands[j] = preparedCommand;
                        }
                        actionCount++; // Increase counter for statistics
                    }
                    count++;
                }
            }

            // Remove previously collected indices in reverse order.
            if (indicesToDelete.Count > 0)
                indicesToDelete
                    .OrderByDescending(a => a)
                    .ToList()
                    .ForEach(item => _editor.Animations[i].WadAnimation.AnimCommands.RemoveAt(item));
        }

        UpdateCommandStates();
        EditingWasDone = true;
        animsToUndo.ForEach(anim => _editor.Tool.AnimationEditorAnimationChanged(anim, false));

        StatusText = delete
            ? _localizationService.Format("StatusDeleteFinished",
                FormatCount(actionCount, "AnimCommandSingular", "AnimCommandPlural"),
                FormatCount(animCount, "AnimationSingular", "AnimationPlural"))
            : _localizationService.Format("StatusReplaceFinished",
                FormatCount(actionCount, "ReplacementSingular", "ReplacementPlural"),
                FormatCount(animCount, "AnimationSingular", "AnimationPlural"));

        // Run one more extra pass to show deselected results
        RunSearch(false);
    }

    /// <summary>
    /// Picks the singular or plural form of a localized "{0} thing(s)" fragment.
    /// <paramref name="pluralAboveOneOnly"/> mirrors the legacy search status, which used
    /// the singular form for both 0 and 1 animations.
    /// </summary>
    private string FormatCount(int count, string singularKey, string pluralKey, bool pluralAboveOneOnly = false)
    {
        bool singular = pluralAboveOneOnly ? count <= 1 : count == 1;
        return _localizationService.Format(singular ? singularKey : pluralKey, count);
    }
}

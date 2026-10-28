#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Types;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.StateChangesEditor;

/// <summary>
/// WPF counterpart of the legacy <c>FormStateChangesEditor</c>. Edits the state changes /
/// anim dispatches of one animation as a flat row list (one row per dispatch), saves every
/// committed cell edit straight back to the animation (with an undo entry, raising
/// <c>AnimationEditorAnimationChanged</c>), and restores the backed-up state changes on Cancel.
/// </summary>
public partial class StateChangesEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private StateChangeRow? _selectedRow;
    [ObservableProperty] private string _stateChangeAnnouncement = string.Empty;
    [ObservableProperty] private bool _isStateChangeAnnouncementVisible;

    public ObservableCollection<StateChangeRow> Rows { get; } = new();

    /// <summary>TEN-only columns (next high frame, blend frames, blend curve) are shown only for TombEngine wads.</summary>
    public bool IsTombEngine { get; }

    /// <summary>Set when the ctor received a fresh state change to append (legacy <c>_createdNew</c>); the window selects and reveals it on load.</summary>
    public bool CreatedNew { get; private set; }

    /// <summary>State changes built by the last save, like the legacy form's public <c>StateChanges</c> property.</summary>
    public List<WadStateChange>? StateChanges { get; private set; }

    /// <summary>Exposed so the window can persist its placement into the legacy config slots.</summary>
    public WadToolClass Tool => _editor.Tool;

    /// <summary>Raised when a blend curve cell was clicked; the window opens the BlendCurveEditor dialog (legacy <c>ShowBlendCurveEditor</c>).</summary>
    public event Action<StateChangeRow>? BlendCurveEditRequested;

    /// <summary>Raised when a row was added through the add button; the window scrolls it into view.</summary>
    public event Action<StateChangeRow>? RowAdded;

    private readonly AnimationEditor _editor;
    private readonly ILocalizationService _localization;

    private AnimationNode _animation;
    private List<WadStateChange> _backupStates = new();
    private bool _initializing;

    public StateChangesEditorWindowViewModel(AnimationEditor editor, AnimationNode animation,
        WadStateChange? newStateChange = null, ILocalizationService? localizationService = null)
    {
        _localization = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _editor = editor;
        _animation = animation;
        IsTombEngine = editor.Tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine;

        Rows.CollectionChanged += OnRowsChanged;
        Initialize(animation, newStateChange);

        _editor.Tool.EditorEventRaised += OnEditorEventRaised;
    }

    /// <summary>Counterpart of the legacy <c>Dispose</c> event unhook; called by the window on <c>Closed</c>.</summary>
    public void Detach() => _editor.Tool.EditorEventRaised -= OnEditorEventRaised;

    private void Initialize(AnimationNode animation, WadStateChange? newStateChange)
    {
        if (_initializing)
            return;

        _initializing = true;

        _animation = animation;

        _backupStates = new List<WadStateChange>();
        foreach (var sc in animation.WadAnimation.StateChanges)
            _backupStates.Add(sc.Clone());

        StateChangeAnnouncement = string.Empty;
        Rows.Clear();

        foreach (var sc in _animation.WadAnimation.StateChanges)
            foreach (var d in sc.Dispatches)
                Rows.Add(new StateChangeRow(this, sc.StateId, d.InFrame, d.OutFrame,
                                            d.NextAnimation, d.NextLowFrame, d.NextHighFrame, d.BlendFrames, d.BlendCurve));

        if (newStateChange is not null && newStateChange.Dispatches.Count == 1)
        {
            var d = newStateChange.Dispatches[0];
            Rows.Add(new StateChangeRow(this, newStateChange.StateId, d.InFrame, d.OutFrame,
                                        d.NextAnimation, d.NextLowFrame, d.NextHighFrame, d.BlendFrames, d.BlendCurve));
            CreatedNew = true;
        }

        _initializing = false;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent currentChangedEvent &&
            currentChangedEvent.Current != _animation)
        {
            currentChangedEvent.Previous.WadAnimation.StateChanges.Clear();
            currentChangedEvent.Previous.WadAnimation.StateChanges.AddRange(_backupStates);
            Initialize(currentChangedEvent.Current, null);
        }

        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent animationChangedEvent &&
            animationChangedEvent.Animation == _animation)
        {
            Initialize(animationChangedEvent.Animation, null);
        }

        if (obj is WadToolClass.AnimationEditorPlaybackEvent playbackEvent)
        {
            StateChangeAnnouncement = string.Empty;
            IsStateChangeAnnouncementVisible = playbackEvent.Playing && playbackEvent.Chained;
        }
    }

    private void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Legacy dgvStateChanges_UserDeletedRow -> SaveChanges(); covers both the delete
        // button and Del-key row deletion performed by the DataGrid itself.
        if (e.Action == NotifyCollectionChangedAction.Remove)
            SaveChanges();
    }

    private void SaveChanges()
    {
        if (_initializing)
            return;

        _initializing = true;

        // Update data
        StateChanges = new List<WadStateChange>();
        var tempDictionary = new Dictionary<int, WadStateChange>();
        foreach (var row in Rows)
        {
            if (!tempDictionary.ContainsKey(row.StateId))
                tempDictionary.Add(row.StateId, new WadStateChange());

            var sc = tempDictionary[row.StateId];
            sc.StateId = (ushort)row.StateId;

            var newDispatch = new WadAnimDispatch((ushort)row.LowFrame, (ushort)row.HighFrame,
                                                  (ushort)row.NextAnimation, (ushort)row.NextLowFrame)
            {
                NextHighFrame = (ushort)row.NextHighFrame,
                BlendFrames = (ushort)row.BlendFrames,
                BlendCurve = row.BlendCurve
            };

            sc.Dispatches.Add(newDispatch);
            tempDictionary[row.StateId] = sc;
        }
        StateChanges.AddRange(tempDictionary.Values.ToList());

        // Undo
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

        // Add the new state changes
        _animation.WadAnimation.StateChanges.Clear();
        _animation.WadAnimation.StateChanges.AddRange(StateChanges);

        // Update state in parent window
        _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        _initializing = false;
    }

    private void DiscardChanges()
    {
        if (_initializing)
            return;

        _initializing = true;

        _animation.WadAnimation.StateChanges.Clear();
        _animation.WadAnimation.StateChanges.AddRange(_backupStates);

        // Update state in parent window
        _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        _initializing = false;
    }

    [RelayCommand]
    private void Confirm()
    {
        SaveChanges();
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DiscardChanges();
        DialogResult = false;
    }

    [RelayCommand]
    private void Apply()
    {
        SaveChanges();
        Initialize(_animation, null);
    }

    /// <summary>Legacy <c>ChangeState</c>: plays the selected state change in chain mode.</summary>
    [RelayCommand]
    private void PlayStateChange()
    {
        if (SelectedRow is not StateChangeRow item)
            return;

        _editor.Tool.ChangeState(item.NextAnimation, item.NextLowFrame, item.LowFrame, item.HighFrame,
                                 item.BlendFrames, item.BlendCurve);

        StateChangeAnnouncement = _localization.Format("PendingStateChange", item.NextAnimation);
    }

    [RelayCommand]
    private void AddRow()
    {
        // Legacy dgvControls.CreateNewRow: zeroed row carrying the catalog name of state 0.
        var row = new StateChangeRow(this, 0, 0, 0, 0, 0, 0, 0, BezierCurve2.Linear);
        Rows.Add(row);
        SelectedRow = row;
        RowAdded?.Invoke(row);
    }

    [RelayCommand]
    private void DeleteRows(IList? selectedItems)
    {
        if (selectedItems is null)
            return;

        foreach (var row in selectedItems.OfType<StateChangeRow>().ToList())
            Rows.Remove(row);
    }

    /// <summary>Indices of the selected rows within <see cref="Rows"/>, in ascending order.</summary>
    private List<int> GetSelectedIndices(IList? selectedItems)
    {
        if (selectedItems is null)
            return new List<int>();

        return selectedItems.OfType<StateChangeRow>()
            .Select(row => Rows.IndexOf(row))
            .Where(index => index >= 0)
            .OrderBy(index => index)
            .ToList();
    }

    /// <summary>Mirrors <c>DarkDataGridViewControls.butUp_Click</c>.</summary>
    [RelayCommand]
    private void MoveRowsUp(IList? selectedItems)
    {
        List<int> indices = GetSelectedIndices(selectedItems);
        if (indices.Count == 0)
            return;

        int lastItemIndex = 0;
        foreach (int index in indices)
        {
            if (index == lastItemIndex++)
                continue;

            Rows.Move(index - 1, index);
        }
    }

    /// <summary>Mirrors <c>DarkDataGridViewControls.butDown_Click</c>.</summary>
    [RelayCommand]
    private void MoveRowsDown(IList? selectedItems)
    {
        List<int> indices = GetSelectedIndices(selectedItems);
        if (indices.Count == 0)
            return;

        indices.Reverse();

        int lastItemIndex = Rows.Count - 1;
        foreach (int index in indices)
        {
            if (index == lastItemIndex--)
                continue;

            Rows.Move(index + 1, index);
        }
    }

    [RelayCommand]
    private void EditBlendCurve(StateChangeRow? row)
    {
        if (row is not null)
            BlendCurveEditRequested?.Invoke(row);
    }

    /// <summary>Called back by the window after the BlendCurveEditor dialog was confirmed.</summary>
    internal void ApplyBlendCurve(StateChangeRow row, BezierCurve2 curve)
    {
        row.BlendCurve = curve;
        SaveChanges();
    }

    internal string GetStateName(int stateId)
        => TrCatalog.GetStateName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, (uint)stateId);

    internal string GetNextAnimationToolTip(int nextAnimation)
    {
        var anim = _editor.Animations.FirstOrDefault(a => a.Index == nextAnimation);
        return anim is not null
            ? anim.WadAnimation.Name
            : TrCatalog.GetAnimationName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, (uint)nextAnimation);
    }

    /// <summary>Legacy <c>dgvStateChanges_CellEndEdit</c>: every committed cell edit is saved immediately.</summary>
    internal void NotifyRowEdited() => SaveChanges();

    /// <summary>
    /// Mirrors the legacy <c>dgvStateChanges_CellValidating</c> rules: values are clamped to
    /// [0, short.MaxValue], optionally narrowed per column when
    /// <c>AnimationEditor_ClampStateChangeValues</c> is enabled.
    /// </summary>
    internal int ClampRowValue(StateChangeRow row, StateChangeRowField field, int proposedValue)
    {
        int limit = short.MaxValue;

        if (_editor.Tool.Configuration.AnimationEditor_ClampStateChangeValues)
        {
            switch (field)
            {
                case StateChangeRowField.NextAnimation:
                    limit = _editor.Animations.Count - 1;
                    break;

                case StateChangeRowField.LowFrame:
                    // Legacy quirk kept as-is: a zero high frame clamps against GetRealNumberOfFrames(0).
                    limit = row.HighFrame == 0 ? _editor.GetRealNumberOfFrames(row.HighFrame) : row.HighFrame;
                    break;

                case StateChangeRowField.HighFrame:
                    limit = _editor.GetRealNumberOfFrames();
                    break;

                case StateChangeRowField.NextLowFrame:
                    limit = row.NextHighFrame == 0 ? _editor.GetRealNumberOfFrames(row.NextAnimation) : row.NextHighFrame;
                    break;

                case StateChangeRowField.NextHighFrame:
                    limit = _editor.GetRealNumberOfFrames(row.NextAnimation);
                    break;
            }
        }

        return Math.Clamp(proposedValue, 0, Math.Max(0, limit));
    }
}

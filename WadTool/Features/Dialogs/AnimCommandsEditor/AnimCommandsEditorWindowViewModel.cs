#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.Graphics;
using TombLib.Services;
using TombLib.Wad;
using TombLib.WPF.Dialogs;

namespace WadTool.Features.Dialogs.AnimCommandsEditor;

/// <summary>
/// WPF counterpart of the legacy <c>FormAnimCommandsEditor</c>. Edits the anim commands of one
/// animation as a flat row list, mirroring the legacy "live preview" model: the animation's command
/// list is kept in sync with the grid on every change (raising <c>AnimationEditorAnimcommandChanged</c>),
/// a backup is taken on open and restored on Cancel, and OK/Apply commit through the undo manager.
/// </summary>
public partial class AnimCommandsEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly AnimationEditor _editor;
    private AnimationNode _animation;
    private List<WadAnimCommand> _backupCommands = new();
    private bool _initializing;
    private bool _closed;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyToAllCommand))]
    private AnimCommandRow? _selectedRow;

    public ObservableCollection<AnimCommandRow> Rows { get; } = new();

    /// <summary>Exposed so the window can initialize the WPF <c>AnimCommandEditor</c>.</summary>
    public AnimationEditor Editor => _editor;

    /// <summary>Exposed so the window can persist its placement into the legacy config slots.</summary>
    public WadToolClass Tool => _editor.Tool;

    /// <summary>Raised when the selected row changes; the window pushes the command into the hosted editor.</summary>
    public event Action<WadAnimCommand?>? SelectedCommandChanged;

    public AnimCommandsEditorWindowViewModel(AnimationEditor editor, AnimationNode animation,
        WadAnimCommand? commandToSelect = null)
    {
        _editor = editor;
        Initialize(animation);

        _editor.Tool.EditorEventRaised += OnEditorEventRaised;

        if (commandToSelect is not null)
            SelectCommand(commandToSelect);
        else
            SelectedRow = Rows.FirstOrDefault();
    }

    /// <summary>Counterpart of the legacy <c>Dispose</c> event unhook; called by the window on <c>Closed</c>.</summary>
    public void Detach()
    {
        _closed = true;
        _editor.Tool.EditorEventRaised -= OnEditorEventRaised;
    }

    private void Initialize(AnimationNode animation)
    {
        if (_initializing)
            return;

        _initializing = true;

        _animation = animation;

        // Deep-copy the current commands as a restore point (legacy _backupCommands), then wrap the
        // live command instances so edits and the 3D preview stay in sync (legacy referenced the same list).
        _backupCommands = animation.WadAnimation.AnimCommands.Select(ac => ac.Clone()).ToList();

        Rows.Clear();
        foreach (var ac in animation.WadAnimation.AnimCommands)
            Rows.Add(new AnimCommandRow(ac));

        _initializing = false;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (_closed)
            return;

        if (obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent currentChanged &&
            currentChanged.Current != _animation)
        {
            ApplyAnimCommands(_backupCommands);
            Initialize(currentChanged.Current);
            SelectedRow = Rows.FirstOrDefault();
        }

        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent animationChanged &&
            animationChanged.Animation == _animation && animationChanged.Focus)
        {
            Initialize(animationChanged.Animation);
            SelectedRow = Rows.FirstOrDefault();
        }
    }

    private bool HasSelection() => SelectedRow is not null;

    partial void OnSelectedRowChanged(AnimCommandRow? value)
        => SelectedCommandChanged?.Invoke(value?.Command);

    /// <summary>Called by the window when the hosted editor edited the selected command in place.</summary>
    public void OnCommandEditedFromControl(WadAnimCommand command)
    {
        AnimCommandRow? row = Rows.FirstOrDefault(r => ReferenceEquals(r.Command, command)) ?? SelectedRow;
        if (row is null)
            return;

        row.RefreshDescription();
        SyncToAnimation();
    }

    [RelayCommand]
    private void AddRow()
    {
        var row = new AnimCommandRow(new WadAnimCommand { Type = WadAnimCommandType.SetPosition });
        Rows.Add(row);
        SelectedRow = row;
        SyncToAnimation();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Delete()
    {
        if (SelectedRow is null)
            return;

        int index = Rows.IndexOf(SelectedRow);
        Rows.Remove(SelectedRow);
        SelectedRow = Rows.Count == 0 ? null : Rows[Math.Min(index, Rows.Count - 1)];
        SyncToAnimation();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveUp() => Move(down: false);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveDown() => Move(down: true);

    private void Move(bool down)
    {
        if (SelectedRow is null)
            return;

        int index = Rows.IndexOf(SelectedRow);
        int target = down ? index + 1 : index - 1;

        if (target < 0 || target > Rows.Count - 1)
            return;

        Rows.Move(index, target);
        SyncToAnimation();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Copy()
    {
        if (SelectedRow is null)
            return;

        int index = Rows.IndexOf(SelectedRow);
        Rows.Insert(index + 1, new AnimCommandRow(SelectedRow.Command.Clone()));
        SyncToAnimation();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void CopyToAll()
    {
        if (_editor.SelectionIsEmpty || SelectedRow is null)
            return;

        int index = Rows.IndexOf(SelectedRow);

        for (int i = (int)(_editor.Selection.Y * _animation.WadAnimation.FrameRate);
                 i >= (int)(_editor.Selection.X * _animation.WadAnimation.FrameRate); i--)
        {
            WadAnimCommand source = Rows[index].Command;

            // Don't create copy if animcommand isn't frame-based or there's already the same one within the selection area.
            if (source.FrameBased && source.Parameter1 == i)
                continue;

            var cmdCopy = source.Clone();

            // Change frame number
            if (source.FrameBased)
                cmdCopy.Parameter1 = (short)i;

            Rows.Insert(index + 1, new AnimCommandRow(cmdCopy));
        }

        SyncToAnimation();
    }

    [RelayCommand]
    private void Confirm()
    {
        ApplyChanges();
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
        ApplyChanges();
        Initialize(_animation);
        SelectedRow = Rows.FirstOrDefault();
    }

    /// <summary>Restores the backup if the window is closed without confirming (legacy DialogChoice.Cancel path).</summary>
    public void RevertIfNotConfirmed()
    {
        if (DialogResult != true)
            DiscardChanges();
    }

    private void ApplyChanges()
    {
        // Bounce to old animcommands for undo (legacy ApplyChanges).
        var newCommands = Rows.Select(r => r.Command).ToList();
        ApplyAnimCommands(_backupCommands);
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

        ApplyAnimCommands(newCommands);
        _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
    }

    private void DiscardChanges()
    {
        ApplyAnimCommands(_backupCommands);
        _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
    }

    private void ApplyAnimCommands(List<WadAnimCommand> newCommands)
    {
        _animation.WadAnimation.AnimCommands.Clear();
        _animation.WadAnimation.AnimCommands.AddRange(newCommands);
    }

    /// <summary>
    /// Mirrors the legacy live preview: pushes the current grid order into the animation and
    /// notifies the editor so the 3D preview updates as commands are added/moved/edited.
    /// </summary>
    private void SyncToAnimation()
    {
        if (_initializing)
            return;

        ApplyAnimCommands(Rows.Select(r => r.Command).ToList());
        _editor.Tool.AnimationEditorAnimcommandChanged();
    }

    private void SelectCommand(WadAnimCommand command)
    {
        AnimCommandRow? match = Rows.FirstOrDefault(r => r.Command.Equals(command));
        if (match is not null)
        {
            SelectedRow = match;
            return;
        }

        // Legacy SelectCommand appended the command when it wasn't found.
        var row = new AnimCommandRow(command);
        Rows.Add(row);
        SelectedRow = row;
        SyncToAnimation();
    }
}

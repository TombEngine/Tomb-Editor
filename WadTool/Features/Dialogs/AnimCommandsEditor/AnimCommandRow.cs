#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.Wad;

namespace WadTool.Features.Dialogs.AnimCommandsEditor;

/// <summary>
/// One grid row, wrapping a live <see cref="WadAnimCommand"/> instance (the same object held by
/// the animation), so edits made through the hosted <c>AnimCommandEditor</c> are reflected without
/// copying. <see cref="Description"/> mirrors the legacy single "Commands" column; the hosted editor
/// mutates the command in place, so the view model calls <see cref="RefreshDescription"/> to repaint it.
/// </summary>
public sealed class AnimCommandRow : ObservableObject
{
    public WadAnimCommand Command { get; }

    public AnimCommandRow(WadAnimCommand command) => Command = command;

    public string Description => Command.Description;

    public void RefreshDescription() => OnPropertyChanged(nameof(Description));
}

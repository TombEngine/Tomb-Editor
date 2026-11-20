#nullable enable

using System.Windows;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Shared behavior of the Animation Editor dockable panels.</summary>
internal static class PanelHelper
{
    /// <summary>Starts a new undo step for the next parameter edit made in the panel.</summary>
    public static void ResetUndoCoalescing(FrameworkElement panel)
        => (panel.DataContext as AnimationEditorWindowViewModel)?.ResetUndoCoalescing();
}

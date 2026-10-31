#nullable enable

using System.Windows.Input;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Keyboard shortcuts of the node editor (same map as the previous editor): N/A add action,
    /// C add condition (Shift or Alt links through Else), L link selected, Ctrl+A select all,
    /// Esc deselect, Delete/Backspace delete selected (Shift clears all), Ctrl+C/X/V clipboard.
    /// </summary>
    public static class NodeEditorKeyMap
    {
        /// <summary>Runs the action bound to the key; returns true when the key was consumed.</summary>
        public static bool TryHandle(NodeEditorViewModel editor, Key key, ModifierKeys modifiers)
        {
            bool elseLink = modifiers is ModifierKeys.Shift or ModifierKeys.Alt;

            switch (key)
            {
                case Key.Delete or Key.Back when modifiers == ModifierKeys.None:
                    editor.DeleteSelectedNodeCommand.Execute(null);
                    return true;

                case Key.Delete or Key.Back when modifiers == ModifierKeys.Shift:
                    editor.ClearNodes();
                    return true;

                case Key.N or Key.A when modifiers is ModifierKeys.None or ModifierKeys.Shift or ModifierKeys.Alt:
                    editor.AddNodeOfKind(false, true, elseLink);
                    return true;

                case Key.C when modifiers is ModifierKeys.None or ModifierKeys.Shift or ModifierKeys.Alt:
                    editor.AddNodeOfKind(true, true, elseLink);
                    return true;

                case Key.L when modifiers == ModifierKeys.None:
                    editor.LinkSelectedCommand.Execute(null);
                    return true;

                case Key.A when modifiers == ModifierKeys.Control:
                    editor.SelectAll();
                    return true;

                case Key.Escape when modifiers == ModifierKeys.None && editor.HasSelectedNode:
                    editor.ClearSelection();
                    return true;

                case Key.C when modifiers == ModifierKeys.Control:
                    editor.CopySelected(false);
                    return true;

                case Key.X when modifiers == ModifierKeys.Control:
                    editor.CopySelected(true);
                    return true;

                case Key.V when modifiers == ModifierKeys.Control:
                    editor.Paste();
                    return true;

                default:
                    return false;
            }
        }
    }
}

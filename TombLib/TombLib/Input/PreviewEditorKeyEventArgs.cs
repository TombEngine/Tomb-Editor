#nullable enable

namespace TombLib.Input;

/// <summary>
/// Allows a view to claim a navigation key before it is processed.
/// </summary>
public sealed class PreviewEditorKeyEventArgs(EditorKey keyData) : EditorKeyEventArgs(keyData)
{
    /// <summary>Gets or sets whether the view claims this key as input.</summary>
    public bool IsInputKey { get; set; }
}

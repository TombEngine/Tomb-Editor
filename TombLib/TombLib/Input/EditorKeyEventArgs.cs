#nullable enable

using System;

namespace TombLib.Input;

/// <summary>
/// Describes a keyboard action using stable virtual key values.
/// </summary>
public class EditorKeyEventArgs(EditorKey keyData) : EventArgs
{
    private bool _suppressKeyPress;

    /// <summary>Gets the key together with its modifier flags.</summary>
    public EditorKey KeyData { get; } = keyData;
    /// <summary>Gets the virtual key without modifier flags.</summary>
    public EditorKey KeyCode => KeyData & EditorKey.KeyCode;
    /// <summary>Gets the numeric virtual key code.</summary>
    public int KeyValue => (int)KeyCode;
    /// <summary>Gets the active modifier flags.</summary>
    public EditorKey Modifiers => KeyData & EditorKey.Modifiers;
    /// <summary>Gets whether Alt is pressed.</summary>
    public bool Alt => (Modifiers & EditorKey.Alt) != 0;
    /// <summary>Gets whether Control is pressed.</summary>
    public bool Control => (Modifiers & EditorKey.Control) != 0;
    /// <summary>Gets whether Shift is pressed.</summary>
    public bool Shift => (Modifiers & EditorKey.Shift) != 0;
    /// <summary>Gets or sets whether this action has been handled.</summary>
    public bool Handled { get; set; }
    /// <summary>Gets or sets whether subsequent character input is suppressed.</summary>
    public bool SuppressKeyPress
    {
        get => _suppressKeyPress;
        set
        {
            _suppressKeyPress = value;
            Handled = value;
        }
    }
}

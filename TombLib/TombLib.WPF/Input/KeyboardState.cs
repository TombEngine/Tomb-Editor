using System.Windows.Input;
using TombLib.Input;

namespace TombLib.WPF.Input;

/// <summary>Reads keyboard modifiers using WPF input state.</summary>
public static class KeyboardState
{
    /// <summary>Gets the currently pressed modifier keys.</summary>
    public static EditorKey ModifierKeys
    {
        get
        {
            var result = EditorKey.None;

            if (Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
                result |= EditorKey.Control;

            if (Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
                result |= EditorKey.Shift;

            if (Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
                result |= EditorKey.Alt;

            return result;
        }
    }
}

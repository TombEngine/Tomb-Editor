using System.Runtime.InteropServices;
using TombLib.Input;

namespace TombLib.WPF.Input;

/// <summary>
/// Reads keyboard modifiers from the system keyboard state, like <c>Control.ModifierKeys</c> did.
/// WPF's own <c>Keyboard.Modifiers</c> reports nothing while the keyboard focus is inside a native child window
/// (such as the Direct3D rendering panel), so it cannot be used for modifier + mouse click actions there.
/// </summary>
public static class KeyboardState
{
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;

    /// <summary>Gets the currently pressed modifier keys.</summary>
    public static EditorKey ModifierKeys
    {
        get
        {
            var result = EditorKey.None;

            if (IsDown(VkControl))
                result |= EditorKey.Control;

            if (IsDown(VkShift))
                result |= EditorKey.Shift;

            if (IsDown(VkMenu))
                result |= EditorKey.Alt;

            return result;
        }
    }

    private static bool IsDown(int virtualKey) => (GetKeyState(virtualKey) & 0x8000) != 0;

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int virtualKey);
}

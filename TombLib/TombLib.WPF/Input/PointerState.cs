using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace TombLib.WPF.Input;

/// <summary>Reads and updates the native pointer in screen pixel coordinates.</summary>
public static class PointerState
{
    /// <summary>Hides the native pointer until a matching show operation.</summary>
    public static void Hide() => ShowCursor(false);

    /// <summary>Restores the native pointer after a matching hide operation.</summary>
    public static void Show() => ShowCursor(true);

    /// <summary>Gets or sets the cursor used while a pointer gesture is active.</summary>
    public static Cursor? CurrentCursor
    {
        get => Mouse.OverrideCursor;
        set => Mouse.OverrideCursor = value;
    }

    /// <summary>Applies a WPF system cursor to a native rendering surface.</summary>
    public static void ApplyCursor(Cursor? cursor)
    {
        cursor = Mouse.OverrideCursor ?? cursor ?? Cursors.Arrow;
        int resource = cursor == Cursors.Cross ? 32515 :
            cursor == Cursors.Hand ? 32649 :
            cursor == Cursors.IBeam ? 32513 :
            cursor == Cursors.Wait ? 32514 :
            cursor == Cursors.SizeAll ? 32646 :
            cursor == Cursors.SizeNS ? 32645 :
            cursor == Cursors.SizeWE ? 32644 :
            cursor == Cursors.SizeNESW ? 32643 :
            cursor == Cursors.SizeNWSE ? 32642 :
            cursor == Cursors.No ? 32648 :
            cursor == Cursors.Help ? 32651 :
            cursor == Cursors.AppStarting ? 32650 : 32512;
        SetCursor(cursor == Cursors.None ? 0 : LoadCursor(0, resource));
    }

    /// <summary>Gets or sets the pointer position in screen pixels.</summary>
    public static Point ScreenPosition
    {
        get
        {
            GetCursorPos(out var point);
            return new Point(point.X, point.Y);
        }
        set => SetCursorPos(value.X, value.Y);
    }

    /// <summary>Gets or sets the native pointer constraint; an empty rectangle releases it.</summary>
    public static Rectangle Clip
    {
        get
        {
            GetClipCursor(out var rectangle);
            return Rectangle.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }
        set
        {
            if (value.IsEmpty)
            {
                ReleaseClip(0);
                return;
            }

            var rectangle = new NativeRectangle(value.Left, value.Top, value.Right, value.Bottom);
            ClipCursor(ref rectangle);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private record struct NativeRectangle(int Left, int Top, int Right, int Bottom);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClipCursor(out NativeRectangle rectangle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClipCursor(ref NativeRectangle rectangle);

    [DllImport("user32.dll", EntryPoint = "ClipCursor")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseClip(nint rectangle);

    [DllImport("user32.dll")]
    private static extern nint LoadCursor(nint instance, nint cursor);

    [DllImport("user32.dll")]
    private static extern nint SetCursor(nint cursor);

    [DllImport("user32.dll")]
    private static extern int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool show);
}

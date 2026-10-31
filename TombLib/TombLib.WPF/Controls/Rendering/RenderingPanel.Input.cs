#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TombLib.Input;
using TombLib.Services;
using TombLib.WPF.Input;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace TombLib.WPF.Controls.Rendering;

public partial class RenderingPanel
{
    private bool _mouseInside;
    /// <summary>Raised after the renderer processes a native mouse press.</summary>
    public new event EventHandler<PointerEventArgs>? MouseDown;

    /// <summary>Raised after the renderer processes a native mouse release.</summary>
    public new event EventHandler<PointerEventArgs>? MouseUp;

    /// <summary>Raised after the renderer processes a double click.</summary>
    public event EventHandler<PointerEventArgs>? MouseDoubleClick;

    protected static EditorKey ModifierKeys => KeyboardState.ModifierKeys;

    /// <summary>Moves keyboard focus to the native viewport.</summary>
    public new bool Focus()
    {
        Keyboard.Focus(this);
        if (_windowHandle != IntPtr.Zero)
            SetFocus(_windowHandle);

        return true;
    }

    protected virtual void OnMouseEnter(EventArgs e) { }
    protected virtual void OnMouseLeave(EventArgs e) { }
    protected virtual void OnMouseMove(PointerEventArgs e) { }
    protected virtual void OnMouseWheel(PointerEventArgs e) { }
    protected virtual void OnMouseDown(PointerEventArgs e) => MouseDown?.Invoke(this, e);
    protected virtual void OnMouseUp(PointerEventArgs e) => MouseUp?.Invoke(this, e);
    protected virtual void OnMouseDoubleClick(PointerEventArgs e) => MouseDoubleClick?.Invoke(this, e);
    protected virtual void OnKeyDown(EditorKeyEventArgs e) { }
    protected virtual void OnKeyUp(EditorKeyEventArgs e) { }

    protected override IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (message)
        {
            case 0x000F:
                ValidateRect(hwnd, IntPtr.Zero);
                Invalidate();
                handled = true;
                break;
            case 0x0014:
                handled = true;
                return new IntPtr(1);
            case 0x0020:
                PointerState.ApplyCursor(Cursor);
                handled = true;
                return new IntPtr(1);
            case 0x0008:
                OnLostFocus(EventArgs.Empty);
                break;
            case 0x0100:
            case 0x0104:
                DispatchKey((int)wParam, true, ref handled);
                break;
            case 0x0101:
            case 0x0105:
                DispatchKey((int)wParam, false, ref handled);
                break;
            case 0x0200:
                EnterMouse(hwnd);
                OnMouseMove(CreateMouseArgs(wParam, lParam));
                handled = true;
                break;
            case 0x0201:
            case 0x0204:
            case 0x0207:
                Focus();
                SetCapture(hwnd);
                OnMouseDown(CreateMouseArgs(wParam, lParam, ButtonForMessage(message)));
                handled = true;
                break;
            case 0x0202:
            case 0x0205:
            case 0x0208:
                ReleaseCapture();
                OnMouseUp(CreateMouseArgs(wParam, lParam, ButtonForMessage(message - 1)));
                handled = true;
                break;
            case 0x0203:
            case 0x0206:
            case 0x0209:
                OnMouseDoubleClick(CreateMouseArgs(wParam, lParam, ButtonForMessage(message - 2), 2));
                handled = true;
                break;
            case 0x020A:
                var screenPoint = new NativePoint { X = (short)(long)lParam, Y = (short)((long)lParam >> 16) };
                ScreenToClient(hwnd, ref screenPoint);
                OnMouseWheel(new PointerEventArgs(PointerButtons.None, 0, screenPoint.X, screenPoint.Y, (short)((long)wParam >> 16)));
                handled = true;
                break;
            case 0x02A3:
                _mouseInside = false;
                OnMouseLeave(EventArgs.Empty);
                handled = true;
                break;
        }

        return base.WndProc(hwnd, message, wParam, lParam, ref handled);
    }

    private void EnterMouse(IntPtr hwnd)
    {
        if (_mouseInside)
            return;

        _mouseInside = true;
        var tracking = new MouseTracking { Size = (uint)Marshal.SizeOf<MouseTracking>(), Flags = 2, Window = hwnd };
        TrackMouseEvent(ref tracking);
        OnMouseEnter(EventArgs.Empty);
    }

    private void DispatchKey(int virtualKey, bool pressed, ref bool handled)
    {
        if (pressed)
            OnPreviewKeyDown(new PreviewEditorKeyEventArgs((EditorKey)virtualKey | ModifierKeys));

        var nativeArgs = new EditorKeyEventArgs((EditorKey)virtualKey | ModifierKeys);
        if (pressed)
            OnKeyDown(nativeArgs);
        else
            OnKeyUp(nativeArgs);

        if (!nativeArgs.Handled && PresentationSource.FromVisual(this) is { } source)
        {
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, KeyInterop.KeyFromVirtualKey(virtualKey))
            {
                RoutedEvent = pressed ? Keyboard.PreviewKeyDownEvent : Keyboard.PreviewKeyUpEvent
            };
            RaiseEvent(args);
            if (!args.Handled)
            {
                args.RoutedEvent = pressed ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent;
                RaiseEvent(args);
            }

            handled = args.Handled;
        }
        else
        {
            handled = nativeArgs.Handled;
        }
    }

    private static PointerEventArgs CreateMouseArgs(IntPtr wParam, IntPtr lParam,
        PointerButtons? button = null, int clicks = 1)
    {
        var buttons = PointerButtons.None;
        if (((long)wParam & 1) != 0)
            buttons |= PointerButtons.Left;
        if (((long)wParam & 2) != 0)
            buttons |= PointerButtons.Right;
        if (((long)wParam & 16) != 0)
            buttons |= PointerButtons.Middle;

        return new PointerEventArgs(button ?? buttons, clicks, (short)(long)lParam, (short)((long)lParam >> 16), 0);
    }

    private static PointerButtons ButtonForMessage(int message) => message switch
    {
        0x0201 => PointerButtons.Left,
        0x0204 => PointerButtons.Right,
        _ => PointerButtons.Middle
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseTracking
    {
        public uint Size;
        public uint Flags;
        public IntPtr Window;
        public uint HoverTime;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr cursor);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TrackMouseEvent(ref MouseTracking tracking);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(IntPtr window, ref NativePoint point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ValidateRect(IntPtr window, IntPtr rectangle);
}

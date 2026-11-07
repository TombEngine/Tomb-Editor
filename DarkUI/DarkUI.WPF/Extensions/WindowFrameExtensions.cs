using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DarkUI.WPF.Extensions;

/// <summary>
/// Native window frame tweaks. Everything here is optional cosmetics: on platforms or Windows
/// versions that do not support it, the calls do nothing.
/// </summary>
public static class WindowFrameExtensions
{
    private const uint DwmwaBorderColor = 34;
    private const int MinimumWindowsBuild = 22000;

    /// <summary>Paints the system frame border of the window (the accent border Windows 11 draws on the active window) in the given color.</summary>
    public static void SetFrameBorderColor(this Window window, Color color)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, MinimumWindowsBuild))
            return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
            return;

        int colorRef = color.R | (color.G << 8) | (color.B << 16);

        try
        {
            DwmSetWindowAttribute(handle, DwmwaBorderColor, ref colorRef, sizeof(int));
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
        {
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attribute, ref int value, uint size);

    private const uint RdwInvalidate = 0x0001;
    private const uint RdwAllChildren = 0x0080;
    private const uint RdwUpdateNow = 0x0100;

    /// <summary>Repaints the whole window including child windows; clears stale pixels that remain after a docking splitter drag with hosted native windows.</summary>
    public static void RedrawSurface(this Window window)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero)
            RedrawWindow(handle, IntPtr.Zero, IntPtr.Zero, RdwInvalidate | RdwAllChildren | RdwUpdateNow);
    }

    [DllImport("user32.dll")]
    private static extern bool RedrawWindow(IntPtr hwnd, IntPtr updateRect, IntPtr updateRegion, uint flags);
}

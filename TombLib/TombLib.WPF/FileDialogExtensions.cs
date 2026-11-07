using Microsoft.Win32;
using System;
using TombLib.Services;

namespace TombLib.WPF;

/// <summary>Resolves native dialog ownership to the corresponding WPF window.</summary>
public static class FileDialogExtensions
{
    /// <summary>Shows a native file dialog owned by an editor window.</summary>
    public static bool? ShowDialog(this CommonDialog dialog, IWindowOwner? owner)
        => dialog.ShowDialog(owner?.Handle ?? IntPtr.Zero);

    /// <summary>Shows a native file dialog owned by the WPF window with this handle.</summary>
    public static bool? ShowDialog(this CommonDialog dialog, IntPtr owner)
    {
        var window = WPFUtils.GetOwnerWindow(owner);

        return window is null ? dialog.ShowDialog() : dialog.ShowDialog(window);
    }
}

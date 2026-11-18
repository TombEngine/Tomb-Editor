#nullable enable

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System;
using System.Windows;

namespace TombLib.WPF;

/// <summary>
/// WPF counterpart of <see cref="TombLib.Utils.ConfigurationBase"/>'s
/// <c>Configuration.ConfigureWindow(Form, ConfigurationBase)</c>.
/// Persists a <see cref="Window"/>'s size, position and maximized flag into
/// <see cref="ConfigurationBase"/> properties named <c>Window_&lt;key&gt;_Size / _Position / _Maximized</c>.
/// A position of (-1,-1) (the config default) means "never moved": the window keeps the placement
/// from its <see cref="Window.WindowStartupLocation"/>, mirroring the previous behaviour.
/// </summary>
public static class WindowConfiguration
{
    /// <summary>
    /// Load size/position/maximized state from <paramref name="config"/> onto <paramref name="window"/>,
    /// then hook <see cref="Window.Closing"/> to save it back. Pass <paramref name="key"/> when the
    /// previous form name differs from the WPF window's class name, so existing user configs
    /// keep applying.
    /// </summary>
    public static void ConfigureWindow(Window window, ConfigurationBase config, string? key = null)
    {
        if (window is null || config is null)
            return;

        var effectiveKey = key ?? window.GetType().Name;
        LoadWindowProperties(window, config, effectiveKey);
        window.Closing += (_, _) => SaveWindowProperties(window, config, effectiveKey);
    }

    private static void LoadWindowProperties(Window window, ConfigurationBase config, string key)
    {
        var prefix = "Window_" + key;
        var size = config.GetType().GetProperty(prefix + "_Size")?.GetValue(config);
        var pos = config.GetType().GetProperty(prefix + "_Position")?.GetValue(config);
        var max = config.GetType().GetProperty(prefix + "_Maximized")?.GetValue(config);

        // Auto-sized dialogs must keep their measured size: applying a persisted size (e.g. one
        // saved by the previous form) would leave a dead band beyond the content.
        if (window.SizeToContent == SizeToContent.Manual && size is Size s)
        {
            window.Width = s.Width;
            window.Height = s.Height;

            // Re-center after resizing: ConfigureWindow runs on Loaded, after CenterOwner has already
            // positioned the (differently sized) window, so changing the size would leave it off-center.
            RecenterOnOwner(window);
        }

        // Restore the last position like the legacy ConfigureWindow; (-1,-1) means the user never
        // moved the window, so the WindowStartupLocation placement stays in effect.
        if (pos is Point p && (p.X != -1 || p.Y != -1))
        {
            // For SizeToContent windows WPF applies CenterOwner/CenterScreen only once the final
            // size is known after Loaded, which would override the position restored here.
            // Neutralize it, like the previous code set StartPosition.Manual.
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = p.X;
            window.Top = p.Y;
            ClampToVirtualScreen(window);
        }

        if (max is bool m && m)
            window.WindowState = WindowState.Maximized;
    }

    private static void RecenterOnOwner(Window window)
    {
        if (window.WindowStartupLocation != WindowStartupLocation.CenterOwner || window.Owner is null)
            return;

        window.Left = window.Owner.Left + (window.Owner.ActualWidth - window.Width) / 2.0;
        window.Top = window.Owner.Top + (window.Owner.ActualHeight - window.Height) / 2.0;
    }

    private static void ClampToVirtualScreen(Window window)
    {
        double width = double.IsNaN(window.Width) ? window.ActualWidth : window.Width;
        double height = double.IsNaN(window.Height) ? window.ActualHeight : window.Height;

        double minLeft = SystemParameters.VirtualScreenLeft;
        double minTop = SystemParameters.VirtualScreenTop;
        double maxLeft = Math.Max(minLeft, minLeft + SystemParameters.VirtualScreenWidth - width);
        double maxTop = Math.Max(minTop, minTop + SystemParameters.VirtualScreenHeight - height);

        window.Left = Math.Min(Math.Max(window.Left, minLeft), maxLeft);
        window.Top = Math.Min(Math.Max(window.Top, minTop), maxTop);
    }

    private static void SaveWindowProperties(Window window, ConfigurationBase config, string key)
    {
        if (window.WindowState == WindowState.Minimized)
            return;

        var prefix = "Window_" + key;
        config.GetType().GetProperty(prefix + "_Maximized")?.SetValue(config, window.WindowState == WindowState.Maximized);

        if (window.WindowState != WindowState.Maximized)
        {
            // Auto-sized dialogs get their size from their content. See LoadWindowProperties.
            if (window.SizeToContent == SizeToContent.Manual)
                config.GetType().GetProperty(prefix + "_Size")?.SetValue(config, new Size((int)window.Width, (int)window.Height));

            config.GetType().GetProperty(prefix + "_Position")?.SetValue(config, new Point((int)window.Left, (int)window.Top));
        }
    }
}

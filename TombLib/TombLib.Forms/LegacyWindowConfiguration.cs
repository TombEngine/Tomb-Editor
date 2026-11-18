#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;

namespace TombLib.Forms;

/// <summary>
/// Preserves window settings for remaining legacy desktop tools.
/// </summary>
public static class LegacyWindowConfiguration
{
    /// <summary>Saves the current window state and restored bounds.</summary>
    public static void SaveWindowProperties(Form form, ConfigurationBase config)
    {
        var name = "Window_" + form.Name;

        // Minimized windows do not have usable bounds.
        if (form.WindowState == FormWindowState.Minimized)
            return;

        config.GetType().GetProperty(name + "_Maximized")?.SetValue(config, form.WindowState == FormWindowState.Maximized);

        // Set size only if form isn't maximized, otherwise we'll get corrupted coordinates.
        if (form.WindowState != FormWindowState.Maximized)
        {
            config.GetType().GetProperty(name + "_Size")?.SetValue(config, form.Size);
            config.GetType().GetProperty(name + "_Position")?.SetValue(config, form.Location);
        }
    }

    /// <summary>Restores saved window settings and clamps the resulting location.</summary>
    public static void LoadWindowProperties(Form form, ConfigurationBase config)
    {
        var name = "Window_" + form.Name;

        var size = config.GetType().GetProperty(name + "_Size")?.GetValue(config);
        var pos = config.GetType().GetProperty(name + "_Position")?.GetValue(config);
        var max = config.GetType().GetProperty(name + "_Maximized")?.GetValue(config);

        if (size is Size windowSize)
            form.Size = windowSize;
        if (pos is Point position)
            form.Location = position;
        if (max is bool maximized)
            form.WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal;

        if (form.Location.X == -1 && form.Location.Y == -1)
        {
            if (form.ParentForm is null)
                form.StartPosition = FormStartPosition.CenterScreen;
            else
                form.StartPosition = FormStartPosition.CenterParent;
        }
        else
        {
            form.StartPosition = FormStartPosition.Manual;
        }

        ClampWindowLocation(form);
    }

    /// <summary>Keeps manually positioned windows inside the available desktop.</summary>
    public static void ClampWindowLocation(Form form)
    {
        if (form.StartPosition != FormStartPosition.Manual)
            return;

        // Keep the window inside the desktop bounds.
        int clampedX = Math.Max(0, form.Location.X);
        int clampedY = Math.Max(0, form.Location.Y);

        // Apply the upper bounds on a single-display desktop.
        var primaryScreen = Screen.PrimaryScreen;

        if (Screen.AllScreens.Length == 1 && primaryScreen is not null)
        {
            int maximumX = Math.Max(0, primaryScreen.Bounds.Width - form.Width);
            int maximumY = Math.Max(0, primaryScreen.Bounds.Height - form.Height);
            clampedX = Math.Min(clampedX, maximumX);
            clampedY = Math.Min(clampedY, maximumY);
        }

        form.Location = new Point(clampedX, clampedY);
    }

    /// <summary>Restores settings and saves them when the window closes.</summary>
    public static void ConfigureWindow(Form? form, ConfigurationBase? config)
    {
        if (form is null || config is null)
            return;

        LoadWindowProperties(form, config);
        form.FormClosing += (_, _) => SaveWindowProperties(form, config);
    }
}

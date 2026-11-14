using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TombLib.WPF.Controls.Rendering;

/// <summary>Display scale helpers for rendering overlays drawn in physical pixels.</summary>
public static class RenderingDpi
{
    /// <summary>
    /// Gets the factor to apply to a configured overlay font size. WinForms was bitmap-scaled by Windows
    /// (the overlay grew with the display scale); WPF draws the viewport in physical pixels.
    /// </summary>
    public static float FontScale
    {
        get
        {
            var window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible)
                         ?? Application.Current?.MainWindow;

            return window is null ? 1.0f : (float)VisualTreeHelper.GetDpi(window).DpiScaleX;
        }
    }
}
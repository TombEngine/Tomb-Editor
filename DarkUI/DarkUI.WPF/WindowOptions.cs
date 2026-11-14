using System.Windows;

namespace DarkUI.WPF;

/// <summary>Per-window options of the dark window style.</summary>
public static class WindowOptions
{
    public static readonly DependencyProperty ShowMinMaxButtonsProperty = DependencyProperty.RegisterAttached(
        "ShowMinMaxButtons", typeof(bool), typeof(WindowOptions), new FrameworkPropertyMetadata(true));

    /// <summary>Gets whether the title bar shows the minimize and maximize buttons (a resizable dialog may show only the close button).</summary>
    public static bool GetShowMinMaxButtons(DependencyObject obj) => (bool)obj.GetValue(ShowMinMaxButtonsProperty);

    public static void SetShowMinMaxButtons(DependencyObject obj, bool value) => obj.SetValue(ShowMinMaxButtonsProperty, value);
}
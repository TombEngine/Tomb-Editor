using System.Windows;

namespace DarkUI.WPF;

/// <summary>Per-progress-bar display options.</summary>
public static class ProgressBarOptions
{
    public static readonly DependencyProperty ShowValueOfMaximumProperty = DependencyProperty.RegisterAttached(
        "ShowValueOfMaximum", typeof(bool), typeof(ProgressBarOptions), new FrameworkPropertyMetadata(false));

    /// <summary>Gets whether the bar shows "value / maximum" instead of a percentage (the WinForms "X of N" mode).</summary>
    public static bool GetShowValueOfMaximum(DependencyObject obj) => (bool)obj.GetValue(ShowValueOfMaximumProperty);

    public static void SetShowValueOfMaximum(DependencyObject obj, bool value) => obj.SetValue(ShowValueOfMaximumProperty, value);
}
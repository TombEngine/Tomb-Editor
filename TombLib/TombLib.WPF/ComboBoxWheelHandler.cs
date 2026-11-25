#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TombLib.WPF;

/// <summary>
/// Makes every closed combo box step through its items with the mouse wheel on hover, like the WinForms combo box did,
/// without the control having to be focused or opened first.
/// </summary>
public static class ComboBoxWheelHandler
{
    /// <summary>Set to false on a combo box that must not react to the wheel on hover.</summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(ComboBoxWheelHandler), new PropertyMetadata(true));

    private static bool _installed;

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    /// <summary>Registers the wheel handler for all combo boxes of the application; safe to call more than once.</summary>
    public static void Install()
    {
        if (_installed)
            return;

        _installed = true;
        EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel));
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Combo boxes can opt out with the attached property.
        if (e.Handled || sender is not ComboBox comboBox || !GetIsEnabled(comboBox))
            return;

        if (comboBox.IsDropDownOpen || !comboBox.IsEnabled || comboBox.Items.Count == 0)
            return;

        // Let the open drop-down list scroll normally when the cursor is inside it.
        if (e.OriginalSource is DependencyObject source && source != comboBox && !comboBox.IsAncestorOf(source))
            return;

        var newIndex = Math.Clamp(comboBox.SelectedIndex + (e.Delta > 0 ? -1 : 1), 0, comboBox.Items.Count - 1);

        if (newIndex != comboBox.SelectedIndex)
            comboBox.SelectedIndex = newIndex;

        e.Handled = true;
    }
}

// Code-behind for LuaPropertyGridControl.
// Handles color picker interaction (opens RealtimeColorDialog via WinForms interop)
// and provides value converters used in the XAML.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DarkUI.WPF.CustomControls;
using TombLib.Controls;
using TombLib.Forms.ViewModels;

namespace TombLib.Forms.Views
{
    /// <summary>
    /// WPF property grid control for editing TombEngine Lua properties.
    /// Hostable inside WinForms DarkForm/DarkToolWindow via ElementHost.
    /// Uses DarkUI.WPF styled controls (NumericUpDown, ColorPickerButton, etc.).
    /// </summary>
    public partial class LuaPropertyGridControl : UserControl
    {
        public LuaPropertyGridControl()
        {
            // Add custom converters to resources before InitializeComponent.
            Resources.Add("BoolToVisConverter", new BooleanToVisibilityConverter());
            Resources.Add("StringEmptyToVisConverter", new StringEmptyToCollapsedConverter());

            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel. Sets DataContext.
        /// </summary>
        public LuaPropertyGridViewModel ViewModel
        {
            get => DataContext as LuaPropertyGridViewModel;
            set => DataContext = value;
        }

        /// <summary>
        /// Handles the color picker click on the ColorPickerButton.
        /// Opens the RealtimeColorDialog (WinForms) and updates the property value.
        /// </summary>
        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ColorPickerButton button ||
                button.DataContext is not LuaPropertyRowViewModel row)
                return;

            // Get current color from ViewModel.
            var currentColor = System.Drawing.Color.FromArgb(255, row.ColorR, row.ColorG, row.ColorB);

            // Open the existing RealtimeColorDialog (WinForms control).
            var mousePos = System.Windows.Forms.Control.MousePosition;
            using (var colorDialog = new RealtimeColorDialog(mousePos.X, mousePos.Y, c =>
            {
                // Live preview callback: update color values in real time.
                row.ColorR = c.R;
                row.ColorG = c.G;
                row.ColorB = c.B;
            }))
            {
                colorDialog.Color = currentColor;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    // Restore original color on cancel.
                    row.ColorR = currentColor.R;
                    row.ColorG = currentColor.G;
                    row.ColorB = currentColor.B;
                    return;
                }

                // Apply selected color.
                if (currentColor != colorDialog.Color)
                {
                    row.ColorR = colorDialog.Color.R;
                    row.ColorG = colorDialog.Color.G;
                    row.ColorB = colorDialog.Color.B;
                }
            }
        }

        /// <summary>
        /// Handles mousewheel on ComboBox to scroll through values on hover,
        /// similar to the WinForms DarkComboBox behavior.
        /// </summary>
        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Items.Count > 0)
            {
                int newIndex = comboBox.SelectedIndex + (e.Delta > 0 ? -1 : 1);
                newIndex = Math.Max(0, Math.Min(newIndex, comboBox.Items.Count - 1));
                comboBox.SelectedIndex = newIndex;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles double-click on the property name label.
        /// Resets the property value to its default.
        /// </summary>
        private void PropertyName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 &&
                sender is FrameworkElement element &&
                element.DataContext is LuaPropertyRowViewModel row)
            {
                row.ResetToDefault();
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Converter: collapses element when string is null or empty.
    /// Used for category headers — hides header when category is empty.
    /// </summary>
    internal class StringEmptyToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

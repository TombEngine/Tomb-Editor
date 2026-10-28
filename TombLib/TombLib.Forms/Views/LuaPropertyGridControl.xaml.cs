#nullable enable

using DarkUI.WPF.CustomControls;
using System;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TombLib.Forms.Services;
using TombLib.Forms.ViewModels;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.Views;

public partial class LuaPropertyGridControl : UserControl
{
    private readonly IColorPickerService _colorPicker;

    public LuaPropertyGridControl() : this(new ColorPickerService())
    {
    }

    public LuaPropertyGridControl(IColorPickerService colorPicker)
    {
        _colorPicker = colorPicker;
        Resources.Add("BoolToVisConverter", new BooleanToVisibilityConverter());
        Resources.Add("StringEmptyToVisConverter", new StringEmptyToCollapsedConverter());
        Resources.Add("BoolToOpacityConverter", new BoolToOpacityConverter());
        InitializeComponent();
    }

    public LuaPropertyGridViewModel? ViewModel
    {
        get => DataContext as LuaPropertyGridViewModel;
        set => DataContext = value;
    }

    private void ColorPicker_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ColorPickerButton button || button.DataContext is not LuaPropertyRowViewModel row)
            return;

        var initialColor = new Vector3(row.ColorR, row.ColorG, row.ColorB) / 255.0f;
        var result = _colorPicker.PickColor(initialColor, color => SetColor(row, color));
        SetColor(row, result ?? initialColor);
    }

    private static void SetColor(LuaPropertyRowViewModel row, Vector3 color)
    {
        row.ColorR = (byte)Math.Clamp((int)Math.Round(color.X * 255.0f), 0, 255);
        row.ColorG = (byte)Math.Clamp((int)Math.Round(color.Y * 255.0f), 0, 255);
        row.ColorB = (byte)Math.Clamp((int)Math.Round(color.Z * 255.0f), 0, 255);
    }

    private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.Items.Count == 0)
            return;

        int newIndex = comboBox.SelectedIndex + (e.Delta > 0 ? -1 : 1);
        comboBox.SelectedIndex = Math.Clamp(newIndex, 0, comboBox.Items.Count - 1);
        e.Handled = true;
    }

    private void PropertyName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement element && element.DataContext is LuaPropertyRowViewModel row)
        {
            row.ResetToDefault();
            e.Handled = true;
        }
    }
}

internal sealed class StringEmptyToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

internal sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? 1.0 : 0.4;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

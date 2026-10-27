using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DarkUI.WPF.Converters;

public class InverseBoolToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

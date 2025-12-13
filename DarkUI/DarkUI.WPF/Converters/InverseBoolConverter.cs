using System;
using System.Globalization;
using System.Windows.Data;

namespace DarkUI.WPF.Converters;

public class InverseBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is bool boolean ? !boolean : value;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is bool boolean ? !boolean : value;
}

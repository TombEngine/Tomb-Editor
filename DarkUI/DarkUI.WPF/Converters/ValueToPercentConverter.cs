using System;
using System.Globalization;
using System.Windows.Data;

namespace DarkUI.WPF.Converters;

public class ValueToPercentConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		double value = System.Convert.ToDouble(values[0]);
		double maximum = System.Convert.ToDouble(values[1]);

		return (int)Math.Ceiling(value / maximum * 100);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace DarkUI.WPF.Converters;

public class AddValueConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		object result = value;

		if (value != null && targetType == typeof(double) &&
			double.TryParse(parameter.ToString(), NumberStyles.Integer, culture, out double parameterValue))
		{
			result = (double)value + parameterValue;
		}

		return result;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

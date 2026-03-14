using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace TombEditor.WPF.Converters
{
	public class HtmlToUIColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var colString = (string)value;
			var col = ColorTranslator.FromHtml(colString);
			return System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var col = (System.Windows.Media.Color)value;
			return ColorTranslator.ToHtml(Color.FromArgb(col.A, col.R, col.G, col.B));
		}
	}
}

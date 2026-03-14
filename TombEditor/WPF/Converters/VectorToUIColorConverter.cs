using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
namespace TombEditor.WPF.Converters
{
	public class VectorToUIColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var vec = (Vector4)value;
			byte r = (byte)(Math.Clamp(vec.X, 0.0f, 1.0f) * 255.0f);
			byte g = (byte)(Math.Clamp(vec.Y, 0.0f, 1.0f) * 255.0f);
			byte b = (byte)(Math.Clamp(vec.Z, 0.0f, 1.0f) * 255.0f);
			byte a = (byte)(Math.Clamp(vec.W, 0.0f, 1.0f) * 255.0f);
			return Color.FromArgb(a, r, g, b);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var col = (Color)value;
			return new Vector4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
		}
	}

	public class VectorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var vec = (Vector4)value;
			byte r = (byte)(Math.Clamp(vec.X, 0.0f, 1.0f) * 255.0f);
			byte g = (byte)(Math.Clamp(vec.Y, 0.0f, 1.0f) * 255.0f);
			byte b = (byte)(Math.Clamp(vec.Z, 0.0f, 1.0f) * 255.0f);
			byte a = (byte)(Math.Clamp(vec.W, 0.0f, 1.0f) * 255.0f);

			return new SolidColorBrush(Color.FromArgb(a, r, g, b));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color col = ((SolidColorBrush)value).Color;
			return new Vector4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
		}
	}
}

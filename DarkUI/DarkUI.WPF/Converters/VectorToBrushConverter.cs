using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;
using System.Windows.Media;

namespace DarkUI.WPF.Converters;

public class VectorToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Vector4 vec)
			return Binding.DoNothing;

		byte r = (byte)(Math.Clamp(vec.X, 0.0f, 1.0f) * 255.0f);
		byte g = (byte)(Math.Clamp(vec.Y, 0.0f, 1.0f) * 255.0f);
		byte b = (byte)(Math.Clamp(vec.Z, 0.0f, 1.0f) * 255.0f);
		byte a = (byte)(Math.Clamp(vec.W, 0.0f, 1.0f) * 255.0f);

		var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
		brush.Freeze();
		return brush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not SolidColorBrush brush)
			return Binding.DoNothing;

		Color col = brush.Color;
		return new Vector4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
	}
}

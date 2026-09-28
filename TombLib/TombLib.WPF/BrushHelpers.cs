using System;
using System.Windows.Media;

namespace TombLib.WPF;

public static class BrushHelpers
{
	public static SolidColorBrush CreateFrozenBrush(Color color)
	{
		var brush = new SolidColorBrush(color);
		brush.Freeze();
		return brush;
	}

	public static SolidColorBrush CreateFrozenBrush(string colorValue)
	{
		if (string.IsNullOrWhiteSpace(colorValue))
			throw new ArgumentException("Color value must not be empty.", nameof(colorValue));

		object converted = ColorConverter.ConvertFromString(colorValue);

		if (converted is not Color color)
			throw new ArgumentException($"'{colorValue}' is not a valid color.", nameof(colorValue));

		var brush = new SolidColorBrush(color);
		brush.Freeze();
		return brush;
	}

	public static Pen CreateFrozenPen(Brush brush, double thickness)
	{
		var pen = new Pen(brush, thickness);
		pen.Freeze();
		return pen;
	}

	public static Pen CreateFrozenPen(Color color, double thickness)
	{
		return CreateFrozenPen(CreateFrozenBrush(color), thickness);
	}
}
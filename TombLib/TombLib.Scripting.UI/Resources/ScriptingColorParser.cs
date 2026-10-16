using System;
using System.Windows.Media;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Parses color strings used by scripting color schemes, falling back to a safe color when the
/// value is empty or malformed so that applying settings or building highlighting rules never
/// crashes on user-edited color data.
/// </summary>
public static class ScriptingColorParser
{
	/// <summary>
	/// Gets the fallback color used for the editor background.
	/// </summary>
	public static readonly Color DefaultBackgroundColor = Colors.Black;

	/// <summary>
	/// Gets the fallback color used for the editor foreground.
	/// </summary>
	public static readonly Color DefaultForegroundColor = Colors.White;

	/// <summary>
	/// Gets the fallback color used for highlighted text.
	/// </summary>
	public static readonly Color DefaultHighlightingColor = Colors.White;

	/// <summary>
	/// Attempts to parse a color from a string.
	/// </summary>
	/// <param name="colorValue">The color value to parse.</param>
	/// <param name="color">The parsed color when successful; otherwise, the default color.</param>
	/// <returns><c>true</c> when the value parsed to a color; otherwise, <c>false</c>.</returns>
	public static bool TryParseColor(string? colorValue, out Color color)
	{
		color = default;

		if (string.IsNullOrWhiteSpace(colorValue))
			return false;

		try
		{
			if (ColorConverter.ConvertFromString(colorValue) is Color parsedColor)
			{
				color = parsedColor;
				return true;
			}
		}
		catch (FormatException)
		{ }
		catch (NotSupportedException)
		{ }

		return false;
	}

	/// <summary>
	/// Parses a color from a string, returning <paramref name="fallbackColor"/> when the value
	/// is empty or malformed.
	/// </summary>
	/// <param name="colorValue">The color value to parse.</param>
	/// <param name="fallbackColor">The color returned when the value cannot be parsed.</param>
	/// <returns>The parsed color, or <paramref name="fallbackColor"/>.</returns>
	public static Color ParseColorOrDefault(string? colorValue, Color fallbackColor)
		=> TryParseColor(colorValue, out Color color) ? color : fallbackColor;

	/// <summary>
	/// Creates a frozen brush from a color string, falling back to a safe color when the value
	/// is empty or malformed.
	/// </summary>
	/// <param name="colorValue">The color value to parse.</param>
	/// <param name="fallbackColor">The color used when the value cannot be parsed.</param>
	/// <returns>A frozen brush of the parsed or fallback color.</returns>
	public static SolidColorBrush CreateBrush(string? colorValue, Color fallbackColor)
		=> CreateFrozenBrush(ParseColorOrDefault(colorValue, fallbackColor));
}

using System.Windows.Media;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Provides the frozen brushes used by the text editor tooltips and completion popups.
/// </summary>
public static class TextEditorColorPalette
{
	/// <summary>
	/// Gets the border brush of a completion tooltip.
	/// </summary>
	public static readonly SolidColorBrush ToolTipBorder = CreateFrozenBrush(Color.FromRgb(96, 96, 96));

	/// <summary>
	/// Gets the background brush of a completion tooltip.
	/// </summary>
	public static readonly SolidColorBrush ToolTipBackground = CreateFrozenBrush(Color.FromRgb(64, 64, 64));

	/// <summary>
	/// Gets the foreground brush of a completion tooltip.
	/// </summary>
	public static readonly SolidColorBrush ToolTipForeground = CreateFrozenBrush(Colors.Gainsboro);

	/// <summary>
	/// Gets the foreground brush of the completion detail text.
	/// </summary>
	public static readonly SolidColorBrush CompletionDetailForeground = CreateFrozenBrush(Color.FromRgb(176, 176, 176));

	/// <summary>
	/// Gets the border brush of an error tooltip.
	/// </summary>
	public static readonly SolidColorBrush ErrorToolTipBorder = CreateFrozenBrush(Color.FromRgb(128, 86, 86));

	/// <summary>
	/// Gets the background brush of an error tooltip.
	/// </summary>
	public static readonly SolidColorBrush ErrorToolTipBackground = CreateFrozenBrush(Color.FromRgb(78, 44, 44));

	/// <summary>
	/// Gets the border brush of a warning tooltip.
	/// </summary>
	public static readonly SolidColorBrush WarningToolTipBorder = CreateFrozenBrush(Color.FromRgb(145, 122, 62));

	/// <summary>
	/// Gets the background brush of a warning tooltip.
	/// </summary>
	public static readonly SolidColorBrush WarningToolTipBackground = CreateFrozenBrush(Color.FromRgb(86, 69, 30));

	/// <summary>
	/// Gets the border brush of an information tooltip.
	/// </summary>
	public static readonly SolidColorBrush InformationToolTipBorder = CreateFrozenBrush(Color.FromRgb(80, 118, 168));

	/// <summary>
	/// Gets the background brush of an information tooltip.
	/// </summary>
	public static readonly SolidColorBrush InformationToolTipBackground = CreateFrozenBrush(Color.FromRgb(46, 68, 104));

	/// <summary>
	/// Gets the border brush of a hint tooltip.
	/// </summary>
	public static readonly SolidColorBrush HintToolTipBorder = CreateFrozenBrush(Color.FromRgb(108, 108, 108));

	/// <summary>
	/// Gets the background brush of a hint tooltip.
	/// </summary>
	public static readonly SolidColorBrush HintToolTipBackground = CreateFrozenBrush(Color.FromRgb(58, 58, 58));
}

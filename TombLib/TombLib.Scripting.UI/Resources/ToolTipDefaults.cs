using System;
using System.Windows;

namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Defines shared sizing constants for editor tooltips and popups.
/// </summary>
public static class ToolTipDefaults
{
	/// <summary>
	/// The maximum width of a tooltip or popup shell.
	/// </summary>
	public const double PopupMaxWidth = 540.0;

	/// <summary>
	/// The maximum width of the wrapped text inside a tooltip.
	/// </summary>
	public const double TextMaxWidth = 500.0;

	/// <summary>
	/// The maximum height of a tooltip.
	/// </summary>
	public const double PopupMaxHeight = 420.0;

	/// <summary>
	/// The font size used for tooltip body text.
	/// </summary>
	public static readonly double TextFontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0);
}

using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System.Windows.Media;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Rendering;

/// <summary>
/// Static helper for tooltip content creation and diagnostic color resolution,
/// extracted from <see cref="Bases.TextEditorBase"/>.
/// </summary>
public static class TextEditorToolTipHelper
{
	private static readonly SolidColorBrush ErrorToolTipBorder = TextEditorColorPalette.ToolTipBorder;
	private static readonly SolidColorBrush ErrorToolTipBackground = TextEditorColorPalette.ErrorToolTipBackground;
	private static readonly SolidColorBrush WarningToolTipBorder = TextEditorColorPalette.WarningToolTipBorder;
	private static readonly SolidColorBrush WarningToolTipBackground = TextEditorColorPalette.WarningToolTipBackground;
	private static readonly SolidColorBrush InformationToolTipBorder = TextEditorColorPalette.InformationToolTipBorder;
	private static readonly SolidColorBrush InformationToolTipBackground = TextEditorColorPalette.InformationToolTipBackground;
	private static readonly SolidColorBrush HintToolTipBorder = TextEditorColorPalette.HintToolTipBorder;
	private static readonly SolidColorBrush HintToolTipBackground = TextEditorColorPalette.HintToolTipBackground;

	/// <summary>
	/// Resolves diagnostic tooltip border and background colors for the given severity.
	/// </summary>
	public static void GetDiagnosticToolTipColors(TextDiagnosticSeverity severity,
		out SolidColorBrush border, out SolidColorBrush background)
	{
		switch (severity)
		{
			case TextDiagnosticSeverity.Warning:
				border = WarningToolTipBorder;
				background = WarningToolTipBackground;
				break;

			case TextDiagnosticSeverity.Information:
				border = InformationToolTipBorder;
				background = InformationToolTipBackground;
				break;

			case TextDiagnosticSeverity.Hint:
				border = HintToolTipBorder;
				background = HintToolTipBackground;
				break;

			default:
				border = ErrorToolTipBorder;
				background = ErrorToolTipBackground;
				break;
		}
	}

	/// <summary>
	/// Creates a plain-text tooltip content element.
	/// </summary>
	public static object CreatePlainToolTipContent(string content, Brush foreground)
		=> MarkdownToolTipRenderer.CreatePlainTextContent(content, foreground);

	/// <summary>
	/// Creates a markdown-styled tooltip content element.
	/// </summary>
	public static object CreateMarkdownToolTipContent(string content, Brush foreground, Brush background)
	{
		if (string.IsNullOrWhiteSpace(content))
			return CreatePlainToolTipContent(string.Empty, foreground);

		return MarkdownToolTipRenderer.CreateContent(content, foreground, background);
	}

	/// <summary>
	/// Normalizes line endings in tooltip text to LF.
	/// </summary>
	public static string NormalizeToolTipLineEndings(string text)
		=> MarkdownToolTipRenderer.NormalizeLineEndings(text);
}

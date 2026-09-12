using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TombLib.Scripting.UI.Rendering;

/// <summary>
/// Builds shared hover tooltip content for plain, markdown, and combined hover-diagnostic displays.
/// </summary>
public static class TextHoverToolTipContentFactory
{
	/// <summary>
	/// Creates the visual content for a hover tooltip.
	/// </summary>
	public static FrameworkElement CreateHoverContent(TextHoverInfo hoverInfo, Brush foreground, Brush background)
	{
		return hoverInfo.ContentKind == TextHoverContentKind.Markdown
			? MarkdownToolTipRenderer.CreateContent(hoverInfo.Content, foreground, background)
			: MarkdownToolTipRenderer.CreatePlainTextContent(hoverInfo.Content, foreground);
	}

	/// <summary>
	/// Creates the visual content for a combined hover and diagnostic tooltip.
	/// </summary>
	public static FrameworkElement CreateCombinedContent(
		TextHoverInfo hoverInfo,
		TextEditorDiagnostic diagnosticInfo,
		Brush foreground,
		Brush background,
		double maxWidth,
		double fontSize,
		Func<TextEditorDiagnosticSeverity, (SolidColorBrush Border, SolidColorBrush Background)> getDiagnosticColors)
	{
		(SolidColorBrush diagnosticBorder, SolidColorBrush diagnosticBackground) = getDiagnosticColors(diagnosticInfo.Severity);

		var panel = new StackPanel { MaxWidth = maxWidth };
		panel.Children.Add(CreateHoverContent(hoverInfo, foreground, background));

		panel.Children.Add(new Border
		{
			Background = diagnosticBackground,
			BorderBrush = diagnosticBorder,
			BorderThickness = new Thickness(1.0),
			CornerRadius = new CornerRadius(3.0),
			Padding = new Thickness(8.0, 4.0, 8.0, 4.0),
			Margin = new Thickness(0.0, 6.0, 0.0, 0.0),
			Child = new TextBlock
			{
				Text = diagnosticInfo.Message,
				Foreground = foreground,
				FontFamily = SystemFonts.MessageFontFamily,
				FontSize = fontSize,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = maxWidth
			}
		});

		return panel;
	}
}

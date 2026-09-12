using ICSharpCode.AvalonEdit;
using Nickelony.IDEKit.AvalonEdit.Extras.Markdown;
using System;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;
using static TombLib.WPF.BrushHelpers;
using PackageMarkdownToolTipRenderer = Nickelony.IDEKit.AvalonEdit.Extras.Markdown.MarkdownToolTipRenderer;

namespace TombLib.Scripting.UI.Rendering;

/// <summary>
/// Renders markdown content into a WPF framework element for tooltips.
/// </summary>
public static class MarkdownToolTipRenderer
{
	private static readonly FontFamily BodyFontFamily = SystemFonts.MessageFontFamily;
	private static readonly FontFamily CodeFontFamily = new(TextEditorBaseDefaults.FontFamily);
	private static readonly double BodyFontSize = ToolTipDefaults.TextFontSize;
	private static readonly double CodeFontSize = Math.Max(BodyFontSize - 1.0, 13.0);
	private static readonly Brush DefaultForeground = TextEditorColorPalette.ToolTipForeground;
	private static readonly Brush DefaultBackground = TextEditorColorPalette.ToolTipBackground;
	private static readonly Brush DefaultLinkForeground = CreateFrozenBrush(Color.FromRgb(112, 192, 231));

	private static MarkdownToolTipTheme CreateTheme(Brush foreground, Brush? background)
		=> new()
		{
			BodyFontFamily = BodyFontFamily,
			BodyFontSize = BodyFontSize,
			CodeFontFamily = CodeFontFamily,
			CodeFontSize = CodeFontSize,
			Foreground = foreground ?? DefaultForeground,
			Background = background ?? DefaultBackground,
			LinkForeground = DefaultLinkForeground,
			MaxWidth = ToolTipDefaults.PopupMaxWidth,
			MaxHeight = ToolTipDefaults.PopupMaxHeight,
			TextMaxWidth = ToolTipDefaults.TextMaxWidth
		};

	private static MarkdownToolTipOptions CreateOptions(bool allowScrolling)
		=> new()
		{
			AllowScrolling = allowScrolling,
			InstallCustomHighlighting = TryInstallCustomHighlighting
		};

	private static bool TryInstallCustomHighlighting(TextEditor editor, string? language)
	{
		if (!string.Equals(language?.Trim(), "lua", StringComparison.OrdinalIgnoreCase))
			return false;

		if (LuaTextMateSyntaxHighlighting.TryInstall(editor, out _))
			return true;

		editor.SyntaxHighlighting = LuaFallbackHighlightingLoader.Load();
		return true;
	}

	/// <summary>
	/// Creates a WPF element that renders the given markdown content.
	/// </summary>
	/// <param name="content">The markdown content to render.</param>
	/// <param name="foreground">The foreground brush of the rendered content.</param>
	/// <param name="background">The background brush of the rendered content.</param>
	/// <param name="allowScrolling">Whether the rendered content may scroll.</param>
	/// <returns>The rendered framework element.</returns>
	public static FrameworkElement CreateContent(string content, Brush foreground, Brush background, bool allowScrolling = true)
		=> PackageMarkdownToolTipRenderer.CreateContent(content, CreateTheme(foreground, background), CreateOptions(allowScrolling));

	/// <summary>
	/// Creates a plain-text element that renders the given content.
	/// </summary>
	/// <param name="content">The text content to render.</param>
	/// <param name="foreground">The foreground brush of the rendered content.</param>
	/// <param name="allowScrolling">Whether the rendered content may scroll.</param>
	/// <returns>The rendered framework element.</returns>
	public static FrameworkElement CreatePlainTextContent(string content, Brush foreground, bool allowScrolling = true)
		=> PackageMarkdownToolTipRenderer.CreatePlainTextContent(content, CreateTheme(foreground, null), CreateOptions(allowScrolling));

	internal static TextEditor CreateCodeBlockEditor(string? language, string code, Brush foreground)
		=> PackageMarkdownToolTipRenderer.CreateCodeBlockEditor(language, code, CreateTheme(foreground, null), CreateOptions(true));

	internal static string NormalizeLineEndings(string text)
		=> (text ?? string.Empty)
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace('\r', '\n');
}

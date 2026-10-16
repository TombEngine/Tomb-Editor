using ICSharpCode.AvalonEdit;
using Nickelony.IDEKit.AvalonEdit.Markdown;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;
using static TombLib.WPF.BrushHelpers;
using PackageMarkdownToolTipRenderer = Nickelony.IDEKit.AvalonEdit.Markdown.MarkdownRenderer;

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

	private static MarkdownRenderTheme CreateTheme(Brush foreground, Brush? background)
		=> new()
		{
			BodyFontFamily = BodyFontFamily,
			BodyFontSize = BodyFontSize,
			CodeFontFamily = CodeFontFamily,
			CodeFontSize = CodeFontSize,
			Foreground = foreground ?? DefaultForeground,
			SurfaceBackground = background ?? DefaultBackground,
			LinkForeground = DefaultLinkForeground,
			MaxWidth = ToolTipDefaults.PopupMaxWidth,
			MaxHeight = ToolTipDefaults.PopupMaxHeight,
			CodeMaxWidth = ToolTipDefaults.TextMaxWidth
		};

	private static MarkdownRenderOptions CreateOptions(bool allowScrolling)
		=> new()
		{
			AllowScrolling = allowScrolling,
			CustomHighlightingInstaller = TryInstallCustomHighlighting,
			OpenExternalUri = OpenUriWithShell
		};

	private static bool TryInstallCustomHighlighting(TextEditor editor, string? language)
	{
		if (!string.Equals(language?.Trim(), "lua", StringComparison.OrdinalIgnoreCase))
			return false;

		if (LuaTextMateSyntaxHighlighting.TryInstall(editor, out LuaTextMateInstallation? installation))
		{
			// The installation owns a TextMate model with a background tokenizer thread, so it must be
			// disposed when the tooltip's code-block editor leaves the visual tree.
			editor.Unloaded += (_, _) => installation.Dispose();
			return true;
		}

		editor.SyntaxHighlighting = LuaFallbackHighlightingLoader.Load();
		return true;
	}

	private static bool OpenUriWithShell(Uri uri)
	{
		// The renderer never opens a link on its own, so the host supplies the external opener it
		// wants; tooltips open links through the operating system's default protocol handler.
		return Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }) is not null;
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

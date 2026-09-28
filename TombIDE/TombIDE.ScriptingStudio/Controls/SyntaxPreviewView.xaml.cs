#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Controls;

public partial class SyntaxPreviewView : UserControl
{
	public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(
		nameof(PreviewText),
		typeof(string),
		typeof(SyntaxPreviewView),
		new PropertyMetadata(string.Empty, OnPreviewPropertyChanged));

	public static readonly DependencyProperty CurrentArgumentIndexProperty = DependencyProperty.Register(
		nameof(CurrentArgumentIndex),
		typeof(int),
		typeof(SyntaxPreviewView),
		new PropertyMetadata(-1, OnPreviewPropertyChanged));

	private const double CharacterWidth = 7.0;

	private ClassicScriptEditorConfiguration? _config;
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();
	private int _viewStart;

	public SyntaxPreviewView()
	{
		InitializeComponent();
		ReloadSettings();
	}

	public string PreviewText
	{
		get => (string)GetValue(PreviewTextProperty);
		set => SetValue(PreviewTextProperty, value);
	}

	public int CurrentArgumentIndex
	{
		get => (int)GetValue(CurrentArgumentIndexProperty);
		set => SetValue(CurrentArgumentIndexProperty, value);
	}

	public void ReloadSettings()
	{
		_config = ConfigurationBase.Load<ClassicScriptEditorConfiguration>();
		Background = CreateBrush(_config.ColorScheme.Background);
		PreviewTextBlock.Foreground = CreateBrush(_config.ColorScheme.Values.HtmlColor);
		UpdatePresentation();
	}

	private static void OnPreviewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((SyntaxPreviewView)d).UpdatePresentation();

	private void UpdatePresentation()
	{
		if (_config is null)
			return;

		string text = PreviewText ?? string.Empty;
		PreviewTextBlock.Inlines.Clear();

		if (text.Length == 0)
		{
			ScrollViewer.ScrollToHorizontalOffset(0.0);
			_viewStart = 0;
			return;
		}

		Color[] colors = Enumerable.Repeat(CreateColor(_config.ColorScheme.Values.HtmlColor), text.Length).ToArray();

		ApplyColorPattern(colors, text, @"\[\b(" + string.Join("|", _commandCatalogService.Sections) + @"|Any)\b\]", _config.ColorScheme.Sections.HtmlColor);
		ApplyColorPattern(colors, text, @"\b(" + string.Join("|", _commandCatalogService.OldCommands) + @")\b\s*=", _config.ColorScheme.StandardCommands.HtmlColor);
		ApplyColorPattern(colors, text, @"\b(" + string.Join("|", _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#'))) + @")\b\s*=", _config.ColorScheme.NewCommands.HtmlColor);
		ApplyColorPattern(colors, text, "(ENABLED|DISABLED|#INCLUDE|#DEFINE|#FIRST_ID)", _config.ColorScheme.References.HtmlColor);
		ApplyColorPattern(colors, text, @"\(.*?_.*?\)", _config.ColorScheme.References.HtmlColor);
		ApplyColorPattern(colors, text, @"(,|/|\(\*Array\*\))", _config.ColorScheme.Foreground);
		ApplyColorPattern(colors, text, ";.*$", _config.ColorScheme.Comments.HtmlColor);

		bool[] underlines = new bool[text.Length];

		if (TryGetCurrentArgumentRange(text, out int selectionStart, out int selectionLength))
		{
			for (int i = selectionStart; i < selectionStart + selectionLength && i < underlines.Length; i++)
				underlines[i] = true;
		}
		else
		{
			selectionStart = -1;
			selectionLength = 0;
		}

		BuildInlines(text, colors, underlines);

		Dispatcher.BeginInvoke(() => ScrollToSelectedArgument(text, selectionStart, selectionLength), DispatcherPriority.Loaded);
	}

	private void BuildInlines(string text, Color[] colors, bool[] underlines)
	{
		int segmentStart = 0;

		for (int i = 1; i <= text.Length; i++)
		{
			bool isSameStyle = i < text.Length
				&& colors[i] == colors[segmentStart]
				&& underlines[i] == underlines[segmentStart];

			if (isSameStyle)
				continue;

			var run = new Run(text.Substring(segmentStart, i - segmentStart))
			{
				Foreground = new SolidColorBrush(colors[segmentStart])
			};

			if (underlines[segmentStart])
				run.TextDecorations = TextDecorations.Underline;

			PreviewTextBlock.Inlines.Add(run);
			segmentStart = i;
		}
	}

	private void ApplyColorPattern(Color[] colors, string text, string regexPattern, string htmlColor)
	{
		Color color = CreateColor(htmlColor);

		foreach (Match match in Regex.Matches(text, regexPattern))
		{
			for (int i = match.Index; i < match.Index + match.Length && i < colors.Length; i++)
				colors[i] = color;
		}
	}

	private bool TryGetCurrentArgumentRange(string text, out int selectionStart, out int selectionLength)
	{
		selectionStart = -1;
		selectionLength = 0;

		if (string.IsNullOrWhiteSpace(text) || !text.Contains('=') || CurrentArgumentIndex < 0)
			return false;

		string[] arguments = text.Split(',');

		if (arguments.Length <= 1 || CurrentArgumentIndex >= arguments.Length)
			return false;

		string currentArgument = arguments[CurrentArgumentIndex];
		string token = CurrentArgumentIndex == 0
			? currentArgument.Split('=')[1].Trim()
			: currentArgument.Trim();

		if (string.IsNullOrWhiteSpace(token))
			return false;

		var previousArguments = new List<string>();

		for (int i = 0; i < CurrentArgumentIndex; i++)
			previousArguments.Add(arguments[i]);

		int startIndex = string.Join(",", previousArguments).Length;
		selectionStart = text.IndexOf(token, startIndex, StringComparison.Ordinal);

		if (selectionStart < 0)
			return false;

		selectionLength = token.Length;
		return true;
	}

	private void ScrollToSelectedArgument(string text, int selectionStart, int selectionLength)
	{
		if (selectionStart < 0 || selectionLength <= 0)
		{
			ScrollViewer.ScrollToHorizontalOffset(0.0);
			_viewStart = 0;
			return;
		}

		int selectionEnd = selectionStart + selectionLength;
		string textBeforeArgument = text[..selectionStart];
		string[] previousArguments = textBeforeArgument.Split(',');

		string textAfterArgument = selectionEnd >= text.Length ? string.Empty : text[selectionEnd..];
		string[] nextSegments = textAfterArgument.Split(',');
		int nextArgumentTextLength = string.IsNullOrEmpty(textAfterArgument) || nextSegments.Length < 2
			? 0
			: nextSegments[1].Length + 2;

		int visibleCharCount = Math.Max(1, (int)Math.Floor(ScrollViewer.ViewportWidth / CharacterWidth));
		int visibleRangeEnd = _viewStart + visibleCharCount;

		if (selectionEnd > visibleRangeEnd - nextArgumentTextLength)
		{
			int charsToScroll = selectionEnd - visibleCharCount + nextArgumentTextLength;
			ScrollViewer.ScrollToHorizontalOffset(charsToScroll * CharacterWidth);
			_viewStart = Math.Max(0, charsToScroll);
		}
		else if (selectionStart < _viewStart || previousArguments.Length == 1)
		{
			int charsToScroll = previousArguments.Length == 1 ? 0 : selectionStart - 2;
			ScrollViewer.ScrollToHorizontalOffset(Math.Max(0, charsToScroll) * CharacterWidth);
			_viewStart = Math.Max(0, charsToScroll);
		}
	}

	private static Brush CreateBrush(string htmlColor)
		=> new SolidColorBrush(CreateColor(htmlColor));

	private static Color CreateColor(string htmlColor)
		=> (Color)ColorConverter.ConvertFromString(htmlColor);
}

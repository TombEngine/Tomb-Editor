#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Providers;
using TombLib.Scripting.UI.Resources;

namespace TombIDE.ScriptingStudio.Settings;

public sealed partial class ScriptingSettingsWindowViewModel : ObservableObject
{
	private readonly ConfigurationCollection _configs = new();
	private readonly ClassicScriptLanguageServices _languageServices;
	private readonly GameFlowLanguageServices _gameFlowLanguageServices;
	private readonly TRXLanguageServices _trxLanguageServices;

	public ScriptingSettingsWindowViewModel(
		ScriptingWorkspaceProfile workspaceProfile,
		ScriptingDocumentRegistration? documentRegistration,
		bool luaEnabled,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);

		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
		SupportsLuaActivation = workspaceProfile.SupportsLuaActivation;
		LuaEnabled = luaEnabled;

		string[] fontFamilies = Fonts.SystemFontFamilies
			.Select(static family => family.Source)
			.OrderBy(static family => family, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		Pages = new ObservableCollection<ScriptingSettingsPageViewModel>(CreatePages(workspaceProfile, fontFamilies, languageServices, gameFlowLanguageServices, trxLanguageServices));
		SelectedPage = SelectInitialPage(workspaceProfile, documentRegistration) ?? Pages.FirstOrDefault();
	}

	public ObservableCollection<ScriptingSettingsPageViewModel> Pages { get; }

	public string WindowTitle => "Script Editor Settings";

	public bool SupportsLuaActivation { get; }

	public string LuaActivationDescription => "Lua documents are enabled when the scripting workspace is reopened.";

	[ObservableProperty]
	private bool? _dialogResult;

	[ObservableProperty]
	private ScriptingSettingsPageViewModel? _selectedPage;

	[ObservableProperty]
	private bool _luaEnabled;

	[RelayCommand]
	private void Cancel()
		=> DialogResult = false;

	[RelayCommand]
	private void ResetCurrentPage()
		=> SelectedPage?.ResetToDefaults();

	[RelayCommand]
	private void Save()
	{
		foreach (ScriptingSettingsPageViewModel page in Pages)
			page.ApplyChanges();

		_configs.SaveAllConfigs();
		DialogResult = true;
	}

	public void RefreshSelectedPagePreview()
		=> SelectedPage?.RefreshPreviewPresentation();

	partial void OnSelectedPageChanged(ScriptingSettingsPageViewModel? value)
		=> value?.RefreshPreviewPresentation();

	private IEnumerable<ScriptingSettingsPageViewModel> CreatePages(ScriptingWorkspaceProfile workspaceProfile, IReadOnlyList<string> fontFamilies, ClassicScriptLanguageServices languageServices, GameFlowLanguageServices gameFlowLanguageServices, TRXLanguageServices trxLanguageServices)
	{
		var orderedKinds = new List<ScriptingSettingsPageKind>();

		foreach (ScriptingWorkspaceSettingsPage page in workspaceProfile.SettingsPages)
		{
			if (!orderedKinds.Contains(page.Kind))
				orderedKinds.Add(page.Kind);
		}

		foreach (ScriptingSettingsPageKind kind in orderedKinds)
		{
			yield return new ScriptingSettingsPageViewModel(
				kind,
				GetTitle(workspaceProfile, kind),
				fontFamilies,
				GetConfiguration(kind),
				languageServices,
				gameFlowLanguageServices,
				trxLanguageServices);
		}
	}

	private ScriptingSettingsPageViewModel? SelectInitialPage(
		ScriptingWorkspaceProfile workspaceProfile,
		ScriptingDocumentRegistration? documentRegistration)
	{
		ScriptingSettingsPageKind? preferredKind = documentRegistration?.Contributions.SettingsPageKind;

		return Pages.FirstOrDefault(page => page.Kind == preferredKind)
			?? Pages.FirstOrDefault();
	}

	private static string GetTitle(ScriptingWorkspaceProfile workspaceProfile, ScriptingSettingsPageKind kind)
	{
		return workspaceProfile.SettingsPages.FirstOrDefault(page => page.Kind == kind)?.Title
			?? kind switch
			{
				ScriptingSettingsPageKind.ClassicScript => "TR4 / TRNG Script",
				ScriptingSettingsPageKind.GameFlowScript => "TR2 / TR3 Script",
				ScriptingSettingsPageKind.TRX => "TRX Script",
				ScriptingSettingsPageKind.Lua => "Lua",
				_ => kind.ToString()
			};
	}

	private TextEditorConfigBase GetConfiguration(ScriptingSettingsPageKind kind) => kind switch
	{
		ScriptingSettingsPageKind.ClassicScript => _configs.ClassicScript,
		ScriptingSettingsPageKind.GameFlowScript => _configs.GameFlowScript,
		ScriptingSettingsPageKind.TRX => _configs.TRX,
		ScriptingSettingsPageKind.Lua => _configs.Lua,
		_ => throw new NotSupportedException($"Unsupported settings page kind: {kind}.")
	};
}

public sealed class ScriptingSettingsPageViewModel : ObservableObject
{
	private const string ClassicPreviewSection = "[Level]";

	private const string LuaPreviewText =
		"---@class Weapon\n"
		+ "local Weapon = {}\n"
		+ "global levelName = \"Lara\"\n\n"
		+ "function Weapon:new(name)\n"
		+ "    local damage = math.max(levelName and 1 or 0, 1)\n"
		+ "    self.name = name\n"
		+ "    return damage, \"mods\\\\ten\\\\preview\", true, TEN\n"
		+ "end";

	private static readonly string[] LuaPreviewLines = LuaPreviewText.Replace("\r", string.Empty).Split('\n');
	private static readonly IReadOnlyList<SemanticToken> LuaPreviewTokens = CreateLuaPreviewTokens();

	private readonly TextEditorConfigBase _config;
	private readonly int _undoStackSize;

	// Editor option toggles.
	private bool _autoAddCommas;
	private bool _autoCloseBraces;
	private bool _autoCloseBrackets;
	private bool _autoCloseDoubleQuotes;
	private bool _autoCloseParentheses;
	private bool _autoCloseSingleQuotes;
	private bool _completionEnabled;
	private bool _diagnosticsUnderliningEnabled;
	private string _fontFamily = string.Empty;
	private double _fontSize;
	private bool _highlightCurrentLine;
	private bool _intelliSenseEnabled;
	private bool _collapseMultipleSpaces;
	private string _selectedThemeName = string.Empty;
	private bool _showLineNumbers;
	private bool _showSectionSeparators;
	private bool _showVisibleSpaces;
	private bool _showVisibleTabs;
	private bool _signatureHelpPopupsEnabled;
	private bool _spaceAfterComma;
	private bool _spaceAfterEquals;
	private bool _spaceBeforeComma;
	private bool _spaceBeforeEquals;
	private bool _wordWrapping;
	private readonly ClassicScriptLanguageServices _languageServices;
	private readonly GameFlowLanguageServices _gameFlowLanguageServices;
	private readonly TRXLanguageServices _trxLanguageServices;

	public ScriptingSettingsPageViewModel(
		ScriptingSettingsPageKind kind,
		string title,
		IReadOnlyList<string> fontFamilies,
		TextEditorConfigBase config,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		Kind = kind;
		Title = title;
		FontFamilies = fontFamilies;
		_config = config;
		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
		_undoStackSize = Math.Max(config.UndoStackSize, TextEditorBaseDefaults.UndoStackSize);

		Description = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => "Tune readability, theme presets, and the simplified Classic Script formatting rules without opening a separate tool-specific dialog.",
			ScriptingSettingsPageKind.GameFlowScript => "Keep the TR2/TR3 script profile focused on editor clarity, typography, and preset-based theme selection.",
			ScriptingSettingsPageKind.TRX => "Configure the JSON5-oriented TRX editor while keeping its format-specific conveniences grouped in one place.",
			ScriptingSettingsPageKind.Lua => "Manage Lua IntelliSense, closing rules, editor presentation, and theme presets from one WPF surface.",
			_ => string.Empty
		};

		SupportsDiagnosticsUnderlining = kind != ScriptingSettingsPageKind.GameFlowScript;
		SupportsSignatureHelpPopups = kind == ScriptingSettingsPageKind.Lua;
		SupportsSectionSeparators = kind == ScriptingSettingsPageKind.ClassicScript;
		SupportsClassicFormatting = kind == ScriptingSettingsPageKind.ClassicScript;
		SupportsAutoAddCommas = kind == ScriptingSettingsPageKind.TRX;

		ThemeOptions = LoadThemeOptions(kind, config);
		PreviewEditor = CreatePreviewEditor(kind);

		LoadFromConfig(config);
		RefreshPreview();
	}

	public string Description { get; }

	public IReadOnlyList<string> FontFamilies { get; }

	public string FontFamily
	{
		get => _fontFamily;
		set => SetAndRefresh(ref _fontFamily, value);
	}

	public double FontSize
	{
		get => _fontSize;
		set => SetAndRefresh(ref _fontSize, value);
	}

	public ScriptingSettingsPageKind Kind { get; }

	public TextEditorBase PreviewEditor { get; }

	public string SelectedThemeName
	{
		get => _selectedThemeName;
		set => SetAndRefresh(ref _selectedThemeName, value);
	}

	public bool SupportsAutoAddCommas { get; }

	public bool SupportsClassicFormatting { get; }

	public bool SupportsDiagnosticsUnderlining { get; }

	public bool SupportsSectionSeparators { get; }

	public bool SupportsSignatureHelpPopups { get; }

	public IReadOnlyList<string> ThemeOptions { get; }

	public string Title { get; }

	public bool AutoAddCommas
	{
		get => _autoAddCommas;
		set => SetAndRefresh(ref _autoAddCommas, value);
	}

	public bool AutoCloseBraces
	{
		get => _autoCloseBraces;
		set => SetAndRefresh(ref _autoCloseBraces, value);
	}

	public bool AutoCloseBrackets
	{
		get => _autoCloseBrackets;
		set => SetAndRefresh(ref _autoCloseBrackets, value);
	}

	public bool AutoCloseDoubleQuotes
	{
		get => _autoCloseDoubleQuotes;
		set => SetAndRefresh(ref _autoCloseDoubleQuotes, value);
	}

	public bool AutoCloseParentheses
	{
		get => _autoCloseParentheses;
		set => SetAndRefresh(ref _autoCloseParentheses, value);
	}

	public bool AutoCloseSingleQuotes
	{
		get => _autoCloseSingleQuotes;
		set => SetAndRefresh(ref _autoCloseSingleQuotes, value);
	}

	public bool CompletionEnabled
	{
		get => _completionEnabled;
		set => SetAndRefresh(ref _completionEnabled, value);
	}

	public bool DiagnosticsUnderliningEnabled
	{
		get => _diagnosticsUnderliningEnabled;
		set => SetAndRefresh(ref _diagnosticsUnderliningEnabled, value);
	}

	public bool HighlightCurrentLine
	{
		get => _highlightCurrentLine;
		set => SetAndRefresh(ref _highlightCurrentLine, value);
	}

	public bool IntelliSenseEnabled
	{
		get => _intelliSenseEnabled;
		set
		{
			if (!SetAndRefresh(ref _intelliSenseEnabled, value))
				return;

			OnPropertyChanged(nameof(IsIntelliSenseDetailsEnabled));
		}
	}

	public bool IsIntelliSenseDetailsEnabled => IntelliSenseEnabled;

	public bool ShowLineNumbers
	{
		get => _showLineNumbers;
		set => SetAndRefresh(ref _showLineNumbers, value);
	}

	public bool ShowSectionSeparators
	{
		get => _showSectionSeparators;
		set => SetAndRefresh(ref _showSectionSeparators, value);
	}

	public bool ShowVisibleSpaces
	{
		get => _showVisibleSpaces;
		set => SetAndRefresh(ref _showVisibleSpaces, value);
	}

	public bool ShowVisibleTabs
	{
		get => _showVisibleTabs;
		set => SetAndRefresh(ref _showVisibleTabs, value);
	}

	public bool SignatureHelpPopupsEnabled
	{
		get => _signatureHelpPopupsEnabled;
		set => SetAndRefresh(ref _signatureHelpPopupsEnabled, value);
	}

	public bool SpaceAfterComma
	{
		get => _spaceAfterComma;
		set => SetAndRefresh(ref _spaceAfterComma, value);
	}

	public bool SpaceAfterEquals
	{
		get => _spaceAfterEquals;
		set => SetAndRefresh(ref _spaceAfterEquals, value);
	}

	public bool SpaceBeforeComma
	{
		get => _spaceBeforeComma;
		set => SetAndRefresh(ref _spaceBeforeComma, value);
	}

	public bool SpaceBeforeEquals
	{
		get => _spaceBeforeEquals;
		set => SetAndRefresh(ref _spaceBeforeEquals, value);
	}

	public bool CollapseMultipleSpaces
	{
		get => _collapseMultipleSpaces;
		set => SetAndRefresh(ref _collapseMultipleSpaces, value);
	}

	public bool WordWrapping
	{
		get => _wordWrapping;
		set => SetAndRefresh(ref _wordWrapping, value);
	}

	public void ApplyChanges()
		=> ApplyToConfig(_config);

	public void RefreshPreviewPresentation()
		=> InvalidatePreview();

	public void ResetToDefaults()
	{
		switch (Kind)
		{
			case ScriptingSettingsPageKind.ClassicScript:
				LoadFromConfig(new ClassicScriptEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.GameFlowScript:
				LoadFromConfig(new GameFlowEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.TRX:
				LoadFromConfig(new TRXEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.Lua:
				LoadFromConfig(new LuaEditorConfiguration());
				break;
		}

		RefreshPreview();
	}

	private void ApplySharedSettings(TextEditorConfigBase config)
	{
		config.FontSize = FontSize;
		config.FontFamily = string.IsNullOrWhiteSpace(FontFamily) ? TextEditorBaseDefaults.FontFamily : FontFamily;
		config.UndoStackSize = _undoStackSize;
		config.IntelliSenseEnabled = IntelliSenseEnabled;
		config.CompletionEnabled = CompletionEnabled;
		config.LiveErrorUnderlining = DiagnosticsUnderliningEnabled;
		config.SignatureHelpPopupsEnabled = SignatureHelpPopupsEnabled;
		config.AutoCloseParentheses = AutoCloseParentheses;
		config.AutoCloseBraces = AutoCloseBraces;
		config.AutoCloseBrackets = AutoCloseBrackets;
		config.AutoCloseDoubleQuotes = AutoCloseDoubleQuotes;
		config.AutoCloseSingleQuotes = AutoCloseSingleQuotes;
		config.WordWrapping = WordWrapping;
		config.HighlightCurrentLine = HighlightCurrentLine;
		config.ShowLineNumbers = ShowLineNumbers;
		config.ShowVisualSpaces = ShowVisibleSpaces;
		config.ShowVisualTabs = ShowVisibleTabs;
	}

	private void ApplyToConfig(TextEditorConfigBase config)
	{
		ApplySharedSettings(config);

		GetColorProvider().SetSelectedName(config, SelectedThemeName);

		switch (config)
		{
			case ClassicScriptEditorConfiguration classicConfig:
				classicConfig.ShowSectionSeparators = ShowSectionSeparators;
				classicConfig.SpaceBeforeEquals = SpaceBeforeEquals;
				classicConfig.SpaceAfterEquals = SpaceAfterEquals;
				classicConfig.SpaceBeforeComma = SpaceBeforeComma;
				classicConfig.SpaceAfterComma = SpaceAfterComma;
				classicConfig.CollapseMultipleSpaces = CollapseMultipleSpaces;
				break;

			case TRXEditorConfiguration trxConfig:
				trxConfig.AutoAddCommas = AutoAddCommas;
				break;
		}
	}

	private TextEditorBase CreatePreviewEditor(ScriptingSettingsPageKind kind)
	{
		TextEditorBase editor = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => new ClassicScriptEditor(new Version(0, 0), _languageServices),
			ScriptingSettingsPageKind.GameFlowScript => new GameFlowEditor(new Version(0, 0), _gameFlowLanguageServices),
			ScriptingSettingsPageKind.TRX => new TRXEditor(new Version(0, 0), _trxLanguageServices),
			ScriptingSettingsPageKind.Lua => new LuaEditor(new Version(0, 0)),
			_ => throw new NotSupportedException($"Unsupported preview editor kind: {kind}.")
		};

		editor.IsReadOnly = true;
		editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
		editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
		editor.TextArea.Margin = new Thickness(6);
		editor.Options.EnableHyperlinks = false;
		editor.Options.EnableEmailHyperlinks = false;

		return editor;
	}

	private TextEditorConfigBase CreatePreviewConfiguration()
	{
		TextEditorConfigBase previewConfig = Kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => new ClassicScriptEditorConfiguration(),
			ScriptingSettingsPageKind.GameFlowScript => new GameFlowEditorConfiguration(),
			ScriptingSettingsPageKind.TRX => new TRXEditorConfiguration(),
			ScriptingSettingsPageKind.Lua => new LuaEditorConfiguration(),
			_ => throw new NotSupportedException($"Unsupported preview config kind: {Kind}.")
		};

		ApplyToConfig(previewConfig);
		return previewConfig;
	}

	private string CreateClassicPreviewText()
	{
		string preEqual = SpaceBeforeEquals ? " " : string.Empty;
		string postEqual = SpaceAfterEquals ? " " : string.Empty;
		string preComma = SpaceBeforeComma ? " " : string.Empty;
		string postComma = SpaceAfterComma ? " " : string.Empty;
		string commentGap = CollapseMultipleSpaces ? " " : "  ";

		return ClassicPreviewSection + "\n"
			+ "Rain" + preEqual + "=" + postEqual + "ENABLED" + preComma + "," + postComma + "12" + commentGap + "; Has error\n"
			+ "Layer1" + preEqual + "=" + postEqual + "128" + preComma + "," + postComma + "128" + preComma + "," + postComma + ">\n"
			+ "\t\t128" + preComma + "," + postComma + "-8\n"
			+ "Mirror" + preEqual + "=" + postEqual + "69" + preComma + "," + postComma + "$2137\n"
			+ ClassicPreviewSection;
	}

	private string GetPreviewText() => Kind switch
	{
		ScriptingSettingsPageKind.ClassicScript => CreateClassicPreviewText(),
		ScriptingSettingsPageKind.GameFlowScript => "DESCRIPTION: Tomb Raider 2 Script File\n\nLEVEL: Scotland Temple\n\tGAME: data\\temp.tr2 // Good level\n\tSECRETS: 21\n\tTRACK: 37\nEND:",
		ScriptingSettingsPageKind.TRX => "\"levels\": [\n\t{\n\t\t\"title\": \"Vatican City\",\n\t\t\"file\": \"data\\\\level21.phd\",\n\t\t\"type\": \"normal\",\n\t\t\"music\": 37,\n\t\t\"demo\": true,\n\t}\n]",
		ScriptingSettingsPageKind.Lua => LuaPreviewText,
		_ => string.Empty
	};

	private static IReadOnlyList<string> LoadThemeOptions(ScriptingSettingsPageKind kind, TextEditorConfigBase config)
	{
		ITextEditorColorProvider colorProvider = GetColorProvider(kind);

		List<string> values = [.. colorProvider.GetAvailableNames()
			.Where(static option => !string.IsNullOrWhiteSpace(option))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(static option => option, StringComparer.OrdinalIgnoreCase)];

		string selectedValue = colorProvider.GetSelectedName(config);

		if (!string.IsNullOrWhiteSpace(selectedValue) && !values.Contains(selectedValue, StringComparer.OrdinalIgnoreCase))
			values.Add(selectedValue);

		return values;
	}

	private static ITextEditorColorProvider GetColorProvider(ScriptingSettingsPageKind kind) => kind switch
	{
		ScriptingSettingsPageKind.ClassicScript => new ClassicScriptColorSchemeProvider(),
		ScriptingSettingsPageKind.GameFlowScript => new GameFlowColorSchemeProvider(),
		ScriptingSettingsPageKind.TRX => new TRXColorSchemeProvider(),
		ScriptingSettingsPageKind.Lua => new LuaThemeProvider(),
		_ => throw new NotSupportedException($"Unsupported scripting settings page kind: {kind}.")
	};

	private ITextEditorColorProvider GetColorProvider()
		=> GetColorProvider(Kind);

	private void LoadFromConfig(TextEditorConfigBase config)
	{
		_fontSize = config.FontSize;
		_fontFamily = FontFamilies.Contains(config.FontFamily, StringComparer.OrdinalIgnoreCase)
			? FontFamilies.First(font => string.Equals(font, config.FontFamily, StringComparison.OrdinalIgnoreCase))
			: TextEditorBaseDefaults.FontFamily;
		_intelliSenseEnabled = config.IntelliSenseEnabled;
		_completionEnabled = config.CompletionEnabled;
		_diagnosticsUnderliningEnabled = config.LiveErrorUnderlining;
		_signatureHelpPopupsEnabled = config.SignatureHelpPopupsEnabled;
		_autoCloseParentheses = config.AutoCloseParentheses;
		_autoCloseBraces = config.AutoCloseBraces;
		_autoCloseBrackets = config.AutoCloseBrackets;
		_autoCloseDoubleQuotes = config.AutoCloseDoubleQuotes;
		_autoCloseSingleQuotes = config.AutoCloseSingleQuotes;
		_wordWrapping = config.WordWrapping;
		_highlightCurrentLine = config.HighlightCurrentLine;
		_showLineNumbers = config.ShowLineNumbers;
		_showVisibleSpaces = config.ShowVisualSpaces;
		_showVisibleTabs = config.ShowVisualTabs;

		_selectedThemeName = GetColorProvider().GetSelectedName(config);

		switch (config)
		{
			case ClassicScriptEditorConfiguration classicConfig:
				_showSectionSeparators = classicConfig.ShowSectionSeparators;
				_spaceBeforeEquals = classicConfig.SpaceBeforeEquals;
				_spaceAfterEquals = classicConfig.SpaceAfterEquals;
				_spaceBeforeComma = classicConfig.SpaceBeforeComma;
				_spaceAfterComma = classicConfig.SpaceAfterComma;
				_collapseMultipleSpaces = classicConfig.CollapseMultipleSpaces;
				_autoAddCommas = false;
				break;

			case GameFlowEditorConfiguration:
			case LuaEditorConfiguration:
				_showSectionSeparators = false;
				_spaceBeforeEquals = false;
				_spaceAfterEquals = false;
				_spaceBeforeComma = false;
				_spaceAfterComma = false;
				_collapseMultipleSpaces = false;
				_autoAddCommas = false;
				break;

			case TRXEditorConfiguration trxConfig:
				_showSectionSeparators = false;
				_spaceBeforeEquals = false;
				_spaceAfterEquals = false;
				_spaceBeforeComma = false;
				_spaceAfterComma = false;
				_collapseMultipleSpaces = false;
				_autoAddCommas = trxConfig.AutoAddCommas;
				break;
		}

		OnPropertyChanged(string.Empty);
	}

	private void RefreshPreview()
	{
		PreviewEditor.UpdateSettings(CreatePreviewConfiguration());

		string previewText = GetPreviewText();

		if (!string.Equals(PreviewEditor.Text, previewText, StringComparison.Ordinal))
			PreviewEditor.Text = previewText;

		if (PreviewEditor is LuaEditor luaEditor)
			luaEditor.SetSemanticTokens(LuaPreviewTokens);

		InvalidatePreview();
	}

	private void InvalidatePreview()
	{
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		PreviewEditor.TextArea.TextView.Redraw();
		PreviewEditor.TextArea.TextView.InvalidateVisual();
		PreviewEditor.InvalidateVisual();
	}

	private bool SetAndRefresh<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (!SetProperty(ref field, value, propertyName))
			return false;

		RefreshPreview();
		return true;
	}

	private static IReadOnlyList<SemanticToken> CreateLuaPreviewTokens()
	{
		return
		[
			CreateLuaPreviewToken(0, "Weapon", "class"),
			CreateLuaPreviewToken(1, "Weapon", "class"),
			CreateLuaPreviewToken(2, "levelName", "variable", 1, "global"),
			CreateLuaPreviewToken(4, "Weapon", "class"),
			CreateLuaPreviewToken(4, "new", "method", 1, "declaration"),
			CreateLuaPreviewToken(4, "name", "parameter"),
			CreateLuaPreviewToken(5, "math", "namespace", 1, "defaultLibrary"),
			CreateLuaPreviewToken(5, "max", "function", 1, "defaultLibrary"),
			CreateLuaPreviewToken(5, "levelName", "variable", 1, "global"),
			CreateLuaPreviewToken(6, "name", "property", 1),
			CreateLuaPreviewToken(6, "name", "parameter", 2)
		];
	}

	private static SemanticToken CreateLuaPreviewToken(int lineIndex, string tokenText, string tokenType, params string[] modifiers)
		=> CreateLuaPreviewToken(lineIndex, tokenText, tokenType, 1, modifiers);

	private static SemanticToken CreateLuaPreviewToken(int lineIndex, string tokenText, string tokenType, int occurrence, params string[] modifiers)
	{
		int characterIndex = GetOccurrenceIndex(LuaPreviewLines[lineIndex], tokenText, occurrence);
		return new SemanticToken(lineIndex, characterIndex, tokenText.Length, tokenType, modifiers);
	}

	private static int GetOccurrenceIndex(string line, string tokenText, int occurrence)
	{
		int startIndex = -1;

		for (int currentOccurrence = 0; currentOccurrence < occurrence; currentOccurrence++)
		{
			startIndex = line.IndexOf(tokenText, startIndex + 1, StringComparison.Ordinal);

			if (startIndex < 0)
				throw new InvalidOperationException("Failed to locate preview token '" + tokenText + "'.");
		}

		return startIndex;
	}
}

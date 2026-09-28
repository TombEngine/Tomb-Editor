using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.IntelliSense.Navigation;
using NLog;
using System;
using System.Threading;
using TombLib.Scripting.Lua.Editing;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Provides a Lua-specific text editor with syntax highlighting, semantic coloring, and language-service integration.
/// </summary>
public sealed partial class LuaEditor : TextEditorBase
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private readonly LuaDefinitionNavigationController _definitionNavigationController;
	private readonly LuaHoverController _hoverController;
	private readonly LuaSignatureHelpController _signatureHelpController;

	/// <summary>
	/// Gets the default file extension associated with Lua documents.
	/// </summary>
	public override string DefaultFileExtension => ".lua";

	private LuaTextMateInstallation? _textMateHighlighting;
	private LuaThemeBrushSet? _themeBrushSet;
	private int _editorDocumentVersion;

	/// <summary>
	/// Gets or sets the IntelliSense provider used to supply completions, hover text, diagnostics, and navigation results.
	/// </summary>
	public ILanguageServerIntelliSenseProvider? IntelliSenseProvider { get; set; }

	/// <summary>
	/// Gets the monotonically increasing version of the logical document content snapshot. The
	/// version advances on every text change and is distinct from the session generation (operation
	/// ownership); the two counters are not interchangeable.
	/// </summary>
	public int DocumentVersion => Volatile.Read(ref _editorDocumentVersion);

	/// <summary>
	/// Occurs when the editor resolves a definition location that should be opened by the host application.
	/// </summary>
	public event Action<TextDefinitionLocation>? DefinitionNavigationRequested;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaEditor"/> class for the specified engine version.
	/// </summary>
	/// <param name="engineVersion">The engine version used to configure editor behavior.</param>
	public LuaEditor(Version engineVersion) : base(engineVersion)
	{
		CommentSyntax = new CommentSyntax("--", null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.SingleQuoted);
		TextArea.IndentationStrategy = new LuaAutoIndentationStrategy(Options);
		_definitionNavigationController = new(this);
		InitializeDefinitionNavigation(TryNavigateDefinitionAsync);
		_hoverController = new(this);
		_signatureHelpController = new(this);
		BindLuaIntelliSenseEvents();
	}

	/// <summary>
	/// Applies the active Lua theme, refreshes syntax highlighting, and updates shared editor settings.
	/// </summary>
	/// <param name="configuration">The editor configuration to apply.</param>
	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		EnsureNotDisposed();

		if (configuration is not LuaEditorConfiguration config)
			return;

		var theme = config.Theme ?? LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName);
		_themeBrushSet = LuaEditorColorPalette.Create(theme);
		_textMateHighlighting?.Dispose();
		_textMateHighlighting = null;

		LuaTextMateSyntaxHighlighting.TryInstall(this, theme.TextMateTheme, out _textMateHighlighting);
		SyntaxHighlighting = _textMateHighlighting is null
			? LuaTextMateSyntaxHighlighting.LoadFallbackHighlighting()
			: null;

		EnsureSemanticTokensColorizerAttached();

		Background = _themeBrushSet.EditorBackground;
		Foreground = _themeBrushSet.EditorForeground;

		base.UpdateSettings(configuration);

		if (!SignatureHelpPopupsEnabled)
			_signatureHelpController.Dismiss();
	}

	private LuaThemeBrushSet GetThemeBrushSet()
	{
		_themeBrushSet ??= LuaEditorColorPalette.Create(LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName));
		return _themeBrushSet;
	}
}

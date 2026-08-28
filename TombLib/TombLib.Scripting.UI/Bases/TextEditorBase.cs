using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.LanguageServer.Abstractions.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Cleaning;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Base class for language-specific script editors built on AvalonEdit.
/// </summary>
public abstract partial class TextEditorBase : TextEditor, IEditorControl
{
	private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

	private static readonly TextEditorFormattingService s_formattingService = new();

	/// <inheritdoc/>
	public EditorType EditorType => EditorType.Text;

	/// <summary>
	/// Gets the default file extension (including the leading dot) used for documents of this editor.
	/// </summary>
	public abstract string DefaultFileExtension { get; }

	/// <summary>
	/// Gets or sets the file path of the current document.
	/// </summary>
	public string FilePath
	{
		get => Document.FileName;
		set
		{
			Document.FileName = value;
			_contentPersistenceCoordinator.FilePath = value;
		}
	}

	/// <inheritdoc/>
	public bool IsSilentSession { get; set; }

	/// <summary>
	/// Gets or sets whether backup files are created for the current document.
	/// </summary>
	public bool CreateBackupFiles
	{
		get => IsSilentSession ? false : _contentPersistenceCoordinator.CreateBackupFiles;
		set => _contentPersistenceCoordinator.CreateBackupFiles = value;
	}

	/// <inheritdoc/>
	public string Content
	{
		get => Text;
		set => SetContent(value);
	}

	/// <summary>
	/// Gets or sets whether the content of the current document has unsaved changes.
	/// </summary>
	public bool IsContentChanged { get; set; }

	/// <summary>
	/// Gets or sets the timestamp of the last content modification.
	/// </summary>
	public DateTime LastModified { get; set; }

	/// <summary>
	/// Gets the line number of the caret position.
	/// </summary>
	public int CurrentRow => TextArea.Caret.Position.Line;

	/// <summary>
	/// Gets the column of the caret position.
	/// </summary>
	public int CurrentColumn => TextArea.Caret.Position.Column;

	/// <summary>
	/// Gets the currently selected content as text, or <c>null</c> when there is no selection.
	/// </summary>
	public string? SelectedContent => SelectedText.Length == 0 ? null : SelectedText;

	/// <summary>
	/// Gets the formatter used when tidying the document.
	/// </summary>
	protected virtual ITextDocumentFormatter DocumentFormatter => TrimTrailingWhitespaceFormatter.Instance;

	private int _minZoom = 25;

	/// <summary>
	/// Gets or sets the minimum allowed zoom percentage.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is less than or equal to zero.</exception>
	public int MinZoom
	{
		get => _minZoom;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
			_minZoom = value;
		}
	}

	private int _maxZoom = 400;

	/// <summary>
	/// Gets or sets the maximum allowed zoom percentage.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is less than or equal to zero.</exception>
	public int MaxZoom
	{
		get => _maxZoom;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
			_maxZoom = value;
		}
	}

	private int _zoomStepSize = 15;

	/// <summary>
	/// Gets or sets the zoom percentage change per step.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is less than or equal to zero.</exception>
	public int ZoomStepSize
	{
		get => _zoomStepSize;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
			_zoomStepSize = value;
		}
	}

	/// <summary>
	/// Gets or sets the prefix used to comment out lines.
	/// </summary>
	public string CommentPrefix { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the delay before the delayed text-changed notification fires.
	/// </summary>
	public TimeSpan TextChangedDelayedInterval
	{
		get => _contentPersistenceCoordinator.DelayedInterval;
		set => _contentPersistenceCoordinator.DelayedInterval = value;
	}

	private string _parenthesesClosingString = ")";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed parenthesis.
	/// </summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public string ParenthesesClosingString
	{
		get => _parenthesesClosingString;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_parenthesesClosingString = value;
		}
	}

	private string _bracesClosingString = "}";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed brace.
	/// </summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public string BracesClosingString
	{
		get => _bracesClosingString;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_bracesClosingString = value;
		}
	}

	private string _bracketsClosingString = "]";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed bracket.
	/// </summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public string BracketsClosingString
	{
		get => _bracketsClosingString;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_bracketsClosingString = value;
		}
	}

	private string _quotesClosingString = "\"";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed quote.
	/// </summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public string QuotesClosingString
	{
		get => _quotesClosingString;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_quotesClosingString = value;
		}
	}

	private Version _engineVersion = new(0, 0);

	/// <summary>
	/// Gets or sets the engine version targeted by this editor.
	/// </summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public Version EngineVersion
	{
		get => _engineVersion;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			_engineVersion = value;
		}
	}

	// Configuration

	/// <summary>
	/// Basically FontSize but zooming doesn't affect its value.
	/// </summary>
	public double DefaultFontSize { get; set; } = TextEditorBaseDefaults.FontSize;

	/// <summary>
	/// Gets or sets whether IntelliSense features are enabled for this editor.
	/// </summary>
	public bool IntelliSenseEnabled { get; set; } = TextEditorBaseDefaults.IntelliSenseEnabled;

	/// <summary>
	/// Gets or sets whether completion suggestions are shown while typing.
	/// </summary>
	public bool CompletionEnabled { get; set; } = TextEditorBaseDefaults.CompletionEnabled;

	/// <summary>
	/// Gets or sets whether errors are underlined as they are detected.
	/// </summary>
	public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

	/// <summary>
	/// Gets or sets whether signature help popups are shown.
	/// </summary>
	public bool SignatureHelpPopupsEnabled { get; set; } = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

	/// <summary>
	/// Gets or sets whether opening parentheses are auto-closed.
	/// </summary>
	public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;

	/// <summary>
	/// Gets or sets whether opening braces are auto-closed.
	/// </summary>
	public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;

	/// <summary>
	/// Gets or sets whether opening brackets are auto-closed.
	/// </summary>
	public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;

	/// <summary>
	/// Gets or sets whether double quotes are auto-closed.
	/// </summary>
	public bool AutoCloseDoubleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseDoubleQuotes;

	/// <summary>
	/// Gets or sets whether single quotes are auto-closed.
	/// </summary>
	public bool AutoCloseSingleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseSingleQuotes;

	// Fields

	/// <summary>
	/// The popup used to show special (non-text) tooltips over the editor.
	/// </summary>
	protected Popup _specialToolTip;

	private readonly BookmarkCoordinator _bookmarkCoordinator;
	private readonly TextAutoClosingService _autoClosingService;
	private readonly TextLineCommentService _commentService;
	private readonly CompletionWindowCoordinator _completionWindowCoordinator;
	private readonly ContentPersistenceCoordinator _contentPersistenceCoordinator;
	private readonly TextDefinitionNavigationService _definitionNavigationService;
	private readonly TextDiagnosticToolTipService _diagnosticToolTipService;
	private readonly TextEditorStatusCoordinator _statusCoordinator;
	private readonly EditorToolTipPresenter _toolTipPresenter;
	private readonly TextEditorViewService _viewService;

	private IBackgroundRenderer _bookmarkRenderer;
	private IBackgroundRenderer _errorRenderer;

	private TextDefinitionTriggerController? _definitionTriggerController;
	private TextHoverController? _hoverController;
	private TextDiagnosticsCoordinator? _diagnosticsCoordinator;
	private bool _isDisposed;

	/// <summary>
	/// Gets the diagnostics currently owned by this editor instance.
	/// </summary>
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnosticToolTipService.Diagnostics;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TextEditorBase"/> class for the given engine version.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets, or <see langword="null"/> when no engine version applies.</param>
	public TextEditorBase(Version? engineVersion = null)
	{
		TextEditorServiceComposition services = TextEditorServiceComposition.Create(this);

		SetNewDefaultSettings();
		_autoClosingService = services.AutoClosingService;
		_bookmarkCoordinator = services.BookmarkCoordinator;
		_commentService = services.CommentService;
		_completionWindowCoordinator = services.CompletionWindowCoordinator;
		_contentPersistenceCoordinator = services.ContentPersistenceCoordinator;
		_definitionNavigationService = services.DefinitionNavigationService;
		_diagnosticToolTipService = services.DiagnosticToolTipService;
		_statusCoordinator = services.StatusCoordinator;
		_toolTipPresenter = services.ToolTipPresenter;
		_viewService = services.ViewService;
		_specialToolTip = _toolTipPresenter.Popup;

		CompletionController = new TextCompletionController(this);

		InitializePersistenceCoordinator();
		InitializeRenderers();

		BindEventMethods();

		EngineVersion = engineVersion ?? new Version(0, 0);
	}

	private void SetNewDefaultSettings()
	{
		Options.AllowScrollBelowDocument = true;
		TextArea.Margin = new Thickness(3, 0, 0, 0);

		FontWeight = FontWeights.Normal;

		TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Color.FromArgb(16, 160, 160, 160));
		TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromArgb(24, 192, 192, 192)), 1);

		TextArea.SelectionCornerRadius = 0;
		TextArea.SelectionBorder = new Pen(Brushes.SteelBlue, 1);

		HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
	}

	private void InitializePersistenceCoordinator()
	{
		_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted += ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted;
		_contentPersistenceCoordinator.TextChangedDelayed += ContentPersistenceCoordinator_TextChangedDelayed;
	}

	[MemberNotNull(nameof(_bookmarkRenderer), nameof(_errorRenderer))]
	private void InitializeRenderers()
	{
		_bookmarkRenderer = new BookmarkRenderer(_bookmarkCoordinator);
		_errorRenderer = new ErrorRenderer(this);

		TextArea.TextView.BackgroundRenderers.Add(_bookmarkRenderer);
		TextArea.TextView.BackgroundRenderers.Add(_errorRenderer);
	}

	private void BindEventMethods()
	{
		_statusCoordinator.Attach();

		TextArea.TextEntering += TextArea_TextEntering;
		TextArea.TextEntered += TextEditor_TextEntered;
		TextChanged += TextEditor_TextChanged;

		AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
		AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);

		MouseHover += TextEditor_MouseHover;
		MouseHoverStopped += TextEditor_MouseHoverStopped;

		PreviewMouseWheel += TextEditor_PreviewMouseWheel;
		MouseRightButtonDown += TextEditor_MouseRightButtonDown;
	}

	// Settings

	/// <summary>
	/// Applies the given configuration to this editor.
	/// </summary>
	/// <param name="configuration">The configuration to apply.</param>
	public virtual void UpdateSettings(ConfigurationBase configuration)
	{
		EnsureNotDisposed();

		if (configuration is not TextEditorConfigBase config)
			return;

		FontSize = config.FontSize;
		DefaultFontSize = config.FontSize;
		FontFamily = new(config.FontFamily);

		Document.UndoStack.SizeLimit = config.UndoStackSize;

		IntelliSenseEnabled = config.IntelliSenseEnabled;
		CompletionEnabled = config.IntelliSenseEnabled && config.CompletionEnabled;
		LiveErrorUnderlining = config.IntelliSenseEnabled && config.LiveErrorUnderlining;
		SignatureHelpPopupsEnabled = config.IntelliSenseEnabled && config.SignatureHelpPopupsEnabled;

		AutoCloseParentheses = config.AutoCloseParentheses;
		AutoCloseBraces = config.AutoCloseBraces;
		AutoCloseBrackets = config.AutoCloseBrackets;
		AutoCloseDoubleQuotes = config.AutoCloseDoubleQuotes;
		AutoCloseSingleQuotes = config.AutoCloseSingleQuotes;

		WordWrap = config.WordWrapping;

		Options.HighlightCurrentLine = config.HighlightCurrentLine;

		ShowLineNumbers = config.ShowLineNumbers;

		Options.ShowSpaces = config.ShowVisualSpaces;
		Options.ShowTabs = config.ShowVisualTabs;
	}

	// IEditorControl methods

	void IEditorControl.Undo()
		=> Undo();

	void IEditorControl.Redo()
		=> Redo();

	/// <summary>
	/// Releases the resources used by this editor. Disposal is idempotent; a disposed editor must not be reused.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		DisposeEditorResources();

		_diagnosticsCoordinator?.Dispose();
		_hoverController?.Dispose();
		CompletionController.Dispose();

		_toolTipPresenter.Dispose();
		_completionWindowCoordinator.Dispose();
		_diagnosticToolTipService.ClearDiagnostics();

		UnbindEventMethods();

		TextArea.TextView.BackgroundRenderers.Remove(_bookmarkRenderer);
		TextArea.TextView.BackgroundRenderers.Remove(_errorRenderer);

		_statusCoordinator.Dispose();
		_contentPersistenceCoordinator.Dispose();
	}

	/// <summary>
	/// Disposes resources owned by the concrete editor type before the shared base resources are released.
	/// </summary>
	protected virtual void DisposeEditorResources()
	{ }

	private void EnsureNotDisposed()
		=> ObjectDisposedException.ThrowIf(_isDisposed, this);

	private void UnbindEventMethods()
	{
		TextArea.TextEntering -= TextArea_TextEntering;
		TextArea.TextEntered -= TextEditor_TextEntered;
		TextChanged -= TextEditor_TextChanged;

		RemoveHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown));
		RemoveHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown));

		MouseHover -= TextEditor_MouseHover;
		MouseHoverStopped -= TextEditor_MouseHoverStopped;

		PreviewMouseWheel -= TextEditor_PreviewMouseWheel;
		MouseRightButtonDown -= TextEditor_MouseRightButtonDown;
	}
}

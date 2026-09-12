using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Nickelony.IDEKit.AvalonEdit.Bookmarks;
using Nickelony.IDEKit.AvalonEdit.ChangeMarkers;
using Nickelony.IDEKit.AvalonEdit.Comments;
using Nickelony.IDEKit.AvalonEdit.Diagnostics;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.AvalonEdit.Editors;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Hover;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Navigation;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.Core.Formatting;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Base class for language-specific script editors built on AvalonEdit.
/// </summary>
/// <remarks>
/// Disposal is terminal for view-backed operations and configuration mutations.
/// Editor metadata and retained scalar state remain readable after disposal.
/// </remarks>
public abstract partial class TextEditorBase : TextEditor, IEditorControl
{
	private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

	private static readonly TextEditorFormattingService s_formattingService = new();

	static TextEditorBase()
	{
		// AvalonEdit inserts the line-number margin at the front of TextArea.LeftMargins when
		// ShowLineNumbers changes; keep the change-marker and bookmark margins at the far left
		// after that, in their fixed order.
		ShowLineNumbersProperty.OverrideMetadata(
			typeof(TextEditorBase),
			new FrameworkPropertyMetadata(OnShowLineNumbersChanged));
	}

	private static void OnShowLineNumbersChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
	{
		if (dependencyObject is not TextEditorBase editor)
			return;

		RepositionMargin(editor.TextArea.LeftMargins, editor._changeMarkerMargin, index: 0);
		RepositionMargin(editor.TextArea.LeftMargins, editor._bookmarkMargin, index: 1);
	}

	private static void RepositionMargin(ObservableCollection<UIElement> margins, FrameworkElement? margin, int index)
	{
		if (margin is null)
			return;

		int currentIndex = margins.IndexOf(margin);

		if (currentIndex < 0 || currentIndex == index)
			return;

		margins.RemoveAt(currentIndex);
		margins.Insert(index, margin);
	}

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
			EnsureNotDisposed();

			// A path change is a rename boundary: it advances the session generation so asynchronous
			// work admitted for the previous logical document is rejected as stale.
			if (!string.Equals(Document.FileName, value, StringComparison.Ordinal))
				Interlocked.Increment(ref _sessionGeneration);

			Document.FileName = value;
			_contentPersistenceCoordinator.FilePath = value;
		}
	}

	private readonly EditorProcessingModeScope _processingModeScope = new();
	private int _sessionGeneration;

	/// <inheritdoc/>
	public EditorProcessingMode ProcessingMode => _processingModeScope.CurrentMode;

	/// <summary>
	/// Gets the monotonically increasing session generation that identifies the current operation
	/// ownership of this editor. The generation advances on load, replace, rename, and disposal
	/// boundaries and invalidates asynchronous work admitted for an earlier generation. It is the
	/// operation-ownership counter: it is distinct from the document version (a logical content
	/// snapshot) and from the projection version (the source version used to parse a view), and
	/// those counters must not be used interchangeably.
	/// </summary>
	public int SessionGeneration => Volatile.Read(ref _sessionGeneration);

	/// <inheritdoc/>
	public IDisposable BeginProcessingScope(EditorProcessingMode mode)
	{
		EnsureNotDisposed();
		return _processingModeScope.Begin(mode);
	}

	/// <summary>
	/// Gets or sets whether backup files are created for the current document.
	/// </summary>
	public bool CreateBackupFiles
	{
		get
		{
			EnsureNotDisposed();
			return ProcessingMode == EditorProcessingMode.Suppressed
				? false
				: _contentPersistenceCoordinator.CreateBackupFiles;
		}
		set
		{
			EnsureNotDisposed();
			_contentPersistenceCoordinator.CreateBackupFiles = value;
		}
	}

	/// <inheritdoc/>
	public string Content
	{
		get
		{
			EnsureNotDisposed();
			return Text;
		}
		set => SetContent(value);
	}

	/// <summary>
	/// Gets or sets the target used to apply workspace edits to this editor.
	/// </summary>
	public ITextEditTarget? WorkspaceEditTarget { get; set; }

	private bool _isContentChanged;

	/// <summary>
	/// Gets or sets whether the content of the current document has unsaved changes.
	/// </summary>
	public bool IsContentChanged
	{
		get => _isContentChanged;
		set
		{
			EnsureNotDisposed();
			_isContentChanged = value;
		}
	}

	private DateTime _lastModified;

	/// <summary>
	/// Gets or sets the timestamp of the last content modification.
	/// </summary>
	public DateTime LastModified
	{
		get => _lastModified;
		set
		{
			EnsureNotDisposed();
			_lastModified = value;
		}
	}

	/// <summary>
	/// Gets the line number of the caret position.
	/// </summary>
	public int CurrentRow
	{
		get
		{
			EnsureNotDisposed();
			return TextArea.Caret.Position.Line;
		}
	}

	/// <summary>
	/// Gets the column of the caret position.
	/// </summary>
	public int CurrentColumn
	{
		get
		{
			EnsureNotDisposed();
			return TextArea.Caret.Position.Column;
		}
	}

	/// <summary>
	/// Gets the currently selected content as text, or <c>null</c> when there is no selection.
	/// </summary>
	public string? SelectedContent
	{
		get
		{
			EnsureNotDisposed();
			return SelectedText.Length == 0 ? null : SelectedText;
		}
	}

	/// <summary>
	/// Gets the formatter used when tidying the document.
	/// </summary>
	protected virtual ITextDocumentFormatter DocumentFormatter => TrimTrailingWhitespaceFormatter.Instance;

	private int _minZoom = 25;

	/// <summary>
	/// Gets or sets the minimum allowed zoom percentage.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is less than or equal to zero or greater than <see cref="MaxZoom"/>.</exception>
	public int MinZoom
	{
		get => _minZoom;
		set
		{
			EnsureNotDisposed();
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

			if (value > _maxZoom)
				throw new ArgumentOutOfRangeException(nameof(value), value, "Minimum zoom cannot exceed maximum zoom.");

			_minZoom = value;
			Zoom = _statusCoordinator.Zoom;
		}
	}

	private int _maxZoom = 400;

	/// <summary>
	/// Gets or sets the maximum allowed zoom percentage.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is less than or equal to zero or less than <see cref="MinZoom"/>.</exception>
	public int MaxZoom
	{
		get => _maxZoom;
		set
		{
			EnsureNotDisposed();
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

			if (value < _minZoom)
				throw new ArgumentOutOfRangeException(nameof(value), value, "Maximum zoom cannot be less than minimum zoom.");

			_maxZoom = value;
			Zoom = _statusCoordinator.Zoom;
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
			EnsureNotDisposed();
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
			_zoomStepSize = value;
		}
	}

	private CommentSyntax _commentSyntax;

	/// <summary>
	/// Gets or sets the comment syntax used when commenting out lines.
	/// </summary>
	public CommentSyntax CommentSyntax
	{
		get => _commentSyntax;
		set
		{
			EnsureNotDisposed();
			_commentSyntax = value;
		}
	}

	/// <summary>
	/// Gets or sets the delay before the delayed text-changed notification fires.
	/// </summary>
	public TimeSpan TextChangedDelayedInterval
	{
		get
		{
			EnsureNotDisposed();
			return _contentPersistenceCoordinator.DelayedInterval;
		}
		set
		{
			EnsureNotDisposed();
			_contentPersistenceCoordinator.DelayedInterval = value;
		}
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
			EnsureNotDisposed();
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
			EnsureNotDisposed();
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
			EnsureNotDisposed();
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
			EnsureNotDisposed();
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
			EnsureNotDisposed();
			ArgumentNullException.ThrowIfNull(value);
			_engineVersion = value;
		}
	}

	// Configuration

	private double _defaultFontSize = TextEditorBaseDefaults.FontSize;

	/// <summary>
	/// Basically FontSize but zooming doesn't affect its value.
	/// </summary>
	public double DefaultFontSize
	{
		get => _defaultFontSize;
		set
		{
			EnsureNotDisposed();
			_defaultFontSize = value;
		}
	}

	private bool _intelliSenseEnabled = TextEditorBaseDefaults.IntelliSenseEnabled;

	/// <summary>
	/// Gets or sets whether IntelliSense features are enabled for this editor.
	/// </summary>
	public bool IntelliSenseEnabled
	{
		get => _intelliSenseEnabled;
		set
		{
			EnsureNotDisposed();
			_intelliSenseEnabled = value;
		}
	}

	private bool _completionEnabled = TextEditorBaseDefaults.CompletionEnabled;

	/// <summary>
	/// Gets or sets whether completion suggestions are shown while typing.
	/// </summary>
	public bool CompletionEnabled
	{
		get => _completionEnabled;
		set
		{
			EnsureNotDisposed();
			_completionEnabled = value;
		}
	}

	private bool _liveErrorUnderlining = TextEditorBaseDefaults.LiveErrorUnderlining;

	/// <summary>
	/// Gets or sets whether errors are underlined as they are detected.
	/// </summary>
	public bool LiveErrorUnderlining
	{
		get => _liveErrorUnderlining;
		set
		{
			EnsureNotDisposed();
			_liveErrorUnderlining = value;
		}
	}

	private bool _signatureHelpPopupsEnabled = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

	/// <summary>
	/// Gets or sets whether signature help popups are shown.
	/// </summary>
	public bool SignatureHelpPopupsEnabled
	{
		get => _signatureHelpPopupsEnabled;
		set
		{
			EnsureNotDisposed();
			_signatureHelpPopupsEnabled = value;
		}
	}

	private bool _autoCloseParentheses = TextEditorBaseDefaults.AutoCloseParentheses;

	/// <summary>
	/// Gets or sets whether opening parentheses are auto-closed.
	/// </summary>
	public bool AutoCloseParentheses
	{
		get => _autoCloseParentheses;
		set
		{
			EnsureNotDisposed();
			_autoCloseParentheses = value;
		}
	}

	private bool _autoCloseBraces = TextEditorBaseDefaults.AutoCloseBraces;

	/// <summary>
	/// Gets or sets whether opening braces are auto-closed.
	/// </summary>
	public bool AutoCloseBraces
	{
		get => _autoCloseBraces;
		set
		{
			EnsureNotDisposed();
			_autoCloseBraces = value;
		}
	}

	private bool _autoCloseBrackets = TextEditorBaseDefaults.AutoCloseBrackets;

	/// <summary>
	/// Gets or sets whether opening brackets are auto-closed.
	/// </summary>
	public bool AutoCloseBrackets
	{
		get => _autoCloseBrackets;
		set
		{
			EnsureNotDisposed();
			_autoCloseBrackets = value;
		}
	}

	private bool _autoCloseDoubleQuotes = TextEditorBaseDefaults.AutoCloseDoubleQuotes;

	/// <summary>
	/// Gets or sets whether double quotes are auto-closed.
	/// </summary>
	public bool AutoCloseDoubleQuotes
	{
		get => _autoCloseDoubleQuotes;
		set
		{
			EnsureNotDisposed();
			_autoCloseDoubleQuotes = value;
		}
	}

	private bool _autoCloseSingleQuotes = TextEditorBaseDefaults.AutoCloseSingleQuotes;

	/// <summary>
	/// Gets or sets whether single quotes are auto-closed.
	/// </summary>
	public bool AutoCloseSingleQuotes
	{
		get => _autoCloseSingleQuotes;
		set
		{
			EnsureNotDisposed();
			_autoCloseSingleQuotes = value;
		}
	}

	// Fields

	/// <summary>
	/// The popup used to show special (non-text) tooltips over the editor.
	/// </summary>
	protected Popup _specialToolTip;

	private readonly BookmarkCoordinator _bookmarkCoordinator;
	private readonly IBookmarkStore _bookmarkStore;
	private readonly TextAutoClosingService _autoClosingService;
	private readonly TextLineCommentService _commentService;
	private readonly CompletionWindowCoordinator _completionWindowCoordinator;
	private readonly ContentPersistenceCoordinator _contentPersistenceCoordinator;
	private readonly TextDiagnosticToolTipService _diagnosticToolTipService;
	private readonly TextEditorStatusCoordinator _statusCoordinator;
	private readonly EditorToolTipPresenter _toolTipPresenter;
	private readonly UnsavedChangesTracker _unsavedChangesTracker;

	private BookmarkMargin _bookmarkMargin;
	private ChangeMarkerMargin _changeMarkerMargin;
	private IBackgroundRenderer _diagnosticsRenderer;

	private TextDefinitionTriggerController? _definitionTriggerController;
	private TextHoverController? _hoverController;
	private TextDiagnosticsCoordinator? _diagnosticsCoordinator;
	private bool _isDisposed;
	private bool _isDisposing;

	/// <summary>
	/// Gets the diagnostics currently owned by this editor instance.
	/// </summary>
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics
	{
		get
		{
			EnsureNotDisposed();
			return _diagnosticToolTipService.Diagnostics;
		}
	}

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
		_bookmarkStore = services.BookmarkStore;
		_commentService = services.CommentService;
		_completionWindowCoordinator = services.CompletionWindowCoordinator;
		_contentPersistenceCoordinator = services.ContentPersistenceCoordinator;
		_diagnosticToolTipService = services.DiagnosticToolTipService;
		_statusCoordinator = services.StatusCoordinator;
		_toolTipPresenter = services.ToolTipPresenter;
		_unsavedChangesTracker = services.UnsavedChangesTracker;
		_specialToolTip = _toolTipPresenter.Popup;

		CompletionController = new TextCompletionController(
			this,
			_completionWindowCoordinator,
			configureWindow: static window => TextCompletionWindowStyle.Apply(window),
			completionItemFactory: static item => new CompletionData(item),
			resolveDescriptionAsync: static item =>
				item is CompletionData completionData && completionData.CanResolve
					? completionData.GetDescriptionAsync()
					: null,
			getDisplayInfo: static item => item is CompletionData completionData
				? (completionData.DisplayText, completionData.DisplayDetail)
				: (item.Text, null),
			toolTipBackground: TextEditorColorPalette.ToolTipBackground,
			toolTipBorder: TextEditorColorPalette.ToolTipBorder);

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

	private void UnsubscribePersistenceCoordinatorEvents()
	{
		_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted -= ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted;
		_contentPersistenceCoordinator.TextChangedDelayed -= ContentPersistenceCoordinator_TextChangedDelayed;
	}

	[MemberNotNull(nameof(_bookmarkMargin), nameof(_changeMarkerMargin), nameof(_diagnosticsRenderer))]
	private void InitializeRenderers()
	{
		_changeMarkerMargin = new ChangeMarkerMargin(_unsavedChangesTracker);
		_bookmarkMargin = new BookmarkMargin(_bookmarkCoordinator);
		_diagnosticsRenderer = new DiagnosticsRenderer(
			documentProvider: () => Document,
			segmentsProvider: CreateDiagnosticSegments);

		TextArea.LeftMargins.Insert(0, _changeMarkerMargin);
		TextArea.LeftMargins.Insert(1, _bookmarkMargin);
		TextArea.TextView.BackgroundRenderers.Add(_diagnosticsRenderer);
	}

	internal void InvalidateBookmarkMargin()
		=> _bookmarkMargin?.InvalidateVisual();

	internal void InvalidateChangeMarkerMargin()
		=> _changeMarkerMargin?.InvalidateVisual();

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

	/// <summary>
	/// Gets whether an undo operation is available.
	/// </summary>
	public new bool CanUndo
	{
		get
		{
			EnsureNotDisposed();
			return Document.UndoStack.CanUndo;
		}
	}

	/// <summary>
	/// Gets whether a redo operation is available.
	/// </summary>
	public new bool CanRedo
	{
		get
		{
			EnsureNotDisposed();
			return Document.UndoStack.CanRedo;
		}
	}

	/// <summary>
	/// Undoes the most recent document operation.
	/// </summary>
	/// <returns><see langword="true"/> when an operation was undone; otherwise, <see langword="false"/>.</returns>
	public new bool Undo()
	{
		EnsureNotDisposed();

		if (!CanUndo)
			return false;

		Document.UndoStack.Undo();
		return true;
	}

	/// <summary>
	/// Redoes the most recently undone document operation.
	/// </summary>
	/// <returns><see langword="true"/> when an operation was redone; otherwise, <see langword="false"/>.</returns>
	public new bool Redo()
	{
		EnsureNotDisposed();

		if (!CanRedo)
			return false;

		Document.UndoStack.Redo();
		return true;
	}

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
		_isDisposing = true;

		// Advance the session generation so in-flight asynchronous work admitted before disposal is
		// rejected as stale instead of publishing to the disposed editor.
		Interlocked.Increment(ref _sessionGeneration);

		try
		{
			DisposeEditorResources();
		}
		finally
		{
			_isDisposing = false;
		}

		_diagnosticsCoordinator?.Dispose();
		_hoverController?.Dispose();
		_definitionTriggerController?.Dispose();
		CompletionController.Dispose();

		_toolTipPresenter.Dispose();
		_completionWindowCoordinator.Dispose();
		_diagnosticToolTipService.ClearDiagnostics();

		UnbindEventMethods();

		TextArea.LeftMargins.Remove(_changeMarkerMargin);
		TextArea.LeftMargins.Remove(_bookmarkMargin);
		TextArea.TextView.BackgroundRenderers.Remove(_diagnosticsRenderer);

		_statusCoordinator.Dispose();
		UnsubscribePersistenceCoordinatorEvents();
		_contentPersistenceCoordinator.Dispose();
	}

	/// <summary>
	/// Disposes resources owned by the concrete editor type before the shared base resources are released.
	/// </summary>
	protected virtual void DisposeEditorResources()
	{ }

	/// <summary>
	/// Throws when this editor is no longer available for normal operations.
	/// </summary>
	protected void EnsureNotDisposed()
		=> ObjectDisposedException.ThrowIf(_isDisposed && !_isDisposing, this);

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

using Nickelony.IDEKit.AvalonEdit.Bookmarks;
using Nickelony.IDEKit.AvalonEdit.ChangeMarkers;
using Nickelony.IDEKit.AvalonEdit.Comments;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.AvalonEdit.Editors;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Construction-time composition of the per-editor services created in the
/// <see cref="TextEditorBase"/> constructor.
///	<para>
/// This is not a general-purpose service locator or DI service collection.
/// Every member is 1:1 with the editor instance it was created for and depends
/// on the editor being constructed (document, text area, popup host, etc.),
/// so real DI composition is not applicable here.
///
/// The name intentionally uses "composition" rather than "collection" to reflect
/// that it is a fixed, narrow set of services rather than an extensible registry.
///
/// Ownership and lifetime:
/// the composition object (and each service) is owned by the editor
/// instance that created it and lives exactly as long as that editor; the constructor fields
/// it into <see cref="TextEditorBase"/> readonly fields, after which the composition object
/// itself is discarded.
/// </para>
/// </summary>
internal sealed class TextEditorServiceComposition
{
	private TextEditorServiceComposition(
		TextAutoClosingService autoClosingService,
		BookmarkCoordinator bookmarkCoordinator,
		IBookmarkStore bookmarkStore,
		TextLineCommentService commentService,
		CompletionWindowCoordinator completionWindowCoordinator,
		ContentPersistenceCoordinator contentPersistenceCoordinator,
		TextDiagnosticToolTipService diagnosticToolTipService,
		TextEditorStatusCoordinator statusCoordinator,
		EditorToolTipPresenter toolTipPresenter,
		UnsavedChangesTracker unsavedChangesTracker)
	{
		AutoClosingService = autoClosingService;
		BookmarkCoordinator = bookmarkCoordinator;
		BookmarkStore = bookmarkStore;
		CommentService = commentService;
		CompletionWindowCoordinator = completionWindowCoordinator;
		ContentPersistenceCoordinator = contentPersistenceCoordinator;
		DiagnosticToolTipService = diagnosticToolTipService;
		StatusCoordinator = statusCoordinator;
		ToolTipPresenter = toolTipPresenter;
		UnsavedChangesTracker = unsavedChangesTracker;
	}

	public TextAutoClosingService AutoClosingService { get; }
	public BookmarkCoordinator BookmarkCoordinator { get; }
	public IBookmarkStore BookmarkStore { get; }
	public TextLineCommentService CommentService { get; }
	public CompletionWindowCoordinator CompletionWindowCoordinator { get; }
	public ContentPersistenceCoordinator ContentPersistenceCoordinator { get; }
	public TextDiagnosticToolTipService DiagnosticToolTipService { get; }
	public TextEditorStatusCoordinator StatusCoordinator { get; }
	public EditorToolTipPresenter ToolTipPresenter { get; }
	public UnsavedChangesTracker UnsavedChangesTracker { get; }

	/// <summary>
	/// Creates the service composition for the given editor.
	/// </summary>
	/// <param name="editor">The editor the services are created for.</param>
	/// <returns>The service composition bound to the editor.</returns>
	public static TextEditorServiceComposition Create(TextEditorBase editor)
	{
		return new TextEditorServiceComposition(
			autoClosingService: new TextAutoClosingService(),

			bookmarkCoordinator: new BookmarkCoordinator(
				documentProvider: () => editor.Document,
				onBookmarksChanged: () =>
				{
					editor.InvalidateBookmarkMargin();
					editor.SaveBookmarks();
				}),

			bookmarkStore: new BookmarkSidecarStore(),

			commentService: new TextLineCommentService(),

			completionWindowCoordinator: new CompletionWindowCoordinator(
				host: new CompletionWindowHost(editor.TextArea),
				defaultBorderBrush: TextEditorColorPalette.ToolTipBorder,
				defaultBackground: TextEditorColorPalette.ToolTipBackground,
				defaultForeground: TextEditorColorPalette.ToolTipForeground),

			contentPersistenceCoordinator: new ContentPersistenceCoordinator(
				contentProvider: () => editor.Content,
				suppressedProvider: () => editor.ProcessingMode == EditorProcessingMode.Suppressed,
				useDelayedScheduling: true),

			diagnosticToolTipService: new TextDiagnosticToolTipService(
				onDiagnosticsChanged: () => editor.InvalidateDiagnosticLayer()),

			statusCoordinator: new TextEditorStatusCoordinator(
				textArea: editor.TextArea,
				raiseStatusChanged: editor.RaiseStatusChanged,
				raiseZoomChanged: editor.RaiseZoomChanged),

			toolTipPresenter: new EditorToolTipPresenter(editor),

			unsavedChangesTracker: new UnsavedChangesTracker(
				documentProvider: () => editor.Document,
				onChanged: () => editor.InvalidateChangeMarkerMargin()));
	}
}

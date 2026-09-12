using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Nickelony.IDEKit.AvalonEdit.Comments;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.AvalonEdit.Navigation;
using Nickelony.IDEKit.Core.Formatting;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region Auto bracket closing

	private void HandleAutoClosing(TextCompositionEventArgs e)
		=> _autoClosingService.HandleTextEntering(this, e, CreateAutoClosingOptions(), OnAutoClosingElementSkipped);

	private TextAutoClosingOptions CreateAutoClosingOptions() => new(
		AutoCloseParentheses,
		AutoCloseBraces,
		AutoCloseBrackets,
		AutoCloseDoubleQuotes,
		AutoCloseSingleQuotes,
		ParenthesesClosingString,
		BracesClosingString,
		BracketsClosingString,
		QuotesClosingString,
		"'"); // TODO: Add field that handles this one as well

	/// <summary>
	/// Called when an auto-closed element is skipped by the user.
	/// </summary>
	/// <param name="element">The auto-closed element that was skipped.</param>
	protected virtual void OnAutoClosingElementSkipped(string element)
	{ }

	#endregion Auto bracket closing

	#region Programmatic edits

	/// <summary>
	/// Inserts <paramref name="newText"/> at <paramref name="insertOffset"/> as one undo step
	/// and places the caret at <paramref name="caretOffset"/> (defaults to just after the inserted text).
	/// </summary>
	/// <param name="insertOffset">The zero-based offset at which to insert the text.</param>
	/// <param name="newText">The text to insert.</param>
	/// <param name="caretOffset">The caret offset after the edit; defaults to just after the inserted text.</param>
	public void InsertText(int insertOffset, string newText, int? caretOffset = null)
		=> TextEditorEditHelper.InsertText(this, insertOffset, newText, caretOffset, WorkspaceEditTarget, () => RunContentChangedWorker());

	/// <summary>
	/// Replaces the range starting at <paramref name="startOffset"/> with <paramref name="newText"/>
	/// as one undo step and places the caret at <paramref name="caretOffset"/>
	/// (defaults to just after the inserted text).
	/// </summary>
	/// <param name="startOffset">The zero-based start offset of the replaced range.</param>
	/// <param name="length">The length of the replaced range.</param>
	/// <param name="newText">The replacement text.</param>
	/// <param name="caretOffset">The caret offset after the edit; defaults to just after the inserted text.</param>
	public void ReplaceText(int startOffset, int length, string newText, int? caretOffset = null)
		=> TextEditorEditHelper.ReplaceText(this, startOffset, length, newText, caretOffset, WorkspaceEditTarget, () => RunContentChangedWorker());

	/// <summary>
	/// Replaces the first line whose selector returns replacement text.
	/// </summary>
	/// <param name="replacementSelector">Returns the replacement text for a matching line, or <see langword="null"/> to skip the line.</param>
	/// <param name="scrollToLine">Whether to scroll the editor to the updated line.</param>
	/// <returns><see langword="true"/> when a matching line was replaced; otherwise, <see langword="false"/>.</returns>
	public bool TryReplaceFirstMatchingLine(Func<string, string?> replacementSelector, bool scrollToLine = true)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(this, replacementSelector, scrollToLine, WorkspaceEditTarget, () => RunContentChangedWorker());

	/// <summary>
	/// Replaces the first occurrence of <paramref name="oldName"/> with <paramref name="newName"/>
	/// on a line that matches <paramref name="lineRegex"/>. The name is extracted from each matching
	/// line via <paramref name="nameExtractor"/> before comparison.
	/// </summary>
	/// <param name="lineRegex">The regular expression used to identify candidate lines.</param>
	/// <param name="nameExtractor">Extracts the normalized name from a candidate line.</param>
	/// <param name="oldName">The name to search for.</param>
	/// <param name="newName">The replacement name.</param>
	/// <param name="scrollToLine">Whether to scroll the editor to the updated line.</param>
	/// <returns><see langword="true"/> when a matching line was replaced; otherwise, <see langword="false"/>.</returns>
	public bool TryReplaceFirstMatchingLine(Regex lineRegex, Func<string, Regex, string> nameExtractor, string oldName, string newName, bool scrollToLine = true)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(this, lineRegex, nameExtractor, oldName, newName, scrollToLine, WorkspaceEditTarget, () => RunContentChangedWorker());

	#endregion Programmatic edits

	#region Multiline commenting

	/// <summary>
	/// Comments out the currently selected lines.
	/// </summary>
	public void CommentOutLines()
	{
		EnsureNotDisposed();
		ApplyLineCommentTransformation(TextLineCommentAction.Comment);
	}

	/// <summary>
	/// Uncomments the currently selected lines.
	/// </summary>
	public void UncommentLines()
	{
		EnsureNotDisposed();
		ApplyLineCommentTransformation(TextLineCommentAction.Uncomment);
	}

	/// <summary>
	/// Toggles commenting on the currently selected lines.
	/// </summary>
	public void ToggleCommentLines()
	{
		EnsureNotDisposed();
		ApplyLineCommentTransformation(TextLineCommentAction.Toggle);
	}

	private void ApplyLineCommentTransformation(TextLineCommentAction action)
		=> _commentService.ApplyEdit(this, CommentSyntax, action);

	#endregion Multiline commenting

	#region Bookmarks

	/// <summary>
	/// Toggles a bookmark at the caret position.
	/// </summary>
	public void ToggleBookmark()
	{
		EnsureNotDisposed();
		_bookmarkCoordinator.ToggleBookmark(CaretOffset);
	}

	/// <summary>
	/// Moves the caret to the next bookmark after the current position.
	/// </summary>
	public void GoToNextBookmark()
	{
		EnsureNotDisposed();

		DocumentLine? nextBookmark = _bookmarkCoordinator.GetNextBookmarkLine(CaretOffset);

		if (nextBookmark is null)
			return;

		CaretOffset = nextBookmark.EndOffset;
		ScrollToLine(nextBookmark.LineNumber);
	}

	/// <summary>
	/// Moves the caret to the previous bookmark before the current position.
	/// </summary>
	public void GoToPrevBookmark()
	{
		EnsureNotDisposed();

		DocumentLine? previousBookmark = _bookmarkCoordinator.GetPreviousBookmarkLine(CaretOffset);

		if (previousBookmark is null)
			return;

		CaretOffset = previousBookmark.EndOffset;
		ScrollToLine(previousBookmark.LineNumber);
	}

	/// <summary>
	/// Clears all bookmarks after confirmation.
	/// </summary>
	/// <param name="confirmClearBookmarks">The confirmation callback to invoke before clearing.</param>
	public void ClearAllBookmarks(Func<bool> confirmClearBookmarks)
	{
		EnsureNotDisposed();

		if (!confirmClearBookmarks())
			return;

		_bookmarkCoordinator.Clear();
	}

	#endregion Bookmarks

	#region Zoom

	/// <summary>
	/// Gets or sets the current zoom percentage. Values outside the configured range are clamped to the nearest bound.
	/// </summary>
	public int Zoom
	{
		get
		{
			EnsureNotDisposed();
			return _statusCoordinator.Zoom;
		}
		set
		{
			EnsureNotDisposed();

			int constrainedZoom = Math.Clamp(value, _minZoom, _maxZoom);
			bool zoomChanged = _statusCoordinator.Zoom != constrainedZoom;

			FontSize = DefaultFontSize * constrainedZoom / 100;
			_statusCoordinator.Zoom = constrainedZoom;

			if (zoomChanged)
				OnZoomChanged(EventArgs.Empty);
		}
	}

	#endregion Zoom

	#region View operations

	/// <summary>
	/// Selects the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to select.</param>
	public void SelectLine(int lineNumber)
	{
		EnsureNotDisposed();
		SelectLine(Document.GetLineByNumber(lineNumber));
	}

	/// <summary>
	/// Selects the given document line.
	/// </summary>
	/// <param name="line">The line to select.</param>
	public void SelectLine(DocumentLine line)
	{
		EnsureNotDisposed();
		TextEditorLineOperations.SelectLine(this, line);
	}

	/// <summary>
	/// Replaces the content of the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to replace.</param>
	/// <param name="replacement">The replacement text.</param>
	/// <param name="deselectAfterwards">Whether to deselect the replaced line afterwards.</param>
	public void ReplaceLine(int lineNumber, string replacement, bool deselectAfterwards = false)
	{
		EnsureNotDisposed();
		ReplaceLine(Document.GetLineByNumber(lineNumber), replacement, deselectAfterwards);
	}

	/// <summary>
	/// Replaces the content of the given document line.
	/// </summary>
	/// <param name="line">The line to replace.</param>
	/// <param name="replacement">The replacement text.</param>
	/// <param name="deselectAfterwards">Whether to deselect the replaced line afterwards.</param>
	public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
	{
		EnsureNotDisposed();
		TextEditorLineOperations.ReplaceLine(this, line, replacement, deselectAfterwards);
	}

	/// <summary>
	/// Replaces the entire document content with the given text.
	/// </summary>
	/// <param name="newContent">The new document content.</param>
	public void ReplaceContent(string newContent)
	{
		EnsureNotDisposed();
		TextEditorLineOperations.ReplaceContent(this, newContent);
	}

	/// <summary>
	/// Resets the current selection to the default state.
	/// </summary>
	public void ResetSelection()
	{
		EnsureNotDisposed();
		TextEditorLineOperations.ResetSelection(this);
	}

	/// <summary>
	/// Resets the selection and places the caret at the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to reset the selection at.</param>
	public void ResetSelectionAt(int lineNumber)
	{
		EnsureNotDisposed();
		ResetSelectionAt(Document.GetLineByNumber(lineNumber));
	}

	/// <summary>
	/// Resets the selection and places the caret at the given line.
	/// </summary>
	/// <param name="line">The line to reset the selection at.</param>
	public void ResetSelectionAt(DocumentLine line)
	{
		EnsureNotDisposed();
		TextEditorLineOperations.ResetSelectionAt(this, line);
	}

	/// <summary>
	/// Gets the document offset corresponding to the given point in the view.
	/// </summary>
	/// <param name="point">The point in view coordinates.</param>
	/// <returns>The document offset, or <c>-1</c> if the point does not map to a position.</returns>
	public int GetOffsetFromPoint(Point point)
	{
		EnsureNotDisposed();
		return EditorNavigationHelper.GetOffsetFromPoint(this, point);
	}

	/// <summary>
	/// Gets the word surrounding the given document offset.
	/// </summary>
	/// <param name="offset">The document offset to inspect.</param>
	/// <returns>The word text, or <see langword="null"/> if no word is found.</returns>
	public string? GetWordFromOffset(int offset)
	{
		EnsureNotDisposed();
		return EditorNavigationHelper.GetWordFromOffset(this, offset);
	}

	#endregion View operations

	#region ToolTips

	/// <summary>
	/// Shows a plain-text tooltip with the default colors.
	/// </summary>
	/// <param name="content">The text to display.</param>
	public void ShowToolTip(string content)
	{
		EnsureNotDisposed();

		ShowToolTip(content,
			TextEditorColorPalette.ToolTipBorder,
			TextEditorColorPalette.ToolTipBackground,
			TextEditorColorPalette.ToolTipForeground);
	}

	/// <summary>
	/// Shows a markdown-formatted tooltip with the default colors.
	/// </summary>
	/// <param name="content">The markdown content to display.</param>
	public void ShowMarkdownToolTip(string content)
	{
		EnsureNotDisposed();

		ShowMarkdownToolTip(content,
			TextEditorColorPalette.ToolTipBorder,
			TextEditorColorPalette.ToolTipBackground,
			TextEditorColorPalette.ToolTipForeground);
	}

	/// <summary>
	/// Shows a plain-text tooltip with the given colors.
	/// </summary>
	/// <param name="content">The text to display.</param>
	/// <param name="border">The border brush to use.</param>
	/// <param name="background">The background brush to use.</param>
	/// <param name="foreground">The foreground brush to use.</param>
	public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
	{
		EnsureNotDisposed();
		ShowToolTip(TextEditorToolTipHelper.CreatePlainToolTipContent(content, foreground), border, background);
	}

	/// <summary>
	/// Shows a markdown-formatted tooltip with the given colors.
	/// </summary>
	/// <param name="content">The markdown content to display.</param>
	/// <param name="border">The border brush to use.</param>
	/// <param name="background">The background brush to use.</param>
	/// <param name="foreground">The foreground brush to use.</param>
	public void ShowMarkdownToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
	{
		EnsureNotDisposed();
		ShowToolTip(TextEditorToolTipHelper.CreateMarkdownToolTipContent(content, foreground, background), border, background);
	}

	/// <summary>
	/// Shows a tooltip with arbitrary content and the given colors.
	/// </summary>
	/// <param name="content">The content to display.</param>
	/// <param name="border">The border brush to use.</param>
	/// <param name="background">The background brush to use.</param>
	public void ShowToolTip(object content, SolidColorBrush border, SolidColorBrush background)
	{
		EnsureNotDisposed();
		_toolTipPresenter.Show(content, border, background);
	}

	#endregion ToolTips

	#region Formatting

	/// <summary>
	/// Converts spaces to tabs throughout the document content.
	/// </summary>
	public void ConvertSpacesToTabs()
	{
		EnsureNotDisposed();
		Content = WhiteSpaceConverter.ConvertSpacesToTabs(Content, 4);
	}

	/// <summary>
	/// Converts tabs to spaces throughout the document content.
	/// </summary>
	public void ConvertTabsToSpaces()
	{
		EnsureNotDisposed();
		Content = WhiteSpaceConverter.ConvertTabsToSpaces(Content, 4);
	}

	/// <summary>
	/// Tidies the document using the configured formatter.
	/// </summary>
	/// <param name="trimOnly">Whether only trailing whitespace should be trimmed.</param>
	public virtual void TidyCode(bool trimOnly = false)
	{
		EnsureNotDisposed();
		s_formattingService.FormatDocument(this, DocumentFormatter, trimOnly);
	}

	#endregion Formatting
}

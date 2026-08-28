using ICSharpCode.AvalonEdit.Document;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Editing;
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

	#region Multiline commenting

	/// <summary>
	/// Comments out the currently selected lines.
	/// </summary>
	public void CommentOutLines()
		=> ApplyLineCommentTransformation(TextLineCommentAction.Comment);

	/// <summary>
	/// Uncomments the currently selected lines.
	/// </summary>
	public void UncommentLines()
		=> ApplyLineCommentTransformation(TextLineCommentAction.Uncomment);

	/// <summary>
	/// Toggles commenting on the currently selected lines.
	/// </summary>
	public void ToggleCommentLines()
		=> ApplyLineCommentTransformation(TextLineCommentAction.Toggle);

	private void ApplyLineCommentTransformation(TextLineCommentAction action)
		=> _commentService.ApplyEdit(this, CommentPrefix, action);

	#endregion Multiline commenting

	#region Bookmarks

	/// <summary>
	/// Toggles a bookmark at the caret position.
	/// </summary>
	public void ToggleBookmark()
		=> _bookmarkCoordinator.ToggleBookmark(CaretOffset);

	/// <summary>
	/// Moves the caret to the next bookmark after the current position.
	/// </summary>
	public void GoToNextBookmark()
	{
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
		if (!confirmClearBookmarks())
			return;

		_bookmarkCoordinator.Clear();
	}

	#endregion Bookmarks

	#region Zoom

	/// <summary>
	/// Gets or sets the current zoom percentage.
	/// </summary>
	public int Zoom
	{
		get => _statusCoordinator.Zoom;
		set
		{
			FontSize = DefaultFontSize * value / 100;
			_statusCoordinator.Zoom = value;
		}
	}

	#endregion Zoom

	#region View operations

	/// <summary>
	/// Selects the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to select.</param>
	public void SelectLine(int lineNumber)
		=> SelectLine(Document.GetLineByNumber(lineNumber));

	/// <summary>
	/// Selects the given document line.
	/// </summary>
	/// <param name="line">The line to select.</param>
	public void SelectLine(DocumentLine line)
		=> _viewService.SelectLine(line);

	/// <summary>
	/// Replaces the content of the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to replace.</param>
	/// <param name="replacement">The replacement text.</param>
	/// <param name="deselectAfterwards">Whether to deselect the replaced line afterwards.</param>
	public void ReplaceLine(int lineNumber, string replacement, bool deselectAfterwards = false)
		=> ReplaceLine(Document.GetLineByNumber(lineNumber), replacement, deselectAfterwards);

	/// <summary>
	/// Replaces the content of the given document line.
	/// </summary>
	/// <param name="line">The line to replace.</param>
	/// <param name="replacement">The replacement text.</param>
	/// <param name="deselectAfterwards">Whether to deselect the replaced line afterwards.</param>
	public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
		=> _viewService.ReplaceLine(line, replacement, deselectAfterwards);

	/// <summary>
	/// Replaces the entire document content with the given text.
	/// </summary>
	/// <param name="newContent">The new document content.</param>
	public void ReplaceContent(string newContent)
		=> _viewService.ReplaceContent(newContent);

	/// <summary>
	/// Resets the current selection to the default state.
	/// </summary>
	public void ResetSelection()
		=> _viewService.ResetSelection();

	/// <summary>
	/// Resets the selection and places the caret at the line with the given line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to reset the selection at.</param>
	public void ResetSelectionAt(int lineNumber)
		=> ResetSelectionAt(Document.GetLineByNumber(lineNumber));

	/// <summary>
	/// Resets the selection and places the caret at the given line.
	/// </summary>
	/// <param name="line">The line to reset the selection at.</param>
	public void ResetSelectionAt(DocumentLine line)
		=> _viewService.ResetSelectionAt(line);

	/// <summary>
	/// Gets the document offset corresponding to the given point in the view.
	/// </summary>
	/// <param name="point">The point in view coordinates.</param>
	/// <returns>The document offset, or <c>-1</c> if the point does not map to a position.</returns>
	public int GetOffsetFromPoint(Point point)
		=> _viewService.GetOffsetFromPoint(point);

	/// <summary>
	/// Gets the word surrounding the given document offset.
	/// </summary>
	/// <param name="offset">The document offset to inspect.</param>
	/// <returns>The word text, or <see langword="null"/> if no word is found.</returns>
	public string? GetWordFromOffset(int offset)
		=> _viewService.GetWordFromOffset(offset);

	#endregion View operations

	#region ToolTips

	/// <summary>
	/// Shows a plain-text tooltip with the default colors.
	/// </summary>
	/// <param name="content">The text to display.</param>
	public void ShowToolTip(string content)
	{
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
		=> ShowToolTip(TextEditorToolTipHelper.CreatePlainToolTipContent(content, foreground), border, background);

	/// <summary>
	/// Shows a markdown-formatted tooltip with the given colors.
	/// </summary>
	/// <param name="content">The markdown content to display.</param>
	/// <param name="border">The border brush to use.</param>
	/// <param name="background">The background brush to use.</param>
	/// <param name="foreground">The foreground brush to use.</param>
	public void ShowMarkdownToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
		=> ShowToolTip(TextEditorToolTipHelper.CreateMarkdownToolTipContent(content, foreground, background), border, background);

	/// <summary>
	/// Shows a tooltip with arbitrary content and the given colors.
	/// </summary>
	/// <param name="content">The content to display.</param>
	/// <param name="border">The border brush to use.</param>
	/// <param name="background">The background brush to use.</param>
	public void ShowToolTip(object content, SolidColorBrush border, SolidColorBrush background)
		=> _toolTipPresenter.Show(content, border, background);

	#endregion ToolTips

	#region Formatting

	/// <summary>
	/// Converts spaces to tabs throughout the document content.
	/// </summary>
	public void ConvertSpacesToTabs()
		=> Content = WhiteSpaceConverter.ConvertSpacesToTabs(Content, 4);

	/// <summary>
	/// Converts tabs to spaces throughout the document content.
	/// </summary>
	public void ConvertTabsToSpaces()
		=> Content = WhiteSpaceConverter.ConvertTabsToSpaces(Content, 4);

	/// <summary>
	/// Tidies the document using the configured formatter.
	/// </summary>
	/// <param name="trimOnly">Whether only trailing whitespace should be trimmed.</param>
	public virtual void TidyCode(bool trimOnly = false)
		=> s_formattingService.FormatDocument(this, DocumentFormatter, trimOnly);

	#endregion Formatting
}

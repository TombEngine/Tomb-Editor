#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombIDE.ScriptingStudio.Controls;
using Nickelony.IDEKit.Core.FindReplace;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using Nickelony.IDEKit.Core.Editing;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// ViewModel for the Find and Replace dialog.
/// Orchestrates search/replace operations using <see cref="FindReplaceService"/>
/// and <see cref="IEditorDocumentController"/>.
/// </summary>
public partial class FindAndReplaceViewModel : ObservableObject
{
	private readonly IEditorDocumentController _documentController;
	private readonly IMessenger _messenger;
	private readonly FindReplaceService _service;

	[ObservableProperty]
	private string _findText = string.Empty;

	[ObservableProperty]
	private string _replaceText = string.Empty;

	[ObservableProperty]
	private bool _caseSensitive;

	[ObservableProperty]
	private bool _matchWholeWord;

	[ObservableProperty]
	private bool _useRegex;

	[ObservableProperty]
	private bool _searchUp;

	[ObservableProperty]
	private bool _searchDown = true;

	[ObservableProperty]
	private bool _searchCurrentDocument = true;

	[ObservableProperty]
	private bool _searchAllTabs;

	[ObservableProperty]
	private string _statusMessage = string.Empty;

	[ObservableProperty]
	private FindReplaceStatusType _statusType = FindReplaceStatusType.None;

	public FindAndReplaceViewModel(
		IEditorDocumentController documentController,
		IMessenger messenger,
		FindReplaceService service)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(service);

		_documentController = documentController;
		_messenger = messenger;
		_service = service;
	}

	/// <summary>
	/// Called when the dialog is shown, to populate the initial find text from
	/// the current editor selection.
	/// </summary>
	public void Initialize(string initialFindText)
	{
		if (!string.IsNullOrEmpty(initialFindText))
		{
			FindText = initialFindText;
		}

		StatusMessage = string.Empty;
		StatusType = FindReplaceStatusType.None;

		OnPropertyChanged(nameof(FindText));
	}

	/// <summary>
	/// Returns the current <see cref="TextEditorBase"/> or null if the current
	/// editor is not a text editor.
	/// </summary>
	private TextEditorBase? GetCurrentTextEditor()
		=> _documentController.CurrentEditor as TextEditorBase;

	/// <summary>
	/// Returns all open text editors.
	/// </summary>
	private IEnumerable<TextEditorBase> GetOpenTextEditors()
		=> _documentController.GetOpenEditors().OfType<TextEditorBase>();

	/// <summary>
	/// Builds the current regex pattern and options from the ViewModel state.
	/// </summary>
	private (string pattern, RegexOptions options) GetCurrentPatternAndOptions()
	{
		string pattern = FindReplaceText.BuildPattern(FindText, UseRegex, MatchWholeWord);
		RegexOptions options = FindReplaceText.BuildRegexOptions(CaseSensitive);
		return (pattern, options);
	}

	private SearchDirection GetCurrentDirection()
		=> SearchUp ? SearchDirection.Previous : SearchDirection.Next;

	/// <summary>
	/// The direction of a search or replace operation. The Core find helpers are direction-specific:
	/// a previous search ends at the selection start and a next search begins at the selection end.
	/// </summary>
	private enum SearchDirection
	{
		Previous,
		Next
	}

	// ---------------------------------------------------------------------------
	// Find
	// ---------------------------------------------------------------------------

	[RelayCommand]
	private void FindPrevious()
	{
		if (string.IsNullOrEmpty(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();
		FindMatch(SearchDirection.Previous, pattern, options);
	}

	[RelayCommand]
	private void FindNext()
	{
		if (string.IsNullOrEmpty(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();
		FindMatch(SearchDirection.Next, pattern, options);
	}

	[RelayCommand]
	private void Find()
	{
		if (string.IsNullOrEmpty(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();
		SearchDirection order = GetCurrentDirection();
		FindMatch(order, pattern, options);
	}

	private void FindMatch(SearchDirection order, string pattern, RegexOptions options)
	{
		TextEditorBase? editor = GetCurrentTextEditor();

		if (editor is null)
		{
			if (SearchAllTabs)
				FindMatchInAnotherTab(order, pattern, options);
			else
				ShowStatus("Unsupported document.", FindReplaceStatusType.Error);

			return;
		}

		var query = new TextSearchQuery(pattern, options);

		// The match helpers return absolute document offsets and exclude the selection themselves:
		// a previous search ends at the selection start and a next search begins at the selection end.
		Match? match = order == SearchDirection.Previous
			? FindReplaceText.FindPreviousMatch(editor.Text, editor.SelectionStart, query)
			: FindReplaceText.FindNextMatch(editor.Text, editor.SelectionStart + editor.SelectionLength, query);

		if (match is null)
		{
			if (SearchAllTabs)
				FindMatchInAnotherTab(order, pattern, options);
			else if (FindReplaceText.CountMatches(editor.Text, pattern, options) == 0)
				ShowStatus("No matches found in the current document.", FindReplaceStatusType.Error);
			else
				EndSuccessfulSearch(order, editor);

			return;
		}

		editor.Select(match.Index, match.Length);
		editor.ScrollTo(editor.TextArea.Caret.Position.Line, editor.TextArea.Caret.Position.Column);
		ShowMatchCountStatus(editor.Text, pattern, options);
	}

	private void FindMatchInAnotherTab(SearchDirection order, string pattern, RegexOptions options)
	{
		if (GetAllTabsMatchCount(pattern, options) == 0)
		{
			ShowStatus("No matches found.", FindReplaceStatusType.Error);
			return;
		}

		switch (order)
		{
			case SearchDirection.Previous:
				FindPrevInPrevTab();
				break;

			case SearchDirection.Next:
				FindNextInNextTab();
				break;
		}
	}

	private void FindPrevInPrevTab()
	{
		if (!_documentController.TryActivatePreviousEditor())
		{
			TextEditorBase? editor = GetCurrentTextEditor();
			MoveCaretToDocumentStart(editor);
			ShowStatus(
				"Reached the start of the first tab document with no more matches found.",
				FindReplaceStatusType.Warning);
		}
		else
		{
			TextEditorBase? nextTarget = GetCurrentTextEditor();

			if (nextTarget is null)
			{
				FindPrevInPrevTab();
				return;
			}

			MoveCaretToDocumentEnd(nextTarget);

			var (pattern, options) = GetCurrentPatternAndOptions();
			FindMatch(SearchDirection.Previous, pattern, options);
		}
	}

	private void FindNextInNextTab()
	{
		if (!_documentController.TryActivateNextEditor())
		{
			TextEditorBase? editor = GetCurrentTextEditor();
			MoveCaretToDocumentEnd(editor);
			ShowStatus(
				"Reached the end of the last tab document with no more matches found.",
				FindReplaceStatusType.Warning);
		}
		else
		{
			TextEditorBase? nextTarget = GetCurrentTextEditor();

			if (nextTarget is null)
			{
				FindNextInNextTab();
				return;
			}

			MoveCaretToDocumentStart(nextTarget);

			var (pattern, options) = GetCurrentPatternAndOptions();
			FindMatch(SearchDirection.Next, pattern, options);
		}
	}

	private void EndSuccessfulSearch(SearchDirection order, TextEditorBase editor)
	{
		switch (order)
		{
			case SearchDirection.Previous:
				MoveCaretToDocumentStart(editor);
				ShowStatus(
					"Reached the start of the document with no more matches found.",
					FindReplaceStatusType.Warning);
				break;

			case SearchDirection.Next:
				MoveCaretToDocumentEnd(editor);
				ShowStatus(
					"Reached the end of the document with no more matches found.",
					FindReplaceStatusType.Warning);
				break;
		}
	}

	// ---------------------------------------------------------------------------
	// Replace
	// ---------------------------------------------------------------------------

	[RelayCommand]
	private void ReplacePrevious()
		=> ReplaceWithDirection(SearchDirection.Previous);

	[RelayCommand]
	private void ReplaceNext()
		=> ReplaceWithDirection(SearchDirection.Next);

	[RelayCommand]
	private void Replace()
		=> ReplaceWithDirection(GetCurrentDirection());

	private void ReplaceWithDirection(SearchDirection order)
	{
		if (string.IsNullOrEmpty(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();

		// Find first, then replace the selection.
		FindMatch(order, pattern, options);

		TextEditorBase? editor = GetCurrentTextEditor();

		if (editor is null)
			return;

		if (UseRegex)
			ApplySelectionReplacement(editor, FindReplaceText.ReplaceAll(editor.SelectedText, pattern, ReplaceText, options));
		else
			ApplySelectionReplacement(editor, ReplaceText);
	}

	// ---------------------------------------------------------------------------
	// Find All
	// ---------------------------------------------------------------------------

	[RelayCommand]
	private void FindAll()
	{
		if (string.IsNullOrEmpty(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();

		if (SearchCurrentDocument)
			FindAllInCurrentDocument(pattern, options);
		else if (SearchAllTabs)
			FindAllInAllTabs(pattern, options);
	}

	private void FindAllInCurrentDocument(string pattern, RegexOptions options)
	{
		TextEditorBase? editor = GetCurrentTextEditor();

		if (editor is null)
		{
			ShowStatus("Unsupported document.", FindReplaceStatusType.Error);
			return;
		}

		FindReplaceSource src = _service.BuildFindReplaceSource(
			editor.Document.FileName ?? "Current Document", editor, pattern, options);

		if (src.Count == 0)
		{
			ShowStatus("No matches found.", FindReplaceStatusType.Error);
			return;
		}

		ShowStatus(src.Count + " matches found in the current document.", FindReplaceStatusType.Info);

		_messenger.Send(new FindAllPerformedMessage([src]));
	}

	private void FindAllInAllTabs(string pattern, RegexOptions options)
	{
		int allMatchCount = GetAllTabsMatchCount(pattern, options);

		if (allMatchCount == 0)
		{
			ShowStatus("No matches found.", FindReplaceStatusType.Error);
			return;
		}

		var sources = new List<FindReplaceSource>();

		foreach (TextEditorBase editor in GetOpenTextEditors())
		{
			FindReplaceSource src = _service.BuildFindReplaceSource(
				editor.Document.FileName ?? "Unknown", editor, pattern, options);

			if (src.Count == 0)
				continue;

			sources.Add(src);
		}

		ShowStatus(
			allMatchCount + " matches found in " + sources.Count + " tabs.",
			FindReplaceStatusType.Info);

		_messenger.Send(new FindAllPerformedMessage(sources));
	}

	// ---------------------------------------------------------------------------
	// Replace All
	// ---------------------------------------------------------------------------

	[RelayCommand]
	private void ReplaceAll()
	{
		if (string.IsNullOrWhiteSpace(FindText))
		{
			ShowStatus("Invalid input.", FindReplaceStatusType.Error);
			return;
		}

		var (pattern, options) = GetCurrentPatternAndOptions();
		int matchCount;

		if (SearchCurrentDocument)
		{
			TextEditorBase? editor = GetCurrentTextEditor();

			if (editor is null)
			{
				ShowStatus("Unsupported document.", FindReplaceStatusType.Error);
				return;
			}

			matchCount = FindReplaceText.CountMatches(editor.Text, pattern, options);

			if (matchCount > 0)
			{
				editor.SelectAll();
				ApplySelectionReplacement(editor, FindReplaceText.ReplaceAll(editor.Text, pattern, ReplaceText, options));
				MoveCaretToDocumentStart(editor);
			}
		}
		else if (SearchAllTabs)
		{
			matchCount = GetAllTabsMatchCount(pattern, options);

			if (matchCount > 0)
			{
				foreach (TextEditorBase editor in GetOpenTextEditors())
				{
					editor.SelectAll();
					ApplySelectionReplacement(editor, FindReplaceText.ReplaceAll(editor.Text, pattern, ReplaceText, options));
					MoveCaretToDocumentStart(editor);
				}
			}
		}
		else
		{
			return;
		}

		if (matchCount == 0)
			ShowStatus("No matches found.", FindReplaceStatusType.Error);
		else
			ShowStatus("Replaced " + matchCount + " matches.", FindReplaceStatusType.Info);
	}

	// ---------------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------------

	private static void ApplySelectionReplacement(TextEditorBase editor, string replacement)
	{
		ITextEditTarget? editTarget = editor.WorkspaceEditTarget;
		if (editTarget is null)
		{
			editor.SelectedText = replacement;
			return;
		}

		int selectionStart = editor.SelectionStart;
		int selectionEnd = selectionStart + editor.SelectionLength;
		if (string.Equals(editor.Document.GetText(selectionStart, selectionEnd - selectionStart), replacement, StringComparison.Ordinal))
			return;

		editTarget.Apply(new PreparedTextEdits([new TextEditOperation(selectionStart, selectionEnd, replacement, 0)]));
		editor.CaretOffset = selectionStart + replacement.Length;
	}

	private void ShowMatchCountStatus(string documentText, string pattern, RegexOptions options)
	{
		int currentDocumentMatchCount = FindReplaceText.CountMatches(documentText, pattern, options);

		if (SearchCurrentDocument)
		{
			ShowStatus(
				currentDocumentMatchCount + " matches found in the current document.",
				FindReplaceStatusType.Info);
		}
		else if (SearchAllTabs)
		{
			ShowStatus(
				currentDocumentMatchCount + " matches found in the current document. "
				+ GetAllTabsMatchCount(pattern, options) + " in all tabs combined.",
				FindReplaceStatusType.Info);
		}
	}

	private int GetAllTabsMatchCount(string pattern, RegexOptions options)
	{
		int matchCount = 0;

		foreach (TextEditorBase editor in GetOpenTextEditors())
			matchCount += FindReplaceText.CountMatches(editor.Text, pattern, options);

		return matchCount;
	}

	private void ShowStatus(string message, FindReplaceStatusType type)
	{
		StatusMessage = message;
		StatusType = type;
	}

	private static void MoveCaretToDocumentStart(TextEditorBase? editor)
	{
		if (editor is null)
			return;

		editor.SelectionStart = 0;
		editor.SelectionLength = 0;
		editor.CaretOffset = 0;
	}

	private static void MoveCaretToDocumentEnd(TextEditorBase? editor)
	{
		if (editor is null)
			return;

		editor.SelectionStart = 0;
		editor.SelectionLength = 0;
		editor.CaretOffset = editor.Document.TextLength;
	}
}

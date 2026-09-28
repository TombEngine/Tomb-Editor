using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Filters and shapes completion data for TRX documents.
/// </summary>
public sealed class CompletionManager
{
	private readonly ITRXLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CompletionManager"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public CompletionManager(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <summary>
	/// Filters the given completion items against the current word being typed.
	/// </summary>
	/// <param name="completionData">The completion items to filter.</param>
	/// <param name="currentWord">The word currently being typed.</param>
	/// <returns>The completion items that match the current word.</returns>
	public IReadOnlyList<TextCompletionItem> FilterCompletions(IReadOnlyList<TextCompletionItem> completionData, string currentWord)
	{
		ArgumentNullException.ThrowIfNull(completionData);
		ArgumentNullException.ThrowIfNull(currentWord);

		if (string.IsNullOrEmpty(currentWord))
			return [.. completionData];

		// Extract the actual word content for matching (remove quotes if present).
		string wordForMatching = currentWord.Trim('"');

		// No actual word content (only quotes): keep every candidate.
		if (string.IsNullOrEmpty(wordForMatching))
			return [.. completionData];

		var matches = new List<TextCompletionItem>(completionData.Count);

		foreach (TextCompletionItem item in completionData)
		{
			string insertText = item.InsertText;

			if (insertText.StartsWith('"' + wordForMatching, StringComparison.OrdinalIgnoreCase)
				|| insertText.StartsWith(wordForMatching, StringComparison.OrdinalIgnoreCase)
				|| insertText.Contains(wordForMatching, StringComparison.OrdinalIgnoreCase))
			{
				matches.Add(item);
			}
		}

		// Prioritize exact prefix matches over contains-only matches.
		return matches
			.OrderBy(item => item.InsertText.StartsWith('"' + wordForMatching, StringComparison.OrdinalIgnoreCase)
				|| item.InsertText.StartsWith(wordForMatching, StringComparison.OrdinalIgnoreCase)
					? 0
					: 1)
			.ToList();
	}

	/// <summary>
	/// Determines whether completion should be triggered on an empty line at the given caret offset.
	/// </summary>
	/// <param name="source">The document snapshot to inspect.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <returns>True if completion should be triggered; otherwise false.</returns>
	public bool ShouldTriggerCompletionOnEmptyLine(ITextSnapshot source, int caretOffset)
	{
		ITextLine line = source.GetLineByOffset(caretOffset);
		string lineText = source.GetText(line.Offset, line.Length);
		string currentLineText = _lineService.EscapeComments(lineText).Trim();

		return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText, char.IsLetter)
			|| currentLineText.Equals("\"\"");
	}

	/// <summary>
	/// Computes the document offsets that should be replaced when accepting a completion.
	/// </summary>
	/// <param name="source">The document snapshot to inspect.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <param name="currentWord">The word currently being typed.</param>
	/// <returns>The start and end offsets of the completion replacement range.</returns>
	public (int startOffset, int endOffset) GetCompletionWindowOffsets(ITextSnapshot source, int caretOffset, string currentWord)
	{
		if (!string.IsNullOrEmpty(currentWord))
		{
			// Calculate the start offset based on the actual current word (including quotes if present)
			int startOffset = caretOffset - currentWord.Length;

			// Check if there's a quote after the caret that should be included in replacement
			int endOffset = caretOffset < source.TextLength && source.GetCharAt(caretOffset) == '\"'
				? caretOffset + 1
				: caretOffset;

			return (startOffset, endOffset);
		}
		else
		{
			// No current word, start from current position
			return (caretOffset, caretOffset);
		}
	}
}

#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.Tomb1Main.Parsers;

namespace TombLib.Scripting.Tomb1Main.Services.Implementations;

public sealed class AutocompleteManager : IAutocompleteManager
{
	public List<ICompletionData> FilterCompletions(List<ICompletionData> autocompleteData, string currentWord)
	{
		if (string.IsNullOrEmpty(currentWord))
			return autocompleteData;

		// Extract the actual word content for matching (remove quotes if present)
		string wordForMatching = currentWord.Trim('"');
		string lowerCurrentWord = wordForMatching.ToLower();

		return autocompleteData.Where(item =>
		{
			string lowerItemText = item.Text.ToLower();

			// If we have no actual word content (just quotes), show all
			if (string.IsNullOrEmpty(lowerCurrentWord))
				return true;

			// Exact prefix match (highest priority)
			if (lowerItemText.StartsWith('"' + lowerCurrentWord))
				return true;

			// Prefix match without quotes
			if (lowerItemText.StartsWith(lowerCurrentWord))
				return true;

			// Contains match (lower priority)
			if (lowerItemText.Contains(lowerCurrentWord))
				return true;

			return false;
		})
		.OrderBy(item =>
		{
			string lowerItemText = item.Text.ToLower();

			// Prioritize exact prefix matches
			return lowerItemText.StartsWith('"' + lowerCurrentWord) || lowerItemText.StartsWith(lowerCurrentWord) ? 0 : 1;
		})
		.ToList();
	}

	public bool ShouldTriggerAutocompleteOnEmptyLine(TextDocument document, int caretOffset)
	{
		string currentLineText = LineParser.EscapeComments(document.GetText(document.GetLineByOffset(caretOffset))).Trim();
		return (currentLineText.Length == 1 && char.IsLetter(currentLineText[0])) || currentLineText.Equals("\"\"");
	}

	public (int startOffset, int endOffset) GetCompletionWindowOffsets(TextDocument document, int caretOffset, string currentWord)
	{
		if (!string.IsNullOrEmpty(currentWord))
		{
			// Calculate the start offset based on the actual current word (including quotes if present)
			int startOffset = caretOffset - currentWord.Length;

			// Check if there's a quote after the caret that should be included in replacement
			int endOffset = caretOffset < document.TextLength && document.GetCharAt(caretOffset) == '\"'
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

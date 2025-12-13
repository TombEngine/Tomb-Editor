#nullable enable

using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Tomb1Main.Services.Implementations;

public sealed class TextAnalysisService : ITextAnalysisService
{
	public bool IsValidPositionForCtrlSpaceAutocomplete(TextDocument document, int caretOffset)
	{
		// Don't allow Ctrl+Space in the middle of a word
		// Only allow it at the end of a word or in whitespace

		if (caretOffset == 0)
			return true; // Beginning of document is valid

		if (caretOffset >= document.TextLength)
			return true; // End of document is valid

		// Get the current line and position within it
		DocumentLine currentLine = document.GetLineByOffset(caretOffset);
		int lineStart = currentLine.Offset;
		int caretPosInLine = caretOffset - lineStart;
		string lineText = document.GetText(lineStart, currentLine.Length);

		// If we're at the end of the line, it's valid
		if (caretPosInLine >= lineText.Length)
			return true;

		// Check the character at the current caret position
		char currentChar = lineText[caretPosInLine];

		// If we're on whitespace or JSON delimiters, it's valid
		if (IsWhitespaceOrDelimiter(currentChar))
			return true;

		// Check if we're at the end of a word (next char is delimiter but current position starts a word)
		if (caretPosInLine > 0)
		{
			char prevChar = lineText[caretPosInLine - 1];

			// If the previous character is not whitespace/delimiter but current is,
			// we're at the end of a word, which is valid
			if (!IsWhitespaceOrDelimiter(prevChar) && IsWhitespaceOrDelimiter(currentChar))
				return true;

			// If both previous and current are non-whitespace, we're in the middle of a word
			if (!IsWhitespaceOrDelimiter(prevChar) && !IsWhitespaceOrDelimiter(currentChar))
				return false;
		}

		return true;
	}

	public bool IsValidContextForAutocomplete(TextDocument document, int caretOffset)
	{
		// Simple check to avoid triggering autocomplete inside existing strings
		// Count quotes on the current line before the caret to determine if we're inside a string

		DocumentLine currentLine = document.GetLineByOffset(caretOffset);
		int lineStart = currentLine.Offset;
		int caretPosInLine = caretOffset - lineStart;
		string lineText = document.GetText(lineStart, caretPosInLine);

		// Remove escaped quotes from consideration
		string cleanedText = lineText.Replace("\\\"", "");

		// Count unescaped quotes - if odd number, we're inside a string
		int quoteCount = 0;

		for (int i = 0; i < cleanedText.Length; i++)
		{
			if (cleanedText[i] == '"')
				quoteCount++;
		}

		// If odd number of quotes, we're inside a string (the quote we just typed makes it odd)
		return quoteCount % 2 == 1; // Odd count: caret is inside a string
	}

	public string GetCurrentWordBeingTyped(TextDocument document, int caretOffset)
	{
		if (caretOffset == 0)
			return string.Empty;

		DocumentLine currentLine = document.GetLineByOffset(caretOffset);
		int lineStart = currentLine.Offset;
		int caretPosInLine = caretOffset - lineStart;
		string lineText = document.GetText(lineStart, currentLine.Length);

		// Find word boundaries using JSON-aware delimiters
		int wordStart = FindWordStart(lineText, caretPosInLine);

		// If we just typed a quote, include it in the current word
		string currentWord = wordStart < caretPosInLine
			? lineText[wordStart..caretPosInLine]
			: string.Empty;

		// If the word starts with a quote but the caret is right after a quote we just typed,
		// we want to return just the quote to trigger autocomplete
		if (caretPosInLine > 0 && lineText[caretPosInLine - 1] == '"')
		{
			// Check if this is a fresh quote (not part of an existing word)
			if (wordStart == caretPosInLine - 1)
				return "\"";
		}

		return currentWord;
	}

	private static int FindWordStart(string lineText, int caretPosition)
	{
		int wordStart = caretPosition;

		// Move backwards to find start of word
		while (wordStart > 0)
		{
			char c = lineText[wordStart - 1];

			// Stop at most delimiters, but include quotes as part of the word for proper replacement
			if (IsWhitespaceOrDelimiter(c) && c != '"')
				break;

			wordStart--;
		}

		return wordStart;
	}

	private static bool IsWhitespaceOrDelimiter(char c)
		=> c is ' ' or ',' or '{' or '}' or '[' or ']' or ':' or '\t' or '\n' or '\r' or '"';
}

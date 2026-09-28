using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Core.Identifiers;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Analyzes document text to determine valid completion contexts and words.
/// </summary>
public sealed class TextAnalysisService
{
	// A token is any run of characters that is not whitespace or a JSON structural delimiter;
	// double quotes belong to the token so a fresh quote can trigger completion.
	private static readonly IdentifierCharacterPolicy WordPolicy = IdentifierCharacterPolicy.Create(
		static c => c is not (' ' or ',' or '{' or '}' or '[' or ']' or ':' or '\t' or '\n' or '\r'));

	/// <summary>
	/// Determines whether Ctrl+Space completion is valid at the given caret offset.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The caret offset to validate.</param>
	/// <returns>True if completion is valid at the position; otherwise false.</returns>
	public bool IsValidPositionForCtrlSpaceCompletion(TextDocument document, int caretOffset)
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

	/// <summary>
	/// Determines whether completion is valid in the current typing context.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The caret offset to validate.</param>
	/// <returns>True when the caret is not inside a string literal; otherwise false.</returns>
	public bool IsValidContextForCompletion(TextDocument document, int caretOffset)
	{
		// Completion is only valid when the caret is not inside a string value.
		// Count unescaped quotes on the current line before the caret: an odd count means the
		// caret sits inside a string literal, where schema completion would be meaningless.

		DocumentLine currentLine = document.GetLineByOffset(caretOffset);
		int lineStart = currentLine.Offset;
		int caretPosInLine = caretOffset - lineStart;
		string lineText = document.GetText(lineStart, caretPosInLine);

		// Remove escaped quotes from consideration
		string cleanedText = lineText.Replace("\\\"", "");

		// Count unescaped quotes - an even count means the caret is outside any string
		int quoteCount = 0;

		for (int i = 0; i < cleanedText.Length; i++)
		{
			if (cleanedText[i] == '"')
				quoteCount++;
		}

		// Even quote count: the caret is not inside a string, so the context is valid.
		return quoteCount % 2 == 0;
	}

	/// <summary>
	/// Gets the word currently being typed at the given caret offset.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The caret offset to inspect.</param>
	/// <returns>The word being typed, which may include a leading quote.</returns>
	public string GetCurrentWordBeingTyped(TextDocument document, int caretOffset)
	{
		if (caretOffset == 0)
			return string.Empty;

		var snapshot = new TextDocumentSnapshot(document);
		TextRange? range = IdentifierOperations.FindTokenSpan(snapshot, caretOffset, WordPolicy, IdentifierSpanMode.EndingAtOffset);

		if (range is null)
			return string.Empty;

			return range.Value.GetTextFrom(snapshot);
	}

	private static bool IsWhitespaceOrDelimiter(char c)
		=> c is ' ' or ',' or '{' or '}' or '[' or ']' or ':' or '\t' or '\n' or '\r' or '"';
}

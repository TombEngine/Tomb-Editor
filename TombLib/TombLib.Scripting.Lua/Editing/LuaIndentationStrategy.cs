using Nickelony.IDEKit.Core.Indentation;
using System;
using System.Collections.Generic;
using System.Text;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Computes Lua-specific newline indentation and multiline completion normalization.
/// </summary>
internal sealed class LuaIndentationStrategy : IIndentationPolicy
{
	/// <summary>
	/// Gets the shared policy instance.
	/// </summary>
	public static LuaIndentationStrategy Instance { get; } = new();

	private LuaIndentationStrategy() { }

	/// <inheritdoc/>
	public string GetDesiredIndentation(in IndentationContext context)
	{
		string indentation = context.PreviousLineIndentation;

		if (!context.UseSmartIndent)
			return indentation;

		if (ShouldIncreaseIndentAfterLine(context.PreviousLineText))
			indentation += context.IndentationUnit;

		if (StartsWithDedentToken(context.CurrentLineText))
			indentation = IndentationTextHelper.RemoveSingleIndentLevel(indentation, context.IndentationUnit);

		return indentation;
	}

	/// <summary>
	/// Builds the text that should be inserted when Enter is pressed inside Lua code.
	/// </summary>
	/// <param name="context">The enter-insertion inputs.</param>
	/// <returns>The text to insert and the resulting caret position.</returns>
	public EnterInsertionResult BuildEnterInsertion(in EnterInsertionContext context)
	{
		string newLineText = string.IsNullOrEmpty(context.NewLineText) ? Environment.NewLine : context.NewLineText;

		string nextLineIndentation = context.CurrentLineIndentation;

		if (context.UseSmartIndent && ShouldIncreaseIndentAfterLine(context.LineTextBeforeCaret))
			nextLineIndentation += context.IndentationUnit;

		bool shouldSplitBeforeDedent = context.UseSmartIndent
			&& nextLineIndentation.Length > context.CurrentLineIndentation.Length
			&& StartsWithDedentToken(context.LineTextAfterCaret);

		if (!shouldSplitBeforeDedent)
		{
			string text = newLineText + nextLineIndentation;
			return new EnterInsertionResult(text, text.Length, 0);
		}

		string splitText = newLineText + nextLineIndentation + newLineText + context.CurrentLineIndentation;
		return new EnterInsertionResult(
			splitText,
			newLineText.Length + nextLineIndentation.Length,
			IndentationTextHelper.GetLeadingWhitespaceLength(context.LineTextAfterCaret));
	}

	/// <summary>
	/// Normalizes multiline completion insertion relative to the current line indentation.
	/// </summary>
	/// <param name="context">The completion-insertion inputs.</param>
	/// <returns>The normalized insertion text and caret offset.</returns>
	public CompletionInsertionResult NormalizeCompletionInsertion(in CompletionInsertionContext context)
	{
		if (string.IsNullOrEmpty(context.Text) || !IndentationTextHelper.ContainsLineBreak(context.Text))
			return new CompletionInsertionResult(context.Text, context.CaretOffset);

		IReadOnlyList<IndentationTextLine> lines = IndentationTextHelper.SplitLines(context.Text);
		var builder = new StringBuilder(context.Text.Length + Math.Max(0, lines.Count - 1) * context.CurrentLineIndentation.Length);
		int? normalizedCaretOffset = null;
		int relativeIndentLevel = 0;

		for (int i = 0; i < lines.Count; i++)
		{
			IndentationTextLine line = lines[i];
			int originalLeadingWhitespaceLength = IndentationTextHelper.GetLeadingWhitespaceLength(line.Content);
			string trimmedContent = line.Content[originalLeadingWhitespaceLength..];
			int currentIndentLevel = i == 0
				? 0
				: Math.Max(0, relativeIndentLevel - GetDedentLevel(trimmedContent));
			string normalizedIndentation = i == 0
				? string.Empty
				: IndentationTextHelper.BuildIndentation(context.CurrentLineIndentation, context.IndentationUnit, currentIndentLevel);
			string normalizedLineContent = trimmedContent.Length == 0
				? normalizedIndentation
				: normalizedIndentation + trimmedContent;

			if (context.CaretOffset.HasValue
				&& context.CaretOffset.Value >= line.StartOffset
				&& context.CaretOffset.Value <= line.StartOffset + line.Content.Length)
			{
				int caretColumn = context.CaretOffset.Value - line.StartOffset;
				int normalizedLeadingWhitespaceLength = normalizedLineContent.Length - trimmedContent.Length;
				int contentColumn = Math.Max(0, caretColumn - originalLeadingWhitespaceLength);
				normalizedCaretOffset = builder.Length + normalizedLeadingWhitespaceLength + contentColumn;
			}

			builder.Append(normalizedLineContent);
			builder.Append(line.Delimiter);
			relativeIndentLevel = currentIndentLevel + GetIndentIncrease(trimmedContent);
		}

		if (context.CaretOffset == context.Text.Length)
			normalizedCaretOffset = builder.Length;

		return new CompletionInsertionResult(builder.ToString(), normalizedCaretOffset ?? context.CaretOffset);
	}

	private static int GetDedentLevel(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).TrimStart();

		if (string.IsNullOrEmpty(codeText))
			return 0;

		return StartsWithDedentToken(codeText) ? 1 : 0;
	}

	private static int GetIndentIncrease(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).Trim();

		if (string.IsNullOrEmpty(codeText))
			return 0;

		return ShouldIncreaseIndentAfterCode(codeText) ? 1 : 0;
	}

	private static bool ShouldIncreaseIndentAfterLine(string lineText)
		=> ShouldIncreaseIndentAfterCode(LuaLineParser.ExtractCodeText(lineText).Trim());

	private static bool ShouldIncreaseIndentAfterCode(string codeText)
	{
		if (string.IsNullOrEmpty(codeText))
			return false;

		if (StartsWithWord(codeText, "repeat")
			|| StartsWithWord(codeText, "else")
			|| StartsWithWord(codeText, "elseif")
			|| EndsWithWord(codeText, "then")
			|| EndsWithWord(codeText, "do")
			|| ContainsWord(codeText, "function"))
		{
			return true;
		}

		return HasPositiveDelimiterBalance(codeText);
	}

	private static bool StartsWithDedentToken(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).TrimStart();

		if (string.IsNullOrEmpty(codeText))
			return false;

		return StartsWithWord(codeText, "end")
			|| StartsWithWord(codeText, "until")
			|| StartsWithWord(codeText, "else")
			|| StartsWithWord(codeText, "elseif")
			|| StartsWithClosingDelimiter(codeText[0]);
	}

	private static bool HasPositiveDelimiterBalance(string codeText)
	{
		int balance = 0;

		foreach (char character in codeText)
		{
			balance += character switch
			{
				'(' or '{' or '[' => 1,
				')' or '}' or ']' => -1,
				_ => 0
			};
		}

		return balance > 0;
	}

	private static bool StartsWithClosingDelimiter(char character)
		=> character is ')' or '}' or ']';

	private static bool StartsWithWord(string text, string word)
	{
		return text.StartsWith(word, StringComparison.Ordinal)
			&& (text.Length == word.Length || !LuaLineParser.IsIdentifierCharacter(text[word.Length]));
	}

	private static bool EndsWithWord(string text, string word)
	{
		if (!text.EndsWith(word, StringComparison.Ordinal))
			return false;

		int wordStart = text.Length - word.Length;
		return wordStart == 0 || !LuaLineParser.IsIdentifierCharacter(text[wordStart - 1]);
	}

	private static bool ContainsWord(string text, string word)
	{
		int searchIndex = 0;

		while (searchIndex < text.Length)
		{
			int wordIndex = text.IndexOf(word, searchIndex, StringComparison.Ordinal);

			if (wordIndex < 0)
				return false;

			bool hasLeadingBoundary = wordIndex == 0 || !LuaLineParser.IsIdentifierCharacter(text[wordIndex - 1]);
			int wordEnd = wordIndex + word.Length;
			bool hasTrailingBoundary = wordEnd == text.Length || !LuaLineParser.IsIdentifierCharacter(text[wordEnd]);

			if (hasLeadingBoundary && hasTrailingBoundary)
				return true;

			searchIndex = wordIndex + word.Length;
		}

		return false;
	}
}

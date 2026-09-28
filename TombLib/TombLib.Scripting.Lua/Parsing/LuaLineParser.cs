using System;
using System.Collections.Generic;
using System.Text;

namespace TombLib.Scripting.Lua.Parsing;

/// <summary>
/// Identifies the parser mode that must continue across Lua document lines.
/// Single and double quoted strings cannot legally span lines, so only the
/// long-bracket modes are ever persisted in <see cref="LuaLineParserState"/>.
/// </summary>
internal enum LuaLineParserStateKind
{
	/// <summary>
	/// The parser is not inside a multi-line Lua construct.
	/// </summary>
	None,

	/// <summary>
	/// The parser is inside a single-quoted string literal.
	/// </summary>
	SingleQuotedString,

	/// <summary>
	/// The parser is inside a double-quoted string literal.
	/// </summary>
	DoubleQuotedString,

	/// <summary>
	/// The parser is inside a long-bracket string literal.
	/// </summary>
	LongString,

	/// <summary>
	/// The parser is inside a long-bracket comment.
	/// </summary>
	LongComment
}

/// <summary>
/// Stores the parser continuation state needed to evaluate long strings and long comments across line boundaries.
/// </summary>
internal readonly struct LuaLineParserState(LuaLineParserStateKind kind, int longBracketEqualsCount)
{
	/// <summary>
	/// Gets the long-block parser mode that should continue onto the next line.
	/// </summary>
	public LuaLineParserStateKind Kind { get; } = kind;

	/// <summary>
	/// Gets the number of <c>=</c> characters used by the active long-bracket delimiter.
	/// </summary>
	public int LongBracketEqualsCount { get; } = longBracketEqualsCount;
}

/// <summary>
/// Provides lightweight line-based parsing helpers for Lua identifiers, comments, and long-bracket strings.
/// </summary>
internal static class LuaLineParser
{
	/// <summary>
	/// Controls what the shared line walker emits for each scanned character.
	/// </summary>
	private enum ScanMode
	{
		/// <summary>
		/// Emits nothing; only tracks the resulting parser state.
		/// </summary>
		Detect,

		/// <summary>
		/// Emits quoted and long-bracket string content, stripping comments.
		/// </summary>
		PreserveStrings,

		/// <summary>
		/// Emits only code characters, skipping comments and string content.
		/// </summary>
		CodeOnly
	}

	/// <summary>
	/// Determines whether a character can appear within a Lua identifier.
	/// </summary>
	/// <param name="character">The character to test.</param>
	/// <returns><see langword="true"/> if the character is valid inside an identifier; otherwise, <see langword="false"/>.</returns>
	public static bool IsIdentifierCharacter(char character)
		=> char.IsLetterOrDigit(character) || character == '_';

	/// <summary>
	/// Determines whether a character can start an identifier-triggered completion request.
	/// </summary>
	/// <param name="character">The character to test.</param>
	/// <returns><see langword="true"/> if the character is a valid identifier trigger; otherwise, <see langword="false"/>.</returns>
	public static bool IsIdentifierTriggerCharacter(char character)
		=> char.IsLetter(character) || character == '_';

	/// <summary>
	/// Determines whether the inspected line fragment currently ends inside a comment or string.
	/// </summary>
	/// <param name="lineText">The line text to inspect, typically truncated at the current offset.</param>
	/// <returns><see langword="true"/> if the fragment is inside a comment or string; otherwise, <see langword="false"/>.</returns>
	public static bool IsInsideCommentOrString(string lineText)
		=> IsInsideCommentOrString(lineText, default, out _);

	internal static bool IsInsideCommentOrString(string lineText, LuaLineParserState initialState, out LuaLineParserState finalState)
	{
		(LuaLineParserStateKind stateKind, int longBracketEqualsCount, bool lineCommentFound) = ScanLine(lineText, initialState, ScanMode.Detect, null);

		if (lineCommentFound)
		{
			finalState = default;
			return true;
		}

		finalState = CreateContinuationState(stateKind, longBracketEqualsCount);
		return stateKind != LuaLineParserStateKind.None;
	}

	/// <summary>
	/// Removes a trailing Lua line comment while preserving quoted strings and long-bracket strings.
	/// </summary>
	/// <param name="lineText">The line text to process.</param>
	/// <returns>The line text without a trailing line comment.</returns>
	public static string StripLineComment(string lineText)
	{
		if (string.IsNullOrEmpty(lineText))
			return string.Empty;

		var builder = new StringBuilder(lineText.Length);
		ScanLine(lineText, default, ScanMode.PreserveStrings, builder);
		return builder.ToString();
	}

	/// <summary>
	/// Extracts the code-visible characters from a line while skipping comment and string contents.
	/// </summary>
	/// <param name="lineText">The line text to process.</param>
	/// <returns>The characters that remain visible to Lua block-indentation heuristics.</returns>
	public static string ExtractCodeText(string lineText)
	{
		if (string.IsNullOrEmpty(lineText))
			return string.Empty;

		var builder = new StringBuilder(lineText.Length);
		ScanLine(lineText, default, ScanMode.CodeOnly, builder);
		return builder.ToString();
	}

	/// <summary>
	/// Enumerates structural characters that remain after stripping comments and string content from a line.
	/// </summary>
	/// <param name="lineText">The line text to inspect.</param>
	/// <returns>The structural characters that participate in brace and delimiter analysis.</returns>
	public static IEnumerable<char> EnumerateStructuralCharacters(string lineText)
		=> EnumerateStructuralCharacters(lineText, default, captureFinalState: null);

	/// <summary>
	/// Enumerates structural characters from a line while carrying long-string and long-comment
	/// continuation state across line boundaries. Use the captured state as the next line's
	/// initial state so multi-line <c>[[...]]</c> blocks can not break callers that do brace
	/// tracking across the whole document.
	/// </summary>
	internal static IEnumerable<char> EnumerateStructuralCharacters(string lineText, LuaLineParserState initialState, Action<LuaLineParserState>? captureFinalState)
	{
		var builder = new StringBuilder(lineText.Length);
		(LuaLineParserStateKind stateKind, int longBracketEqualsCount, _) = ScanLine(lineText, initialState, ScanMode.CodeOnly, builder);

		captureFinalState?.Invoke(CreateContinuationState(stateKind, longBracketEqualsCount));

		foreach (char character in builder.ToString())
			yield return character;
	}

	private static (LuaLineParserStateKind FinalState, int LongBracketEqualsCount, bool LineCommentFound) ScanLine(string lineText, LuaLineParserState initialState, ScanMode mode, StringBuilder? output)
	{
		LuaLineParserStateKind state = initialState.Kind;
		int longBracketEqualsCount = initialState.LongBracketEqualsCount;
		bool lineCommentFound = false;

		for (int i = 0; i < lineText.Length; i++)
		{
			char currentChar = lineText[i];

			if (state == LuaLineParserStateKind.LongString || state == LuaLineParserStateKind.LongComment)
			{
				if (TryMatchLongBracketEnd(lineText, i, longBracketEqualsCount, out int endTokenLength))
				{
					if (mode == ScanMode.PreserveStrings && state == LuaLineParserStateKind.LongString)
						output?.Append(lineText, i, endTokenLength);

					i += endTokenLength - 1;
					state = LuaLineParserStateKind.None;

					if (mode == ScanMode.CodeOnly)
						longBracketEqualsCount = 0;
				}
				else if (mode == ScanMode.PreserveStrings && state == LuaLineParserStateKind.LongString)
				{
					output?.Append(currentChar);
				}

				continue;
			}

			if (state == LuaLineParserStateKind.SingleQuotedString || state == LuaLineParserStateKind.DoubleQuotedString)
			{
				if (mode == ScanMode.PreserveStrings)
					output?.Append(currentChar);

				if (currentChar == '\\' && i + 1 < lineText.Length)
				{
					if (mode == ScanMode.PreserveStrings)
						output?.Append(lineText[i + 1]);

					i++;
					continue;
				}

				if ((state == LuaLineParserStateKind.SingleQuotedString && currentChar == '\'')
					|| (state == LuaLineParserStateKind.DoubleQuotedString && currentChar == '"'))
				{
					state = LuaLineParserStateKind.None;
				}

				continue;
			}

			if (TryMatchLongCommentStart(lineText, i, out longBracketEqualsCount, out int longCommentStartLength))
			{
				state = LuaLineParserStateKind.LongComment;
				i += longCommentStartLength - 1;
				continue;
			}

			if (IsLineCommentStart(lineText, i))
			{
				lineCommentFound = true;
				break;
			}

			if (TryMatchLongBracketStart(lineText, i, out longBracketEqualsCount, out int longStringStartLength))
			{
				if (mode == ScanMode.PreserveStrings)
					output?.Append(lineText, i, longStringStartLength);

				state = LuaLineParserStateKind.LongString;
				i += longStringStartLength - 1;
				continue;
			}

			if (currentChar == '\'')
			{
				if (mode == ScanMode.PreserveStrings)
					output?.Append(currentChar);

				state = LuaLineParserStateKind.SingleQuotedString;
				continue;
			}

			if (currentChar == '"')
			{
				if (mode == ScanMode.PreserveStrings)
					output?.Append(currentChar);

				state = LuaLineParserStateKind.DoubleQuotedString;
				continue;
			}

			if (mode is ScanMode.PreserveStrings or ScanMode.CodeOnly)
				output?.Append(currentChar);
		}

		return (state, longBracketEqualsCount, lineCommentFound);
	}

	private static bool TryMatchLongCommentStart(string lineText, int index, out int equalsCount, out int tokenLength)
	{
		equalsCount = 0;
		tokenLength = 0;

		if (!IsLineCommentStart(lineText, index) || !TryMatchLongBracketStart(lineText, index + 2, out equalsCount, out int bracketTokenLength))
			return false;

		tokenLength = 2 + bracketTokenLength;
		return true;
	}

	private static bool TryMatchLongBracketStart(string lineText, int index, out int equalsCount, out int tokenLength)
	{
		equalsCount = 0;
		tokenLength = 0;

		if (index >= lineText.Length || lineText[index] != '[')
			return false;

		int probeIndex = index + 1;

		while (probeIndex < lineText.Length && lineText[probeIndex] == '=')
		{
			equalsCount++;
			probeIndex++;
		}

		if (probeIndex >= lineText.Length || lineText[probeIndex] != '[')
		{
			equalsCount = 0;
			return false;
		}

		tokenLength = probeIndex - index + 1;
		return true;
	}

	private static bool TryMatchLongBracketEnd(string lineText, int index, int equalsCount, out int tokenLength)
	{
		tokenLength = 0;

		if (index >= lineText.Length || lineText[index] != ']')
			return false;

		int probeIndex = index + 1;

		for (int i = 0; i < equalsCount; i++)
		{
			if (probeIndex >= lineText.Length || lineText[probeIndex] != '=')
				return false;

			probeIndex++;
		}

		if (probeIndex >= lineText.Length || lineText[probeIndex] != ']')
			return false;

		tokenLength = probeIndex - index + 1;
		return true;
	}

	private static bool IsLineCommentStart(string lineText, int index)
		=> lineText[index] == '-' && index + 1 < lineText.Length && lineText[index + 1] == '-';

	private static LuaLineParserState CreateContinuationState(LuaLineParserStateKind state, int longBracketEqualsCount)
	{
		return state is LuaLineParserStateKind.LongString or LuaLineParserStateKind.LongComment
			? new LuaLineParserState(state, longBracketEqualsCount)
			: default;
	}
}

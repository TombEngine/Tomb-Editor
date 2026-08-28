using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Types;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Default implementation of <see cref="IClassicScriptLineService"/>.
/// Provides line-level text operations using Core helpers and, where needed,
/// regex patterns for the ClassicScript section, include, and NG-string syntax.
/// </summary>
public sealed class ClassicScriptLineService : IClassicScriptLineService
{
	private const string CommentDelimiter = ";";
	private const char ContinuationMarker = '>';

	// Regex patterns for the ClassicScript section-header, include, and NG-string syntax.
	private static readonly Regex SectionHeaderRegex = new(@"^\s*\[(\b.*\b)\]\s*(;.*)?$", RegexOptions.Compiled);
	private static readonly Regex IncludeLineRegex = new("\".*\"", RegexOptions.Compiled);
	private static readonly Regex NGStringIndexRegex = new(@"^\d+:\s*", RegexOptions.Compiled | RegexOptions.Multiline);

	/// <inheritdoc/>
	public string? GetWordAtOffset(ITextSnapshot source, int offset)
	{
		if (offset > source.TextLength)
			return null;

		ITextLine line = source.GetLineByOffset(offset);

		int wordStart = -1;
		int wordEnd = -1;

		for (int i = offset; i <= line.EndOffset; i++)
		{
			if (i == line.EndOffset)
			{
				wordEnd = i;
				break;
			}

			char c = source.GetCharAt(i);

			if (c == ',' || c == '=' || c == ';' || c == '+' || c == '-' || c == '*' || c == '/' || c == ')')
			{
				wordEnd = i;
				break;
			}
		}

		if (offset == line.Offset)
		{
			wordStart = offset;
		}
		else
		{
			for (int i = offset - 1; i >= line.Offset; i--)
			{
				if (i == line.Offset)
				{
					wordStart = i;
					break;
				}

				char c = source.GetCharAt(i);

				if (c == ',' || c == '=' || c == '+' || c == '-' || c == '*' || c == '/' || c == '(')
				{
					wordStart = i + 1;
					break;
				}
			}
		}

		if (wordStart >= 0 && wordEnd >= 0 && wordStart < wordEnd)
			return source.GetText(wordStart, wordEnd - wordStart).Trim();

		return null;
	}

	/// <inheritdoc/>
	public WordType GetWordTypeAtOffset(ITextSnapshot source, int offset)
	{
		if (offset > source.TextLength)
			return WordType.Unknown;

		ITextLine line = source.GetLineByOffset(offset);

		for (int i = offset; i <= line.EndOffset; i++)
		{
			if (i == line.EndOffset)
			{
				for (int j = i - 1; j >= line.Offset; j--)
				{
					char ch = source.GetCharAt(j);

					if (ch == '_')
						return WordType.MnemonicConstant;
					else if (ch == '$')
						return WordType.Hexadecimal;
					else if (ch == ',' || ch == '=' || ch == '+' || ch == '-' || ch == '*' || ch == '/' || ch == '(')
						return WordType.Unknown;
				}

				break;
			}

			char c = source.GetCharAt(i);

			if (c == ']')
			{
				for (int j = i - 1; j >= line.Offset; j--)
				{
					if (source.GetCharAt(j) == '[')
						return WordType.Header;
				}
			}
			else if (c == '=')
			{
				return WordType.Command;
			}
			else if (c == '_')
			{
				return WordType.MnemonicConstant;
			}
			else if (c == '$')
			{
				return WordType.Hexadecimal;
			}
			else if (c == ',' || c == ';' || c == '+' || c == '-' || c == '*' || c == '/' || c == ')')
			{
				for (int j = i - 1; j >= line.Offset; j--)
				{
					char ch = source.GetCharAt(j);

					if (ch == '_')
						return WordType.MnemonicConstant;
					else if (ch == '$')
						return WordType.Hexadecimal;
					else if (ch == ',' || ch == '=' || ch == '+' || ch == '-' || ch == '*' || ch == '/' || ch == '(')
						return WordType.Unknown;
				}
			}
		}

		return WordType.Unknown;
	}

	/// <inheritdoc/>
	public bool IsSectionHeaderLine(string lineText)
		=> SectionHeaderRegex.IsMatch(lineText);

	/// <inheritdoc/>
	public string? GetSectionHeaderText(string sectionHeaderLine)
	{
		Match match = SectionHeaderRegex.Match(sectionHeaderLine);

		if (!match.Success)
			return null;

		return match.Groups[1].Value;
	}

	/// <inheritdoc/>
	public bool IsEmptyOrComments(string? lineText)
		=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(CommentDelimiter, StringComparison.Ordinal);

	/// <inheritdoc/>
	public bool IsValidIncludeLine(string lineText)
	{
		return lineText.TrimStart().StartsWith("#include ", StringComparison.OrdinalIgnoreCase)
			&& IncludeLineRegex.IsMatch(lineText);
	}

	/// <inheritdoc/>
	public bool IsStandardStringSectionName(string? sectionName)
	{
		if (string.IsNullOrEmpty(sectionName))
			return false;

		return sectionName.Equals("strings", StringComparison.OrdinalIgnoreCase)
			|| sectionName.Equals("pcstrings", StringComparison.OrdinalIgnoreCase)
			|| sectionName.Equals("psxstrings", StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public bool IsExtraNGSectionName(string? sectionName)
	{
		if (string.IsNullOrEmpty(sectionName))
			return false;

		return sectionName.Equals("extrang", StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public bool IsStringSectionName(string? sectionName)
		=> IsStandardStringSectionName(sectionName) || IsExtraNGSectionName(sectionName);

	/// <inheritdoc/>
	public string RemoveComments(string lineText)
		=> LineCommentHelper.RemoveLineComment(lineText, CommentDelimiter);

	/// <inheritdoc/>
	public string EscapeComments(string lineText)
		=> LineCommentHelper.MaskLineComment(lineText, CommentDelimiter);

	/// <inheritdoc/>
	public string EscapeCommentsAndNewLines(string lineText)
		=> EscapeComments(lineText).Replace(ContinuationMarker, ' ').Replace('\n', ' ').Replace('\r', ' ');

	/// <inheritdoc/>
	public string RemoveNGStringIndex(string lineText)
		=> NGStringIndexRegex.Replace(lineText, string.Empty);
}

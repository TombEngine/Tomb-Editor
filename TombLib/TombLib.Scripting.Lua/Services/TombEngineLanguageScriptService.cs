#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Lua.Services;

public sealed class TombEngineLanguageScriptService
{
	private static readonly Regex SetStringsRegex = new(@"TEN\.Flow\.SetStrings\s*\(\s*(?<name>[^)\s]+)\s*\)", RegexOptions.IgnoreCase);

	public int? TryInsertLanguageScript(TextDocument document, string languageScript)
	{
		string? stringsVariableName = TryGetStringsVariableName(document);

		if (stringsVariableName is null)
			return null;

		DocumentLine? stringsStartLine = FindStringsStartLine(document, stringsVariableName);

		if (stringsStartLine is null)
			return null;

		DocumentLine? stopLine = FindStringsStopLine(document, stringsStartLine);

		if (stopLine is null)
			return null;

		DocumentLine? insertionLine = FindLanguageInsertionLine(document, stringsStartLine, stopLine);

		if (insertionLine is not null)
			return InsertLanguageScript(document, languageScript, insertionLine);

		return InsertLanguageScriptIntoEmptyTable(document, languageScript, stopLine);
	}

	private static string? TryGetStringsVariableName(TextDocument document)
	{
		foreach (DocumentLine line in document.Lines)
		{
			string lineText = StripLuaLineComment(document.GetText(line));
			Match match = SetStringsRegex.Match(lineText);

			if (match.Success)
				return match.Groups["name"].Value;
		}

		return null;
	}

	private static DocumentLine? FindStringsStartLine(TextDocument document, string stringsVariableName)
	{
		var regex = new Regex(@"^\s*local\s+" + Regex.Escape(stringsVariableName) + @"\s*=");

		foreach (DocumentLine line in document.Lines)
		{
			if (regex.IsMatch(StripLuaLineComment(document.GetText(line))))
				return line;
		}

		return null;
	}

	private static DocumentLine? FindStringsStopLine(TextDocument document, DocumentLine stringsStartLine)
	{
		int bracketDepth = 0;
		bool foundOpeningBracket = false;

		for (DocumentLine? line = stringsStartLine; line is not null; line = line.NextLine)
		{
			string lineText = StripLuaLineComment(document.GetText(line));

			foreach (char character in EnumerateStructuralLuaCharacters(lineText))
			{
				if (character == '{')
				{
					bracketDepth++;
					foundOpeningBracket = true;
				}
				else if (character == '}')
				{
					if (!foundOpeningBracket || bracketDepth == 0)
						return null;

					bracketDepth--;

					if (bracketDepth == 0)
						return line;
				}
			}
		}

		return null;
	}

	private static DocumentLine? FindLanguageInsertionLine(TextDocument document, DocumentLine stringsStartLine, DocumentLine stopLine)
	{
		for (int i = stopLine.LineNumber - 1; i > stringsStartLine.LineNumber; i--)
		{
			DocumentLine line = document.GetLineByNumber(i);
			string cleanLine = StripLuaLineComment(document.GetText(line)).TrimEnd();

			if (cleanLine.EndsWith("}") || cleanLine.EndsWith("},"))
				return line;
		}

		return null;
	}

	private static int InsertLanguageScript(TextDocument document, string languageScript, DocumentLine insertionLine)
	{
		string rawLine = document.GetText(insertionLine);
		string cleanLine = StripLuaLineComment(rawLine).TrimEnd();

		if (cleanLine.EndsWith("}"))
		{
			int commaOffset = insertionLine.Offset + cleanLine.Length;
			document.Insert(commaOffset, ",");
		}

		document.Insert(insertionLine.EndOffset, Environment.NewLine + languageScript);
		return insertionLine.LineNumber + 1;
	}

	private static int InsertLanguageScriptIntoEmptyTable(TextDocument document, string languageScript, DocumentLine stopLine)
	{
		document.Insert(stopLine.Offset, languageScript + Environment.NewLine);
		return stopLine.LineNumber;
	}

	private static string StripLuaLineComment(string lineText)
	{
		var builder = new StringBuilder(lineText.Length);

		bool isInSingleQuotedString = false;
		bool isInDoubleQuotedString = false;

		for (int i = 0; i < lineText.Length; i++)
		{
			char character = lineText[i];

			if ((isInSingleQuotedString || isInDoubleQuotedString) && character == '\\' && i + 1 < lineText.Length)
			{
				builder.Append(character);
				builder.Append(lineText[i + 1]);

				i++;
				continue;
			}

			if (!isInDoubleQuotedString && character == '\'')
			{
				isInSingleQuotedString = !isInSingleQuotedString;
				builder.Append(character);
				continue;
			}

			if (!isInSingleQuotedString && character == '"')
			{
				isInDoubleQuotedString = !isInDoubleQuotedString;
				builder.Append(character);
				continue;
			}

			if (!isInSingleQuotedString && !isInDoubleQuotedString && character == '-' && i + 1 < lineText.Length && lineText[i + 1] == '-')
				break;

			builder.Append(character);
		}

		return builder.ToString();
	}

	private static IEnumerable<char> EnumerateStructuralLuaCharacters(string lineText)
	{
		bool isInSingleQuotedString = false;
		bool isInDoubleQuotedString = false;

		for (int i = 0; i < lineText.Length; i++)
		{
			char character = lineText[i];

			if ((isInSingleQuotedString || isInDoubleQuotedString) && character == '\\' && i + 1 < lineText.Length)
			{
				i++;
				continue;
			}

			if (!isInDoubleQuotedString && character == '\'')
			{
				isInSingleQuotedString = !isInSingleQuotedString;
				continue;
			}

			if (!isInSingleQuotedString && character == '"')
			{
				isInDoubleQuotedString = !isInDoubleQuotedString;
				continue;
			}

			if (!isInSingleQuotedString && !isInDoubleQuotedString)
				yield return character;
		}
	}
}

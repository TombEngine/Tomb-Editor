using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.Lua.Parsing;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.Lua.Documents;

/// <summary>
/// Inserts generated Tomb Engine language strings into an existing Lua strings table.
/// </summary>
public sealed partial class TombEngineLanguageScriptService
{
	[GeneratedRegex(@"TEN\.Flow\.SetStrings\s*\(\s*(?<name>[^)\s]+)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex GetSetStringsRegex();

	private static readonly Regex SetStringsRegex = GetSetStringsRegex();

	/// <summary>
	/// Inserts a generated language entry into the strings table referenced by <c>TEN.Flow.SetStrings(...)</c>.
	/// </summary>
	/// <param name="document">The document to modify.</param>
	/// <param name="languageScript">The generated language-table entry to insert.</param>
	/// <returns>The one-based line number of the inserted entry, or <see langword="null"/> when no suitable strings table could be found.</returns>
	public int? InsertLanguageScript(TextDocument document, string languageScript)
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

	/// <summary>
	/// Inserts a generated language entry through the editor's canonical edit target.
	/// </summary>
	/// <param name="textEditor">The editor whose target should receive the mutation.</param>
	/// <param name="languageScript">The generated language-table entry to insert.</param>
	/// <returns>The one-based line number of the inserted entry, or <see langword="null"/> when no suitable strings table could be found.</returns>
	public int? InsertLanguageScript(TextEditorBase textEditor, string languageScript)
	{
		string? stringsVariableName = TryGetStringsVariableName(textEditor.Document);

		if (stringsVariableName is null)
			return null;

		DocumentLine? stringsStartLine = FindStringsStartLine(textEditor.Document, stringsVariableName);

		if (stringsStartLine is null)
			return null;

		DocumentLine? stopLine = FindStringsStopLine(textEditor.Document, stringsStartLine);

		if (stopLine is null)
			return null;

		DocumentLine? insertionLine = FindLanguageInsertionLine(textEditor.Document, stringsStartLine, stopLine);

		return insertionLine is not null
			? InsertLanguageScript(textEditor, languageScript, insertionLine)
			: InsertLanguageScriptIntoEmptyTable(textEditor, languageScript, stopLine);
	}

	private static string? TryGetStringsVariableName(TextDocument document)
	{
		foreach (DocumentLine line in document.Lines)
		{
			string lineText = LuaLineParser.StripLineComment(document.GetText(line));
			Match match = SetStringsRegex.Match(lineText);

			if (match.Success)
				return match.Groups["name"].Value;
		}

		return null;
	}

	private static DocumentLine? FindStringsStartLine(TextDocument document, string stringsVariableName)
	{
		Regex regex = CreateStringsStartRegex(stringsVariableName);

		foreach (DocumentLine line in document.Lines)
		{
			if (regex.IsMatch(LuaLineParser.StripLineComment(document.GetText(line))))
				return line;
		}

		return null;
	}

	private static Regex CreateStringsStartRegex(string stringsVariableName)
		=> new(@"^\s*local\s+" + Regex.Escape(stringsVariableName) + @"\s*=", RegexOptions.Compiled);

	private static DocumentLine? FindStringsStopLine(TextDocument document, DocumentLine stringsStartLine)
	{
		int bracketDepth = 0;
		bool foundOpeningBracket = false;
		LuaLineParserState parserState = default;

		for (DocumentLine? line = stringsStartLine; line is not null; line = line.NextLine)
		{
			// Use the raw line text (not StripLineComment) so the structural enumerator can keep
			// long-string and long-comment continuation state in sync across lines. A multi-line
			// `[[...]]` entry inside the strings table would otherwise leak `{` / `}` characters
			// from the string body into our brace counter.
			string lineText = document.GetText(line);
			LuaLineParserState capturedState = parserState;

			foreach (char character in LuaLineParser.EnumerateStructuralCharacters(lineText, parserState, state => capturedState = state))
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

			parserState = capturedState;
		}

		return null;
	}

	private static DocumentLine? FindLanguageInsertionLine(TextDocument document, DocumentLine stringsStartLine, DocumentLine stopLine)
	{
		// Walk forward from the strings-table opener so we can keep parser continuation state in
		// lockstep, then pick the latest line that ends with `}` or `},` while not sitting inside
		// a long string or comment carried over from earlier lines.
		LuaLineParserState parserState = default;
		DocumentLine? bestCandidate = null;

		for (DocumentLine? line = stringsStartLine; line is not null && line.LineNumber <= stopLine.LineNumber; line = line.NextLine)
		{
			LuaLineParserState capturedState = parserState;
			bool insideLongBlockAtLineStart = parserState.Kind != LuaLineParserStateKind.None;

			// Drain the enumerator to advance parser state to the next line; ignore the chars.
			foreach (char _ in LuaLineParser.EnumerateStructuralCharacters(document.GetText(line), parserState, state => capturedState = state))
			{ }

			if (line.LineNumber <= stringsStartLine.LineNumber || line.LineNumber >= stopLine.LineNumber)
			{
				parserState = capturedState;
				continue;
			}

			if (!insideLongBlockAtLineStart)
			{
				string cleanLine = LuaLineParser.StripLineComment(document.GetText(line)).TrimEnd();

				if (cleanLine.EndsWith('}') || cleanLine.EndsWith("},"))
					bestCandidate = line;
			}

			parserState = capturedState;
		}

		return bestCandidate;
	}

	private static int InsertLanguageScript(TextDocument document, string languageScript, DocumentLine insertionLine)
	{
		string rawLine = document.GetText(insertionLine);
		string cleanLine = LuaLineParser.StripLineComment(rawLine).TrimEnd();

		if (cleanLine.EndsWith('}'))
		{
			int commaOffset = insertionLine.Offset + cleanLine.Length;
			document.Insert(commaOffset, ",");
		}

		document.Insert(insertionLine.EndOffset, Environment.NewLine + languageScript);
		return insertionLine.LineNumber + 1;
	}

	private static int InsertLanguageScript(TextEditorBase textEditor, string languageScript, DocumentLine insertionLine)
	{
		string rawLine = textEditor.Document.GetText(insertionLine);
		string cleanLine = LuaLineParser.StripLineComment(rawLine).TrimEnd();

		if (cleanLine.EndsWith('}'))
			textEditor.InsertText(insertionLine.Offset + cleanLine.Length, ",");

		textEditor.InsertText(insertionLine.EndOffset, Environment.NewLine + languageScript);
		return insertionLine.LineNumber + 1;
	}

	private static int InsertLanguageScriptIntoEmptyTable(TextDocument document, string languageScript, DocumentLine stopLine)
	{
		document.Insert(stopLine.Offset, languageScript + Environment.NewLine);
		return stopLine.LineNumber;
	}

	private static int InsertLanguageScriptIntoEmptyTable(TextEditorBase textEditor, string languageScript, DocumentLine stopLine)
	{
		textEditor.InsertText(stopLine.Offset, languageScript + Environment.NewLine);
		return stopLine.LineNumber;
	}
}

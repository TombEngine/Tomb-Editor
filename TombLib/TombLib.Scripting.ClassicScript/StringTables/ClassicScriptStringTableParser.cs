using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TombLib.Scripting.ClassicScript.StringTables;

/// <summary>
/// Parses the ClassicScript INI-like string-table format without UI dependencies.
/// </summary>
public static class ClassicScriptStringTableParser
{
	/// <summary>
	/// Parses a source string into a string-table model.
	/// </summary>
	/// <param name="source">The source text to parse.</param>
	/// <param name="options">The explicit escape-decoding options.</param>
	/// <returns>A successful model or typed source diagnostics.</returns>
	public static ClassicScriptStringTableParseResult Parse(
		string? source,
		ClassicScriptStringTableParseOptions? options = null)
	{
		options ??= new ClassicScriptStringTableParseOptions();

		var sections = new List<ClassicScriptStringTableSection>();
		var diagnostics = new List<ClassicScriptStringTableDiagnostic>();
		var sectionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		ClassicScriptStringTableSection? currentSection = null;

		foreach ((string line, int lineNumber) in SplitLines(source ?? string.Empty))
		{
			string trimmedLine = line.Trim();

			if (trimmedLine.StartsWith("[", StringComparison.Ordinal))
			{
				if (!TryGetSectionName(trimmedLine, out string sectionName))
				{
					diagnostics.Add(CreateDiagnostic(
						ClassicScriptStringTableDiagnosticCode.MalformedSectionHeader,
						lineNumber,
						line.IndexOf('[', StringComparison.Ordinal) + 1,
						"The section header must contain one non-empty name enclosed in brackets."));
					continue;
				}

				if (!sectionNames.Add(sectionName))
				{
					diagnostics.Add(CreateDiagnostic(
						ClassicScriptStringTableDiagnosticCode.DuplicateSectionName,
						lineNumber,
							line.IndexOf('[', StringComparison.Ordinal) + 2,
						$"The section '{sectionName}' is declared more than once."));
					continue;
				}

				currentSection = new ClassicScriptStringTableSection(
					sectionName,
					sectionName.Equals("ExtraNG", StringComparison.OrdinalIgnoreCase),
					[]);
				sections.Add(currentSection);
				continue;
			}

			if (currentSection is null)
			{
				if (string.IsNullOrWhiteSpace(line) || trimmedLine.StartsWith(";", StringComparison.Ordinal))
					continue;

				diagnostics.Add(CreateDiagnostic(
					ClassicScriptStringTableDiagnosticCode.UnexpectedPreambleText,
					lineNumber,
					FirstContentColumn(line),
					"Only comments and blank lines are allowed before the first section."));
				continue;
			}

			string content = RemoveComment(line);
			if (string.IsNullOrWhiteSpace(content))
				continue;

			if (currentSection.IsExtraNg)
			{
				if (!TryParseExtraNgRow(content, options.ParsedNewline, out int id, out string value, out ClassicScriptStringTableDiagnosticCode code, out string message, out int column))
				{
					diagnostics.Add(CreateDiagnostic(code, lineNumber, column, message));
					continue;
				}

				currentSection = AddRow(sections, currentSection, new ClassicScriptStringTableRow(id, value));
			}
			else
			{
				currentSection = AddRow(
					sections,
					currentSection,
					new ClassicScriptStringTableRow(null, DecodeEscapes(content, options.ParsedNewline)));
			}
		}

		return diagnostics.Count == 0
			? new ClassicScriptStringTableParseResult(new ClassicScriptStringTable(sections), diagnostics)
			: new ClassicScriptStringTableParseResult(null, diagnostics);
	}

	private static ClassicScriptStringTableSection AddRow(
		List<ClassicScriptStringTableSection> sections,
		ClassicScriptStringTableSection section,
		ClassicScriptStringTableRow row)
	{
		var rows = section.Rows.ToList();
		rows.Add(row);
		var replacement = new ClassicScriptStringTableSection(section.Name, section.IsExtraNg, rows);
		int sectionIndex = sections.IndexOf(section);
		if (sectionIndex >= 0)
			sections[sectionIndex] = replacement;

		return replacement;
	}

	private static bool TryGetSectionName(string line, out string sectionName)
	{
		sectionName = string.Empty;

		if (line.Length < 3 || !line.EndsWith("]", StringComparison.Ordinal))
			return false;

		string name = line[1..^1].Trim();
		if (name.Length == 0 || name.Contains('[', StringComparison.Ordinal) || name.Contains(']', StringComparison.Ordinal))
			return false;

		sectionName = name;
		return true;
	}

	private static bool TryParseExtraNgRow(
		string content,
		string parsedNewline,
		out int id,
		out string value,
		out ClassicScriptStringTableDiagnosticCode code,
		out string message,
		out int column)
	{
		id = default;
		value = string.Empty;
		code = default;
		message = string.Empty;
		column = FirstContentColumn(content);

		int colonIndex = content.IndexOf(':');
		if (colonIndex <= 0)
		{
			code = ClassicScriptStringTableDiagnosticCode.MalformedExtraNgRow;
			message = "An ExtraNG row must have the form integer: value.";
			return false;
		}

		string idText = content[..colonIndex].Trim();
		if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
		{
			code = ClassicScriptStringTableDiagnosticCode.InvalidExtraNgId;
			message = "The ExtraNG row ID must be a valid integer.";
			column += content.IndexOf(idText, StringComparison.Ordinal);
			return false;
		}

		value = DecodeEscapes(content[(colonIndex + 1)..].TrimStart(), parsedNewline);
		return true;
	}

	private static string RemoveComment(string line)
	{
		int commentIndex = line.IndexOf(';');
		return commentIndex >= 0 ? line[..commentIndex].TrimEnd() : line;
	}

	private static string DecodeEscapes(string value, string parsedNewline)
		=> value.Replace("\\x3B", ";", StringComparison.Ordinal).Replace("\\n", parsedNewline, StringComparison.Ordinal);

	private static int FirstContentColumn(string line)
	{
		int index = 0;
		while (index < line.Length && char.IsWhiteSpace(line[index]))
			index++;

		return index + 1;
	}

	private static ClassicScriptStringTableDiagnostic CreateDiagnostic(
		ClassicScriptStringTableDiagnosticCode code,
		int line,
		int column,
		string message)
		=> new(code, line, Math.Max(1, column), message);

	private static IEnumerable<(string line, int lineNumber)> SplitLines(string source)
	{
		int lineStart = 0;
		int lineNumber = 1;

		for (int index = 0; index < source.Length; index++)
		{
			if (source[index] is not ('\r' or '\n'))
				continue;

			 yield return (source[lineStart..index], lineNumber++);

			if (source[index] == '\r' && index + 1 < source.Length && source[index + 1] == '\n')
				index++;

			lineStart = index + 1;
		}

		yield return (source[lineStart..], lineNumber);
	}
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

/// <summary>
/// Serializes the string-table editor's model into the canonical string-table document text.
/// </summary>
public static class ContentBuilder
{
	public static string BuildContent(IReadOnlyList<StringTableSection> sections)
	{
		var builder = new StringBuilder();

		builder.AppendLine($"; Automatically generated document using TombIDE {Application.ProductVersion}");
		builder.AppendLine($"; Do not add any comments into this document as they are");
		builder.AppendLine($"; going to be removed next time the file is regenerated.");

		foreach (StringTableSection section in sections)
		{
			bool isExtraNG = section.Mode == StringTableMode.ExtraNG;

			builder.AppendLine(Environment.NewLine + section.SectionName);

			foreach (StringTableRow row in section.Rows)
			{
				string line = GetParsedLine(row, isExtraNG);
				builder.AppendLine(line);
			}
		}

		return builder.ToString();
	}

	private static string GetParsedLine(StringTableRow row, bool isExtraNG = false)
	{
		string @string = GetParsedString(row.StringValue);

		if (isExtraNG)
			return $"{row.Id}: {@string}";
		else
			return @string;
	}

	private static string GetParsedString(string? @string)
		=> HandleNewLine(EscapeComments(@string));

	private static string EscapeComments(string? @string)
		=> @string?.Replace(";", "\\x3B") ?? string.Empty;

	private static string HandleNewLine(string? @string)
		=> @string?.Replace("\r", string.Empty).Replace("\n", "\\n") ?? string.Empty;
}

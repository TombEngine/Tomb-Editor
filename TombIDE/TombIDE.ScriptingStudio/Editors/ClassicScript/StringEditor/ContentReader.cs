#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public class ContentReader
{
	private static readonly Regex SectionHeaderPattern = new Regex(@"^\s*\[\b.*\b\]\s*(;.*)?$", RegexOptions.Compiled);

	public static bool NextSectionExists(string[] lines, int lineNumber, out int nextSectionLineNumber)
	{
		for (int i = lineNumber; i < lines.Length; i++)
		{
			if (IsSectionHeaderLine(lines[i]))
			{
				nextSectionLineNumber = i;
				return true;
			}
		}

		nextSectionLineNumber = -1;
		return false;
	}

	public static List<string> GetStrings(string[] lines, int sectionStartLineNumber)
	{
		var strings = new List<string>();

		for (int i = sectionStartLineNumber + 1; i < lines.Length; i++)
		{
			if (IsSectionHeaderLine(lines[i]))
				break;

			string line = GetParsedLine(lines[i]);

			if (!IsEmptyOrComments(line))
				strings.Add(line);
		}

		return strings;
	}

	public static string GetParsedLine(string line)
	{
		line = RemoveComments(line);
		line = line.Replace("\\x3B", ";");
		line = line.Replace("\\n", Environment.NewLine);

		return line;
	}

	private static bool IsSectionHeaderLine(string lineText)
		=> SectionHeaderPattern.IsMatch(lineText);

	private static bool IsEmptyOrComments(string lineText)
		=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(";", StringComparison.Ordinal);

	private static string RemoveComments(string lineText)
		=> Regex.Replace(lineText, @"\s*;.*$", string.Empty, RegexOptions.Multiline);

	public static string GetShortHex(short decimalValue, int size = 4)
	{
		string hexValue = decimalValue.ToString("X");

		int zerosToAdd = size - hexValue.Length;
		hexValue = $"${new string('0', zerosToAdd)}{hexValue}";

		return hexValue;
	}
}

using System;
using System.Text;
using Nickelony.IDEKit.Core.Formatting;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.UI.Extensions;

namespace TombLib.Scripting.ClassicScript.Cleaning;

/// <summary>
/// Formats ClassicScript document content by normalizing spacing around tokens.
/// </summary>
public sealed class ClassicScriptDocumentFormatter : ITextDocumentFormatter
{
	/// <summary>
	/// Gets or sets whether a space is inserted before the equals sign.
	/// </summary>
	public bool SpaceBeforeEquals { get; set; } = ConfigurationDefaults.SpaceBeforeEquals;

	/// <summary>
	/// Gets or sets whether a space is inserted after the equals sign.
	/// </summary>
	public bool SpaceAfterEquals { get; set; } = ConfigurationDefaults.SpaceAfterEquals;

	/// <summary>
	/// Gets or sets whether a space is inserted before the comma.
	/// </summary>
	public bool SpaceBeforeComma { get; set; } = ConfigurationDefaults.SpaceBeforeComma;

	/// <summary>
	/// Gets or sets whether a space is inserted after the comma.
	/// </summary>
	public bool SpaceAfterComma { get; set; } = ConfigurationDefaults.SpaceAfterComma;

	/// <summary>
	/// Gets or sets whether multiple spaces are collapsed.
	/// </summary>
	public bool CollapseMultipleSpaces { get; set; } = ConfigurationDefaults.CollapseMultipleSpaces;

	/// <summary>
	/// Formats the given editor content.
	/// </summary>
	/// <param name="editorContent">The content to format.</param>
	/// <returns>The formatted content.</returns>
	public string FormatDocument(string editorContent)
	{
		string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');

		for (int i = 0; i < lines.Length; i++)
			lines[i] = FormatLine(lines[i]);

		return string.Join(Environment.NewLine, lines.TrimTrailingWhitespaceOnLines());
	}

	/// <summary>
	/// Formats compiler output by removing spaces before equals signs.
	/// </summary>
	/// <param name="content">The compiler output to format.</param>
	/// <returns>The formatted output.</returns>
	public static string FormatCompilerOutput(string content)
	{
		string[] lines = content.Replace("\r", string.Empty).Split('\n');

		for (int i = 0; i < lines.Length; i++)
			lines[i] = RemoveSpacesBeforeEquals(lines[i]);

		return string.Join(Environment.NewLine, lines.TrimTrailingWhitespaceOnLines());
	}

	private string FormatLine(string line)
	{
		var builder = new StringBuilder(line.Length);

		for (int index = 0; index < line.Length; index++)
		{
			char currentChar = line[index];

			if (currentChar is '=' or ',')
			{
				ApplySpacingBeforeToken(builder, currentChar);
				builder.Append(currentChar);
				index = SkipSpacesAfterToken(line, index);
				ApplySpacingAfterToken(builder, currentChar, line, index + 1);
				continue;
			}

			if (CollapseMultipleSpaces && currentChar == ' ' && builder.Length > 0 && builder[builder.Length - 1] == ' ')
				continue;

			builder.Append(currentChar);
		}

		return builder.ToString();
	}

	private void ApplySpacingBeforeToken(StringBuilder builder, char token)
	{
		while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
			builder.Length--;

		bool needsSpace = token == '=' ? SpaceBeforeEquals : SpaceBeforeComma;

		if (needsSpace && builder.Length > 0)
			builder.Append(' ');
	}

	private void ApplySpacingAfterToken(StringBuilder builder, char token, string line, int nextIndex)
	{
		bool needsSpace = token == '=' ? SpaceAfterEquals : SpaceAfterComma;

		if (!needsSpace || nextIndex >= line.Length)
			return;

		builder.Append(' ');
	}

	private static int SkipSpacesAfterToken(string line, int index)
	{
		while (index + 1 < line.Length && line[index + 1] == ' ')
			index++;

		return index;
	}

	private static string RemoveSpacesBeforeEquals(string line)
	{
		var builder = new StringBuilder(line.Length);

		for (int index = 0; index < line.Length; index++)
		{
			char currentChar = line[index];

			if (currentChar == '=')
			{
				while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
					builder.Length--;
			}

			builder.Append(currentChar);
		}

		return builder.ToString();
	}
}

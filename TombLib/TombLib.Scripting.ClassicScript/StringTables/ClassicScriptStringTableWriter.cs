using System;
using System.Collections.Generic;
using System.Text;

namespace TombLib.Scripting.ClassicScript.StringTables;

/// <summary>
/// Writes ClassicScript string-table models as normalized source text.
/// </summary>
public static class ClassicScriptStringTableWriter
{
	/// <summary>
	/// Serializes a model using the requested newline style and optional generated header.
	/// </summary>
	/// <param name="table">The table to serialize.</param>
	/// <param name="options">The explicit output options.</param>
	/// <returns>The normalized serialized source.</returns>
	public static string Write(
		ClassicScriptStringTable table,
		ClassicScriptStringTableWriteOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(table);
		options ??= new ClassicScriptStringTableWriteOptions();

		string newline = options.NewlineStyle switch
		{
			ClassicScriptStringTableNewlineStyle.CrLf => "\r\n",
			ClassicScriptStringTableNewlineStyle.Lf => "\n",
			ClassicScriptStringTableNewlineStyle.Cr => "\r",
			_ => throw new ArgumentOutOfRangeException(nameof(options))
		};

		var lines = new List<string>();
		if (options.GeneratedHeader is not null)
		{
			lines.Add($"; Automatically generated document using {options.GeneratedHeader.GeneratorName} {options.GeneratedHeader.GeneratorVersion}");
			lines.Add("; Do not add any comments into this document as they are");
			lines.Add("; going to be removed next time the file is regenerated.");
		}

		foreach (ClassicScriptStringTableSection section in table.Sections)
		{
			if (lines.Count > 0)
				lines.Add(string.Empty);

			lines.Add($"[{section.Name}]");
			foreach (ClassicScriptStringTableRow row in section.Rows)
			{
				string value = EscapeValue(row.Value);
				lines.Add(section.IsExtraNg
					? $"{row.Id?.ToString() ?? throw new InvalidOperationException("ExtraNG rows require an integer ID.")}: {value}"
					: value);
			}
		}

		return lines.Count == 0 ? string.Empty : string.Join(newline, lines) + newline;
	}

	private static string EscapeValue(string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		return value
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace('\r', '\n')
			.Replace("\n", "\\n", StringComparison.Ordinal)
			.Replace(";", "\\x3B", StringComparison.Ordinal);
	}
}

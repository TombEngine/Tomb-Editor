using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.ClassicScript.ReferenceTables;

/// <summary>
/// Loads a reference table from its JSON resource.
/// </summary>
public sealed class ClassicScriptReferenceTableLoader
{
	/// <summary>
	/// Loads the reference table at the given path.
	/// </summary>
	/// <param name="jsonPath">The path of the reference table JSON file.</param>
	/// <returns>The loaded table.</returns>
	public DataTable Load(string jsonPath)
	{
		using JsonDocument document = JsonDocument.Parse(File.ReadAllText(jsonPath));

		if (document.RootElement.ValueKind != JsonValueKind.Object ||
			!document.RootElement.TryGetProperty("Columns", out JsonElement columnsElement) ||
			!document.RootElement.TryGetProperty("Rows", out JsonElement rowsElement))
		{
			return new DataTable();
		}

		var table = new DataTable();
		var columnNames = new List<string>();

		foreach (JsonElement columnElement in columnsElement.EnumerateArray())
		{
			string? columnName = columnElement.GetString();

			if (string.IsNullOrEmpty(columnName))
				continue;

			columnNames.Add(columnName);
			table.Columns.Add(columnName, typeof(string));
		}

		foreach (JsonElement rowElement in rowsElement.EnumerateArray())
		{
			var values = new List<string>();

			foreach (JsonElement valueElement in rowElement.EnumerateArray())
			{
				string value = valueElement.ValueKind == JsonValueKind.String
					? valueElement.GetString() ?? string.Empty
					: string.Empty;

				values.Add(value);
			}

			while (values.Count < columnNames.Count)
				values.Add(string.Empty);

			table.Rows.Add(values.ToArray());
		}

		return table;
	}
}

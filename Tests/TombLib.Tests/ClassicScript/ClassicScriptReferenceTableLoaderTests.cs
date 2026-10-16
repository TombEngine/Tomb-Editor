using System.Data;
using System.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.ReferenceTables;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptReferenceTableLoaderTests
{
	private readonly ClassicScriptReferenceTableLoader _loader = new();

	[TestMethod]
	public void Load_ReadsColumnsRowsAndValues()
	{
		string filePath = CreateTemporaryJson(
			("decimal", "hex", "flag"),
			["3", "$0003", "ADD_BLOOD"],
			["1", "$0001", "ADD_FLAME"]);

		try
		{
			DataTable table = _loader.Load(filePath);

			Assert.AreEqual(3, table.Columns.Count);
			Assert.AreEqual("decimal", table.Columns[0].ColumnName);
			Assert.AreEqual("hex", table.Columns[1].ColumnName);
			Assert.AreEqual("flag", table.Columns[2].ColumnName);
			Assert.AreEqual(typeof(string), table.Columns[0].DataType);

			Assert.AreEqual(2, table.Rows.Count);
			Assert.AreEqual("3", table.Rows[0]["decimal"]);
			Assert.AreEqual("$0003", table.Rows[0]["hex"]);
			Assert.AreEqual("ADD_BLOOD", table.Rows[0]["flag"]);
			Assert.AreEqual("ADD_FLAME", table.Rows[1]["flag"]);
		}
		finally
		{
			DeleteFile(filePath);
		}
	}

	[TestMethod]
	public void Load_ReadsAllRealReferenceTables()
	{
		(string tableName, int expectedRows)[] tables =
		[
			("OCBList", 70),
			("MnemonicConstants", 1030),
			("KeyboardScancodes", 85),
			("SoundIndices", 370),
			("MoveableSlotIndices", 521),
			("StaticObjectIndices", 160),
			("EnemyDamageValues", 61),
			("VariablePlaceholders", 164)
		];

		foreach ((string tableName, int expectedRows) in tables)
		{
			DataTable table = _loader.Load(ClassicScriptResourcePaths.GetResourcePath(tableName + ".json"));

			Assert.IsTrue(table.Rows.Count > 0, $"{tableName} should contain rows.");
			Assert.AreEqual(expectedRows, table.Rows.Count, $"{tableName} row count mismatch.");
		}
	}

	[TestMethod]
	public void Load_EnemyDamageValues_PreservesEmptyArgumentCells()
	{
		DataTable table = _loader.Load(ClassicScriptResourcePaths.GetResourcePath("EnemyDamageValues.json"));

		Assert.AreEqual(5, table.Columns.Count);
		Assert.AreEqual("decimal", table.Columns[0].ColumnName);
		Assert.AreEqual("flag", table.Columns[1].ColumnName);
		Assert.AreEqual("argument1", table.Columns[2].ColumnName);
		Assert.AreEqual("argument2", table.Columns[3].ColumnName);
		Assert.AreEqual("argument3", table.Columns[4].ColumnName);

		// JEEP (row 0) has an empty argument2 and argument3.
		Assert.AreEqual("JEEP", table.Rows[0]["flag"]);
		Assert.AreEqual("-1000 to +1000 (Default: 150) <Collision shock>", table.Rows[0]["argument1"]);
		Assert.AreEqual(string.Empty, table.Rows[0]["argument2"]);
		Assert.AreEqual(string.Empty, table.Rows[0]["argument3"]);
	}

	[TestMethod]
	public void Load_OCBList_ReadsNameColumn()
	{
		DataTable table = _loader.Load(ClassicScriptResourcePaths.GetResourcePath("OCBList.json"));

		Assert.AreEqual(1, table.Columns.Count);
		Assert.AreEqual("Name", table.Columns[0].ColumnName);
		Assert.AreEqual("_NEW Animating", table.Rows[0]["Name"]);
	}

	[TestMethod]
	public void Load_ObjectWithoutColumnsOrRows_ReturnsEmptyTable()
	{
		string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
		File.WriteAllText(filePath, "{ }");

		try
		{
			DataTable table = _loader.Load(filePath);

			Assert.AreEqual(0, table.Columns.Count);
			Assert.AreEqual(0, table.Rows.Count);
		}
		finally
		{
			DeleteFile(filePath);
		}
	}

	[TestMethod]
	public void Load_PadsShortRowsWithEmptyStrings()
	{
		string filePath = CreateTemporaryJson(
			("decimal", "hex", "flag"),
			["3", "$0003"]);

		try
		{
			DataTable table = _loader.Load(filePath);

			Assert.AreEqual(1, table.Rows.Count);
			Assert.AreEqual("3", table.Rows[0]["decimal"]);
			Assert.AreEqual("$0003", table.Rows[0]["hex"]);
			Assert.AreEqual(string.Empty, table.Rows[0]["flag"]);
		}
		finally
		{
			DeleteFile(filePath);
		}
	}

	private static string CreateTemporaryJson((string Column1, string Column2, string Column3) columns, params string[][] rows)
	{
		string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
		var builder = new StringBuilder();

		builder.AppendLine("{");
		builder.AppendLine("\t\"Columns\": [");
		builder.AppendLine($"\t\t\"{columns.Column1}\",");
		builder.AppendLine($"\t\t\"{columns.Column2}\",");
		builder.AppendLine($"\t\t\"{columns.Column3}\"");
		builder.AppendLine("\t],");
		builder.AppendLine("\t\"Rows\": [");

		for (int i = 0; i < rows.Length; i++)
		{
			builder.Append("\t\t[");
			builder.Append(string.Join(", ", rows[i].Select(value => "\"" + value + "\"")));
			builder.Append(']');
			builder.AppendLine(i < rows.Length - 1 ? "," : string.Empty);
		}

		builder.AppendLine("\t]");
		builder.AppendLine("}");
		File.WriteAllText(filePath, builder.ToString());
		return filePath;
	}

	private static void DeleteFile(string filePath)
	{
		if (File.Exists(filePath))
			File.Delete(filePath);
	}
}

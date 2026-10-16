using System.Text;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Mnemonics.Models;
using TombLib.Scripting.ClassicScript.Mnemonics.Services;

namespace TombLib.Tests;

[TestClass]
public class MnemonicDefinitionsLoaderTests
{
	private readonly MnemonicDefinitionsLoader _loader = new();

	[TestMethod]
	public void Load_ReadsStandardConstantsFromRealJson()
	{
		MnemonicDefinitions definitions = LoadReal();

		Assert.AreEqual(1030, definitions.StandardConstants.Count);

		MnemonicConstantDefinition first = definitions.StandardConstants[0];
		Assert.AreEqual("3", first.DecimalValue);
		Assert.AreEqual("$0003", first.HexValue);
		Assert.AreEqual("ADD_BLOOD", first.FlagName);
	}

	[TestMethod]
	public void Load_ReadsHexAndDecimalValues()
	{
		MnemonicDefinitions definitions = LoadReal();

		MnemonicConstantDefinition ammoAddGunShell = definitions.StandardConstants
			.First(definition => definition.FlagName == "AMMO_ADD_GUN_SHELL");

		Assert.AreEqual("16", ammoAddGunShell.DecimalValue);
		Assert.AreEqual("$0010", ammoAddGunShell.HexValue);
	}

	[TestMethod]
	public void Load_MissingFile_ReturnsEmptyStandardConstants()
	{
		string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

		MnemonicDefinitions definitions = _loader.Load(missingPath, Path.GetTempPath());

		Assert.AreEqual(0, definitions.StandardConstants.Count);
	}

	[TestMethod]
	public void Load_FiltersEmptyFlagNames()
	{
		string filePath = CreateTemporaryJson(
			["3", "$0003", "ADD_BLOOD"],
			["1", "$0001", ""],
			["2", "$0002", "ADD_SMOKE"]);

		try
		{
			MnemonicDefinitions definitions = _loader.Load(filePath, Path.GetTempPath());

			Assert.AreEqual(2, definitions.StandardConstants.Count);
			Assert.AreEqual("ADD_BLOOD", definitions.StandardConstants[0].FlagName);
			Assert.AreEqual("ADD_SMOKE", definitions.StandardConstants[1].FlagName);
		}
		finally
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}
	}

	[TestMethod]
	public void Load_MalformedJson_ReturnsEmptyStandardConstants()
	{
		string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
		File.WriteAllText(filePath, "not valid json {");

		try
		{
			MnemonicDefinitions definitions = _loader.Load(filePath, Path.GetTempPath());

			Assert.AreEqual(0, definitions.StandardConstants.Count);
		}
		finally
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}
	}

	private static MnemonicDefinitions LoadReal()
	{
		return new MnemonicDefinitionsLoader().Load(
			ClassicScriptResourcePaths.GetMnemonicConstantsPath(),
			Path.GetTempPath());
	}

	private static string CreateTemporaryJson(params string[][] rows)
	{
		string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
		var builder = new StringBuilder();

		builder.AppendLine("{");
		builder.AppendLine("\t\"Columns\": [");
		builder.AppendLine("\t\t\"decimal\",");
		builder.AppendLine("\t\t\"hex\",");
		builder.AppendLine("\t\t\"flag\"");
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
}

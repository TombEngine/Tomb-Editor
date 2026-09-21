using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombLib.Tests;

[TestClass]
public class ScriptGeneratorTests
{
	[TestMethod]
	public void HasOutput_WhenOnlyFilesAreGenerated_ReturnsTrue()
	{
		var result = new ScriptGenerationResult("Level01")
		{
			FilesToCreate =
			[
				new GeneratedScriptFile("Levels\\Level01.lua", "-- test")
			]
		};

		Assert.IsFalse(result.HasContent);
		Assert.IsTrue(result.HasOutput);
	}

	[TestMethod]
	public void GenerateScripts_ForTombEngine_IncludesLevelLuaFile()
	{
		var result = ScriptGenerator.GenerateScripts("My Level", "MyLevel", TRVersion.Game.TombEngine, 5, true);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.HasContent);
		Assert.IsTrue(result.HasOutput);
		Assert.AreEqual(1, result.FilesToCreate.Count);
		Assert.AreEqual(Path.Combine("Levels", "MyLevel.lua"), result.FilesToCreate[0].RelativePath);
		StringAssert.Contains(result.GameFlowScript, "Scripts\\\\Levels\\\\MyLevel.lua");
		StringAssert.Contains(result.LanguageScript, "MyLevel = { \"My Level\" }");
		StringAssert.Contains(result.FilesToCreate[0].Content, "LevelFuncs.OnLoad = function() end");
	}
}

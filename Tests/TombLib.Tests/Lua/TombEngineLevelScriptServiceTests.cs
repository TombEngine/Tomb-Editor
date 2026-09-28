using System;
using TombLib.Scripting.Lua.Documents;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public class TombEngineLevelScriptServiceTests
{
	private readonly TombEngineLevelScriptService _service = new();

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsTrueWhenLanguageEntryAndFlowRegistrationMatch()
	{
		var scriptDocument = CreateDocument(
			"LevelOne = TEN.Flow.Level()",
			"LevelOne.nameKey = \"LevelOne\"",
			"TEN.Flow.AddLevel(LevelOne)");
		var languageDocument = CreateDocument("LevelOne = { \"First Level\" }");

		bool result = _service.IsLevelScriptDefined(scriptDocument, languageDocument, "First Level");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsFalseWhenLanguageEntryIsMissing()
	{
		var scriptDocument = CreateDocument("TEN.Flow.AddLevel(LevelOne)");
		var languageDocument = CreateDocument("OtherLevel = { \"Other Level\" }");

		bool result = _service.IsLevelScriptDefined(scriptDocument, languageDocument, "First Level");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsFalseWhenFlowRegistrationIsMissing()
	{
		var scriptDocument = CreateDocument("LevelOne = TEN.Flow.Level()");
		var languageDocument = CreateDocument("LevelOne = { \"First Level\" }");

		bool result = _service.IsLevelScriptDefined(scriptDocument, languageDocument, "First Level");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLevelScriptDefined_IgnoresCommentedOutEntries()
	{
		var scriptDocument = CreateDocument(
			"-- TEN.Flow.AddLevel(LevelOne)",
			"TEN.Flow.AddLevel(LevelTwo)");
		var languageDocument = CreateDocument(
			"-- LevelOne = { \"First Level\" }",
			"LevelTwo = { \"Second Level\" }");

		bool firstResult = _service.IsLevelScriptDefined(scriptDocument, languageDocument, "First Level");
		bool secondResult = _service.IsLevelScriptDefined(scriptDocument, languageDocument, "Second Level");

		Assert.IsFalse(firstResult);
		Assert.IsTrue(secondResult);
	}

	private static ITextSnapshot CreateDocument(params string[] lines)
		=> new StringTextSnapshot(string.Join(Environment.NewLine, lines));
}
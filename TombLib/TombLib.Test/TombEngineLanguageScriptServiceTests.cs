using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua.Services;

namespace TombLib.Test;

[TestClass]
public class TombEngineLanguageScriptServiceTests
{
	private readonly TombEngineLanguageScriptService _service = new();

	[TestMethod]
	public void TryInsertLanguageScript_InsertsAfterExistingEntry()
	{
		var document = CreateDocument(
			"local strings = {",
			"    existing = { \"Existing\" }",
			"}",
			string.Empty,
			"TEN.Flow.SetStrings(strings)");

		int? insertedLineNumber = _service.TryInsertLanguageScript(document, "    newLevel = { \"New Level\" }");

		Assert.AreEqual(3, insertedLineNumber);
		StringAssert.Contains(document.Text, "existing = { \"Existing\" },");
		StringAssert.Contains(document.Text, "newLevel = { \"New Level\" }");
	}

	[TestMethod]
	public void TryInsertLanguageScript_InsertsIntoEmptyTable()
	{
		var document = CreateDocument(
			"local strings = {",
			"}",
			string.Empty,
			"TEN.Flow.SetStrings(strings)");

		int? insertedLineNumber = _service.TryInsertLanguageScript(document, "    newLevel = { \"New Level\" }");

		Assert.AreEqual(2, insertedLineNumber);
		StringAssert.Contains(document.Text, "local strings = {" + Environment.NewLine + "    newLevel = { \"New Level\" }" + Environment.NewLine + "}");
	}

	[TestMethod]
	public void TryInsertLanguageScript_IgnoresQuotedBracesAndCommentMarkers()
	{
		var document = CreateDocument(
			"local strings = {",
			"    existing = { \"A } brace and -- comment marker\" }",
			"}",
			string.Empty,
			"TEN.Flow.SetStrings(strings)");

		int? insertedLineNumber = _service.TryInsertLanguageScript(document, "    newLevel = { \"New Level\" }");

		Assert.AreEqual(3, insertedLineNumber);
		StringAssert.Contains(document.Text, "existing = { \"A } brace and -- comment marker\" },");
		StringAssert.Contains(document.Text, "newLevel = { \"New Level\" }");
	}

	[TestMethod]
	public void TryInsertLanguageScript_PlacesCommaBeforeTrailingComment()
	{
		var document = CreateDocument(
			"local strings = {",
			"    existing = { \"Existing\" } -- note",
			"}",
			string.Empty,
			"TEN.Flow.SetStrings(strings)");

		int? insertedLineNumber = _service.TryInsertLanguageScript(document, "    newLevel = { \"New Level\" }");

		Assert.AreEqual(3, insertedLineNumber);
		StringAssert.Contains(document.Text, "existing = { \"Existing\" }, -- note");
		StringAssert.Contains(document.Text, "newLevel = { \"New Level\" }");
	}

	[TestMethod]
	public void TryInsertLanguageScript_ReturnsNullWhenStringsTableIsMissing()
	{
		var document = CreateDocument(
			"local other = {}",
			string.Empty,
			"TEN.Flow.SetStrings(strings)");

		int? insertedLineNumber = _service.TryInsertLanguageScript(document, "    newLevel = { \"New Level\" }");

		Assert.IsNull(insertedLineNumber);
		Assert.IsFalse(document.Text.Contains("newLevel"));
	}

	private static TextDocument CreateDocument(params string[] lines)
		=> new(string.Join(Environment.NewLine, lines));
}

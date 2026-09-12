using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Indentation;
using TombLib.Scripting.Lua.Editing;

namespace TombLib.Tests;

[TestClass]
public class LuaIndentationStrategyTests
{
	[TestMethod]
	public void IndentLine_AfterThen_AddsIndentToNewLine()
	{
		var strategy = new LuaAutoIndentationStrategy(new TextEditorOptions
		{
			ConvertTabsToSpaces = true,
			IndentationSize = 4
		});

		var document = new TextDocument("if value then\r\n");

		strategy.IndentLine(document, document.GetLineByNumber(2));

		Assert.AreEqual("if value then\r\n    ", document.Text);
	}

	[TestMethod]
	public void IndentLine_BeforeEnd_DedentsCurrentLineWithoutExtraInsertion()
	{
		var strategy = new LuaAutoIndentationStrategy(new TextEditorOptions
		{
			ConvertTabsToSpaces = true,
			IndentationSize = 4
		});

		var document = new TextDocument("if value then\r\n end");

		strategy.IndentLine(document, document.GetLineByNumber(2));

		Assert.AreEqual("if value then\r\nend", document.Text);
	}

	[TestMethod]
	public void NormalizeCompletionInsertion_DedentsEndLineAndPreservesCaretAndCrLf()
	{
		CompletionInsertionResult result = LuaIndentationStrategy.Instance.NormalizeCompletionInsertion(new CompletionInsertionContext(
			"if condition then\r\n\t\r\nend",
			"if condition then\r\n\t".Length,
			"    ",
			"    "));

		Assert.AreEqual("if condition then\r\n        \r\n    end", result.Text);
		Assert.AreEqual("if condition then\r\n        ".Length, result.CaretOffset);
	}

	[TestMethod]
	public void BuildEnterInsertion_BeforeDedent_SplitsLineAndRemovesExistingIndentation()
	{
		EnterInsertionResult result = LuaIndentationStrategy.Instance.BuildEnterInsertion(new EnterInsertionContext(
			"if condition then",
			"    end",
			string.Empty,
			"    ",
			"\r\n",
			UseSmartIndent: true));

		Assert.AreEqual("\r\n    \r\n", result.Text);
		Assert.AreEqual("\r\n    ".Length, result.CaretOffset);
		Assert.AreEqual(4, result.RemoveFollowingWhitespaceLength);
	}

	[TestMethod]
	public void BuildEnterInsertion_WithoutDedentSplit_InsertsIndentedNewLineOnly()
	{
		EnterInsertionResult result = LuaIndentationStrategy.Instance.BuildEnterInsertion(new EnterInsertionContext(
			"if condition then",
			"value = 1",
			string.Empty,
			"    ",
			"\r\n",
			UseSmartIndent: true));

		Assert.AreEqual("\r\n    ", result.Text);
		Assert.AreEqual("\r\n    ".Length, result.CaretOffset);
		Assert.AreEqual(0, result.RemoveFollowingWhitespaceLength);
	}
}

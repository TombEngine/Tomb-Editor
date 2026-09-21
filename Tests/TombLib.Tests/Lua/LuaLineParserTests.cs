using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Tests;

[TestClass]
public class LuaLineParserTests
{
	[TestMethod]
	public void IsInsideCommentOrString_ReturnsTrueInsideLongComment()
	{
		bool result = LuaLineParser.IsInsideCommentOrString("--[[ comment");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsInsideCommentOrString_ReturnsTrueInsideLongString()
	{
		bool result = LuaLineParser.IsInsideCommentOrString("value = [[comment");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void StripLineComment_RemovesInlineLongCommentAndKeepsCodeAfterIt()
	{
		string result = LuaLineParser.StripLineComment("value = 1 --[[ remove this ]] + 2");
		Assert.AreEqual("value = 1  + 2", result);
	}

	[TestMethod]
	public void EnumerateStructuralCharacters_SkipsLongStringContents()
	{
		string result = new(LuaLineParser.EnumerateStructuralCharacters("{ [[ignored } text]] }").ToArray());
		Assert.AreEqual("{  }", result);
	}

	[TestMethod]
	public void ExtractCodeText_RemovesQuotedAndCommentText()
	{
		string result = LuaLineParser.ExtractCodeText("if value == \"then\" then -- comment");
		Assert.AreEqual("if value ==  then ", result);
	}

	[TestMethod]
	public void IsInsideCommentOrString_LongCommentContinuation_TracksStateAcrossLines()
	{
		bool inside = LuaLineParser.IsInsideCommentOrString("--[[ comment", default, out LuaLineParserState state);

		Assert.IsTrue(inside);
		Assert.AreEqual(LuaLineParserStateKind.LongComment, state.Kind);

		// A following line is still inside the long comment until the closing bracket.
		Assert.IsTrue(LuaLineParser.IsInsideCommentOrString("still commented", state, out LuaLineParserState stillInside));
		Assert.AreEqual(LuaLineParserStateKind.LongComment, stillInside.Kind);

		Assert.IsFalse(LuaLineParser.IsInsideCommentOrString("]] end", stillInside, out LuaLineParserState closed));
		Assert.AreEqual(LuaLineParserStateKind.None, closed.Kind);
	}

	[TestMethod]
	public void IsInsideCommentOrString_LongStringContinuation_PreservesDelimiterEqualsCount()
	{
		bool inside = LuaLineParser.IsInsideCommentOrString("value = [=[", default, out LuaLineParserState state);

		Assert.IsTrue(inside);
		Assert.AreEqual(LuaLineParserStateKind.LongString, state.Kind);
		Assert.AreEqual(1, state.LongBracketEqualsCount);

		// The closing delimiter must match the opening equals count, so a plain "]]" does not
		// close a "[=[" long string and the parser stays inside.
		Assert.IsTrue(LuaLineParser.IsInsideCommentOrString("]]", state, out LuaLineParserState notClosed));
		Assert.AreEqual(LuaLineParserStateKind.LongString, notClosed.Kind);

		// The matching "]=]" closes the long string.
		Assert.IsFalse(LuaLineParser.IsInsideCommentOrString("]=]", state, out LuaLineParserState closed));
		Assert.AreEqual(LuaLineParserStateKind.None, closed.Kind);
	}

	[TestMethod]
	public void EnumerateStructuralCharacters_LongStringContinuation_SkipsContentsAcrossLines()
	{
		LuaLineParserState captured = default;

		string firstLine = new(LuaLineParser.EnumerateStructuralCharacters("local s = [[", default, captureFinalState: state => captured = state).ToArray());
		Assert.AreEqual("local s = ", firstLine);
		Assert.AreEqual(LuaLineParserStateKind.LongString, captured.Kind);

		string middleLine = new(LuaLineParser.EnumerateStructuralCharacters("{ ignored }", captured, captureFinalState: state => captured = state).ToArray());
		Assert.AreEqual(string.Empty, middleLine);
		Assert.AreEqual(LuaLineParserStateKind.LongString, captured.Kind);

		string lastLine = new(LuaLineParser.EnumerateStructuralCharacters("]] end", captured, captureFinalState: state => captured = state).ToArray());
		Assert.AreEqual(" end", lastLine);
		Assert.AreEqual(LuaLineParserStateKind.None, captured.Kind);
	}
}

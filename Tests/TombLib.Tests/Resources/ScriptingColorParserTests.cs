using System.Windows.Media;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Tests;

// Phase 1 color-hardening tests: prove the shared color parser never throws on malformed
// user-edited color data and falls back to safe editor colors.
[TestClass]
public class ScriptingColorParserTests
{
	[TestMethod]
	public void TryParseColor_ValidHexColor_ReturnsTrue()
	{
		bool success = ScriptingColorParser.TryParseColor("#FFAABB", out Color color);

		Assert.IsTrue(success);
		Assert.AreEqual(Color.FromRgb(0xFF, 0xAA, 0xBB), color);
	}

	[TestMethod]
	public void TryParseColor_ValidNamedColor_ReturnsTrue()
	{
		bool success = ScriptingColorParser.TryParseColor("Gainsboro", out Color color);

		Assert.IsTrue(success);
		Assert.AreEqual(Colors.Gainsboro, color);
	}

	[TestMethod]
	public void TryParseColor_EmptyValue_ReturnsFalse()
	{
		bool success = ScriptingColorParser.TryParseColor(string.Empty, out Color color);

		Assert.IsFalse(success);
		Assert.AreEqual(default, color);
	}

	[TestMethod]
	public void TryParseColor_WhitespaceValue_ReturnsFalse()
	{
		Assert.IsFalse(ScriptingColorParser.TryParseColor("   ", out _));
	}

	[TestMethod]
	public void TryParseColor_MalformedValue_ReturnsFalse()
	{
		Assert.IsFalse(ScriptingColorParser.TryParseColor("not-a-color", out _));
	}

	[TestMethod]
	public void ParseColorOrDefault_MalformedBackground_FallsBackToBlack()
	{
		Color color = ScriptingColorParser.ParseColorOrDefault("not-a-color", ScriptingColorParser.DefaultBackgroundColor);

		Assert.AreEqual(ScriptingColorParser.DefaultBackgroundColor, color);
	}

	[TestMethod]
	public void ParseColorOrDefault_MalformedForeground_FallsBackToWhite()
	{
		Color color = ScriptingColorParser.ParseColorOrDefault("not-a-color", ScriptingColorParser.DefaultForegroundColor);

		Assert.AreEqual(ScriptingColorParser.DefaultForegroundColor, color);
	}

	[TestMethod]
	public void ParseColorOrDefault_MalformedHighlighting_FallsBackToWhite()
	{
		Color color = ScriptingColorParser.ParseColorOrDefault("not-a-color", ScriptingColorParser.DefaultHighlightingColor);

		Assert.AreEqual(ScriptingColorParser.DefaultHighlightingColor, color);
	}

	[TestMethod]
	public void ParseColorOrDefault_ValidValue_IgnoresFallback()
	{
		Color color = ScriptingColorParser.ParseColorOrDefault("#123456", ScriptingColorParser.DefaultForegroundColor);

		Assert.AreEqual(Color.FromRgb(0x12, 0x34, 0x56), color);
	}

	[TestMethod]
	public void CreateBrush_MalformedValue_ReturnsFrozenFallbackBrush()
	{
		SolidColorBrush brush = ScriptingColorParser.CreateBrush("not-a-color", ScriptingColorParser.DefaultForegroundColor);

		Assert.IsTrue(brush.IsFrozen);
		Assert.AreEqual(ScriptingColorParser.DefaultForegroundColor, brush.Color);
	}
}

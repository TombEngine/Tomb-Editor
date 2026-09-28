using ClassicColorScheme = TombLib.Scripting.ClassicScript.Highlighting.ColorScheme;
using GameFlowColorScheme = TombLib.Scripting.GameFlowScript.Highlighting.ColorScheme;
using TombLib.Scripting.UI.Highlighting;
using TrxColorScheme = TombLib.Scripting.TRX.Highlighting.ColorScheme;

namespace TombLib.Tests;

[TestClass]
public class ColorSchemeEqualityTests
{
	[TestMethod]
	public void ClassicScriptColorScheme_EqualSchemes_AreEqualAndHashEqual()
	{
		var first = new ClassicColorScheme();
		var second = new ClassicColorScheme();

		Assert.IsTrue(first == second);
		Assert.IsFalse(first != second);
		Assert.AreEqual(first, second);
		Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
	}

	[TestMethod]
	public void ClassicScriptColorScheme_DifferentSchemes_AreNotEqual()
	{
		var first = new ClassicColorScheme();
		var second = new ClassicColorScheme
		{
			Sections = new HighlightingObject { HtmlColor = "Red" }
		};

		Assert.IsFalse(first == second);
		Assert.IsTrue(first != second);
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void ClassicScriptColorScheme_NullComparisons_AreNullSafe()
	{
		var scheme = new ClassicColorScheme();
		ClassicColorScheme? nullScheme = null;

		Assert.IsFalse(scheme == null);
		Assert.IsTrue(scheme != null);
		Assert.IsTrue(nullScheme == null);
		Assert.IsTrue(nullScheme == nullScheme);
		Assert.IsFalse(scheme.Equals(null));
	}

	[TestMethod]
	public void GameFlowColorScheme_EqualSchemes_AreEqualAndHashEqual()
	{
		var first = new GameFlowColorScheme();
		var second = new GameFlowColorScheme();

		Assert.IsTrue(first == second);
		Assert.IsFalse(first != second);
		Assert.AreEqual(first, second);
		Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
	}

	[TestMethod]
	public void GameFlowColorScheme_DifferentSchemes_AreNotEqual()
	{
		var first = new GameFlowColorScheme();
		var second = new GameFlowColorScheme
		{
			Comments = new HighlightingObject { IsBold = true }
		};

		Assert.IsFalse(first == second);
		Assert.IsTrue(first != second);
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void GameFlowColorScheme_NullComparisons_AreNullSafe()
	{
		var scheme = new GameFlowColorScheme();
		GameFlowColorScheme? nullScheme = null;

		Assert.IsFalse(scheme == null);
		Assert.IsTrue(scheme != null);
		Assert.IsTrue(nullScheme == null);
		Assert.IsTrue(nullScheme == nullScheme);
		Assert.IsFalse(scheme.Equals(null));
	}

	[TestMethod]
	public void TrxColorScheme_EqualSchemes_AreEqualAndHashEqual()
	{
		var first = new TrxColorScheme();
		var second = new TrxColorScheme();

		Assert.IsTrue(first == second);
		Assert.IsFalse(first != second);
		Assert.AreEqual(first, second);
		Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
	}

	[TestMethod]
	public void TrxColorScheme_DifferentSchemes_AreNotEqual()
	{
		var first = new TrxColorScheme();
		var second = new TrxColorScheme
		{
			Constants = new HighlightingObject { HtmlColor = "Orchid" }
		};

		Assert.IsFalse(first == second);
		Assert.IsTrue(first != second);
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void TrxColorScheme_NullComparisons_AreNullSafe()
	{
		var scheme = new TrxColorScheme();
		TrxColorScheme? nullScheme = null;

		Assert.IsFalse(scheme == null);
		Assert.IsTrue(scheme != null);
		Assert.IsTrue(nullScheme == null);
		Assert.IsTrue(nullScheme == nullScheme);
		Assert.IsFalse(scheme.Equals(null));
	}

	[TestMethod]
	public void HighlightingObject_EqualObjects_AreEqualAndHashEqual()
	{
		var first = new HighlightingObject { HtmlColor = "Red", IsBold = true, IsItalic = false };
		var second = new HighlightingObject { HtmlColor = "RED", IsBold = true, IsItalic = false };

		Assert.IsTrue(first == second);
		Assert.IsFalse(first != second);
		Assert.AreEqual(first, second);
		Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
	}

	[TestMethod]
	public void HighlightingObject_DifferentObjects_AreNotEqual()
	{
		var first = new HighlightingObject { HtmlColor = "Red" };
		var second = new HighlightingObject { HtmlColor = "Blue" };

		Assert.IsFalse(first == second);
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	public void HighlightingObject_NullComparisons_AreNullSafe()
	{
		var obj = new HighlightingObject();
		HighlightingObject? nullObject = null;

		Assert.IsFalse(obj == null);
		Assert.IsTrue(nullObject == null);
		Assert.IsTrue(nullObject == nullObject);
		Assert.IsFalse(obj.Equals(null));
	}
}

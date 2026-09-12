using Nickelony.IDEKit.AvalonEdit.Extras.TextMate.Highlighting;
using System.Text.Json;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.TRX.Resources;

namespace TombLib.Tests;

[TestClass]
public class ImmutableCatalogTests
{
	[TestMethod]
	public void Keywords_Values_IsReadOnly()
	{
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<string>)Keywords.Values).Add("custom"));
	}

	[TestMethod]
	public void Keywords_RemovedConstants_IsReadOnly()
	{
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<RemovedKeyword>)Keywords.RemovedConstants).Add(default));
	}

	[TestMethod]
	public void Keywords_RemovedProperties_IsReadOnly()
	{
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<RemovedKeyword>)Keywords.RemovedProperties).Add(default));
	}

	[TestMethod]
	public void GameFlowDefinitionSet_Collections_AreOwnedAndReadOnly()
	{
		var source = new List<string> { "Level" };
		var set = new GameFlowDefinitionSet([], [], [], source);

		source.Add("Injected");

		Assert.AreEqual(1, set.Properties.Count);
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<string>)set.Properties).Add("custom"));
	}

	[TestMethod]
	public void GameFlowDefinitionSet_DeserializesFromPascalCaseJson()
	{
		string json = """{"SpecialProperties":["DESCRIPTION"],"Sections":["TITLE"],"Constants":["LAVA"],"Properties":["path"]}""";
		GameFlowDefinitionSet? set = JsonSerializer.Deserialize<GameFlowDefinitionSet>(json);

		Assert.IsNotNull(set);
		Assert.AreEqual(1, set!.Sections.Count);
		Assert.AreEqual("TITLE", set.Sections[0]);
		Assert.AreEqual("LAVA", set.Constants[0]);
		Assert.AreEqual("path", set.Properties[0]);
	}

	[TestMethod]
	public void ColorSchemeBase_Aliases_AreOwnedAndReadOnly()
	{
		var source = new List<string> { "alias-a" };
		var theme = new LuaTheme { Aliases = source };

		source.Add("Injected");

		Assert.AreEqual(1, theme.Aliases.Count);
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<string>)theme.Aliases).Add("custom"));
	}

	[TestMethod]
	public void TextMateTokenTheme_Rules_AreOwnedAndReadOnly()
	{
		var source = new List<TextMateTokenThemeRule> { new() { Scope = "comment" } };
		var theme = new TextMateTokenTheme { Rules = source };

		source.Add(new TextMateTokenThemeRule());

		Assert.AreEqual(1, theme.Rules.Count);
		Assert.ThrowsException<NotSupportedException>(() => ((ICollection<TextMateTokenThemeRule>)theme.Rules).Add(new TextMateTokenThemeRule()));
	}

	[TestMethod]
	public void TextMateTokenTheme_DeserializesFromJson()
	{
		string json = """{"Rules":[{"Scope":"comment","Foreground":"#999999","FontStyle":""}]}""";
		TextMateTokenTheme? theme = JsonSerializer.Deserialize<TextMateTokenTheme>(json);

		Assert.IsNotNull(theme);
		Assert.AreEqual(1, theme!.Rules.Count);
		Assert.AreEqual("comment", theme.Rules[0].Scope);
		Assert.AreEqual("#999999", theme.Rules[0].Foreground);
	}
}

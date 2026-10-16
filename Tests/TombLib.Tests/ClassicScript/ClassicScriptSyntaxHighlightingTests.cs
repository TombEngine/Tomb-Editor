using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptSyntaxHighlightingTests
{
	[TestMethod]
	public void MainRuleSet_IsRebuiltWhenMnemonicCatalogReloads()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());
		HighlightingRuleSet first = highlighting.MainRuleSet;

		// Reloading the shared catalog bumps the snapshot version, which must invalidate the cache.
		int versionBefore = ClassicScriptMnemonicCatalogService.CurrentSnapshotVersion;
		new ClassicScriptMnemonicCatalogService().Reload();
		int versionAfter = ClassicScriptMnemonicCatalogService.CurrentSnapshotVersion;

		Assert.AreNotEqual(versionBefore, versionAfter);
		Assert.AreNotSame(first, highlighting.MainRuleSet);
	}

	[TestMethod]
	public void MainRuleSet_CommandAndSectionRegexesComeFromCatalog()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());
		HighlightingRuleSet ruleSet = highlighting.MainRuleSet;

		HighlightingRule sectionsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("[Level]"));
		HighlightingRule oldCommandsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("Legend=") && !rule.Regex.IsMatch("[Level]"));
		HighlightingRule newCommandsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("FMV=") && !rule.Regex.IsMatch("Legend="));

		Assert.IsTrue(sectionsRule.Regex.IsMatch("[Options]"));
		Assert.IsTrue(sectionsRule.Regex.IsMatch("[Title]"));

		Assert.IsTrue(oldCommandsRule.Regex.IsMatch("AnimatingMIP="));
		Assert.IsTrue(oldCommandsRule.Regex.IsMatch("Cut=")); // Legacy array-only name is kept.

		Assert.IsTrue(newCommandsRule.Regex.IsMatch("AddEffect="));
		Assert.IsTrue(newCommandsRule.Regex.IsMatch("FMV=")); // One of the four entries absent from the legacy array.

		Assert.IsFalse(newCommandsRule.Regex.IsMatch("#DEFINE=")); // Directives are not command alternatives.
	}

	[TestMethod]
	public void MainRuleSet_MalformedHighlightingColors_DoesNotThrow()
	{
		var scheme = new ColorScheme
		{
			Background = "not-a-color",
			Foreground = "not-a-color",
			Comments = new HighlightingObject { HtmlColor = "not-a-color" },
			Sections = new HighlightingObject { HtmlColor = "not-a-color" },
			NewCommands = new HighlightingObject { HtmlColor = "not-a-color" }
		};

		var highlighting = new SyntaxHighlighting(scheme);

		// Malformed user-edited color data must not prevent the rule set from being built.
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsTrue(highlighting.MainRuleSet.Rules.Count > 0);
	}

	[TestMethod]
	public void HighlightingContract_AllIHighlightingDefinitionMembersAreUsable()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());

		Assert.AreEqual("ClassicScript Rules", highlighting.Name);
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsFalse(highlighting.NamedHighlightingColors.Any());
		Assert.IsNotNull(highlighting.Properties);
		Assert.AreEqual(0, highlighting.Properties.Count);
		Assert.IsNull(highlighting.GetNamedColor("anything"));
		Assert.IsNull(highlighting.GetNamedRuleSet("DoesNotExist"));
		Assert.IsNotNull(highlighting.GetNamedRuleSet(highlighting.Name));
	}
}

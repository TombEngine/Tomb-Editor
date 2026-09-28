using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class GameFlowSyntaxHighlightingTests
{
	[TestMethod]
	public void HighlightingContract_AllIHighlightingDefinitionMembersAreUsable()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());

		Assert.AreEqual("GameFlowScript Rules", highlighting.Name);
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsFalse(highlighting.NamedHighlightingColors.Any());
		Assert.IsNotNull(highlighting.Properties);
		Assert.AreEqual(0, highlighting.Properties.Count);
		Assert.IsNull(highlighting.GetNamedColor("anything"));
		Assert.IsNull(highlighting.GetNamedRuleSet("DoesNotExist"));
		Assert.IsNotNull(highlighting.GetNamedRuleSet(highlighting.Name));
	}

	[TestMethod]
	public void MainRuleSet_MalformedHighlightingColors_DoesNotThrow()
	{
		var scheme = new ColorScheme
		{
			Background = "not-a-color",
			Foreground = "not-a-color",
			Comments = new HighlightingObject { HtmlColor = "not-a-color" },
			Properties = new HighlightingObject { HtmlColor = "not-a-color" },
			Values = new HighlightingObject { HtmlColor = "not-a-color" }
		};

		var highlighting = new SyntaxHighlighting(scheme);

		// Malformed user-edited color data must not prevent the rule set from being built.
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsTrue(highlighting.MainRuleSet.Rules.Count > 0);
	}
}

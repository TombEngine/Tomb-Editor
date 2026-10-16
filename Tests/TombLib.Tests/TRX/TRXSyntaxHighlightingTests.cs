using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class TRXSyntaxHighlightingTests
{
	[TestMethod]
	public void HighlightingContract_AllIHighlightingDefinitionMembersAreUsable()
	{
		var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());
		var highlighting = new SyntaxHighlighting(new ColorScheme(), schemaService);

		Assert.AreEqual("TRX Rules", highlighting.Name);
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsFalse(highlighting.NamedHighlightingColors.Any());
		Assert.IsNotNull(highlighting.Properties);
		Assert.AreEqual(0, highlighting.Properties.Count);
		Assert.IsNull(highlighting.GetNamedColor("anything"));
		Assert.IsNull(highlighting.GetNamedRuleSet("DoesNotExist"));
		Assert.IsNotNull(highlighting.GetNamedRuleSet(highlighting.Name));
	}

	[TestMethod]
	public void Patterns_BuildsWithRegexSpecialKeywords_WithoutThrowing()
	{
		string path = WriteFixture("gameflow-regex-keywords.json", RegexSpecialKeywordFixture);

		try
		{
			var schemaService = new TRXGameFlowSchemaService(path);
			var patterns = new Patterns(schemaService);

			// Schema-derived keywords are regex-escaped, so the pattern matches them literally.
			Assert.IsTrue(patterns.Properties.Contains(Regex.Escape("level.foo(")));
			Assert.IsTrue(patterns.Properties.Contains(Regex.Escape("path[0]")));
			Assert.IsTrue(patterns.Properties.Contains(Regex.Escape("cost(1)")));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void SyntaxHighlighting_MalformedKeywords_DoesNotThrow()
	{
		string path = WriteFixture("gameflow-regex-keywords.json", RegexSpecialKeywordFixture);

		try
		{
			var schemaService = new TRXGameFlowSchemaService(path);
			var highlighting = new SyntaxHighlighting(new ColorScheme(), schemaService);

			// Building the rule set must not throw because schema text produced an invalid regex.
			Assert.IsNotNull(highlighting.MainRuleSet);
			Assert.IsTrue(highlighting.MainRuleSet.Rules.Count > 0);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public void MainRuleSet_MalformedHighlightingColors_DoesNotThrow()
	{
		var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());
		var scheme = new ColorScheme
		{
			Background = "not-a-color",
			Foreground = "not-a-color",
			Comments = new HighlightingObject { HtmlColor = "not-a-color" },
			Properties = new HighlightingObject { HtmlColor = "not-a-color" },
			Values = new HighlightingObject { HtmlColor = "not-a-color" }
		};

		var highlighting = new SyntaxHighlighting(scheme, schemaService);

		// Malformed user-edited color data must not prevent the rule set from being built.
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsTrue(highlighting.MainRuleSet.Rules.Count > 0);
	}

	private static string WriteFixture(string fileName, string content)
	{
		string path = Path.Combine(Path.GetTempPath(), fileName);

		File.WriteAllText(path, content);

		return path;
	}

	private const string RegexSpecialKeywordFixture =
		"""
		{
		  "type": "object",
		  "properties": {
		    "level.foo(": { "type": "string" },
		    "path[0]": { "type": "string" },
		    "cost(1)": { "type": "integer" }
		  }
		}
		""";
}

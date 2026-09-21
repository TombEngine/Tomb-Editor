using TombLib.Scripting.ClassicScript.Cleaning;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptDocumentFormatterTests
{
	[TestMethod]
	public void FormatDocument_UsesConfiguredSpacingRulesAndTrimsTrailingWhitespace()
	{
		var formatter = new ClassicScriptDocumentFormatter
		{
			SpaceBeforeEquals = true,
			SpaceAfterEquals = true,
			SpaceBeforeComma = false,
			SpaceAfterComma = true,
			CollapseMultipleSpaces = false
		};

		string formatted = formatter.FormatDocument("Customize= CUST_BAR,foo   \r\nLegend =1\t");

		Assert.AreEqual("Customize = CUST_BAR, foo" + Environment.NewLine + "Legend = 1", formatted);
	}

	[TestMethod]
	public void FormatDocument_IsIdempotentAcrossRepresentativeScriptLines()
	{
		var formatter = new ClassicScriptDocumentFormatter
		{
			SpaceBeforeEquals = true,
			SpaceAfterEquals = true,
			SpaceBeforeComma = false,
			SpaceAfterComma = true,
			CollapseMultipleSpaces = true
		};
		string[] inputs =
		[
			"Legend=1\r\nCustomize =  CUST_BAR,foo   ",
			"\tLegend = 1\t\nCustomize= CUST_BAR, foo",
			"Malformed command =\r\n#INCLUDE = file,",
			"Name=\"Level=1, test\"  \r\n; inline comment = unchanged",
			"#DEFINE FOO = 1\nLegend = 1 ; continuation"
		];

		foreach (string input in inputs)
		{
			string formatted = formatter.FormatDocument(input);

			Assert.AreEqual(formatted, formatter.FormatDocument(formatted), input);
		}
	}

	[TestMethod]
	public void FormatCompilerOutput_RemovesSpacesBeforeEqualsWithoutChangingSpacesAfterEquals()
	{
		string formatted = ClassicScriptDocumentFormatter.FormatCompilerOutput("Legend =1   \r\nCustomize =  CUST_BAR\t");

		Assert.AreEqual("Legend=1" + Environment.NewLine + "Customize=  CUST_BAR", formatted);
	}
}
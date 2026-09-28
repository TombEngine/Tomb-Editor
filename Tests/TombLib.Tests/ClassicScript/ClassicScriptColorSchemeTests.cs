using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Highlighting;

namespace TombLib.Tests;

// Verify that the bundled ClassicScript color schemes load from JSON through the
// real ClassicScriptEditorConfiguration path.
[TestClass]
public class ClassicScriptColorSchemeTests
{
	private static ClassicScriptEditorConfiguration CreateConfiguration(string schemeName)
		=> new() { SelectedColorSchemeName = schemeName };

	[TestMethod]
	public void DefaultConfiguration_UsesVs15JsonScheme()
	{
		var config = CreateConfiguration("VS15");

		Assert.AreEqual("#202020", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("SteelBlue", config.ColorScheme.Sections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Sections.IsBold);
		Assert.AreEqual("LightSalmon", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("Orchid", config.ColorScheme.References.HtmlColor);
		Assert.AreEqual("MediumAquamarine", config.ColorScheme.StandardCommands.HtmlColor);
		Assert.AreEqual("SpringGreen", config.ColorScheme.NewCommands.HtmlColor);
		Assert.AreEqual("Green", config.ColorScheme.Comments.HtmlColor);
	}

	[TestMethod]
	public void ObsidianJsonScheme_PreservesOriginalColors()
	{
		var config = CreateConfiguration("Obsidian");

		Assert.AreEqual("#283032", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("#A082BD", config.ColorScheme.Sections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Sections.IsBold);
		Assert.AreEqual("#D97640", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("#FFCD22", config.ColorScheme.References.HtmlColor);
		Assert.AreEqual("#93C763", config.ColorScheme.StandardCommands.HtmlColor);
		Assert.AreEqual("#668BB0", config.ColorScheme.NewCommands.HtmlColor);
		Assert.AreEqual("#66747B", config.ColorScheme.Comments.HtmlColor);
	}

	[TestMethod]
	public void NgCenterJsonScheme_PreservesLightThemeFlags()
	{
		var config = CreateConfiguration("NG_Center");

		Assert.AreEqual("White", config.ColorScheme.Background);
		Assert.AreEqual("Black", config.ColorScheme.Foreground);
		Assert.AreEqual("Black", config.ColorScheme.Sections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Sections.IsBold);
		Assert.AreEqual("Black", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("Black", config.ColorScheme.References.HtmlColor);
		Assert.IsTrue(config.ColorScheme.References.IsItalic);
		Assert.AreEqual("Gray", config.ColorScheme.Comments.HtmlColor);
	}

	[TestMethod]
	public void MonokaiJsonScheme_PreservesOriginalColors()
	{
		var config = CreateConfiguration("Monokai");

		Assert.AreEqual("#2C2C2A", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("#F92672", config.ColorScheme.Sections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Sections.IsBold);
		Assert.AreEqual("#E6DB74", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("#AE81FF", config.ColorScheme.References.HtmlColor);
		Assert.AreEqual("#93C763", config.ColorScheme.StandardCommands.HtmlColor);
		Assert.AreEqual("#66D9EF", config.ColorScheme.NewCommands.HtmlColor);
		Assert.AreEqual("#696969", config.ColorScheme.Comments.HtmlColor);
	}

	[TestMethod]
	public void UnknownScheme_FallsBackToDefaultColorScheme()
	{
		var config = CreateConfiguration("DoesNotExist");

		Assert.AreEqual("Black", config.ColorScheme.Background);
		Assert.AreEqual("White", config.ColorScheme.Foreground);
		Assert.AreEqual("White", config.ColorScheme.Sections.HtmlColor);
		Assert.IsFalse(config.ColorScheme.Sections.IsBold);
	}

	[TestMethod]
	public void SelectedColorSchemeName_IsPreservedOnSwitch()
	{
		var config = CreateConfiguration("Obsidian");

		config.SelectedColorSchemeName = "Monokai";

		Assert.AreEqual("Monokai", config.SelectedColorSchemeName);
		Assert.AreEqual("#2C2C2A", config.ColorScheme.Background);
		Assert.AreEqual("#F92672", config.ColorScheme.Sections.HtmlColor);
	}
}

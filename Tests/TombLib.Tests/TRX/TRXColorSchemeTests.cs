using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Highlighting;

namespace TombLib.Tests;

// Verify that the bundled TRX color schemes load from JSON through the real
// TRXEditorConfiguration path.
[TestClass]
public class TRXColorSchemeTests
{
	private static TRXEditorConfiguration CreateConfiguration(string schemeName)
		=> new() { SelectedColorSchemeName = schemeName };

	[TestMethod]
	public void DefaultConfiguration_UsesVs15JsonScheme()
	{
		var config = CreateConfiguration("VS15");

		Assert.AreEqual("#202020", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("Green", config.ColorScheme.Comments.HtmlColor);
		Assert.AreEqual("Orchid", config.ColorScheme.Constants.HtmlColor);
		Assert.AreEqual("SpringGreen", config.ColorScheme.Collections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Collections.IsBold);
		Assert.AreEqual("MediumAquamarine", config.ColorScheme.Properties.HtmlColor);
		Assert.AreEqual("SteelBlue", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("LightSalmon", config.ColorScheme.Strings.HtmlColor);
	}

	[TestMethod]
	public void ObsidianJsonScheme_PreservesOriginalColors()
	{
		var config = CreateConfiguration("Obsidian");

		Assert.AreEqual("#283032", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("#66747B", config.ColorScheme.Comments.HtmlColor);
		Assert.AreEqual("#FFCD22", config.ColorScheme.Constants.HtmlColor);
		Assert.AreEqual("#668BB0", config.ColorScheme.Collections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Collections.IsBold);
		Assert.AreEqual("#93C763", config.ColorScheme.Properties.HtmlColor);
		Assert.AreEqual("#A082BD", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("LightSalmon", config.ColorScheme.Strings.HtmlColor);
	}

	[TestMethod]
	public void NgCenterJsonScheme_PreservesLightThemeFlags()
	{
		var config = CreateConfiguration("NG_Center");

		Assert.AreEqual("White", config.ColorScheme.Background);
		Assert.AreEqual("Black", config.ColorScheme.Foreground);
		Assert.AreEqual("Gray", config.ColorScheme.Comments.HtmlColor);
		Assert.AreEqual("Black", config.ColorScheme.Constants.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Constants.IsItalic);
		Assert.AreEqual("Black", config.ColorScheme.Collections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Collections.IsBold);
		Assert.AreEqual("Black", config.ColorScheme.Properties.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Properties.IsBold);
		Assert.AreEqual("Black", config.ColorScheme.Values.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Values.IsItalic);
		Assert.AreEqual("Black", config.ColorScheme.Strings.HtmlColor);
	}

	[TestMethod]
	public void MonokaiJsonScheme_PreservesOriginalColors()
	{
		var config = CreateConfiguration("Monokai");

		Assert.AreEqual("#2C2C2A", config.ColorScheme.Background);
		Assert.AreEqual("Gainsboro", config.ColorScheme.Foreground);
		Assert.AreEqual("#696969", config.ColorScheme.Comments.HtmlColor);
		Assert.AreEqual("#AE81FF", config.ColorScheme.Constants.HtmlColor);
		Assert.AreEqual("#66D9EF", config.ColorScheme.Collections.HtmlColor);
		Assert.IsTrue(config.ColorScheme.Collections.IsBold);
		Assert.AreEqual("#93C763", config.ColorScheme.Properties.HtmlColor);
		Assert.AreEqual("#F92672", config.ColorScheme.Values.HtmlColor);
		Assert.AreEqual("#E6DB74", config.ColorScheme.Strings.HtmlColor);
	}

}

using System.IO;
using TombLib.Utils;

namespace TombLib.Tests;

// Verify that the shared JsonUtils.ReadJsonFile<T> helper reads the bundled JSON
// color scheme files through the real output directory layout.
[TestClass]
public class JsonUtilsColorSchemeTests
{
	private static readonly string[] SchemeNames = ["VS15", "Obsidian", "NG_Center", "Monokai"];

	private static string GetSchemesDirectory(string languageDirectory)
		=> Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "ColorSchemes", languageDirectory);

	[TestMethod]
	public void AllBundledClassicScriptSchemes_ExistAsJsonFiles()
	{
		foreach (string schemeName in SchemeNames)
		{
			string path = Path.Combine(GetSchemesDirectory("ClassicScript"), schemeName + ".json");

			Assert.IsTrue(File.Exists(path), $"Missing bundled scheme: {path}");
		}
	}

	[TestMethod]
	public void AllBundledTrxSchemes_ExistAsJsonFiles()
	{
		foreach (string schemeName in SchemeNames)
		{
			string path = Path.Combine(GetSchemesDirectory("TRX"), schemeName + ".json");

			Assert.IsTrue(File.Exists(path), $"Missing bundled scheme: {path}");
		}
	}

	[TestMethod]
	public void AllBundledGameFlowSchemes_ExistAsJsonFiles()
	{
		foreach (string schemeName in SchemeNames)
		{
			string path = Path.Combine(GetSchemesDirectory("GameFlowScript"), schemeName + ".json");

			Assert.IsTrue(File.Exists(path), $"Missing bundled scheme: {path}");
		}
	}

	[TestMethod]
	public void NoBundledXmlColorSchemesRemain()
	{
		string[] classicScriptFiles = Directory.GetFiles(GetSchemesDirectory("ClassicScript"), "*.xml", SearchOption.TopDirectoryOnly);
		string[] trxFiles = Directory.GetFiles(GetSchemesDirectory("TRX"), "*.xml", SearchOption.TopDirectoryOnly);
		string[] gameFlowFiles = Directory.GetFiles(GetSchemesDirectory("GameFlowScript"), "*.xml", SearchOption.TopDirectoryOnly);

		Assert.AreEqual(0, classicScriptFiles.Length);
		Assert.AreEqual(0, trxFiles.Length);
		Assert.AreEqual(0, gameFlowFiles.Length);
	}

	[TestMethod]
	public void ReadJsonFile_DeserializesBundledMonokaiScheme()
	{
		string path = Path.Combine(GetSchemesDirectory("ClassicScript"), "Monokai.json");

		var scheme = JsonUtils.ReadJsonFile<TombLib.Scripting.ClassicScript.Highlighting.ColorScheme>(path);

		Assert.AreEqual("#2C2C2A", scheme.Background);
		Assert.AreEqual("Gainsboro", scheme.Foreground);
		Assert.AreEqual("#F92672", scheme.Sections.HtmlColor);
		Assert.IsTrue(scheme.Sections.IsBold);
	}
}

using System;
using System.IO;
using System.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Tests;

[TestClass]
public class TextEditorConfigValidationTests
{
	public sealed class TestConfig : TextEditorConfigBase
	{
		public override string DefaultPath => "unused";
	}

	[TestMethod]
	public void FontSize_NonPositive_Throws()
	{
		var config = new TestConfig();

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => config.FontSize = 0);
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => config.FontSize = -1);
	}

	[TestMethod]
	public void UndoStackSize_Negative_Throws_ZeroIsAllowed()
	{
		var config = new TestConfig();

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => config.UndoStackSize = -1);

		config.UndoStackSize = 0;
		Assert.AreEqual(0, config.UndoStackSize);
	}

	[TestMethod]
	public void Load_InvalidNumericSetting_FallsBackToDefaults()
	{
		string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestConfig><FontSize>0</FontSize></TestConfig>";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

		TestConfig loaded = TombLib.Scripting.UI.Bases.ConfigurationBase.Load<TestConfig>(stream);

		Assert.AreEqual(TextEditorBaseDefaults.FontSize, loaded.FontSize);
		Assert.AreEqual(TextEditorBaseDefaults.UndoStackSize, loaded.UndoStackSize);
	}

	[TestMethod]
	public void Load_MissingFile_FallsBackToDefaults()
	{
		string missingPath = Path.Combine(Path.GetTempPath(), "Missing-" + Guid.NewGuid().ToString("N") + ".xml");

		TestConfig loaded = TombLib.Scripting.UI.Bases.ConfigurationBase.Load<TestConfig>(missingPath);

		Assert.AreEqual(TextEditorBaseDefaults.FontSize, loaded.FontSize);
		Assert.AreEqual(TextEditorBaseDefaults.UndoStackSize, loaded.UndoStackSize);
	}
}

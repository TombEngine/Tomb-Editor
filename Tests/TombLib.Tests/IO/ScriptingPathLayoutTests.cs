using System.IO;
using TombLib.Scripting.ClassicScript.Compilers;
using TombLib.Scripting.GameFlowScript.Compilers;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Tests;

// Phase 2 path-layout tests: prove the typed path layouts compose the same installation-relative
// directories as the removed linked GlobalPaths.cs, using an explicit temporary root.
[TestClass]
public class ScriptingPathLayoutTests
{
	private string _tempRoot = string.Empty;

	[TestInitialize]
	public void SetUp()
	{
		_tempRoot = Path.Combine(Path.GetTempPath(), "TombScriptingPaths-" + Guid.NewGuid().ToString("N"));
	}

	[TestCleanup]
	public void TearDown()
	{
		if (Directory.Exists(_tempRoot))
			Directory.Delete(_tempRoot, recursive: true);
	}

	[TestMethod]
	public void ScriptingPaths_ComposesSharedConfigurationLayout()
	{
		var paths = new ScriptingPaths(_tempRoot);

		Assert.AreEqual(_tempRoot, paths.ProgramDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors"), paths.TextEditorConfigsDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "Themes"), paths.TextEditorThemesDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "ColorSchemes"), paths.ColorSchemesDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "ColorSchemes", "ClassicScript"), paths.ClassicScriptColorConfigsDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "ColorSchemes", "GameFlowScript"), paths.GameFlowColorConfigsDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "ColorSchemes", "TRX"), paths.TRXColorConfigsDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "Configs", "TextEditors", "Themes", "Lua"), paths.LuaThemeConfigsDirectory);
	}

	[TestMethod]
	public void ClassicScriptCompilerPaths_ComposesCompilerLayout()
	{
		var paths = new ClassicScriptCompilerPaths(_tempRoot);

		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE"), paths.TIDEDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "DOS"), paths.DOSDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "DOS", "TR4"), paths.TR4ScriptCompilerDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "DOS", "DOSBox.exe"), paths.DOSBoxExecutable);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "NGC"), paths.InternalNGCDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "NGC", "NG_Center.exe"), paths.NGCExecutable);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "NGC", "VGE"), paths.VGEDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "NGC", "VGE", "Script"), paths.VGEScriptDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TombIDE Library Registration.exe"), paths.LibraryRegistrationExecutable);
	}

	[TestMethod]
	public void GameFlowCompilerPaths_ComposesCompilerLayout()
	{
		var paths = new GameFlowCompilerPaths(_tempRoot);

		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE"), paths.TIDEDirectory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "GFL"), paths.GameFlow2Directory);
		Assert.AreEqual(Path.Combine(_tempRoot, "TIDE", "GF3"), paths.GameFlow3Directory);
	}
}

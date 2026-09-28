using System;
using System.IO;
using System.Windows;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.Lua;

namespace TombLib.Tests;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public class TextEditorSessionGenerationTests
{
	[TestMethod]
	public void SessionGeneration_AdvancesOnLoadRenameAndDispose()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				string filePath = Path.Combine(Path.GetTempPath(), $"tomb-session-{Guid.NewGuid():N}.cs");
				File.WriteAllText(filePath, "Name=Test");

				int initial = editor.SessionGeneration;
				editor.Load(filePath);
				int afterLoad = editor.SessionGeneration;
				Assert.IsTrue(afterLoad > initial);

				// A path change is a rename boundary and advances the session generation.
				editor.FilePath = Path.ChangeExtension(filePath, ".renamed.cs");
				int afterRename = editor.SessionGeneration;
				Assert.IsTrue(afterRename > afterLoad);

				// Disposal is a boundary and advances the session generation so in-flight work
				// admitted before disposal is rejected as stale.
				editor.Dispose();
				Assert.IsTrue(editor.SessionGeneration > afterRename);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SessionGeneration_AdvancesOnWorkspaceContentReplacement()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				int before = editor.SessionGeneration;
				editor.ApplyWorkspaceContent(@"C:\Scripts\replaced.cs", "Name=Replaced");
				Assert.IsTrue(editor.SessionGeneration > before);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SessionGeneration_DoesNotAdvanceOnOrdinaryTextChange()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				int generationBefore = editor.SessionGeneration;
				int versionBefore = editor.DocumentVersion;

				editor.Text = "local value = 1";

				// Ordinary editing advances the logical document version but not the session
				// generation; the two counters are not interchangeable.
				Assert.AreEqual(generationBefore, editor.SessionGeneration);
				Assert.IsTrue(editor.DocumentVersion > versionBefore);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static ClassicScriptLanguageServices CreateClassicScriptLanguageServices()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
		var indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
		var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

		return new ClassicScriptLanguageServices(
			new ClassicScriptDefinitionProvider(commandService),
			new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
			new ClassicScriptSignatureHelpProvider(commandService),
			errorDetector,
			lineService,
			commandService,
			indexService);
	}
}

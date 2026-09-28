using System.Windows;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.TRX.Writers;

namespace TombLib.Tests;

[TestClass]
public class TRXScriptReplacerTests
{
	private static TRXLanguageServices CreateLanguageServices()
	{
		var lineService = new TRXLineService();
		var documentService = new TRXDocumentService(lineService);
		var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

		return new TRXLanguageServices(
			schemaService,
			lineService,
			documentService,
			new TRXDefinitionProvider(documentService),
			new TRXGameFlowCompletionService(schemaService),
			new TRXGameFlowHoverService(schemaService));
	}

	[TestMethod]
	public void RenameLevelScript_RenamesTitleValue()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "\"title\": \"MyLevel\",\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer();
				replacer.RenameLevelScript(editor, "MyLevel", "NewLevel");

				Assert.AreEqual("\"title\": \"NewLevel\",\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLevelScript_NoMatchingLine_LeavesDocumentUnchanged()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "\"title\": \"MyLevel\",\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer();
				replacer.RenameLevelScript(editor, "Missing", "NewLevel");

				Assert.AreEqual("\"title\": \"MyLevel\",\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

using System.Windows;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Writers;

namespace TombLib.Tests;

[TestClass]
public class GameFlowScriptReplacerTests
{
	private static GameFlowLanguageServices CreateLanguageServices()
	{
		var lineService = new GameFlowScriptLineService();
		var documentService = new GameFlowScriptDocumentService(lineService);

		return new GameFlowLanguageServices(
			new GameFlowDefinitionProvider(documentService),
			new GameFlowHoverProvider(),
			new GameFlowCompletionProvider(),
			lineService,
			documentService);
	}

	[TestMethod]
	public void RenameLanguageString_PreservesLeadingIndentation()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "MY_LEVEL\r\n  MyString\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				new ScriptReplacer().RenameLanguageString(editor, "MyString", "NewString");

				Assert.AreEqual("MY_LEVEL\r\n  NewString\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLanguageString_NoMatchingLine_LeavesDocumentUnchanged()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "MY_LEVEL\r\n  MyString\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				new ScriptReplacer().RenameLanguageString(editor, "Missing", "NewString");

				Assert.AreEqual("MY_LEVEL\r\n  MyString\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

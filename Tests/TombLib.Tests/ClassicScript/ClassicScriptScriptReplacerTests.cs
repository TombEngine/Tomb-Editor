using System.Windows;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Writers;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptScriptReplacerTests
{
	private static ClassicScriptLanguageServices CreateLanguageServices()
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

	[TestMethod]
	public void RenameLevelScript_RenamesNameValue()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[Level]\r\nName=Level1\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLevelScript(editor, "Level1", "Level2");

				Assert.AreEqual("[Level]\r\nName=Level2\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLevelScript_WithTrailingComment_PreservesComment()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[Level]\r\nName=Level1 ; keep me\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLevelScript(editor, "Level1", "Level2");

				Assert.AreEqual("[Level]\r\nName=Level2 ; keep me\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLevelScript_ChangesOnlyTargetValueAndIsStable()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[Level]\r\nName=Level1 ; Level1 comment\r\nAuthor=Level1\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLevelScript(editor, "Level1", "Level2");

				Assert.AreEqual(
					"[Level]\r\nName=Level2 ; Level1 comment\r\nAuthor=Level1\r\n",
					editor.Text);

				replacer.RenameLevelScript(editor, "Level1", "Level2");

				Assert.AreEqual(
					"[Level]\r\nName=Level2 ; Level1 comment\r\nAuthor=Level1\r\n",
					editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLanguageString_WithNGStringIndex_RenamesValue()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[ExtraNG]\r\n0: MyString\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLanguageString(editor, "MyString", "NewString");

				Assert.AreEqual("[ExtraNG]\r\n0: NewString\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RenameLanguageString_ChangesOnlyTargetValueAndIsStable()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[ExtraNG]\r\n0: MyString ; MyString comment\r\n1: OtherString\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLanguageString(editor, "MyString", "NewString");

				Assert.AreEqual(
					"[ExtraNG]\r\n0: NewString ; MyString comment\r\n1: OtherString\r\n",
					editor.Text);

				replacer.RenameLanguageString(editor, "MyString", "NewString");

				Assert.AreEqual(
					"[ExtraNG]\r\n0: NewString ; MyString comment\r\n1: OtherString\r\n",
					editor.Text);
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
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[Level]\r\nName=Level1\r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var replacer = new ScriptReplacer(CreateLanguageServices().LineService);
				replacer.RenameLevelScript(editor, "Missing", "Level2");

				Assert.AreEqual("[Level]\r\nName=Level1\r\n", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Writers;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TextEditorProgrammaticEditTests
{
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

	private static TRXLanguageServices CreateTrxLanguageServices()
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
	public void InputFreeIndex_InsertsNextFreeIndexAtCaret()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[Triggers]\r\nTriggerGroup= \r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 26;
				editor.InputFreeIndex();

				Assert.AreEqual("[Triggers]\r\nTriggerGroup= 1\r\n", editor.Text);
				Assert.AreEqual(27, editor.CaretOffset);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void InputFreeIndex_DoesNotOpenCompletionWindow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[Triggers]\r\nTriggerGroup= \r\n"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 26;
				editor.InputFreeIndex();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Assert.IsNull(editor.ActiveCompletionWindow);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryAddNewPluginEntry_AppendsPluginLineToOptionsSection()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[Options]\r\nPlugin= 0, SomePlugin, IGNORE"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				bool added = editor.TryAddNewPluginEntry("NewPlugin");

				Assert.IsTrue(added);
				Assert.AreEqual("[Options]\r\nPlugin= 0, SomePlugin, IGNORE\r\nPlugin= 1, NewPlugin, IGNORE", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryAddNewPluginEntry_EditIsSingleUndoStep()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[Options]\r\nPlugin= 0, SomePlugin, IGNORE"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Document.UndoStack.ClearAll();

				bool added = editor.TryAddNewPluginEntry("NewPlugin");

				Assert.IsTrue(added);
				Assert.IsTrue(editor.Document.UndoStack.CanUndo);

				editor.Document.UndoStack.Undo();

				Assert.AreEqual("[Options]\r\nPlugin= 0, SomePlugin, IGNORE", editor.Text);
				Assert.IsFalse(editor.Document.UndoStack.CanUndo);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryAddNewPluginEntry_MarksContentChanged()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[Options]\r\nPlugin= 0, SomePlugin, IGNORE"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.ApplyPersistedContent(editor.Text);
				Assert.IsFalse(editor.IsContentChanged);

				bool added = editor.TryAddNewPluginEntry("NewPlugin");

				Assert.IsTrue(added);
				Assert.IsTrue(editor.IsContentChanged);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void WriteNewNGString_InsertsNumberedLineAfterLastIndex()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateClassicScriptLanguageServices())
			{
				Text = "[ExtraNG]\r\n0: ExistingString"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var writer = new LanguageStringWriter(CreateClassicScriptLanguageServices().CommandService);
				bool written = writer.WriteNewNGString(editor, "NewString");

				Assert.IsTrue(written);
				Assert.AreEqual("[ExtraNG]\r\n0: ExistingString\r\n1: NewString", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TrxBracketAutospacing_EnterInsideBraces_ExpandsBrackets()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateTrxLanguageServices())
			{
				Text = "{}"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 1;
				editor.TextArea.PerformTextInput("\n");

				Assert.AreEqual("{\r\n\r\n\t}", editor.Text);
				Assert.AreEqual(6, editor.CaretOffset);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TrxBracketAutospacing_HelperInsertIsSingleUndoStep()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateTrxLanguageServices())
			{
				Text = "{}"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 1;
				editor.Document.UndoStack.ClearAll();

				editor.TextArea.PerformTextInput("\n");

				Assert.AreEqual("{\r\n\r\n\t}", editor.Text);

				// The user's newline is one undo step; the helper's newline-plus-tab is one grouped step.
				editor.Document.UndoStack.Undo();
				Assert.AreEqual("{\r\n}", editor.Text);

				editor.Document.UndoStack.Undo();
				Assert.AreEqual("{}", editor.Text);
				Assert.IsFalse(editor.Document.UndoStack.CanUndo);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

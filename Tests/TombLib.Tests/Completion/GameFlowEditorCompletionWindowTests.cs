using ICSharpCode.AvalonEdit.CodeCompletion;
using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Completion;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class GameFlowEditorCompletionWindowTests
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
	public void CompletionWindow_ClosingItClearsTheActiveWindow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "test"
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				TextCompletionController completionController = GetCompletionController(editor);

				Assert.IsTrue(completionController.OpenOrRefresh([new CompletionData("Level")], 1, 3));
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(completionWindow);

				completionWindow.Close();
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
	public void OpenOrRefreshCompletionWindow_WithChangedRange_ReplacesTheVisibleWindow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "test"
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				TextCompletionController completionController = GetCompletionController(editor);

				Assert.IsTrue(completionController.OpenOrRefresh([new CompletionData("Level")], 1, 3));
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? firstWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(firstWindow);

				// A changed replacement start closes the visible window and opens a new one instead of
				// leaving the old window open.
				Assert.IsTrue(completionController.OpenOrRefresh([new CompletionData("Level")], 0, 2));
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? secondWindow = editor.ActiveCompletionWindow;

				Assert.AreNotSame(firstWindow, secondWindow);
				Assert.IsFalse(firstWindow.IsVisible);
				Assert.IsTrue(secondWindow!.IsVisible);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void OpenOrRefreshCompletionWindow_PopulatesItemsAndOffsets()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "test"
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				TextCompletionController completionController = GetCompletionController(editor);

				bool opened = completionController.OpenOrRefresh([new CompletionData("Level")], 1, 3);

				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;

				Assert.IsTrue(opened);
				Assert.IsNotNull(completionWindow);
				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(1, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}

	private static TextCompletionController GetCompletionController(GameFlowEditor editor)
	{
		object? controller = WPFTestHelper.InvokeInstanceMethod(editor, "get_CompletionController", Type.EmptyTypes);

		return controller as TextCompletionController
			?? throw new InvalidOperationException("Completion controller was not found.");
	}
}

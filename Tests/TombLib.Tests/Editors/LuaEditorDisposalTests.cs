using Moq;
using System;
using System.Windows;
using System.Windows.Threading;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorDisposalTests
{
	[TestMethod]
	public void Dispose_WithoutUnloaded_ClosesLanguageServerDocument()
	{
		RunInSta(() =>
		{
			var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				IntelliSenseProvider = provider.Object
			};

			// Explicit disposal must close the LSP document even when Unloaded never fires.
			editor.Dispose();

			provider.Verify(x => x.CloseDocument(@"C:\Workspace\Scripts\test.lua"), Times.Once);
		});
	}

	[TestMethod]
	public void Dispose_WhileLoaded_ClosesDocumentOnce_AndUnloadedIsIdempotent()
	{
		RunInSta(() =>
		{
			var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				IntelliSenseProvider = provider.Object
			};

			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				editor.Dispose();
			}
			finally
			{
				// Unloaded fires after disposal; the cleanup must stay idempotent.
				hostWindow.Close();
			}

			provider.Verify(x => x.CloseDocument(@"C:\Workspace\Scripts\test.lua"), Times.Once);
		});
	}

	[TestMethod]
	public void Dispose_IsIdempotent_AndDoesNotThrow()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua"
			};

			editor.Dispose();
			editor.Dispose();
		});
	}

	[TestMethod]
	public void ExercisedEditor_IsCollectibleAfterHostAndProviderDisposal()
	{
		WeakReference? editorReference = null;
		RunInSta(() => editorReference = CreateAndDisposeExercisedEditor());

		Assert.IsNotNull(editorReference);
		AssertCollected(editorReference, nameof(LuaEditor));
	}

	private static WeakReference CreateAndDisposeExercisedEditor()
	{
		var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
		var editor = new LuaEditor(new Version(1, 0))
		{
			FilePath = @"C:\Workspace\Scripts\exercised.lua",
			Text = "local value = 1",
			IntelliSenseProvider = provider.Object
		};
		Window hostWindow = ShowInHostWindow(editor);

		editor.SetSemanticTokens([new SemanticToken(0, 6, 5, "variable", [])]);
		editor.RunContentChangedWorker();
		editor.Dispose();
		hostWindow.Content = null;
		hostWindow.Close();
		PumpDispatcher(hostWindow.Dispatcher, DispatcherPriority.ContextIdle);

		return new WeakReference(editor);
	}
}

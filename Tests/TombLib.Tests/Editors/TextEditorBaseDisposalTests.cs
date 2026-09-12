using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Hover;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Hover;

namespace TombLib.Tests;

[TestClass]
public class TextEditorBaseDisposalTests
{
	[TestMethod]
	public void Dispose_CanBeCalledTwice_DoesNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();
				editor.Dispose();
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_AllowsDerivedCleanupToUseEditorOperations()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new CleanupAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.IsTrue(editor.CleanupCompleted);
				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.UpdateSettings(new ClassicScriptEditorConfiguration()));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateSettings_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.UpdateSettings(new ClassicScriptEditorConfiguration()));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void ApplyPersistedContent_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.ApplyPersistedContent("content"));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Load_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				// The guard throws before any file I/O occurs.
				Assert.ThrowsException<ObjectDisposedException>(() => editor.Load("unused.tmp"));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Content_Setter_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.Content = "new content");
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void FilePath_Access_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.FilePath = "current path";
				editor.Dispose();

				Assert.AreEqual("current path", editor.FilePath);
				Assert.ThrowsException<ObjectDisposedException>(() => editor.FilePath = "new path");
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RetainedScalarState_RemainsReadable_ButCannotBeMutatedAfterDispose()
	{
		WPFTestHelper.RunInSta(() =>
		{
			DateTime lastModified = new(2026, 1, 2);
			Version engineVersion = new(4, 8);
			var editor = new PlainTextEditor
			{
				IsContentChanged = true,
				LastModified = lastModified,
				EngineVersion = engineVersion
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.IsTrue(editor.IsContentChanged);
				Assert.AreEqual(lastModified, editor.LastModified);
				Assert.AreSame(engineVersion, editor.EngineVersion);
				Assert.AreEqual(EditorType.Text, editor.EditorType);
				Assert.ThrowsException<ObjectDisposedException>(() => editor.IsContentChanged = false);
				Assert.ThrowsException<ObjectDisposedException>(() => editor.LastModified = default);
				Assert.ThrowsException<ObjectDisposedException>(() => editor.EngineVersion = new Version(5, 0));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunContentChangedWorker_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.RunContentChangedWorker());
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_StopsDiagnosticsWorker()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var coordinator = (TextDiagnosticsCoordinator?)WPFTestHelper.GetPrivateFieldValue(editor, "_diagnosticsCoordinator");
				Assert.IsNotNull(coordinator);

				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.SetDiagnostics([]));
				Assert.ThrowsException<ObjectDisposedException>(() => editor.ClearDiagnostics());

				// Post-dispose scheduling must be a no-op and must not start the worker timer.
				coordinator.RunOnIdle("new content");

				Assert.IsFalse(coordinator.IsBusy);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void InitializeHover_RepeatedInitialization_DisposesPreviousController()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ReinitializableTextEditor();

			try
			{
				editor.ReinitializeHover();
				var firstController = WPFTestHelper.GetPrivateField<TextHoverController>(editor, "_hoverController");

				editor.ReinitializeHover();
				var secondController = WPFTestHelper.GetPrivateField<TextHoverController>(editor, "_hoverController");

				Assert.AreNotSame(firstController, secondController);
				Assert.IsTrue((bool)(WPFTestHelper.GetPrivateFieldValue(firstController, "_isDisposed") ?? false));
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void InitializeDiagnostics_RepeatedInitialization_DisposesPreviousCoordinator()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ReinitializableTextEditor();

			try
			{
				editor.ReinitializeDiagnostics();
				var firstCoordinator = WPFTestHelper.GetPrivateField<TextDiagnosticsCoordinator>(editor, "_diagnosticsCoordinator");

				editor.ReinitializeDiagnostics();
				var secondCoordinator = WPFTestHelper.GetPrivateField<TextDiagnosticsCoordinator>(editor, "_diagnosticsCoordinator");

				Assert.AreNotSame(firstCoordinator, secondCoordinator);
				Assert.IsTrue((bool)(WPFTestHelper.GetPrivateFieldValue(firstCoordinator, "_isDisposed") ?? false));
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void ExercisedEditor_IsCollectibleAfterHostAndDisposal()
	{
		WeakReference? editorReference = null;
		WPFTestHelper.RunInSta(() => editorReference = CreateAndDisposeExercisedEditor());

		Assert.IsNotNull(editorReference);
		WPFTestHelper.AssertCollected(editorReference, nameof(ClassicScriptEditor));
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void GoToDefinition_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.InvokeGoToDefinition(CreateLanguageServices().DefinitionProvider, "Name"));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void TryGoToDefinition_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				ClassicScriptLanguageServices services = CreateLanguageServices();
				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.InvokeTryGoToDefinition(services.DefinitionProvider, services.HoverProvider, 0));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void TryShowDiagnosticToolTip_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.InvokeTryShowDiagnosticToolTip(0));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void TryHandleCtrlSpaceCompletion_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				// The disposal guard runs before the event arguments are inspected.
				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.InvokeTryHandleCtrlSpaceCompletion(null!, () => { }));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void InitializeDefinitionNavigation_RepeatedInitialization_DisposesPreviousController()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();

			try
			{
				editor.ReinitializeDefinitionNavigation();
				var firstController = WPFTestHelper.GetPrivateField<TextDefinitionTriggerController>(editor, "_definitionTriggerController");

				editor.ReinitializeDefinitionNavigation();
				var secondController = WPFTestHelper.GetPrivateField<TextDefinitionTriggerController>(editor, "_definitionTriggerController");

				Assert.AreNotSame(firstController, secondController);
				Assert.IsTrue((bool)(WPFTestHelper.GetPrivateFieldValue(firstController, "_isDisposed") ?? false));
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void Dispose_DisposesDefinitionNavigationController()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new NavigationAwareTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.ReinitializeDefinitionNavigation();
				var triggerController = WPFTestHelper.GetPrivateField<TextDefinitionTriggerController>(editor, "_definitionTriggerController");

				editor.Dispose();

				Assert.IsTrue((bool)(WPFTestHelper.GetPrivateFieldValue(triggerController, "_isDisposed") ?? false));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void Dispose_UnsubscribesPersistenceCoordinatorCallbacks()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var coordinator = WPFTestHelper.GetPrivateField<ContentPersistenceCoordinator>(editor, "_contentPersistenceCoordinator");

				// Sanity check that the editor is subscribed while alive.
				Assert.AreEqual(1, GetEventHandlerTargets(coordinator, "ContentChangedWorkerRunCompleted").Length);
				Assert.AreEqual(1, GetEventHandlerTargets(coordinator, "TextChangedDelayed").Length);

				editor.Dispose();

				Assert.AreEqual(0, GetEventHandlerTargets(coordinator, "ContentChangedWorkerRunCompleted").Length);
				Assert.AreEqual(0, GetEventHandlerTargets(coordinator, "TextChangedDelayed").Length);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ReplacedHoverController_IsCollectibleAfterReinitialization()
	{
		WeakReference? firstControllerReference = null;
		WPFTestHelper.RunInSta(() => firstControllerReference = CreateAndReplaceHoverController());

		Assert.IsNotNull(firstControllerReference);
		WPFTestHelper.AssertCollected(firstControllerReference, nameof(TextHoverController));
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ReplacedDiagnosticsCoordinator_IsCollectibleAfterReinitialization()
	{
		WeakReference? firstCoordinatorReference = null;
		WPFTestHelper.RunInSta(() => firstCoordinatorReference = CreateAndReplaceDiagnosticsCoordinator());

		Assert.IsNotNull(firstCoordinatorReference);
		WPFTestHelper.AssertCollected(firstCoordinatorReference, nameof(TextDiagnosticsCoordinator));
	}

	private static WeakReference CreateAndDisposeExercisedEditor()
	{
		var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
		{
			Content = "Name=Level1"
		};
		Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

		editor.RunContentChangedWorker();
		editor.SetDiagnostics([new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "warning", 0, 5)]);
		editor.Dispose();
		hostWindow.Content = null;
		hostWindow.Close();
		WPFTestHelper.PumpDispatcher(hostWindow.Dispatcher, DispatcherPriority.ContextIdle);

		return new WeakReference(editor);
	}

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

	private static WeakReference CreateAndReplaceHoverController()
	{
		var editor = new ReinitializableTextEditor();
		editor.ReinitializeHover();
		var firstController = WPFTestHelper.GetPrivateField<TextHoverController>(editor, "_hoverController");
		var reference = new WeakReference(firstController);

		editor.ReinitializeHover();
		editor.Dispose();
		return reference;
	}

	private static WeakReference CreateAndReplaceDiagnosticsCoordinator()
	{
		var editor = new ReinitializableTextEditor();
		editor.ReinitializeDiagnostics();
		var firstCoordinator = WPFTestHelper.GetPrivateField<TextDiagnosticsCoordinator>(editor, "_diagnosticsCoordinator");
		var reference = new WeakReference(firstCoordinator);

		editor.ReinitializeDiagnostics();
		editor.Dispose();
		return reference;
	}

	private static object?[] GetEventHandlerTargets(object instance, string eventFieldName)
	{
		FieldInfo? field = instance.GetType().GetField(eventFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		object? value = field?.GetValue(instance);

		return value is Delegate handler
			? handler.GetInvocationList().Select(static invocation => invocation.Target).ToArray()
			: [];
	}

	private sealed class CleanupAwareTextEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".txt";

		public bool CleanupCompleted { get; private set; }

		protected override void DisposeEditorResources()
		{
			UpdateSettings(new ClassicScriptEditorConfiguration());
			CleanupCompleted = true;
		}
	}

	private sealed class ReinitializableTextEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".txt";

		public void ReinitializeHover()
			=> InitializeHover(
				_ => new TextHoverRequestState(false, -1, false, false, null),
				(_, _) => Task.FromResult<TextHoverInfo?>(null));

		public void ReinitializeDiagnostics()
			=> InitializeDiagnostics(new Version(1, 0));
	}

	private sealed class NavigationAwareTextEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".txt";

		public void ReinitializeDefinitionNavigation()
			=> InitializeDefinitionNavigation((_, _) => Task.FromResult(true));

		public bool InvokeGoToDefinition(ITextDefinitionProvider definitionProvider, string objectName)
			=> GoToDefinition(definitionProvider, objectName);

		public bool InvokeTryGoToDefinition(ITextDefinitionProvider definitionProvider, ITextHoverProvider hoverProvider, int offset)
			=> TryGoToDefinition(definitionProvider, hoverProvider, offset);

		public bool InvokeTryShowDiagnosticToolTip(int hoveredOffset)
			=> TryShowDiagnosticToolTip(hoveredOffset);

		public bool InvokeTryHandleCtrlSpaceCompletion(TextCompositionEventArgs e, Action onTriggered)
			=> TryHandleCtrlSpaceCompletion(e, onTriggered);
	}
}

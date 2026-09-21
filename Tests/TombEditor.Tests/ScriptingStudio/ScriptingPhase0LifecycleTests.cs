#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Moq;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Presentation;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class ScriptingPhase0LifecycleTests
{
	private static IEditorControl? _collectibilityRoot;

	[TestMethod]
	public void CurrentEditorEdit_UsesUndoAndDirtyState()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript());
			try
			{
				editor.ApplyPersistedContent("original");
				editor.Document.UndoStack.ClearAll();

				Assert.IsFalse(editor.IsContentChanged);

				editor.SelectAll();
				editor.SelectedText = "edited";
				editor.RunContentChangedWorker();

				Assert.AreEqual("edited", editor.Content);
				Assert.IsTrue(editor.IsContentChanged);
				Assert.IsTrue(editor.CanUndo);

				editor.Undo();
				editor.RunContentChangedWorker();

				Assert.AreEqual("original", editor.Content);
				Assert.IsFalse(editor.IsContentChanged);
			}
			finally
			{
				editor.Dispose();
			}
		});

	[TestMethod]
	public void StringEditorView_Load_UsesSerializedContentAndCleanBaseline()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "[Strings]\r\nHello");

			var editor = new StringEditorView(new Version(1, 0));
			try
			{
				editor.Load(filePath, new(EditorProcessingMode.Suppressed));

				Assert.AreEqual(filePath, editor.FilePath);
				Assert.AreEqual(EditorProcessingMode.Normal, editor.ProcessingMode);
				Assert.IsFalse(editor.IsContentChanged);
				StringAssert.Contains(editor.Content, "Hello");
			}
			finally
			{
				editor.Dispose();
				File.Delete(filePath);
			}
		});

	[TestMethod]
	public void TextEditorBase_Load_UsesLoadedContentAndCleanBaseline()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "local value = 1");

			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript());
			try
			{
				editor.Load(filePath, new(EditorProcessingMode.Suppressed));

				Assert.AreEqual(filePath, editor.FilePath);
				Assert.AreEqual(EditorProcessingMode.Normal, editor.ProcessingMode);
				Assert.IsFalse(editor.IsContentChanged);
				Assert.AreEqual("local value = 1", editor.Content);
			}
			finally
			{
				editor.Dispose();
				File.Delete(filePath);
			}
		});

	[TestMethod]
	public void TextEditorBase_Load_CancelsQueuedPersistenceAndDelayedPublication()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "loaded");

			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript());
			int delayedNotificationCount = 0;
			editor.TextChangedDelayed += OnTextChangedDelayed;
			try
			{
				editor.FilePath = filePath;
				editor.ApplyPersistedContent("old");
				editor.SelectAll();
				editor.SelectedText = "pending";

				editor.Load(filePath, default);
				PumpFor(TimeSpan.FromMilliseconds(400.0));

				Assert.AreEqual(0, delayedNotificationCount);
				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				editor.TextChangedDelayed -= OnTextChangedDelayed;
				editor.Dispose();
				File.Delete(filePath);
				File.Delete(filePath + ".backup");
			}

			void OnTextChangedDelayed(object? sender, EventArgs e)
				=> delayedNotificationCount++;
		});

	[TestMethod]
	public void StringEditorView_Load_CancelsPersistenceAndRestoresPreviousProcessingState()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "[Strings]\r\nLoaded");

			var editor = new StringEditorView(new Version(1, 0));
			try
			{
				editor.FilePath = filePath;
				editor.ApplyPersistedContent("[Strings]\r\nOld");
				using IDisposable suppressedScope = editor.BeginProcessingScope(EditorProcessingMode.Suppressed);

				editor.Load(filePath, default);
				PumpFor(TimeSpan.FromMilliseconds(100.0));

				Assert.AreEqual(EditorProcessingMode.Suppressed, editor.ProcessingMode);
				Assert.IsFalse(editor.IsContentChanged);
				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				editor.Dispose();
				File.Delete(filePath);
				File.Delete(filePath + ".backup");
			}
		});

	[TestMethod]
	public void TextEditorBase_WorkerAfterDispose_ThrowsObjectDisposedException()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript());
			editor.Dispose();

			Assert.ThrowsException<ObjectDisposedException>(() => editor.RunContentChangedWorker());
		});

	[TestMethod]
	public void StringEditorView_WorkerAfterDispose_DoesNotThrow()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new StringEditorView(new Version(1, 0));
			editor.Dispose();

			editor.RunContentChangedWorker();
		});

	[TestMethod]
	public void ClosedEditor_DetachesStudioLifecycleHandlers()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\closed.txt"
			};
			var controller = new Mock<IEditorDocumentController>();
			controller.Setup(value => value.GetOpenEditors()).Returns([editor]);
			var coordinator = CreateLifecycleCoordinator(controller);
			int baselineSubscriptionCount = GetContentChangedWorkerSubscriptionCount(editor);

			coordinator.Attach();
			controller.Raise(value => value.FileOpened += null, editor, EventArgs.Empty);
			Assert.AreEqual(baselineSubscriptionCount + 1, GetContentChangedWorkerSubscriptionCount(editor));

			controller.Raise(value => value.EditorClosed += null, new EditorControlEventArgs(editor));

			Assert.AreEqual(baselineSubscriptionCount, GetContentChangedWorkerSubscriptionCount(editor));
			coordinator.Dispose();
			editor.Dispose();
		});

	[TestMethod]
	public void ClosedEditor_AndDisposedCoordinator_IsCollectibleWhileEditorRemainsRooted()
	{
		WeakReference coordinatorReference = StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\closed.txt"
			};
			_collectibilityRoot = editor;
			try
			{
				WeakReference reference = CreateDisposedLifecycleCoordinator(editor);
				StaTestHelper.AssertCollected(reference, "StudioEditorLifecycleCoordinator with a rooted closed editor");
				return reference;
			}
			finally
			{
				editor.Dispose();
				_collectibilityRoot = null;
			}
		});

		Assert.IsFalse(coordinatorReference.IsAlive);
	}

	[TestMethod]
	public void RepeatedLifecycleAttachDetachAndClose_DoesNotMultiplyHandlers()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new Mock<IEditorControl>();
			var controller = new Mock<IEditorDocumentController>();
			var openEditors = new List<IEditorControl> { editor.Object };
			controller.Setup(value => value.GetOpenEditors()).Returns(() => openEditors);
			var coordinator = CreateLifecycleCoordinator(controller);

			coordinator.Attach();
			coordinator.Attach();
			controller.Raise(value => value.FileOpened += null, editor.Object, EventArgs.Empty);
			controller.Raise(value => value.FileOpened += null, editor.Object, EventArgs.Empty);

			coordinator.Detach();
			coordinator.Detach();
			controller.Raise(value => value.EditorClosed += null, new EditorControlEventArgs(editor.Object));

			editor.VerifyAdd(value => value.ContentChangedWorkerRunCompleted += It.IsAny<EventHandler>(), Times.Exactly(2));
		});

	[TestMethod]
	public void DisposedCoordinator_IgnoresLateEditorPublication()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\late.txt"
			};
			var controller = new Mock<IEditorDocumentController>();
			controller.Setup(value => value.GetOpenEditors()).Returns([editor]);
			var coordinator = CreateLifecycleCoordinator(controller);
			int baselineSubscriptionCount = GetContentChangedWorkerSubscriptionCount(editor);

			coordinator.Attach();
			controller.Raise(value => value.FileOpened += null, editor, EventArgs.Empty);
			coordinator.Dispose();
			controller.Raise(value => value.FileOpened += null, editor, EventArgs.Empty);

			Assert.AreEqual(baselineSubscriptionCount, GetContentChangedWorkerSubscriptionCount(editor));
			editor.Dispose();
		});

	[TestMethod]
	public void ClosedEditor_IgnoresLateContentPublicationBeforeCoordinatorDisposal()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = new Mock<IEditorControl>();
			var controller = new Mock<IEditorDocumentController>();
			controller.Setup(value => value.GetOpenEditors()).Returns([editor.Object]);
			var messenger = new WeakReferenceMessenger();
			var recipient = new CommandStateRefreshRecipient();
			messenger.Register<CommandStateRefreshMessage>(recipient);
			var coordinator = new StudioEditorLifecycleCoordinator(
				controller.Object,
				messenger,
				_ => { },
				_ => { },
				_ => false,
				new Mock<IKeyBindingService<UICommand>>().Object);

			coordinator.Attach();
			controller.Raise(value => value.FileOpened += null, editor.Object, EventArgs.Empty);
			controller.Raise(value => value.EditorClosed += null, new EditorControlEventArgs(editor.Object));
			editor.Raise(value => value.ContentChangedWorkerRunCompleted += null, EventArgs.Empty);

			Assert.AreEqual(0, recipient.Count);
			coordinator.Dispose();
		});

	[TestMethod]
	public void RepeatedClassicScriptActivation_DoesNotMultiplyDiagnosticsSubscriptions()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();

			builder.Activate(editor, registration);
			builder.Activate(editor, registration);

			Assert.AreEqual(1, GetDiagnosticsSubscriptionCount(editor));
		});

	[TestMethod]
	public void RepeatedClassicScriptActivation_PresentsDiagnosticsOncePerPublication()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt",
				Text = "local value = 1"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();

			builder.Activate(editor, registration);
			builder.Activate(editor, registration);

			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(
				builder.PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics)!);
			int propertyChangedCount = 0;
			diagnostics.PropertyChanged += CountDiagnosticPresentationChanges;

			try
			{
				editor.SetDiagnostics([new TextDiagnostic(TextDiagnosticSeverity.Error, "error", 0, 1)]);
			}
			finally
			{
				diagnostics.PropertyChanged -= CountDiagnosticPresentationChanges;
			}

			Assert.AreEqual(2, propertyChangedCount);

			void CountDiagnosticPresentationChanges(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName is nameof(TextDiagnosticsViewModel.SelectedItem) or nameof(TextDiagnosticsViewModel.StatusText))
					propertyChangedCount++;
			}
		});

	[TestMethod]
	public void RepeatedTrxActivation_DoesNotMultiplyDiagnosticsSubscriptions()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new TRXEditor(new Version(4, 8), ScriptingLanguageServicesTestFactory.CreateTRX())
			{
				FilePath = @"C:\Scripts\active.json5"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.TRX,
				editor,
				ScriptingDocumentConfigurationKind.TRX);
			builder.AddEditor(editor, registration);
			builder.Build();

			builder.Activate(editor, registration);
			builder.Activate(editor, registration);

			Assert.AreEqual(1, GetDiagnosticsSubscriptionCount(editor));
		});

	[TestMethod]
	public void DiagnosticsSubscription_DetachesWhenActiveEditorCloses()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();
			builder.Activate(editor, registration);

			builder.RaiseEditorClosed(editor);
			builder.SetNoDocument();

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
		});

	[TestMethod]
	public void ClosedEditor_LateDiagnosticsPublication_LeavesPaneStateUnchanged()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt",
				Text = "local value = 1"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();
			builder.Activate(editor, registration);

			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(
				builder.PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics)!);
			builder.RaiseEditorClosed(editor);
			builder.SetNoDocument();
			string statusBeforeLatePublication = diagnostics.StatusText;

			editor.SetDiagnostics([new TextDiagnostic(TextDiagnosticSeverity.Error, "late", 0, 1)]);

			Assert.AreEqual(statusBeforeLatePublication, diagnostics.StatusText);
			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
		});

	[TestMethod]
	public void SwitchingClassicScriptEditors_DetachesPreviousEditorAndAttachesCurrentEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var firstEditor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\first.txt"
			};
			var secondEditor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\second.txt"
			};
			ScriptingDocumentRegistration firstRegistration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				firstEditor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			ScriptingDocumentRegistration secondRegistration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				secondEditor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(firstEditor, firstRegistration);
			builder.AddEditor(secondEditor, secondRegistration);
			builder.Build();

			builder.Activate(firstEditor, firstRegistration);
			builder.Activate(secondEditor, secondRegistration);

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(firstEditor));
			Assert.AreEqual(1, GetDiagnosticsSubscriptionCount(secondEditor));
		});

	[TestMethod]
	public void SwitchingTextEditorToLua_DetachesTextDiagnosticsSubscription()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var textEditor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			var luaEditor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua"
			};
			ScriptingDocumentRegistration textRegistration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				textEditor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			ScriptingDocumentRegistration luaRegistration = builder.CreateRegistration(
				DocumentMode.Lua,
				luaEditor,
				ScriptingDocumentConfigurationKind.Lua);
			builder.AddEditor(textEditor, textRegistration);
			builder.AddEditor(luaEditor, luaRegistration);
			builder.Build();

			builder.Activate(textEditor, textRegistration);
			builder.Activate(luaEditor, luaRegistration);

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(textEditor));
		});

	[TestMethod]
	public void NewlyOpenedNeutralEditor_RetainsExistingSettings()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\neutral.txt",
				DefaultFontSize = 23.5,
				FontSize = 23.5
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.PlainText,
				editor,
				ScriptingDocumentConfigurationKind.None);
			builder.Build();
			builder.AddEditor(editor, registration);

			builder.RaiseFileOpened(editor);

			Assert.AreEqual(23.5, editor.DefaultFontSize);
			Assert.AreEqual(23.5, editor.FontSize);
		});

	[TestMethod]
	public void DiagnosticsSubscription_DetachesWhenContextLeavesTextEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();
			builder.Activate(editor, registration);

			builder.SetNoDocument();

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
		});

	[TestMethod]
	public void Dispose_DetachesDiagnosticsSubscriptionAndDoesNotRetainWorkbench()
	{
		WeakReference workbenchReference = StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			ScriptingDocumentRegistration registration = builder.CreateRegistration(
				DocumentMode.ClassicScript,
				editor,
				ScriptingDocumentConfigurationKind.ClassicScript);
			builder.AddEditor(editor, registration);
			builder.Build();
			builder.Activate(editor, registration);

			WeakReference reference = builder.DisposeWorkbench();

			Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
			return reference;
		});

		StaTestHelper.AssertCollected(workbenchReference, "WorkbenchService");
	}

	[TestMethod]
	public void Dispose_DoesNotRetainWorkbenchThroughStronglyRootedEditor()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = @"C:\Scripts\active.txt"
			};
			_collectibilityRoot = editor;
			try
			{
				WeakReference reference = CreateDisposedWorkbench(editor);
				Assert.AreEqual(0, GetDiagnosticsSubscriptionCount(editor));
				StaTestHelper.AssertCollected(reference, "WorkbenchService with a rooted editor");
			}
			finally
			{
				editor.Dispose();
				_collectibilityRoot = null;
			}
		});
	}

	private static WeakReference CreateDisposedWorkbench(IEditorControl editor)
	{
		var builder = new WorkbenchServiceTestBuilder();
		ScriptingDocumentRegistration registration = builder.CreateRegistration(
			DocumentMode.ClassicScript,
			editor,
			ScriptingDocumentConfigurationKind.ClassicScript);
		builder.AddEditor(editor, registration);
		builder.Build();
		builder.Activate(editor, registration);

		return builder.DisposeWorkbench();
	}

	private static int GetDiagnosticsSubscriptionCount(TextEditorBase editor)
	{
		FieldInfo? field = typeof(TextEditorBase).GetField(
			"DiagnosticsChanged",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field);

		return (field.GetValue(editor) as Delegate)?.GetInvocationList().Length ?? 0;
	}

	private static StudioEditorLifecycleCoordinator CreateLifecycleCoordinator(Mock<IEditorDocumentController> controller)
		=> new(
			controller.Object,
			new WeakReferenceMessenger(),
			_ => { },
			_ => { },
			_ => false,
			new Mock<IKeyBindingService<UICommand>>().Object);

	private static WeakReference CreateDisposedLifecycleCoordinator(IEditorControl editor)
	{
		var controller = new Mock<IEditorDocumentController>();
		var openEditors = new List<IEditorControl> { editor };
		controller.Setup(value => value.GetOpenEditors()).Returns(() => openEditors);
		var coordinator = CreateLifecycleCoordinator(controller);

		coordinator.Attach();
		controller.Raise(value => value.FileOpened += null, editor, EventArgs.Empty);
		openEditors.Remove(editor);
		controller.Raise(value => value.EditorClosed += null, new EditorControlEventArgs(editor));
		coordinator.Dispose();

		return new WeakReference(coordinator);
	}

	private static int GetContentChangedWorkerSubscriptionCount(TextEditorBase editor)
	{
		FieldInfo? field = typeof(TextEditorBase).GetField(
			"ContentChangedWorkerRunCompleted",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field);

		return (field.GetValue(editor) as Delegate)?.GetInvocationList().Length ?? 0;
	}

	private static TextDiagnosticsViewModel GetDiagnosticsViewModel(TextDiagnosticsToolWindow pane)
	{
		FieldInfo? field = typeof(TextDiagnosticsToolWindow).GetField(
			"_viewModel",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field);

		return (TextDiagnosticsViewModel)field.GetValue(pane)!;
	}

	private static void PumpFor(TimeSpan duration)
	{
		var frame = new DispatcherFrame();
		var timer = new DispatcherTimer { Interval = duration };
		timer.Tick += OnTimerTick;
		timer.Start();
		Dispatcher.PushFrame(frame);

		void OnTimerTick(object? sender, EventArgs e)
		{
			timer.Stop();
			timer.Tick -= OnTimerTick;
			frame.Continue = false;
		}
	}

	public sealed class CommandStateRefreshRecipient : IRecipient<CommandStateRefreshMessage>
	{
		public int Count { get; private set; }

		public void Receive(CommandStateRefreshMessage message)
			=> Count++;
	}
}

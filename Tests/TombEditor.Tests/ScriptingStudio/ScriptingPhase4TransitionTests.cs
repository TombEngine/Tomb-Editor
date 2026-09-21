using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.LanguageServer.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhase4TransitionTests
{
	[TestMethod]
	public void DiagnosticsPane_TransitionsAcrossLuaClassicTrxAndBack_UsesOneStablePane()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness();
			TextDiagnosticsToolWindow diagnosticsPane = harness.DiagnosticsPane;
			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(diagnosticsPane);

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.SendLuaDiagnostics("lua-initial");
			AssertDiagnostic(diagnostics, "lua-initial");

			harness.Activate(harness.ClassicScriptEditor, harness.ClassicScriptRegistration);
			harness.ClassicScriptEditor.SetDiagnostics([CreateDiagnostic("classic")]);
			AssertDiagnostic(diagnostics, "classic");

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.SendLuaDiagnostics("lua-returned");
			AssertDiagnostic(diagnostics, "lua-returned");

			harness.Activate(harness.TrxEditor, harness.TrxRegistration);
			harness.TrxEditor.SetDiagnostics([CreateDiagnostic("trx")]);
			AssertDiagnostic(diagnostics, "trx");

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.SendLuaDiagnostics("lua-final");
			AssertDiagnostic(diagnostics, "lua-final");
			Assert.AreSame(diagnosticsPane, harness.PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics));
			Assert.AreEqual("LuaDiagnostics", diagnosticsPane.SerializationKey);
		});
	}

	[TestMethod]
	public void ClassicScriptFileTransition_LateInactiveDiagnosticsCannotReplaceActiveFile()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness(includeSecondPrimaryEditors: true);
			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(harness.DiagnosticsPane);

			harness.Activate(harness.ClassicScriptEditor, harness.ClassicScriptRegistration);
			harness.ClassicScriptEditor.SetDiagnostics([CreateDiagnostic("file-a")]);
			harness.Activate(harness.SecondClassicScriptEditor!, harness.SecondClassicScriptRegistration!);
			harness.SecondClassicScriptEditor!.SetDiagnostics([CreateDiagnostic("file-b")]);

			harness.ClassicScriptEditor.SetDiagnostics([CreateDiagnostic("late-file-a")]);

			AssertDiagnostic(diagnostics, "file-b");
		});
	}

	[TestMethod]
	public void TrxFileTransition_LateInactiveDiagnosticsCannotReplaceActiveFile()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness(includeSecondPrimaryEditors: true);
			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(harness.DiagnosticsPane);

			harness.Activate(harness.TrxEditor, harness.TrxRegistration);
			harness.TrxEditor.SetDiagnostics([CreateDiagnostic("file-a")]);
			harness.Activate(harness.SecondTrxEditor!, harness.SecondTrxRegistration!);
			harness.SecondTrxEditor!.SetDiagnostics([CreateDiagnostic("file-b")]);

			harness.TrxEditor.SetDiagnostics([CreateDiagnostic("late-file-a")]);

			AssertDiagnostic(diagnostics, "file-b");
		});
	}

	[TestMethod]
	public void LuaFileTransition_LateInactiveDiagnosticsCannotReplaceActiveFile()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness(includeSecondLuaEditor: true);
			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(harness.DiagnosticsPane);

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.SendLuaDiagnostics("file-a");
			harness.Activate(harness.SecondLuaEditor!, harness.SecondLuaRegistration!);
			harness.SendLuaDiagnostics(harness.SecondLuaEditor!.FilePath, "file-b");

			harness.Messenger.Send(new LuaDiagnosticsUpdatedMessage(
				new LuaDiagnosticsPayload(
					harness.LuaEditor.FilePath,
					[CreateDiagnostic("late-file-a")] )));

			AssertDiagnostic(diagnostics, "file-b");
		});
	}

	[TestMethod]
	public void NoDocumentTransition_ClearsDiagnosticsAndReferencesWithoutReplacingPanes()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness();
			TextDiagnosticsToolWindow diagnosticsPane = harness.DiagnosticsPane;
			TextReferencesResultsToolWindow referencesPane = harness.ReferencesPane;
			TextDiagnosticsViewModel diagnostics = GetDiagnosticsViewModel(diagnosticsPane);
			TextReferencesResultsViewModel references = GetReferencesViewModel(referencesPane);

			harness.Activate(harness.ClassicScriptEditor, harness.ClassicScriptRegistration);
			harness.ClassicScriptEditor.SetDiagnostics([CreateDiagnostic("active")]);
			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.SendLuaDiagnostics("lua-active");
			harness.ReferenceCompletion.SetResult([
				new TextReferenceLocation(harness.LuaEditor.FilePath, new TextPositionRange(new TextPosition(0, 0), new TextPosition(0, 4)))]);
			StartReferenceSearch(harness.Workbench, harness.LuaEditor).GetAwaiter().GetResult();
			Assert.AreEqual(1, references.Groups.Count);

			harness.SetNoDocument();

			Assert.AreEqual(0, diagnostics.Diagnostics.Cast<object>().Count());
			Assert.IsTrue(diagnostics.HasStatusText);
			Assert.AreEqual(0, references.Groups.Count);
			Assert.IsTrue(references.HasStatusText);
			Assert.AreSame(diagnosticsPane, harness.PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics));
			Assert.AreSame(referencesPane, harness.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults));
		});
	}

	[TestMethod]
	public void RapidLuaPrimaryLuaTransition_RejectsLateReferenceResultForReactivatedEditor()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness();
			TextReferencesResultsViewModel references = GetReferencesViewModel(harness.ReferencesPane);

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			Task referenceTask = StartReferenceSearch(harness.Workbench, harness.LuaEditor);
			Assert.IsTrue(harness.ReferenceCancellationToken.HasValue);

			harness.Activate(harness.ClassicScriptEditor, harness.ClassicScriptRegistration);
			Assert.IsTrue(harness.ReferenceCancellationToken!.Value.IsCancellationRequested);
			Assert.AreEqual(0, references.Groups.Count);
			Assert.IsTrue(references.HasStatusText);

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.ReferenceCompletion.SetResult([
				new TextReferenceLocation(harness.LuaEditor.FilePath, new TextPositionRange(new TextPosition(0, 0), new TextPosition(0, 4)))]);
			referenceTask.GetAwaiter().GetResult();

			Assert.AreEqual(0, references.Groups.Count);
			Assert.IsTrue(references.HasStatusText);
		});
	}

	[TestMethod]
	public void PaneIdentityAndSerializationRemainStableAcrossDocumentRebinding()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var harness = new TransitionHarness();
			TextDiagnosticsToolWindow diagnosticsPane = harness.DiagnosticsPane;
			TextReferencesResultsToolWindow referencesPane = harness.ReferencesPane;
			int initialPaneCount = harness.PaneCatalog.Panes.Count;

			harness.Activate(harness.LuaEditor, harness.LuaRegistration);
			harness.Activate(harness.ClassicScriptEditor, harness.ClassicScriptRegistration);
			harness.Activate(harness.TrxEditor, harness.TrxRegistration);
			harness.SetNoDocument();

			Assert.AreEqual(initialPaneCount, harness.PaneCatalog.Panes.Count);
			Assert.AreSame(diagnosticsPane, harness.PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics));
			Assert.AreSame(referencesPane, harness.PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults));
			Assert.AreEqual("LuaDiagnostics", diagnosticsPane.SerializationKey);
			Assert.AreEqual("LuaReferencesResults", referencesPane.SerializationKey);
		});
	}

	[TestMethod]
	public void EveryRegistrationCatalogDerivesAllowedDocumentModesExactly()
	{
		foreach (TRVersion.Game gameVersion in new[]
		{
			TRVersion.Game.TR1,
			TRVersion.Game.TR2,
			TRVersion.Game.TR4,
			TRVersion.Game.TombEngine
		})
		{
			using var scriptDirectory = new TemporaryScriptDirectory();
			ScriptingWorkspaceProfile profile = CreateProfileFromSelector(gameVersion, scriptDirectory.Path);
			DocumentMode[] expectedModes = profile.DocumentRegistrations
				.SelectMany(registration => registration.SupportedDocumentModes)
				.Distinct()
				.ToArray();

			CollectionAssert.AreEquivalent(expectedModes, profile.AllowedDocumentModes.ToArray());
		}
	}

	[TestMethod]
	public void Phase3CompatibilitySurfacesRemainAbsent()
	{
		Assert.IsNull(typeof(ScriptingDocumentContributions).GetMethod("For"));
		Assert.IsNull(typeof(IEditorDocumentController).GetMethod("GetDocumentMode"));
		Assert.IsNull(typeof(ScriptingWorkspaceSettingsPage).GetProperty("DocumentModes"));
		Assert.IsNull(typeof(ScriptingWorkspaceSettingsPage).GetMethod("Matches"));

		string[] obsoleteRegistrationMethods =
		[
			"RegisterJson5Editor",
			"RegisterLuaEditor",
			"RegisterPlainTextEditor",
			"RegisterStringsEditor",
			"RegisterTextEditor"
		];

		foreach (string methodName in obsoleteRegistrationMethods)
			Assert.IsNull(typeof(IEditorDocumentController).GetMethod(methodName));
	}

	private static TextDiagnosticsViewModel GetDiagnosticsViewModel(TextDiagnosticsToolWindow pane)
		=> (TextDiagnosticsViewModel)((TombLib.Scripting.UI.Presentation.TextDiagnosticsView)pane.Content).DataContext;

	private static TextReferencesResultsViewModel GetReferencesViewModel(TextReferencesResultsToolWindow pane)
		=> (TextReferencesResultsViewModel)((TombLib.Scripting.UI.Presentation.TextReferencesResultsView)pane.Content).DataContext;

	private static TextDiagnostic CreateDiagnostic(string message)
		=> new(TextDiagnosticSeverity.Error, message, 0, 1);

	private static Task StartReferenceSearch(WorkbenchService workbench, LuaEditor editor)
	{
		MethodInfo? method = typeof(WorkbenchService).GetMethod(
			"FindLuaReferencesAsync",
			BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(method);
		return (Task)method.Invoke(workbench, [editor])!;
	}

	private static void AssertDiagnostic(TextDiagnosticsViewModel diagnostics, string message)
	{
		TextDiagnosticListItem item = (TextDiagnosticListItem)diagnostics.Diagnostics.Cast<object>().Single();
		Assert.AreEqual(message, item.Message);
	}

	private static ScriptingWorkspaceProfile CreateProfileFromSelector(
		TRVersion.Game gameVersion,
		string scriptDirectoryPath)
		=> ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, true, scriptDirectoryPath);

	private sealed class TransitionHarness : IDisposable
	{
		private readonly List<IEditorControl> _openEditors = [];
		private readonly Dictionary<IEditorControl, ScriptingDocumentRegistration> _registrations = [];
		private long _generation;

		public TransitionHarness(
			bool includeSecondPrimaryEditors = false,
			bool includeSecondLuaEditor = false)
		{
			Messenger = new WeakReferenceMessenger();
			Controller = new Mock<IEditorDocumentController>();
			ReferencesProvider = new Mock<ILanguageServerReferencesProvider>();
			ReferenceCompletion = new TaskCompletionSource<IReadOnlyList<TextReferenceLocation>>(
				TaskCreationOptions.RunContinuationsAsynchronously);

			LuaEditor = CreateLuaEditor("active.lua");
			ClassicScriptEditor = CreateClassicScriptEditor("active.txt");
			TrxEditor = CreateTrxEditor("active.json5");
			LuaRegistration = CreateRegistration(DocumentMode.Lua, LuaEditor, ScriptingDocumentConfigurationKind.Lua);
			ClassicScriptRegistration = CreateRegistration(DocumentMode.ClassicScript, ClassicScriptEditor, ScriptingDocumentConfigurationKind.ClassicScript);
			TrxRegistration = CreateRegistration(DocumentMode.TRX, TrxEditor, ScriptingDocumentConfigurationKind.TRX);
			AddEditor(LuaEditor, LuaRegistration);
			AddEditor(ClassicScriptEditor, ClassicScriptRegistration);
			AddEditor(TrxEditor, TrxRegistration);

			if (includeSecondPrimaryEditors)
			{
				SecondClassicScriptEditor = CreateClassicScriptEditor("second.txt");
				SecondClassicScriptRegistration = CreateRegistration(
					DocumentMode.ClassicScript,
					SecondClassicScriptEditor,
					ScriptingDocumentConfigurationKind.ClassicScript);
				SecondTrxEditor = CreateTrxEditor("second.json5");
				SecondTrxRegistration = CreateRegistration(
					DocumentMode.TRX,
					SecondTrxEditor,
					ScriptingDocumentConfigurationKind.TRX);
				AddEditor(SecondClassicScriptEditor, SecondClassicScriptRegistration);
				AddEditor(SecondTrxEditor, SecondTrxRegistration);
			}

			if (includeSecondLuaEditor)
			{
				SecondLuaEditor = CreateLuaEditor("second.lua");
				SecondLuaRegistration = CreateRegistration(
					DocumentMode.Lua,
					SecondLuaEditor,
					ScriptingDocumentConfigurationKind.Lua);
				AddEditor(SecondLuaEditor, SecondLuaRegistration);
			}

			ITextEditorHost textEditorHost = CreateTextEditorHost();
			var intellisenseProvider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
			intellisenseProvider.Setup(provider => provider.GetDiagnostics(It.IsAny<string>())).Returns([]);
			intellisenseProvider.Setup(provider => provider.GetSemanticTokens(It.IsAny<string>())).Returns([]);
			var trackedDocumentState = new LuaTrackedDocumentStateService(textEditorHost, intellisenseProvider.Object);

			ConfigureController();
			ReferencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
			ReferencesProvider
				.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
				.Callback<TextReferenceRequest, CancellationToken>((_, cancellationToken) => ReferenceCancellationToken = cancellationToken)
				.Returns((TextReferenceRequest _, CancellationToken _) => ReferenceCompletion.Task);

			ScriptingWorkspaceProfile profile = ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(
				viewCommands: [UICommand.LuaDiagnostics, UICommand.LuaReferencesResults],
				supportsLuaActivation: true);
			PaneCatalog = new PaneCatalog([
				new DocumentDiagnosticsPaneProvider(profile, Controller.Object),
				new LuaReferencesPaneProvider(profile, Controller.Object)]);

			var dockHost = new Mock<IAvalonDockHost>();
			dockHost.SetupGet(host => host.View).Returns(Mock.Of<FrameworkElement>());
			dockHost.SetupGet(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
			dockHost.Setup(host => host.SaveLayout()).Returns(string.Empty);

			var project = new Mock<IGameProject>();
			project.Setup(value => value.GetEngineRootDirectoryPath()).Returns("C:\\Engine");
			project.Setup(value => value.GetEngineExecutableFilePath()).Returns("C:\\Engine\\Game.exe");
			var projectContext = new Mock<IScriptingProjectContext>();
			projectContext.Setup(value => value.Project).Returns(project.Object);

			var findAndReplaceViewModel = new FindAndReplaceViewModel(
				Controller.Object,
				Messenger,
				new FindReplaceService());
			var luaWorkspaceCommandService = new TextWorkspaceCommandService(
				new TextWorkspaceEditApplier(textEditorHost),
				new Mock<ILanguageServerRenameProvider>().Object);

			Workbench = WorkbenchServiceTestFactory.Create(
				profile,
				projectContext.Object,
				Messenger,
				new Mock<IMessageService>().Object,
				CreateKeyBindingService(),
				ScriptingStudioChromeTestFixture.CreateMenuServiceMock().Object,
				ScriptingStudioChromeTestFixture.CreateToolBarServiceMock().Object,
				ScriptingStudioChromeTestFixture.CreateStatusBarServiceMock().Object,
				new Mock<IPaneHostService>().Object,
				new Mock<IWin32DialogOwnerProvider>().Object,
				Controller.Object,
				dockHost.Object,
				PaneCatalog,
				findAndReplaceViewModel,
				new LuaHostServices(
					new Mock<ILuaEditorLifecycleService>().Object,
					new Mock<ILuaIntellisenseBridge>().Object,
					trackedDocumentState,
					new LuaReferenceSearchService(textEditorHost, ReferencesProvider.Object, @"C:\Scripts"),
					luaWorkspaceCommandService),
				new Mock<MvvmDialogs.IDialogService>().Object,
				() => false,
				() => false,
				ScriptingLanguageServicesTestFactory.CreateClassicScript(),
				ScriptingLanguageServicesTestFactory.CreateGameFlowScript(),
				ScriptingLanguageServicesTestFactory.CreateTRX());

			DiagnosticsPane = PaneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics)!;
			ReferencesPane = PaneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)!;
		}

		public WeakReferenceMessenger Messenger { get; }

		public Mock<IEditorDocumentController> Controller { get; }

		public Mock<ILanguageServerReferencesProvider> ReferencesProvider { get; }

		public TaskCompletionSource<IReadOnlyList<TextReferenceLocation>> ReferenceCompletion { get; }

		public CancellationToken? ReferenceCancellationToken { get; private set; }

		public PaneCatalog PaneCatalog { get; }

		public WorkbenchService Workbench { get; }

		public TextDiagnosticsToolWindow DiagnosticsPane { get; }

		public TextReferencesResultsToolWindow ReferencesPane { get; }

		public LuaEditor LuaEditor { get; }

		public ScriptingDocumentRegistration LuaRegistration { get; }

		public ClassicScriptEditor ClassicScriptEditor { get; }

		public ScriptingDocumentRegistration ClassicScriptRegistration { get; }

		public TRXEditor TrxEditor { get; }

		public ScriptingDocumentRegistration TrxRegistration { get; }

		public LuaEditor? SecondLuaEditor { get; }

		public ScriptingDocumentRegistration? SecondLuaRegistration { get; }

		public ClassicScriptEditor? SecondClassicScriptEditor { get; }

		public ScriptingDocumentRegistration? SecondClassicScriptRegistration { get; }

		public TRXEditor? SecondTrxEditor { get; }

		public ScriptingDocumentRegistration? SecondTrxRegistration { get; }

		public void Activate(IEditorControl editor, ScriptingDocumentRegistration registration)
		{
			ControllerCurrentEditor = editor;
			ControllerCurrentContext = new ScriptingDocumentContext(
				++_generation,
				editor,
				editor.FilePath,
				registration);
			Controller.Raise(controller => controller.CurrentEditorChanged += null,
				new ScriptingDocumentContextChangedEventArgs(ControllerCurrentContext));
		}

		public void SetNoDocument()
		{
			ControllerCurrentEditor = null;
			ControllerCurrentContext = new ScriptingDocumentContext(++_generation, null, null, null);
			Controller.Raise(controller => controller.CurrentEditorChanged += null,
				new ScriptingDocumentContextChangedEventArgs(ControllerCurrentContext));
		}

		public void SendLuaDiagnostics(string message)
			=> SendLuaDiagnostics(LuaEditor.FilePath, message);

		public void SendLuaDiagnostics(string filePath, string message)
			=> Messenger.Send(new LuaDiagnosticsUpdatedMessage(
				new LuaDiagnosticsPayload(filePath, [CreateDiagnostic(message)])));

		public void Dispose()
		{
			Workbench.Dispose();

			foreach (IEditorControl editor in _openEditors)
				editor.Dispose();
		}

		private IEditorControl? ControllerCurrentEditor { get; set; }

		private ScriptingDocumentContext ControllerCurrentContext { get; set; } = ScriptingDocumentContext.Empty;

		private void ConfigureController()
		{
			Controller.SetupGet(controller => controller.CurrentEditor).Returns(() => ControllerCurrentEditor);
			Controller.SetupGet(controller => controller.CurrentDocumentContext).Returns(() => ControllerCurrentContext);
			Controller.SetupGet(controller => controller.ScriptRootDirectoryPath).Returns(@"C:\Scripts");
			Controller.Setup(controller => controller.GetOpenEditors()).Returns(() => _openEditors);
			Controller.Setup(controller => controller.GetDocumentRegistration(It.IsAny<IEditorControl?>()))
				.Returns((IEditorControl? editor) => editor is not null && _registrations.TryGetValue(editor, out ScriptingDocumentRegistration? registration)
					? registration
					: null);
			Controller.Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
				.Returns((string filePath) => _openEditors.Where(editor =>
					string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase)));
			Controller.Setup(controller => controller.IsEveryDocumentSaved()).Returns(true);
		}

		private void AddEditor(IEditorControl editor, ScriptingDocumentRegistration registration)
		{
			_openEditors.Add(editor);
			_registrations.Add(editor, registration);
		}

		private static ScriptingDocumentRegistration CreateRegistration(
			DocumentMode documentMode,
			IEditorControl editor,
			ScriptingDocumentConfigurationKind configurationKind)
			=> new(
				EditorType.Text,
				documentMode,
				_ => true,
				_ => true,
				_ => editor,
				new(null, configurationKind));

		private static LuaEditor CreateLuaEditor(string fileName)
			=> new(new Version(1, 0))
			{
				FilePath = $"C:\\Scripts\\{fileName}",
				Text = "local value = 1"
			};

		private static ClassicScriptEditor CreateClassicScriptEditor(string fileName)
			=> new(
				new Version(1, 0),
				ScriptingLanguageServicesTestFactory.CreateClassicScript())
			{
				FilePath = $"C:\\Scripts\\{fileName}",
				Text = "[Level]"
			};

		private static TRXEditor CreateTrxEditor(string fileName)
			=> new(
				new Version(4, 8),
				ScriptingLanguageServicesTestFactory.CreateTRX())
			{
				FilePath = $"C:\\Scripts\\{fileName}",
				Text = "{}"
			};

		private ITextEditorHost CreateTextEditorHost()
		{
			var textEditorHost = new Mock<ITextEditorHost>();
			textEditorHost
				.Setup(host => host.GetOpenEditors(It.IsAny<string>()))
				.Returns((string filePath) => _openEditors
					.Where(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
					.ToArray());
			return textEditorHost.Object;
		}

		private static IKeyBindingService<UICommand> CreateKeyBindingService()
			=> new KeyBindingService<UICommand>(
				new CommandCatalog<UICommand>([]),
				new KeyBindingTestStore());

	}

	private sealed class TemporaryScriptDirectory : IDisposable
	{
		public TemporaryScriptDirectory()
		{
			Path = System.IO.Directory.CreateTempSubdirectory("TombEditor-Phase4-").FullName;
			System.IO.File.WriteAllText(System.IO.Path.Combine(Path, "Script.txt"), string.Empty);
			System.IO.File.WriteAllText(System.IO.Path.Combine(Path, "gameflow.json5"), "{}");
			System.IO.File.WriteAllText(System.IO.Path.Combine(Path, "Gameflow.lua"), string.Empty);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (System.IO.Directory.Exists(Path))
				System.IO.Directory.Delete(Path, recursive: true);
		}
	}
}

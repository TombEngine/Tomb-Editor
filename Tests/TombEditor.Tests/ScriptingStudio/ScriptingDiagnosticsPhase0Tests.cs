using CommunityToolkit.Mvvm.Messaging;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using MvvmDialogs;
using Moq;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingDiagnosticsPhase0Tests
{
    [TestMethod]
    public void ActiveClassicScriptDiagnosticsCompletion_RefreshesSharedPane()
        => RunActiveDiagnosticsTest(DocumentMode.ClassicScript);

    [TestMethod]
    public void ActiveTrxDiagnosticsCompletion_RefreshesSharedPane()
        => RunActiveDiagnosticsTest(DocumentMode.TRX);

    [TestMethod]
    public void InactiveClassicScriptDiagnosticsCompletion_DoesNotReplaceActivePane()
        => RunInactiveDiagnosticsTest(DocumentMode.ClassicScript);

    [TestMethod]
    public void InactiveTrxDiagnosticsCompletion_DoesNotReplaceActivePane()
        => RunInactiveDiagnosticsTest(DocumentMode.TRX);

    [TestMethod]
    public void LuaDisabledClassicScriptProfile_ContributesDiagnosticsPane()
    {
        StaTestHelper.RunInSta(() =>
        {
            string scriptDirectoryPath = Directory.CreateTempSubdirectory("TombEditor-Phase0-").FullName;
            try
            {
                File.WriteAllText(Path.Combine(scriptDirectoryPath, "Script.txt"), string.Empty);
                ScriptingWorkspaceProfile profile = ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(TRVersion.Game.TR4, false, scriptDirectoryPath);
                var provider = new DocumentDiagnosticsPaneProvider(
                    profile,
                    new Mock<IEditorDocumentController>().Object);

                Assert.AreEqual(1, provider.GetPaneContributions().Count);
            }
            finally
            {
                Directory.Delete(scriptDirectoryPath, recursive: true);
            }
        });
    }

    private static ScriptingWorkspaceProfile CreateLuaDisabledClassicScriptProfile(string scriptDirectoryPath)
        => ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(TRVersion.Game.TR4, false, scriptDirectoryPath);

    private static void RunActiveDiagnosticsTest(DocumentMode documentMode)
    {
        StaTestHelper.RunInSta(() =>
        {
            Version engineVersion = documentMode == DocumentMode.TRX
                ? new Version(4, 8)
                : new Version(1, 0);
            IEditorControl editor = CreateEditor(documentMode, engineVersion, out ScriptingDocumentRegistration registration, out string diagnosticContent);
            editor.FilePath = documentMode == DocumentMode.TRX
                ? "C:\\Scripts\\gameflow.json5"
                : "C:\\Scripts\\Script.txt";

            var controller = new Mock<IEditorDocumentController>();
            controller.SetupGet(value => value.CurrentEditor).Returns(editor);
            controller.SetupGet(value => value.CurrentDocumentContext).Returns(
                new ScriptingDocumentContext(1, editor, editor.FilePath, registration));
            controller.SetupGet(value => value.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            controller.Setup(value => value.GetOpenEditors()).Returns([editor]);
            controller.Setup(value => value.GetDocumentRegistration(editor)).Returns(registration);
            controller.Setup(value => value.FindEditorsOfFile(It.IsAny<string>())).Returns([editor]);
            controller.Setup(value => value.IsEveryDocumentSaved()).Returns(true);

            ScriptingWorkspaceProfile profile = CreateProfile(documentMode);
            var paneCatalog = new PaneCatalog([
                new DocumentDiagnosticsPaneProvider(profile, controller.Object)]);
            var pane = paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics);
            Assert.IsNotNull(pane);
            var view = (TombLib.Scripting.UI.Presentation.TextDiagnosticsView)pane.Content;
            var viewModel = (TombLib.Scripting.UI.Presentation.TextDiagnosticsViewModel)view.DataContext;

            WorkbenchService workbench = CreateWorkbench(profile, controller.Object, paneCatalog);
            try
            {
                Assert.AreEqual(0, viewModel.Diagnostics.Cast<object>().Count());

                TextDiagnosticsCoordinator coordinator = ReplaceDiagnosticsCoordinator(editor);
                coordinator.RunErrorCheck(diagnosticContent);
                PumpUntil(() => !coordinator.IsBusy);

                Assert.AreEqual(1, GetEditorDiagnosticsCount(editor));
                Assert.AreEqual(1, viewModel.Diagnostics.Cast<object>().Count());
            }
            finally
            {
                workbench.Dispose();
                editor.Dispose();
            }
        });
    }

    private static WorkbenchService CreateWorkbench(
        ScriptingWorkspaceProfile profile,
        IEditorDocumentController documentController,
        PaneCatalog paneCatalog)
    {
        var projectContext = new Mock<IScriptingProjectContext>();
        var project = new Mock<IGameProject>();
        project.Setup(value => value.GetEngineRootDirectoryPath()).Returns("C:\\Engine");
        project.Setup(value => value.GetEngineExecutableFilePath()).Returns("C:\\Engine\\Game.exe");
        project.Setup(value => value.GetCurrentEngineVersion()).Returns(new Version(4, 8));
        projectContext.Setup(value => value.Project).Returns(project.Object);

        var menuService = new Mock<IMenuService>();
        menuService.Setup(value => value.MenuView).Returns(Mock.Of<FrameworkElement>());
        var toolBarService = new Mock<IToolBarService>();
        toolBarService.Setup(value => value.ToolBarView).Returns(Mock.Of<FrameworkElement>());
        var statusBarService = new Mock<IStatusBarService>();
        statusBarService.Setup(value => value.StatusBarView).Returns(Mock.Of<FrameworkElement>());
        var dockHost = new Mock<IAvalonDockHost>();
        dockHost.Setup(value => value.View).Returns(Mock.Of<FrameworkElement>());
        dockHost.Setup(value => value.Dispatcher).Returns(Dispatcher.CurrentDispatcher);

        var messenger = new WeakReferenceMessenger();
        var keyBindingService = new KeyBindingService<UICommand>(
            new CommandCatalog<UICommand>([]),
            new KeyBindingTestStore());
        var findAndReplaceViewModel = new FindAndReplaceViewModel(
            documentController,
            messenger,
            new FindReplaceService());

        return WorkbenchServiceTestFactory.Create(
            profile,
            projectContext.Object,
            messenger,
            new Mock<IMessageService>().Object,
            keyBindingService,
            menuService.Object,
            toolBarService.Object,
            statusBarService.Object,
            new Mock<IPaneHostService>().Object,
            new Mock<IWin32DialogOwnerProvider>().Object,
            documentController,
            dockHost.Object,
            paneCatalog,
            findAndReplaceViewModel,
            null,
            new Mock<IDialogService>().Object,
            () => false,
            () => false,
            ScriptingLanguageServicesTestFactory.CreateClassicScript(),
            ScriptingLanguageServicesTestFactory.CreateGameFlowScript(),
            ScriptingLanguageServicesTestFactory.CreateTRX());
    }

    private static void RunInactiveDiagnosticsTest(DocumentMode inactiveDocumentMode)
    {
        StaTestHelper.RunInSta(() =>
        {
            var activeEditor = new TRXEditor(new Version(4, 8), ScriptingLanguageServicesTestFactory.CreateTRX())
            {
                FilePath = "C:\\Scripts\\active.json5"
            };
            IEditorControl inactiveEditor = CreateEditor(
                inactiveDocumentMode,
                inactiveDocumentMode == DocumentMode.TRX ? new Version(4, 8) : new Version(1, 0),
                out ScriptingDocumentRegistration inactiveRegistration,
                out string diagnosticContent);
            inactiveEditor.FilePath = inactiveDocumentMode == DocumentMode.TRX
                ? "C:\\Scripts\\inactive.json5"
                : "C:\\Scripts\\Script.txt";
            var activeRegistration = CreateRegistration(
                DocumentMode.TRX,
                activeEditor,
                new(ScriptingSettingsPageKind.TRX, ScriptingDocumentConfigurationKind.TRX));
            activeEditor.SetDiagnostics([new TextDiagnostic(TextDiagnosticSeverity.Warning, "active", 0, 1)]);

            var controller = new Mock<IEditorDocumentController>();
            controller.SetupGet(value => value.CurrentEditor).Returns(activeEditor);
            controller.SetupGet(value => value.CurrentDocumentContext).Returns(
                new ScriptingDocumentContext(1, activeEditor, activeEditor.FilePath, activeRegistration));
            controller.SetupGet(value => value.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            controller.Setup(value => value.GetOpenEditors()).Returns([activeEditor, inactiveEditor]);
            controller.Setup(value => value.GetDocumentRegistration(activeEditor)).Returns(activeRegistration);
            controller.Setup(value => value.GetDocumentRegistration(inactiveEditor)).Returns(inactiveRegistration);
            controller.Setup(value => value.FindEditorsOfFile(It.IsAny<string>())).Returns([activeEditor]);
            controller.Setup(value => value.IsEveryDocumentSaved()).Returns(true);

            ScriptingWorkspaceProfile profile = CreateProfile(DocumentMode.TRX);
            var paneCatalog = new PaneCatalog([
                new DocumentDiagnosticsPaneProvider(profile, controller.Object)]);
            var pane = paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics);
            Assert.IsNotNull(pane);
            var view = (TombLib.Scripting.UI.Presentation.TextDiagnosticsView)pane.Content;
            var viewModel = (TombLib.Scripting.UI.Presentation.TextDiagnosticsViewModel)view.DataContext;

            WorkbenchService workbench = CreateWorkbench(profile, controller.Object, paneCatalog);
            try
            {
                Assert.AreEqual(1, viewModel.Diagnostics.Cast<object>().Count());

                TextDiagnosticsCoordinator coordinator = ReplaceDiagnosticsCoordinator(inactiveEditor);
                coordinator.RunErrorCheck(diagnosticContent);
                PumpUntil(() => !coordinator.IsBusy);

                Assert.AreEqual(1, viewModel.Diagnostics.Cast<object>().Count());
                TextDiagnosticListItem item = (TextDiagnosticListItem)viewModel.Diagnostics.Cast<object>().Single();
                Assert.AreEqual("active", item.Message);
            }
            finally
            {
                workbench.Dispose();
                activeEditor.Dispose();
                inactiveEditor.Dispose();
            }
        });
    }

    private static IEditorControl CreateEditor(
        DocumentMode documentMode,
        Version engineVersion,
        out ScriptingDocumentRegistration registration,
        out string diagnosticContent)
    {
        if (documentMode == DocumentMode.TRX)
        {
            var editor = new TRXEditor(engineVersion, ScriptingLanguageServicesTestFactory.CreateTRX());
            diagnosticContent = "{\n  \"file\": \"obsolete\"\n}";
            registration = CreateRegistration(
                documentMode,
                editor,
                new(ScriptingSettingsPageKind.TRX, ScriptingDocumentConfigurationKind.TRX));
            return editor;
        }

        var classicEditor = new ClassicScriptEditor(
            engineVersion,
            ScriptingLanguageServicesTestFactory.CreateClassicScript());
        diagnosticContent = "[Bogus]";
        registration = CreateRegistration(
            documentMode,
            classicEditor,
            new(ScriptingSettingsPageKind.ClassicScript, ScriptingDocumentConfigurationKind.ClassicScript));
        return classicEditor;
    }

    private static ScriptingDocumentRegistration CreateRegistration(
        DocumentMode documentMode,
        IEditorControl editor,
        ScriptingDocumentContributions contributions)
        => new(
            EditorType.Text,
            documentMode,
            _ => true,
            _ => true,
            _ => editor,
            contributions);

    private static ScriptingWorkspaceProfile CreateProfile(DocumentMode documentMode, bool includeDiagnosticsView = true)
    {
        ScriptingWorkspaceKind workspaceKind = documentMode == DocumentMode.TRX
            ? ScriptingWorkspaceKind.TRX
            : ScriptingWorkspaceKind.ClassicScript;
        TRVersion.Game gameVersion = documentMode == DocumentMode.TRX
            ? TRVersion.Game.TR1
            : TRVersion.Game.TR4;
        string fileExplorerFilter = documentMode == DocumentMode.TRX ? "*.json5" : "*.txt";
        string commentPrefix = documentMode == DocumentMode.TRX ? "//" : ";";

        return new ScriptingWorkspaceProfile(
            workspaceKind,
            gameVersion,
            [],
            string.Empty,
            includeDiagnosticsView
                ? [new ScriptingWorkspaceViewContribution(UICommand.LuaDiagnostics)]
                : [],
            [],
            [],
            [],
            [],
            fileExplorerFilter,
            string.Empty,
            commentPrefix,
            supportsBuild: false,
            supportsDocumentation: false,
            new ScriptingWorkspaceLayoutPersistence(
                new DockPanelState(),
                () => new DockPanelState(),
                () => string.Empty,
                _ => { }));
    }

    private static TextDiagnosticsCoordinator GetDiagnosticsCoordinator(IEditorControl editor)
    {
        FieldInfo? field = typeof(TextEditorBase).GetField(
            "_diagnosticsCoordinator",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field);
        Assert.IsNotNull(field.GetValue(editor));
        return (TextDiagnosticsCoordinator)field.GetValue(editor)!;
    }

    private static TextDiagnosticsCoordinator ReplaceDiagnosticsCoordinator(IEditorControl editor)
    {
        FieldInfo? field = typeof(TextEditorBase).GetField(
            "_diagnosticsCoordinator",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field);

        TextDiagnosticsCoordinator currentCoordinator = GetDiagnosticsCoordinator(editor);
        currentCoordinator.Dispose();
        var replacementCoordinator = new TextDiagnosticsCoordinator(
            (TextEditorBase)editor,
            new FixedDiagnosticsProvider());
        field.SetValue(editor, replacementCoordinator);
        return replacementCoordinator;
    }

    private static int GetEditorDiagnosticsCount(IEditorControl editor)
        => ((TextEditorBase)editor).Diagnostics.Count;

    private static void PumpUntil(Func<bool> condition)
    {
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        int attempts = 0;

        while (!condition() && attempts++ < 5000)
            dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        Assert.IsTrue(condition());
    }

    private sealed class FixedDiagnosticsProvider : ITextDiagnosticsProvider
    {
        public IReadOnlyList<TextDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
            => [new TextDiagnostic(TextDiagnosticSeverity.Error, "completed", 0, 1)];
    }
}

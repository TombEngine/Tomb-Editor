using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using TombLib.WPF.Services.Abstract;
using TombIDE.ScriptingStudio.Messaging;
using TombLib.Scripting.Lua;
using static TombEditor.Tests.ScriptingStudio.ScriptingStudioChromeTestFixture;
using static TombEditor.Tests.ScriptingStudio.ScriptingWorkspaceProfileTestFactory;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class WorkbenchServiceTests
{
    [TestMethod]
    public void Constructor_WithNullComposition_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() => new WorkbenchService(null!));
        });
    }

    private static IScriptingProjectContext CreateProjectContext()
    {
        var contextMock = new Mock<IScriptingProjectContext>();
        contextMock.Setup(c => c.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var projectMock = new Mock<IGameProject>();
        projectMock.Setup(p => p.GetCurrentEngineVersion()).Returns(new Version(1, 0));
        projectMock.Setup(p => p.GetEngineRootDirectoryPath()).Returns("C:\\Engine");
        projectMock.Setup(p => p.GetEngineExecutableFilePath()).Returns("C:\\Engine\\Game.exe");
        contextMock.Setup(c => c.Project).Returns(projectMock.Object);

        return contextMock.Object;
    }

    private static IKeyBindingService<UICommand> CreateKeyBindingService()
    {
        return new KeyBindingService<UICommand>(
            CreateDefaultCatalog(),
            new KeyBindingOverrideCollection(),
            _ => true);
    }

    private static CommandCatalog<UICommand> CreateDefaultCatalog()
    {
        return new CommandCatalog<UICommand>([
            new CommandDescriptor<UICommand>(UICommand.Save, nameof(UICommand.Save), isRemappable: true, isHostReserved: false,
                new KeyCombo(System.Windows.Input.Key.S, System.Windows.Input.ModifierKeys.Control)),
            new CommandDescriptor<UICommand>(UICommand.Find, nameof(UICommand.Find), isRemappable: true, isHostReserved: false,
                new KeyCombo(System.Windows.Input.Key.F, System.Windows.Input.ModifierKeys.Control),
                new KeyCombo(System.Windows.Input.Key.H, System.Windows.Input.ModifierKeys.Control))
        ]);
    }

    private static IEditorDocumentController CreateDocumentControllerMock()
    {
        var mock = new Mock<IEditorDocumentController>();
        mock.Setup(c => c.GetOpenEditors()).Returns([]);
        return mock.Object;
    }

    private static IMenuService CreateMenuServiceMock()
        => ScriptingStudioChromeTestFixture.CreateMenuServiceMock().Object;

    private static IToolBarService CreateToolBarServiceMock()
        => ScriptingStudioChromeTestFixture.CreateToolBarServiceMock().Object;

    private static IStatusBarService CreateStatusBarServiceMock()
        => ScriptingStudioChromeTestFixture.CreateStatusBarServiceMock().Object;

    private static IAvalonDockHost CreateDockHostMock()
    {
        var mock = new Mock<IAvalonDockHost>();
        mock.Setup(d => d.View).Returns(Mock.Of<System.Windows.FrameworkElement>());
        mock.Setup(d => d.Dispatcher).Returns(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        return mock.Object;
    }

    private static PaneCatalog CreatePaneCatalog()
    {
        return new PaneCatalog([]);
    }

    private static FindAndReplaceViewModel CreateFindAndReplaceViewModel()
    {
        var messenger = new Mock<IMessenger>().Object;
        var documentController = CreateDocumentControllerMock();
        return new FindAndReplaceViewModel(documentController, messenger, new FindReplaceService());
    }

    private static ILuaEditorLifecycleService CreateLuaEditorLifecycleServiceMock()
    {
        return new Mock<ILuaEditorLifecycleService>().Object;
    }

    private static ILuaIntellisenseBridge CreateLuaIntellisenseBridgeMock()
    {
        return new Mock<ILuaIntellisenseBridge>().Object;
    }

    private static LuaTrackedDocumentStateService CreateLuaTrackedDocumentStateService()
    {
        return new LuaTrackedDocumentStateService(
            new Mock<ITextEditorHost>().Object,
            new Mock<ILuaIntelliSenseProvider>().Object);
    }

    private static LuaReferenceSearchService CreateLuaReferenceSearchService()
    {
        return new LuaReferenceSearchService(
            new Mock<ITextEditorHost>().Object,
            new Mock<ITextReferencesProvider>().Object,
            "C:\\Scripts");
    }

    private static TextWorkspaceCommandService CreateLuaWorkspaceCommandService()
    {
        return new TextWorkspaceCommandService(
            new TextWorkspaceEditApplier(new Mock<ITextEditorHost>().Object),
            new Mock<ITextEditProvider>().Object);
    }

    private static LuaHostServices CreateLuaHostServices()
    {
        return new LuaHostServices(
            CreateLuaEditorLifecycleServiceMock(),
            CreateLuaIntellisenseBridgeMock(),
            CreateLuaTrackedDocumentStateService(),
            CreateLuaReferenceSearchService(),
            CreateLuaWorkspaceCommandService());
    }

    private static IDialogService CreateDialogService()
        => new Mock<IDialogService>().Object;

    [TestMethod]
    public void Constructor_WithNullWorkspaceProfile_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    null!,
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullProjectContext_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    null!,
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMessenger_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    null!,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    null!,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullDocumentController_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    null!,
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullDockHost_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    null!,
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullPaneCatalog_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    null!,
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void LuaHostServices_WithNullEditorLifecycleService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new LuaHostServices(
                    null!,
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    CreateLuaReferenceSearchService(),
                    CreateLuaWorkspaceCommandService()));
        });
    }

    [TestMethod]
    public void LuaHostServices_WithNullIntellisenseBridge_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new LuaHostServices(
                    CreateLuaEditorLifecycleServiceMock(),
                    null!,
                    CreateLuaTrackedDocumentStateService(),
                    CreateLuaReferenceSearchService(),
                    CreateLuaWorkspaceCommandService()));
        });
    }

    [TestMethod]
    public void LuaHostServices_WithNullTrackedDocumentStateService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new LuaHostServices(
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    null!,
                    CreateLuaReferenceSearchService(),
                    CreateLuaWorkspaceCommandService()));
        });
    }

    [TestMethod]
    public void LuaHostServices_WithNullReferenceSearchService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new LuaHostServices(
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    null!,
                    CreateLuaWorkspaceCommandService()));
        });
    }

    [TestMethod]
    public void LuaHostServices_WithNullWorkspaceCommandService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new LuaHostServices(
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    CreateLuaReferenceSearchService(),
                    null!));
        });
    }

    [TestMethod]
    public void LuaCapableProfile_WithNullLuaHostServices_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    null,
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullLanguageServices_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    null!,
                    ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullGameFlowLanguageServices_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                WorkbenchServiceTestFactory.Create(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateKeyBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaHostServices(),
                    CreateDialogService(),
                    () => false,
                    () => false,
                    ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                    null!,
                    ScriptingLanguageServicesTestFactory.CreateTRXStub()));
        });
    }

    [TestMethod]
    public void ShellRefreshMessage_ReevaluatesLuaCapabilityCommands()
    {
        StaTestHelper.RunInSta(() =>
        {
            var editor = new LuaEditor(new Version(1, 0))
            {
                FilePath = @"C:\Scripts\test.lua",
                Content = "local value = 1"
            };
            bool supportsReferences = false;
            bool supportsRename = false;
            Func<UICommand, bool>? menuCanExecute = null;
            Func<UICommand, bool>? toolBarCanExecute = null;
            var menuService = new Mock<IMenuService>();
            menuService.Setup(m => m.MenuView).Returns(Mock.Of<System.Windows.FrameworkElement>());
            menuService
                .Setup(m => m.UpdateCommandEnabledStates(It.IsAny<Func<UICommand, bool>>()))
                .Callback<Func<UICommand, bool>>(canExecute => menuCanExecute = canExecute);
            var toolBarService = new Mock<IToolBarService>();
            toolBarService.Setup(m => m.ToolBarView).Returns(Mock.Of<System.Windows.FrameworkElement>());
            toolBarService
                .Setup(m => m.UpdateCommandEnabledStates(It.IsAny<Func<UICommand, bool>>()))
                .Callback<Func<UICommand, bool>>(canExecute => toolBarCanExecute = canExecute);
            var documentController = new Mock<IEditorDocumentController>();
            documentController.SetupGet(controller => controller.CurrentEditor).Returns(editor);
            documentController.Setup(controller => controller.GetOpenEditors()).Returns([editor]);
            documentController
                .Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
                .Returns([editor]);
            var referencesProvider = new Mock<ITextReferencesProvider>();
            referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(() => supportsReferences);
            var editProvider = new Mock<ITextEditProvider>();
            editProvider.SetupGet(provider => provider.SupportsRename).Returns(() => supportsRename);
            var messenger = new WeakReferenceMessenger();
            var workbench = WorkbenchServiceTestFactory.Create(
                CreateLuaProfile(),
                CreateProjectContext(),
                messenger,
                new Mock<IMessageService>().Object,
                CreateKeyBindingService(),
                menuService.Object,
                toolBarService.Object,
                CreateStatusBarServiceMock(),
                new Mock<IPaneHostService>().Object,
                new Mock<IWin32DialogOwnerProvider>().Object,
                documentController.Object,
                CreateDockHostMock(),
                CreatePaneCatalog(),
                CreateFindAndReplaceViewModel(),
                new LuaHostServices(
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    new LuaReferenceSearchService(new Mock<ITextEditorHost>().Object, referencesProvider.Object, "C:\\Scripts"),
                    new TextWorkspaceCommandService(new TextWorkspaceEditApplier(new Mock<ITextEditorHost>().Object), editProvider.Object)),
                CreateDialogService(),
                () => false,
                () => false,
                ScriptingLanguageServicesTestFactory.CreateClassicScriptStub(),
                ScriptingLanguageServicesTestFactory.CreateGameFlowScriptStub(),
                ScriptingLanguageServicesTestFactory.CreateTRXStub());

            try
            {
                Assert.IsNotNull(menuCanExecute);
                Assert.IsNotNull(toolBarCanExecute);
                Assert.IsFalse(menuCanExecute(UICommand.FindReferences));
                Assert.IsFalse(menuCanExecute(UICommand.RenameSymbol));
                Assert.IsFalse(toolBarCanExecute(UICommand.FindReferences));
                Assert.IsFalse(toolBarCanExecute(UICommand.RenameSymbol));

                supportsReferences = true;
                supportsRename = true;
                messenger.Send(new ShellUiRefreshMessage());

                Assert.IsTrue(menuCanExecute(UICommand.FindReferences));
                Assert.IsTrue(menuCanExecute(UICommand.RenameSymbol));
                Assert.IsTrue(toolBarCanExecute(UICommand.FindReferences));
                Assert.IsTrue(toolBarCanExecute(UICommand.RenameSymbol));
            }
            finally
            {
                workbench.Dispose();
                editor.Dispose();
            }
        });
    }
}

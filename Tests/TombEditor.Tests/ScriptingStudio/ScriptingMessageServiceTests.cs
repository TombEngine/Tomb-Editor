using CommunityToolkit.Mvvm.Messaging;
using Moq;
using TombIDE.ScriptingStudio;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public class ScriptingMessageServiceTests
{
    private static readonly ClassicScriptLanguageServices ClassicLanguageServices = ScriptingLanguageServicesTestFactory.CreateClassicScript();
    private static readonly GameFlowLanguageServices GameFlowLanguageServices = ScriptingLanguageServicesTestFactory.CreateGameFlowScript();
    private static readonly TRXLanguageServices TrxLanguageServices = ScriptingLanguageServicesTestFactory.CreateTRX();

    private static ScriptingWorkspaceProfile CreateLuaWorkspaceProfile()
        => ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(fileExplorerFilter: string.Empty);

    private static ScriptingWorkspaceProfile CreateClassicScriptWorkspaceProfile()
    {
        return new ScriptingWorkspaceProfile(
            ScriptingWorkspaceKind.ClassicScript,
            TRVersion.Game.TR4,
            [],
            string.Empty,
            [],
            [],
            [],
            [],
            [],
            "*.txt",
            string.Empty,
            ";",
            supportsBuild: false,
            supportsDocumentation: false,
            new ScriptingWorkspaceLayoutPersistence(
                new DockPanelState(),
                () => new DockPanelState(),
                () => string.Empty,
                _ => { }));
    }

    private static ScriptingWorkspaceProfile CreateTrxWorkspaceProfile()
    {
        return new ScriptingWorkspaceProfile(
            ScriptingWorkspaceKind.TRX,
            TRVersion.Game.TR1,
            [],
            string.Empty,
            [],
            [],
            [],
            [],
            [],
            "*.json5",
            string.Empty,
            "//",
            supportsBuild: false,
            supportsDocumentation: false,
            new ScriptingWorkspaceLayoutPersistence(
                new DockPanelState(),
                () => new DockPanelState(),
                () => string.Empty,
                _ => { }));
    }

    private static ScriptingMessageServiceOptions CreateDefaultOptions()
    {
        return new ScriptingMessageServiceOptions
        {
            GetDockLayoutXml = () => string.Empty,
            ShowCompilerLogsPane = () => { },
            UpdateCompilerLogs = _ => { },
            ShowCompilerLogsAfterBuild = () => false,
            UseNewIncludeMethod = () => false
        };
    }

    [TestMethod]
    public void Constructor_WithValidArguments_DoesNotThrow()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Constructor_WithNullMessenger_ThrowsArgumentNullException()
    {
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                null!,
                profile,
                documentController,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_WithNullDocumentController_ThrowsArgumentNullException()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                messenger,
                profile,
                null!,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_WithNullEngineDirectoryPath_ThrowsArgumentNullException()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                messenger,
                profile,
                documentController,
                null!,
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_RegistersExpectedMessageHandlers()
    {
        var messengerMock = new Mock<IMessenger>();
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messengerMock.Object,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Dispose_UnregistersAllMessageHandlers()
    {
        var messengerMock = new Mock<IMessenger>();
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messengerMock.Object,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.Dispose();

        messengerMock.Verify(
            m => m.UnregisterAll(service),
            Times.Once);
    }

    [TestMethod]
    public void Build_DoesNotThrow_WhenAutomationProviderIsCreated()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentControllerMock = new Mock<IEditorDocumentController>();
        documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentControllerMock.Object,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.Build();
    }

    [TestMethod]
    public void ShowDocumentation_DoesNotThrow_WhenAutomationProviderIsCreated()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentControllerMock = new Mock<IEditorDocumentController>();
        documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentControllerMock.Object,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.ShowDocumentation();
    }

    [TestMethod]
    public void ApplyEditorSettings_ClassicScriptWorkspace_AppliesConfigurationForEachRegistration()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var classicEditor = new ClassicScriptEditor(new Version(1, 0), ClassicLanguageServices);
            using var luaEditor = new LuaEditor(new Version(1, 0));
            var expectedConfigurations = new ConfigurationCollection();
            double unconfiguredFontSize = expectedConfigurations.Lua.FontSize + 1.0;
            classicEditor.DefaultFontSize = unconfiguredFontSize;
            classicEditor.FontSize = unconfiguredFontSize;
            luaEditor.DefaultFontSize = unconfiguredFontSize;
            luaEditor.FontSize = unconfiguredFontSize;

            var documentControllerMock = new Mock<IEditorDocumentController>();
            documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            documentControllerMock.Setup(m => m.GetOpenEditors()).Returns([classicEditor, luaEditor]);
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(classicEditor))
                .Returns(CreateRegistration(
                    DocumentMode.ClassicScript,
                    classicEditor,
                    new(ScriptingSettingsPageKind.ClassicScript, ScriptingDocumentConfigurationKind.ClassicScript)));
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(luaEditor))
                .Returns(CreateRegistration(
                    DocumentMode.Lua,
                    luaEditor,
                    new(ScriptingSettingsPageKind.Lua, ScriptingDocumentConfigurationKind.Lua)));

            var service = new ScriptingMessageService(
                new Mock<IMessenger>().Object,
                CreateClassicScriptWorkspaceProfile(),
                documentControllerMock.Object,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices);

            try
            {
                service.ApplyEditorSettings();

                Assert.AreEqual(expectedConfigurations.ClassicScript.FontSize, classicEditor.DefaultFontSize);
                Assert.AreEqual(expectedConfigurations.Lua.FontSize, luaEditor.DefaultFontSize);
            }
            finally
            {
                service.Dispose();
            }
        });
    }

    [TestMethod]
    public void ApplyEditorSettings_TrxWorkspace_AppliesConfigurationForEachRegistration()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var trxEditor = new TRXEditor(new Version(1, 0), TrxLanguageServices);
            using var luaEditor = new LuaEditor(new Version(1, 0));
            var expectedConfigurations = new ConfigurationCollection();
            double unconfiguredFontSize = expectedConfigurations.Lua.FontSize + 1.0;
            trxEditor.DefaultFontSize = unconfiguredFontSize;
            trxEditor.FontSize = unconfiguredFontSize;
            luaEditor.DefaultFontSize = unconfiguredFontSize;
            luaEditor.FontSize = unconfiguredFontSize;

            var documentControllerMock = new Mock<IEditorDocumentController>();
            documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            documentControllerMock.Setup(m => m.GetOpenEditors()).Returns([trxEditor, luaEditor]);
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(trxEditor))
                .Returns(CreateRegistration(
                    DocumentMode.TRX,
                    trxEditor,
                    new(ScriptingSettingsPageKind.TRX, ScriptingDocumentConfigurationKind.TRX)));
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(luaEditor))
                .Returns(CreateRegistration(
                    DocumentMode.Lua,
                    luaEditor,
                    new(ScriptingSettingsPageKind.Lua, ScriptingDocumentConfigurationKind.Lua)));

            var service = new ScriptingMessageService(
                new Mock<IMessenger>().Object,
                CreateTrxWorkspaceProfile(),
                documentControllerMock.Object,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices);

            try
            {
                service.ApplyEditorSettings();

                Assert.AreEqual(expectedConfigurations.TRX.FontSize, trxEditor.DefaultFontSize);
                Assert.AreEqual(expectedConfigurations.Lua.FontSize, luaEditor.DefaultFontSize);
            }
            finally
            {
                service.Dispose();
            }
        });
    }

    [TestMethod]
    public void ApplyEditorSettings_UnregisteredEditor_RetainsExistingSettings()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var editor = new ClassicScriptEditor(new Version(1, 0), ClassicLanguageServices);
            const double sentinelFontSize = 23.5;
            editor.DefaultFontSize = sentinelFontSize;
            editor.FontSize = sentinelFontSize;

            var documentControllerMock = new Mock<IEditorDocumentController>();
            documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            documentControllerMock.Setup(m => m.GetOpenEditors()).Returns([editor]);
            documentControllerMock.Setup(m => m.GetDocumentRegistration(editor)).Returns((ScriptingDocumentRegistration?)null);

            using var service = new ScriptingMessageService(
                new Mock<IMessenger>().Object,
                CreateClassicScriptWorkspaceProfile(),
                documentControllerMock.Object,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices);

            service.ApplyEditorSettings();

            Assert.AreEqual(sentinelFontSize, editor.DefaultFontSize);
            Assert.AreEqual(sentinelFontSize, editor.FontSize);
        });
    }

    [TestMethod]
    public void ApplyEditorSettings_NeutralRegistration_RetainsExistingSettings()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var editor = new ClassicScriptEditor(new Version(1, 0), ClassicLanguageServices);
            const double sentinelFontSize = 23.5;
            editor.DefaultFontSize = sentinelFontSize;
            editor.FontSize = sentinelFontSize;

            var documentControllerMock = new Mock<IEditorDocumentController>();
            documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            documentControllerMock.Setup(m => m.GetOpenEditors()).Returns([editor]);
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(editor))
                .Returns(CreateRegistration(
                    DocumentMode.PlainText,
                    editor,
                    ScriptingDocumentContributions.None));

            using var service = new ScriptingMessageService(
                new Mock<IMessenger>().Object,
                CreateClassicScriptWorkspaceProfile(),
                documentControllerMock.Object,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices);

            service.ApplyEditorSettings();

            Assert.AreEqual(sentinelFontSize, editor.DefaultFontSize);
            Assert.AreEqual(sentinelFontSize, editor.FontSize);
        });
    }

    [TestMethod]
    public void ApplyUserSettings_ClassicScriptWorkspace_AppliesConfigurationForEachRegistration()
    {
        StaTestHelper.RunInSta(() =>
        {
            using var classicEditor = new ClassicScriptEditor(new Version(1, 0), ClassicLanguageServices);
            using var luaEditor = new LuaEditor(new Version(1, 0));
            var expectedConfigurations = new ConfigurationCollection();
            double unconfiguredFontSize = expectedConfigurations.Lua.FontSize + 1.0;
            classicEditor.DefaultFontSize = unconfiguredFontSize;
            classicEditor.FontSize = unconfiguredFontSize;
            luaEditor.DefaultFontSize = unconfiguredFontSize;
            luaEditor.FontSize = unconfiguredFontSize;

            var messenger = new WeakReferenceMessenger();
            var documentControllerMock = new Mock<IEditorDocumentController>();
            documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");
            documentControllerMock.Setup(m => m.GetOpenEditors()).Returns([classicEditor, luaEditor]);
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(classicEditor))
                .Returns(CreateRegistration(
                    DocumentMode.ClassicScript,
                    classicEditor,
                    new(ScriptingSettingsPageKind.ClassicScript, ScriptingDocumentConfigurationKind.ClassicScript)));
            documentControllerMock
                .Setup(m => m.GetDocumentRegistration(luaEditor))
                .Returns(CreateRegistration(
                    DocumentMode.Lua,
                    luaEditor,
                    new(ScriptingSettingsPageKind.Lua, ScriptingDocumentConfigurationKind.Lua)));

            var service = new ScriptingMessageService(
                messenger,
                CreateClassicScriptWorkspaceProfile(),
                documentControllerMock.Object,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices);

            try
            {
                messenger.Send(new ScriptingReloadSyntaxHighlightingRequestedMessage());

                Assert.AreEqual(expectedConfigurations.ClassicScript.FontSize, classicEditor.DefaultFontSize);
                Assert.AreEqual(expectedConfigurations.Lua.FontSize, luaEditor.DefaultFontSize);
            }
            finally
            {
                service.Dispose();
            }
        });
    }

    private static ScriptingDocumentRegistration CreateRegistration(
        DocumentMode documentMode,
        IEditorControl editor,
        ScriptingDocumentContributions contributions)
    {
        return new ScriptingDocumentRegistration(
            EditorType.Text,
            documentMode,
            _ => true,
            _ => true,
            _ => editor,
            contributions);
    }
}

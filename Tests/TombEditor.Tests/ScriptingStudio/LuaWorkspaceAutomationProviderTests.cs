using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class LuaWorkspaceAutomationProviderTests
{
    [TestMethod]
    public void ProgramClosing_DoesNotDisposeTheLanguageServerProvider()
    {
        var documentController = new Mock<IEditorDocumentController>();
        var hostOperations = new Mock<IScriptingHostOperations>();
        var callbacks = new LuaWorkspaceAutomationCallbacks(
            _ => (false, false),
            _ => false,
            _ => false,
            (_, _) => { });
        var provider = new LuaWorkspaceAutomationProvider(
            new StudioSilentActionService(documentController.Object, hostOperations.Object),
            @"C:\Scripts",
            callbacks);

        provider.HandleIDEEvent(new IDE.ProgramClosingEvent());
    }
}

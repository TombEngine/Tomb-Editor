using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.Lua;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class LuaDocumentLifecycleCoordinatorTests
{
    [TestMethod]
    public void Attach_IsIdempotent_AndRoutesOpenAndRenameOnce()
    {
        StaTestHelper.RunInSta(() =>
        {
            var editor = CreateEditor();
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var documentController = CreateDocumentController(editor);
            var trackedState = CreateTrackedState(editor, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            coordinator.Attach();

            documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
            documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
            documentController.Raise(
                controller => controller.DocumentRenamed += null,
                new DocumentRenamedEventArgs(editor.FilePath, @"C:\Scripts\renamed.lua"));

            provider.Verify(item => item.OpenDocument(editor.FilePath, editor.Text), Times.Once);
            provider.Verify(item => item.MoveDocument(
                editor.FilePath,
                @"C:\Scripts\renamed.lua",
                editor.Text), Times.Once);

            coordinator.Dispose();
            editor.Dispose();
        });
    }

    [TestMethod]
    public void Detach_StopsRoutingFurtherOpenAndRenameEvents()
    {
        StaTestHelper.RunInSta(() =>
        {
            var editor = CreateEditor();
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var documentController = CreateDocumentController(editor);
            var trackedState = CreateTrackedState(editor, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            coordinator.Detach();

            documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
            documentController.Raise(
                controller => controller.DocumentRenamed += null,
                new DocumentRenamedEventArgs(editor.FilePath, @"C:\Scripts\renamed.lua"));

            provider.Verify(item => item.OpenDocument(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            provider.Verify(item => item.MoveDocument(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);

            coordinator.Dispose();
            editor.Dispose();
        });
    }

    [TestMethod]
    public void EditorClosed_DetachesEditorLifecycleBeforeWorkspaceDisposal()
    {
        StaTestHelper.RunInSta(() =>
        {
            var editor = CreateEditor();
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var documentController = CreateDocumentController(editor);
            var trackedState = CreateTrackedState(editor, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
            documentController.Raise(
                controller => controller.EditorClosed += null,
                new EditorControlEventArgs(editor));
            documentController.Raise(
                controller => controller.DocumentRenamed += null,
                new DocumentRenamedEventArgs(editor.FilePath, @"C:\Scripts\renamed.lua"));

            provider.Verify(item => item.OpenDocument(editor.FilePath, editor.Text), Times.Once);
            provider.Verify(item => item.MoveDocument(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            coordinator.Dispose();
            editor.Dispose();
        });
    }

    [TestMethod]
    public void AttachedEditor_DisposalClosesItsProviderDocumentOnce()
    {
        StaTestHelper.RunInSta(() =>
        {
            var editor = CreateEditor();
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var documentController = CreateDocumentController(editor);
            var trackedState = CreateTrackedState(editor, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            editor.IntelliSenseProvider = provider.Object;
            editor.Dispose();

            provider.Verify(item => item.CloseDocument(editor.FilePath), Times.Once);

            coordinator.Dispose();
        });
    }

    [TestMethod]
    public void CloseAndReopen_UsesOneProviderReferencePerEditor()
    {
        StaTestHelper.RunInSta(() =>
        {
            var firstEditor = CreateEditor();
            var reopenedEditor = CreateEditor();
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var openEditors = new List<IEditorControl> { firstEditor };
            var documentController = new Mock<IEditorDocumentController>();
            documentController.Setup(controller => controller.GetOpenEditors()).Returns(() => openEditors);
            documentController
                .Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
                .Returns((string filePath) => openEditors.FindAll(editor => editor.FilePath == filePath));
            documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => openEditors.LastOrDefault());

            var trackedState = CreateTrackedState(firstEditor, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            documentController.Raise(controller => controller.FileOpened += null, firstEditor, EventArgs.Empty);
            documentController.Raise(controller => controller.EditorClosed += null, new EditorControlEventArgs(firstEditor));
            firstEditor.Dispose();

            openEditors.Clear();
            openEditors.Add(reopenedEditor);
            documentController.Raise(controller => controller.FileOpened += null, reopenedEditor, EventArgs.Empty);

            provider.Verify(item => item.OpenDocument(firstEditor.FilePath, firstEditor.Text), Times.Exactly(2));
            provider.Verify(item => item.CloseDocument(firstEditor.FilePath), Times.Once);

            coordinator.Dispose();
            reopenedEditor.Dispose();
        });
    }

    [TestMethod]
    public void MultipleEditors_ShareProviderAndCloseEachDocumentOnce()
    {
        StaTestHelper.RunInSta(() =>
        {
            var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "local first = 1");
            var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "local second = 2");
            var provider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
            var openEditors = new List<IEditorControl> { firstEditor, secondEditor };
            var documentController = new Mock<IEditorDocumentController>();
            documentController.Setup(controller => controller.GetOpenEditors()).Returns(() => openEditors);
            documentController
                .Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
                .Returns((string filePath) => openEditors.FindAll(editor => editor.FilePath == filePath));
            documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => secondEditor);
            var textEditorHost = new Mock<ITextEditorHost>();
            textEditorHost
                .Setup(host => host.GetOpenEditors(It.IsAny<string>()))
                .Returns((string filePath) => openEditors
                    .Where(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                    .ToArray());
            var trackedState = new LuaTrackedDocumentStateService(textEditorHost.Object, provider.Object);
            var coordinator = new LuaDocumentLifecycleCoordinator(
                documentController.Object,
                new WeakReferenceMessenger(),
                provider.Object,
                trackedState);

            coordinator.Attach();
            documentController.Raise(controller => controller.FileOpened += null, firstEditor, EventArgs.Empty);
            documentController.Raise(controller => controller.FileOpened += null, secondEditor, EventArgs.Empty);

            provider.Verify(item => item.OpenDocument(firstEditor.FilePath, firstEditor.Text), Times.Once);
            provider.Verify(item => item.OpenDocument(secondEditor.FilePath, secondEditor.Text), Times.Once);

            documentController.Raise(controller => controller.EditorClosed += null, new EditorControlEventArgs(firstEditor));
            documentController.Raise(controller => controller.EditorClosed += null, new EditorControlEventArgs(secondEditor));
			firstEditor.Dispose();
			secondEditor.Dispose();

            provider.Verify(item => item.CloseDocument(firstEditor.FilePath), Times.Once);
            provider.Verify(item => item.CloseDocument(secondEditor.FilePath), Times.Once);

            coordinator.Dispose();
        });
    }

    private static LuaEditor CreateEditor(string? filePath = null, string? content = null)
        => new(new Version(1, 0))
        {
            FilePath = filePath ?? @"C:\Scripts\test.lua",
            Content = content ?? "local value = 1"
        };

    private static Mock<IEditorDocumentController> CreateDocumentController(LuaEditor editor)
    {
        var mock = new Mock<IEditorDocumentController>();
        mock.Setup(controller => controller.GetOpenEditors()).Returns(new[] { editor });
        mock.Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>())).Returns(new[] { editor });
        mock.SetupGet(controller => controller.CurrentEditor).Returns(editor);
        return mock;
    }

    private static LuaTrackedDocumentStateService CreateTrackedState(
        LuaEditor editor,
        ILuaLanguageServerIntelliSenseProvider provider)
    {
        var textEditorHost = new Mock<ITextEditorHost>();
        textEditorHost
            .Setup(host => host.GetOpenEditors(It.IsAny<string>()))
            .Returns(new IEditorControl[] { editor });

        return new LuaTrackedDocumentStateService(textEditorHost.Object, provider);
    }
}

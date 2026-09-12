using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.LanguageServer.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class LuaIntellisenseEventBridgeTests
{
    [TestMethod]
    public void Attach_DispatchesStatusEventsFromInterfaceProvider()
    {
        StaTestHelper.RunInSta(() =>
        {
            var provider = new Mock<ILuaIntelliSenseProvider>();
            var messenger = new WeakReferenceMessenger();
            var recipient = new StatusMessageRecipient();
            messenger.Register<LuaStartupFailedMessage>(recipient);
            messenger.Register<LuaWorkspaceWatcherFailedMessage>(recipient);
            var bridge = CreateBridge(provider.Object, messenger);
            var startupFailure = new LanguageServerStartupFailure("startup failed", true);
            var watcherFailure = new WorkspaceWatcherFailure("watcher failed");

            bridge.Attach();
            provider.Raise(item => item.StartupFailed += null, startupFailure);
            provider.Raise(item => item.WorkspaceWatcherFailed += null, watcherFailure);

            Assert.AreEqual(startupFailure, recipient.StartupFailure);
            Assert.AreEqual(watcherFailure, recipient.WorkspaceWatcherFailure);

            bridge.Dispose();
        });
    }

    [TestMethod]
    public void Detach_StopsDispatchingStatusEvents()
    {
        StaTestHelper.RunInSta(() =>
        {
            var provider = new Mock<ILuaIntelliSenseProvider>();
            var messenger = new WeakReferenceMessenger();
            var recipient = new StatusMessageRecipient();
            messenger.Register<LuaStartupFailedMessage>(recipient);
            messenger.Register<LuaWorkspaceWatcherFailedMessage>(recipient);
            var bridge = CreateBridge(provider.Object, messenger);

            bridge.Attach();
            bridge.Detach();
            provider.Raise(item => item.StartupFailed += null, new LanguageServerStartupFailure("startup failed", false));
            provider.Raise(item => item.WorkspaceWatcherFailed += null, new WorkspaceWatcherFailure("watcher failed"));

            Assert.IsNull(recipient.StartupFailure);
            Assert.IsNull(recipient.WorkspaceWatcherFailure);

            bridge.Dispose();
        });
    }

    [TestMethod]
    public void Dispose_DetachesStatusEventsWithoutDisposingProvider()
    {
        StaTestHelper.RunInSta(() =>
        {
            var provider = new Mock<ILuaIntelliSenseProvider>();
            var messenger = new WeakReferenceMessenger();
            var recipient = new StatusMessageRecipient();
            messenger.Register<LuaStartupFailedMessage>(recipient);
            messenger.Register<LuaWorkspaceWatcherFailedMessage>(recipient);
            var bridge = CreateBridge(provider.Object, messenger);

            bridge.Attach();
            bridge.Dispose();
            provider.Raise(item => item.StartupFailed += null, new LanguageServerStartupFailure("startup failed", false));
            provider.Raise(item => item.WorkspaceWatcherFailed += null, new WorkspaceWatcherFailure("watcher failed"));

            provider.Verify(item => item.Dispose(), Times.Never);
            Assert.IsNull(recipient.StartupFailure);
            Assert.IsNull(recipient.WorkspaceWatcherFailure);
        });
    }

    [TestMethod]
    public void CapabilitiesChanged_RefreshesShellUntilDetached()
    {
        StaTestHelper.RunInSta(() =>
        {
            var provider = new Mock<ILuaIntelliSenseProvider>();
            var messenger = new WeakReferenceMessenger();
            int refreshCount = 0;
            messenger.Register<ShellUiRefreshMessage>(new RefreshRecipient(() => refreshCount++));
            var bridge = CreateBridge(provider.Object, messenger);

            bridge.Attach();
            provider.Raise(item => item.CapabilitiesChanged += null);
            Assert.AreEqual(1, refreshCount);

            bridge.Detach();
            provider.Raise(item => item.CapabilitiesChanged += null);
            Assert.AreEqual(1, refreshCount);

            bridge.Dispose();
        });
    }

    [TestMethod]
    public void Attach_DispatchesDiagnosticsAndSemanticTokensWithFileIdentity()
    {
        StaTestHelper.RunInSta(() =>
        {
            var provider = new Mock<ILuaIntelliSenseProvider>();
            var messenger = new WeakReferenceMessenger();
            var recipient = new DocumentUpdateRecipient();
            messenger.Register<LuaDiagnosticsUpdatedMessage>(recipient);
            messenger.Register<LuaSemanticTokensUpdatedMessage>(recipient);
            var bridge = CreateBridge(provider.Object, messenger);
            const string filePath = @"C:\Scripts\tracked.lua";
            var diagnostics = new[]
            {
                new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "unused", 0, 5)
            };
            var semanticTokens = new[]
            {
                new LuaSemanticToken(0, 0, 5, "variable", [])
            };

            bridge.Attach();
            provider.Raise(item => item.DiagnosticsUpdated += null, filePath, diagnostics);
            provider.Raise(item => item.SemanticTokensUpdated += null, filePath, semanticTokens);

            Assert.AreEqual(filePath, recipient.Diagnostics?.FilePath);
            CollectionAssert.AreEqual(diagnostics, recipient.Diagnostics?.Diagnostics.ToArray());
            Assert.AreEqual(filePath, recipient.SemanticTokens?.FilePath);
            CollectionAssert.AreEqual(semanticTokens, recipient.SemanticTokens?.SemanticTokens.ToArray());

            bridge.Detach();
            provider.Raise(item => item.DiagnosticsUpdated += null, @"C:\Scripts\late.lua", diagnostics);
            provider.Raise(item => item.SemanticTokensUpdated += null, @"C:\Scripts\late.lua", semanticTokens);

            Assert.AreEqual(filePath, recipient.Diagnostics?.FilePath);
            Assert.AreEqual(filePath, recipient.SemanticTokens?.FilePath);
            bridge.Dispose();
        });
    }

    private static LuaIntellisenseEventBridge CreateBridge(
        ILuaIntelliSenseProvider provider,
        IMessenger messenger)
    {
        var dockHost = new Mock<IAvalonDockHost>();
        dockHost.SetupGet(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
        return new LuaIntellisenseEventBridge(dockHost.Object, messenger, provider);
    }

    internal sealed class StatusMessageRecipient :
        IRecipient<LuaStartupFailedMessage>,
        IRecipient<LuaWorkspaceWatcherFailedMessage>
    {
        public LanguageServerStartupFailure? StartupFailure { get; private set; }

        public WorkspaceWatcherFailure? WorkspaceWatcherFailure { get; private set; }

        public void Receive(LuaStartupFailedMessage message)
            => StartupFailure = message.Value;

        public void Receive(LuaWorkspaceWatcherFailedMessage message)
            => WorkspaceWatcherFailure = message.Value;
    }

    internal sealed class RefreshRecipient(Action refresh) : IRecipient<ShellUiRefreshMessage>
    {
        public void Receive(ShellUiRefreshMessage message)
            => refresh();
    }

    [TestMethod]
    public void ExercisedWorkspace_IsCollectibleAfterDetachAndDisposal()
    {
        WorkspaceReferences? references = null;
        StaTestHelper.RunInSta(() => references = CreateAndDisposeWorkspace());

        Assert.IsNotNull(references);
        StaTestHelper.AssertCollected(references.Editor, nameof(LuaEditor));
        StaTestHelper.AssertCollected(references.Provider, nameof(ILuaIntelliSenseProvider));
        StaTestHelper.AssertCollected(references.Bridge, nameof(LuaIntellisenseEventBridge));
        StaTestHelper.AssertCollected(references.Coordinator, nameof(LuaDocumentLifecycleCoordinator));
        StaTestHelper.AssertCollected(references.Messenger, nameof(IMessenger));
    }

    [TestMethod]
    public void RepeatedWorkspaceLifecycle_IsCollectibleAfterDetachAndDisposal()
    {
        WorkspaceReferences[]? references = null;
        StaTestHelper.RunInSta(() => references =
            [
                CreateAndDisposeWorkspace(),
                CreateAndDisposeWorkspace(),
                CreateAndDisposeWorkspace()
            ]);

        Assert.IsNotNull(references);
        foreach (WorkspaceReferences workspace in references)
        {
            StaTestHelper.AssertCollected(workspace.Editor, nameof(LuaEditor));
            StaTestHelper.AssertCollected(workspace.Provider, nameof(ILuaIntelliSenseProvider));
            StaTestHelper.AssertCollected(workspace.Bridge, nameof(LuaIntellisenseEventBridge));
            StaTestHelper.AssertCollected(workspace.Coordinator, nameof(LuaDocumentLifecycleCoordinator));
            StaTestHelper.AssertCollected(workspace.Messenger, nameof(IMessenger));
        }
    }

    private static WorkspaceReferences CreateAndDisposeWorkspace()
    {
        var provider = new Mock<ILuaIntelliSenseProvider>();
        var editor = new LuaEditor(new Version(1, 0))
        {
            FilePath = @"C:\Scripts\exercised.lua",
            Text = "local value = 1"
        };
        var documentController = new Mock<IEditorDocumentController>();
        documentController.Setup(controller => controller.GetOpenEditors()).Returns(() => [editor]);
        documentController
            .Setup(controller => controller.FindEditorsOfFile(It.IsAny<string>()))
            .Returns((string filePath) => string.Equals(filePath, editor.FilePath, StringComparison.OrdinalIgnoreCase)
                ? [editor]
                : []);
        documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => editor);
        var textEditorHost = new Mock<ITextEditorHost>();
        textEditorHost
            .Setup(host => host.GetOpenEditors(It.IsAny<string>()))
            .Returns((string filePath) => string.Equals(filePath, editor.FilePath, StringComparison.OrdinalIgnoreCase)
                ? [editor]
                : []);
        var trackedState = new LuaTrackedDocumentStateService(textEditorHost.Object, provider.Object);
        var messenger = new WeakReferenceMessenger();
        var updateRecipient = new DocumentUpdateRecipient();
        messenger.Register<LuaDiagnosticsUpdatedMessage>(updateRecipient);
        messenger.Register<LuaSemanticTokensUpdatedMessage>(updateRecipient);
        var dockHost = new Mock<IAvalonDockHost>();
        dockHost.SetupGet(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
        var bridge = new LuaIntellisenseEventBridge(dockHost.Object, messenger, provider.Object);
        var coordinator = new LuaDocumentLifecycleCoordinator(
            documentController.Object,
            messenger,
            provider.Object,
            trackedState);
        Window hostWindow = CreateHostWindow(editor);

        bridge.Attach();
        coordinator.Attach();
        documentController.Raise(controller => controller.FileOpened += null, editor, EventArgs.Empty);
        provider.Raise(
            item => item.DiagnosticsUpdated += null,
            editor.FilePath,
            Array.Empty<TextEditorDiagnostic>());
        provider.Raise(
            item => item.SemanticTokensUpdated += null,
            editor.FilePath,
            new[] { new LuaSemanticToken(0, 6, 5, "variable", Array.Empty<string>()) });
        Assert.AreEqual(1, updateRecipient.DiagnosticsCount);
        Assert.AreEqual(1, updateRecipient.SemanticTokensCount);
        documentController.Raise(controller => controller.EditorClosed += null, new EditorControlEventArgs(editor));
        bridge.Dispose();
        coordinator.Dispose();
        provider.Raise(
            item => item.DiagnosticsUpdated += null,
            editor.FilePath,
            Array.Empty<TextEditorDiagnostic>());
        provider.Raise(
            item => item.SemanticTokensUpdated += null,
            editor.FilePath,
            Array.Empty<LuaSemanticToken>());
        Assert.AreEqual(1, updateRecipient.DiagnosticsCount);
        Assert.AreEqual(1, updateRecipient.SemanticTokensCount);
        editor.Dispose();
        hostWindow.Content = null;
        hostWindow.Close();
        hostWindow.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

        return new WorkspaceReferences(
            new WeakReference(editor),
            new WeakReference(provider.Object),
            new WeakReference(bridge),
            new WeakReference(coordinator),
            new WeakReference(messenger));
    }

    private static Window CreateHostWindow(LuaEditor editor)
    {
        var hostWindow = new Window
        {
            Content = editor,
            Width = 800.0,
            Height = 600.0,
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };

        hostWindow.Show();
        hostWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        return hostWindow;
    }

    internal sealed class DocumentUpdateRecipient :
        IRecipient<LuaDiagnosticsUpdatedMessage>,
        IRecipient<LuaSemanticTokensUpdatedMessage>
    {
        public LuaDiagnosticsPayload? Diagnostics { get; private set; }

        public LuaSemanticTokensPayload? SemanticTokens { get; private set; }

        public int DiagnosticsCount { get; private set; }

        public int SemanticTokensCount { get; private set; }

        public void Receive(LuaDiagnosticsUpdatedMessage message)
        {
            Diagnostics = message.Value;
            DiagnosticsCount++;
        }

        public void Receive(LuaSemanticTokensUpdatedMessage message)
        {
            SemanticTokens = message.Value;
            SemanticTokensCount++;
        }
    }

    private sealed record WorkspaceReferences(
        WeakReference Editor,
        WeakReference Provider,
        WeakReference Bridge,
        WeakReference Coordinator,
        WeakReference Messenger);
}
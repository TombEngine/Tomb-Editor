using Moq;
using MvvmDialogs;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nickelony.IDEKit.Core.Text;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Shell;
using TombLib.WPF.Services.Abstract;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class ViewModelTests
{
    // ── ReferenceBrowserViewModel ──

    [TestMethod]
    public void ReferenceBrowserViewModel_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ReferenceBrowserViewModel(
                    null!,
                    CreateLocalizationService()));
        });
    }

    [TestMethod]
    public void ReferenceBrowserViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ReferenceBrowserViewModel(
                    CreateMessageService(),
                    null!));
        });
    }

    [TestMethod]
    public void ReferenceBrowserViewModel_WithValidDependencies_InitializesCategories()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new ReferenceBrowserViewModel(
                CreateMessageService(),
                CreateLocalizationService());

            Assert.IsNotNull(viewModel.Categories);
            Assert.IsTrue(viewModel.Categories.Count > 0);
            Assert.IsNotNull(viewModel.SelectedCategory);
        });
    }

    // ── DocumentOutlineViewModel ──

    [TestMethod]
    public void DocumentOutlineViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new DocumentOutlineViewModel(null!));
        });
    }

    [TestMethod]
    public void DocumentOutlineViewModel_WithValidDependencies_HasEmptyNodes()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new DocumentOutlineViewModel(
                CreateLocalizationService());

            Assert.IsNotNull(viewModel.Nodes);
            Assert.AreEqual(0, viewModel.Nodes.Count);
            Assert.IsTrue(viewModel.IsEmpty);
        });
    }

    // ── FileExplorerViewModel ──

    [TestMethod]
    public void FileExplorerViewModel_WithNullDialogService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    null!,
                    CreateMessageService(),
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    null!,
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    CreateMessageService(),
                    null!,
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullDialogOwnerProvider_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    CreateMessageService(),
                    CreateLocalizationService(),
                    null!));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithValidDependencies_HasEmptyRootNodes()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new FileExplorerViewModel(
                CreateDialogService(),
                CreateMessageService(),
                CreateLocalizationService(),
                CreateDialogOwnerProvider());

            Assert.IsNotNull(viewModel.RootNodes);
            Assert.AreEqual(0, viewModel.RootNodes.Count);
            Assert.IsTrue(viewModel.IsEmpty);
        });
    }

    [TestMethod]
    [TestCategory("TextEditorBaseModernization")]
    public void FileExplorerViewModel_DeleteDirectory_UsesWorkspaceBridgeAndRecycleBin()
    {
        StaTestHelper.RunInSta(() =>
        {
            string rootPath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}");
            Directory.CreateDirectory(rootPath);
            string directoryPath = Path.Combine(rootPath, "Folder");
            Directory.CreateDirectory(directoryPath);

            try
            {
                var messageService = new Mock<IMessageService>();
                messageService
                    .Setup(service => service.ShowConfirmation(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool?>(),
                        It.IsAny<bool>()))
                    .Returns(true);
                var manager = new RecordingDocumentBridge();

                using var viewModel = new FileExplorerViewModel(
                    new Mock<IDialogService>().Object,
                    messageService.Object,
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider(),
                    manager)
                {
                    RootDirectoryPath = rootPath
                };
                viewModel.SelectedItem = viewModel.RootNodes[0].Children[0];

                viewModel.DeleteSelectedItemCommand.Execute(null);

                Assert.IsNotNull(manager.Request);
                Assert.AreEqual(directoryPath, manager.Request!.DirectoryPath);
                Assert.IsTrue(manager.Request.UseRecycleBin);
                Assert.AreEqual(1, manager.DeleteDirectoryCount);
                Assert.IsTrue(Directory.Exists(directoryPath));
            }
            finally
            {
                if (Directory.Exists(rootPath))
                    Directory.Delete(rootPath, true);
            }
        });
    }

    [TestMethod]
    [TestCategory("TextEditorBaseModernization")]
    public void FileExplorerViewModel_DeleteDirtyFile_RecapturesAfterSavePrompt()
    {
        StaTestHelper.RunInSta(() =>
        {
            string rootPath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}");
            Directory.CreateDirectory(rootPath);
            string filePath = Path.Combine(rootPath, "script.lua");
            File.WriteAllText(filePath, "disk");
            var snapshot = new WorkspaceDocumentSnapshot(
                new WorkspaceDocumentKey(Guid.NewGuid()),
                Path.GetFullPath(filePath),
                filePath,
                1,
                0,
                true,
                new StringTextSnapshot("logical", filePath),
                new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf),
                new FileStamp(true, 4, DateTime.UnixEpoch, "disk"));

            try
            {
                var messageService = new Mock<IMessageService>();
                messageService
                    .Setup(service => service.ShowConfirmation(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool?>(),
                        It.IsAny<bool>()))
                    .Returns(true);
                messageService
                    .Setup(service => service.ShowConfirmation<DialogResult>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        DialogResult.Yes,
                        DialogResult.No,
                        DialogResult.Cancel,
                        DialogResult.Yes,
                        It.IsAny<bool>()))
                    .Returns(DialogResult.Yes);
                var manager = new RecordingDocumentBridge(snapshot);

                using var viewModel = new FileExplorerViewModel(
                    new Mock<IDialogService>().Object,
                    messageService.Object,
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider(),
                    manager)
                {
                    RootDirectoryPath = rootPath
                };
                viewModel.SelectedItem = viewModel.RootNodes[0].Children[0];

                viewModel.DeleteSelectedItemCommand.Execute(null);

                Assert.AreEqual(3, manager.OpenCount);
                Assert.AreEqual(1, manager.CommitCount);
                Assert.AreEqual(1, manager.DeleteCount);
                Assert.IsNotNull(manager.DeleteRequest);
                Assert.AreEqual(2, manager.DeleteRequest!.ExpectedVersion);
            }
            finally
            {
                if (Directory.Exists(rootPath))
                    Directory.Delete(rootPath, true);
            }
        });
    }

    // ── Helpers ──

    private static IDialogService CreateDialogService()
        => new Mock<IDialogService>().Object;

    private static IMessageService CreateMessageService()
        => new Mock<IMessageService>().Object;

    private static ILocalizationService CreateLocalizationService()
    {
        var mock = new Mock<ILocalizationService>();
        mock.Setup(m => m[It.IsAny<string>()]).Returns((string key) => key);
        mock.Setup(m => m.Format(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => string.Format(key, args));
        mock.Setup(m => m.WithKeysFor(It.IsAny<INotifyPropertyChanged>())).Returns(mock.Object);
        return mock.Object;
    }

    private static IWin32DialogOwnerProvider CreateDialogOwnerProvider()
        => new Mock<IWin32DialogOwnerProvider>().Object;

    private sealed class RecordingDocumentBridge : IWorkspaceDocumentManager
    {
        private WorkspaceDocumentSnapshot? _snapshot;

        public RecordingDocumentBridge(WorkspaceDocumentSnapshot? snapshot = null)
        {
            _snapshot = snapshot;
        }

        public WorkspaceDocumentDirectoryDeleteRequest? Request { get; private set; }

        public WorkspaceDocumentDeleteRequest? DeleteRequest { get; private set; }

        public int DeleteDirectoryCount { get; private set; }

        public int DeleteCount { get; private set; }

        public int OpenCount { get; private set; }

        public int CommitCount { get; private set; }

        public Task<WorkspaceDocumentOpenResult> OpenAsync(
            string? filePath,
            WorkspaceDocumentOpenOptions options,
            CancellationToken cancellationToken = default)
        {
            OpenCount++;
            return Task.FromResult(new WorkspaceDocumentOpenResult(
                WorkspaceDocumentOpenStatus.AlreadyOpen,
                _snapshot));
        }

        public IReadOnlyList<WorkspaceDocumentSnapshot> GetSnapshotsUnderDirectory(string directoryPath)
            => [];

        public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
            string? filePath,
            WorkspaceDocumentOpenOptions options,
            IWorkspaceDocumentView view,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<WorkspaceDocumentConflictResolutionResult> ResolveExternalConflictAsync(
            WorkspaceDocumentConflictResolutionRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public WorkspaceDocumentManagerOpenResult OpenWithView(
            string? filePath,
            WorkspaceDocumentOpenOptions options,
            IWorkspaceDocumentView view,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public WorkspaceDocumentMutationResult Replace(WorkspaceDocumentReplaceRequest request)
            => throw new NotSupportedException();

        public WorkspaceDocumentMutationResult Discard(WorkspaceDocumentDiscardRequest request)
            => throw new NotSupportedException();

        public Task<WorkspaceDocumentRenameResult> RenameAsync(
            WorkspaceDocumentRenameRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<WorkspaceDocumentSaveAsResult> SaveAsAsync(
            WorkspaceDocumentSaveAsRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<WorkspaceDocumentDeleteResult> DeleteAsync(
            WorkspaceDocumentDeleteRequest request,
            CancellationToken cancellationToken = default)
        {
            DeleteCount++;
            DeleteRequest = request;
            return Task.FromResult(new WorkspaceDocumentDeleteResult(
                WorkspaceDocumentDeleteStatus.Deleted,
                request.ExpectedDocumentKey,
                request.DocumentId,
                request.ExpectedVersion,
                _snapshot));
        }

        public Task<WorkspaceDocumentDirectoryRenameResult> RenameDirectoryAsync(
            WorkspaceDocumentDirectoryRenameRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<WorkspaceDocumentDirectoryDeleteResult> DeleteDirectoryAsync(
            WorkspaceDocumentDirectoryDeleteRequest request,
            CancellationToken cancellationToken = default)
        {
            DeleteDirectoryCount++;
            Request = request;
            return Task.FromResult(new WorkspaceDocumentDirectoryDeleteResult(
                WorkspaceDocumentDirectoryDeleteStatus.Deleted,
                request.DirectoryPath,
                []));
        }

        public Task<WorkspaceDocumentCommitResult> CommitAsync(
            WorkspaceDocumentCommitRequest request,
            CancellationToken cancellationToken = default)
        {
            CommitCount++;
            if (_snapshot is not null)
                _snapshot = _snapshot with
                {
                    Version = _snapshot.Version + 1,
                    PersistedVersion = _snapshot.Version + 1,
                    IsDirty = false
                };

            return Task.FromResult(new WorkspaceDocumentCommitResult(
                WorkspaceDocumentCommitStatus.Committed,
                request.ExpectedDocumentKey,
                request.DocumentId,
                request.ExpectedVersion,
                _snapshot));
        }

        public Task<WorkspaceDocumentReloadResult> ReloadAsync(
            WorkspaceDocumentReloadRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void UnregisterOpenView(IWorkspaceDocumentView view)
            => throw new NotSupportedException();

        public Task StopAsync()
            => throw new NotSupportedException();

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}

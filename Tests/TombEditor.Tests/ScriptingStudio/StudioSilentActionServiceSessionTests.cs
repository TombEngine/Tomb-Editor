#nullable enable

using Moq;
using System;
using Nickelony.IDEKit.Core.Text;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;
using Nickelony.IDEKit.Workspace.Views;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class StudioSilentActionServiceSessionTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	[TestMethod]
	public void ExistingDirtyEditor_SessionCompletionDoesNotSaveOrCloseEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "C:\\Scripts\\dirty.lua";
			var editor = new Mock<IEditorControl>();
			editor.SetupGet(control => control.IsContentChanged).Returns(true);
			var documentController = new Mock<IEditorDocumentController>();
			documentController
				.Setup(controller => controller.FindEditor(filePath, EditorType.Default))
				.Returns(editor.Object);
			documentController.Setup(controller => controller.ContainsEditor(It.IsAny<IEditorControl>())).Returns(true);
			var hostOperations = new Mock<IScriptingHostOperations>();
			var viewHost = new TestViewHost();
			var documentManager = new TestDocumentBridge(CreateSnapshot(filePath));
			var service = new StudioSilentActionService(
				documentController.Object,
				hostOperations.Object,
				viewHost,
				documentManager);

			SilentActionFileState fileState = service.CaptureFileState(filePath);
			SilentActionCompletion completion = service.CreateCompletion(fileState);
			service.Complete(false, completion);

			documentController.Verify(controller => controller.SaveFile(It.IsAny<IEditorControl>()), Times.Never);
			documentController.Verify(controller => controller.TryCloseEditor(It.IsAny<IEditorControl>()), Times.Never);
			Assert.AreEqual(1, viewHost.Session.DisposeCount);
		});

	[TestMethod]
	public void ExistingCleanEditor_SessionCompletionSavesButDoesNotCloseEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "C:\\Scripts\\clean.lua";
			var editor = new Mock<IEditorControl>();
			editor.SetupGet(control => control.IsContentChanged).Returns(false);
			var documentController = new Mock<IEditorDocumentController>();
			documentController
				.Setup(controller => controller.FindEditor(filePath, EditorType.Default))
				.Returns(editor.Object);
			documentController.Setup(controller => controller.ContainsEditor(It.IsAny<IEditorControl>())).Returns(true);
			var hostOperations = new Mock<IScriptingHostOperations>();
			var viewHost = new TestViewHost();
			var documentManager = new TestDocumentBridge(CreateSnapshot(filePath));
			var service = new StudioSilentActionService(
				documentController.Object,
				hostOperations.Object,
				viewHost,
				documentManager);

			SilentActionFileState fileState = service.CaptureFileState(filePath);
			SilentActionCompletion completion = service.CreateCompletion(fileState);
			service.Complete(false, completion);

			documentController.Verify(controller => controller.SaveFile(editor.Object), Times.Once);
			documentController.Verify(controller => controller.TryCloseEditor(It.IsAny<IEditorControl>()), Times.Never);
			Assert.AreEqual(1, viewHost.Session.DisposeCount);
		});

	private static WorkspaceDocumentSnapshot CreateSnapshot(string filePath)
		=> new(
			new WorkspaceDocumentKey(Guid.NewGuid()),
			filePath,
			filePath,
			0,
			0,
			false,
			new StringTextSnapshot("canonical", filePath),
			FileFormat,
			FileStamp.Missing);

	private sealed class TestViewHost : IEditorViewHost
	{
		public TrackingSession Session { get; } = new();

		public EditorSessionOpenResult Open(WorkspaceDocumentSnapshot snapshot, EditorSessionOptions options)
			=> new(EditorSessionOpenStatus.AlreadyOpen, Session);
	}

	private sealed class TrackingSession : IEditorSession
	{
		public int DisposeCount { get; private set; }

		public WorkspaceDocumentKey DocumentKey { get; } = new(Guid.NewGuid());

		public string DocumentId => "C:\\Scripts\\dirty.lua";

		public EditorSessionMode Mode => EditorSessionMode.Transient;

		public bool IsActive => DisposeCount == 0;

		public void Dispose() => DisposeCount++;
	}

	private sealed class TestDocumentBridge(WorkspaceDocumentSnapshot snapshot) : IWorkspaceDocumentManager
	{
		private readonly WorkspaceDocumentSnapshot _snapshot = snapshot;

		public Task<WorkspaceDocumentOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new WorkspaceDocumentOpenResult(
				WorkspaceDocumentOpenStatus.AlreadyOpen,
				_snapshot));

		public IReadOnlyList<WorkspaceDocumentSnapshot> GetSnapshotsUnderDirectory(string directoryPath)
			=> [_snapshot];

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
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
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryRenameResult> RenameDirectoryAsync(
			WorkspaceDocumentDirectoryRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryDeleteResult> DeleteDirectoryAsync(
			WorkspaceDocumentDirectoryDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public void UnregisterOpenView(IWorkspaceDocumentView view) { }

		public Task StopAsync() => Task.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}
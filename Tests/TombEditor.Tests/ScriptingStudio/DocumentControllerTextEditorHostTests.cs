#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Views;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class DocumentControllerTextEditorHostTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	[TestMethod]
	public void TryGetTextSnapshot_UnopenedFileUsesCanonicalBridgeSnapshot()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "language.lua";
			var documentController = new Mock<IEditorDocumentController>();
			documentController
				.Setup(controller => controller.FindEditorsOfFile(filePath))
				.Returns(Array.Empty<IEditorControl>());

			WorkspaceDocumentSnapshot snapshot = CreateSnapshot(filePath, "canonical", 4);
			var documentManager = new TestDocumentBridge(snapshot);

			var host = new DocumentControllerTextEditorHost(
				documentController.Object,
				documentManager);
			ITextSnapshot? result = host.TryGetTextSnapshot(filePath);

			Assert.IsNotNull(result);
			Assert.AreEqual("canonical", result.GetText(0, result.TextLength));
			Assert.AreEqual(1, documentManager.OpenCount);
		});

	[TestMethod]
	public void TryGetTextSnapshot_OpenTextEditorCapturesEditorAndSkipsBridge()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "script.lua";
			using var editor = new PlainTextEditor();
			editor.Document.Text = "live";

			var documentController = new Mock<IEditorDocumentController>();
			documentController
				.Setup(controller => controller.FindEditorsOfFile(filePath))
				.Returns(new IEditorControl[] { editor });
			var documentManager = new TestDocumentBridge(CreateSnapshot(filePath, "canonical", 4));
			var host = new DocumentControllerTextEditorHost(
				documentController.Object,
				documentManager);

			ITextSnapshot? result = host.TryGetTextSnapshot(filePath);

			Assert.IsNotNull(result);
			Assert.AreEqual("live", result.GetText(0, result.TextLength));
			Assert.AreEqual(0, documentManager.OpenCount);
		});

	private static WorkspaceDocumentSnapshot CreateSnapshot(
		string filePath,
		string content,
		long version)
		=> new(
			new WorkspaceDocumentKey(Guid.NewGuid()),
			filePath,
			filePath,
			version,
			version,
			false,
			new StringTextSnapshot(content, filePath),
			FileFormat,
			FileStamp.Missing);

	private sealed class TestDocumentBridge(WorkspaceDocumentSnapshot snapshot) : IWorkspaceDocumentManager
	{
		private readonly WorkspaceDocumentSnapshot _snapshot = snapshot;

		public int OpenCount { get; private set; }

		public Task<WorkspaceDocumentManagerOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
		{
			OpenCount++;
			return Task.FromResult(new WorkspaceDocumentManagerOpenResult(
				WorkspaceDocumentManagerOpenOutcome.AlreadyOpen,
				_snapshot));
		}

		public IWorkspaceDocumentReader Documents => throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerMutationResult> ReplaceAsync(WorkspaceDocumentReplaceRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerMutationResult> DiscardAsync(WorkspaceDocumentDiscardRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerRenameResult> RenameAsync(
			WorkspaceDocumentRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerSaveAsResult> SaveAsAsync(
			WorkspaceDocumentSaveAsRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDeleteResult> DeleteAsync(
			WorkspaceDocumentDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDirectoryRenameResult> RenameDirectoryAsync(
			WorkspaceDocumentDirectoryRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDirectoryDeleteResult> DeleteDirectoryAsync(
			WorkspaceDocumentDirectoryDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public void UnregisterOpenView(IWorkspaceDocumentView view) { }

		public Task StopAsync() => Task.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}

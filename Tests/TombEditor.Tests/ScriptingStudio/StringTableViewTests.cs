#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Views;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Core.Editing;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class StringTableViewTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.CrLf);

	[TestMethod]
	public void CleanAttach_PreservesCanonicalSourceAndPublishesNothing()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);
			int publicationCount = 0;
			int workspaceChangeCount = 0;
			int workerCompletionCount = 0;
			view.WorkspaceContentChanged += (_, _) => workspaceChangeCount++;
			view.ContentChangedWorkerRunCompleted += (_, _) => workerCompletionCount++;
			workspaceView.ApplyRequested += (_, _) => publicationCount++;
			WorkspaceDocumentSnapshot snapshot = CreateSnapshot(
				"language.txt",
				"; keep this source comment\r\n\r\n[Strings]\r\nHello", 4);

			WorkspaceDocumentViewOpenResult result = workspaceView.Open(snapshot);

			Assert.AreEqual(WorkspaceDocumentViewOpenOutcome.Opened, result.Outcome);
			Assert.AreEqual(snapshot.Content, workspaceView.Text);
			Assert.AreEqual(snapshot.Content, view.WorkspaceCanonicalContent);
			Assert.IsFalse(workspaceView.HasPendingEdits);
			Assert.IsFalse(workspaceView.HasConflict);
			Assert.IsFalse(view.IsContentChanged);
			Assert.AreEqual(0, publicationCount);
			Assert.AreEqual(0, workspaceChangeCount);
			Assert.AreEqual(0, workerCompletionCount);
		});

	[TestMethod]
	public void ApplyEdit_PublishesNormalizedReplacementOnce()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			workspaceView.ApplyRequested += (_, args) => requests.Add(args.Request);
			WorkspaceDocumentSnapshot snapshot = CreateSnapshot("language.txt", "[Strings]\r\nHello", 4);
			workspaceView.Open(snapshot);

			workspaceView.Apply(new PreparedTextEdits([new TextEditOperation(11, 16, "Changed", 0)]));

			Assert.AreEqual(1, requests.Count);
			Assert.AreEqual(snapshot.DocumentKey, requests[0].Identity.DocumentKey);
			Assert.AreEqual(snapshot.Version, requests[0].Identity.Version);
			Assert.AreEqual("[Strings]\r\nChanged", requests[0].Content);
			Assert.IsTrue(workspaceView.HasPendingEdits);
			Assert.IsFalse(workspaceView.HasConflict);
		});

	[TestMethod]
	public void BoundRowMutation_PublishesExactlyOnce()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			workspaceView.ApplyRequested += (_, args) => requests.Add(args.Request);
			workspaceView.Open(CreateSnapshot("language.txt", "[Strings]\r\nHello", 4));

			var viewModel = (StringEditorViewModel)view.DataContext;
			viewModel.Sections[0].Rows[0].StringValue = "Changed";

			Assert.AreEqual(1, requests.Count);
			Assert.IsTrue(requests[0].Content.Contains("Changed", StringComparison.Ordinal));
			Assert.IsTrue(workspaceView.HasPendingEdits);
		});

	[TestMethod]
	public void AcknowledgeApply_ClearsPendingStateAndAdvancesCanonicalView()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);
			WorkspaceDocumentSnapshot initial = CreateSnapshot("language.txt", "[Strings]\r\nHello", 4);
			workspaceView.Open(initial);
			workspaceView.Apply(new PreparedTextEdits([new TextEditOperation(11, 16, "Local", 0)]));
			WorkspaceDocumentSnapshot acknowledged = CreateSnapshot(
				"language.txt",
				"[Strings]\r\nCanonical",
				5,
				initial.DocumentKey);

			WorkspaceDocumentViewRefreshResult result = workspaceView.AcknowledgeApply(new WorkspaceDocumentMutationResult(
				WorkspaceDocumentMutationOutcome.Changed,
				new WorkspaceDocumentRequestIdentity(
					acknowledged.DocumentKey,
					acknowledged.DocumentId,
					acknowledged.Version),
				acknowledged));

			Assert.AreEqual(WorkspaceDocumentViewRefreshOutcome.Refreshed, result.Outcome);
			Assert.AreEqual(acknowledged.Content, workspaceView.Text);
			Assert.IsFalse(workspaceView.HasPendingEdits);
			Assert.IsFalse(workspaceView.HasConflict);
		});

	[TestMethod]
	public void RefreshWhilePending_MarksConflictAndRetainsGridModel()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);
			workspaceView.Open(CreateSnapshot("language.txt", "[Strings]\r\nHello", 4));
			workspaceView.Apply(new PreparedTextEdits([new TextEditOperation(11, 16, "Local", 0)]));

			WorkspaceDocumentViewRefreshResult result = workspaceView.Refresh(
				CreateSnapshot("language.txt", "[Strings]\r\nCanonical", 5, workspaceView.DocumentKey));

			Assert.AreEqual(WorkspaceDocumentViewRefreshOutcome.MarkedStale, result.Outcome);
			Assert.IsTrue(workspaceView.HasPendingEdits);
			Assert.IsTrue(workspaceView.HasConflict);
			Assert.AreEqual("[Strings]\r\nLocal", workspaceView.Text);
		});

	[TestMethod]
	public void OpenMalformedCanonicalSource_FailsWithoutOpeningView()
		=> StaTestHelper.RunInSta(() =>
		{
			using var view = new StringEditorView(new Version(1, 0));
			var workspaceView = new StringEditorWorkspaceView(view);

			WorkspaceDocumentViewOpenResult result = workspaceView.Open(
				CreateSnapshot("language.txt", "[ExtraNG]\r\nnot a row", 1));

			Assert.AreEqual(WorkspaceDocumentViewOpenOutcome.Unavailable, result.Outcome);
			Assert.AreEqual("ParseFailed", result.Failure!.Code);
			Assert.IsNull(workspaceView.DocumentKey);
			Assert.IsFalse(view.WorkspaceViewAttached);
		});

	[TestMethod]
	public void ControllerOpen_StringEditorAttachesBeforeRegistration()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				$"tomb-editor-{Guid.NewGuid():N}.txt");
			System.IO.File.WriteAllText(filePath, "[Strings]\r\nDisk");

			try
			{
				var manager = new TestStringDocumentBridge("[Strings]\r\nCanonical");
				var controller = new EditorDocumentController(
					new Version(1, 0),
					string.Empty,
					messageService: null,
					documentManager: manager);
				controller.RegisterDocument(new ScriptingDocumentRegistration(
					EditorType.Strings,
					DocumentMode.Strings,
					static _ => true,
					static _ => true,
					static version => new StringEditorView(version),
					ScriptingDocumentContributions.None));

				controller.OpenFile(filePath);

				var editor = (StringEditorView)controller.CurrentEditor!;
				Assert.AreEqual(string.Empty, manager.ContentBeforeAttach);
				Assert.AreEqual("[Strings]\r\nCanonical", editor.WorkspaceCanonicalContent);
				Assert.AreEqual(1, controller.GetOpenEditors().Count());
				Assert.AreEqual(1, manager.OpenCount);
				controller.TryCloseEditor(editor);
			}
			finally
			{
				System.IO.File.Delete(filePath);
			}
		});

	private static WorkspaceDocumentSnapshot CreateSnapshot(
		string fileName,
		string content,
		long version,
		WorkspaceDocumentKey? documentKey = null)
		=> new(
			documentKey ?? new WorkspaceDocumentKey(Guid.NewGuid()),
			fileName,
			fileName,
			version,
			version,
			false,
			new StringTextSnapshot(content, fileName),
			FileFormat,
			FileStamp.Missing);

	private sealed class TestStringDocumentBridge : IWorkspaceDocumentManager
	{
		private readonly string _content;
		private readonly WorkspaceDocumentKey _documentKey = new(Guid.NewGuid());

		public TestStringDocumentBridge(string content)
		{
			_content = content;
		}

		public int OpenCount { get; private set; }

		public string? ContentBeforeAttach { get; private set; }

		public Task<WorkspaceDocumentManagerOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public IWorkspaceDocumentReader Documents => throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
		{
			OpenCount++;
			ContentBeforeAttach = (view as ITextEditTarget)?.Text;
			WorkspaceDocumentSnapshot snapshot = CreateSnapshot(
				filePath ?? string.Empty,
				_content,
				1,
				_documentKey);
			WorkspaceDocumentViewOpenResult attach = view.Open(snapshot);

			if (attach.Outcome != WorkspaceDocumentViewOpenOutcome.Opened)
				return Task.FromResult(new WorkspaceDocumentManagerOpenResult(WorkspaceDocumentManagerOpenOutcome.ViewRejected, snapshot));

			return Task.FromResult(new WorkspaceDocumentManagerOpenResult(
				WorkspaceDocumentManagerOpenOutcome.Opened,
				snapshot));
		}

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

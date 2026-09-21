#nullable enable

using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;
using TombIDE.ScriptingStudio.TextEditing;
using TombLib.Scripting.UI.Bases;
using Nickelony.IDEKit.Core.Editing;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class TextEditorWorkspaceViewTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	[TestMethod]
	public void AttachRefreshAndAcknowledge_SuppressPublicationAndMaintainState()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var view = new TextEditorWorkspaceView(editor, new FakeViewHost(editor));
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			view.ApplyRequested += (_, args) => requests.Add(args.Request);

			WorkspaceDocumentSnapshot initial = CreateSnapshot(
				"script.txt",
				"initial",
				version: 4);
			WorkspaceDocumentViewOpenResult attach = view.Open(initial);

			Assert.AreEqual(WorkspaceDocumentViewOpenOutcome.Opened, attach.Outcome);
			Assert.AreEqual("initial", editor.Text);
			Assert.AreEqual(initial.DocumentId, view.DocumentId);
			Assert.AreEqual(initial.DocumentKey, view.DocumentKey);
			Assert.IsFalse(view.HasPendingEdits);
			Assert.IsFalse(view.HasConflict);
			Assert.AreEqual(0, requests.Count);

			WorkspaceDocumentSnapshot refreshed = CreateSnapshot(
				"script.txt",
				"refreshed",
				version: 5,
				documentKey: initial.DocumentKey);
			WorkspaceDocumentViewRefreshResult refresh = view.Refresh(refreshed);

			Assert.AreEqual(WorkspaceDocumentViewRefreshOutcome.Refreshed, refresh.Outcome);
			Assert.AreEqual("refreshed", editor.Text);
			Assert.AreEqual(0, requests.Count);

			WorkspaceDocumentMutationResult acknowledgement = new(
				WorkspaceDocumentMutationOutcome.Changed,
				new WorkspaceDocumentRequestIdentity(refreshed.DocumentKey, refreshed.DocumentId, refreshed.Version),
				CreateSnapshot(
					"script.txt",
					"acknowledged",
					version: 6,
					documentKey: refreshed.DocumentKey));
			WorkspaceDocumentViewRefreshResult acknowledgedResult = view.AcknowledgeApply(acknowledgement);

			Assert.AreEqual(WorkspaceDocumentViewRefreshOutcome.Refreshed, acknowledgedResult.Outcome);
			Assert.AreEqual("acknowledged", editor.Text);
			Assert.AreEqual(0, requests.Count);

			editor.Text = "next";
			Assert.AreEqual(6, requests[0].Identity.Version);
		});

	[TestMethod]
	public void UserMutation_PublishesExactlyOneVersionedRequest()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var view = new TextEditorWorkspaceView(editor, new FakeViewHost(editor));
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			view.ApplyRequested += (_, args) => requests.Add(args.Request);

			WorkspaceDocumentSnapshot initial = CreateSnapshot("script.txt", "initial", version: 4);
			view.Open(initial);
			editor.Text = "changed";

			Assert.AreEqual(1, requests.Count);
			Assert.AreEqual(initial.DocumentKey, requests[0].Identity.DocumentKey);
			Assert.AreEqual(initial.DocumentId, requests[0].Identity.DocumentId);
			Assert.AreEqual(initial.Version, requests[0].Identity.Version);
			Assert.AreEqual("changed", requests[0].Content);
			Assert.IsTrue(view.HasPendingEdits);
			Assert.IsFalse(view.HasConflict);
		});

	[TestMethod]
	public void ApplyPreparedOperations_SuppressesFeedbackAndPreservesUndoState()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var view = new TextEditorWorkspaceView(editor, new FakeViewHost(editor));
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			view.ApplyRequested += (_, args) => requests.Add(args.Request);

			view.Open(CreateSnapshot("script.txt", "initial", version: 4));
			view.Apply(new PreparedTextEdits([new TextEditOperation(0, "initial".Length, "changed", 0)]));

			Assert.AreEqual("changed", view.Text);
			Assert.AreEqual(1, requests.Count);
			Assert.AreEqual("changed", requests[0].Content);
			Assert.IsTrue(view.HasPendingEdits);
			Assert.IsTrue(editor.CanUndo);

			editor.Undo();
			Assert.AreEqual("initial", view.Text);
			Assert.AreEqual(2, requests.Count);
			Assert.AreEqual("initial", requests[1].Content);

			editor.Redo();
			Assert.AreEqual("changed", view.Text);
			Assert.AreEqual(3, requests.Count);
			Assert.AreEqual("changed", requests[2].Content);
		});

	[TestMethod]
	public void StaleAcknowledgement_MarksConflictAndRetainsLocalText()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var view = new TextEditorWorkspaceView(editor, new FakeViewHost(editor));
			WorkspaceDocumentSnapshot initial = CreateSnapshot("script.txt", "initial", version: 4);
			view.Open(initial);
			editor.Text = "local";

			WorkspaceDocumentMutationResult stale = new(
				WorkspaceDocumentMutationOutcome.StaleDocument,
				new WorkspaceDocumentRequestIdentity(initial.DocumentKey, initial.DocumentId, initial.Version),
				CreateSnapshot("script.txt", "canonical", version: 5, documentKey: initial.DocumentKey));
			WorkspaceDocumentViewRefreshResult result = view.AcknowledgeApply(stale);

			Assert.AreEqual(WorkspaceDocumentViewRefreshOutcome.MarkedStale, result.Outcome);
			Assert.AreEqual("local", editor.Text);
			Assert.IsTrue(view.HasPendingEdits);
			Assert.IsTrue(view.HasConflict);
		});

	[TestMethod]
	public void Detach_ClearsActiveEditTargetOnlyWhenItStillOwnsTheTarget()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var host = new FakeViewHost(editor);
			var view = new TextEditorWorkspaceView(editor, host);

			view.Open(CreateSnapshot("script.txt", "initial", version: 4));
			Assert.AreSame(view, host.ActiveEditTarget);

			view.Close();
			Assert.IsNull(host.ActiveEditTarget);

			var secondView = new TextEditorWorkspaceView(editor, host);
			secondView.Open(CreateSnapshot("script.txt", "initial", version: 4));
			var otherTarget = new AvalonEditTextEditTarget(editor);
			host.ActiveEditTarget = otherTarget;

			secondView.Close();
			Assert.AreSame(otherTarget, host.ActiveEditTarget);
		});

	[TestMethod]
	public void FailedOpen_DoesNotLeaveHostBoundToStaleViewTarget()
		=> StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor();
			var host = new FakeViewHost(editor) { ThrowOnApplyAuthoritativeContent = true };
			var view = new TextEditorWorkspaceView(editor, host);

			WorkspaceDocumentViewOpenResult attach = view.Open(CreateSnapshot("script.txt", "canonical", version: 4));

			Assert.AreEqual(WorkspaceDocumentViewOpenOutcome.Unavailable, attach.Outcome);
			Assert.IsNotNull(attach.Failure);
			Assert.IsNull(host.ActiveEditTarget);
		});

	[TestMethod]
	public void PlainTextEditor_ConstructsAndEditsWithoutWorkspaceIntegrationTypes()
		=> StaTestHelper.RunInSta(() =>
		{
			using var editor = new PlainTextEditor();

			editor.Text = "initial";
			editor.IsContentChanged = true;

			Assert.AreEqual("initial", editor.Text);
			Assert.IsTrue(editor.IsContentChanged);
			Assert.IsNull(editor.WorkspaceEditTarget);
		});

	private static ICSharpCode.AvalonEdit.TextEditor CreateEditor()
		=> new() { Document = new TextDocument(string.Empty) };

	private static WorkspaceDocumentSnapshot CreateSnapshot(
		string fileName,
		string content,
		long version,
		WorkspaceDocumentKey? documentKey = null)
	{
		return new WorkspaceDocumentSnapshot(
			documentKey ?? new WorkspaceDocumentKey(Guid.NewGuid()),
			fileName,
			fileName,
			version,
			version,
			false,
			new StringTextSnapshot(content, fileName),
			FileFormat,
			FileStamp.Missing);
	}

	private sealed class FakeViewHost : IAvalonEditWorkspaceViewHost
	{
		private readonly ICSharpCode.AvalonEdit.TextEditor _editor;

		public FakeViewHost(ICSharpCode.AvalonEdit.TextEditor editor)
		{
			_editor = editor;
		}

		public bool ThrowOnApplyAuthoritativeContent { get; init; }

		public ITextEditTarget? ActiveEditTarget { get; set; }

		public string FilePath { get; set; } = string.Empty;

		public bool IsContentChanged { get; set; }

		public void ApplyAuthoritativeContent(string filePath, string content)
		{
			if (ThrowOnApplyAuthoritativeContent)
				throw new InvalidOperationException("Simulated authoritative content failure.");

			FilePath = filePath;
			_editor.Text = content;
		}

		public void ApplyAuthoritativeBaseline(string filePath, string content)
		{
			FilePath = filePath;
		}

		public void RecordPersistedContent(string content)
		{
		}

		public void ProcessContentChange(string content)
		{
		}
	}
}

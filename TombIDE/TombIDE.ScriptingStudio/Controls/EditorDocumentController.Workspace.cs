#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workspace;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Documents.FileSystem;
using Nickelony.IDEKit.Workspace.Documents.Reloading;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Implements session restore, file reload, and workspace view attachment for the controller.
/// </summary>
internal sealed partial class EditorDocumentController
{
	public void CheckPreviousSession()
	{
		if (string.IsNullOrWhiteSpace(ScriptRootDirectoryPath) || !Directory.Exists(ScriptRootDirectoryPath))
			return;

		string[] files = Directory.GetFiles(ScriptRootDirectoryPath, $"*{SupportedFormats.Backup}", SearchOption.AllDirectories);

		if (files.Length == 0)
			return;

		DialogResult result = _messageService.ShowConfirmation(
			Strings.Default.AskRestoreSession,
			Strings.Default.RestoreSessionMBT,
			DialogResult.Yes,
			DialogResult.No,
			defaultValue: DialogResult.Yes);

		if (result == DialogResult.Yes)
			RestoreSession(files);
		else if (result == DialogResult.No)
			SharedMethods.DeleteFiles(files);
	}

	public void AddFileToReloadQueue(string filePath)
		=> _fileReloadCoordinator.QueueFile(filePath);

	public void TryRunFileReloadQueue()
	{
		if (_documentManager is null)
			return;

		_fileReloadCoordinator
			.ProcessQueuedFilesAsync(new FileReloadHooks<DialogResult>(
				(WorkspaceDocumentReloadResult result, CancellationToken _) =>
					Task.FromResult(ShowFileReloadPrompt(result.Snapshot?.DisplayPath ?? result.RequestedIdentity.DocumentId)),
				(string filePath, CancellationToken _) => Task.FromResult(ReloadWorkspaceDocument(filePath)),
				(WorkspaceDocumentReloadResult result, CancellationToken _) =>
				{
					ReportReloadFailure(result);
					return Task.CompletedTask;
				},
				(WorkspaceDocumentReloadResult result, DialogResult choice, CancellationToken _) =>
					Task.FromResult(ResolveWorkspaceConflict(result, choice))))
			.GetAwaiter()
			.GetResult();
		CloseInvalidEditors();
	}

	private DialogResult ShowFileReloadPrompt(string filePath)
	{
		return _messageService.ShowConfirmation(
			string.Format(Strings.Default.AskFileReload, filePath),
			Strings.Default.FileReloadMBT,
			DialogResult.Yes,
			DialogResult.No,
			defaultValue: DialogResult.Yes);
	}

	private void RestoreSession(IEnumerable<string> files)
	{
		foreach (string file in files)
		{
			if (!File.Exists(file))
				continue;

			string backupFileContent = ReadBackupFileContent(file);
			string originalFilePath = FileHelper.GetOriginalFilePathFromBackupFile(file);

			OpenFile(originalFilePath);

			if (_documentManager is null
				|| !TryGetWorkspaceSnapshot(originalFilePath, out WorkspaceDocumentSnapshot? snapshot)
				|| snapshot is null)
			{
				if (_currentEditor is not null)
					_currentEditor.Content = backupFileContent;

				continue;
			}

			if (snapshot.IsDirty)
				continue;

			WorkspaceDocumentManagerMutationResult replacement = _documentManager
				.ReplaceAsync(new WorkspaceDocumentReplaceRequest(
					new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
					backupFileContent,
					snapshot.FileFormat))
				.GetAwaiter()
				.GetResult();
			if (replacement.Outcome is not (WorkspaceDocumentMutationOutcome.Changed or WorkspaceDocumentMutationOutcome.NoChange))
				System.Diagnostics.Debug.WriteLine(
					$"Unable to restore backup for '{originalFilePath}': {replacement.Outcome}.");
		}
	}

	private string ReadBackupFileContent(string filePath)
	{
		if (_workspaceFileSystem is null)
			return File.ReadAllText(filePath);

		WorkspaceFileReadResult result = _workspaceFileSystem
			.ReadAsync(filePath, CancellationToken.None)
			.GetAwaiter()
			.GetResult();
		if (result.RawBytes is ReadOnlyMemory<byte> rawBytes)
			return WorkspaceTextCodec.Decode(rawBytes.Span, TextEncodingKind.Utf8, out _);

		return result.Content;
	}

	private WorkspaceDocumentReloadResult ReloadWorkspaceDocument(string filePath)
	{
		if (!TryGetWorkspaceSnapshot(filePath, out WorkspaceDocumentSnapshot? snapshot) || snapshot is null)
			return new WorkspaceDocumentReloadResult(
				WorkspaceDocumentReloadOutcome.DocumentNotFound,
				new WorkspaceDocumentRequestIdentity(new WorkspaceDocumentKey(Guid.Empty), filePath, 0),
				null);

		var request = new WorkspaceDocumentReloadRequest(
			new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version));

		WorkspaceDocumentManagerReloadResult reload = _documentManager!
			.ReloadAsync(request)
			.GetAwaiter()
			.GetResult();

		// The reload coordinator consumes store-level results. Attached views that blocked the reload
		// report no store result; mapping that to a non-reloaded status makes the coordinator report
		// the failure, which is the desired outcome for a view-blocked reload.
		return reload.StoreResult ?? new WorkspaceDocumentReloadResult(
			WorkspaceDocumentReloadOutcome.OperationInProgress,
			request.Identity,
			reload.Snapshot);
	}

	private WorkspaceDocumentConflictResolutionResult? ResolveWorkspaceConflict(
		WorkspaceDocumentReloadResult reloadResult,
		DialogResult choice)
	{
		if (_documentManager is null
			|| reloadResult.Snapshot is not WorkspaceDocumentSnapshot snapshot)
			return null;

		WorkspaceDocumentConflictResolutionChoice resolutionChoice;
		if (choice == DialogResult.Yes)
			resolutionChoice = WorkspaceDocumentConflictResolutionChoice.UseDisk;
		else if (choice == DialogResult.No)
			resolutionChoice = WorkspaceDocumentConflictResolutionChoice.UseLogical;
		else
			return null;

		if (resolutionChoice == WorkspaceDocumentConflictResolutionChoice.UseDisk)
		{
			List<string> unsynchronizedViewIds = [];
			foreach (IWorkspaceDocumentView view in _workspaceViews.Values
				.Where(view => view is IWorkspaceScriptView scriptView
					&& string.Equals(scriptView.DocumentId, snapshot.DocumentId, StringComparison.Ordinal)))
			{
				WorkspaceDocumentViewRefreshResult discardResult = view is IWorkspaceViewPendingEdits pendingEdits
					? pendingEdits.DiscardPendingEdits(snapshot)
					: new WorkspaceDocumentViewRefreshResult(
						WorkspaceDocumentViewRefreshOutcome.Failed,
						new WorkspaceOperationFailure(
							WorkspaceViewOperationFailureCodes.ViewRefreshFailed,
							"The view does not support discarding pending edits."));

				if (discardResult.Outcome != WorkspaceDocumentViewRefreshOutcome.Refreshed)
					unsynchronizedViewIds.Add(view.ViewId);
			}

			// The reload coordinator consumes store-level results only; a view that could not discard
			// its pending edits reports a failed resolution, which re-reports the original conflict.
			if (unsynchronizedViewIds.Count > 0)
				return null;
		}

		WorkspaceDocumentManagerConflictResolutionResult resolved = _documentManager
			.ResolveExternalConflictAsync(new WorkspaceDocumentConflictResolutionRequest(
				new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
				reloadResult.ObservedOnDiskStamp ?? snapshot.OnDiskStamp,
				resolutionChoice))
			.GetAwaiter()
			.GetResult();

			// The reload coordinator consumes store-level results. A resolution whose attached views
			// stayed unsynchronized is reported as a failed resolution - the coordinator re-reports the
			// original conflict - while the manager still tracks the unsynchronized view and blocks
			// later disk operations for it.
			if (resolved.Views.Outcome == WorkspaceDocumentViewSynchronizationOutcome.Unsynchronized)
				return null;

			return resolved.StoreResult;
	}

	private bool TryGetWorkspaceSnapshot(string filePath, out WorkspaceDocumentSnapshot? snapshot)
	{
		snapshot = null;
		if (_documentManager is null)
			return false;

		WorkspaceDocumentManagerOpenResult result = _documentManager
				.OpenAsync(filePath, DefaultWorkspaceOpenOptions)
				.GetAwaiter()
				.GetResult();
		snapshot = result.Snapshot;
		return snapshot is not null;
	}

	private void ReportReloadFailure(WorkspaceDocumentReloadResult result)
	{
		if (result.Outcome is WorkspaceDocumentReloadOutcome.ExternalFileConflict
			or WorkspaceDocumentReloadOutcome.Unchanged
			or WorkspaceDocumentReloadOutcome.Reloaded)
			return;

		if (result.Failure is not null)
			System.Diagnostics.Debug.WriteLine(result.Failure.Message);
	}

	private bool TryAttachWorkspaceView(IEditorControl editor, string filePath, DocumentLoadOptions options)
	{
		if (_documentManager is null)
		{
			_documentController.InitializeEditor(editor, filePath, options);
			return true;
		}

		IWorkspaceDocumentView? view = editor switch
		{
			TextEditorBase textEditor when editor.EditorType == EditorType.Text
				=> new TextEditorWorkspaceView(textEditor, new AvalonEditWorkspaceViewHostAdapter(textEditor)),
			StringEditorView stringEditor when editor.EditorType == EditorType.Strings => new StringEditorWorkspaceView(stringEditor),
			_ => null
		};

		if (view is null)
		{
			_documentController.InitializeEditor(editor, filePath, options);
			return true;
		}

		using IDisposable processingScope = editor.BeginProcessingScope(options.ProcessingMode);

		WorkspaceDocumentManagerOpenResult result = _documentManager
			.OpenWithViewAsync(filePath, DefaultWorkspaceOpenOptions, view)
			.GetAwaiter()
			.GetResult();

		if (result.Outcome != WorkspaceDocumentManagerOpenOutcome.Opened)
		{
			view.Close();
			editor.Dispose();
			return false;
		}

		_workspaceViews.Add(editor, view);
		return true;
	}

	private static readonly WorkspaceDocumentOpenOptions DefaultWorkspaceOpenOptions = new(
		TextEncodingKind.Utf8,
		new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf));

	private void Editor_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl senderEditor)
			return;

		foreach (IEditorControl fileEditor in FindEditorsOfFile(senderEditor.FilePath))
		{
			fileEditor.IsContentChanged = senderEditor.IsContentChanged;
			RaiseEditorTitleChanged(fileEditor);
		}
	}
}

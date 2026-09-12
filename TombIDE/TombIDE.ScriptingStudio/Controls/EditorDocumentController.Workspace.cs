#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
using Nickelony.IDEKit.Workspace.Documents;

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

		_fileReloadCoordinator.ProcessQueuedFiles(
			ShowFileReloadPrompt,
			ReloadWorkspaceDocument,
			ReportReloadFailure,
			ResolveWorkspaceConflict);
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

			WorkspaceDocumentMutationResult replacement = _documentManager.Replace(new WorkspaceDocumentReplaceRequest(
				snapshot.DocumentKey,
				snapshot.DocumentId,
				snapshot.Version,
				backupFileContent,
				snapshot.FileFormat));
			if (replacement.Status is not (WorkspaceDocumentMutationStatus.Replaced or WorkspaceDocumentMutationStatus.NoChange))
				System.Diagnostics.Debug.WriteLine(
					$"Unable to restore backup for '{originalFilePath}': {replacement.Status}.");
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
			return WorkspaceFileCodec.Decode(rawBytes.Span, TextEncodingKind.Utf8, out _);

		return result.Content;
	}

	private WorkspaceDocumentReloadResult ReloadWorkspaceDocument(string filePath)
	{
		if (!TryGetWorkspaceSnapshot(filePath, out WorkspaceDocumentSnapshot? snapshot) || snapshot is null)
			return new WorkspaceDocumentReloadResult(
				WorkspaceDocumentReloadStatus.DocumentNotFound,
				new WorkspaceDocumentKey(Guid.Empty),
				filePath,
				0,
				null);

		return _documentManager!
			.ReloadAsync(new WorkspaceDocumentReloadRequest(
				snapshot.DocumentKey,
				snapshot.DocumentId,
				snapshot.Version,
				snapshot.OnDiskStamp))
			.GetAwaiter()
			.GetResult();
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
			List<string> failedViewIds = [];
			foreach (IWorkspaceDocumentView view in _workspaceViews.Values
				.Where(view => string.Equals(view.DocumentId, snapshot.DocumentId, StringComparison.Ordinal)))
			{
				WorkspaceDocumentViewRefreshResult discardResult = view.DiscardPendingEdits(snapshot);
				if (discardResult.Status != WorkspaceDocumentViewRefreshStatus.Refreshed)
					failedViewIds.Add(view.ViewId);
			}

			if (failedViewIds.Count > 0)
				return new WorkspaceDocumentConflictResolutionResult(
					WorkspaceDocumentConflictResolutionStatus.ViewNotSynchronized,
					snapshot.DocumentKey,
					snapshot.DocumentId,
					snapshot.Version,
					resolutionChoice,
					snapshot,
					BlockingViewIds: failedViewIds);
		}

		return _documentManager
			.ResolveExternalConflictAsync(new WorkspaceDocumentConflictResolutionRequest(
				snapshot.DocumentKey,
				snapshot.DocumentId,
				snapshot.Version,
				reloadResult.ObservedOnDiskStamp ?? snapshot.OnDiskStamp,
				resolutionChoice))
			.GetAwaiter()
			.GetResult();
	}

	private bool TryGetWorkspaceSnapshot(string filePath, out WorkspaceDocumentSnapshot? snapshot)
	{
		snapshot = null;
		if (_documentManager is null)
			return false;

		WorkspaceDocumentOpenResult result = _documentManager
			.OpenAsync(filePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();
		snapshot = result.Snapshot;
		return snapshot is not null;
	}

	private void ReportReloadFailure(WorkspaceDocumentReloadResult result)
	{
		if (result.Status is WorkspaceDocumentReloadStatus.ExternalFileConflict
			or WorkspaceDocumentReloadStatus.Unchanged
			or WorkspaceDocumentReloadStatus.Reloaded)
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

		WorkspaceDocumentManagerOpenResult result = _documentManager.OpenWithView(
			filePath,
			DefaultWorkspaceOpenOptions,
			view);

		if (result.Status != WorkspaceDocumentManagerOpenStatus.Opened)
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

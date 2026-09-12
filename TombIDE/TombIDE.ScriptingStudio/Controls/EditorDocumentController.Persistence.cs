#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;
using TombIDE.ScriptingStudio.Workspace;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Implements save, save-as, close, and dirty-state operations for the controller.
/// </summary>
internal sealed partial class EditorDocumentController
{
	public bool AskSaveAll()
	{
		foreach (string path in _documentController.GetFilePaths())
		{
			IEditorControl? editor = FindEditorsOfFile(path).FirstOrDefault();

			FileSavingResult result = TryAskSaveFile(editor);

			if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
				return false;

			if (result == FileSavingResult.Rejected
				&& (editor is null || !DiscardWorkspaceDocument(editor)))
				return false;
		}

		return true;
	}

	public void SaveAll()
	{
		foreach (string path in _documentController.GetFilePaths())
		{
			IEditorControl? editor = FindEditorsOfFile(path).FirstOrDefault();

			if (editor is not null)
				SaveFile(editor);
		}
	}

	public FileSavingResult SaveFile()
		=> _currentEditor is null ? FileSavingResult.Failed : SaveFile(_currentEditor);

	public FileSavingResult SaveFile(IEditorControl editor)
	{
		if (editor is null)
			return FileSavingResult.Failed;

		try
		{
			if (_documentManager is not null && _workspaceViews.ContainsKey(editor))
			{
				FileSavingResult workspaceResult = CommitWorkspaceDocument(editor);
				if (workspaceResult != FileSavingResult.Success)
					return workspaceResult;
			}
			else
			{
				editor.Save();
			}

			RaiseEditorTitleChanged(editor);
			if (_documentManager is null)
				SaveOtherEditorsOfFile(editor);
		}
		catch (Exception ex)
		{
			DialogResult result = _messageService.ShowConfirmation(
				ex.Message,
				Strings.Default.Error,
				DialogResult.Retry,
				DialogResult.Cancel,
				defaultValue: DialogResult.Retry);

			if (result == DialogResult.Retry)
				return SaveFile(editor);

			return FileSavingResult.Failed;
		}

		return FileSavingResult.Success;
	}

	private FileSavingResult CommitWorkspaceDocument(IEditorControl editor)
	{
		WorkspaceDocumentOpenResult openResult = _documentManager!
			.OpenAsync(editor.FilePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();

		if (openResult.Snapshot is not WorkspaceDocumentSnapshot snapshot)
			return FileSavingResult.Failed;

		WorkspaceDocumentCommitResult result = _documentManager
			.CommitAsync(
				new WorkspaceDocumentCommitRequest(
					snapshot.DocumentKey,
					snapshot.DocumentId,
					snapshot.Version,
					snapshot.OnDiskStamp))
			.GetAwaiter()
			.GetResult();

		return result.Status switch
		{
			WorkspaceDocumentCommitStatus.Committed => FileSavingResult.Success,
			WorkspaceDocumentCommitStatus.CommittedWithUnsynchronizedView => FileSavingResult.Failed,
			WorkspaceDocumentCommitStatus.Cancelled => FileSavingResult.Canceled,
			_ => FileSavingResult.Failed
		};
	}

	public FileSavingResult SaveFileAs()
		=> _currentEditor is null ? FileSavingResult.Failed : SaveFileAs(_currentEditor);

	public FileSavingResult SaveFileAs(IEditorControl editor)
	{
		if (editor is null)
			return FileSavingResult.Failed;

		string oldFilePath = editor.FilePath;
		string[] ignoredPaths = [];

		if (editor.DefaultFileExtension == ".lua")
			ignoredPaths = [@"Scripts\Engine"];

		var fileCreationViewModel = new FileCreationViewModel(
			ScriptRootDirectoryPath,
			FileCreationMode.SavingAs,
			editor.DefaultFileExtension,
			null,
			null,
			ignoredPaths);

		var view = new FileCreationView(fileCreationViewModel);
		string? newFilePath = view.ShowDialogAndGetResult();

		if (newFilePath is null)
			return FileSavingResult.Canceled;

		if (string.IsNullOrWhiteSpace(oldFilePath)
			|| oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
		{
			editor.FilePath = newFilePath;
			RaiseEditorTitleChanged(editor);
			return SaveFile(editor);
		}

		if (_documentManager is not null)
		{
			WorkspaceDocumentOpenResult openResult = _documentManager
				.OpenAsync(oldFilePath, DefaultWorkspaceOpenOptions)
				.GetAwaiter()
				.GetResult();
			WorkspaceDocumentSnapshot? snapshot = openResult.Snapshot;
			if (snapshot is not null)
			{
				WorkspaceDocumentSaveAsResult saveAsResult = _documentManager
					.SaveAsAsync(
						new WorkspaceDocumentSaveAsRequest(
							snapshot.DocumentKey,
							snapshot.DocumentId,
							snapshot.Version,
							snapshot.OnDiskStamp,
							newFilePath))
					.GetAwaiter()
					.GetResult();

				if (saveAsResult.Status != WorkspaceDocumentSaveAsStatus.SavedAs)
					return saveAsResult.Status == WorkspaceDocumentSaveAsStatus.Cancelled
						? FileSavingResult.Canceled
						: FileSavingResult.Failed;

				foreach (IEditorControl openEditor in FindEditorsOfFile(oldFilePath).ToList())
				{
					openEditor.FilePath = newFilePath;
					RaiseEditorTitleChanged(openEditor);
				}

				OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
				return FileSavingResult.Success;
			}
		}

		editor.FilePath = newFilePath;
		RaiseEditorTitleChanged(editor);

		FileSavingResult result = SaveFile(editor);

		if (result != FileSavingResult.Success)
		{
			editor.FilePath = oldFilePath;
			RaiseEditorTitleChanged(editor);
			return result;
		}

		if (FindEditorsOfFile(oldFilePath).Any())
		{
			RenameDocument(oldFilePath, newFilePath);
			SaveOtherEditorsOfFile(editor);
		}
		else
		{
			OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
		}

		return result;
	}

	private void CloseAllEditors()
	{
		foreach (IEditorControl editor in GetOpenEditors().ToList())
			CloseEditor(editor, promptToSave: false);
	}

	private bool CloseEditor(IEditorControl editor, bool promptToSave)
	{
		if (editor is null || !_documentController.ContainsEditor(editor))
			return false;

		if (promptToSave && FindEditorsOfFile(editor.FilePath).Count() == 1)
		{
			FileSavingResult result = TryAskSaveFile(editor);

			if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
				return false;

			if (result == FileSavingResult.Rejected && !DiscardWorkspaceDocument(editor))
				return false;
		}
		IEditorControl? nextEditor = GetAdjacentEditor(editor);

		OnEditorClosed(editor);
		DetachEditor(editor);
		_documentController.RemoveEditor(editor);
		editor.Dispose();

		if (ReferenceEquals(_currentEditor, editor))
			SetCurrentEditor(nextEditor, forceRaise: true);

		return true;
	}

	private FileSavingResult TryAskSaveFile(IEditorControl? editor)
	{
		if (editor is null || !IsDocumentDirty(editor))
			return FileSavingResult.AlreadySaved;

		ActivateEditor(editor);

		string fileName = Path.GetFileName(editor.FilePath);
		DialogResult result = _messageService.ShowConfirmation(
			string.Format(Strings.Default.AskUnsavedChanged, fileName),
			Strings.Default.UnsavedChangedMBT,
			DialogResult.Yes,
			DialogResult.No,
			DialogResult.Cancel,
			DialogResult.Yes);

		if (result == DialogResult.Yes)
			return SaveFile(editor);

		if (result == DialogResult.No)
			return FileSavingResult.Rejected;

		return FileSavingResult.Canceled;
	}

	private bool IsDocumentDirty(IEditorControl editor)
	{
		if (_documentManager is null || !_workspaceViews.ContainsKey(editor))
			return editor.IsContentChanged;

		try
		{
			WorkspaceDocumentOpenResult openResult = _documentManager
				.OpenAsync(editor.FilePath, DefaultWorkspaceOpenOptions)
				.GetAwaiter()
				.GetResult();

			return openResult.Snapshot?.IsDirty ?? editor.IsContentChanged;
		}
		catch (NotSupportedException)
		{
			return editor.IsContentChanged;
		}
	}

	private bool DiscardWorkspaceDocument(IEditorControl editor)
	{
		if (_documentManager is null || !_workspaceViews.ContainsKey(editor))
			return true;

		WorkspaceDocumentOpenResult openResult = _documentManager
			.OpenAsync(editor.FilePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();

		if (openResult.Snapshot is not WorkspaceDocumentSnapshot snapshot)
			return false;

		WorkspaceDocumentMutationResult result = _documentManager.Discard(
			new WorkspaceDocumentDiscardRequest(
				snapshot.DocumentKey,
				snapshot.DocumentId,
				snapshot.Version));

		return result.Status is WorkspaceDocumentMutationStatus.Replaced or WorkspaceDocumentMutationStatus.NoChange;
	}

	private void SaveOtherEditorsOfFile(IEditorControl excludedEditor)
	{
		foreach (IEditorControl fileEditor in FindEditorsOfFile(excludedEditor.FilePath))
		{
			if (fileEditor.EditorType == excludedEditor.EditorType)
				continue;

			if (fileEditor.Content != excludedEditor.Content)
				fileEditor.ApplyPersistedContent(excludedEditor.Content);

			fileEditor.RunContentChangedWorker();
			RaiseEditorTitleChanged(fileEditor);
		}
	}
}

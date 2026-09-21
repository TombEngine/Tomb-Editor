#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Implements editor activation, close, tab navigation, and event raising for the controller.
/// </summary>
internal sealed partial class EditorDocumentController
{
	public void CloseInvalidEditors()
	{
		List<IEditorControl> editorsToClose = [];

		foreach (IEditorControl editor in GetOpenEditors().ToList())
		{
			if (!File.Exists(editor.FilePath))
				editorsToClose.Add(editor);
		}

		foreach (IEditorControl editor in editorsToClose)
			TryCloseEditor(editor);
	}

	public void ActivateEditor(IEditorControl editor)
	{
		if (editor is null || !_documentController.ContainsEditor(editor))
			return;

		SynchronizeEditorsOfFile(editor.FilePath);
		SetCurrentEditor(editor);
	}

	public bool TryCloseEditor(IEditorControl editor)
		=> CloseEditor(editor, promptToSave: true);

	public bool TryActivatePreviousEditor()
	{
		IEditorControl? previousEditor = GetRelativeEditor(-1);

		if (previousEditor is null)
			return false;

		ActivateEditor(previousEditor);
		return true;
	}

	public bool TryActivateNextEditor()
	{
		IEditorControl? nextEditor = GetRelativeEditor(1);

		if (nextEditor is null)
			return false;

		ActivateEditor(nextEditor);
		return true;
	}

	private void SynchronizeEditorsOfFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		if (_documentManager is null)
			return;
	}

	private IEditorControl? GetRelativeEditor(int offset)
	{
		List<IEditorControl> editors = [.. GetOpenEditors()];

		if (_currentEditor is null || editors.Count == 0)
			return null;

		int currentIndex = editors.IndexOf(_currentEditor);

		if (currentIndex < 0)
			return null;

		int targetIndex = currentIndex + offset;

		if (targetIndex < 0 || targetIndex >= editors.Count)
			return null;

		return editors[targetIndex];
	}

	private IEditorControl? GetAdjacentEditor(IEditorControl editor)
	{
		List<IEditorControl> editors = [.. GetOpenEditors()];
		int editorIndex = editors.IndexOf(editor);

		if (editorIndex < 0 || editors.Count <= 1)
			return null;

		int candidateIndex = editorIndex > 0 ? editorIndex - 1 : 1;
		return candidateIndex >= 0 && candidateIndex < editors.Count ? editors[candidateIndex] : null;
	}

	private void SetCurrentEditor(IEditorControl? editor, bool forceRaise = false)
	{
		if (!forceRaise && ReferenceEquals(_currentEditor, editor))
			return;

		_currentEditor = editor;
		_documentContextGeneration++;
		_currentDocumentContext = new ScriptingDocumentContext(
			_documentContextGeneration,
			editor,
			editor?.FilePath,
			_documentController.GetDocumentRegistration(editor));
		CurrentEditorChanged?.Invoke(this, new ScriptingDocumentContextChangedEventArgs(_currentDocumentContext));
	}

	private void AttachEditor(IEditorControl editor)
		=> editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

	private void DetachEditor(IEditorControl editor)
	{
		editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;

		if (!_workspaceViews.Remove(editor, out IWorkspaceDocumentView? view))
			return;

		_documentManager?.UnregisterOpenView(view);
		view.Close();
	}

	private void RaiseEditorTitleChanged(IEditorControl editor)
		=> EditorTitleChanged?.Invoke(this, new EditorControlEventArgs(editor));

	private void OnFileOpened(EventArgs e)
		=> FileOpened?.Invoke(_currentEditor, e);

	private void OnEditorClosed(IEditorControl editor)
		=> EditorClosed?.Invoke(this, new EditorControlEventArgs(editor));

	private void OnDocumentRenamed(DocumentRenamedEventArgs e)
		=> DocumentRenamed?.Invoke(this, e);
}

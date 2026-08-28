using ICSharpCode.AvalonEdit.Document;
using System;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region File I/O

	/// <summary>
	/// Loads the file at the given path into the editor.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	public new void Load(string filePath)
		=> Load(filePath, false);

	/// <summary>
	/// Loads the file at the given path into the editor, optionally starting a silent session.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	/// <param name="silentSession">Whether to start a silent session that skips background processing.</param>
	public void Load(string filePath, bool silentSession)
	{
		EnsureNotDisposed();

		base.Load(filePath);
		FilePath = filePath;

		_contentPersistenceCoordinator.SetPersistedContent(Content);

		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		IsSilentSession = silentSession;

		_bookmarkCoordinator.Restore(FilePath);
	}

	/// <summary>
	/// Saves the current document to its associated file path.
	/// </summary>
	public void Save()
		=> Save(FilePath);

	/// <summary>
	/// Saves the current document to the given file path.
	/// </summary>
	/// <param name="filePath">The path to save the document to.</param>
	public new void Save(string filePath)
	{
		EnsureNotDisposed();
		base.Save(filePath);

		_contentPersistenceCoordinator.SetPersistedContent(Content);

		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	internal void SaveBookmarks()
		=> _bookmarkCoordinator.Save(FilePath);

	#endregion File I/O

	#region Content

	/// <summary>
	/// Runs the content-change worker check and updates the changed state.
	/// </summary>
	public void RunContentChangedWorker()
	{
		EnsureNotDisposed();
		IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
	}

	/// <summary>
	/// Applies the given content and marks it as the persisted baseline.
	/// </summary>
	/// <param name="content">The content to apply.</param>
	public void ApplyPersistedContent(string content)
	{
		EnsureNotDisposed();
		SetContent(content);

		_contentPersistenceCoordinator.SetPersistedContent(Content);

		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	private void SetContent(string content)
	{
		EnsureNotDisposed();

		DocumentLine cachedLine = Document.GetLineByOffset(CaretOffset);

		Document.UndoStack.StartUndoGroup();

		SelectAll();
		SelectedText = content;

		Document.UndoStack.EndUndoGroup();

		if (cachedLine.EndOffset <= Document.TextLength)
			ResetSelectionAt(cachedLine);
		else
			ResetSelection();

		RunContentChangedWorker();
	}

	#endregion Content
}

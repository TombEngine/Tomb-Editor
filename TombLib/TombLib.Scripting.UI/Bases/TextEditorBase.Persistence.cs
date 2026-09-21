using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Bookmarks;
using System;
using System.Threading;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region File I/O

	/// <summary>
	/// Loads the file at the given path into the editor.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	public new void Load(string filePath)
		=> Load(filePath, default);

	/// <summary>
	/// Loads the file at the given path into the editor, optionally starting a suppressed processing scope.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	/// <param name="options">The load options that select the initial processing mode.</param>
	public void Load(string filePath, DocumentLoadOptions options)
	{
		EnsureNotDisposed();
		using IDisposable processingScope = BeginProcessingScope(options.ProcessingMode);
		using IDisposable resetScope = BeginLoadResetScope();

		base.Load(filePath);
		FilePath = filePath;

		if (WorkspaceEditTarget is null)
		{
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		}

		_unsavedChangesTracker.SetBaseline(Content);
		_bookmarkCoordinator.RestoreBookmarks(_bookmarkStore, FilePath);
	}

	/// <summary>
	/// Applies workspace-owned content without performing another file-system read.
	/// </summary>
	/// <param name="filePath">The workspace document path.</param>
	/// <param name="content">The content supplied by the workspace.</param>
	public void ApplyWorkspaceContent(string filePath, string content)
	{
		EnsureNotDisposed();
		using IDisposable resetScope = BeginLoadResetScope();

		FilePath = filePath;
		SetContent(content);
		if (WorkspaceEditTarget is null)
		{
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		}

		_unsavedChangesTracker.SetBaseline(Content);
		_bookmarkCoordinator.RestoreBookmarks(_bookmarkStore, FilePath);
	}

	/// <summary>
	/// Initializes workspace path and persistence state without replacing existing content.
	/// </summary>
	/// <param name="filePath">The workspace document path.</param>
	/// <param name="content">The content represented by the persisted baseline.</param>
	public void ApplyWorkspaceBaseline(string filePath, string content)
	{
		EnsureNotDisposed();
		using IDisposable resetScope = BeginLoadResetScope();

		FilePath = filePath;
		if (WorkspaceEditTarget is null)
		{
			_contentPersistenceCoordinator.SetPersistedContent(content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		}

		_unsavedChangesTracker.SetBaseline(content);
		_bookmarkCoordinator.RestoreBookmarks(_bookmarkStore, FilePath);
	}

	private IDisposable BeginLoadResetScope()
	{
		// Load and replace boundaries advance the session generation so asynchronous work admitted
		// for the previous document is invalidated.
		Interlocked.Increment(ref _sessionGeneration);

		_diagnosticsCoordinator?.Reset();
		OnLoadReset();

		return _contentPersistenceCoordinator.BeginResetScope();
	}

	/// <summary>
	/// Invalidates language-specific asynchronous work before loaded content replaces the document.
	/// </summary>
	protected virtual void OnLoadReset() { }

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

		if (WorkspaceEditTarget is null)
		{
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		}
		_unsavedChangesTracker.SetBaseline(Content);
		LastModified = DateTime.Now;
	}

	internal void SaveBookmarks()
		=> _bookmarkCoordinator.SaveBookmarks(_bookmarkStore, FilePath);

	#endregion File I/O

	#region Content

	/// <summary>
	/// Runs the content-change worker check and updates the changed state.
	/// </summary>
	public void RunContentChangedWorker()
	{
		EnsureNotDisposed();
		if (WorkspaceEditTarget is not null)
		{
			IsContentChanged = true;
			return;
		}

		IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
	}

	/// <summary>
	/// Runs the content-change worker check for the given content and updates the changed state.
	/// </summary>
	/// <param name="content">The content to compare against the persisted baseline.</param>
	public void RunContentChangedWorker(string content)
	{
		EnsureNotDisposed();
		if (WorkspaceEditTarget is not null)
		{
			IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck(content);
			return;
		}

		IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck(content);
	}

	/// <summary>
	/// Records workspace content as the persisted baseline for backup synchronization.
	/// </summary>
	/// <param name="content">The content represented by the workspace baseline.</param>
	public void SetWorkspacePersistedContent(string content)
	{
		EnsureNotDisposed();
		_contentPersistenceCoordinator.SetPersistedContent(content);
		_unsavedChangesTracker.SetBaseline(content);
	}

	/// <summary>
	/// Applies the given content and marks it as the persisted baseline.
	/// </summary>
	/// <param name="content">The content to apply.</param>
	public void ApplyPersistedContent(string content)
	{
		EnsureNotDisposed();
		SetContent(content);

		if (WorkspaceEditTarget is null)
		{
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		}
		_unsavedChangesTracker.SetBaseline(Content);
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

		if (WorkspaceEditTarget is null)
			RunContentChangedWorker();
	}

	#endregion Content
}

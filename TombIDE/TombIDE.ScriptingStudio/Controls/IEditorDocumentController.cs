#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.Shared;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Manages the lifecycle of editor documents including registration, opening, saving,
/// closing, and navigation between editors.
/// </summary>
public interface IEditorDocumentController
{
	/// <summary>
	/// Gets or sets the root directory path for script files managed by this controller.
	/// </summary>
	string ScriptRootDirectoryPath { get; set; }

	/// <summary>
	/// Gets the currently active editor, or <see langword="null"/> if no editor is active.
	/// </summary>
	IEditorControl? CurrentEditor { get; }

	/// <summary>
	/// Gets the immutable context for the last activated document, or the explicit empty context.
	/// </summary>
	ScriptingDocumentContext CurrentDocumentContext { get; }

	/// <summary>
	/// Gets the exact document registration associated with the specified editor.
	/// </summary>
	ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor);

	/// <summary>
	/// Finds an editor for the specified file path, optionally filtering by editor type.
	/// </summary>
	IEditorControl? FindEditor(string filePath, EditorType editorType = EditorType.Default);

	/// <summary>
	/// Finds an editor that was opened in source view mode (<see cref="EditorType.Text"/>)
	/// for the specified file path. The source view shows the raw code/text representation
	/// rather than a specialized domain editor (e.g. the string table grid).
	/// </summary>
	IEditorControl? FindSourceEditor(string filePath);

	/// <summary>
	/// Gets all currently open editors.
	/// </summary>
	IEnumerable<IEditorControl> GetOpenEditors();

	/// <summary>
	/// Finds all editors that have the specified file path open. A single file may be open in multiple editors
	/// (e.g. a string table file may be open in both the string table grid and the source/text view).
	/// </summary>
	IEnumerable<IEditorControl> FindEditorsOfFile(string filePath);

	/// <summary>
	/// Determines whether the specified editor is managed by this controller.
	/// </summary>
	bool ContainsEditor(IEditorControl editor);

	void RegisterDocument(ScriptingDocumentRegistration registration);

	/// <summary>
	/// Checks for a previous session and restores its state if applicable.
	/// </summary>
	void CheckPreviousSession();

	/// <summary>
	/// Opens the specified file in an appropriate editor.
	/// </summary>
	/// <param name="options">
	/// The load options that select the initial processing mode. Suppressed processing
	/// disables backup file creation, background content-change processing, and
	/// diagnostics/error checking for the editor. The tab is still created and
	/// activated normally. Intended for bulk or recovery open scenarios where
	/// side-effects should be avoided.
	/// </param>
	void OpenFile(string filePath, EditorType editorType = EditorType.Default, DocumentLoadOptions options = default);

	/// <summary>
	/// Opens the specified file in source view mode (<see cref="EditorType.Text"/>),
	/// showing the raw code/text representation rather than a specialized domain editor.
	/// </summary>
	/// <param name="options">
	/// The load options that select the initial processing mode. Suppressed processing
	/// disables backup file creation, background content-change processing, and
	/// diagnostics/error checking for the editor. The tab is still created and
	/// activated normally.
	/// </param>
	void OpenSourceFile(string filePath, DocumentLoadOptions options = default);

	/// <summary>
	/// Adds a file path to the reload queue for deferred processing.
	/// </summary>
	void AddFileToReloadQueue(string filePath);

	/// <summary>
	/// Attempts to process all queued file reload requests.
	/// </summary>
	void TryRunFileReloadQueue();

	/// <summary>
	/// Asks the user whether to save all modified documents. Returns <see langword="true"/> if all were saved or discarded.
	/// </summary>
	bool AskSaveAll();

	/// <summary>
	/// Saves all modified documents.
	/// </summary>
	void SaveAll();

	/// <summary>
	/// Saves the currently active document.
	/// </summary>
	FileSavingResult SaveFile();

	/// <summary>
	/// Saves the specified editor's document.
	/// </summary>
	FileSavingResult SaveFile(IEditorControl editor);

	/// <summary>
	/// Prompts the user to save the currently active document with a new path.
	/// </summary>
	FileSavingResult SaveFileAs();

	/// <summary>
	/// Prompts the user to save the specified editor's document with a new path.
	/// </summary>
	FileSavingResult SaveFileAs(IEditorControl editor);

	/// <summary>
	/// Closes any editors that are no longer valid (e.g. file was deleted externally).
	/// </summary>
	void CloseInvalidEditors();

	/// <summary>
	/// Activates the specified editor, bringing it into view.
	/// </summary>
	void ActivateEditor(IEditorControl editor);

	/// <summary>
	/// Attempts to close the specified editor, prompting for save if modified.
	/// Returns <see langword="true"/> if the editor was closed.
	/// </summary>
	bool TryCloseEditor(IEditorControl editor);

	/// <summary>
	/// Attempts to activate the previously active editor.
	/// Returns <see langword="true"/> if a previous editor was available.
	/// </summary>
	bool TryActivatePreviousEditor();

	/// <summary>
	/// Attempts to activate the next editor in the tab order.
	/// Returns <see langword="true"/> if a next editor was available.
	/// </summary>
	bool TryActivateNextEditor();

	/// <summary>
	/// Gets the display title for the specified editor's document.
	/// </summary>
	string GetDocumentTitle(IEditorControl editor);

	/// <summary>
	/// Renames a document from the old file path to the new file path, updating all open editors.
	/// </summary>
	void RenameDocument(string oldFilePath, string newFilePath);

	/// <summary>
	/// Returns <see langword="true"/> if every open document is saved (not modified).
	/// </summary>
	bool IsEveryDocumentSaved();

	/// <summary>
	/// Ensures that open document tabs are synchronized with the file system state.
	/// </summary>
	void EnsureTabFileSynchronization();

	event EventHandler? FileOpened;

	event EventHandler<ScriptingDocumentContextChangedEventArgs>? CurrentEditorChanged;

	event EventHandler<EditorControlEventArgs>? EditorClosed;

	event EventHandler<EditorControlEventArgs>? EditorTitleChanged;

	event EventHandler<DocumentRenamedEventArgs>? DocumentRenamed;
}

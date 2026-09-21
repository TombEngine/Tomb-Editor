using System;

namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Defines the common surface of an editor control hosted in the scripting studio.
/// </summary>
public interface IEditorControl : IDisposable
{
	// Properties

	/// <summary>
	/// Gets the editor type of the control.
	/// </summary>
	EditorType EditorType { get; }

	/// <summary>
	/// Gets or sets the path of the file loaded in the editor.
	/// </summary>
	string FilePath { get; set; }

	/// <summary>
	/// Gets the current processing mode of the editor.
	/// </summary>
	/// <remarks>
	/// The mode is owned by a nest-safe processing scope entered through
	/// <see cref="BeginProcessingScope"/>. A control exposes the current mode but does not own
	/// an unbalanced public flag.
	/// </remarks>
	EditorProcessingMode ProcessingMode
	{
		get => EditorProcessingMode.Normal;
	}

	/// <summary>
	/// Begins a nest-safe scope that applies the given processing mode to the editor and
	/// restores the previous mode when the returned scope is disposed.
	/// </summary>
	/// <param name="mode">The processing mode to apply for the scope.</param>
	/// <returns>A scope that restores the previous mode when disposed.</returns>
	IDisposable BeginProcessingScope(EditorProcessingMode mode)
	{
		// Legacy compatibility default for the unregistered WinForms string editor, which is
		// removed by Phase 3C. Active editors implement the real processing scope.
		return new NullScope();
	}

	/// <summary>
	/// Gets or sets whether backup files are created when the content changes.
	/// </summary>
	bool CreateBackupFiles { get; set; }

	/// <summary>
	/// A string representation of the editor's content.
	/// <para><b>Note:</b> Every <c>IEditorControl</c> should have some way of representing its contents using a string!</para>
	/// </summary>
	string Content { get; set; }

	/// <summary>
	/// Gets or sets whether the editor content differs from the persisted content.
	/// </summary>
	bool IsContentChanged { get; set; }

	/// <summary>
	/// Replaces the editor content without marking the editor as changed.
	/// </summary>
	/// <param name="content">The persisted content to apply.</param>
	void ApplyPersistedContent(string content);

	/// <summary>
	/// Gets or sets the time the content was last modified.
	/// </summary>
	DateTime LastModified { get; set; }

	/// <summary>
	/// Gets whether an undo operation is currently available.
	/// </summary>
	bool CanUndo { get; }

	/// <summary>
	/// Gets whether a redo operation is currently available.
	/// </summary>
	bool CanRedo { get; }

	/* Status data */

	/// <summary>
	/// Gets the one-based row of the caret.
	/// </summary>
	int CurrentRow { get; }

	/// <summary>
	/// Gets the one-based column of the caret.
	/// </summary>
	int CurrentColumn { get; }

	/// <summary>
	/// Gets the selected content as text, or <c>null</c> when there is no selection.
	/// </summary>
	string? SelectedContent { get; }

	/// <summary>
	/// Gets the length of the current selection.
	/// </summary>
	int SelectionLength { get; }

	/// <summary>
	/// Gets or sets the zoom level of the editor.
	/// </summary>
	int Zoom { get; set; }

	/// <summary>
	/// Gets or sets the minimum zoom level.
	/// </summary>
	int MinZoom { get; set; }

	/// <summary>
	/// Gets or sets the maximum zoom level.
	/// </summary>
	int MaxZoom { get; set; }

	/// <summary>
	/// Gets or sets the step size used when changing the zoom level.
	/// </summary>
	int ZoomStepSize { get; set; }

	/// <summary>
	/// Gets the default file extension used when saving the editor content.
	/// </summary>
	string DefaultFileExtension { get; }

	/// <summary>
	/// Gets or sets the engine version the editor targets.
	/// </summary>
	Version EngineVersion { get; set; }

	// Methods

	/// <summary>
	/// Loads the file at the given path into the editor.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	void Load(string filePath);

	/// <summary>
	/// Loads the file at the given path into the editor using the given load options.
	/// </summary>
	/// <param name="filePath">The path of the file to load.</param>
	/// <param name="options">The load options that select the initial processing mode.</param>
	void Load(string filePath, DocumentLoadOptions options = default)
		=> Load(filePath);

	/// <summary>
	/// Saves the editor content to the current file path.
	/// </summary>
	void Save();

	/// <summary>
	/// Saves the editor content to the given file path.
	/// </summary>
	/// <param name="filePath">The path of the file to save to.</param>
	void Save(string filePath);

	/// <summary>
	/// Reverts the most recent document edit.
	/// </summary>
	void Undo();

	/// <summary>
	/// Reapplies the most recently reverted document edit.
	/// </summary>
	void Redo();

	/// <summary>
	/// Cuts the current selection to the clipboard.
	/// </summary>
	void Cut();

	/// <summary>
	/// Copies the current selection to the clipboard.
	/// </summary>
	void Copy();

	/// <summary>
	/// Pastes the clipboard content at the caret.
	/// </summary>
	void Paste();

	/// <summary>
	/// Selects the entire editor content.
	/// </summary>
	void SelectAll();

	/// <summary>
	/// Applies the given configuration to the editor.
	/// </summary>
	/// <param name="configuration">The configuration to apply.</param>
	void UpdateSettings(Bases.ConfigurationBase configuration);

	/// <summary>
	/// Runs the content-changed worker if the editor has pending content changes.
	/// </summary>
	void RunContentChangedWorker();

	// Events

	/// <summary>
	/// Raised when a content-changed worker run completes.
	/// </summary>
	event EventHandler ContentChangedWorkerRunCompleted;

	/// <summary>
	/// Raised when the editor status (caret, selection or zoom) changes.
	/// </summary>
	event EventHandler StatusChanged;

	/// <summary>
	/// Raised when the zoom level changes.
	/// </summary>
	event EventHandler ZoomChanged;

	private sealed class NullScope : IDisposable
	{
		public void Dispose() { }
	}
}

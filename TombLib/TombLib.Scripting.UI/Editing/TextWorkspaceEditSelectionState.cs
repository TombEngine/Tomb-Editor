using ICSharpCode.AvalonEdit;
using System;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Stores editor selection and caret offsets captured before a workspace edit.
/// </summary>
/// <remarks>
/// Offsets use AvalonEdit's zero-based UTF-16 document offsets.
/// <see cref="SelectionEnd"/> is exclusive and is calculated from the selection start and length at capture time.
/// </remarks>
public sealed class TextWorkspaceEditSelectionState
{
	private TextWorkspaceEditSelectionState(string filePath, int selectionStart, int selectionEnd, int caretOffset)
	{
		FilePath = filePath;
		SelectionStart = selectionStart;
		SelectionEnd = selectionEnd;
		CaretOffset = caretOffset;
	}

	/// <summary>
	/// Gets the file path of the document associated with the captured editor state.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the zero-based selection start offset captured from the editor.
	/// </summary>
	public int SelectionStart { get; }

	/// <summary>
	/// Gets the zero-based exclusive end offset of the selection captured from the editor.
	/// </summary>
	public int SelectionEnd { get; }

	/// <summary>
	/// Gets the zero-based caret offset captured from the editor.
	/// </summary>
	public int CaretOffset { get; }

	/// <summary>
	/// Captures the current selection and caret offsets from an editor.
	/// </summary>
	/// <param name="editor">The editor whose selection and caret offsets are captured.</param>
	/// <param name="filePath">The file path of the document associated with the editor.</param>
	/// <returns>The captured selection state.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="editor"/> or <paramref name="filePath"/> is <see langword="null"/>.
	/// </exception>
	public static TextWorkspaceEditSelectionState Capture(TextEditor editor, string filePath)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(filePath);

		int selectionStart = editor.SelectionStart;
		int selectionEnd = selectionStart + editor.SelectionLength;

		return new TextWorkspaceEditSelectionState(filePath, selectionStart, selectionEnd, editor.CaretOffset);
	}
}

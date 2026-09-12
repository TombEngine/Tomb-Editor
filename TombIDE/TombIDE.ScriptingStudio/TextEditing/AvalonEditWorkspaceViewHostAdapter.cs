#nullable enable

using System;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.TextEditing;

/// <summary>
/// Adapts a <see cref="TextEditorBase"/> to the workspace view host contract,
/// mapping the view's semantic operations onto the editor's persistence behavior.
/// </summary>
internal sealed class AvalonEditWorkspaceViewHostAdapter : IAvalonEditWorkspaceViewHost
{
	private readonly TextEditorBase _editor;

	/// <summary>
	/// Initializes a new instance of the <see cref="AvalonEditWorkspaceViewHostAdapter"/> class.
	/// </summary>
	/// <param name="editor">The editor that backs the view.</param>
	public AvalonEditWorkspaceViewHostAdapter(TextEditorBase editor)
	{
		_editor = editor ?? throw new ArgumentNullException(nameof(editor));
	}

	/// <inheritdoc/>
	public ITextEditTarget? ActiveEditTarget
	{
		get => _editor.WorkspaceEditTarget;
		set => _editor.WorkspaceEditTarget = value;
	}

	/// <inheritdoc/>
	public string FilePath
	{
		get => _editor.FilePath;
		set => _editor.FilePath = value;
	}

	/// <inheritdoc/>
	public bool IsContentChanged
	{
		get => _editor.IsContentChanged;
		set => _editor.IsContentChanged = value;
	}

	/// <inheritdoc/>
	public void ApplyAuthoritativeContent(string filePath, string content)
		=> _editor.ApplyWorkspaceContent(filePath, content);

	/// <inheritdoc/>
	public void ApplyAuthoritativeBaseline(string filePath, string content)
		=> _editor.ApplyWorkspaceBaseline(filePath, content);

	/// <inheritdoc/>
	public void RecordPersistedContent(string content)
		=> _editor.SetWorkspacePersistedContent(content);

	/// <inheritdoc/>
	public void ProcessContentChange(string content)
		=> _editor.RunContentChangedWorker(content);
}

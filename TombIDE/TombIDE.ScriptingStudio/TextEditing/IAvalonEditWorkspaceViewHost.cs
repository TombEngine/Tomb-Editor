#nullable enable

using Nickelony.IDEKit.Core.Text;

namespace TombIDE.ScriptingStudio.TextEditing;

/// <summary>
/// Exposes the host-specific editor state that the AvalonEdit workspace view requires.
/// </summary>
internal interface IAvalonEditWorkspaceViewHost
{
	/// <summary>Gets or sets the editor's active workspace edit target.</summary>
	ITextEditTarget? ActiveEditTarget { get; set; }

	/// <summary>Gets or sets the editor's file path.</summary>
	string FilePath { get; set; }

	/// <summary>Gets or sets whether the editor has unsaved changes.</summary>
	bool IsContentChanged { get; set; }

	/// <summary>Applies workspace-owned content without performing another file-system read.</summary>
	void ApplyAuthoritativeContent(string filePath, string content);

	/// <summary>Initializes workspace path and persistence state without replacing existing content.</summary>
	void ApplyAuthoritativeBaseline(string filePath, string content);

	/// <summary>Records workspace content as the persisted baseline.</summary>
	void RecordPersistedContent(string content);

	/// <summary>Runs the content-change worker check for the given content.</summary>
	void ProcessContentChange(string content);
}

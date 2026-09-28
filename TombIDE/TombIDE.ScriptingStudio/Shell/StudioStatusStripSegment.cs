using System.Collections.Generic;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

public enum StudioStatusStripSegment
{
	CaretPosition,
	SelectionLength,
	Zoom,
	SyntaxPreview
}

/// <summary>
/// Provides status strip segments for a registered document editor.
/// </summary>
public interface IStudioDocumentStatusStripProvider
{
	/// <summary>
	/// Gets the status strip segments to display for the specified editor.
	/// </summary>
	IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor);
}

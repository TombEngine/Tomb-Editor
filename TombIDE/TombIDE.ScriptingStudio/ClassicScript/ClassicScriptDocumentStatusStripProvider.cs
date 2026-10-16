using System.Collections.Generic;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed class ClassicScriptDocumentStatusStripProvider : IStudioDocumentStatusStripProvider
{
	public IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor)
		=> [StudioStatusStripSegment.SyntaxPreview];
}

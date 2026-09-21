using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Provides document-level operations for GameFlowScript text, including
/// level-script detection and definition-location lookups.
/// </summary>
public interface IGameFlowScriptDocumentService
{
	/// <summary>
	/// Returns <c>true</c> when a level property line with the given <paramref name="levelName"/>
	/// exists in the document.
	/// </summary>
	bool IsLevelScriptDefined(ITextSnapshot source, string levelName);

	/// <summary>
	/// Finds the one-based line number of a section header or level property line
	/// matching the given <paramref name="objectName"/>. Returns <c>null</c> when
	/// no matching line is found.
	/// </summary>
	int? FindDocumentLineOfObject(ITextSnapshot source, string objectName, ObjectType type);
}

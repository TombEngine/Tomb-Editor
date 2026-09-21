#nullable enable

namespace TombIDE.ScriptingStudio.Workspace;

/// <summary>
/// Host-side contract for editor projections: exposes the normalized document id a view is bound to,
/// which the controller uses to map editors to tracked workspace documents.
/// </summary>
internal interface IWorkspaceScriptView
{
	/// <summary>Gets the normalized document id the view is currently bound to.</summary>
	string DocumentId { get; }
}

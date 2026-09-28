#nullable enable

using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;

namespace TombIDE.ScriptingStudio.Workspace;

/// <summary>
/// Exposes the host-side pending-edit discard operation of a workspace view.
/// </summary>
/// <remarks>
/// The workspace manager no longer requires this operation on <see cref="IWorkspaceDocumentView"/>;
/// the host applies it directly when the user resolves an external conflict in favor of the disk
/// content.
/// </remarks>
internal interface IWorkspaceViewPendingEdits
{
	/// <summary>Discards the view's pending edits and applies the authoritative snapshot content.</summary>
	/// <param name="snapshot">The authoritative workspace snapshot to apply.</param>
	/// <returns>The refresh outcome; any status other than refreshed leaves the view unsynchronized.</returns>
	WorkspaceDocumentViewRefreshResult DiscardPendingEdits(WorkspaceDocumentSnapshot snapshot);
}

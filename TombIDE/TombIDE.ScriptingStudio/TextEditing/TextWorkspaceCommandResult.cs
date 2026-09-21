#nullable enable

using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Workspace.Editing;

namespace TombIDE.ScriptingStudio.TextEditing;

internal enum TextWorkspaceCommandStatus
{
	Applied,
	NoChanges,
	Cancelled,
	ValidationFailed,
	PartiallyApplied
}

internal readonly record struct TextWorkspaceCommandResult(
	TextWorkspaceCommandStatus Status,
	WorkspaceEditApplicationResult? ApplicationResult)
{
	public WorkspaceEditChangeSet? ChangeSet => ApplicationResult?.ChangeSet;

	public static TextWorkspaceCommandResult NoChanges { get; } = new(TextWorkspaceCommandStatus.NoChanges, null);

	public static TextWorkspaceCommandResult Cancelled { get; } = new(TextWorkspaceCommandStatus.Cancelled, null);

	public static TextWorkspaceCommandResult Applied(WorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.Applied, result);

	public static TextWorkspaceCommandResult ValidationFailed(WorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.ValidationFailed, result);

	public static TextWorkspaceCommandResult PartiallyApplied(WorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.PartiallyApplied, result);
}

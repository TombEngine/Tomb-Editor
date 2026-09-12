#nullable enable

using Nickelony.IDEKit.Core.Editing;

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
	TextWorkspaceEditApplicationResult? ApplicationResult)
{
	public TextWorkspaceEditTransaction? Transaction => ApplicationResult?.ChangeSet;

	public static TextWorkspaceCommandResult NoChanges { get; } = new(TextWorkspaceCommandStatus.NoChanges, null);

	public static TextWorkspaceCommandResult Cancelled { get; } = new(TextWorkspaceCommandStatus.Cancelled, null);

	public static TextWorkspaceCommandResult Applied(TextWorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.Applied, result);

	public static TextWorkspaceCommandResult ValidationFailed(TextWorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.ValidationFailed, result);

	public static TextWorkspaceCommandResult PartiallyApplied(TextWorkspaceEditApplicationResult result)
		=> new(TextWorkspaceCommandStatus.PartiallyApplied, result);
}

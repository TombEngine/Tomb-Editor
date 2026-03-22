namespace TombIDE.ProjectMaster.Services.Level.Rename;

/// <summary>
/// Result of a level rename operation.
/// </summary>
public sealed record LevelRenameResult
{
	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	/// Indicates whether any changes were made.
	/// </summary>
	public bool ChangesMade { get; init; }

	/// <summary>
	/// Indicates whether a script rename is needed.
	/// </summary>
	public bool ScriptRenameNeeded { get; init; }

	/// <summary>
	/// The old level name (for script rename).
	/// </summary>
	public string? OldName { get; init; }

	/// <summary>
	/// The new level name (for script rename).
	/// </summary>
	public string? NewName { get; init; }
}

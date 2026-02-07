namespace TombIDE.ProjectMaster.Services.Level.Rename;

/// <summary>
/// Options for renaming a level.
/// </summary>
public sealed record LevelRenameOptions
{
	/// <summary>
	/// The new name for the level.
	/// </summary>
	public string NewName { get; init; } = string.Empty;

	/// <summary>
	/// Whether to rename the level's directory as well.
	/// </summary>
	public bool RenameDirectory { get; init; }

	/// <summary>
	/// Whether to rename the script entry as well.
	/// </summary>
	public bool RenameScriptEntry { get; init; }
}

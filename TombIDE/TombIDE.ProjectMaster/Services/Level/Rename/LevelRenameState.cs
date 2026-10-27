namespace TombIDE.ProjectMaster.Services.Level.Rename;

/// <summary>
/// Information about the rename checkbox states.
/// </summary>
public sealed class LevelRenameState
{
	/// <summary>
	/// Whether the directory rename checkbox should be enabled.
	/// </summary>
	public bool CanRenameDirectory { get; set; }

	/// <summary>
	/// Whether the directory rename checkbox should be checked by default.
	/// </summary>
	public bool ShouldRenameDirectory { get; set; }

	/// <summary>
	/// The text to display for the directory rename checkbox.
	/// </summary>
	public string DirectoryRenameText { get; set; } = "Rename level folder as well (Recommended)";

	/// <summary>
	/// Whether the script rename checkbox should be enabled.
	/// </summary>
	public bool CanRenameScript { get; set; }

	/// <summary>
	/// Whether the script rename checkbox should be checked by default.
	/// </summary>
	public bool ShouldRenameScript { get; set; }

	/// <summary>
	/// The text to display for the script rename checkbox.
	/// </summary>
	public string ScriptRenameText { get; set; } = "Rename script entry as well (Recommended)";

	/// <summary>
	/// Whether to show the script error label.
	/// </summary>
	public bool ShowScriptError { get; set; }

	/// <summary>
	/// Whether to show the language error label.
	/// </summary>
	public bool ShowLanguageError { get; set; }
}

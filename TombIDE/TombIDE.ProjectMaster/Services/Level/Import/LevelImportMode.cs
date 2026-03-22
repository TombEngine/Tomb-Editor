namespace TombIDE.ProjectMaster.Services.Level.Import;

/// <summary>
/// Specifies how to import a level.
/// </summary>
public enum LevelImportMode
{
	/// <summary>
	/// Copy only the specified .prj2 file into the project's Levels folder.
	/// </summary>
	CopySpecifiedFile,

	/// <summary>
	/// Copy selected .prj2 files from the source directory into the project's Levels folder.
	/// </summary>
	CopySelectedFiles,

	/// <summary>
	/// Keep files in their original location and link to them as an external level.
	/// </summary>
	KeepInPlace
}

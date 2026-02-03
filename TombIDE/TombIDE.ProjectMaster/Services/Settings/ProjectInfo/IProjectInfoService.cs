using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.ProjectInfo;

/// <summary>
/// Provides functionality for displaying and formatting project information.
/// </summary>
public interface IProjectInfoService
{
	/// <summary>
	/// Formats a file path for display based on user preference.
	/// </summary>
	/// <param name="filePath">The full file path.</param>
	/// <param name="projectBasePath">The project's base directory.</param>
	/// <param name="showFullPaths">Whether to show full paths or relative paths.</param>
	/// <returns>The formatted path string.</returns>
	string FormatPathForDisplay(string filePath, string projectBasePath, bool showFullPaths);

	/// <summary>
	/// Gets the display text for the script directory path.
	/// </summary>
	/// <param name="project">The project to get the script path from.</param>
	/// <param name="showFullPaths">Whether to show full paths or relative paths.</param>
	/// <returns>The formatted script directory path.</returns>
	string GetScriptDirectoryDisplay(IGameProject project, bool showFullPaths);

	/// <summary>
	/// Gets the display text for the levels directory path.
	/// </summary>
	/// <param name="project">The project to get the levels path from.</param>
	/// <param name="showFullPaths">Whether to show full paths or relative paths.</param>
	/// <returns>The formatted levels directory path.</returns>
	string GetLevelsDirectoryDisplay(IGameProject project, bool showFullPaths);

	/// <summary>
	/// Changes the script directory for a project.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="newScriptDirectory">The new script directory path.</param>
	void ChangeScriptDirectory(IGameProject project, string newScriptDirectory);

	/// <summary>
	/// Changes the levels directory for a project.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="newLevelsDirectory">The new levels directory path.</param>
	void ChangeLevelsDirectory(IGameProject project, string newLevelsDirectory);

	/// <summary>
	/// Validates if a directory path is suitable for use as a script/levels directory.
	/// </summary>
	/// <param name="directoryPath">The directory path to validate.</param>
	/// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
	bool IsValidDirectory(string directoryPath);
}

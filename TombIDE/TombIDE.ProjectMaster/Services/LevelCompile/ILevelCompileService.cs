using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.LevelCompile;

/// <summary>
/// Provides functionality for batch building levels.
/// </summary>
public interface ILevelCompileService
{
	/// <summary>
	/// Performs a batch rebuild of all levels in the project.
	/// </summary>
	/// <param name="project">The project containing levels to rebuild.</param>
	/// <returns><see langword="true"/> if levels were found and the rebuild was initiated; <see langword="false"/> if no levels exist in the project.</returns>
	bool RebuildAllLevels(IGameProject project);

	/// <summary>
	/// Performs a rebuild of a single level.
	/// </summary>
	/// <param name="level">The level to rebuild.</param>
	void RebuildLevel(ILevelProject level);
}

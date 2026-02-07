using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Level.Setup;

/// <summary>
/// Provides functionality for creating new levels in a project.
/// </summary>
public interface ILevelSetupService
{
	/// <summary>
	/// Creates a new level project in the target project's Levels directory.
	/// </summary>
	/// <param name="targetProject">The project to create the level in.</param>
	/// <param name="options">The level setup options.</param>
	/// <returns>The result of the setup operation.</returns>
	LevelSetupResult CreateLevel(IGameProject targetProject, LevelSetupOptions options);
}

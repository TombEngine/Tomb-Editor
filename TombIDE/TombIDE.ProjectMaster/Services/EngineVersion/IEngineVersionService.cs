using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineVersion;

/// <summary>
/// Provides functionality for checking and managing engine versions.
/// </summary>
public interface IEngineVersionService
{
	/// <summary>
	/// Gets comprehensive version information for the specified project.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns>An <see cref="EngineVersionInfo"/> containing current and latest version information.</returns>
	EngineVersionInfo GetVersionInfo(IGameProject project);
}

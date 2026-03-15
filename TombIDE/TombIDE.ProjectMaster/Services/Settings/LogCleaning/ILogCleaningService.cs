using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.LogCleaning;

/// <summary>
/// Provides functionality for cleaning log files and error dumps from the project.
/// </summary>
public interface ILogCleaningService
{
	/// <summary>
	/// Deletes all log files and error dumps from the project's engine directory.
	/// </summary>
	/// <param name="project">The project to clean logs for.</param>
	/// <returns>The number of files deleted.</returns>
	int DeleteAllLogFiles(IGameProject project);
}

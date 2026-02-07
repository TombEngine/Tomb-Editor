using TombIDE.Shared;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Level.Import;

/// <summary>
/// Provides functionality for importing existing levels into a project.
/// </summary>
public interface ILevelImportService
{
	/// <summary>
	/// Imports a level into the target project.
	/// </summary>
	/// <param name="targetProject">The project to import the level into.</param>
	/// <param name="options">The import options.</param>
	/// <param name="progressForm">Optional progress reporter form for file processing.</param>
	/// <returns>The result of the import operation.</returns>
	LevelImportResult ImportLevel(IGameProject targetProject, LevelImportOptions options, IProgressReportingForm? progressForm = null);
}

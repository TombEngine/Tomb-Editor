using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Level.Import;

/// <summary>
/// Result of a level import operation.
/// </summary>
public sealed record LevelImportResult
{
	/// <summary>
	/// The imported level project. Null if import failed.
	/// </summary>
	public ILevelProject? ImportedLevel { get; init; }

	/// <summary>
	/// Script generation result for the level, if requested.
	/// </summary>
	public ScriptGenerationResult? GeneratedScript { get; init; }

	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	public bool Success => ImportedLevel is not null;
}

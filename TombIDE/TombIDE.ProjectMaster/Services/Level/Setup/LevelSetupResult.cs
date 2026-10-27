using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Level.Setup;

/// <summary>
/// Result of a level setup operation.
/// </summary>
public sealed record LevelSetupResult
{
	/// <summary>
	/// The created level project. Null if creation failed.
	/// </summary>
	public ILevelProject? CreatedLevel { get; init; }

	/// <summary>
	/// Script generation result for the level, if requested.
	/// </summary>
	public ScriptGenerationResult? GeneratedScript { get; init; }

	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	public bool Success => CreatedLevel is not null;
}

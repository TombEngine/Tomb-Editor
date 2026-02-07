using System.Collections.Generic;
using TombIDE.Shared.NewStructure;

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
	/// Script lines generated for the level, if requested.
	/// </summary>
	public IReadOnlyList<string> GeneratedScriptLines { get; init; } = [];

	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	public bool Success => CreatedLevel is not null;
}

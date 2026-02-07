using System.Collections.Generic;
using TombIDE.Shared.NewStructure;

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
	/// Script lines generated for the level, if requested.
	/// </summary>
	public IReadOnlyList<string> GeneratedScriptLines { get; init; } = [];

	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	public bool Success => ImportedLevel is not null;
}

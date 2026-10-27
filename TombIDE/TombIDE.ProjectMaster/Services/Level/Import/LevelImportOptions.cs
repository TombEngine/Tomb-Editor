using System.Collections.Generic;

namespace TombIDE.ProjectMaster.Services.Level.Import;

/// <summary>
/// Options for importing a level.
/// </summary>
public sealed record LevelImportOptions
{
	/// <summary>
	/// The name for the imported level.
	/// </summary>
	public string LevelName { get; init; } = string.Empty;

	/// <summary>
	/// The full path to the originally specified .prj2 file.
	/// </summary>
	public string SourcePrj2FilePath { get; init; } = string.Empty;

	/// <summary>
	/// The custom data file name (without extension).
	/// </summary>
	public string DataFileName { get; init; } = string.Empty;

	/// <summary>
	/// The import mode to use.
	/// </summary>
	public LevelImportMode ImportMode { get; init; }

	/// <summary>
	/// When using <see cref="LevelImportMode.CopySelectedFiles"/>, the list of .prj2 file paths to copy.
	/// </summary>
	public IReadOnlyList<string> SelectedFilePaths { get; init; } = [];

	/// <summary>
	/// Whether to generate script lines for the level.
	/// </summary>
	public bool GenerateScript { get; init; }

	/// <summary>
	/// The ambient sound ID to use in generated script.
	/// </summary>
	public int AmbientSoundId { get; init; }

	/// <summary>
	/// Whether to enable horizon in the generated script.
	/// </summary>
	public bool EnableHorizon { get; init; }

	/// <summary>
	/// Whether to update .prj2 game settings when keeping files in place.
	/// Only used when <see cref="ImportMode"/> is <see cref="LevelImportMode.KeepInPlace"/>.
	/// </summary>
	public bool UpdatePrj2SettingsForExternalLevel { get; init; }
}

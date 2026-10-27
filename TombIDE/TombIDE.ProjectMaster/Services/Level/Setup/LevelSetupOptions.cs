namespace TombIDE.ProjectMaster.Services.Level.Setup;

/// <summary>
/// Options for setting up a new level.
/// </summary>
public sealed record LevelSetupOptions
{
	/// <summary>
	/// The name of the level.
	/// </summary>
	public string LevelName { get; init; } = string.Empty;

	/// <summary>
	/// The custom data file name (without extension).
	/// </summary>
	public string DataFileName { get; init; } = string.Empty;

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
}

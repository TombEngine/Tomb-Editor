#nullable enable

using System.Collections.Generic;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Represents a file that should be created as part of script generation.
/// </summary>
/// <param name="RelativePath">The path relative to the script root directory (e.g., "Levels\MyLevel.lua").</param>
/// <param name="Content">The full text content of the file.</param>
public sealed record GeneratedScriptFile(string RelativePath, string Content);

/// <summary>
/// Contains the results of script generation, with explicit separation between
/// gameflow script content and language file content.
/// </summary>
public sealed record ScriptGenerationResult(string DataFileName)
{
	/// <summary>
	/// The data file name (without extension) used to generate the script.
	/// </summary>
	public string DataFileName { get; init; } = DataFileName;

	/// <summary>
	/// Text to append to the main gameflow/script file.
	/// </summary>
	public string GameFlowScript { get; init; } = string.Empty;

	/// <summary>
	/// Text to insert into the language/strings file (e.g., TombEngine's Strings.lua).
	/// Empty for game versions that handle language strings via a separate mechanism.
	/// </summary>
	public string LanguageScript { get; init; } = string.Empty;

	/// <summary>
	/// New script files that should be created on disk, relative to the script root directory.
	/// </summary>
	public IReadOnlyList<GeneratedScriptFile> FilesToCreate { get; init; } = [];

	/// <summary>
	/// Whether any script content was generated.
	/// </summary>
	public bool HasContent => GameFlowScript.Length > 0 || LanguageScript.Length > 0;
}

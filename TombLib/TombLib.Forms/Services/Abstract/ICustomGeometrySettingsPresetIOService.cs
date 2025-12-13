using System.Collections.Generic;
using TombLib.GeometryIO;

namespace TombLib.Services.Abstract;

/// <summary>
/// Provides methods for loading and saving custom geometry settings presets.
/// </summary>
public interface ICustomGeometrySettingsPresetIOService
{
	/// <summary>
	/// Loads geometry settings presets from a file.
	/// </summary>
	/// <param name="filePath">Path to the file containing the presets.</param>
	/// <returns>A collection of geometry settings presets.</returns>
	IReadOnlyList<IOGeometrySettingsPreset> LoadPresets(string filePath);

	/// <summary>
	/// Saves geometry settings presets to a file.
	/// </summary>
	/// <param name="filePath">Path to the file where the presets will be saved.</param>
	/// <param name="presets">A collection of geometry settings presets to save.</param>
	/// <returns><see langword="true" /> if the presets were saved successfully; otherwise, <see langword="false" />.</returns>
	bool SavePresets(string filePath, IEnumerable<IOGeometrySettingsPreset> presets);
}

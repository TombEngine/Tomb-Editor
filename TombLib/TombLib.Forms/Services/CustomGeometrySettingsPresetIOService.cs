#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.GeometryIO;
using TombLib.Services.Abstract;
using TombLib.Utils;

namespace TombLib.Services;

/// <summary>
/// Service for loading and saving custom geometry settings presets to and from XML files.
/// </summary>
public sealed class CustomGeometrySettingsPresetIOService : ICustomGeometrySettingsPresetIOService
{
	public IReadOnlyList<IOGeometrySettingsPreset> LoadPresets(string filePath)
	{
		if (!File.Exists(filePath))
			return [];

		try
		{
			return XmlUtils.ReadXmlFile<List<IOGeometrySettingsPreset>>(filePath);
		}
		catch
		{
			return [];
		}
	}

	public bool SavePresets(string filePath, IEnumerable<IOGeometrySettingsPreset> presets)
	{
		try
		{
			string? directoryName = Path.GetDirectoryName(filePath);

			if (directoryName is not null && !Directory.Exists(directoryName))
				Directory.CreateDirectory(directoryName);

			XmlUtils.WriteXmlFile(filePath, presets.ToList());
			return true;
		}
		catch
		{
			return false;
		}
	}
}

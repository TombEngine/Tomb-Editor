using TombIDE.ProjectMaster.Services.Plugins.Models;

namespace TombIDE.ProjectMaster.Services.Plugins.Metadata;

/// <summary>
/// Provides functionality for reading plugin metadata.
/// </summary>
public interface IPluginMetadataService
{
	/// <summary>
	/// Reads metadata for a plugin from its directory.
	/// </summary>
	/// <param name="pluginDirectoryPath">The path to the plugin directory.</param>
	/// <param name="dllFileName">The name of the plugin DLL file.</param>
	/// <returns>Plugin metadata information.</returns>
	PluginInfo ReadPluginMetadata(string pluginDirectoryPath, string dllFileName);
}

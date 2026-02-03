using System.Collections.Generic;
using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Discovery;

/// <summary>
/// Provides functionality for discovering and enumerating plugins in a project.
/// </summary>
public interface IPluginDiscoveryService
{
	/// <summary>
	/// Discovers all installed plugins in the specified project.
	/// </summary>
	/// <param name="project">The project to search for plugins.</param>
	/// <returns>A collection of discovered plugins.</returns>
	IEnumerable<PluginInfo> DiscoverPlugins(IGameProject project);

	/// <summary>
	/// Gets detailed information about a specific plugin.
	/// </summary>
	/// <param name="pluginDirectoryPath">The path to the plugin directory.</param>
	/// <returns>Detailed plugin information, or <see langword="null"/> if not found.</returns>
	PluginInfo? GetPluginInfo(string pluginDirectoryPath);
}

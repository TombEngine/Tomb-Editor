using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Installation;

/// <summary>
/// Provides functionality for installing plugins into a project.
/// </summary>
public interface IPluginInstallationService
{
	/// <summary>
	/// Installs a plugin from the specified source.
	/// </summary>
	/// <param name="project">The project to install the plugin into.</param>
	/// <param name="source">The source location of the plugin.</param>
	/// <returns>The installed plugin information.</returns>
	PluginInfo InstallPlugin(IGameProject project, PluginInstallationSource source);

	/// <summary>
	/// Removes a plugin from the project.
	/// </summary>
	/// <param name="project">The project to remove the plugin from.</param>
	/// <param name="plugin">The plugin to remove.</param>
	void RemovePlugin(IGameProject project, PluginInfo plugin);
}

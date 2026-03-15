using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Deployment;

/// <summary>
/// Provides functionality for deploying plugin files to the engine directory.
/// </summary>
public interface IPluginDeploymentService
{
	/// <summary>
	/// Deploys all plugins from the project's plugin directory to the engine directory.
	/// </summary>
	/// <param name="project">The project containing the plugins.</param>
	void DeployPlugins(IGameProject project);

	/// <summary>
	/// Synchronizes plugin script reference files to enable IDE script editor support.
	/// </summary>
	/// <param name="project">The project whose plugin script references should be synchronized.</param>
	void SynchronizeScriptReferences(IGameProject project);
}

using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Initialization;

/// <summary>
/// Provides functionality for initializing the plugin directory structure.
/// </summary>
public interface IPluginInitializationService
{
	/// <summary>
	/// Ensures the plugin directory is properly initialized with default plugins if needed.
	/// </summary>
	/// <param name="project">The project to initialize.</param>
	void InitializePluginDirectory(IGameProject project);
}

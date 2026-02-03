using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombIDE.ProjectMaster.Services.Plugins.Deployment;

public sealed class TRNGPluginDeploymentService : IPluginDeploymentService
{
	private const string PluginDllPattern = "plugin_*.dll";
	private const string PluginScriptPattern = "plugin_*.script";

	public void DeployPlugins(IGameProject project)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
			return;

		string engineRootDirectoryPath = project.GetEngineRootDirectoryPath();

		// Copy all plugin DLL files to the engine directory
		foreach (FileInfo dllFile in pluginsDirectory.GetFiles(PluginDllPattern, SearchOption.AllDirectories))
		{
			string destinationPath = Path.Combine(engineRootDirectoryPath, dllFile.Name);
			dllFile.CopyTo(destinationPath, true);
		}
	}

	public void SynchronizeScriptReferences(IGameProject project)
	{
		// Delete all .script files from the internal /NGC/ folder
		foreach (string file in Directory.GetFiles(DefaultPaths.InternalNGCDirectory, PluginScriptPattern, SearchOption.TopDirectoryOnly))
			File.Delete(file);

		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
			return;

		// Copy all plugin .script files to the internal NGC directory
		foreach (FileInfo scriptFile in pluginsDirectory.GetFiles(PluginScriptPattern, SearchOption.AllDirectories))
		{
			string destinationPath = Path.Combine(DefaultPaths.InternalNGCDirectory, scriptFile.Name);
			scriptFile.CopyTo(destinationPath, true);
		}

		// Refresh mnemonic data
		MnemonicData.SetupConstants(DefaultPaths.InternalNGCDirectory);
	}
}

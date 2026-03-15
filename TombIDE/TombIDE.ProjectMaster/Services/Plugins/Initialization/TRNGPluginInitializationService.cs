using System.IO;
using System.IO.Compression;
using System.Linq;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Plugins.Initialization;

public sealed class TRNGPluginInitializationService : IPluginInitializationService
{
	private const string PluginDllPattern = "plugin_*.dll";

	public void InitializePluginDirectory(IGameProject project)
	{
		string pluginsPath = project.PluginsDirectoryPath;
		var pluginsDirectory = new DirectoryInfo(pluginsPath);

		if (pluginsDirectory.Exists && pluginsDirectory.EnumerateFiles("*", SearchOption.AllDirectories).Any())
			return;

		if (!pluginsDirectory.Exists)
		{
			pluginsDirectory.Create();
			pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
		}

		// TODO: Remove .parc support in future versions.
		// This was a dirty hack to keep a copy of the user's plugins when TIDE stored plugins internally, rather than in the project folder.
		// This is not the case anymore and .parc files are therefore deprecated.
		string parcPath = Path.Combine(project.GetEngineRootDirectoryPath(), "plugins.parc");

		if (File.Exists(parcPath))
			CopyPluginsFromPARCToProject(project, parcPath, pluginsDirectory);

		if (Directory.Exists(DefaultPaths.TRNGPluginsDirectory)) // Priority
			CopyPluginsFromTIDEToProject(project, pluginsDirectory);
	}

	private static void CopyPluginsFromTIDEToProject(IGameProject project, DirectoryInfo pluginsDirectory)
	{
		string[] internalPluginDirs = Directory.GetDirectories(DefaultPaths.TRNGPluginsDirectory, "*", SearchOption.TopDirectoryOnly);

		foreach (string pluginFile in Directory.GetFiles(project.GetEngineRootDirectoryPath(), PluginDllPattern, SearchOption.TopDirectoryOnly))
		{
			string? tidePluginDir = internalPluginDirs.FirstOrDefault(x =>
				Path.GetFileName(x).Equals(Path.GetFileNameWithoutExtension(pluginFile)));

			if (tidePluginDir is not null)
				SharedMethods.CopyFilesRecursively(tidePluginDir, Path.Combine(pluginsDirectory.FullName, Path.GetFileName(tidePluginDir)));
		}
	}

	private static void CopyPluginsFromPARCToProject(IGameProject project, string parcPath, DirectoryInfo pluginsDirectory)
	{
		using ZipArchive parc = ZipFile.OpenRead(parcPath);

		foreach (string pluginFile in Directory.GetFiles(project.GetEngineRootDirectoryPath(), PluginDllPattern, SearchOption.TopDirectoryOnly))
		{
			string pluginName = Path.GetFileNameWithoutExtension(pluginFile);

			foreach (ZipArchiveEntry entry in parc.Entries)
			{
				if (entry.FullName.Split('\\')[0].Equals(pluginName, System.StringComparison.OrdinalIgnoreCase))
				{
					string destPath = Path.Combine(pluginsDirectory.FullName, entry.FullName);
					string? dirName = Path.GetDirectoryName(destPath);

					if (dirName is not null && !Directory.Exists(dirName))
						Directory.CreateDirectory(dirName);

					entry.ExtractToFile(destPath, true);
				}
			}
		}
	}
}

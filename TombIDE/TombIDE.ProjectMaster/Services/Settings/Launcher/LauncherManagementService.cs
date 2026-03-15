using System;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.Launcher;

public sealed class LauncherManagementService : ILauncherManagementService
{
	public bool CanRenameLauncher(IGameProject project)
	{
		// Can't rename launcher if project directory equals engine directory (legacy structure)
		return !project.DirectoryPath.Equals(project.GetEngineRootDirectoryPath(), StringComparison.OrdinalIgnoreCase);
	}

	public string GetLauncherName(IGameProject project)
	{
		string launcherPath = project.GetLauncherFilePath();
		return Path.GetFileNameWithoutExtension(launcherPath);
	}

	public void RenameLauncher(IGameProject project, string newName)
	{
		if (!CanRenameLauncher(project))
			throw new InvalidOperationException("Cannot rename launcher in legacy project structure.");

		if (string.IsNullOrWhiteSpace(newName))
			throw new ArgumentException("Launcher name cannot be empty.");

		string currentPath = project.GetLauncherFilePath();

		if (!File.Exists(currentPath))
			throw new FileNotFoundException("Launcher file not found.");

		string directory = Path.GetDirectoryName(currentPath) ?? string.Empty;
		string newPath = Path.Combine(directory, newName + ".exe");

		if (File.Exists(newPath) && !newPath.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
			throw new IOException("A file with that name already exists.");

		File.Move(currentPath, newPath);
	}
}

using System;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.Launcher;

/// <summary>
/// Provides functionality for managing the game launcher executable.
/// </summary>
public interface ILauncherManagementService
{
	/// <summary>
	/// Gets the current launcher filename without extension.
	/// </summary>
	/// <param name="project">The project to get the launcher name from.</param>
	/// <returns>The launcher filename without .exe extension.</returns>
	string GetLauncherName(IGameProject project);

	/// <summary>
	/// Renames the project launcher executable.
	/// </summary>
	/// <param name="project">The project containing the launcher.</param>
	/// <param name="newName">The new name for the launcher (without .exe extension).</param>
	/// <exception cref="InvalidOperationException">Thrown when launcher cannot be renamed (legacy projects).</exception>
	/// <exception cref="ArgumentException">Thrown when the new name is invalid.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the launcher file doesn't exist.</exception>
	/// <exception cref="IOException">Thrown when a file with the new name already exists.</exception>
	void RenameLauncher(IGameProject project, string newName);

	/// <summary>
	/// Validates if the launcher can be renamed.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if launcher can be renamed; otherwise, <see langword="false"/>.</returns>
	bool CanRenameLauncher(IGameProject project);
}

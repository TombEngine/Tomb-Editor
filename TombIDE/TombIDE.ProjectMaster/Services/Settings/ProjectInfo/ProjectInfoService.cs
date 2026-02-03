using System;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.ProjectInfo;

public sealed class ProjectInfoService : IProjectInfoService
{
	public string FormatPathForDisplay(string filePath, string projectBasePath, bool showFullPaths)
	{
		if (showFullPaths || string.IsNullOrWhiteSpace(projectBasePath))
			return filePath;

		// Return path with $(ProjectDirectory) placeholder if inside project directory
		if (filePath.StartsWith(projectBasePath, StringComparison.OrdinalIgnoreCase))
			return filePath.Replace(projectBasePath, "$(ProjectDirectory)");

		return filePath;
	}

	public string GetScriptDirectoryDisplay(IGameProject project, bool showFullPaths)
	{
		string scriptPath = project.GetScriptRootDirectory();
		return FormatPathForDisplay(scriptPath, project.DirectoryPath, showFullPaths);
	}

	public string GetLevelsDirectoryDisplay(IGameProject project, bool showFullPaths)
	{
		string levelsPath = project.LevelsDirectoryPath;
		return FormatPathForDisplay(levelsPath, project.DirectoryPath, showFullPaths);
	}

	public void ChangeScriptDirectory(IGameProject project, string newScriptDirectory)
	{
		if (!IsValidDirectory(newScriptDirectory))
			throw new ArgumentException("Invalid directory path.");

		// Validation: script.txt must exist in the directory
		if (!File.Exists(Path.Combine(newScriptDirectory, "script.txt")))
			throw new ArgumentException("Selected /Script/ folder does not contain a Script.txt file.");

		// TODO: Take this responsibility away from the IDE class
	}

	public void ChangeLevelsDirectory(IGameProject project, string newLevelsDirectory)
	{
		if (!IsValidDirectory(newLevelsDirectory))
			throw new ArgumentException("Invalid directory path.");

		// TODO: Take this responsibility away from the IDE class
	}

	public bool IsValidDirectory(string directoryPath)
	{
		if (string.IsNullOrWhiteSpace(directoryPath))
			return false;

		try
		{
			// Check if path is valid format
			Path.GetFullPath(directoryPath);
			return true;
		}
		catch
		{
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Level.Import;

public sealed class LevelImportService : ILevelImportService
{
	public LevelImportResult ImportLevel(IGameProject targetProject, LevelImportOptions options, IProgressReportingForm? progressForm = null)
	{
		string levelName = LevelNameHelper.ValidateLevelName(options.LevelName)
			?? throw new ArgumentException("You must enter a valid name for the level.");

		string dataFileName = LevelNameHelper.ValidateDataFileName(options.DataFileName)
			?? throw new ArgumentException("You must specify a valid custom DATA file name.");

		if (options.ImportMode == LevelImportMode.CopySelectedFiles && options.SelectedFilePaths.Count == 0)
			throw new ArgumentException("You must select which .prj2 files you want to import.");

		return options.ImportMode switch
		{
			LevelImportMode.CopySpecifiedFile => ImportCopySpecifiedFile(targetProject, options, levelName, dataFileName),
			LevelImportMode.CopySelectedFiles => ImportCopySelectedFiles(targetProject, options, levelName, dataFileName, progressForm),
			LevelImportMode.KeepInPlace => ImportKeepInPlace(targetProject, options, levelName, dataFileName, progressForm),
			_ => throw new ArgumentException($"Unknown import mode: {options.ImportMode}")
		};
	}

	private static LevelImportResult ImportCopySpecifiedFile(
		IGameProject targetProject,
		LevelImportOptions options,
		string levelName,
		string dataFileName)
	{
		string levelFolderPath = Path.Combine(targetProject.LevelsDirectoryPath, levelName);
		string specificFileName = Path.GetFileName(options.SourcePrj2FilePath);

		EnsureLevelFolderEmpty(levelFolderPath);

		// Copy the specified file
		string destinationPath = Path.Combine(levelFolderPath, specificFileName);
		File.Copy(options.SourcePrj2FilePath, destinationPath);

		// Update settings for the copied file
		Prj2Helper.UpdateGameSettings(destinationPath, targetProject, dataFileName);

		return CreateLevelAndResult(targetProject, options, levelName, levelFolderPath, dataFileName, specificFileName);
	}

	private static LevelImportResult ImportCopySelectedFiles(
		IGameProject targetProject,
		LevelImportOptions options,
		string levelName,
		string dataFileName,
		IProgressReportingForm? progress)
	{
		string levelFolderPath = Path.Combine(targetProject.LevelsDirectoryPath, levelName);
		string specificFileName = Path.GetFileName(options.SourcePrj2FilePath);

		EnsureLevelFolderEmpty(levelFolderPath);

		// Check if the originally specified file was selected
		bool specificFileSelected = false;

		// Copy all selected files
		foreach (string sourcePath in options.SelectedFilePaths)
		{
			string fileName = Path.GetFileName(sourcePath);

			if (fileName == specificFileName)
				specificFileSelected = true;

			string destinationPath = Path.Combine(levelFolderPath, fileName);
			File.Copy(sourcePath, destinationPath);
		}

		// If the specified file wasn't selected, set target to null
		string? targetFileName = specificFileSelected ? specificFileName : null;

		// Update settings for all copied files
		UpdateAllPrj2Files(levelFolderPath, targetProject, dataFileName, progress);

		return CreateLevelAndResult(targetProject, options, levelName, levelFolderPath, dataFileName, targetFileName);
	}

	private static LevelImportResult ImportKeepInPlace(
		IGameProject targetProject,
		LevelImportOptions options,
		string levelName,
		string dataFileName,
		IProgressReportingForm? progress)
	{
		string levelFolderPath = Path.GetDirectoryName(options.SourcePrj2FilePath)
			?? throw new ArgumentException("Could not determine the directory of the source file.");

		string specificFileName = Path.GetFileName(options.SourcePrj2FilePath);

		// Update settings if requested
		if (options.UpdatePrj2SettingsForExternalLevel)
			UpdateAllPrj2Files(levelFolderPath, targetProject, dataFileName, progress);

		return CreateLevelAndResult(targetProject, options, levelName, levelFolderPath, dataFileName, specificFileName);
	}

	private static void EnsureLevelFolderEmpty(string levelFolderPath)
	{
		if (!Directory.Exists(levelFolderPath))
			Directory.CreateDirectory(levelFolderPath);

		if (Directory.EnumerateFileSystemEntries(levelFolderPath).Any())
		{
			throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
				"the project's /Levels/ folder and it's not empty.");
		}
	}

	private static void UpdateAllPrj2Files(
		string levelFolderPath,
		IGameProject targetProject,
		string dataFileName,
		IProgressReportingForm? progress)
	{
		string[] files = Directory.GetFiles(levelFolderPath, "*.prj2", SearchOption.TopDirectoryOnly);

		progress?.SetTotalProgress(files.Length);

		foreach (string file in files)
		{
			if (!Prj2Helper.IsBackupFile(file))
				Prj2Helper.UpdateGameSettings(file, targetProject, dataFileName);

			progress?.IncrementProgress(1);
		}
	}

	private static LevelImportResult CreateLevelAndResult(
		IGameProject targetProject,
		LevelImportOptions options,
		string levelName,
		string levelFolderPath,
		string dataFileName,
		string? targetFileName)
	{
		var importedLevel = new LevelProject(levelName, levelFolderPath, targetFileName);
		IReadOnlyList<string> generatedScriptLines = [];

		if (options.GenerateScript)
			generatedScriptLines = ScriptLineGenerator.GenerateScriptLines(levelName, dataFileName, targetProject.GameVersion, options.AmbientSoundId, options.EnableHorizon);

		var result = new LevelImportResult
		{
			ImportedLevel = importedLevel,
			GeneratedScriptLines = generatedScriptLines
		};

		importedLevel.Save();

		return result;
	}
}

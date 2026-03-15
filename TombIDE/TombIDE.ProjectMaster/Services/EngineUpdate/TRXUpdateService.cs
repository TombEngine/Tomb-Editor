using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

/// <summary>
/// Unified update service for TRX-based engines (TR1X and TR2X).
/// </summary>
public sealed class TRXUpdateService : IEngineUpdateService
{
	private static readonly Version MinAutoUpdateVersion = new(1, 3, 1);

	private readonly IFileExtractionService _fileExtractionService;
	private readonly TRVersion.Game _gameVersion;

	/// <summary>
	/// Maps game versions to their corresponding preset archive names.
	/// </summary>
	private static readonly IReadOnlyDictionary<TRVersion.Game, string> PresetArchiveNames = new Dictionary<TRVersion.Game, string>
	{
		{ TRVersion.Game.TR1, "TR1.zip" },
		{ TRVersion.Game.TR1X, "TR1.zip" },
		{ TRVersion.Game.TR2X, "TR2X.zip" }
	};

	public TRXUpdateService(IFileExtractionService fileExtractionService, TRVersion.Game gameVersion)
	{
		_fileExtractionService = fileExtractionService;

		if (!PresetArchiveNames.ContainsKey(gameVersion))
			throw new ArgumentException($"Unsupported game version: {gameVersion}", nameof(gameVersion));

		_gameVersion = gameVersion;
	}

	public bool CanAutoUpdate(Version currentVersion, [NotNullWhen(false)] out string? blockReason)
	{
		if (currentVersion < MinAutoUpdateVersion)
		{
			blockReason = "Cannot Auto-Update engine. TRX 1.3 introduced breaking changes, which require manual migration.";
			return false;
		}

		blockReason = null;
		return true;
	}

	public bool UpdateEngine(IGameProject project, Version currentVersion, Version latestVersion, IWin32Window owner)
	{
		if (!CanAutoUpdate(currentVersion, out string? blockReason))
		{
			MessageBox.Show(owner, blockReason,
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}

		DialogResult result = MessageBox.Show(owner,
			"This update will replace the following directories and files:\n\n" +

			"- Engine/shaders/\n" +
			"- Engine/TRX.exe\n\n" +

			"If any of these directories / files are important to you, please update the engine manually or create a copy of these files before performing this update.\n\n" +

			"Are you sure you want to continue?\n" +
			"This action cannot be reverted.",
			"Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

		if (result is not DialogResult.Yes)
			return false;

		string engineDirectoryPath = Path.Combine(project.DirectoryPath, "Engine");

		if (!Directory.Exists(engineDirectoryPath))
		{
			DarkMessageBox.Show(owner, "Couldn't locate \"Engine\" directory. Updating is not supported for your project structure.",
				"Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}

		try
		{
			string presetArchiveName = PresetArchiveNames[_gameVersion];
			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, presetArchiveName);
			using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

			var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/shaders")).ToList();
			_fileExtractionService.ExtractEntries(shaders, project.DirectoryPath);

			var executables = engineArchive.Entries.Where(entry => entry.FullName.EndsWith(".exe")).ToList();
			_fileExtractionService.ExtractEntries(executables, project.DirectoryPath);

			DarkMessageBox.Show(owner, "Engine has been updated successfully!", "Done.",
				MessageBoxButtons.OK, MessageBoxIcon.Information);

			return true;
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(owner, "An error occurred while updating the engine:\n\n" + ex.Message,
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}
	}
}

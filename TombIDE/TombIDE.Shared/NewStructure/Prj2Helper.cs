#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombIDE.Shared.NewStructure
{
	/// <summary>
	/// Provides helper methods for working with .prj2 (Tomb Editor project) files.
	/// </summary>
	public static class Prj2Helper
	{
		/// <summary>
		/// Determines whether a .prj2 file is a backup file by checking its name for backup suffixes
		/// or embedded date patterns.
		/// </summary>
		/// <param name="filePath">The full path to the .prj2 file.</param>
		/// <returns><see langword="true"/> if the file appears to be a backup; otherwise, <see langword="false"/>.</returns>
		public static bool IsBackupFile(string filePath)
		{
			try
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

				if (fileNameWithoutExtension.EndsWith(".backup", StringComparison.OrdinalIgnoreCase))
					return true;

				// 05-06-23
				if (DateTime.TryParse(fileNameWithoutExtension[..7], out _) || DateTime.TryParse(fileNameWithoutExtension[^7..], out _))
					return true;

				// 05-06-2023 || 2023-06-05
				if (DateTime.TryParse(fileNameWithoutExtension[..9], out _) || DateTime.TryParse(fileNameWithoutExtension[^9..], out _))
					return true;
			}
			catch { }

			return false;
		}

		/// <summary>
		/// Returns all non-backup .prj2 files in the specified directory.
		/// </summary>
		/// <param name="directoryPath">The directory to scan.</param>
		/// <param name="searchOption">
		/// Whether to search only the top directory or all subdirectories.
		/// Defaults to <see cref="SearchOption.TopDirectoryOnly"/>.
		/// </param>
		/// <returns>A list of full paths to valid (non-backup) .prj2 files, or an empty list if the directory doesn't exist.</returns>
		public static IReadOnlyList<string> GetValidFiles(string directoryPath, SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			if (!Directory.Exists(directoryPath))
				return Array.Empty<string>();

			return Directory.GetFiles(directoryPath, "*.prj2", searchOption)
				.Where(file => !IsBackupFile(file))
				.ToList();
		}

		/// <summary>
		/// Loads a .prj2 file and updates its game settings to match the target project configuration.
		/// <para>
		/// This updates the game directory, executable path, script directory, game version,
		/// and optionally the output data file path. It then saves the modified file back to disk.
		/// </para>
		/// </summary>
		/// <param name="prj2FilePath">The full path to the .prj2 file to update.</param>
		/// <param name="destProject">The target project whose settings should be applied.</param>
		/// <param name="dataFileName">
		/// Optional custom data file name (without extension). If <see langword="null"/> or empty,
		/// the existing data file name is preserved and only its directory path is updated.
		/// </param>
		public static void UpdateGameSettings(string prj2FilePath, IGameProject destProject, string? dataFileName = null)
		{
			Level level = Prj2Loader.LoadFromPrj2(prj2FilePath, null, CancellationToken.None, new Prj2Loader.Settings());

			string exeFilePath = destProject.GetEngineExecutableFilePath();
			string engineDirectory = destProject.GetEngineRootDirectoryPath();

			level.Settings.LevelFilePath = prj2FilePath;

			level.Settings.GameDirectory = level.Settings.MakeRelative(engineDirectory, VariableType.LevelDirectory);
			level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
			level.Settings.ScriptDirectory = level.Settings.MakeRelative(destProject.GetScriptRootDirectory(), VariableType.LevelDirectory);
			level.Settings.GameVersion = destProject.GameVersion is TRVersion.Game.TR1 ? TRVersion.Game.TR1X : destProject.GameVersion;

			if (string.IsNullOrWhiteSpace(dataFileName))
			{
				string? fileName = Path.GetFileName(level.Settings.GameLevelFilePath);

				if (fileName is not null)
				{
					string filePath = Path.Combine(engineDirectory, "data", fileName);
					level.Settings.GameLevelFilePath = level.Settings.MakeRelative(filePath, VariableType.LevelDirectory);
				}
			}
			else
			{
				string fileName = dataFileName + destProject.DataFileExtension;
				string filePath = Path.Combine(engineDirectory, "data", fileName);

				level.Settings.GameLevelFilePath = level.Settings.MakeRelative(filePath, VariableType.LevelDirectory);
			}

			Prj2Writer.SaveToPrj2(prj2FilePath, level);
		}
	}
}

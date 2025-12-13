using System;
using System.Diagnostics;
using System.IO;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public static class GameProjectIdentifier
	{
		private const string TemplateSpecificDirectory = "Engine"; // Hardcoded engine root directory name for modern projects
		private const string OriginalLauncherFileName = "launch.exe"; // Used for file identification (TODO: Add a GUID for identifying instead!)

		private static readonly string[] ValidEngineExecutableNames = new string[]
		{
			"Tomb1Main.exe", "TR1X.exe", "TR2X.exe", "Tomb2.exe", "tomb3.exe", "tomb4.exe", "TombEngine.exe" // Only the ones TIDE currently supports
		};

		private static readonly string[] PlatformSpecificDirectories = new string[] // TEN only
		{
			"Bin\\x86", "Bin\\x64"
		};

		/// <param name="directoryPath">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// A DTO object with all the necessary information about the project.
		/// </returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static GameProjectDTO GetProjectFromDirectory(string directoryPath)
		{
			string engineExecutable = FindEngineExecutable(directoryPath, out TRVersion.Game version);
			string launcherExecutable = FindLauncherExecutable(directoryPath);

			string projectDirectoryPath = Path.GetDirectoryName(launcherExecutable);
			string projectName = Path.GetFileName(projectDirectoryPath); // Name of project directory

			string levelsDirectory = FindLevelsDirectory(projectDirectoryPath);
			string scriptDirectory = FindScriptDirectory(projectDirectoryPath);
			string pluginsDirectory = FindPluginsDirectory(projectDirectoryPath);

			return new GameProjectDTO
			{
				ProjectName = projectName,
				RootDirectoryPath = projectDirectoryPath,

				GameVersion = version,

				EngineExecutableFilePath = engineExecutable,
				LauncherExecutableFilePath = launcherExecutable,

				LevelsDirectoryPath = levelsDirectory,
				ScriptDirectoryPath = scriptDirectory,
				PluginsDirectoryPath = pluginsDirectory
			};
		}

		/// <param name="searchingDirectory">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// The path to the engine executable. (e.g. tomb4.exe, TombEngine.exe, etc.)
		/// </returns>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static string FindEngineExecutable(string searchingDirectory, out TRVersion.Game version)
		{
			string result = string.Empty;
			string platformSpecificDirectory = PlatformSpecificDirectories[Environment.Is64BitOperatingSystem ? 1 : 0];

			string engineSubdirectory = Path.Combine(searchingDirectory, TemplateSpecificDirectory);

			if (Directory.Exists(engineSubdirectory))
				result = FindValidEngineExecutable(engineSubdirectory);

			if (string.IsNullOrEmpty(result))
			{
				engineSubdirectory = Path.Combine(engineSubdirectory, platformSpecificDirectory);

				if (Directory.Exists(engineSubdirectory))
					result = FindValidEngineExecutable(engineSubdirectory);
			}

			if (string.IsNullOrEmpty(result))
			{
				engineSubdirectory = Path.Combine(searchingDirectory, platformSpecificDirectory);

				if (Directory.Exists(engineSubdirectory))
					result = FindValidEngineExecutable(engineSubdirectory);
			}

			if (string.IsNullOrEmpty(result))
				result = FindValidEngineExecutable(searchingDirectory);

			version = GetGameVersionFromExecutable(result);
			return result;
		}

		/// <param name="searchingDirectory">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// The path to either the PLAY.exe file or the engine executable if PLAY.exe doesn't exist.
		/// </returns>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static string FindLauncherExecutable(string searchingDirectory)
		{
			string launcherExecutable = FindValidLauncherExecutable(searchingDirectory);

			if (File.Exists(launcherExecutable))
				return launcherExecutable;

			// Search based on the engine executable location

			string engineExecutable = FindEngineExecutable(searchingDirectory, out TRVersion.Game version);

			if (string.IsNullOrEmpty(engineExecutable))
				throw new FileNotFoundException("Could not find a valid launcher executable.");

			string engineDirectory = Path.GetDirectoryName(engineExecutable);
			string engineDirectoryName = Path.GetFileName(engineDirectory);

			string result = null;

			switch (version)
			{
				case TRVersion.Game.TombEngine:
					if (engineDirectoryName.Equals("x64", StringComparison.OrdinalIgnoreCase) ||
						engineDirectoryName.Equals("x86", StringComparison.OrdinalIgnoreCase))
					{
						string grandGrandparentDirectory =
							Path.GetDirectoryName(
								Path.GetDirectoryName(
									Path.GetDirectoryName(engineDirectory))); // 3 steps back

						launcherExecutable = FindValidLauncherExecutable(grandGrandparentDirectory);

						if (File.Exists(launcherExecutable))
							result = launcherExecutable;
					}
					else // Possibly using old TEN structure (without /Bin/ directory)
					{
						string parentDirectory = Path.GetDirectoryName(engineDirectory);
						launcherExecutable = FindValidLauncherExecutable(parentDirectory);

						if (File.Exists(launcherExecutable))
							result = launcherExecutable;
					}

					break;

				default: // Tomb1Main, TR2, TR3, TR4, TRNG
					if (engineDirectoryName.Equals("Engine", StringComparison.OrdinalIgnoreCase))
					{
						string parentDirectory = Path.GetDirectoryName(engineDirectory);
						launcherExecutable = FindValidLauncherExecutable(parentDirectory);

						if (File.Exists(launcherExecutable))
							result = launcherExecutable;
					}

					break;
			}

			if (string.IsNullOrEmpty(result))
				result = engineExecutable;

			return result;
		}

		/// <param name="executableFilePath">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// The path to the project's "Levels" directory.
		/// </returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static string FindLevelsDirectory(string searchingDirectory)
		{
			string launcherExecutable = FindLauncherExecutable(searchingDirectory);

			if (string.IsNullOrEmpty(launcherExecutable))
				throw new FileNotFoundException("Could not find a valid launcher executable.");

			string launcherDirectory = Path.GetDirectoryName(launcherExecutable);
			string result = FindLevelsSubdirectory(launcherDirectory);

			if (string.IsNullOrEmpty(result))
			{
				string engineSubdirectory = Path.Combine(launcherExecutable, TemplateSpecificDirectory);

				if (Directory.Exists(engineSubdirectory))
					result = FindLevelsSubdirectory(engineSubdirectory);
			}

			return result;
		}

		/// <param name="searchingDirectory">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// The path to the project's "Script" directory.
		/// </returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static string FindScriptDirectory(string searchingDirectory)
		{
			string engineExecutable = FindEngineExecutable(searchingDirectory, out TRVersion.Game version);

			if (string.IsNullOrEmpty(engineExecutable))
				throw new FileNotFoundException("Could not find the engine executable.");

			string engineDirectory = Path.GetDirectoryName(engineExecutable);
			string scriptsDirectory, result = null;

			switch (version)
			{
				case TRVersion.Game.TR1 or TRVersion.Game.TR2X:
					scriptsDirectory = Path.Combine(engineDirectory, "cfg");

					if (!Directory.Exists(scriptsDirectory))
						throw new DirectoryNotFoundException("The game's \"cfg\" directory could not be found.");

					if (!IsValidScriptDirectory(scriptsDirectory, version))
						throw new Exception("The game's \"cfg\" directory does not contain a valid gameflow file.");

					result = scriptsDirectory;
					break;

				case TRVersion.Game.TombEngine:
					string directoryName = Path.GetFileName(engineDirectory);

					if (directoryName.Equals("x64", StringComparison.OrdinalIgnoreCase) ||
						directoryName.Equals("x86", StringComparison.OrdinalIgnoreCase))
					{
						string grandparentDirectory = Path.GetDirectoryName(Path.GetDirectoryName(engineDirectory)); // 2 steps back
						scriptsDirectory = Path.Combine(grandparentDirectory, "Scripts");

						if (Directory.Exists(scriptsDirectory))
							result = scriptsDirectory;
					}
					else // Possibly using old TEN structure (without /Bin/ directory)
					{
						scriptsDirectory = Path.Combine(engineDirectory, "Scripts");

						if (Directory.Exists(scriptsDirectory))
							result = scriptsDirectory;
					}

					if (string.IsNullOrEmpty(result))
						throw new Exception("The game's \"Scripts\" directory could not be found.");

					if (!IsValidScriptDirectory(result, TRVersion.Game.TombEngine))
						throw new Exception($"The game's \"Scripts\" directory does not contain a valid {TENGameProject.MainScriptFileName} file.");

					break;

				default: // TR2, TR3, TR4, TRNG
					if (Path.GetFileName(engineDirectory).Equals("Engine", StringComparison.OrdinalIgnoreCase))
					{
						string parentDirectory = Path.GetDirectoryName(engineDirectory); // 1 step back
						scriptsDirectory = Path.Combine(parentDirectory, "Script");

						if (Directory.Exists(scriptsDirectory)) // Prioritize "Script" directory outside of "Engine" directory
							result = scriptsDirectory;
					}

					if (string.IsNullOrEmpty(result))
					{
						scriptsDirectory = Path.Combine(engineDirectory, "Script");

						if (Directory.Exists(scriptsDirectory))
							result = scriptsDirectory;
					}

					break;
			}

			return result;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="searchingDirectory">
		/// Path to the directory that contains the game's launcher or engine executable.
		/// </param>
		/// <returns>
		/// The path to the project's "Plugins" directory.
		/// </returns>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static string FindPluginsDirectory(string searchingDirectory) // TODO: Update this in the future!
		{
			FindEngineExecutable(searchingDirectory, out TRVersion.Game version);

			if (version is not TRVersion.Game.TRNG)
				return null;

			string launcherExecutable = FindLauncherExecutable(searchingDirectory);

			if (string.IsNullOrEmpty(launcherExecutable))
				throw new FileNotFoundException("Could not find a valid launcher executable.");

			string launcherDirectory = Path.GetDirectoryName(launcherExecutable);

			return Path.Combine(launcherDirectory, "Plugins");
		}

		/// <exception cref="NotSupportedException"></exception>
		public static TRVersion.Game GetGameVersionFromExecutable(string engineExecutableFilePath)
		{
			string fileName = Path.GetFileNameWithoutExtension(engineExecutableFilePath);

			TRVersion.Game version = fileName.ToUpper() switch
			{
				"TOMB1MAIN" => TRVersion.Game.TR1,
				"TR1X" => TRVersion.Game.TR1,
				"TR2X" => TRVersion.Game.TR2X,
				"TOMB2" => TRVersion.Game.TR2,
				"TOMB3" => TRVersion.Game.TR3,
				"TOMB4" => TRVersion.Game.TR4,
				"TOMBENGINE" => TRVersion.Game.TombEngine,
				_ => throw new NotSupportedException("Given game engine is either not supported or the engine's .exe file name has been changed."),
			};

			if (version == TRVersion.Game.TR4)
			{
				string trngDllFilePath = Path.Combine(Path.GetDirectoryName(engineExecutableFilePath), "Tomb_NextGeneration.dll");

				if (File.Exists(trngDllFilePath))
					version = TRVersion.Game.TRNG;
			}

			return version;
		}

		public static bool IsValidEngineExecutable(string filePath)
			=> Array.Exists(ValidEngineExecutableNames, validName => validName.Equals(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase));

		public static bool IsValidScriptDirectory(string directoryPath, TRVersion.Game targetGameVersion)
		{
			switch (targetGameVersion)
			{
				case TRVersion.Game.TR1 or TRVersion.Game.TR2X:
					return true; // File names will change soon, pass validation for now

				case TRVersion.Game.TombEngine:
					string[] luaFiles = Directory.GetFiles(directoryPath, "*.lua", SearchOption.TopDirectoryOnly);

					return Array.Exists(luaFiles, file
						=> Path.GetFileName(file).Equals(TENGameProject.MainScriptFileName, StringComparison.OrdinalIgnoreCase));

				default: // Core-structured projects
					string[] txtFiles = Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly);

					return Array.Exists(txtFiles, file
						=> Path.GetFileName(file).Equals(CoreStructuredProjectBase.MainScriptFileName, StringComparison.OrdinalIgnoreCase));
			}
		}

		private static string FindValidEngineExecutable(string searchingDirectory)
		{
			string[] files = Directory.GetFiles(searchingDirectory, "*.exe", SearchOption.TopDirectoryOnly);
			return Array.Find(files, file => IsValidEngineExecutable(file));
		}

		private static string FindValidLauncherExecutable(string searchingDirectory)
		{
			string[] files = Directory.GetFiles(searchingDirectory, "*.exe", SearchOption.TopDirectoryOnly);
			return Array.Find(files, file => FileVersionInfo.GetVersionInfo(file).OriginalFilename == OriginalLauncherFileName);
		}

		private static string FindLevelsSubdirectory(string searchingDirectory)
		{
			string levelsDirectory = Path.Combine(searchingDirectory, "Levels");
			string mapsDirectory = Path.Combine(searchingDirectory, "maps");

			if (Directory.Exists(levelsDirectory)) // Prioritize "Levels" directory over "maps" directory
				return levelsDirectory;

			if (Directory.Exists(mapsDirectory))
				return mapsDirectory;

			return null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public interface IGameProject : IProject
	{
		Version TargetTrprojVersion { get; }

		/// <summary>
		/// Game engine version. (e.g. <c>TR4</c>, <c>TRNG</c>, <c>TombEngine</c>, ...)
		/// </summary>
		TRVersion.Game GameVersion { get; }

		/// <summary>
		/// The level data file extension depending on the project's GameVersion. (e.g. <c>".tr4"</c>, <c>".trc"</c>, <c>".ten"</c>, ...)
		/// </summary>
		string DataFileExtension { get; }

		/// <summary>
		/// The .exe file name (not the path) of the engine executable, depending on the project's GameVersion. (e.g. <c>"tomb4.exe"</c>, <c>"PCTomb5.exe"</c>, <c>"TombEngine.exe"</c>, ...)
		/// </summary>
		string EngineExecutableFileName { get; }

		/// <summary>
		/// The path where all the project's newly created / imported levels are stored.
		/// </summary>
		string LevelsDirectoryPath { get; set; }

		/// <summary>
		/// The path where the project's plugins are stored (if the game engine supports them).
		/// </summary>
		string PluginsDirectoryPath { get; set; }

		/// <summary>
		/// The path of the main script file. (e.g. <c>"C:\...\Script\Script.txt"</c>, <c>"C:\...\Engine\Scripts\Gameflow.lua"</c>, ...)
		/// </summary>
		string MainScriptFilePath { get; }

		/// <summary>
		/// The name of the default game language chosen by the author. (e.g. <c>"English"</c>, <c>"German"</c>, <c>"French"</c>, ...)
		/// </summary>
		string DefaultGameLanguageName { get; set; }

		bool SupportsCustomScriptPaths { get; }
		bool SupportsPlugins { get; }

		List<string> KnownLevelProjectFilePaths { get; }

		/// <summary>
		/// A list of all available game languages. (e.g. <c>"English"</c>, <c>"German"</c>, <c>"French"</c>, ...)
		/// </summary>
		List<string> GameLanguageNames { get; }

		/// <summary>
		/// .trproj file name = game's .exe file name (tomb4, PCTomb5, ...) + ".trproj"
		/// </summary>
		string GetTrprojFilePath();

		/// <summary>
		/// Returns the path of the file, which launches the game. This may either be the engine executable file or a launcher file.
		/// </summary>
		string GetLauncherFilePath();

		/// <summary>
		/// Returns the path where the project's engine files are stored. (root directory may not contain the engine executable file in TombEngine based projects)
		/// </summary>
		string GetEngineRootDirectoryPath();

		/// <summary>
		/// Returns the path of the engine executable file. (e.g. <c>"C:\...\Engine\tomb4.exe"</c>, <c>"C:\...\Bin\x64\TombEngine.exe"</c>, ...)
		/// </summary>
		string GetEngineExecutableFilePath();

		/// <summary>
		/// Returns the path of the default file where language strings are stored. (e.g. <c>"C:\...\Script\English.txt"</c>, <c>"C:\...\Engine\Scripts\Strings.lua"</c>, ...)
		/// </summary>
		string GetDefaultGameLanguageFilePath();

		/// <summary>
		/// Returns a list of all valid .trlvl files in the project's Levels directory and external level file paths.
		/// </summary>
		FileInfo[] GetAllValidTrlvlFiles();

		/// <summary>
		/// Returns a list of all valid level projects in the project's Levels directory and external level file paths.
		/// </summary>
		ILevelProject[] GetAllValidLevelProjects();

		string GetScriptRootDirectory();

		/// <summary>
		/// Sets the project's script root directory to the given path. May require a restart of the IDE, depending on the implementation.
		/// </summary>
		void SetScriptRootDirectory(string newDirectoryPath);

		/// <summary>
		/// Gets the current engine version number from the project's engine files.
		/// </summary>
		Version GetCurrentEngineVersion();

		/// <summary>
		/// Gets the latest engine version number that's available for the project's GameVersion.
		/// </summary>
		Version GetLatestEngineVersion();
	}
}

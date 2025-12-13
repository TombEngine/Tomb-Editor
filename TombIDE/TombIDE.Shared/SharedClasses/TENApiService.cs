using LuaApiBuilder;
using System;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses;

public static class TENApiService
{
	/// <summary>
	/// Injects the TEN API into the specified game project's script root directory.
	/// </summary>
	public static void InjectTENApi(IGameProject project, Version currentEngineVersion)
	{
		if (project.GameVersion != TRVersion.Game.TombEngine)
			return;

		string scriptRootDirectory = project.GetScriptRootDirectory();

		string inputPath = Path.Combine(DefaultPaths.TENApiDirectory, "API.xml");
		string outputPath = Path.Combine(scriptRootDirectory, ".API");

		var converter = new ApiConverter();
		converter.Convert(currentEngineVersion, inputPath, outputPath);

		// Copy .luarc.json file into the script root directory
		string luarcPath = Path.Combine(DefaultPaths.TENApiDirectory, ".luarc.json");
		string luarcTargetPath = Path.Combine(scriptRootDirectory, ".luarc.json");

		if (File.Exists(luarcPath))
			File.Copy(luarcPath, luarcTargetPath, true);
	}
}

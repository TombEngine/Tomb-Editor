using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombIDE.Shared.SharedClasses
{
	public static class LevelHandling
	{
		public static void UpdatePrj2GameSettings(string prj2FilePath, IGameProject destProject, string dataFileName = null)
		{
			Level level = Prj2Loader.LoadFromPrj2(prj2FilePath, null, System.Threading.CancellationToken.None, new Prj2Loader.Settings());

			string exeFilePath = destProject.GetEngineExecutableFilePath();
			string engineDirectory = destProject.GetEngineRootDirectoryPath();

			level.Settings.LevelFilePath = prj2FilePath;

			level.Settings.GameDirectory = level.Settings.MakeRelative(engineDirectory, VariableType.LevelDirectory);
			level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
			level.Settings.ScriptDirectory = level.Settings.MakeRelative(destProject.GetScriptRootDirectory(), VariableType.LevelDirectory);
			level.Settings.GameVersion = destProject.GameVersion is TRVersion.Game.TR1 ? TRVersion.Game.TR1X : destProject.GameVersion; // Map TR1 to TR1X - we never supported vanilla TR1 in TombIDE

			if (string.IsNullOrWhiteSpace(dataFileName))
			{
				string fileName = Path.GetFileName(level.Settings.GameLevelFilePath);

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

		public static List<string> GetValidPrj2FilesFromDirectory(string directoryPath)
		{
			List<string> validPrj2Files = new List<string>();

			foreach (string file in Directory.GetFiles(directoryPath, "*.prj2", SearchOption.AllDirectories))
			{
				if (!Prj2Helper.IsBackupFile(file))
					validPrj2Files.Add(file);
			}

			return validPrj2Files;
		}

		public static List<string> GenerateScriptLines(string levelName, string dataFileName, TRVersion.Game gameVersion, int ambientSoundID, bool horizon = false)
		{
			if (gameVersion == TRVersion.Game.TR2 || gameVersion == TRVersion.Game.TR3)
			{
				return new List<string>
				{
					"\nLEVEL: " + levelName,
					"",
					"	LOAD_PIC: " + "pix\\" + (gameVersion == TRVersion.Game.TR2 ? "mansion.pcx" : "house.bmp"),
					"	TRACK: " + ambientSoundID,
					"	GAME: data\\" + dataFileName.ToLower() + ".tr2",
					"	COMPLETE:",
					"",
					"END:"
				};
			}
			else if (gameVersion == TRVersion.Game.TR4 || gameVersion == TRVersion.Game.TRNG)
			{
				return new List<string>
				{
					"\n[Level]",
					"Name= " + levelName,
					"Level= DATA\\" + dataFileName.ToUpper() + ", " + ambientSoundID,
					"LoadCamera= 0, 0, 0, 0, 0, 0, 0",
					"Horizon= " + (horizon ? "ENABLED" : "DISABLED")
				};
			}
			else if (gameVersion == TRVersion.Game.TombEngine)
			{
				return new List<string>
				{
					$"\n-- {dataFileName} level\n",
					$"{dataFileName} = TEN.Flow.Level()\n",
					$"{dataFileName}.nameKey = \"{dataFileName}\"",
					$"{dataFileName}.scriptFile = \"Scripts\\\\Levels\\\\{dataFileName}.lua\"",
					$"{dataFileName}.ambientTrack = \"{ambientSoundID}\"",
					$"{dataFileName}.horizon1.enabled = " + (horizon ? "true" : "false"),
					$"{dataFileName}.levelFile = \"Data\\\\{dataFileName}.ten\"",
					$"{dataFileName}.loadScreenFile = \"Screens\\\\loading.png\"\n",
					$"TEN.Flow.AddLevel({dataFileName})\n",
					"--------------------------------------------------",
					$"	{dataFileName} = {{ \"{levelName}\" }}"
				};
			}

			return new List<string>();
		}

		public static string RemoveIllegalNameSymbols(string levelName)
		{
			char[] illegalNameChars = { ';', '[', ']', '=', ',', '.', '!' };
			return illegalNameChars.Aggregate(levelName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}

		public static string MakeValidVariableName(string levelName)
		{
			char[] illegalNameChars = { ' ', ';', ':', '(', ')', '[', ']', '{', '}', '<', '>', '=', ',', '.', '!', '-', '+', '*', '?', '/', '\\', '\"', '\'', '&', '%', '#', '@', '|', '^', '`', '~', '$' };
			string result = illegalNameChars.Aggregate(levelName.Trim(), (current, c) => current.Replace(c.ToString(), "_"));

			if (char.IsDigit(result.FirstOrDefault()))
				result = "_" + result;

			// Reduce the amount of '_' chars
			while (result.Contains("__"))
				result = result.Replace("__", "_");

			return result;
		}
	}
}

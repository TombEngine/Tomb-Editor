using System;
using System.IO;
using System.Linq;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses
{
	public static class PathHelper
	{
		public static string RemoveIllegalPathSymbols(string fileName)
			=> Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

		/// <exception cref="FileNotFoundException" />
		public static string GetScriptFilePath(string scriptRootDirectoryPath, TRVersion.Game version)
		{
			string FindFile(string fileName, string searchPattern)
			{
				foreach (string file in Directory.GetFiles(scriptRootDirectoryPath, searchPattern, SearchOption.AllDirectories))
				{
					if (Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
						return file;
				}

				return null;
			}

			string targetFile;
			string targetExtension;

			switch (version)
			{
				case TRVersion.Game.TR1 or TRVersion.Game.TR1X or TRVersion.Game.TR2X:
					targetFile = "gameflow.json5";
					targetExtension = "*.json5";
					break;

				case TRVersion.Game.TombEngine:
					targetFile = "Gameflow.lua";
					targetExtension = "*.lua";
					break;

				default:
					targetFile = "Script.txt";
					targetExtension = "*.txt";
					break;
			}

			string foundFile = FindFile(targetFile, targetExtension);

			if (!string.IsNullOrEmpty(foundFile))
				return foundFile;

			// File not found...

			if (version is TRVersion.Game.TR1 or TRVersion.Game.TR1X)
			{
				// Try fall-back to TR1X_gameflow.json5
				foundFile = FindFile("TR1X_gameflow.json5", targetExtension);

				if (!string.IsNullOrEmpty(foundFile))
					return foundFile;

				// Try fall-back to Tomb1Main_gameflow.json5
				foundFile = FindFile("Tomb1Main_gameflow.json5", targetExtension);

				if (!string.IsNullOrEmpty(foundFile))
					return foundFile;
			}
			else if (version == TRVersion.Game.TR2X)
			{
				// Try fall-back to TR2X_gameflow.json5
				foundFile = FindFile("TR2X_gameflow.json5", targetExtension);

				if (!string.IsNullOrEmpty(foundFile))
					return foundFile;
			}

			throw new FileNotFoundException("Script file is missing.");
		}

		/// <exception cref="FileNotFoundException" />
		public static string GetLanguageFilePath(string scriptRootDirectoryPath, TRVersion.Game version)
		{
			if (version == TRVersion.Game.TombEngine)
			{
				foreach (string file in Directory.GetFiles(scriptRootDirectoryPath, "*.lua", SearchOption.TopDirectoryOnly))
					if (Path.GetFileName(file).Equals("Strings.lua", StringComparison.OrdinalIgnoreCase))
						return file;

				throw new FileNotFoundException("Strings.lua is missing.");
			}
			else
			{
				foreach (string file in Directory.GetFiles(scriptRootDirectoryPath, "*.txt", SearchOption.TopDirectoryOnly))
					if (Path.GetFileName(file).Equals("English.txt", StringComparison.OrdinalIgnoreCase))
						return file;

				throw new FileNotFoundException("English.txt is missing.");
			}
		}
	}
}

using System;
using System.IO;
using System.Linq;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public class LevelProject : ILevelProject
	{
		public string TargetPrj2FileName { get; set; }

		public string Name { get; protected set; } = "UNTITLED LEVEL";
		public string DirectoryPath { get; protected set; } = string.Empty;

		public LevelProject(string name, string directoryPath, string targetPrj2FileName = null)
		{
			Name = name;
			DirectoryPath = directoryPath;
			TargetPrj2FileName = targetPrj2FileName;
		}

		public FileInfo[] GetPrj2Files(bool includeBackups = false)
		{
			FileInfo[] allPrj2Files = new DirectoryInfo(DirectoryPath).GetFiles("*.prj2", SearchOption.TopDirectoryOnly);

			return includeBackups
				? allPrj2Files
				: allPrj2Files.Where(x => !Prj2Helper.IsBackupFile(x.FullName)).ToArray();
		}

		public string GetMostRecentlyModifiedPrj2FileName()
		{
			foreach (FileInfo file in GetPrj2Files().OrderByDescending(file => file.LastWriteTime))
			{
				if (Prj2Helper.IsBackupFile(file.FullName))
					continue;

				return file.Name;
			}

			return null; // This will only happen when the directory is empty or consists of only backup files
		}

		public string GetTrlvlFilePath()
		{
			string[] allTrlvlFiles = Directory.GetFiles(DirectoryPath, "*.trlvl", SearchOption.TopDirectoryOnly);
			return allTrlvlFiles.Length == 0 ? Path.Combine(DirectoryPath, "project.trlvl") : allTrlvlFiles[0];
		}

		public bool IsValid(out string errorMessage)
		{
			if (!Directory.Exists(DirectoryPath))
			{
				errorMessage = "The level project directory does not exist.";
				return false;
			}

			string[] allPrj2Files = Directory.GetFiles(DirectoryPath, "*.prj2", SearchOption.TopDirectoryOnly);

			if (allPrj2Files.Length == 0)
			{
				errorMessage = "The level project directory does not contain any .prj2 files.";
				return false;
			}

			string targetPrj2FilePath = TargetPrj2FileName is not null
				? Path.Combine(DirectoryPath, TargetPrj2FileName)
				: Path.Combine(DirectoryPath, GetMostRecentlyModifiedPrj2FileName());

			if (!File.Exists(targetPrj2FilePath))
			{
				errorMessage = "The target .prj2 file does not exist.";
				return false;
			}

			errorMessage = string.Empty;
			return true;
		}

		public bool IsExternal(string relativeToLevelsDirectoryPath)
			=> !DirectoryPath.StartsWith(relativeToLevelsDirectoryPath, StringComparison.OrdinalIgnoreCase);

		public void Rename(string newName, bool renameDirectory = false, bool renameTrprojFile = false)
		{
			if (renameDirectory)
			{
				string newFolderPath = Path.Combine(Path.GetDirectoryName(DirectoryPath), newName);

				Directory.Move(DirectoryPath, newFolderPath + "_TEMP");
				Directory.Move(newFolderPath + "_TEMP", newFolderPath);

				DirectoryPath = newFolderPath;
			}

			Name = newName;
		}

		public void Save()
		{
			var trlvl = new TrlvlFile
			{
				LevelName = Name,
				StartupFileName = TargetPrj2FileName
			};

			trlvl.WriteToFile(GetTrlvlFilePath());
		}

		public static LevelProject FromTrlvl(string trlvlFilePath)
		{
			TrlvlFile trlvl = XmlUtils.ReadXmlFile<TrlvlFile>(trlvlFilePath);
			var level = new LevelProject(trlvl.LevelName, Path.GetDirectoryName(trlvlFilePath), trlvl.StartupFileName);

			return level;
		}
	}
}

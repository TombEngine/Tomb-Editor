using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	[XmlRoot("GameProject")]
	public class TrprojFile : ITrproj
	{
		[XmlIgnore]
		public string FilePath { get; protected set; }

		[XmlAttribute]
		public string FileFormatVersion { get; set; } = "2.0";

		[XmlAttribute]
		public TRVersion.Game TargetGameVersion { get; set; }

		[XmlElement("Name")]
		public string ProjectName { get; set; }

		[XmlElement("LevelSourcingPath")]
		public string LevelSourcingDirectory { get; set; }

		[XmlElement("ScriptingPath")]
		public string ScriptingDirectory { get; set; }

		[XmlElement("PluginSourcingPath")]
		public string PluginSourcingDirectory { get; set; }

		[XmlIgnore]
		public string DefaultLanguage { get; set; } = "English";

		[XmlIgnore, XmlArrayItem(typeof(string), ElementName = "Language")]
		public List<string> SupportedLanguages { get; set; } = new() { "English" };

		[XmlArrayItem(typeof(string), ElementName = "ProjectFile")]
		public List<string> KnownLevels { get; set; } = new();

		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelSourcingDirectory = Path.GetRelativePath(projectPath, LevelSourcingDirectory);

			if (ScriptingDirectory is not null)
				ScriptingDirectory = Path.GetRelativePath(projectPath, ScriptingDirectory);

			if (PluginSourcingDirectory is not null)
				PluginSourcingDirectory = Path.GetRelativePath(projectPath, PluginSourcingDirectory);

			KnownLevels = KnownLevels.Select(path => Path.GetRelativePath(projectPath, path)).ToList();
		}

		public void DecodeProjectPaths(string trprojFilePath)
		{
			FilePath = trprojFilePath;

			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelSourcingDirectory = Path.GetFullPath(LevelSourcingDirectory, projectPath);

			if (ScriptingDirectory is not null)
				ScriptingDirectory = Path.GetFullPath(ScriptingDirectory, projectPath);

			if (PluginSourcingDirectory is not null)
				PluginSourcingDirectory = Path.GetFullPath(PluginSourcingDirectory, projectPath);

			KnownLevels = KnownLevels.Select(path => Path.GetFullPath(path, projectPath)).ToList();
		}

		public void WriteToFile(string filePath)
		{
			try
			{
				EncodeProjectPaths(filePath);
				XmlUtils.WriteXmlFile(filePath, this);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to save the project file.", ex);
			}
			finally
			{
				DecodeProjectPaths(filePath);
			}
		}

		public static TrprojFile FromFile(string filePath, out Version version)
		{
			try
			{
				TrprojFile trprojFile = XmlUtils.ReadXmlFile<TrprojFile>(filePath);
				trprojFile.DecodeProjectPaths(filePath);

				version = new Version(trprojFile.FileFormatVersion);

				if (version != new Version(2, 0))
					throw new Exception("Failed to load the project file. Unsupported version.");

				return trprojFile;
			}
			catch
			{
				try
				{
					LegacyTrprojFile legacyTrproj = XmlUtils.ReadXmlFile<LegacyTrprojFile>(filePath);
					legacyTrproj.DecodeProjectPaths(filePath);

					version = new Version(legacyTrproj.FileFormatVersion);

					if (version != new Version(1, 0))
						throw new Exception("Failed to load the project file. Unsupported version.");

					return FromLegacy(legacyTrproj);
				}
				catch
				{
					version = null;
					return null;
				}
			}
		}

		public static TrprojFile FromLegacy(LegacyTrprojFile legacyTrproj)
		{
			string trprojDirectory = Path.GetDirectoryName(legacyTrproj.FilePath);

			var trproj = new TrprojFile
			{
				FilePath = legacyTrproj.FilePath,

				ProjectName = legacyTrproj.Name,
				TargetGameVersion = legacyTrproj.GameVersion,

				LevelSourcingDirectory = legacyTrproj.LevelsPath,
				ScriptingDirectory = legacyTrproj.ScriptPath,

				SupportedLanguages = new List<string> { "English" },
				DefaultLanguage = "English"
			};

			if (trproj.TargetGameVersion is TRVersion.Game.TRNG or TRVersion.Game.TRNGCE)
				trproj.PluginSourcingDirectory = Path.Combine(trprojDirectory, "Plugins");

			foreach (LegacyProjectLevel level in legacyTrproj.Levels)
			{
				try
				{
					var trlvl = new TrlvlFile
					{
						LevelName = level.Name,
						StartupFileName = level.SpecificFile
					};

					if (trlvl.StartupFileName == "$(LatestFile)")
						trlvl.StartupFileName = null; // New method of specifying the latest file

					string trlvlFilePath = Path.Combine(level.FolderPath, "project.trlvl");
					trlvl.WriteToFile(trlvlFilePath);

					trproj.KnownLevels.Add(trlvlFilePath);
				}
				catch { } // Skip
			}

			return trproj;
		}
	}
}

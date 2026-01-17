using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR2XGameProject : GameProjectBase
	{
		public const string MainScriptFileNameFilter = "*gameflow.json5";
		public const string LanguageFileNameFilter = MainScriptFileNameFilter; // Same file as main script file

		public override TRVersion.Game GameVersion => TRVersion.Game.TR2X;

		public override string DataFileExtension => ".tr2";
		public override string EngineExecutableFileName
		{
			get
			{
				var rootDir = GetEngineRootDirectoryPath();
				foreach (var exe in new[] { "TRX.exe", "TR2X.exe" })
				{
					var path = Path.Combine(rootDir, exe);
					if (File.Exists(path))
						return exe;
				}

				return "TRX.exe"; // Default
			}
		}

		public override string MainScriptFilePath => Directory
			.GetFiles(GetScriptRootDirectory(), MainScriptFileNameFilter, SearchOption.AllDirectories)
			.FirstOrDefault();

		public override bool SupportsCustomScriptPaths => false;
		public override bool SupportsPlugins => false;

		public TR2XGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TR2XGameProject(string name, string directoryPath, string levelsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath)
		{ }

		public override string GetDefaultGameLanguageFilePath()
		{
			string defaultLanguageFilePath = MainScriptFilePath;

			return File.Exists(defaultLanguageFilePath)
				? defaultLanguageFilePath
				: throw new FileNotFoundException("The default game language file could not be found.\n" +
					"Required file not found: gameflow.json5");
		}

		public override bool IsValid(out string errorMessage)
		{
			if (!base.IsValid(out errorMessage))
				return false;

			if (!File.Exists(MainScriptFilePath))
			{
				errorMessage = "The project does not have a valid gameflow file.";
				return false;
			}

			return true;
		}

		public override string GetScriptRootDirectory()
			=> Path.Combine(GetEngineRootDirectoryPath(), "cfg"); // Hardcoded path

		public override void SetScriptRootDirectory(string newDirectoryPath)
			=> throw new NotSupportedException("Current project type does not allow changing Script directories.");

		public override Version GetCurrentEngineVersion()
		{
			try
			{
				string engineExecutablePath = GetEngineExecutableFilePath();
				string versionInfo = FileVersionInfo.GetVersionInfo(engineExecutablePath).ProductVersion;
				if (!versionInfo.StartsWith("TR", StringComparison.InvariantCultureIgnoreCase))
				{
					// Legacy TR2X builds did not have similar version strings to TR1X.
					// This ensures such exes are marked as being outdated.
					versionInfo = $"TR2X {versionInfo}";
				}
				versionInfo = versionInfo.Replace("TRX ", string.Empty);

				return new Version(versionInfo);
			}
			catch
			{
				return new Version(0, 0);
			}
		}

		public override Version GetLatestEngineVersion()
		{
			string tempFileName = Path.GetTempFileName();

			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR2X.zip");

				using ZipArchive archive = ZipFile.OpenRead(enginePresetPath);
				ZipArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Name == "TRX.exe");

				if (entry == null)
					return null;

				entry.ExtractToFile(tempFileName, true);
				string productVersion = FileVersionInfo.GetVersionInfo(tempFileName).ProductVersion;
				productVersion = productVersion.Replace("TRX ", string.Empty);

				return new Version(productVersion);
			}
			catch
			{
				return null;
			}
			finally
			{
				if (File.Exists(tempFileName))
					File.Delete(tempFileName);
			}
		}
	}
}

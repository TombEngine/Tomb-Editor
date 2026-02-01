using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR1XGameProject : GameProjectBase
	{
		public const string MainScriptFileNameFilter = "*gameflow.json5";
		public const string LanguageFileNameFilter = MainScriptFileNameFilter; // Same file as main script file

		/// <summary>
		/// Version prefix used in TRX engine executables.
		/// </summary>
		private const string VersionPrefix = "TRX ";

		/// <summary>
		/// Legacy version prefix used in older TR1X engine versions (before unification with TR2X).
		/// </summary>
		private const string LegacyVersionPrefix = "TR1X ";

		/// <summary>
		/// Valid executable names for TR1X projects, ordered by priority.
		/// </summary>
		private readonly string[] ValidOrderedExecutableNames = new[]
		{
			"TRX.exe",
			"TR1X.exe", // Legacy name (before TR1X and TR2X were unified into TRX)
			"Tomb1Main.exe" // Very legacy name (before TR1X rebranding)
		};

		public override TRVersion.Game GameVersion => TRVersion.Game.TR1;

		public override string DataFileExtension => ".phd";

		public override string EngineExecutableFileName
		{
			get
			{
				string rootDirectory = GetEngineRootDirectoryPath();

				foreach (string exeName in ValidOrderedExecutableNames)
				{
					string exePath = Path.Combine(rootDirectory, exeName);

					if (File.Exists(exePath))
						return exeName;
				}

				return "TRX.exe"; // Default
			}
		}

		public override string MainScriptFilePath => Directory
			.GetFiles(GetScriptRootDirectory(), MainScriptFileNameFilter, SearchOption.AllDirectories)
			.FirstOrDefault();

		public override bool SupportsCustomScriptPaths => false;
		public override bool SupportsPlugins => false;

		public TR1XGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TR1XGameProject(string name, string directoryPath, string levelsDirectoryPath)
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

				return GetActualVersion(versionInfo);
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
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR1.zip");

				using ZipArchive archive = ZipFile.OpenRead(enginePresetPath);
				ZipArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Name == "TRX.exe");

				if (entry is null)
					return null;

				entry.ExtractToFile(tempFileName, true);
				string productVersion = FileVersionInfo.GetVersionInfo(tempFileName).ProductVersion;
				productVersion = productVersion.Replace(VersionPrefix, string.Empty);

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

		/// <summary>
		/// Parses the actual version from a version string, handling legacy version formats.
		/// </summary>
		/// <param name="versionString">The version string to parse.</param>
		private static Version GetActualVersion(string versionString)
		{
			bool isLegacyVersion = versionString.StartsWith(LegacyVersionPrefix);
			string versionPrefix = isLegacyVersion ? LegacyVersionPrefix : VersionPrefix;

			versionString = versionString.Replace(versionPrefix, string.Empty).Trim();

			if (string.IsNullOrEmpty(versionString))
				throw new FormatException("Version string is empty after removing prefix.");

			// For legacy versions, prepend "0." only if the version doesn't already have too many components
			if (isLegacyVersion)
			{
				// Count version components (dots + 1)
				int componentCount = versionString.Count(c => c == '.') + 1;
				
				// Version class supports up to 4 components, so if we already have 4, don't prepend
				if (componentCount >= 4)
					return new Version(versionString);
				
				return new Version("0." + versionString); // Legacy versions get a 0.x major version
			}
			
			return new Version(versionString);
		}
	}
}

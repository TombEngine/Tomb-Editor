using System;
using System.Diagnostics;
using System.IO;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public sealed class TRNGCEGameProject : TR4GameProject
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TRNGCE;

		public override bool SupportsPlugins => true;

		public TRNGCEGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TRNGCEGameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath, string pluginsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath, pluginsDirectoryPath)
		{ }

		public override Version GetCurrentEngineVersion()
		{
			try
			{
				string trngceDllFilePath = Path.Combine(GetEngineRootDirectoryPath(), "TRNGCE.dll");
				string versionInfo = FileVersionInfo.GetVersionInfo(trngceDllFilePath).FileVersion;
				return new Version(versionInfo.Replace(" ", string.Empty).Replace(',', '.'));
			}
			catch
			{
				return new Version(0, 0);
			}
		}
	}
}

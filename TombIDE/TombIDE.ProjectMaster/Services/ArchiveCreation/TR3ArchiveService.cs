using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TR3ArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion == TRVersion.Game.TR3;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => [
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "pix")
	];

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory)
	{
		var files = new List<string>(GetCommonFiles(engineDirectory))
		{
			Path.Combine(engineDirectory, "audio", "cdaudio.wad"),
			Path.Combine(engineDirectory, "data.bin"),
			Path.Combine(engineDirectory, "tomb3.exe"),
			Path.Combine(engineDirectory, "tomb3_ConfigTool.exe")
		};

		// Add all DLL files
		files.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

		return files;
	}

	protected override bool CreateSavesFolder() => true;
}

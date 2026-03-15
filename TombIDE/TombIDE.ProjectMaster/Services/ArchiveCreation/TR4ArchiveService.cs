using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TR4ArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion is TRVersion.Game.TR4 or TRVersion.Game.TRNG;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => [
		Path.Combine(engineDirectory, "audio"),
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "pix"),
		Path.Combine(engineDirectory, "patches")
	];

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory)
	{
		var files = new List<string>(GetCommonFiles(engineDirectory))
		{
			Path.Combine(engineDirectory, "load.bmp"),
			Path.Combine(engineDirectory, "patches.bin"),
			Path.Combine(engineDirectory, "patches.fpd"),
			Path.Combine(engineDirectory, "tomb4.exe")
		};

		// Add all DLL and DAT files
		files.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));
		files.AddRange(Directory.GetFiles(engineDirectory, "*.dat", SearchOption.TopDirectoryOnly));

		return files;
	}
}

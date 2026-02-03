using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TR2ArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion == TRVersion.Game.TR2;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => [
		Path.Combine(engineDirectory, "audio"),
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "ExtraOptions"),
		Path.Combine(engineDirectory, "music"),
		Path.Combine(engineDirectory, "pix")
	];

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory)
	{
		var files = new List<string>(GetCommonFiles(engineDirectory))
		{
			Path.Combine(engineDirectory, "COPYING.txt"),
			Path.Combine(engineDirectory, "Tomb2.exe"),
			Path.Combine(engineDirectory, "TR2Main.json")
		};

		// Add all DLL files
		files.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

		return files;
	}
}

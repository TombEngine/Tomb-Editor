using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TombEngineArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion == TRVersion.Game.TombEngine;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => [
		Path.Combine(engineDirectory, "Audio"),
		Path.Combine(engineDirectory, "Bin"),
		Path.Combine(engineDirectory, "Data"),
		Path.Combine(engineDirectory, "FMV"),
		Path.Combine(engineDirectory, "Screens"),
		Path.Combine(engineDirectory, "Scripts"),
		Path.Combine(engineDirectory, "Shaders"),
		Path.Combine(engineDirectory, "Textures")
	];

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory)
	{
		var files = new List<string>(GetCommonFiles(engineDirectory))
		{
			Path.Combine(engineDirectory, "TombEngine.exe")
		};

		// Add all DLL files
		files.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

		return files;
	}
}

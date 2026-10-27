using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

/// <summary>
/// Unified archive service for TRX-based engines (TR1X, TR2X and TR3X).
/// </summary>
public sealed class TRXArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR1X or TRVersion.Game.TR2X or TRVersion.Game.TR3X;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => [
		Path.Combine(engineDirectory, "audio"),
		Path.Combine(engineDirectory, "music"),
		Path.Combine(engineDirectory, "cfg"),
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "shaders"),
		Path.Combine(engineDirectory, "game"),
		Path.Combine(engineDirectory, "modules"),
		Path.Combine(engineDirectory, "scripts"),
	];

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory) => [
		.. GetCommonFiles(engineDirectory),

		// TRX unified executable (current)
		Path.Combine(engineDirectory, "TRX.exe"),

		// TR1X legacy executables
		Path.Combine(engineDirectory, "TR1X.exe"),
		Path.Combine(engineDirectory, "Tomb1Main.exe"),
		Path.Combine(engineDirectory, "TR1X_ConfigTool.exe"),
		Path.Combine(engineDirectory, "Tomb1Main_ConfigTool.exe"),

		// TR2X legacy executables
		Path.Combine(engineDirectory, "TR2X.exe"),
		Path.Combine(engineDirectory, "TR2X_ConfigTool.exe")
	];
}

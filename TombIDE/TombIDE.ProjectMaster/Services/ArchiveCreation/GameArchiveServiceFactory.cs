using System;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

/// <summary>
/// Factory for creating game archive services based on the project's game version.
/// </summary>
public interface IGameArchiveServiceFactory
{
	/// <summary>
	/// Gets the appropriate archive service for the given project.
	/// </summary>
	/// <param name="gameVersion">The game version.</param>
	/// <returns>An archive service instance for the project's game version.</returns>
	/// <exception cref="NotSupportedException">Thrown when the game version is not supported.</exception>
	IGameArchiveService GetArchiveService(TRVersion.Game gameVersion);
}

public sealed class GameArchiveServiceFactory : IGameArchiveServiceFactory
{
	public IGameArchiveService GetArchiveService(TRVersion.Game gameVersion) => gameVersion switch
	{
		TRVersion.Game.TR1 or TRVersion.Game.TR1X or TRVersion.Game.TR2X => new TRXArchiveService(),
		TRVersion.Game.TR2 => new TR2ArchiveService(),
		TRVersion.Game.TR3 => new TR3ArchiveService(),
		TRVersion.Game.TR4 or TRVersion.Game.TRNG => new TR4ArchiveService(),
		TRVersion.Game.TombEngine => new TombEngineArchiveService(),
		_ => throw new NotSupportedException($"Archive creation is not supported for {gameVersion}.")
	};
}

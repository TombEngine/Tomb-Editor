using TombIDE.ProjectMaster.Services.FileExtraction;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

/// <summary>
/// Factory for creating the appropriate engine update service based on the game version.
/// </summary>
public interface IEngineUpdateServiceFactory
{
	/// <summary>
	/// Gets the appropriate engine update service for the specified game version.
	/// </summary>
	/// <param name="gameVersion">The game version.</param>
	/// <returns>An <see cref="IEngineUpdateService"/> if auto-update is supported for this game version; otherwise, <see langword="null"/>.</returns>
	IEngineUpdateService? GetUpdateService(TRVersion.Game gameVersion);
}

public sealed class EngineUpdateServiceFactory : IEngineUpdateServiceFactory
{
	private readonly IFileExtractionService _fileExtractionService;

	public EngineUpdateServiceFactory(IFileExtractionService fileExtractionService)
		=> _fileExtractionService = fileExtractionService;

	public IEngineUpdateService? GetUpdateService(TRVersion.Game gameVersion) => gameVersion switch
	{
		TRVersion.Game.TombEngine => new TombEngineUpdateService(_fileExtractionService),
		TRVersion.Game.TR1 or
		TRVersion.Game.TR1X or
		TRVersion.Game.TR2X => new TRXUpdateService(_fileExtractionService, gameVersion),
		_ => null
	};
}

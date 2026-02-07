using TombIDE.ProjectMaster.Services.EngineUpdate;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineVersion;

public sealed class EngineVersionService : IEngineVersionService
{
	private readonly IEngineUpdateServiceFactory _updateServiceFactory;

	public EngineVersionService(IEngineUpdateServiceFactory updateServiceFactory)
		=> _updateServiceFactory = updateServiceFactory;

	public EngineVersionInfo GetVersionInfo(IGameProject project)
	{
		var currentVersion = project.GetCurrentEngineVersion();
		var latestVersion = project.GetLatestEngineVersion();

		// Check if auto-update is supported
		var updateService = _updateServiceFactory.GetUpdateService(project.GameVersion);

		bool supportsAutoUpdate = false;
		string? autoUpdateBlockReason = null;

		if (updateService is not null && currentVersion is not null)
		{
			supportsAutoUpdate = updateService.CanAutoUpdate(currentVersion, out autoUpdateBlockReason);
		}

		return new EngineVersionInfo
		{
			CurrentVersion = currentVersion,
			LatestVersion = latestVersion,
			SupportsAutoUpdate = supportsAutoUpdate,
			AutoUpdateBlockReason = autoUpdateBlockReason
		};
	}
}

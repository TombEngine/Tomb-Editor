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
		var info = new EngineVersionInfo
		{
			CurrentVersion = project.GetCurrentEngineVersion(),
			LatestVersion = project.GetLatestEngineVersion()
		};

		// Check if auto-update is supported
		var updateService = _updateServiceFactory.GetUpdateService(project.GameVersion);

		if (updateService is not null && info.CurrentVersion is not null)
		{
			info.SupportsAutoUpdate = updateService.CanAutoUpdate(info.CurrentVersion, out string? blockReason);
			info.AutoUpdateBlockReason = blockReason;
		}
		else
		{
			info.SupportsAutoUpdate = false;
		}

		return info;
	}
}

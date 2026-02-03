using System;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineVersion;

/// <summary>
/// Represents version information for an engine.
/// </summary>
public class EngineVersionInfo
{
	public Version? CurrentVersion { get; set; }
	public Version? LatestVersion { get; set; }

	public bool IsOutdated => CurrentVersion is not null && LatestVersion is not null && CurrentVersion < LatestVersion;
	public bool IsLatest => CurrentVersion is not null && LatestVersion is not null && CurrentVersion >= LatestVersion;

	public bool SupportsAutoUpdate { get; set; }
	public string? AutoUpdateBlockReason { get; set; }
}

/// <summary>
/// Provides functionality for checking and managing engine versions.
/// </summary>
public interface IEngineVersionService
{
	/// <summary>
	/// Gets comprehensive version information for the specified project.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns>An <see cref="EngineVersionInfo"/> containing current and latest version information.</returns>
	EngineVersionInfo GetVersionInfo(IGameProject project);
}

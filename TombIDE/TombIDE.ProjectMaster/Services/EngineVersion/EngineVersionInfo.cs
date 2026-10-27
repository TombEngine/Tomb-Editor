using System;

namespace TombIDE.ProjectMaster.Services.EngineVersion;

/// <summary>
/// Represents version information for an engine, including current and latest versions,
/// update status, and auto-update availability.
/// </summary>
public sealed record EngineVersionInfo
{
	/// <summary>
	/// Gets the currently installed version of the engine.
	/// </summary>
	/// <value>
	/// The current <see cref="Version"/>, or <see langword="null"/> if the version cannot be determined.
	/// </value>
	public Version? CurrentVersion { get; init; }

	/// <summary>
	/// Gets the latest available version of the engine.
	/// </summary>
	/// <value>
	/// The latest <see cref="Version"/>, or <see langword="null"/> if the version cannot be determined.
	/// </value>
	public Version? LatestVersion { get; init; }

	/// <summary>
	/// Gets a value indicating whether the current engine version is outdated.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if both <see cref="CurrentVersion"/> and <see cref="LatestVersion"/> are available
	/// and the current version is lower than the latest version; otherwise, <see langword="false"/>.
	/// </value>
	public bool IsOutdated => CurrentVersion is not null && LatestVersion is not null && CurrentVersion < LatestVersion;

	/// <summary>
	/// Gets a value indicating whether the current engine version is up-to-date.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if both <see cref="CurrentVersion"/> and <see cref="LatestVersion"/> are available
	/// and the current version is equal to or greater than the latest version; otherwise, <see langword="false"/>.
	/// </value>
	public bool IsLatest => CurrentVersion is not null && LatestVersion is not null && CurrentVersion >= LatestVersion;

	/// <summary>
	/// Gets a value indicating whether the engine supports automatic updates.
	/// </summary>
	/// <value>
	/// <see langword="true"/> if the engine can be automatically updated; otherwise, <see langword="false"/>.
	/// </value>
	public bool SupportsAutoUpdate { get; init; }

	/// <summary>
	/// Gets the reason why auto-update is blocked, if applicable.
	/// </summary>
	/// <value>
	/// A message explaining why auto-update is not available, or <see langword="null"/> if auto-update is supported.
	/// </value>
	public string? AutoUpdateBlockReason { get; init; }
}

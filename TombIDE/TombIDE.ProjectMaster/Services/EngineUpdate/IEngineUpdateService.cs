using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

/// <summary>
/// Represents a service that can update a specific game engine version.
/// </summary>
public interface IEngineUpdateService
{
	/// <summary>
	/// Checks if the current engine version can be auto-updated.
	/// </summary>
	/// <param name="currentVersion">The current engine version installed in the project.</param>
	/// <param name="blockReason">When the method returns <see langword="false"/>, contains the reason why auto-update is blocked; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if auto-update is supported for this version; otherwise, <see langword="false"/>.</returns>
	bool CanAutoUpdate(Version currentVersion, [NotNullWhen(false)] out string? blockReason);

	/// <summary>
	/// Performs the engine update operation.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="currentVersion">The current engine version.</param>
	/// <param name="latestVersion">The latest available engine version.</param>
	/// <param name="owner">The owner window for dialogs.</param>
	/// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
	bool UpdateEngine(IGameProject project, Version currentVersion, Version latestVersion, IWin32Window owner);
}

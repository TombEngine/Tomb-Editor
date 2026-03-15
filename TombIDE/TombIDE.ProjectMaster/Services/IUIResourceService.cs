using System.Drawing;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services;

/// <summary>
/// Provides game-specific UI resources for different Tomb Raider versions.
/// </summary>
public interface IUIResourceService
{
	/// <summary>
	/// Gets the level panel icon for the specified game version.
	/// </summary>
	/// <param name="gameVersion">The game version.</param>
	/// <returns>The icon for the game version.</returns>
	Image? GetLevelPanelIcon(TRVersion.Game gameVersion);
}

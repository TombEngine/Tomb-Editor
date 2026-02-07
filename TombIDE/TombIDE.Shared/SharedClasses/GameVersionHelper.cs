using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Provides helper methods for querying game-version-specific capabilities and defaults.
/// </summary>
public static class GameVersionHelper
{
	/// <summary>
	/// Gets the default ambient sound ID for the given project's game version.
	/// </summary>
	/// <param name="project">The target game project.</param>
	/// <returns>The default ambient sound ID for the project's game version, or 0 if the game version is unrecognized.</returns>
	public static int GetDefaultAmbientSoundId(IGameProject project) => project.GameVersion switch
	{
		TRVersion.Game.TR2 => 33,
		TRVersion.Game.TR3 => 28,

		TRVersion.Game.TR4
		or TRVersion.Game.TRNG
		or TRVersion.Game.TombEngine => 110,

		_ => 0
	};

	/// <summary>
	/// Determines whether automatic script generation is supported for the target project.
	/// </summary>
	/// <param name="project">The target game project.</param>
	/// <returns><see langword="true"/> if script generation is supported; otherwise, <see langword="false"/>.</returns>
	public static bool IsScriptGenerationSupported(IGameProject project)
		=> project.GameVersion is not TRVersion.Game.TR1
			and not TRVersion.Game.TR1X
			and not TRVersion.Game.TR2X;

	/// <summary>
	/// Determines whether the horizon setting is available for the target project.
	/// </summary>
	/// <param name="project">The target game project.</param>
	/// <returns><see langword="true"/> if the horizon setting is available; otherwise, <see langword="false"/>.</returns>
	public static bool IsHorizonSettingAvailable(IGameProject project)
		=> project.GameVersion is TRVersion.Game.TR4 or TRVersion.Game.TRNG or TRVersion.Game.TombEngine;
}

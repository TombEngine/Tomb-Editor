#nullable enable

using System.Collections.Generic;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Generates game-version-specific script lines for registering levels in game script files.
/// </summary>
public static class ScriptLineGenerator
{
	/// <summary>
	/// Generates the script or gameflow lines needed to register a level in a game's script system.
	/// </summary>
	/// <param name="levelName">The display name of the level.</param>
	/// <param name="dataFileName">The data file name (without extension).</param>
	/// <param name="gameVersion">The target game version.</param>
	/// <param name="ambientSoundId">The ambient sound or track ID.</param>
	/// <param name="horizon">Whether to enable the horizon effect (TR4/TRNG/TombEngine only).</param>
	/// <returns>A list of script lines ready to be inserted into the game's script file, or an empty list for unsupported versions.</returns>
	public static IReadOnlyList<string> GenerateScriptLines(string levelName, string dataFileName, TRVersion.Game gameVersion, int ambientSoundId, bool horizon = false)
	{
		string? script = gameVersion switch
		{
			TRVersion.Game.TR2 => GenerateTR2Script(levelName, dataFileName, ambientSoundId),
			TRVersion.Game.TR3 => GenerateTR3Script(levelName, dataFileName, ambientSoundId),
			TRVersion.Game.TR4 or TRVersion.Game.TRNG => GenerateTR4Script(levelName, dataFileName, ambientSoundId, horizon),
			TRVersion.Game.TombEngine => GenerateTombEngineScript(levelName, dataFileName, ambientSoundId, horizon),
			_ => null
		};

		return script is not null ? script.Split('\n') : [];
	}

	private static string GenerateTR2Script(string levelName, string dataFileName, int ambientSoundId)
	{
		return $$"""

			LEVEL: {{levelName}}

				LOAD_PIC: pix\mansion.pcx
				TRACK: {{ambientSoundId}}
				GAME: data\{{dataFileName.ToLower()}}.tr2
				COMPLETE:

			END:
			""";
	}

	private static string GenerateTR3Script(string levelName, string dataFileName, int ambientSoundId)
	{
		return $$"""

			LEVEL: {{levelName}}

				LOAD_PIC: pix\house.bmp
				TRACK: {{ambientSoundId}}
				GAME: data\{{dataFileName.ToLower()}}.tr2
				COMPLETE:

			END:
			""";
	}

	private static string GenerateTR4Script(string levelName, string dataFileName, int ambientSoundId, bool horizon)
	{
		string horizonValue = horizon ? "ENABLED" : "DISABLED";

		return $$"""

			[Level]
			Name= {{levelName}}
			Level= DATA\{{dataFileName.ToUpper()}}, {{ambientSoundId}}
			LoadCamera= 0, 0, 0, 0, 0, 0, 0
			Horizon= {{horizonValue}}
			""";
	}

	private static string GenerateTombEngineScript(string levelName, string dataFileName, int ambientSoundId, bool horizon)
	{
		string horizonValue = horizon ? "true" : "false";

		return $$"""

			-- {{dataFileName}} level

			{{dataFileName}} = TEN.Flow.Level()

			{{dataFileName}}.nameKey = "{{dataFileName}}"
			{{dataFileName}}.scriptFile = "Scripts\\Levels\\{{dataFileName}}.lua"
			{{dataFileName}}.ambientTrack = "{{ambientSoundId}}"
			{{dataFileName}}.horizon1.enabled = {{horizonValue}}
			{{dataFileName}}.levelFile = "Data\\{{dataFileName}}.ten"
			{{dataFileName}}.loadScreenFile = "Screens\\loading.png"

			TEN.Flow.AddLevel({{dataFileName}})

			--------------------------------------------------
				{{dataFileName}} = { "{{levelName}}" }
			""";
	}
}

#nullable enable

using System.IO;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Generates game-version-specific scripts for registering levels in game script files.
/// </summary>
public static class ScriptGenerator
{
	/// <summary>
	/// Generates the scripts needed to register a level in a game's script system.
	/// </summary>
	/// <param name="levelName">The display name of the level.</param>
	/// <param name="dataFileName">The data file name (without extension).</param>
	/// <param name="gameVersion">The target game version.</param>
	/// <param name="ambientSoundId">The ambient sound or track ID.</param>
	/// <param name="horizon">Whether to enable the horizon effect (TR4/TRNG/TombEngine only).</param>
	/// <returns>
	/// A <see cref="ScriptGenerationResult"/> containing the generated scripts and any additional files to create,
	/// or <see langword="null"/> if the game version is unsupported.
	/// </returns>
	public static ScriptGenerationResult? GenerateScripts(string levelName, string dataFileName, TRVersion.Game gameVersion, int ambientSoundId, bool horizon = false)
	{
		return gameVersion switch
		{
			// TODO: Implement script insertion for TRX
			// TRVersion.Game.TR1 or TRVersion.Game.TR1X => GenerateTRXScript(levelName, dataFileName, ambientSoundId, "phd"),
			// TRVersion.Game.TR2X => GenerateTRXScript(levelName, dataFileName, ambientSoundId, "tr2"),

			TRVersion.Game.TR2 => new ScriptGenerationResult(dataFileName)
			{
				GameFlowScript = GenerateTR2Script(levelName, dataFileName, ambientSoundId)
			},

			TRVersion.Game.TR3 => new ScriptGenerationResult(dataFileName)
			{
				GameFlowScript = GenerateTR3Script(levelName, dataFileName, ambientSoundId)
			},

			TRVersion.Game.TR4 or TRVersion.Game.TRNG => new ScriptGenerationResult(dataFileName)
			{
				GameFlowScript = GenerateTR4Script(levelName, dataFileName, ambientSoundId, horizon)
			},

			TRVersion.Game.TombEngine => GenerateTombEngineScript(levelName, dataFileName, ambientSoundId, horizon),
			_ => null
		};
	}

	private static ScriptGenerationResult GenerateTRXScript(string levelName, string dataFileName, int ambientSoundId, string fileExtension)
	{
		string gameFlowScript = $$"""
		        // {{levelName}}
		        {
		            "path": "data/{{dataFileName.ToLowerInvariant()}}.{{fileExtension}}",
		            "music_track": {{ambientSoundId}},
		            "sequence": [
		                {"type": "loading_screen", "path": "data/images/loading.webp", "fade_in_time": 1.0, "fade_out_time": 1.0},
		                {"type": "loop_game"},
		                {"type": "play_music", "music_track": 37},
		                {"type": "level_stats"},
		                {"type": "level_complete"},
		            ],
		        },
		""";

		string languageScript = $$"""
		        {
		            "title": "{{levelName}}",
		        },
		""";

		return new ScriptGenerationResult(dataFileName)
		{
			GameFlowScript = gameFlowScript,
			LanguageScript = languageScript
		};
	}

	private static string GenerateTR2Script(string levelName, string dataFileName, int ambientSoundId)
	{
		return $$"""
			LEVEL: {{levelName}}

				LOAD_PIC: pix\mansion.pcx
				TRACK: {{ambientSoundId}}
				GAME: data\{{dataFileName.ToLowerInvariant()}}.tr2
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
				GAME: data\{{dataFileName.ToLowerInvariant()}}.tr2
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
			Level= DATA\{{dataFileName.ToUpperInvariant()}}, {{ambientSoundId}}
			LoadCamera= 0, 0, 0, 0, 0, 0, 0
			Horizon= {{horizonValue}}
			""";
	}

	private static ScriptGenerationResult GenerateTombEngineScript(string levelName, string dataFileName, int ambientSoundId, bool horizon)
	{
		string horizonValue = horizon ? "true" : "false";

		string gameFlowScript = $$"""
			-- {{dataFileName}} level

			{{dataFileName}} = TEN.Flow.Level()

			{{dataFileName}}.nameKey = "{{dataFileName}}"
			{{dataFileName}}.scriptFile = "Scripts\\Levels\\{{dataFileName}}.lua"
			{{dataFileName}}.ambientTrack = "{{ambientSoundId}}"
			{{dataFileName}}.horizon1.enabled = {{horizonValue}}
			{{dataFileName}}.levelFile = "Data\\{{dataFileName}}.ten"
			{{dataFileName}}.loadScreenFile = "Screens\\loading.png"

			TEN.Flow.AddLevel({{dataFileName}})
			""";

		string languageScript = $$"""
				{{dataFileName}} = { "{{levelName}}" }
			""";

		string levelScript = $$"""
			-- FILE: Levels\{{dataFileName}}.lua

			LevelFuncs.OnLoad = function() end
			LevelFuncs.OnSave = function() end
			LevelFuncs.OnStart = function() end
			LevelFuncs.OnLoop = function() end
			LevelFuncs.OnEnd = function() end
			LevelFuncs.OnUseItem = function() end
			LevelFuncs.OnFreeze = function() end

			""";

		return new ScriptGenerationResult(dataFileName)
		{
			GameFlowScript = gameFlowScript,
			LanguageScript = languageScript,
			FilesToCreate =
			[
				new GeneratedScriptFile(
					RelativePath: Path.Combine("Levels", dataFileName + ".lua"),
					Content: levelScript)
			]
		};
	}
}

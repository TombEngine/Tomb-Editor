using System;

namespace TombLib.Scripting.Tomb1Main.Resources;

public static class Keywords
{
	public static readonly string[] Values = new string[]
	{
		"true",
		"false",
		"null"
	};

	public static readonly RemovedKeyword[] RemovedConstants = new RemovedKeyword[]
	{
		new("cutscene", new(4, 8), "Instead, the cutscenes need to be placed in the top-level \"cutscenes\" array."),
		new("exit_to_cine", new(4, 8)),
		new("exit_to_level", new(4, 8), "Instead, use \"level_complete\". No parameter needed."),
		new("start_game", new(4, 8)),
		new("draw_distance_fade", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
		new("draw_distance_max", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
	};

	public static readonly RemovedKeyword[] RemovedProperties = new RemovedKeyword[]
	{
		new("file", new(4, 8), "Instead, use the \"path\" property for every level."),
		new("fmv_path", new(4, 8), "Instead, use the \"fmv_id\" property."),
		new("level_id", new(4, 8)),
		new("music", new(4, 8), "Instead, use the \"music_track\" property for every level."),
		new("picture_path", new(4, 8), "Instead, use the \"background_path\" property for \"total_stats\", or the \"path\" property for everything else."),
	};
}

public record struct RemovedKeyword(string Keyword, Version RemovedVersion, string Message = "");

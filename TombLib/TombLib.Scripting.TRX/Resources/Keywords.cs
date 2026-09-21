using System;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Defines the TRX keywords used for highlighting, completion and error detection.
/// All collections are owned read-only storage shared process-wide; consumers cannot mutate them.
/// </summary>
public static class Keywords
{
	/// <summary>
	/// Gets the primitive JSON keyword values.
	/// </summary>
	public static IReadOnlyList<string> Values { get; } = Array.AsReadOnly(
		new string[]
		{
			"true",
			"false",
			"null"
		});

	/// <summary>
	/// Gets the constants that have been removed from the TRX syntax.
	/// </summary>
	public static IReadOnlyList<RemovedKeyword> RemovedConstants { get; } = Array.AsReadOnly(
		new RemovedKeyword[]
		{
			new("cutscene", new(4, 8), "Instead, the cutscenes need to be placed in the top-level \"cutscenes\" array."),
			new("exit_to_cine", new(4, 8)),
			new("exit_to_level", new(4, 8), "Instead, use \"level_complete\". No parameter needed."),
			new("start_game", new(4, 8)),
			new("draw_distance_fade", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
			new("draw_distance_max", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
		});

	/// <summary>
	/// Gets the properties that have been removed from the TRX syntax.
	/// </summary>
	public static IReadOnlyList<RemovedKeyword> RemovedProperties { get; } = Array.AsReadOnly(
		new RemovedKeyword[]
		{
			new("file", new(4, 8), "Instead, use the \"path\" property for every level."),
			new("fmv_path", new(4, 8), "Instead, use the \"fmv_id\" property."),
			new("level_id", new(4, 8)),
			new("music", new(4, 8), "Instead, use the \"music_track\" property for every level."),
			new("picture_path", new(4, 8), "Instead, use the \"background_path\" property for \"total_stats\", or the \"path\" property for everything else."),
		});
}

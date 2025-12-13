using TombLib.Scripting.Tomb1Main.Services;

namespace TombLib.Scripting.Tomb1Main.Resources;

public sealed class Patterns
{
	public Patterns(IGameflowSchemaService schemaService)
	{
		Comments = "//.*$";

		var schemaKeywords = schemaService.GetSchemaKeywords();

		if (schemaKeywords != null)
		{
			Constants = $"\"\\b({string.Join("|", schemaKeywords.Constants)})\\b\"";
			Collections = $"\"\\b({string.Join("|", schemaKeywords.Collections)})\\b\"";
			Properties = $"\"\\b({string.Join("|", schemaKeywords.Properties)})\\b\"";
		}

		Values = $@"\b({string.Join("|", Keywords.Values)})\b";
		Strings = "\"(.+?)\"";
	}

	public string Comments { get; }
	public string Constants { get; }
	public string Collections { get; }
	public string Properties { get; }
	public string Values { get; }
	public string Strings { get; }

	public static string LevelProperty => "\"title\":\\s*\"";
	public static string LevelCommentName => @"^\s*\/\/\s*(Level)?\s*\d+\s*(:|\.)\s*(.+)$";
}

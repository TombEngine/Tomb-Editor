using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Defines the regex patterns used for TRX syntax highlighting. Schema-derived keywords are
/// regex-escaped before being embedded so schema text can never produce an invalid pattern.
/// </summary>
public sealed class Patterns
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Patterns"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to derive keyword patterns.</param>
	public Patterns(ITRXGameFlowSchemaService schemaService)
	{
		Comments = "//.*$";

		var schemaKeywords = schemaService.Keywords;

		Constants = BuildKeywordPattern(schemaKeywords.Constants);
		Collections = BuildKeywordPattern(schemaKeywords.Collections);
		Properties = BuildKeywordPattern(schemaKeywords.Properties);

		Values = $@"\b({string.Join("|", Keywords.Values)})\b";
		Strings = "\"(.+?)\"";
	}

	private static string BuildKeywordPattern(IReadOnlyList<string> keywords)
		=> keywords.Count == 0 ? string.Empty : $"\"\\b({string.Join("|", keywords.Select(Regex.Escape))})\\b\"";

	/// <summary>
	/// Gets the pattern matching line comments.
	/// </summary>
	public string Comments { get; }

	/// <summary>
	/// Gets the pattern matching constant keywords.
	/// </summary>
	public string Constants { get; }

	/// <summary>
	/// Gets the pattern matching collection keywords.
	/// </summary>
	public string Collections { get; }

	/// <summary>
	/// Gets the pattern matching property keywords.
	/// </summary>
	public string Properties { get; }

	/// <summary>
	/// Gets the pattern matching primitive values.
	/// </summary>
	public string Values { get; }

	/// <summary>
	/// Gets the pattern matching string literals.
	/// </summary>
	public string Strings { get; }

	/// <summary>
	/// Gets the pattern matching a TRX title property.
	/// </summary>
	public static string LevelProperty => "\"title\":\\s*\"";

	/// <summary>
	/// Gets the pattern matching a level-name comment such as <c>// Level 1: Name</c>.
	/// </summary>
	public static string LevelCommentName => @"^\s*\/\/\s*(Level)?\s*\d+\s*(:|\.)\s*(.+)$";
}

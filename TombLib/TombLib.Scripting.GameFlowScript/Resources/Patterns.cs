namespace TombLib.Scripting.GameFlowScript.Resources;

/// <summary>
/// Provides the regular expression patterns used by the GameFlow highlighting and navigation.
/// </summary>
public static class Patterns
{
	/// <summary>
	/// Gets the pattern for line comments.
	/// </summary>
	public static string Comments => "//.*$";

	/// <summary>
	/// Gets the pattern for block comments.
	/// </summary>
	public static string BlockComments => "/[*]([^*]|([*][^/]))*[*]+/";

	/// <summary>
	/// Gets the pattern for section definitions.
	/// </summary>
	public static string Sections => @"^\b(" + string.Join("|", GameFlowDefinitionCatalog.Sections) + @")\b:";

	/// <summary>
	/// Gets the pattern for special property definitions.
	/// </summary>
	public static string SpecialProperties => @"^\b(" + string.Join("|", GameFlowDefinitionCatalog.SpecialProperties) + @")\b:";

	/// <summary>
	/// Gets the pattern for property definitions.
	/// </summary>
	public static string Properties => @"^\s*\b(" + string.Join("|", GameFlowDefinitionCatalog.Properties) + @")\b:";

	/// <summary>
	/// Gets the pattern for constant definitions.
	/// </summary>
	public static string Constants => @"\b(" + string.Join("|", GameFlowDefinitionCatalog.Constants) + @")\b";

	/// <summary>
	/// Gets the pattern for literal values.
	/// </summary>
	public static string Values => "\\d|\\w|\"|'|\\.|\\\\";

	/// <summary>
	/// Gets the pattern for level property definitions.
	/// </summary>
	public static string LevelProperty => @"^\bLEVEL:\s*";
}

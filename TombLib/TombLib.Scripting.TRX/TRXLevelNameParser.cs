using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Resources;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Parses level names from TRX title property lines.
/// </summary>
public static class TRXLevelNameParser
{
	/// <summary>
	/// Matches a TRX title property line. The pattern has no <c>^</c> anchor, so
	/// leading whitespace before the title property is tolerated.
	/// </summary>
	public static readonly Regex LevelPropertyRegex = new(Patterns.LevelProperty, RegexOptions.IgnoreCase);

	/// <summary>
	/// Extracts the level name from a title property line, e.g. <c>"title": "MyLevel",</c>
	/// returns <c>MyLevel</c>.
	/// </summary>
	public static string ExtractTitleName(string lineText)
		=> LevelPropertyRegex.Replace(lineText, string.Empty).Trim().TrimEnd(',').Trim('"');
}

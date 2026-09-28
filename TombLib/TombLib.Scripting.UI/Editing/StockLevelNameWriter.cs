using System.Text.RegularExpressions;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Provides stock-level-name assignment helpers for language writers that share the TRX slot convention.
/// </summary>
public static class StockLevelNameWriter
{
	/// <summary>
	/// Assigns <paramref name="levelName"/> to the first "EMPTY STRING SLOT" line in the document.
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="levelName">The level name to write into the slot.</param>
	/// <returns><see langword="true"/> when a slot line was replaced; otherwise, <see langword="false"/>.</returns>
	public static bool TryAssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
		=> textEditor.TryReplaceFirstMatchingLine(
			lineText => Regex.IsMatch(lineText, @"EMPTY\sSTRING\sSLOT\s\d+") ? levelName : null,
			scrollToLine: false);
}

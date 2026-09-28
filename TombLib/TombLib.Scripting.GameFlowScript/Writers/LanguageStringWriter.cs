using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers;

/// <summary>
/// Writes GameFlow language string entries into an open editor.
/// </summary>
/// <remarks>
/// GameFlow has no NG-string mechanism, so level names are written into the shared stock
/// language slot used by ClassicScript rather than appending a new numbered string.
/// </remarks>
public sealed class LanguageStringWriter
{
	/// <summary>
	/// Writes a new level name string for the given level name.
	/// </summary>
	/// <param name="textEditor">The editor to write into.</param>
	/// <param name="levelName">The level name to write.</param>
	public void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		=> StockLevelNameWriter.TryAssignStockLevelNameStringSlot(textEditor, levelName);
}

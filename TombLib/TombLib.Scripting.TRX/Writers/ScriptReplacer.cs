using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.Comments;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.TRX.Writers;

/// <summary>
/// Applies script-wide renames to TRX documents.
/// </summary>
public sealed class ScriptReplacer
{
	private static readonly Regex s_levelPropertyRegex = TRXLevelNameParser.LevelPropertyRegex;

	/// <summary>
	/// Renames a level script in the given editor by replacing the matching title property value.
	/// </summary>
	/// <param name="textEditor">The editor containing the level script.</param>
	/// <param name="oldName">The current level name.</param>
	/// <param name="newName">The new level name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
	{
		textEditor.TryReplaceFirstMatchingLine(
			s_levelPropertyRegex,
			(lineText, _) => TRXLevelNameParser.ExtractTitleName(CommentOperations.RemoveComments(lineText, new CommentSyntax("//", null, StringLiteralStyle.DoubleQuoted))),
			oldName,
			newName);
	}
}

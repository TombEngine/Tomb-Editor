using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using Nickelony.IDEKit.Core.Comments;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.GameFlowScript.Writers;

/// <summary>
/// Performs script-wide renames inside an open GameFlow editor.
/// </summary>
public sealed class ScriptReplacer
{
	private static readonly Regex LevelPropertyRegex = new(Patterns.LevelProperty, RegexOptions.IgnoreCase);

	/// <summary>
	/// Renames a level script reference in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current level script name.</param>
	/// <param name="newName">The new level script name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
	{
		textEditor.TryReplaceFirstMatchingLine(
			LevelPropertyRegex,
			(lineText, regex) => regex.Replace(CommentOperations.RemoveComments(lineText, new CommentSyntax("//", null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.TripleDoubleQuoted)), string.Empty).Trim(),
			oldName,
			newName);
	}

	/// <summary>
	/// Renames a language string in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current language string name.</param>
	/// <param name="newName">The new language string name.</param>
	public void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
	{
		textEditor.TryReplaceFirstMatchingLine(
			lineText =>
			{
				string trimmedLineText = lineText.Trim();
				return trimmedLineText == oldName
					? lineText.Replace(oldName, newName)
					: null;
			});
	}
}

using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.ClassicScript.Writers;

/// <summary>
/// Performs script-wide renames inside an open ClassicScript editor.
/// </summary>
public sealed class ScriptReplacer
{
	private static readonly Regex NameCommandRegex = new(@"^\s*\bName\s*=\s*", RegexOptions.IgnoreCase);

	private readonly IClassicScriptLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ScriptReplacer"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to clean lines before matching.</param>
	public ScriptReplacer(IClassicScriptLineService lineService)
		=> _lineService = lineService;

	/// <summary>
	/// Renames a level script reference in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current level script name.</param>
	/// <param name="newName">The new level script name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
	{
		textEditor.TryReplaceFirstMatchingLine(
			lineText =>
			{
				if (!NameCommandRegex.IsMatch(lineText))
					return null;

				string cleanName = NameCommandRegex.Replace(_lineService.RemoveComments(lineText), string.Empty).Trim();
				return cleanName == oldName
					? ReplaceCodeValue(lineText, oldName, newName)
					: null;
			});
	}

	/// <summary>
	/// Renames a language string in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current language string name.</param>
	/// <param name="newName">The new language string name.</param>
	public void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
	{
		textEditor.TryReplaceFirstMatchingLine(lineText =>
		{
			string cleanString = _lineService.RemoveComments(_lineService.RemoveNGStringIndex(lineText)).Trim();
			return cleanString == oldName
				? ReplaceCodeValue(lineText, oldName, newName)
				: null;
		});
	}

	private static string ReplaceCodeValue(string lineText, string oldName, string newName)
	{
		TextRange codeRange = CommentHelper.GetCodeRange(lineText, new CommentSyntax(";", null, null, StringLiteralStyle.None));
		string codeText = lineText[..codeRange.Length];
		return codeText.Replace(oldName, newName) + lineText[codeRange.Length..];
	}
}

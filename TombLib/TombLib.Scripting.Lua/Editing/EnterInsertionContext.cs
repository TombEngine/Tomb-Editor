namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Describes the inputs required to compute the text inserted when Enter is pressed.
/// </summary>
/// <param name="LineTextBeforeCaret">The line text before the caret.</param>
/// <param name="LineTextAfterCaret">The line text after the caret.</param>
/// <param name="CurrentLineIndentation">The leading whitespace of the current line.</param>
/// <param name="IndentationUnit">The text appended for one additional indent level.</param>
/// <param name="NewLineText">The line terminator to use for the inserted new lines.</param>
/// <param name="UseSmartIndent">Whether language-aware smart-indent rules should apply.</param>
public readonly record struct EnterInsertionContext(
	string LineTextBeforeCaret,
	string LineTextAfterCaret,
	string CurrentLineIndentation,
	string IndentationUnit,
	string NewLineText,
	bool UseSmartIndent);

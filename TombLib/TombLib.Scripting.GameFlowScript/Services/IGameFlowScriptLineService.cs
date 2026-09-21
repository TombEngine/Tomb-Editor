namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Provides line-level operations for GameFlowScript text, including
/// comment removal, section header recognition, and empty-line detection.
/// </summary>
public interface IGameFlowScriptLineService
{
	/// <summary>
	/// Removes line comments from the text. Delegates to <c>CommentOperations.RemoveComments</c>
	/// with a <c>"//"</c> <c>CommentSyntax</c>.
	/// </summary>
	string RemoveComments(string lineText);

	/// <summary>
	/// Replaces comments with spaces to maintain the original string length.
	/// Delegates to <c>CommentOperations.MaskComments</c> with a <c>"//"</c> <c>CommentSyntax</c>.
	/// </summary>
	string EscapeComments(string lineText);

	/// <summary>
	/// Returns <c>true</c> when the line is null, empty, whitespace-only, or starts with <c>//</c>.
	/// </summary>
	bool IsEmptyOrComments(string? lineText);

	/// <summary>
	/// Returns <c>true</c> when the line matches a GameFlowScript section header keyword
	/// followed by a colon (<c>Keyword:</c>). Uses the <c>Patterns.Sections</c> regex.
	/// </summary>
	bool IsSectionHeaderLine(string lineText);

	/// <summary>
	/// Extracts the section keyword text from a section header line.
	/// Returns <c>null</c> when the line is not a recognized section header.
	/// </summary>
	string? GetSectionHeaderText(string lineText);
}

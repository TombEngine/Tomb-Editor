namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Provides line-level text operations for TRX gameflow scripts.
/// The seam is retained deliberately: besides forwarding comment handling to the Core
/// <c>CommentHelper</c> with a <c>"//"</c> <c>CommentSyntax</c>, it adds the
/// <see cref="IsEmptyOrComments"/> predicate that <c>CommentHelper</c> does not
/// expose, and it is consumed directly by TombIDE composition and TRX editor tests.
/// </summary>
public interface ITRXLineService
{
	/// <summary>
	/// Removes the line comment (including leading whitespace before <c>//</c>)
	/// from <paramref name="lineText"/>. Returns <see cref="string.Empty"/> for
	/// comment-only lines.
	/// </summary>
	string RemoveComments(string lineText);

	/// <summary>
	/// Replaces the comment portion of <paramref name="lineText"/> with spaces,
	/// preserving the original string length.
	/// </summary>
	string EscapeComments(string lineText);

	/// <summary>
	/// Returns <see langword="true"/> when <paramref name="lineText"/> is
	/// <see langword="null"/>, empty, whitespace-only, or consists entirely of
	/// a <c>//</c> comment (with or without leading whitespace).
	/// </summary>
	bool IsEmptyOrComments(string? lineText);
}

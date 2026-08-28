using System;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Default implementation of <see cref="ITRXLineService"/>.
/// Delegates comment removal and masking to <see cref="LineCommentHelper"/>
/// with the <c>"//"</c> delimiter.
/// </summary>
public sealed class TRXLineService : ITRXLineService
{
	private const string CommentDelimiter = "//";

	/// <inheritdoc/>
	public string RemoveComments(string lineText)
		=> LineCommentHelper.RemoveLineComment(lineText, CommentDelimiter);

	/// <inheritdoc/>
	public string EscapeComments(string lineText)
		=> LineCommentHelper.MaskLineComment(lineText, CommentDelimiter);

	/// <inheritdoc/>
	public bool IsEmptyOrComments(string? lineText)
		=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(CommentDelimiter, StringComparison.Ordinal);
}

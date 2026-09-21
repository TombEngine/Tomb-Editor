using Nickelony.IDEKit.Core.Comments;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Default implementation of <see cref="ITRXLineService"/>.
/// Delegates comment removal and masking to the shared <see cref="CommentOperations"/>
/// with a <c>"//"</c> <see cref="CommentSyntax"/>.
/// </summary>
public sealed class TRXLineService : ITRXLineService
{
	private static readonly CommentSyntax s_commentSyntax = new("//", null, StringLiteralStyle.DoubleQuoted);

	/// <inheritdoc/>
	public string RemoveComments(string lineText)
		=> CommentOperations.RemoveComments(lineText, s_commentSyntax);

	/// <inheritdoc/>
	public string EscapeComments(string lineText)
		=> CommentOperations.MaskComments(lineText, s_commentSyntax);

	/// <inheritdoc/>
	public bool IsEmptyOrComments(string? lineText)
			=> CommentOperations.IsBlankOrStartsWithLineComment(lineText, s_commentSyntax);
}

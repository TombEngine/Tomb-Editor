using Nickelony.IDEKit.Core.Comments;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Default implementation of <see cref="ITRXLineService"/>.
/// Delegates comment removal and masking to the shared <see cref="TextLineSyntaxService"/>
/// with a <c>"//"</c> <see cref="CommentSyntax"/>.
/// </summary>
public sealed class TRXLineService : TextLineSyntaxService, ITRXLineService
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TRXLineService"/> class.
	/// </summary>
	public TRXLineService()
		: base(new CommentSyntax("//", null, null, StringLiteralStyle.DoubleQuoted))
	{ }

	/// <inheritdoc/>
	public string RemoveComments(string lineText)
		=> base.RemoveComments(lineText);

	/// <inheritdoc/>
	public string EscapeComments(string lineText)
		=> base.EscapeComments(lineText);

	/// <inheritdoc/>
	public bool IsEmptyOrComments(string? lineText)
		=> base.IsEmptyOrComments(lineText);
}

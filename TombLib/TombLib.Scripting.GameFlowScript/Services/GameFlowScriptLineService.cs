using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using Nickelony.IDEKit.Core.Comments;

namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Default implementation of <see cref="IGameFlowScriptLineService"/>.
/// Provides line-level text operations using Core helpers and, where needed,
/// regex patterns for the GameFlow section-header syntax.
/// </summary>
public sealed class GameFlowScriptLineService : IGameFlowScriptLineService
{
	// Regex pattern for the GameFlow section-header syntax.
	private static readonly Regex SectionHeaderRegex = new(Patterns.Sections, RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly CommentSyntax s_commentSyntax = new("//", null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.TripleDoubleQuoted);

	/// <inheritdoc/>
	public string RemoveComments(string lineText)
		=> CommentOperations.RemoveComments(lineText, s_commentSyntax);

	/// <inheritdoc/>
	public string EscapeComments(string lineText)
		=> CommentOperations.MaskComments(lineText, s_commentSyntax);

	/// <inheritdoc/>
	public bool IsEmptyOrComments(string? lineText)
		=> CommentOperations.IsBlankOrStartsWithLineComment(lineText, s_commentSyntax);

	/// <inheritdoc/>
	public bool IsSectionHeaderLine(string lineText)
		=> SectionHeaderRegex.IsMatch(lineText);

	/// <inheritdoc/>
	public string? GetSectionHeaderText(string lineText)
	{
		Match match = SectionHeaderRegex.Match(lineText);

		if (!match.Success)
			return null;

		return match.Value.Trim().Trim(':');
	}
}

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
public sealed class GameFlowScriptLineService : TextLineSyntaxService, IGameFlowScriptLineService
{
	// Regex pattern for the GameFlow section-header syntax.
	private static readonly Regex SectionHeaderRegex = new(Patterns.Sections, RegexOptions.IgnoreCase | RegexOptions.Compiled);

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowScriptLineService"/> class.
	/// </summary>
	public GameFlowScriptLineService()
		: base(new CommentSyntax("//", null, null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.TripleDoubleQuoted))
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

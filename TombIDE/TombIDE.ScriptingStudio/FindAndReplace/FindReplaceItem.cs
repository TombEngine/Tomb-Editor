#nullable enable

using Nickelony.IDEKit.Core.Text;
using System;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Describes a single find-and-replace match within a document for presentation in search results.
/// </summary>
public sealed class FindReplaceItem
{
	/// <summary>
	/// Gets the one-based line number of the match.
	/// </summary>
	public int LineNumber { get; }

	/// <summary>
	/// Gets the full text of the line containing the match, as captured when the result was created.
	/// </summary>
	/// <remarks>
	/// The stored text is intended for presentation; <see cref="SearchResultLocationResolver"/>
	/// reads the line from the live document instead.
	/// </remarks>
	public string LineText { get; }

	/// <summary>
	/// Gets the matched text segment.
	/// </summary>
	public string MatchSegmentText { get; }

	/// <summary>
	/// Gets the zero-based index of this match among the matches on its line.
	/// </summary>
	public int MatchSegmentIndex { get; }

	/// <summary>
	/// Gets the zero-based offset and length of the match within <see cref="LineText"/>, when known.
	/// </summary>
	/// <remarks>
	/// When set, <see cref="SearchResultLocationResolver"/> selects this range directly instead of
	/// locating <see cref="MatchSegmentText"/> literally. Leave it <see langword="null"/> for results
	/// whose match offsets are unknown, such as persisted results; the resolver then falls back to the
	/// literal match. The range is measured against the captured line (not the document) and may extend
	/// beyond it when the match spans lines, so consumers compare it against the live line before use.
	/// </remarks>
	public TextRange? MatchRangeInLine { get; init; }

	/// <summary>
	/// Creates a find-and-replace match item.
	/// </summary>
	/// <param name="lineNumber">The one-based line number of the match.</param>
	/// <param name="lineText">The full text of the line containing the match.</param>
	/// <param name="matchSegmentText">The matched text segment.</param>
	/// <param name="matchSegmentIndex">The zero-based index of this match among the matches on its line.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="lineText"/> or <paramref name="matchSegmentText"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="lineNumber"/> is less than 1, or <paramref name="matchSegmentIndex"/> is negative.
	/// </exception>
	public FindReplaceItem(int lineNumber, string lineText, string matchSegmentText, int matchSegmentIndex)
	{
		ArgumentNullException.ThrowIfNull(lineText);
		ArgumentNullException.ThrowIfNull(matchSegmentText);

		ArgumentOutOfRangeException.ThrowIfLessThan(lineNumber, 1);
		ArgumentOutOfRangeException.ThrowIfNegative(matchSegmentIndex);

		LineNumber = lineNumber;
		LineText = lineText;
		MatchSegmentText = matchSegmentText;
		MatchSegmentIndex = matchSegmentIndex;
	}
}

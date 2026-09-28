#nullable enable

using System;
using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Core.Navigation;
using Nickelony.IDEKit.Core.Text;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Resolves a stored <see cref="FindReplaceItem"/> to a <see cref="NavigationLocation"/> in a live
/// document, so navigating to a search result re-selects the match.
/// </summary>
/// <remarks>
/// <para>
/// When the item carries a <see cref="FindReplaceItem.MatchRangeInLine"/> that still fits its line,
/// that range is selected directly. Otherwise the stored match text is matched literally on its
/// line, so characters such as <c>(</c>, <c>.</c>, or <c>$</c> in a match are not treated as
/// regular-expression syntax.
/// </para>
/// <para>
/// Search modes that are not literal, such as whole-word or regular-expression searches, cannot be
/// reconstructed from the item. The same applies to search modes that index occurrences differently,
/// for example with overlapping matches; map such matches in a caller that knows the search options.
/// </para>
/// </remarks>
public static class SearchResultLocationResolver
{
	/// <summary>
	/// Resolves a location for a search result.
	/// </summary>
	/// <param name="document">The document that contains the search result.</param>
	/// <param name="filePath">The logical path of the document.</param>
	/// <param name="item">The search result to re-select.</param>
	/// <param name="comparison">
	/// The comparison used to match the stored match text. Defaults to <see cref="StringComparison.Ordinal"/>
	/// and can be set to <see cref="StringComparison.OrdinalIgnoreCase"/> to re-select a match found by a
	/// case-insensitive search.
	/// </param>
	/// <returns>
	/// The resolution outcome: a <see cref="SearchResultLocationStatus"/> and the resolved
	/// <see cref="SearchResultLocation.Location"/>, which is <see langword="null"/> only for
	/// <see cref="SearchResultLocationStatus.LineNotFound"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="document"/>, <paramref name="filePath"/>, or <paramref name="item"/> is <see langword="null"/>.
	/// </exception>
	public static SearchResultLocation Resolve(
		TextDocument document,
		string filePath,
		FindReplaceItem item,
		StringComparison comparison = StringComparison.Ordinal)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(filePath);
		ArgumentNullException.ThrowIfNull(item);

		if (item.LineNumber < 1 || item.LineNumber > document.LineCount)
			return new(SearchResultLocationStatus.LineNotFound, null);

		DocumentLine line = document.GetLineByNumber(item.LineNumber);
		string lineText = document.GetText(line);

		// An exact match range is preferred when the result carries one and it still fits the line;
		// results without offsets fall back to locating the stored match text literally.
		if (item.MatchRangeInLine is TextRange matchRange && matchRange.EndOffset <= lineText.Length)
		{
			int selectionStart = document.ClampOffset(line.Offset + matchRange.Offset);

			return new(
				SearchResultLocationStatus.MatchLocated,
				new NavigationLocation(filePath, selectionStart, selectionStart, matchRange.Length, line.LineNumber));
		}

		// An empty match matches at every position, which has no meaningful occurrence index.
		if (item.MatchSegmentText.Length > 0
			&& TryGetMatchIndex(lineText, item.MatchSegmentText, comparison, item.MatchSegmentIndex) is int matchIndex)
		{
			int selectionStart = document.ClampOffset(line.Offset + matchIndex);

			return new(
				SearchResultLocationStatus.MatchLocated,
				new NavigationLocation(filePath, selectionStart, selectionStart, item.MatchSegmentText.Length, line.LineNumber));
		}

		return new(
			SearchResultLocationStatus.LineStartFallback,
			new NavigationLocation(filePath, line.Offset, line.Offset, 0, line.LineNumber));
	}

	/// <summary>
	/// Finds the character index of the requested non-overlapping literal occurrence
	/// of <paramref name="matchText"/>.
	/// </summary>
	/// <returns>The match index, or <see langword="null"/> when the requested occurrence does not exist.</returns>
	private static int? TryGetMatchIndex(
		string lineText,
		string matchText,
		StringComparison comparison,
		int matchIndex)
	{
		if (matchIndex < 0 || matchText.Length > lineText.Length)
			return null;

		int position = 0;

		for (int occurrence = 0; occurrence <= matchIndex; occurrence++)
		{
			int found = lineText.IndexOf(matchText, position, comparison);

			if (found < 0)
				return null;

			if (occurrence == matchIndex)
				return found;

			position = found + matchText.Length;
		}

		return null;
	}
}

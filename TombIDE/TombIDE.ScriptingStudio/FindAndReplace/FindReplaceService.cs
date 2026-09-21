#nullable enable

using System;
using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.FindReplace;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Builds find-and-replace result sources from an editor document. The text-level search and
/// replace primitives live in <see cref="Nickelony.IDEKit.Core.FindReplace.FindReplaceText"/>.
/// </summary>
public sealed class FindReplaceService
{
	/// <summary>
	/// Builds a <see cref="FindReplaceSource"/> from a document's match collection,
	/// mapping each match to its line number and line text.
	/// </summary>
	public FindReplaceSource BuildFindReplaceSource(
		string documentName,
		TextEditorBase textEditor,
		string pattern,
		RegexOptions options)
	{
		ArgumentNullException.ThrowIfNull(textEditor);

		MatchCollection documentMatches = FindReplaceText.FindAllMatches(textEditor.Text, pattern, options);

		if (documentMatches.Count == 0)
			return new FindReplaceSource { Name = documentName };

		var source = new FindReplaceSource { Name = documentName };

		foreach (Match match in documentMatches)
		{
			var line = textEditor.Document.GetLineByOffset(match.Index);
			string lineText = textEditor.Document.GetText(line.Offset, line.Length);
			string matchSegmentText = match.Value;

			string lineTextBeforeMatch = lineText.Substring(0, match.Index - line.Offset);
			int matchSegmentIndex = Regex.Matches(lineTextBeforeMatch, Regex.Escape(match.Value)).Count;

			source.Add(new FindReplaceItem(line.LineNumber, lineText, matchSegmentText, matchSegmentIndex)
			{
				// The exact match offsets make navigation independent of occurrence-index heuristics.
				MatchRangeInLine = new TextRange(match.Index - line.Offset, match.Length)
			});
		}

		return source;
	}
}

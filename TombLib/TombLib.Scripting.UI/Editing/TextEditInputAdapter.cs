using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Core.Text;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Editing;

internal static class TextEditInputAdapter
{
	public static IEnumerable<TextEditInput?> Convert(
		ITextSnapshot snapshot,
		IEnumerable<TextEdit> edits)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(edits);

		foreach (TextEdit? edit in edits)
		{
			if (edit is null)
			{
				yield return null;
				continue;
			}

			if (!TryGetOffset(snapshot, edit.Range.StartLineNumber, edit.Range.StartColumnNumber, out int startOffset)
				|| !TryGetOffset(snapshot, edit.Range.EndLineNumber, edit.Range.EndColumnNumber, out int endOffset)
				|| endOffset < startOffset)
			{
				yield return new TextEditInput(new TextRange(snapshot.TextLength, 1), edit.NewText ?? string.Empty);
				continue;
			}

			yield return new TextEditInput(
				new TextRange(startOffset, endOffset - startOffset),
				edit.NewText ?? string.Empty);
		}
	}

	private static bool TryGetOffset(
		ITextSnapshot snapshot,
		int lineNumber,
		int columnNumber,
		out int offset)
	{
		offset = 0;

		if (lineNumber < 1 || lineNumber > snapshot.LineCount || columnNumber < 1)
			return false;

		ITextLine line = snapshot.GetLineByNumber(lineNumber);
		int columnOffset = columnNumber - 1;
		if (columnOffset > line.Length)
			return false;

		offset = line.Offset + columnOffset;
		return true;
	}
}

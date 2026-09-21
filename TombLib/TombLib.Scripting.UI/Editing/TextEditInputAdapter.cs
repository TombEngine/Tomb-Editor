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

			if (!TryGetOffset(snapshot, edit.Range.Start.Line, edit.Range.Start.Character, out int startOffset)
				|| !TryGetOffset(snapshot, edit.Range.End.Line, edit.Range.End.Character, out int endOffset)
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
		int lineIndex,
		int character,
		out int offset)
	{
		offset = 0;

		if (lineIndex < 0 || lineIndex >= snapshot.LineCount || character < 0)
			return false;

		ITextLine line = snapshot.GetLineByNumber(lineIndex + 1);

		if (character > line.Length)
			return false;

		offset = line.Offset + character;
		return true;
	}
}

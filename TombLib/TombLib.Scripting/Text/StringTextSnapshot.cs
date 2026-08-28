using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Text;

/// <summary>
/// An <see cref="ITextSnapshot"/> backed by an immutable string.
/// Line metadata is precomputed at construction time.
/// Recognizes LF, CRLF, and CR as line endings.
/// </summary>
public sealed class StringTextSnapshot : ITextSnapshot
{
	private readonly string _text;
	private readonly StringTextLine[] _lines;
	private readonly int[] _lineStartOffsets;

	/// <summary>
	/// Initializes a new instance of the <see cref="StringTextSnapshot"/> class.
	/// </summary>
	/// <param name="text">The source text. A <see langword="null"/> value is treated as an empty string.</param>
	/// <param name="fileName">An optional file name associated with this text.</param>
	public StringTextSnapshot(string? text, string? fileName = null)
	{
		_text = text ?? string.Empty;

		FileName = fileName;

		(_lines, _lineStartOffsets) = BuildLines(_text);
	}

	/// <inheritdoc/>
	public string? FileName { get; }

	/// <inheritdoc/>
	public int TextLength => _text.Length;

	/// <inheritdoc/>
	public int LineCount => _lines.Length;

	/// <inheritdoc/>
	public IEnumerable<ITextLine> Lines => _lines;

	/// <inheritdoc/>
	public char GetCharAt(int offset)
	{
		if (offset < 0 || offset >= _text.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));

		return _text[offset];
	}

	/// <inheritdoc/>
	public string GetText(int offset, int length)
	{
		// Overflow-safe bounds check: the subtraction cannot overflow because
		// offset is verified to be within the text length first.
		if (offset < 0 || offset > _text.Length || length < 0 || length > _text.Length - offset)
			throw new ArgumentOutOfRangeException(nameof(offset));

		return _text.Substring(offset, length);
	}

	/// <inheritdoc/>
	public ITextLine GetLineByOffset(int offset)
	{
		if (offset < 0 || offset > _text.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));

		// Offset equal to TextLength always belongs to the last line.
		if (offset == _text.Length && _lines.Length > 0)
			return _lines[_lines.Length - 1];

		// Binary search for the line containing the offset.
		// A line owns offsets from its start offset up to (but not including)
		// the next line's start offset, or TextLength for the last line.
		int lo = 0;
		int hi = _lineStartOffsets.Length - 1;

		while (lo <= hi)
		{
			int mid = (lo + hi) / 2;
			int lineStart = _lineStartOffsets[mid];
			int nextLineStart = (mid + 1 < _lineStartOffsets.Length) ? _lineStartOffsets[mid + 1] : _text.Length;

			if (offset < lineStart)
				hi = mid - 1;
			else if (offset >= nextLineStart)
				lo = mid + 1;
			else
				return _lines[mid];
		}

		// This should not be reachable with valid input.
		throw new ArgumentOutOfRangeException(nameof(offset));
	}

	/// <inheritdoc/>
	public ITextLine GetLineByNumber(int lineNumber)
	{
		if (lineNumber < 1 || lineNumber > _lines.Length)
			throw new ArgumentOutOfRangeException(nameof(lineNumber));

		return _lines[lineNumber - 1];
	}

	/// <summary>
	/// Builds the line index by scanning the text for LF, CRLF, and CR line endings.
	/// Returns the line objects and their start offsets for binary search.
	/// </summary>
	private static (StringTextLine[] lines, int[] lineStartOffsets) BuildLines(string text)
	{
		if (text.Length == 0)
		{
			var emptyLine = new StringTextLine(0, 0, 1);
			return ([emptyLine], [0]);
		}

		var lines = new List<StringTextLine>();
		var lineStartOffsets = new List<int>();
		int lineStart = 0;
		int lineNumber = 1;
		int i = 0;

		while (i < text.Length)
		{
			char ch = text[i];

			if (ch == '\r')
			{
				// CRLF or standalone CR.
				int delimiterLength = (i + 1 < text.Length && text[i + 1] == '\n') ? 2 : 1;
				int lineLength = i - lineStart;

				lines.Add(new StringTextLine(lineStart, lineLength, lineNumber));
				lineStartOffsets.Add(lineStart);

				lineNumber++;
				i += delimiterLength;

				lineStart = i;
			}
			else if (ch == '\n')
			{
				// Standalone LF.
				int lineLength = i - lineStart;

				lines.Add(new StringTextLine(lineStart, lineLength, lineNumber));
				lineStartOffsets.Add(lineStart);

				lineNumber++;
				i++;

				lineStart = i;
			}
			else
			{
				i++;
			}
		}

		// Add the final line (may be empty if text ends with a newline).
		int finalLength = text.Length - lineStart;
		lines.Add(new StringTextLine(lineStart, finalLength, lineNumber));
		lineStartOffsets.Add(lineStart);

		return (lines.ToArray(), lineStartOffsets.ToArray());
	}

	/// <summary>
	/// Internal implementation of <see cref="ITextLine"/> for string-backed sources.
	/// </summary>
	private sealed class StringTextLine : ITextLine
	{
		public StringTextLine(int offset, int length, int lineNumber)
		{
			Offset = offset;
			Length = length;
			EndOffset = offset + length;
			LineNumber = lineNumber;
		}

		public int Offset { get; }

		public int Length { get; }

		public int EndOffset { get; }

		public int LineNumber { get; }
	}
}

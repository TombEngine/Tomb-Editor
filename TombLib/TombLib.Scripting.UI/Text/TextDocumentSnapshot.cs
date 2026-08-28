using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.UI.Text;

/// <summary>
/// An immutable <see cref="ITextSnapshot"/> that captures the content of an AvalonEdit
/// <see cref="ICSharpCode.AvalonEdit.Document.TextDocument"/> at construction time.
/// The snapshot does not observe later document edits and may be retained freely.
/// This type lives in the UI layer because it depends on AvalonEdit.
/// </summary>
public sealed class TextDocumentSnapshot : ITextSnapshot
{
	private readonly StringTextSnapshot _snapshot;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDocumentSnapshot"/> class
	/// by capturing the document content and file name.
	/// </summary>
	/// <param name="document">The AvalonEdit document to snapshot. Must not be null.</param>
	/// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
	public TextDocumentSnapshot(TextDocument document)
	{
		ArgumentNullException.ThrowIfNull(document);
		_snapshot = new StringTextSnapshot(document.Text, document.FileName);
	}

	/// <inheritdoc/>
	public string? FileName => _snapshot.FileName;

	/// <inheritdoc/>
	public int TextLength => _snapshot.TextLength;

	/// <inheritdoc/>
	public int LineCount => _snapshot.LineCount;

	/// <inheritdoc/>
	public char GetCharAt(int offset)
		=> _snapshot.GetCharAt(offset);

	/// <inheritdoc/>
	public string GetText(int offset, int length)
		=> _snapshot.GetText(offset, length);

	/// <inheritdoc/>
	public ITextLine GetLineByOffset(int offset)
		=> _snapshot.GetLineByOffset(offset);

	/// <inheritdoc/>
	public ITextLine GetLineByNumber(int lineNumber)
		=> _snapshot.GetLineByNumber(lineNumber);

	/// <inheritdoc/>
	public IEnumerable<ITextLine> Lines => _snapshot.Lines;
}

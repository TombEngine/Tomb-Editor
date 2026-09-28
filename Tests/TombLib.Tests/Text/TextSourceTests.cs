using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Shared table-driven tests for <see cref="StringTextSnapshot"/> and <see cref="TextDocumentSnapshot"/>.
/// Every test runs against both implementations to ensure behavioral parity.
/// </summary>
[TestClass]
public class TextSourceTests
{
    /// <summary>
    /// Creates both implementations from the same text content.
    /// </summary>
    private static IEnumerable<ITextSnapshot> CreateSources(string text)
    {
        yield return new StringTextSnapshot(text);
        yield return new TextDocumentSnapshot(new TextDocument(text));
    }

    // ---------------------------------------------------------------------------
    // Empty document
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void EmptyDocument_TextLength_IsZero()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            Assert.AreEqual(0, source.TextLength);
            Assert.AreEqual(1, source.LineCount);
        }
    }

    [TestMethod]
    public void StringTextSnapshot_NullText_TreatedAsEmpty()
    {
        var source = new StringTextSnapshot(null);

        Assert.AreEqual(0, source.TextLength);
        Assert.AreEqual(1, source.LineCount);
        Assert.AreEqual(string.Empty, source.GetText(0, 0));
    }

    [TestMethod]
    public void StringTextSnapshot_EmptyText_HasSingleEmptyLine()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.AreEqual(0, source.TextLength);
        Assert.AreEqual(1, source.LineCount);
        Assert.AreEqual(string.Empty, source.GetText(0, 0));
    }

    [TestMethod]
    public void EmptyDocument_GetLineByNumber_ReturnsEmptyLine()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            ITextLine line = source.GetLineByNumber(1);

            Assert.AreEqual(1, line.LineNumber);
            Assert.AreEqual(0, line.Offset);
            Assert.AreEqual(0, line.Length);
            Assert.AreEqual(0, line.EndOffset);
        }
    }

    [TestMethod]
    public void EmptyDocument_GetLineByOffset_AtZero_ReturnsFirstLine()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            ITextLine line = source.GetLineByOffset(0);

            Assert.AreEqual(1, line.LineNumber);
            Assert.AreEqual(0, line.Offset);
            Assert.AreEqual(0, line.Length);
        }
    }

    [TestMethod]
    public void EmptyDocument_GetLineByOffset_AtTextLength_ReturnsFirstLine()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            ITextLine line = source.GetLineByOffset(0);

            Assert.AreEqual(1, line.LineNumber);
        }
    }

    [TestMethod]
    public void EmptyDocument_GetCharAt_Throws()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetCharAt(0));
        }
    }

    [TestMethod]
    public void EmptyDocument_GetLineByNumber_Zero_Throws()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetLineByNumber(0));
        }
    }

    [TestMethod]
    public void EmptyDocument_GetLineByNumber_BeyondCount_Throws()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetLineByNumber(2));
        }
    }

    [TestMethod]
    public void EmptyDocument_Lines_Enumeration_YieldsOneLine()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            List<ITextLine> lines = source.Lines.ToList();

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual(1, lines[0].LineNumber);
        }
    }

    // ---------------------------------------------------------------------------
    // Single line, no trailing newline
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SingleLine_NoTrailingNewline_Properties()
    {
        foreach (ITextSnapshot source in CreateSources("hello"))
        {
            Assert.AreEqual(5, source.TextLength);
            Assert.AreEqual(1, source.LineCount);

            ITextLine line = source.GetLineByNumber(1);

            Assert.AreEqual(1, line.LineNumber);
            Assert.AreEqual(0, line.Offset);
            Assert.AreEqual(5, line.Length);
            Assert.AreEqual(5, line.EndOffset);
        }
    }

    [TestMethod]
    public void SingleLine_NoTrailingNewline_GetCharAt()
    {
        foreach (ITextSnapshot source in CreateSources("hello"))
        {
            Assert.AreEqual('h', source.GetCharAt(0));
            Assert.AreEqual('o', source.GetCharAt(4));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetCharAt(5));
        }
    }

    [TestMethod]
    public void SingleLine_NoTrailingNewline_GetText()
    {
        foreach (ITextSnapshot source in CreateSources("hello"))
        {
            Assert.AreEqual("hello", source.GetText(0, 5));
            Assert.AreEqual("hel", source.GetText(0, 3));
            Assert.AreEqual("llo", source.GetText(2, 3));
        }
    }

    [TestMethod]
    public void SingleLine_NoTrailingNewline_GetLineByOffset()
    {
        foreach (ITextSnapshot source in CreateSources("hello"))
        {
            ITextLine line0 = source.GetLineByOffset(0);
            Assert.AreEqual(1, line0.LineNumber);

            ITextLine line4 = source.GetLineByOffset(4);
            Assert.AreEqual(1, line4.LineNumber);

            ITextLine line5 = source.GetLineByOffset(5);
            Assert.AreEqual(1, line5.LineNumber);
        }
    }

    // ---------------------------------------------------------------------------
    // Single line with trailing LF
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SingleLine_TrailingLF_Properties()
    {
        foreach (ITextSnapshot source in CreateSources("hello\n"))
        {
            Assert.AreEqual(6, source.TextLength);
            Assert.AreEqual(2, source.LineCount);

            ITextLine first = source.GetLineByNumber(1);

            Assert.AreEqual(1, first.LineNumber);
            Assert.AreEqual(0, first.Offset);
            Assert.AreEqual(5, first.Length);
            Assert.AreEqual(5, first.EndOffset);

            ITextLine second = source.GetLineByNumber(2);

            Assert.AreEqual(2, second.LineNumber);
            Assert.AreEqual(6, second.Offset);
            Assert.AreEqual(0, second.Length);
            Assert.AreEqual(6, second.EndOffset);
        }
    }

    [TestMethod]
    public void SingleLine_TrailingLF_GetCharAt()
    {
        foreach (ITextSnapshot source in CreateSources("hello\n"))
        {
            Assert.AreEqual('h', source.GetCharAt(0));
            Assert.AreEqual('o', source.GetCharAt(4));
            Assert.AreEqual('\n', source.GetCharAt(5));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetCharAt(6));
        }
    }

    // ---------------------------------------------------------------------------
    // Single line with trailing CRLF
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SingleLine_TrailingCRLF_Properties()
    {
        foreach (ITextSnapshot source in CreateSources("hello\r\n"))
        {
            Assert.AreEqual(7, source.TextLength);
            Assert.AreEqual(2, source.LineCount);

            ITextLine first = source.GetLineByNumber(1);

            Assert.AreEqual(1, first.LineNumber);
            Assert.AreEqual(0, first.Offset);
            Assert.AreEqual(5, first.Length);
            Assert.AreEqual(5, first.EndOffset);

            ITextLine second = source.GetLineByNumber(2);

            Assert.AreEqual(2, second.LineNumber);
            Assert.AreEqual(7, second.Offset);
            Assert.AreEqual(0, second.Length);
            Assert.AreEqual(7, second.EndOffset);
        }
    }

    // ---------------------------------------------------------------------------
    // Single line with trailing CR
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void SingleLine_TrailingCR_Properties()
    {
        foreach (ITextSnapshot source in CreateSources("hello\r"))
        {
            Assert.AreEqual(6, source.TextLength);
            Assert.AreEqual(2, source.LineCount);

            ITextLine first = source.GetLineByNumber(1);

            Assert.AreEqual(1, first.LineNumber);
            Assert.AreEqual(0, first.Offset);
            Assert.AreEqual(5, first.Length);
            Assert.AreEqual(5, first.EndOffset);

            ITextLine second = source.GetLineByNumber(2);

            Assert.AreEqual(2, second.LineNumber);
            Assert.AreEqual(6, second.Offset);
            Assert.AreEqual(0, second.Length);
            Assert.AreEqual(6, second.EndOffset);
        }
    }

    // ---------------------------------------------------------------------------
    // Multi-line with LF
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void MultiLine_LF_LineCount_And_Offsets()
    {
        foreach (ITextSnapshot source in CreateSources("a\nbb\nccc"))
        {
            Assert.AreEqual(8, source.TextLength);
            Assert.AreEqual(3, source.LineCount);

            ITextLine line1 = source.GetLineByNumber(1);
            Assert.AreEqual(1, line1.LineNumber);
            Assert.AreEqual(0, line1.Offset);
            Assert.AreEqual(1, line1.Length);
            Assert.AreEqual(1, line1.EndOffset);

            ITextLine line2 = source.GetLineByNumber(2);
            Assert.AreEqual(2, line2.LineNumber);
            Assert.AreEqual(2, line2.Offset);
            Assert.AreEqual(2, line2.Length);
            Assert.AreEqual(4, line2.EndOffset);

            ITextLine line3 = source.GetLineByNumber(3);
            Assert.AreEqual(3, line3.LineNumber);
            Assert.AreEqual(5, line3.Offset);
            Assert.AreEqual(3, line3.Length);
            Assert.AreEqual(8, line3.EndOffset);
        }
    }

    [TestMethod]
    public void MultiLine_LF_GetLineByOffset_AtLineStart()
    {
        foreach (ITextSnapshot source in CreateSources("a\nbb\nccc"))
        {
            Assert.AreEqual(1, source.GetLineByOffset(0).LineNumber);
            Assert.AreEqual(2, source.GetLineByOffset(2).LineNumber);
            Assert.AreEqual(3, source.GetLineByOffset(5).LineNumber);
        }
    }

    [TestMethod]
    public void MultiLine_LF_GetLineByOffset_InsideLine()
    {
        foreach (ITextSnapshot source in CreateSources("a\nbb\nccc"))
        {
            Assert.AreEqual(1, source.GetLineByOffset(0).LineNumber);
            Assert.AreEqual(2, source.GetLineByOffset(3).LineNumber);
            Assert.AreEqual(3, source.GetLineByOffset(6).LineNumber);
        }
    }

    [TestMethod]
    public void MultiLine_LF_GetText()
    {
        foreach (ITextSnapshot source in CreateSources("a\nbb\nccc"))
        {
            Assert.AreEqual("a\nbb\nccc", source.GetText(0, 8));
            Assert.AreEqual("a", source.GetText(0, 1));
            Assert.AreEqual("bb", source.GetText(2, 2));
            Assert.AreEqual("ccc", source.GetText(5, 3));
        }
    }

    // ---------------------------------------------------------------------------
    // Multi-line with CRLF
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void MultiLine_CRLF_LineCount_And_Offsets()
    {
        foreach (ITextSnapshot source in CreateSources("a\r\nbb\r\nccc"))
        {
            Assert.AreEqual(10, source.TextLength);
            Assert.AreEqual(3, source.LineCount);

            ITextLine line1 = source.GetLineByNumber(1);
            Assert.AreEqual(1, line1.LineNumber);
            Assert.AreEqual(0, line1.Offset);
            Assert.AreEqual(1, line1.Length);
            Assert.AreEqual(1, line1.EndOffset);

            ITextLine line2 = source.GetLineByNumber(2);
            Assert.AreEqual(2, line2.LineNumber);
            Assert.AreEqual(3, line2.Offset);
            Assert.AreEqual(2, line2.Length);
            Assert.AreEqual(5, line2.EndOffset);

            ITextLine line3 = source.GetLineByNumber(3);
            Assert.AreEqual(3, line3.LineNumber);
            Assert.AreEqual(7, line3.Offset);
            Assert.AreEqual(3, line3.Length);
            Assert.AreEqual(10, line3.EndOffset);
        }
    }

    [TestMethod]
    public void MultiLine_CRLF_GetLineByOffset()
    {
        foreach (ITextSnapshot source in CreateSources("a\r\nbb\r\nccc"))
        {
            Assert.AreEqual(1, source.GetLineByOffset(0).LineNumber);
            Assert.AreEqual(1, source.GetLineByOffset(1).LineNumber);   // \r belongs to line 1's terminator.
            Assert.AreEqual(1, source.GetLineByOffset(2).LineNumber);   // \n belongs to line 1's terminator.
            Assert.AreEqual(2, source.GetLineByOffset(5).LineNumber);   // \r at end of line 2 content.
            Assert.AreEqual(3, source.GetLineByOffset(7).LineNumber);
        }
    }

    // ---------------------------------------------------------------------------
    // Trailing empty line after LF
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TrailingEmptyLine_AfterLF_Properties()
    {
        foreach (ITextSnapshot source in CreateSources("hello\n"))
        {
            Assert.AreEqual(2, source.LineCount);

            ITextLine last = source.GetLineByNumber(2);
            Assert.AreEqual(0, last.Length);
            Assert.AreEqual(source.TextLength, last.Offset);
            Assert.AreEqual(source.TextLength, last.EndOffset);
        }
    }

    // ---------------------------------------------------------------------------
    // Mixed line endings (LF + CRLF in same document)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void MixedLineEndings_Properties()
    {
        // This verifies each implementation handles mixed endings consistently.
        foreach (ITextSnapshot source in CreateSources("line1\nline2\r\nline3"))
        {
            Assert.AreEqual(3, source.LineCount);

            ITextLine line1 = source.GetLineByNumber(1);
            Assert.AreEqual(5, line1.Length);
            Assert.AreEqual("line1", source.GetText(line1.Offset, line1.Length));

            ITextLine line2 = source.GetLineByNumber(2);
            Assert.AreEqual(5, line2.Length);
            Assert.AreEqual("line2", source.GetText(line2.Offset, line2.Length));

            ITextLine line3 = source.GetLineByNumber(3);
            Assert.AreEqual(5, line3.Length);
            Assert.AreEqual("line3", source.GetText(line3.Offset, line3.Length));
        }
    }

    // ---------------------------------------------------------------------------
    // Lines enumeration
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Lines_Enumeration_Order_MatchesLineNumbers()
    {
        foreach (ITextSnapshot source in CreateSources("first\nsecond\nthird"))
        {
            int expectedLineNumber = 1;

            foreach (ITextLine line in source.Lines)
            {
                Assert.AreEqual(expectedLineNumber, line.LineNumber);
                expectedLineNumber++;
            }

            Assert.AreEqual(4, expectedLineNumber);
        }
    }

    // ---------------------------------------------------------------------------
    // FileName
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void FileName_StringTextSnapshot_ReturnsSuppliedName()
    {
        var source = new StringTextSnapshot("text", @"C:\path\to\file.txt");

        Assert.AreEqual(@"C:\path\to\file.txt", source.FileName);
    }

    [TestMethod]
    public void FileName_StringTextSnapshot_DefaultsToNull()
    {
        var source = new StringTextSnapshot("text");

        Assert.IsNull(source.FileName);
    }

    [TestMethod]
    public void FileName_TextDocumentSnapshot_ReturnsDocumentFileName()
    {
        var document = new TextDocument("text");
        document.FileName = @"C:\path\to\file.txt";
        var source = new TextDocumentSnapshot(document);

        Assert.AreEqual(@"C:\path\to\file.txt", source.FileName);
    }

    // ---------------------------------------------------------------------------
    // LF vs CRLF parity
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void LfAndCrlf_SameLogicalContent_HaveEqualLineLengths()
    {
        string lfText = "abc\ndef\n";
        string crlfText = "abc\r\ndef\r\n";

        var lfSource = new StringTextSnapshot(lfText);
        var crlfSource = new StringTextSnapshot(crlfText);

        Assert.AreEqual(lfSource.LineCount, crlfSource.LineCount);

        for (int i = 1; i <= lfSource.LineCount; i++)
        {
            Assert.AreEqual(
                lfSource.GetLineByNumber(i).Length,
                crlfSource.GetLineByNumber(i).Length,
                $"Line {i} length mismatch");
            Assert.AreEqual(
                lfSource.GetText(lfSource.GetLineByNumber(i).Offset, lfSource.GetLineByNumber(i).Length),
                crlfSource.GetText(crlfSource.GetLineByNumber(i).Offset, crlfSource.GetLineByNumber(i).Length),
                $"Line {i} content mismatch");
        }
    }

    // ---------------------------------------------------------------------------
    // Out of range boundary validation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetLineByOffset_NegativeOffset_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetLineByOffset(-1));
        }
    }

    [TestMethod]
    public void GetLineByOffset_BeyondTextLength_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetLineByOffset(source.TextLength + 1));
        }
    }

    [TestMethod]
    public void GetCharAt_NegativeOffset_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetCharAt(-1));
        }
    }

    [TestMethod]
    public void GetText_NegativeLength_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetText(0, -1));
        }
    }

    [TestMethod]
    public void GetText_BeyondBounds_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetText(0, 5));
        }
    }

    // ---------------------------------------------------------------------------
    // Offset at end of document
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void OffsetAtEnd_EmptyDocument_GetText_ReturnsEmpty()
    {
        foreach (ITextSnapshot source in CreateSources(string.Empty))
        {
            Assert.AreEqual(string.Empty, source.GetText(0, 0));
        }
    }

    [TestMethod]
    public void OffsetAtEnd_TrailingNewline_GetLineByOffset_ReturnsTrailingEmptyLine()
    {
        foreach (ITextSnapshot source in CreateSources("hello\n"))
        {
            ITextLine line = source.GetLineByOffset(source.TextLength);

            Assert.AreEqual(2, line.LineNumber);
            Assert.AreEqual(source.TextLength, line.Offset);
            Assert.AreEqual(0, line.Length);
        }
    }

    [TestMethod]
    public void OffsetAtEnd_TrailingNewline_GetText_ReturnsEmpty()
    {
        foreach (ITextSnapshot source in CreateSources("hello\n"))
        {
            Assert.AreEqual(string.Empty, source.GetText(source.TextLength, 0));
        }
    }

    [TestMethod]
    public void OffsetAtEnd_MultiLine_GetLineByOffset_ReturnsLastLine()
    {
        foreach (ITextSnapshot source in CreateSources("a\nbb\nccc"))
        {
            ITextLine line = source.GetLineByOffset(source.TextLength);

            Assert.AreEqual(3, line.LineNumber);
            Assert.AreEqual("ccc", source.GetText(line.Offset, line.Length));
        }
    }

    [TestMethod]
    public void OffsetAtEnd_GetCharAt_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetCharAt(source.TextLength));
        }
    }

    [TestMethod]
    public void OffsetAtEnd_GetText_PastEnd_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetText(source.TextLength, 1));
        }
    }

    [TestMethod]
    public void OffsetAtEnd_GetLineByOffset_BeyondTextLength_Throws()
    {
        foreach (ITextSnapshot source in CreateSources("text"))
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.GetLineByOffset(source.TextLength + 1));
        }
    }
}

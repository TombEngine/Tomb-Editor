using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.ClassicScript.Services;

/// <summary>
/// Direct service tests for <see cref="ClassicScriptLineService"/> using <see cref="StringTextSnapshot"/>.
/// </summary>
[TestClass]
public class ClassicScriptLineServiceTests
{
    private readonly IClassicScriptLineService _lineService = new ClassicScriptLineService();

    // ------------------------------------------------------------------
    // GetWordAtOffset
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetWordAtOffset_AtStartOfCommand_ReturnsCommandKey()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("Legend", _lineService.GetWordAtOffset(source, 0));
    }

    [TestMethod]
    public void GetWordAtOffset_AfterEquals_ReturnsFirstArgument()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("42", _lineService.GetWordAtOffset(source, 8));
    }

    [TestMethod]
    public void GetWordAtOffset_AtCommaBoundary_ReturnsSecondArgument()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5");

        Assert.AreEqual("5", _lineService.GetWordAtOffset(source, 22));
    }

    [TestMethod]
    public void GetWordAtOffset_AtWhitespaceOnly_ReturnsEmptyString()
    {
        var source = new StringTextSnapshot("   ");

        Assert.AreEqual(string.Empty, _lineService.GetWordAtOffset(source, 1));
    }

    [TestMethod]
    public void GetWordAtOffset_PastEndOfDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.IsNull(_lineService.GetWordAtOffset(source, 999));
    }

    [TestMethod]
    public void GetWordAtOffset_AtEndOfLine_ReturnsLastWord()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("42", _lineService.GetWordAtOffset(source, source.TextLength));
    }

    [TestMethod]
    public void GetWordAtOffset_EmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsNull(_lineService.GetWordAtOffset(source, 0));
    }

    [TestMethod]
    public void GetWordAtOffset_MnemonicConstant_ReturnsFullConstant()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5");

        Assert.AreEqual("CUST_BAR", _lineService.GetWordAtOffset(source, 12));
    }

    [TestMethod]
    public void GetWordAtOffset_HexValue_ReturnsHexValue()
    {
        var source = new StringTextSnapshot("Legend= $1A2F");

        Assert.AreEqual("$1A2F", _lineService.GetWordAtOffset(source, 8));
    }

    [TestMethod]
    public void GetWordAtOffset_SemicolonInQuotedString_IsTreatedAsDelimiter()
    {
        var source = new StringTextSnapshot("Legend= \"hello;world\"");

        Assert.AreEqual("\"hello", _lineService.GetWordAtOffset(source, 9));
    }

    // ------------------------------------------------------------------
    // GetWordTypeAtOffset
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetWordTypeAtOffset_Command_ReturnsCommand()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(WordType.Command, _lineService.GetWordTypeAtOffset(source, 0));
    }

    [TestMethod]
    public void GetWordTypeAtOffset_Header_ReturnsHeader()
    {
        var source = new StringTextSnapshot("[Level]");

        Assert.AreEqual(WordType.Header, _lineService.GetWordTypeAtOffset(source, 1));
    }

    [TestMethod]
    public void GetWordTypeAtOffset_MnemonicConstant_ReturnsMnemonicConstant()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5");

        Assert.AreEqual(WordType.MnemonicConstant, _lineService.GetWordTypeAtOffset(source, 12));
    }

    [TestMethod]
    public void GetWordTypeAtOffset_Hexadecimal_ReturnsHexadecimal()
    {
        var source = new StringTextSnapshot("Legend= $1A2F");

        Assert.AreEqual(WordType.Hexadecimal, _lineService.GetWordTypeAtOffset(source, 8));
    }

    [TestMethod]
    public void GetWordTypeAtOffset_PastEnd_ReturnsUnknown()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(WordType.Unknown, _lineService.GetWordTypeAtOffset(source, 999));
    }

    [TestMethod]
    public void GetWordTypeAtOffset_UnknownText_ReturnsUnknown()
    {
        var source = new StringTextSnapshot("just some text");

        Assert.AreEqual(WordType.Unknown, _lineService.GetWordTypeAtOffset(source, 0));
    }

    // ------------------------------------------------------------------
    // IsSectionHeaderLine
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsSectionHeaderLine_ValidHeader_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsSectionHeaderLine("[Level]"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_HeaderWithComment_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsSectionHeaderLine("[Options] ; section comment"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_Malformed_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsSectionHeaderLine("[Level"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_PlainText_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsSectionHeaderLine("Legend= 42"));
    }

    // ------------------------------------------------------------------
    // GetSectionHeaderText
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetSectionHeaderText_SimpleHeader_ReturnsName()
    {
        Assert.AreEqual("Level", _lineService.GetSectionHeaderText("[Level]"));
    }

    [TestMethod]
    public void GetSectionHeaderText_HeaderWithComment_ReturnsName()
    {
        Assert.AreEqual("Options", _lineService.GetSectionHeaderText("[Options] ; comment"));
    }

    [TestMethod]
    public void GetSectionHeaderText_NonHeader_ReturnsNull()
    {
        Assert.IsNull(_lineService.GetSectionHeaderText("Legend= 42"));
    }

    // ------------------------------------------------------------------
    // IsEmptyOrComments
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsEmptyOrComments_Whitespace_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("   "));
    }

    [TestMethod]
    public void IsEmptyOrComments_CommentLine_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("; comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_CommentWithLeadingWhitespace_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("   ; comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_ContentLine_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsEmptyOrComments("Legend= 42"));
    }

    [TestMethod]
    public void IsEmptyOrComments_EmptyString_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments(string.Empty));
    }

    [TestMethod]
    public void IsEmptyOrComments_Null_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments(null!));
    }

    [TestMethod]
    public void IsEmptyOrComments_SectionHeader_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsEmptyOrComments("[Level]"));
    }

    // ------------------------------------------------------------------
    // IsValidIncludeLine
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsValidIncludeLine_ValidInclude_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsValidIncludeLine("#include \"path\\\\to\\\\file.txt\""));
    }

    [TestMethod]
    public void IsValidIncludeLine_MissingQuotes_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsValidIncludeLine("#include path"));
    }

    [TestMethod]
    public void IsValidIncludeLine_LeadingWhitespace_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsValidIncludeLine("  #include \"file.txt\""));
    }

    [TestMethod]
    public void IsValidIncludeLine_WrongCasing_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsValidIncludeLine("#INCLUDE \"file.txt\""));
    }

    [TestMethod]
    public void IsValidIncludeLine_EmptyString_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsValidIncludeLine(string.Empty));
    }

    // ------------------------------------------------------------------
    // IsStandardStringSectionName / IsExtraNGSectionName
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsStandardStringSectionName_Strings_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsStandardStringSectionName("Strings"));
    }

    [TestMethod]
    public void IsStandardStringSectionName_PCStrings_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsStandardStringSectionName("PCStrings"));
    }

    [TestMethod]
    public void IsStandardStringSectionName_PSXStrings_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsStandardStringSectionName("PSXStrings"));
    }

    [TestMethod]
    public void IsStandardStringSectionName_Null_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsStandardStringSectionName(null));
    }

    [TestMethod]
    public void IsStandardStringSectionName_Level_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsStandardStringSectionName("Level"));
    }

    [TestMethod]
    public void IsExtraNGSectionName_ExtraNG_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsExtraNGSectionName("ExtraNG"));
    }

    [TestMethod]
    public void IsExtraNGSectionName_CaseInsensitive_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsExtraNGSectionName("extrang"));
    }

    [TestMethod]
    public void IsExtraNGSectionName_Null_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsExtraNGSectionName(null));
    }

    [TestMethod]
    public void IsExtraNGSectionName_Level_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsExtraNGSectionName("Level"));
    }

    // ------------------------------------------------------------------
    // RemoveComments
    // ------------------------------------------------------------------

    [TestMethod]
    public void RemoveComments_RemovesCommentAndPrecedingWhitespace()
    {
        Assert.AreEqual("Legend= 42", _lineService.RemoveComments("Legend= 42 ; comment"));
    }

    [TestMethod]
    public void RemoveComments_NoComment_ReturnsOriginal()
    {
        Assert.AreEqual("Legend= 42", _lineService.RemoveComments("Legend= 42"));
    }

    [TestMethod]
    public void RemoveComments_CommentOnlyLine_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, _lineService.RemoveComments("; just a comment"));
    }

    [TestMethod]
    public void RemoveComments_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, _lineService.RemoveComments(string.Empty));
    }

    // ------------------------------------------------------------------
    // EscapeComments
    // ------------------------------------------------------------------

    [TestMethod]
    public void EscapeComments_ReplacesCommentWithSpaces_PreservesLength()
    {
        string result = _lineService.EscapeComments("Legend= 42 ; comment");

        Assert.AreEqual("Legend= 42          ", result);
        Assert.AreEqual(20, result.Length);
    }

    [TestMethod]
    public void EscapeComments_NoComment_ReturnsOriginal()
    {
        Assert.AreEqual("Legend= 42", _lineService.EscapeComments("Legend= 42"));
    }

    [TestMethod]
    public void EscapeComments_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, _lineService.EscapeComments(string.Empty));
    }

    // ------------------------------------------------------------------
    // EscapeCommentsAndNewLines
    // ------------------------------------------------------------------

    [TestMethod]
    public void EscapeCommentsAndNewLines_ReplacesContinuationMarkerAndNewlines()
    {
        string result = _lineService.EscapeCommentsAndNewLines("Legend= 42 >\n  ; comment");

        Assert.IsFalse(result.Contains('>'));
        Assert.IsFalse(result.Contains('\n'));
        Assert.IsFalse(result.Contains(';'));
    }

    // ------------------------------------------------------------------
    // RemoveNGStringIndex
    // ------------------------------------------------------------------

    [TestMethod]
    public void RemoveNGStringIndex_RemovesNumberedPrefix()
    {
        Assert.AreEqual("some text", _lineService.RemoveNGStringIndex("42: some text"));
    }

    [TestMethod]
    public void RemoveNGStringIndex_NoPrefix_ReturnsOriginal()
    {
        Assert.AreEqual("some text", _lineService.RemoveNGStringIndex("some text"));
    }

    [TestMethod]
    public void RemoveNGStringIndex_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, _lineService.RemoveNGStringIndex(string.Empty));
    }
}

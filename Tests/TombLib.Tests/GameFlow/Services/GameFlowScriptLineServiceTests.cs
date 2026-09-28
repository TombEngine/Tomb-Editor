using TombLib.Scripting.GameFlowScript.Services;

namespace TombLib.Tests.GameFlow.Services;

[TestClass]
public class GameFlowScriptLineServiceTests
{
    private readonly IGameFlowScriptLineService _lineService = new GameFlowScriptLineService();

    // ---- RemoveComments ----

    [TestMethod]
    public void RemoveComments_EmptyInput_ReturnsEmptyString()
    {
        string result = _lineService.RemoveComments(string.Empty);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveComments_NoComment_ReturnsOriginalText()
    {
        string result = _lineService.RemoveComments("LEVEL: Caves");

        Assert.AreEqual("LEVEL: Caves", result);
    }

    [TestMethod]
    public void RemoveComments_CommentOnlyLine_ReturnsEmptyString()
    {
        string result = _lineService.RemoveComments("// This is a comment");

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveComments_CommentAfterCode_RemovesCommentAndLeadingWhitespace()
    {
        string result = _lineService.RemoveComments("LEVEL: Caves // level comment");

        Assert.AreEqual("LEVEL: Caves", result);
    }

    [TestMethod]
    public void RemoveComments_LeadingWhitespaceComment_RemovesAll()
    {
        string result = _lineService.RemoveComments("    // indented comment");

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveComments_MultilineInput_RemovesCommentsPerLine()
    {
        string input = "TITLE:\r\nLEVEL: Caves // first level\r\nEND: // end comment\r\n";
        string result = _lineService.RemoveComments(input);

        // Legacy quirk: \r before \n is consumed by the .*$ regex, producing LF-only lines.
        StringAssert.Contains(result, "TITLE:");
        StringAssert.Contains(result, "LEVEL: Caves");
        StringAssert.Contains(result, "END:");
        Assert.IsFalse(result.Contains("//"));
    }

    // ---- EscapeComments ----

    [TestMethod]
    public void EscapeComments_EmptyInput_ReturnsEmptyString()
    {
        string result = _lineService.EscapeComments(string.Empty);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void EscapeComments_NoComment_ReturnsOriginalText()
    {
        string result = _lineService.EscapeComments("LEVEL: Caves");

        Assert.AreEqual("LEVEL: Caves", result);
    }

    [TestMethod]
    public void EscapeComments_CommentOnlyLine_PreservesLength()
    {
        string input = "// comment";
        string result = _lineService.EscapeComments(input);

        Assert.AreEqual(input.Length, result.Length);

        for (int i = 0; i < result.Length; i++)
            Assert.AreEqual(' ', result[i]);
    }

    [TestMethod]
    public void EscapeComments_CommentAfterCode_PreservesLength()
    {
        string input = "LEVEL: Caves // comment";
        string result = _lineService.EscapeComments(input);

        Assert.AreEqual(input.Length, result.Length);
        Assert.AreEqual("LEVEL: Caves           ", result);
    }

    // ---- IsEmptyOrComments ----

    [TestMethod]
    public void IsEmptyOrComments_NullInput_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments(null!));
    }

    [TestMethod]
    public void IsEmptyOrComments_EmptyString_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments(string.Empty));
    }

    [TestMethod]
    public void IsEmptyOrComments_WhitespaceOnly_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("   \t  "));
    }

    [TestMethod]
    public void IsEmptyOrComments_CommentOnlyLine_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("// This is a comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_CommentWithLeadingWhitespace_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsEmptyOrComments("  // indented comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_CodePlusComment_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsEmptyOrComments("LEVEL: Caves // comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_CodeOnly_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsEmptyOrComments("LEVEL: Caves"));
    }

    // ---- IsSectionHeaderLine ----

    [TestMethod]
    public void IsSectionHeaderLine_KnownSection_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsSectionHeaderLine("TITLE:"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_UnknownKeyword_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsSectionHeaderLine("ARBITRARY: value"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_NoColon_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsSectionHeaderLine("TITLE"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_LeadingWhitespace_ReturnsFalse()
    {
        // Legacy quirk: Patterns.Sections ^ anchor prevents leading whitespace.
        Assert.IsFalse(_lineService.IsSectionHeaderLine("  TITLE:"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_CaseInsensitive_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsSectionHeaderLine("title:"));
    }

    [TestMethod]
    public void IsSectionHeaderLine_TrailingComment_ReturnsTrue()
    {
        Assert.IsTrue(_lineService.IsSectionHeaderLine("TITLE: // section comment"));
    }

    // ---- GetSectionHeaderText ----

    [TestMethod]
    public void GetSectionHeaderText_KnownSection_ReturnsSectionKeyword()
    {
        string? result = _lineService.GetSectionHeaderText("TITLE:");

        Assert.AreEqual("TITLE", result);
    }

    [TestMethod]
    public void GetSectionHeaderText_UnknownKeyword_ReturnsNull()
    {
        string? result = _lineService.GetSectionHeaderText("ARBITRARY: value");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetSectionHeaderText_LeadingWhitespace_ReturnsNull()
    {
        // Legacy quirk: Patterns.Sections ^ anchor prevents leading whitespace.
        string? result = _lineService.GetSectionHeaderText("  TITLE:");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetSectionHeaderText_CasePreserved_ReturnsOriginalCase()
    {
        string? result = _lineService.GetSectionHeaderText("title:");

        Assert.AreEqual("title", result);
    }
}

#nullable enable

using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX.Services;

[TestClass]
public class TRXLineServiceTests
{
    private readonly ITRXLineService _lineService = new TRXLineService();

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
        string result = _lineService.RemoveComments("\"title\": \"Caves\",");

        Assert.AreEqual("\"title\": \"Caves\",", result);
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
        string result = _lineService.RemoveComments("\"title\": \"Caves\", // level title");

        Assert.AreEqual("\"title\": \"Caves\",", result);
    }

    [TestMethod]
    public void RemoveComments_LeadingWhitespaceComment_RemovesAll()
    {
        string result = _lineService.RemoveComments("    // indented comment");

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveComments_CRLFMultiline_ConsumesCR()
    {
        string input = "\"title\": \"A\",\r\n\"title\": \"B\", // second\r\n\"title\": \"C\",\r\n";
        string result = _lineService.RemoveComments(input);

        StringAssert.Contains(result, "\"title\": \"A\",");
        StringAssert.Contains(result, "\"title\": \"B\",");
        StringAssert.Contains(result, "\"title\": \"C\",");
        Assert.IsFalse(result.Contains("//"));
    }

    [TestMethod]
    public void RemoveComments_SlashSlashInsideQuotedString_PreservesUrl()
    {
        // The // inside a double-quoted string is not a comment in the JSON-like TRX format.
        string result = _lineService.RemoveComments("\"url\": \"http://example.com\",");

        Assert.AreEqual("\"url\": \"http://example.com\",", result);
    }

    [TestMethod]
    public void RemoveComments_CommentAfterQuotedUrl_RemovesComment()
    {
        string result = _lineService.RemoveComments("\"url\": \"http://example.com\", // the site");

        Assert.AreEqual("\"url\": \"http://example.com\",", result);
    }

    [TestMethod]
    public void RemoveComments_QuotedSlashSlashThenComment_RemovesOnlyComment()
    {
        string result = _lineService.RemoveComments("\"path\": \"a//b\", \"title\": \"Caves\" // level");

        Assert.AreEqual("\"path\": \"a//b\", \"title\": \"Caves\"", result);
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
        string result = _lineService.EscapeComments("\"title\": \"Caves\",");

        Assert.AreEqual("\"title\": \"Caves\",", result);
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
        string input = "\"title\": \"Caves\", // comment";
        string result = _lineService.EscapeComments(input);

        Assert.AreEqual(input.Length, result.Length);
        Assert.AreEqual("\"title\": \"Caves\",           ", result);
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
        Assert.IsFalse(_lineService.IsEmptyOrComments("\"title\": \"Caves\", // comment"));
    }

    [TestMethod]
    public void IsEmptyOrComments_CodeOnly_ReturnsFalse()
    {
        Assert.IsFalse(_lineService.IsEmptyOrComments("\"title\": \"Caves\","));
    }
}

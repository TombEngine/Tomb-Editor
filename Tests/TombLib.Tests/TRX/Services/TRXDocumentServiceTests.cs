#nullable enable

using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX.Services;

[TestClass]
public class TRXDocumentServiceTests
{
    private readonly ITRXLineService _lineService = new TRXLineService();
    private readonly ITRXDocumentService _documentService;

    public TRXDocumentServiceTests()
    {
        _documentService = new TRXDocumentService(_lineService);
    }

    // ---- IsLevelScriptDefined ----

    [TestMethod]
    public void IsLevelScriptDefined_ExactMatch_ReturnsTrue()
    {
        var source = new StringTextSnapshot(
            "{\r\n" +
            "  \"levels\": [\r\n" +
            "    {\r\n" +
            "      \"title\": \"Vatican City\",\r\n" +
            "    }\r\n" +
            "  ]\r\n" +
            "}\r\n");

        bool result = _documentService.IsLevelScriptDefined(source, "Vatican City");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_NoMatch_ReturnsFalse()
    {
        var source = new StringTextSnapshot(
            "{\r\n" +
            "  \"levels\": [\r\n" +
            "    {\r\n" +
            "      \"title\": \"Vatican City\",\r\n" +
            "    }\r\n" +
            "  ]\r\n" +
            "}\r\n");

        bool result = _documentService.IsLevelScriptDefined(source, "Colosseum");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_EmptyDocument_ReturnsFalse()
    {
        var source = new StringTextSnapshot(string.Empty);

        bool result = _documentService.IsLevelScriptDefined(source, "Caves");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_TitleWithTrailingComma_NormalizesCorrectly()
    {
        var source = new StringTextSnapshot("\"title\": \"Caves\",");

        bool result = _documentService.IsLevelScriptDefined(source, "Caves");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_TitleWithTrailingComment_StillMatches()
    {
        var source = new StringTextSnapshot("\"title\": \"Caves\", // first level");

        bool result = _documentService.IsLevelScriptDefined(source, "Caves");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_TitleCaseInsensitiveRegex_ButExactNameComparison()
    {
        var source = new StringTextSnapshot("\"TITLE\": \"CAVES\",");

        bool result = _documentService.IsLevelScriptDefined(source, "Caves");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLevelScriptDefined_LeadingWhitespaceBeforeProperty_StillMatches()
    {
        var source = new StringTextSnapshot("  \"title\": \"Caves\",");

        bool result = _documentService.IsLevelScriptDefined(source, "Caves");

        Assert.IsTrue(result);
    }

    // ---- FindDocumentLineOfLevel ----

    [TestMethod]
    public void FindDocumentLineOfLevel_ExactTitleMatch_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot(
            "{\r\n" +
            "  \"levels\": [\r\n" +
            "    {\r\n" +
            "      \"title\": \"Vatican City\",\r\n" +
            "    }\r\n" +
            "  ]\r\n" +
            "}\r\n");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Vatican City");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(4, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_NoMatch_ReturnsNull()
    {
        var source = new StringTextSnapshot(
            "{\r\n" +
            "  \"levels\": [\r\n" +
            "    {\r\n" +
            "      \"title\": \"Vatican City\",\r\n" +
            "    }\r\n" +
            "  ]\r\n" +
            "}\r\n");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Colosseum");

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_StartsWithMatch_FindsPartialPrefix()
    {
        var source = new StringTextSnapshot("\"title\": \"CavesOfDoom\",");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_EmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_CommentNameMatch_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("// Level 1: Caves");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_CommentNameWithoutLevelPrefix_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("// 1: Caves");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_CommentNameWithDot_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("// Level 1. Caves");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_CommentNameDoesNotMatch_ReturnsNull()
    {
        var source = new StringTextSnapshot("// Level 1: Caves");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Colosseum");

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_TitleMatchTakesPriorityOverCommentName()
    {
        var source = new StringTextSnapshot(
            "\"title\": \"First\",\r\n" +
            "// Level 1: Caves\r\n");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "First");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);

        lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(2, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_TitleWithCRLF_Works()
    {
        var source = new StringTextSnapshot("\"title\": \"Caves\",\r\n\"title\": \"Venice\",\r\n");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Venice");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(2, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_TitleWithTrailingComma_NormalizesCorrectly()
    {
        var source = new StringTextSnapshot("\"title\": \"Caves\",");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfLevel_TitleWithTrailingComment_StillMatchesThroughNormalization()
    {
        var source = new StringTextSnapshot("\"title\": \"Caves\", // comment");

        int? lineNumber = _documentService.FindDocumentLineOfLevel(source, "Caves");

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }
}

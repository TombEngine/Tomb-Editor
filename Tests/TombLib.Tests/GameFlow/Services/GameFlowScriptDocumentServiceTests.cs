using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.GameFlow.Services;

[TestClass]
public class GameFlowScriptDocumentServiceTests
{
    private readonly IGameFlowScriptLineService _lineService = new GameFlowScriptLineService();
    private readonly IGameFlowScriptDocumentService _documentService;

    public GameFlowScriptDocumentServiceTests()
    {
        _documentService = new GameFlowScriptDocumentService(_lineService);
    }

    // ---- IsLevelScriptDefined ----

    [TestMethod]
    public void IsLevelScriptDefined_ExactMatch_ReturnsTrue()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\r\n");

        Assert.IsTrue(_documentService.IsLevelScriptDefined(source, "Caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_NoMatch_ReturnsFalse()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\r\n");

        Assert.IsFalse(_documentService.IsLevelScriptDefined(source, "Venice"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_LevelWithTrailingComment_StillMatches()
    {
        var source = new StringTextSnapshot("LEVEL: Caves // first level\r\n");

        Assert.IsTrue(_documentService.IsLevelScriptDefined(source, "Caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_LevelWithLeadingWhitespace_ReturnsFalse()
    {
        // Legacy quirk: ^ anchor in Patterns.LevelProperty prevents leading whitespace.
        var source = new StringTextSnapshot("  LEVEL: Caves\r\n");

        Assert.IsFalse(_documentService.IsLevelScriptDefined(source, "Caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_EmptyDocument_ReturnsFalse()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsFalse(_documentService.IsLevelScriptDefined(source, "Caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_PartialNameMatch_ReturnsFalse()
    {
        var source = new StringTextSnapshot("LEVEL: CavesPart2\r\n");

        Assert.IsFalse(_documentService.IsLevelScriptDefined(source, "Caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_CaseSensitiveName_RequiresExactCase()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\r\n");

        Assert.IsFalse(_documentService.IsLevelScriptDefined(source, "caves"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_LFOnlyNewlines_WorksWithLF()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\nLEVEL: Venice\n");

        Assert.IsTrue(_documentService.IsLevelScriptDefined(source, "Venice"));
    }

    // ---- FindDocumentLineOfObject (Section) ----

    [TestMethod]
    public void FindDocumentLineOfObject_SectionExactMatch_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("TITLE:\nLEVEL: Caves\nEND:\n");
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "TITLE:", ObjectType.Section);

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_SectionNotFound_ReturnsNull()
    {
        var source = new StringTextSnapshot("TITLE:\nLEVEL: Caves\nEND:\n");
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "OPTIONS:", ObjectType.Section);

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_SectionLeadingWhitespace_DoesNotMatch()
    {
        var source = new StringTextSnapshot("  TITLE:\nLEVEL: Caves\n");

        // Legacy behavior: StartsWith is case-sensitive and does not trim.
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "TITLE:", ObjectType.Section);

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_SectionCaseSensitive_RequiresExactCase()
    {
        var source = new StringTextSnapshot("TITLE:\nLEVEL: Caves\n");

        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "title:", ObjectType.Section);

        Assert.IsNull(lineNumber);
    }

    // ---- FindDocumentLineOfObject (Level) ----

    [TestMethod]
    public void FindDocumentLineOfObject_LevelExactMatch_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\n");
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Caves", ObjectType.Level);

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_LevelNotFound_ReturnsNull()
    {
        var source = new StringTextSnapshot("LEVEL: Caves\n");
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Venice", ObjectType.Level);

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_LevelWithLeadingWhitespace_ReturnsNull()
    {
        var source = new StringTextSnapshot("  LEVEL: Caves\n");

        // Patterns.LevelProperty ^ anchor prevents matching leading whitespace.
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Caves", ObjectType.Level);

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_LevelSubstringMatch_FindsPartial()
    {
        var source = new StringTextSnapshot("LEVEL: CavesMore\n");

        // Legacy quirk: StartsWith allows partial prefix match.
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Caves", ObjectType.Level);

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(1, lineNumber!.Value);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_LevelEmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Caves", ObjectType.Level);

        Assert.IsNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_LevelCRLFNewlines_HandlesCRLF()
    {
        var source = new StringTextSnapshot("LEVEL: First\r\nLEVEL: Second\r\n");
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, "Second", ObjectType.Level);

        Assert.IsNotNull(lineNumber);
        Assert.AreEqual(2, lineNumber!.Value);
    }
}

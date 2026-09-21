using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.ClassicScript.Services;

/// <summary>
/// Direct service tests for <see cref="ClassicScriptIndexService"/> using <see cref="StringTextSnapshot"/>.
/// </summary>
[TestClass]
public class ClassicScriptIndexServiceTests
{
    private readonly IClassicScriptIndexService _indexService;

    public ClassicScriptIndexServiceTests()
    {
        var lineService = new ClassicScriptLineService();
        var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
        var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
        var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

        _indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
    }

    // ------------------------------------------------------------------
    // Unsupported command
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_UnsupportedCommand_ReturnsNegativeOne()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(-1, _indexService.GetNextFreeIndex(source, 0));
    }

    [TestMethod]
    public void GetNextFreeIndex_NullCommandKey_ReturnsNegativeOne()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(-1, _indexService.GetNextFreeIndex(source, 0, null!));
    }

    [TestMethod]
    public void GetNextFreeIndex_EmptyCommandKey_ReturnsNegativeOne()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(-1, _indexService.GetNextFreeIndex(source, 0, string.Empty));
    }

    // ------------------------------------------------------------------
    // TriggerGroup
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_TriggerGroup_NoExistingEntries_ReturnsTwo()
    {
        var source = new StringTextSnapshot("[Level]\nTriggerGroup= 1, $2000, 80, $FF\n; no other trigger groups");

        // The second TriggerGroup would get index 2.
        Assert.AreEqual(2, _indexService.GetNextFreeIndex(source, 12));
    }

    [TestMethod]
    public void GetNextFreeIndex_TriggerGroup_WithExistingEntries_ReturnsNextFree()
    {
        var source = new StringTextSnapshot(
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\nTriggerGroup= 2, $2000, 80, $FF\nTriggerGroup= 3, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(4, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_TriggerGroup_WithGap_ReturnsFirstGap()
    {
        var source = new StringTextSnapshot(
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\nTriggerGroup= 3, $2000, 80, $FF\n; gap at 2");

        int tgLineOffset = source.GetLineByNumber(3).Offset;

        Assert.AreEqual(2, _indexService.GetNextFreeIndex(source, tgLineOffset + 1));
    }

    // ------------------------------------------------------------------
    // GlobalTrigger
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_GlobalTrigger_NoExistingEntries_ReturnsTwo()
    {
        var source = new StringTextSnapshot("[Level]\nGlobalTrigger= 1, ...");

        Assert.AreEqual(2, _indexService.GetNextFreeIndex(source, 15));
    }

    // ------------------------------------------------------------------
    // Organizer
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_Organizer_NoExistingEntries_ReturnsTwo()
    {
        var source = new StringTextSnapshot("[Level]\nOrganizer= 1, ...");

        Assert.AreEqual(2, _indexService.GetNextFreeIndex(source, 15));
    }

    // ------------------------------------------------------------------
    // #FIRST_ID
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_WithFirstId_StartsAtFirstId()
    {
        var source = new StringTextSnapshot(
            "[Level]\n#FIRST_ID TriggerGroup= 10\nTriggerGroup= 10, $2000, 80, $FF\nTriggerGroup= 11, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(12, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdWithExpression_EvaluatesExpression()
    {
        var source = new StringTextSnapshot(
            "[Level]\n#FIRST_ID TriggerGroup= 100+5\nTriggerGroup= 105, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(106, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdBelowOne_ReturnsOne()
    {
        var source = new StringTextSnapshot(
            "[Level]\n#FIRST_ID TriggerGroup= 0\nTriggerGroup= 0, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(1, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    // ------------------------------------------------------------------
    // #DEFINE variable resolution
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_FirstIdWithDefineVariable_ResolvesVariable()
    {
        var source = new StringTextSnapshot(
            "#define BASE_ID 100\n[Level]\n#FIRST_ID TriggerGroup= BASE_ID+5\nTriggerGroup= 105, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(106, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdWithNestedDefine_ResolvesNested()
    {
        var source = new StringTextSnapshot(
            "#define A 10\n#define B A+20\n[Level]\n#FIRST_ID TriggerGroup= B+5\nTriggerGroup= 35, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(36, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdWithUndefinedVariable_UsesZero()
    {
        var source = new StringTextSnapshot(
            "[Level]\n#FIRST_ID TriggerGroup= UNKNOWN_ID+1\nTriggerGroup= 1, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(2, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    [TestMethod]
    public void GetNextFreeIndex_CyclicDefine_DoesNotOverflow()
    {
        var source = new StringTextSnapshot(
            "#define X X+1\n[Level]\n#FIRST_ID TriggerGroup= X\nTriggerGroup= 1, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;
        int result = _indexService.GetNextFreeIndex(source, lastLineOffset + 1);

        Assert.AreEqual(2, result);
    }

    // ------------------------------------------------------------------
    // Section-scoped
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_OnlyLooksInCurrentSection()
    {
        var source = new StringTextSnapshot(
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\n" +
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\nTriggerGroup= 2, $2000, 80, $FF");

        int secondSectionLineOffset = source.GetLineByNumber(source.LineCount).Offset;
        int result = _indexService.GetNextFreeIndex(source, secondSectionLineOffset + 1);

        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void GetNextFreeIndex_LaterSectionIndices_DoNotLeak()
    {
        // Indices are section-scoped: the second [Level] section reuses index 2, which must
        // not be treated as taken when computing the first section's next free index.
        var source = new StringTextSnapshot(
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\n" +
            "[Level]\nTriggerGroup= 2, $2000, 80, $FF");

        int firstSectionCommandOffset = source.GetLineByNumber(2).Offset;
        int result = _indexService.GetNextFreeIndex(source, firstSectionCommandOffset + 1);

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdInLaterSection_DoesNotLeak()
    {
        // #FIRST_ID is section-scoped (Scope: [Title] or [Level] sections); a #FIRST_ID in a
        // later section must not change the first section's free-index baseline.
        var source = new StringTextSnapshot(
            "[Level]\nTriggerGroup= 1, $2000, 80, $FF\n" +
            "[Level]\n#FIRST_ID TriggerGroup= 100\nTriggerGroup= 100, $2000, 80, $FF");

        int firstSectionCommandOffset = source.GetLineByNumber(2).Offset;
        int result = _indexService.GetNextFreeIndex(source, firstSectionCommandOffset + 1);

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void GetNextFreeIndex_FirstIdInLaterSection_DoesNotOverrideCurrentSection()
    {
        // When both the current and a later section declare #FIRST_ID, only the current
        // section's directive participates in the scan.
        var source = new StringTextSnapshot(
            "[Level]\n#FIRST_ID TriggerGroup= 10\nTriggerGroup= 10, $2000, 80, $FF\n" +
            "[Level]\n#FIRST_ID TriggerGroup= 100\nTriggerGroup= 100, $2000, 80, $FF");

        int firstSectionCommandOffset = source.GetLineByNumber(3).Offset;
        int result = _indexService.GetNextFreeIndex(source, firstSectionCommandOffset + 1);

        Assert.AreEqual(11, result);
    }

    [TestMethod]
    public void GetNextFreeIndex_NoSections_ScansEntireDocument()
    {
        var source = new StringTextSnapshot(
            "TriggerGroup= 1, $2000, 80, $FF\nTriggerGroup= 2, $2000, 80, $FF");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(3, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    // ------------------------------------------------------------------
    // Plugin
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_Plugin_ReturnsNextFreePluginIndex()
    {
        var source = new StringTextSnapshot(
            "[Options]\nPlugin= 1, my_plugin\nPlugin= 2, other_plugin");

        int lastLineOffset = source.GetLineByNumber(source.LineCount).Offset;

        Assert.AreEqual(3, _indexService.GetNextFreeIndex(source, lastLineOffset + 1));
    }

    // ------------------------------------------------------------------
    // Multiple #FIRST_ID lines (last wins)
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetNextFreeIndex_MultipleFirstId_LastOneWins()
    {
        var source = new StringTextSnapshot(
            "[Level]\n" +
            "#FIRST_ID TriggerGroup= 10\n" +
            "#FIRST_ID TriggerGroup= 50\n" +
            "TriggerGroup= 50, $2000, 80, $FF");

        Assert.AreEqual(51, _indexService.GetNextFreeIndex(source, 90));
    }
}

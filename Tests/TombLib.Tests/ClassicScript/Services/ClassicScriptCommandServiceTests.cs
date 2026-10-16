using System.IO;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Types;
using Nickelony.IDEKit.Core.Text;

namespace TombLib.Tests.ClassicScript.Services;

/// <summary>
/// Direct service tests for <see cref="ClassicScriptCommandService"/> using <see cref="StringTextSnapshot"/>.
/// </summary>
[TestClass]
public class ClassicScriptCommandServiceTests
{
    private readonly IClassicScriptCommandService _commandService;

    public ClassicScriptCommandServiceTests()
    {
        var lineService = new ClassicScriptLineService();
        var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
        var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();

        _commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
    }

    // ------------------------------------------------------------------
    // GetCommandKey
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetCommandKey_SimpleCommand_ReturnsCommandKey()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("Legend", _commandService.GetCommandKey(source, 0));
    }

    [TestMethod]
    public void GetCommandKey_LeadingWhitespace_ReturnsCommandKey()
    {
        var source = new StringTextSnapshot("   Legend= 42");

        Assert.AreEqual("Legend", _commandService.GetCommandKey(source, 5));
    }

    [TestMethod]
    public void GetCommandKey_Directive_ReturnsDirectiveWithHash()
    {
        var source = new StringTextSnapshot("#include \"file.txt\"");

        Assert.AreEqual("#include", _commandService.GetCommandKey(source, 2));
    }

    [TestMethod]
    public void GetCommandKey_DefineDirective_ReturnsDefineWithHash()
    {
        var source = new StringTextSnapshot("#define MY_CONSTANT 42");

        Assert.AreEqual("#define", _commandService.GetCommandKey(source, 2));
    }

    [TestMethod]
    public void GetCommandKey_FirstIdDirective_ReturnsFirstIdWithHash()
    {
        var source = new StringTextSnapshot("#FIRST_ID TriggerGroup= 1");

        Assert.AreEqual("#FIRST_ID", _commandService.GetCommandKey(source, 2));
    }

    [TestMethod]
    public void GetCommandKey_CommentLine_ReturnsNull()
    {
        var source = new StringTextSnapshot("; this is a comment");

        Assert.IsNull(_commandService.GetCommandKey(source, 3));
    }

    [TestMethod]
    public void GetCommandKey_SectionHeader_ReturnsNull()
    {
        var source = new StringTextSnapshot("[Level]");

        Assert.IsNull(_commandService.GetCommandKey(source, 2));
    }

    [TestMethod]
    public void GetCommandKey_LevelCommandInDefaultSection_ReturnsLevelLevel()
    {
        var source = new StringTextSnapshot("[Level]\nLevel= data\\tut1.tr4, 0");

        Assert.AreEqual("LevelLevel", _commandService.GetCommandKey(source, 10));
    }

    [TestMethod]
    public void GetCommandKey_CutCommandInDefaultSection_ReturnsNull()
    {
        var source = new StringTextSnapshot("[Level]\nCut= 1, 2");

        Assert.IsNull(_commandService.GetCommandKey(source, 10));
    }

    [TestMethod]
    public void GetCommandKey_ExtensionSectionVariations_ReturnExpectedKeys()
    {
        (string Section, string Command, string ExpectedKey)[] cases =
        [
            ("PCExtensions", "Level= data\\tut1.tr4, 0", "LevelPC"),
            ("PSXExtensions", "Level= data\\tut1.tr4, 0", "LevelPSX"),
            ("PCExtensions", "Cut= 1, 2", "CutPC"),
            ("PSXExtensions", "Cut= 1, 2", "CutPSX"),
            ("PCExtensions", "FMV= intro", "FMVPC"),
            ("PSXExtensions", "FMV= intro", "FMVPSX")
        ];

        foreach ((string section, string command, string expectedKey) in cases)
        {
            var source = new StringTextSnapshot($"[{section}]\n{command}");
            int commandOffset = source.GetLineByNumber(2).Offset;

            Assert.AreEqual(expectedKey, _commandService.GetCommandKey(source, commandOffset));
        }
    }

    [TestMethod]
    public void GetCommandKey_EmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsNull(_commandService.GetCommandKey(source, 0));
    }

    [TestMethod]
    public void GetCommandKey_CommentInLine_UsesEscapedText()
    {
        var source = new StringTextSnapshot("Legend= 42 ; some comment");

        Assert.AreEqual("Legend", _commandService.GetCommandKey(source, 0));
    }

    // ------------------------------------------------------------------
    // GetCommandStartLine
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetCommandStartLine_SimpleCommand_ReturnsSameLine()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(1, _commandService.GetCommandStartLine(source, 0));
    }

    [TestMethod]
    public void GetCommandStartLine_ContinuationLine_ReturnsFirstLine()
    {
        var source = new StringTextSnapshot("Legend= 42 >\n  , 43");

        Assert.AreEqual(1, _commandService.GetCommandStartLine(source, 17));
    }

    [TestMethod]
    public void GetCommandStartLine_SectionHeader_ReturnsNull()
    {
        var source = new StringTextSnapshot("[Level]");

        Assert.IsNull(_commandService.GetCommandStartLine(source, 2));
    }

    [TestMethod]
    public void GetCommandStartLine_OrphanedContinuation_ReturnsNull()
    {
        var source = new StringTextSnapshot("  , 43");

        Assert.IsNull(_commandService.GetCommandStartLine(source, 2));
    }

    [TestMethod]
    public void GetCommandStartLine_DirectiveLine_ReturnsSameLine()
    {
        var source = new StringTextSnapshot("#include \"file.txt\"");

        Assert.AreEqual(1, _commandService.GetCommandStartLine(source, 2));
    }

    // ------------------------------------------------------------------
    // GetWholeCommandLineText
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetWholeCommandLineText_SingleLine_ReturnsLineText()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("Legend= 42", _commandService.GetWholeCommandLineText(source, 0));
    }

    [TestMethod]
    public void GetWholeCommandLineText_MultiLine_ReturnsMergedText()
    {
        var source = new StringTextSnapshot("Legend= 42 >\n  , 43 >\n  , 44");
        string text = _commandService.GetWholeCommandLineText(source, 0);

        Assert.IsNotNull(text);
        Assert.IsTrue(text.Contains("42"));
        Assert.IsTrue(text.Contains("43"));
        Assert.IsTrue(text.Contains("44"));
        Assert.IsTrue(text.Contains("\n"));
    }

    [TestMethod]
    public void GetWholeCommandLineText_ContinuationMarkerWithComment_StillMerges()
    {
        var source = new StringTextSnapshot("Legend= 42 > ; comment\n  , 43");
        string text = _commandService.GetWholeCommandLineText(source, 0);

        Assert.IsNotNull(text);
        Assert.IsTrue(text.Contains("42"));
        Assert.IsTrue(text.Contains("43"));
    }

    [TestMethod]
    public void GetWholeCommandLineText_DuplicateContinuationMarker_UsesFinalMarker()
    {
        var source = new StringTextSnapshot("Legend= 42 >>\n  , 43");
        string text = _commandService.GetWholeCommandLineText(source, 0);

        Assert.IsNotNull(text);
        Assert.IsTrue(text.Contains(">>"));
    }

    // ------------------------------------------------------------------
    // GetCommandSyntax
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetCommandSyntax_LegendCommand_ReturnsSyntax()
    {
        var source = new StringTextSnapshot("Legend= 42");

        string? syntax = _commandService.GetCommandSyntax(source, 0);

        Assert.IsNotNull(syntax);
        Assert.IsTrue(syntax.Length > 0);
    }

    [TestMethod]
    public void GetCommandSyntax_UnknownCommand_ReturnsNull()
    {
        var source = new StringTextSnapshot("UnknownCmd= 42");

        string? syntax = _commandService.GetCommandSyntax(source, 0);

        Assert.IsNull(syntax);
    }

    // ------------------------------------------------------------------
    // GetArgumentIndexAtOffset
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetArgumentIndexAtOffset_FirstArgument_ReturnsZero()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(0, _commandService.GetArgumentIndexAtOffset(source, 8));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_SecondArgument_ReturnsOne()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5");

        Assert.AreEqual(1, _commandService.GetArgumentIndexAtOffset(source, 22));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_NoEquals_ReturnsNegativeOne()
    {
        var source = new StringTextSnapshot("just some text");

        Assert.AreEqual(-1, _commandService.GetArgumentIndexAtOffset(source, 0));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_EmptyDocument_ReturnsNegativeOne()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.AreEqual(-1, _commandService.GetArgumentIndexAtOffset(source, 0));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_ThirdArgument_ReturnsTwo()
    {
        var source = new StringTextSnapshot("Legend= 42, 43, 44");

        Assert.AreEqual(2, _commandService.GetArgumentIndexAtOffset(source, 16));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_MultiLineCommand_GetsCorrectIndex()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5 >\n  , 10");

        Assert.AreEqual(2, _commandService.GetArgumentIndexAtOffset(source, source.TextLength - 1));
    }

    [TestMethod]
    public void GetArgumentIndexAtOffset_CommentInLine_IgnoresComment()
    {
        var source = new StringTextSnapshot("Legend= 42 ; comment");

        Assert.AreEqual(0, _commandService.GetArgumentIndexAtOffset(source, 8));
    }

    // ------------------------------------------------------------------
    // GetArgumentFromIndex
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetArgumentFromIndex_ZeroIndex_ReturnsEntireLine()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual("Legend= 42", _commandService.GetArgumentFromIndex(source, 0, 0));
    }

    [TestMethod]
    public void GetArgumentFromIndex_OutOfRange_ThrowsIndexOutOfRangeException()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.ThrowsException<IndexOutOfRangeException>(() =>
            _commandService.GetArgumentFromIndex(source, 0, 5));
    }

    [TestMethod]
    public void GetArgumentFromIndex_MultiLine_ReturnsCorrectArgumentFromMergedText()
    {
        var source = new StringTextSnapshot("Customize= CUST_BAR, 5 >\n  , 10");
        string? arg = _commandService.GetArgumentFromIndex(source, 0, 2);

        Assert.IsNotNull(arg);
        Assert.IsTrue(arg.Contains("10"));
    }

    // ------------------------------------------------------------------
    // GetFlagPrefixOfCurrentArgument
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_NoFlagPrefix_ReturnsNull()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, 8));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_UndefinedCommand_ReturnsNull()
    {
        var source = new StringTextSnapshot("UnknownCmd= 42");

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, 12));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_EmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, 0));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_GenericCustomizeCommand_ReturnsMnemonicPrefix()
    {
        var source = new StringTextSnapshot("Customize= UNKNOWN, 5");

        Assert.AreEqual("CUST_", _commandService.GetFlagPrefixOfCurrentArgument(source, 12));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_FewerArgsThanSyntax_ReturnsPrefix()
    {
        // The command provides two arguments but the GlobalTrigger syntax defines seven
        // slots; the caret on the second argument must still resolve to the FGT_ prefix.
        var source = new StringTextSnapshot("GlobalTrigger= 1, 2");

        Assert.AreEqual("FGT_", _commandService.GetFlagPrefixOfCurrentArgument(source, 18));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_TooManyArguments_ReturnsNull()
    {
        // Customize defines two syntax slots; the caret on the third argument is past the
        // last slot and must return a deliberate no-result instead of an out-of-range access.
        var source = new StringTextSnapshot("Customize= UNKNOWN, 5, 6");

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, 23));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_ArrayArgument_ReturnsNull()
    {
        // The second Customize argument is an (*Array*) slot, which has no flag prefix.
        var source = new StringTextSnapshot("Customize= UNKNOWN, 5");

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, 19));
    }

    [TestMethod]
    public void GetFlagPrefixOfCurrentArgument_MalformedCommandLine_ReturnsNull()
    {
        // Empty/extra argument slots produce an argument index beyond the syntax definition;
        // the service must return null without throwing.
        var source = new StringTextSnapshot("Customize= 5, , ,");

        Assert.IsNull(_commandService.GetFlagPrefixOfCurrentArgument(source, source.TextLength - 1));
    }

    // ------------------------------------------------------------------
    // Include path resolution
    // ------------------------------------------------------------------

    [TestMethod]
    public void GetFullIncludePath_ValidIncludeWithSourceFileName_ReturnsCombinedPath()
    {
        var source = new StringTextSnapshot("#include \"scripts\\common.txt\"", "C:\\project\\level.txt");

        Assert.AreEqual(Path.Combine("C:\\project", "scripts\\common.txt"), _commandService.GetFullIncludePath(source, 0));
    }

    [TestMethod]
    public void GetFullIncludePath_NotAnIncludeLine_ReturnsNull()
    {
        var source = new StringTextSnapshot("Legend= 42", "C:\\project\\level.txt");

        Assert.IsNull(_commandService.GetFullIncludePath(source, 0));
    }

    // ------------------------------------------------------------------
    // Document-level queries
    // ------------------------------------------------------------------

    [TestMethod]
    public void DocumentContainsSections_HasSections_ReturnsTrue()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.IsTrue(_commandService.DocumentContainsSections(source));
    }

    [TestMethod]
    public void DocumentContainsSections_NoSections_ReturnsFalse()
    {
        var source = new StringTextSnapshot("Legend= 42\nCustomize= CUST_BAR, 5");

        Assert.IsFalse(_commandService.DocumentContainsSections(source));
    }

    [TestMethod]
    public void DocumentContainsSections_EmptyDocument_ReturnsFalse()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsFalse(_commandService.DocumentContainsSections(source));
    }

    [TestMethod]
    public void GetSectionsCount_TwoSections_ReturnsTwo()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n[Options]\nName= tut1");

        Assert.AreEqual(2, _commandService.GetSectionsCount(source));
    }

    [TestMethod]
    public void GetSectionsCount_NoSections_ReturnsZero()
    {
        var source = new StringTextSnapshot("Legend= 42");

        Assert.AreEqual(0, _commandService.GetSectionsCount(source));
    }

    [TestMethod]
    public void GetCurrentSectionName_InsideSection_ReturnsSectionName()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.AreEqual("Level", _commandService.GetCurrentSectionName(source, 10));
    }

    [TestMethod]
    public void GetCurrentSectionName_BeforeFirstSection_ReturnsNull()
    {
        var source = new StringTextSnapshot("Legend= 42\n[Level]\nName= tut1");

        Assert.IsNull(_commandService.GetCurrentSectionName(source, 0));
    }

    [TestMethod]
    public void GetCurrentSectionName_MultipleSections_ReturnsClosestPreviousSection()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n[Options]\nName= tut1");

        Assert.AreEqual("Options", _commandService.GetCurrentSectionName(source, 35));
    }

    [TestMethod]
    public void GetCurrentSectionName_OnSectionHeaderLine_ReturnsThatSection()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.AreEqual("Level", _commandService.GetCurrentSectionName(source, 1));
    }

    [TestMethod]
    public void GetCurrentSectionName_EmptyDocument_ReturnsNull()
    {
        var source = new StringTextSnapshot(string.Empty);

        Assert.IsNull(_commandService.GetCurrentSectionName(source, 0));
    }

    [TestMethod]
    public void GetStartLineOfCurrentSection_InsideSection_ReturnsSectionHeaderLine()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.AreEqual(1, _commandService.GetStartLineOfCurrentSection(source, 10));
    }

    [TestMethod]
    public void GetStartLineOfCurrentSection_BeforeFirstSection_ReturnsNull()
    {
        var source = new StringTextSnapshot("Legend= 42\n[Level]");

        Assert.IsNull(_commandService.GetStartLineOfCurrentSection(source, 0));
    }

    [TestMethod]
    public void GetLastLineOfCurrentSection_InsideSection_ReturnsLastNonEmptyLine()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\nName= tut1\n\n[Options]\n");

        int? lineNumber = _commandService.GetLastLineOfCurrentSection(source, 10);

        Assert.IsNotNull(lineNumber);
        ITextLine line = source.GetLineByNumber(lineNumber.Value);
        Assert.AreEqual("Name= tut1", source.GetText(line.Offset, line.Length));
    }

    [TestMethod]
    public void GetLastLineOfCurrentSection_LastSection_ReturnsLastNonEmptyLine()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n");

        int? lineNumber = _commandService.GetLastLineOfCurrentSection(source, 10);

        Assert.IsNotNull(lineNumber);
        ITextLine line = source.GetLineByNumber(lineNumber.Value);
        Assert.AreEqual("Legend= 42", source.GetText(line.Offset, line.Length));
    }

    [TestMethod]
    public void FindDocumentLineOfSection_ExistingSection_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n[Options]\nName= tut1");

        Assert.AreEqual(3, _commandService.FindDocumentLineOfSection(source, "Options"));
    }

    [TestMethod]
    public void FindDocumentLineOfSection_CaseInsensitive_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.AreEqual(1, _commandService.FindDocumentLineOfSection(source, "level"));
    }

    [TestMethod]
    public void FindDocumentLineOfSection_WithBracketsInName_StripsBrackets()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.IsNotNull(_commandService.FindDocumentLineOfSection(source, "[Level]"));
    }

    [TestMethod]
    public void FindDocumentLineOfSection_NotFound_ReturnsNull()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42");

        Assert.IsNull(_commandService.FindDocumentLineOfSection(source, "Nonexistent"));
    }

    // ------------------------------------------------------------------
    // IsLevelScriptDefined
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsLevelScriptDefined_ExistingLevel_ReturnsTrue()
    {
        var source = new StringTextSnapshot("[Level]\nName= tut1");

        Assert.IsTrue(_commandService.IsLevelScriptDefined(source, "tut1"));
    }

    [TestMethod]
    public void IsLevelScriptDefined_NonExistingLevel_ReturnsFalse()
    {
        var source = new StringTextSnapshot("[Level]\nName= tut1");

        Assert.IsFalse(_commandService.IsLevelScriptDefined(source, "nonexistent"));
    }

    // ------------------------------------------------------------------
    // IsLevelLanguageStringDefined
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsLevelLanguageStringDefined_ExistingString_ReturnsTrue()
    {
        var source = new StringTextSnapshot("[Strings]\ntut1");

        Assert.IsTrue(_commandService.IsLevelLanguageStringDefined(source, "tut1"));
    }

    [TestMethod]
    public void IsLevelLanguageStringDefined_WithNGIndex_ReturnsTrue()
    {
        var source = new StringTextSnapshot("[ExtraNG]\n42: tut1");

        Assert.IsTrue(_commandService.IsLevelLanguageStringDefined(source, "tut1"));
    }

    // ------------------------------------------------------------------
    // FindDocumentLineOfObject
    // ------------------------------------------------------------------

    [TestMethod]
    public void FindDocumentLineOfObject_Section_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n[Options]\nName= tut1");

        Assert.AreEqual(3, _commandService.FindDocumentLineOfObject(source, "[Options]", ObjectType.Section));
    }

    [TestMethod]
    public void FindDocumentLineOfObject_Level_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("[Level]\nName= tut1\nLegend= 42");

        int? lineNumber = _commandService.FindDocumentLineOfObject(source, "tut1", ObjectType.Level);

        Assert.IsNotNull(lineNumber);
    }

    [TestMethod]
    public void FindDocumentLineOfObject_Include_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("#include \"scripts\\common.txt\"\n[Level]");

        Assert.AreEqual(1, _commandService.FindDocumentLineOfObject(source, "scripts", ObjectType.Include));
    }

    [TestMethod]
    public void FindDocumentLineOfObject_Define_ReturnsLineNumber()
    {
        var source = new StringTextSnapshot("#define MY_CONSTANT 42\n[Level]");

        Assert.AreEqual(1, _commandService.FindDocumentLineOfObject(source, "MY_CONSTANT", ObjectType.Define));
    }

    // ------------------------------------------------------------------
    // Plugin lookup
    // ------------------------------------------------------------------

    [TestMethod]
    public void IsPluginDefined_ExistingPlugin_ReturnsTrue()
    {
        var source = new StringTextSnapshot("[Options]\nPlugin= 1, my_plugin\n[Level]\nName= tut1");

        Assert.IsTrue(_commandService.IsPluginDefined(source, "my_plugin"));
    }

    [TestMethod]
    public void IsPluginDefined_WithoutOptionsSection_ReturnsFalse()
    {
        var source = new StringTextSnapshot("[Level]\nName= tut1");

        Assert.IsFalse(_commandService.IsPluginDefined(source, "my_plugin"));
    }
}

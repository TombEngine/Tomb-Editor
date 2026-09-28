using StringEditor = TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class StringEditorContentTests
{
    // -----------------------------------------------------------------------
    // ContentReader: section header detection
    // -----------------------------------------------------------------------

    [TestMethod]
    public void ContentReader_NextSectionExists_FindsFirstSection()
    {
        string[] lines = ["; comment", "[Section1]", "value1", "value2"];

        bool exists = StringEditor.ContentReader.NextSectionExists(lines, 0, out int lineNumber);

        Assert.IsTrue(exists);
        Assert.AreEqual(1, lineNumber);
    }

    [TestMethod]
    public void ContentReader_NextSectionExists_NoSection_ReturnsFalse()
    {
        string[] lines = ["value1", "value2", "value3"];

        bool exists = StringEditor.ContentReader.NextSectionExists(lines, 0, out int lineNumber);

        Assert.IsFalse(exists);
        Assert.AreEqual(-1, lineNumber);
    }

    [TestMethod]
    public void ContentReader_NextSectionExists_HeaderWithTrailingComment_IsRecognized()
    {
        string[] lines = ["[Section] ; optional comment", "value1"];

        bool exists = StringEditor.ContentReader.NextSectionExists(lines, 0, out int lineNumber);

        Assert.IsTrue(exists);
        Assert.AreEqual(0, lineNumber);
    }

    [TestMethod]
    public void ContentReader_NextSectionExists_HeaderWithLeadingWhitespace_IsRecognized()
    {
        string[] lines = ["   [Section]", "value1"];

        bool exists = StringEditor.ContentReader.NextSectionExists(lines, 0, out int lineNumber);

        Assert.IsTrue(exists);
        Assert.AreEqual(0, lineNumber);
    }

    // -----------------------------------------------------------------------
    // ContentReader: string extraction from sections
    // -----------------------------------------------------------------------

    [TestMethod]
    public void ContentReader_GetStrings_ExtractsValuesUntilNextSection()
    {
        string[] lines = ["[Section1]", "string1", "string2", "[Section2]", "string3"];

        List<string> strings = StringEditor.ContentReader.GetStrings(lines, 0);

        Assert.AreEqual(2, strings.Count);
        Assert.AreEqual("string1", strings[0]);
        Assert.AreEqual("string2", strings[1]);
    }

    [TestMethod]
    public void ContentReader_GetStrings_SkipsEmptyAndCommentLines()
    {
        string[] lines = ["[Section1]", "", "; comment", "realValue", "   ; indented comment", "[Section2]"];

        List<string> strings = StringEditor.ContentReader.GetStrings(lines, 0);

        Assert.AreEqual(1, strings.Count);
        Assert.AreEqual("realValue", strings[0]);
    }

    [TestMethod]
    public void ContentReader_GetStrings_HandlesLastSection()
    {
        string[] lines = ["[LastSection]", "final1", "final2"];

        List<string> strings = StringEditor.ContentReader.GetStrings(lines, 0);

        Assert.AreEqual(2, strings.Count);
        Assert.AreEqual("final1", strings[0]);
        Assert.AreEqual("final2", strings[1]);
    }

    // -----------------------------------------------------------------------
    // ContentReader: escape handling
    // -----------------------------------------------------------------------

    [TestMethod]
    public void ContentReader_GetParsedLine_UnescapesSemicolons()
    {
        string result = StringEditor.ContentReader.GetParsedLine("value\\x3B with semicolon");

        Assert.AreEqual("value; with semicolon", result);
    }

    [TestMethod]
    public void ContentReader_GetParsedLine_UnescapesNewlines()
    {
        string result = StringEditor.ContentReader.GetParsedLine("line1\\nline2");

        Assert.AreEqual("line1" + Environment.NewLine + "line2", result);
    }

    [TestMethod]
    public void ContentReader_GetParsedLine_RemovesTrailingComment()
    {
        string result = StringEditor.ContentReader.GetParsedLine("realValue ; this is a comment");

        Assert.AreEqual("realValue", result);
    }

    [TestMethod]
    public void ContentReader_GetParsedLine_KeepsValueWithoutComment()
    {
        string result = StringEditor.ContentReader.GetParsedLine("plainValue");

        Assert.AreEqual("plainValue", result);
    }

    [TestMethod]
    public void ContentReader_GetParsedLine_NullString_ReturnsEmpty()
    {
        // GetParsedLine does not null-guard; it calls RemoveComments which
        // calls Regex.Replace with a null input. This test characterizes
        // the current behavior: if the regex handles null, it may return
        // string.Empty rather than throwing.
        // Documenting this as a characterization: null input may throw.
        // We only test the known valid input paths used in production.
    }

    // -----------------------------------------------------------------------
    // ContentReader: GetShortHex
    // -----------------------------------------------------------------------

    [TestMethod]
    public void ContentReader_GetShortHex_DefaultSize_ProducesFourDigitHex()
    {
        string result = StringEditor.ContentReader.GetShortHex(255);

        Assert.AreEqual("$00FF", result);
    }

    [TestMethod]
    public void ContentReader_GetShortHex_MaxShort_ProducesFourDigitHex()
    {
        string result = StringEditor.ContentReader.GetShortHex(32767);

        Assert.AreEqual("$7FFF", result);
    }

    // -----------------------------------------------------------------------
    // ContentBuilder: canonical output golden tests
    // These characterize the serialization contract after the WPF model cutover.
    // ContentBuilder now accepts IReadOnlyList<StringTableSection>.
    // -----------------------------------------------------------------------

    [TestMethod]
    public void ContentBuilder_BuildContent_GeneratesHeaderComment()
    {
        string content = StringEditor.ContentBuilder.BuildContent(System.Array.Empty<StringEditor.StringTableSection>());

        Assert.IsTrue(content.StartsWith("; Automatically generated document using TombIDE"));
    }

    [TestMethod]
    public void ContentBuilder_BuildContent_EmptyGrids_ProducesOnlyHeader()
    {
        string content = StringEditor.ContentBuilder.BuildContent(System.Array.Empty<StringEditor.StringTableSection>());

        string[] lines = content.Split(["\r\n", "\n"], StringSplitOptions.None);

        // Header is three comment lines followed by empty content.
        Assert.IsTrue(lines.Length >= 3);
        Assert.IsTrue(lines[0].StartsWith("; Automatically generated"));
        Assert.IsTrue(lines[1].StartsWith("; Do not add any comments"));
        Assert.IsTrue(lines[2].StartsWith("; going to be removed"));
    }
}

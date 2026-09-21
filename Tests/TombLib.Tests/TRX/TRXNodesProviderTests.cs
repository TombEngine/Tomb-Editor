using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using TombLib.Scripting.TRX.ContentNodes;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX;

/// <summary>
/// Tests for <see cref="TRXNodesProvider"/> level-name symbol extraction.
/// </summary>
[TestClass]
public class TRXNodesProviderTests
{
    private readonly ITRXLineService _lineService = new TRXLineService();

    private static IReadOnlyList<TextDocumentSymbol> GetSymbols(string content, string filter = "")
        => new TRXNodesProvider(new TRXLineService()).GetSymbols(new TextDocumentSymbolRequest(content, filter));

    // ---------------------------------------------------------------------------
    // Title properties
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetSymbols_TitleProperty_ReturnsLevelSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("\"title\": \"Caves\",\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Name);
    }

    [TestMethod]
    public void GetSymbols_TitlePropertyWithComment_ReturnsLevelSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("\"title\": \"Caves\", // level title\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Name);
    }

    [TestMethod]
    public void GetSymbols_TitlePropertyWithUrlValue_ReturnsFullValue()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("\"title\": \"http://example.com\",\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("http://example.com", nodes[0].Name);
    }

    [TestMethod]
    public void GetSymbols_EmptyTitleValue_ReturnsNoSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("\"title\": \"\",\n");

        Assert.AreEqual(0, nodes.Count);
    }

    [TestMethod]
    public void GetSymbols_MalformedTitleValue_ReturnsNoSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("\"title\": ,\n");

        Assert.AreEqual(0, nodes.Count);
    }

    // ---------------------------------------------------------------------------
    // // Level N: Name fallback lines
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetSymbols_LevelCommentLine_ReturnsFallbackSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("// Level 1: Caves\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Name);
    }

    [TestMethod]
    public void GetSymbols_LevelCommentWithDotSeparator_ReturnsFallbackSymbol()
    {
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("// Level 2. Venice\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Venice", nodes[0].Name);
    }

    [TestMethod]
    public void GetSymbols_CommentLineContainingTitle_StillUsesRawFallback()
    {
        // Malformed mixed input: the line is a comment that also contains a title property.
        // The fallback must run against the raw line text, not the comment-stripped text.
        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols("// Level 1: \"title\": \"Caves\"\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("\"title\": \"Caves\"", nodes[0].Name);
    }

    // ---------------------------------------------------------------------------
    // Filtering
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetSymbols_Filter_ReturnsOnlyMatchingSymbols()
    {
        const string content = "\"title\": \"Caves\",\n\"title\": \"Venice\",\n\"title\": \"City\",\n";

        IReadOnlyList<TextDocumentSymbol> nodes = GetSymbols(content, "ven");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Venice", nodes[0].Name);
    }
}

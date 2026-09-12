using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using TombLib.Scripting.GameFlowScript.ContentNodes;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Tests;

[TestClass]
public class GameFlowNodesProviderTests
{
    private readonly IGameFlowScriptLineService _lineService = new GameFlowScriptLineService();

    [TestMethod]
    public void GetSymbols_ReturnsSectionAndLevelGroupsForMatchingContent()
    {
        var provider = new GameFlowNodesProvider(_lineService);
        const string content = "TITLE:\r\nLEVEL: Caves // comment\r\nEND:\r\n";

        IReadOnlyList<TextDocumentSymbol> nodes = provider.GetSymbols(new TextDocumentSymbolRequest(content, string.Empty));

        Assert.AreEqual(2, nodes.Count);
        Assert.AreEqual("Sections", nodes[0].Name);
        Assert.AreEqual(1, nodes[0].Children.Count);
        Assert.AreEqual("TITLE", nodes[0].Children[0].Name);
        Assert.AreEqual(new GameFlowObjectDiscriminator(ObjectType.Section), nodes[0].Children[0].Data);

        Assert.AreEqual("Levels", nodes[1].Name);
        Assert.AreEqual(1, nodes[1].Children.Count);
        Assert.AreEqual("Caves", nodes[1].Children[0].Name);
        Assert.AreEqual(new GameFlowObjectDiscriminator(ObjectType.Level), nodes[1].Children[0].Data);
    }

    [TestMethod]
    public void GetSymbols_FiltersOutUnmatchedGroups()
    {
        var provider = new GameFlowNodesProvider(_lineService);
        const string content = "TITLE:\r\nLEVEL: Caves\r\nLEVEL: Venice\r\n";

        IReadOnlyList<TextDocumentSymbol> nodes = provider.GetSymbols(new TextDocumentSymbolRequest(content, "ven"));

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Levels", nodes[0].Name);
        Assert.AreEqual(1, nodes[0].Children.Count);
        Assert.AreEqual("Venice", nodes[0].Children[0].Name);
    }
}
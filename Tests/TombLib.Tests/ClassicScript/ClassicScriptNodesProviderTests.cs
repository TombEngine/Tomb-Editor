using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using TombLib.Scripting.ClassicScript.ContentNodes;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptNodesProviderTests
{
	private static ClassicScriptNodesProvider CreateProvider()
		=> new(new ClassicScriptLineService());

	[TestMethod]
	public void GetSymbols_ReturnsExpectedGroupsForMatchingContent()
	{
		var provider = CreateProvider();
		const string content = "[Options]\r\nName = Caves ; comment\r\n#include \"strings.txt\"\r\n#define SECRET_FLAG ENABLED\r\n[Level]\r\n";

		IReadOnlyList<TextDocumentSymbol> nodes = provider.GetSymbols(new TextDocumentSymbolRequest(content, string.Empty));

		Assert.AreEqual(4, nodes.Count);
		Assert.AreEqual("Sections", nodes[0].Name);
		Assert.AreEqual("[Options]", nodes[0].Children[0].Name);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Section), nodes[0].Children[0].Data);

		Assert.AreEqual("Levels", nodes[1].Name);
		Assert.AreEqual("Caves", nodes[1].Children[0].Name);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Level), nodes[1].Children[0].Data);

		Assert.AreEqual("Includes", nodes[2].Name);
		Assert.AreEqual("strings.txt", nodes[2].Children[0].Name);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Include), nodes[2].Children[0].Data);

		Assert.AreEqual("Defines", nodes[3].Name);
		Assert.AreEqual("SECRET_FLAG", nodes[3].Children[0].Name);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Define), nodes[3].Children[0].Data);
	}

	[TestMethod]
	public void GetSymbols_FiltersOutUnmatchedGroups()
	{
		var provider = CreateProvider();
		const string content = "[Options]\r\nName = Caves\r\n#include \"scripts.dat\"\r\n#define SECRET_FLAG ENABLED\r\n";

		IReadOnlyList<TextDocumentSymbol> nodes = provider.GetSymbols(new TextDocumentSymbolRequest(content, "script"));

		Assert.AreEqual(1, nodes.Count);
		Assert.AreEqual("Includes", nodes[0].Name);
		Assert.AreEqual(1, nodes[0].Children.Count);
		Assert.AreEqual("scripts.dat", nodes[0].Children[0].Name);
	}
}
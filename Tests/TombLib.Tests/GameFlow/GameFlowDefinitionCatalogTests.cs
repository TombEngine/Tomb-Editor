using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Tests;

[TestClass]
public class GameFlowDefinitionCatalogTests
{
	[TestMethod]
	public void SectionHover_ProvidesDefinitionIdentifier()
	{
		var hoverProvider = new GameFlowHoverProvider();
		TextHoverInfo? hoverInfo = hoverProvider.GetHoverInfo(new TextHoverRequest("TITLE:\nLEVEL: Caves", 1));

		Assert.IsNotNull(hoverInfo);
		Assert.AreEqual(new GameFlowObjectDiscriminator(ObjectType.Section), hoverInfo.Identifier);

		var lineService = new GameFlowScriptLineService();
		var documentService = new GameFlowScriptDocumentService(lineService);
		var definitionProvider = new GameFlowDefinitionProvider(documentService);
		var request = new TextDefinitionRequest("TITLE:\nLEVEL: Caves", hoverInfo.SymbolName!, hoverInfo.Identifier as TextDefinitionDiscriminator);

		TextDefinitionLocation? definition = definitionProvider.GetDefinition(request);

		Assert.IsNotNull(definition);
		Assert.AreEqual(1, definition.LineNumber);
	}

	[TestMethod]
	public void Definitions_AreLoadedFromBundledJson()
	{
		Assert.IsTrue(GameFlowDefinitionCatalog.Sections.Count > 0);
		Assert.IsTrue(GameFlowDefinitionCatalog.Properties.Count > 0);
		Assert.IsTrue(GameFlowDefinitionCatalog.Constants.Count > 0);
	}

	[TestMethod]
	public void DefinitionProvider_RequiresGameFlowDiscriminator()
	{
		var lineService = new GameFlowScriptLineService();
		var documentService = new GameFlowScriptDocumentService(lineService);
		var definitionProvider = new GameFlowDefinitionProvider(documentService);
		const string document = "TITLE:\nLEVEL: Caves";

		Assert.IsNull(definitionProvider.GetDefinition(new TextDefinitionRequest(document, "TITLE")));
		Assert.IsNull(definitionProvider.GetDefinition(new TextDefinitionRequest(document, "TITLE", new UnrecognizedDiscriminator())));

		TextDefinitionLocation? location = definitionProvider.GetDefinition(
			new TextDefinitionRequest(document, "TITLE", new GameFlowObjectDiscriminator(ObjectType.Section)));

		Assert.IsNotNull(location);
		Assert.AreEqual(1, location!.LineNumber);
	}

	private sealed record UnrecognizedDiscriminator : TextDefinitionDiscriminator;
}
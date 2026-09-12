using Nickelony.IDEKit.IntelliSense.Navigation;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX.Navigation;

/// <summary>
/// Direct tests for <see cref="TRXDefinitionProvider"/>.
/// </summary>
[TestClass]
public class TRXDefinitionProviderTests
{
	private readonly TRXDefinitionProvider _definitionProvider = new(new TRXDocumentService(new TRXLineService()));

	[TestMethod]
	public void GetDefinition_KnownLevelTitle_ReturnsLocation()
	{
		const string document = "\"title\": \"Caves\"";
		TextDefinitionLocation? location = _definitionProvider.GetDefinition(new TextDefinitionRequest(document, "Caves"));

		Assert.IsNotNull(location);
		Assert.AreEqual(1, location!.LineNumber);
	}

	[TestMethod]
	public void GetDefinition_LevelCommentName_ReturnsLocation()
	{
		const string document = "// Level 1: Caves";
		TextDefinitionLocation? location = _definitionProvider.GetDefinition(new TextDefinitionRequest(document, "Caves"));

		Assert.IsNotNull(location);
		Assert.AreEqual(1, location!.LineNumber);
	}

	[TestMethod]
	public void GetDefinition_UnknownLevel_ReturnsNull()
	{
		const string document = "\"title\": \"Caves\"";
		TextDefinitionLocation? location = _definitionProvider.GetDefinition(new TextDefinitionRequest(document, "Atlantis"));

		Assert.IsNull(location);
	}

	[TestMethod]
	public void GetDefinition_EmptySymbolName_ReturnsNull()
	{
		const string document = "\"title\": \"Caves\"";
		TextDefinitionLocation? location = _definitionProvider.GetDefinition(new TextDefinitionRequest(document, " "));

		Assert.IsNull(location);
	}
}

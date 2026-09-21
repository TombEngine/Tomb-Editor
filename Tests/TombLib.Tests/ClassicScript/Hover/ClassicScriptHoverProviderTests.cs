using Nickelony.IDEKit.IntelliSense.Hover;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Tests.ClassicScript.Hover;

/// <summary>
/// Direct tests for <see cref="ClassicScriptHoverProvider"/> covering commands,
/// section headers, directives, mnemonics, and invalid contexts.
/// </summary>
[TestClass]
public class ClassicScriptHoverProviderTests
{
	private readonly ClassicScriptHoverProvider _hoverProvider;

	public ClassicScriptHoverProviderTests()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

		_hoverProvider = new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService);
	}

	[TestMethod]
	public void GetHoverInfo_KnownCommand_ReturnsCommandHover()
	{
		const string text = "Legend= 42";
		TextHoverInfo? hoverInfo = _hoverProvider.GetHoverInfo(new TextHoverRequest(text, 1));

		Assert.IsNotNull(hoverInfo);
		StringAssert.Contains(hoverInfo!.Content, "Legend");
		Assert.AreEqual("Legend", hoverInfo.SymbolName);
	}

	[TestMethod]
	public void GetHoverInfo_SectionHeader_ReturnsSectionHoverWithTypedDiscriminator()
	{
		const string text = "[Level]";
		TextHoverInfo? hoverInfo = _hoverProvider.GetHoverInfo(new TextHoverRequest(text, 1));

		Assert.IsNotNull(hoverInfo);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Section), hoverInfo!.DefinitionDiscriminator);
	}

	[TestMethod]
	public void GetHoverInfo_Directive_ReturnsDirectiveHover()
	{
		const string text = "#include \"file.txt\"";
		TextHoverInfo? hoverInfo = _hoverProvider.GetHoverInfo(new TextHoverRequest(text, 2));

		Assert.IsNotNull(hoverInfo);
		Assert.AreEqual("#include", hoverInfo!.SymbolName);
	}

	[TestMethod]
	public void GetHoverInfo_UnknownWord_ReturnsNull()
	{
		// A bare word that is not a command, section, directive, number, or mnemonic
		// yields no hover information.
		const string text = "hello";
		TextHoverInfo? hoverInfo = _hoverProvider.GetHoverInfo(new TextHoverRequest(text, 1));

		Assert.IsNull(hoverInfo);
	}

	[TestMethod]
	public void GetHoverInfo_InvalidContext_ReturnsNull()
	{
		// Hovering a plain decimal value that is not a known mnemonic yields no hover.
		const string text = "Legend= 1234";
		TextHoverInfo? hoverInfo = _hoverProvider.GetHoverInfo(new TextHoverRequest(text, text.IndexOf("1234", StringComparison.Ordinal) + 2));

		Assert.IsNull(hoverInfo);
	}
}

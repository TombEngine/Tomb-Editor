using Nickelony.IDEKit.IntelliSense.Signatures;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests.ClassicScript.Signatures;

/// <summary>
/// Direct tests for <see cref="ClassicScriptSignatureHelpProvider"/>.
/// </summary>
[TestClass]
public class ClassicScriptSignatureHelpProviderTests
{
	private readonly ClassicScriptSignatureHelpProvider _signatureHelpProvider;

	public ClassicScriptSignatureHelpProviderTests()
	{
		var lineService = new ClassicScriptLineService();
		var commandService = new ClassicScriptCommandService(lineService, new ClassicScriptMnemonicCatalogService(), new ClassicScriptSyntaxCatalogService());

		_signatureHelpProvider = new ClassicScriptSignatureHelpProvider(commandService);
	}

	[TestMethod]
	public void GetSignatureHelp_InsideKnownCommand_ReturnsSyntaxAndActiveParameter()
	{
		const string text = "[Level]\nLegend= ";
		TextSignatureHelp? info = _signatureHelpProvider.GetSignatureHelp(new TextSignatureHelpRequest(text, text.Length));

		Assert.IsNotNull(info);
		StringAssert.Contains(info!.ActiveSignature.Label, "Legend");
		Assert.IsTrue(info.ActiveParameterIndex >= 0);
	}

	[TestMethod]
	public void GetSignatureHelp_OutsideKnownCommand_ReturnsNull()
	{
		const string text = "not a command at all";
		TextSignatureHelp? info = _signatureHelpProvider.GetSignatureHelp(new TextSignatureHelpRequest(text, 5));

		Assert.IsNull(info);
	}

	[TestMethod]
	public void GetSignatureHelp_InCommentLine_ReturnsNull()
	{
		const string text = "[Level]\n; Legend= commented out";
		TextSignatureHelp? info = _signatureHelpProvider.GetSignatureHelp(new TextSignatureHelpRequest(text, text.Length));

		Assert.IsNull(info);
	}
}

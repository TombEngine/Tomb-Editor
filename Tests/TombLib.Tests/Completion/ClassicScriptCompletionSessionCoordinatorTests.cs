using Nickelony.IDEKit.IntelliSense.Completion;
using System.Linq;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptCompletionSessionCoordinatorTests
{
	private static ClassicScriptCompletionSessionCoordinator CreateCoordinator()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

		return new ClassicScriptCompletionSessionCoordinator(lineService, commandService, mnemonicCatalogService);
	}

	[TestMethod]
	public async Task GetTextEnteredDecisionAsync_AfterEqualsSpace_OpensContextualCompletionAtCaret()
	{
		var coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = await coordinator.GetTextEnteredDecisionAsync(
			"Horizon= ",
			null,
			9,
			" ",
			false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(decision.Items.Any(item => item.Label == "DISABLED"));
		Assert.AreEqual(9, decision.StartOffset);
		Assert.AreEqual(9, decision.EndOffset);
	}

	[TestMethod]
	public async Task GetTextEnteredDecisionAsync_AfterCommaSpace_OpensContextualCompletionAtCaret()
	{
		var coordinator = CreateCoordinator();

		const string text = "Customize= CUST_LOOK_TRASPARENT, ";

		TextCompletionSessionDecision decision = await coordinator.GetTextEnteredDecisionAsync(
			text,
			null,
			text.Length,
			" ",
			false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(decision.Items.Any(item => item.Label == "DISABLED"));
		Assert.AreEqual(text.Length, decision.StartOffset);
		Assert.AreEqual(text.Length, decision.EndOffset);
	}

	[TestMethod]
	public async Task GetCtrlSpaceDecisionAsync_EmptyDocument_ReturnsEmptyLineCompletion()
	{
		var coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = await coordinator.GetCtrlSpaceDecisionAsync(string.Empty, null, 0, false);

		Assert.IsNotNull(decision.Items);
	}

	[TestMethod]
	public async Task GetCtrlSpaceDecisionAsync_CaretAtStartOfCommandLine_DoesNotThrow()
	{
		var coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = await coordinator.GetCtrlSpaceDecisionAsync("Horizon=", null, 0, false);

		Assert.AreEqual(TextCompletionSessionDecision.None, decision);
	}
}
using Nickelony.IDEKit.IntelliSense.Completion;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests.ClassicScript;

/// <summary>
/// Tests for the <see cref="ClassicScriptCompletionProvider"/> contextual argument resolution
/// from the caret position and null-request rejection.
/// </summary>
[TestClass]
public class ClassicScriptCompletionProviderTests
{
	private static ITextCompletionProvider CreateProvider()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

		return new ClassicScriptCompletionProvider(commandService, mnemonicCatalogService);
	}

	[TestMethod]
	public void ContextualTrigger_CaretInSecondArgument_ResolvesArgumentFromCaret()
	{
		ITextCompletionProvider provider = CreateProvider();

		// The caret sits after the comma, in the second argument, so the index resolved
		// from the caret is beyond the single-argument syntax and yields no items.
		IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
			new TextCompletionRequest("Horizon= ENABLED, ", 18, ClassicScriptCompletionTriggers.Contextual));

		Assert.AreEqual(0, items.Count);
	}

	[TestMethod]
	public void ContextualTrigger_CaretInFirstArgument_OffersEnabledAndDisabled()
	{
		ITextCompletionProvider provider = CreateProvider();

		// The caret is inside the first argument, so the syntax resolves the first slot and
		// the ENABLED/DISABLED values are offered.
		IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
			new TextCompletionRequest("Horizon= ", 9, ClassicScriptCompletionTriggers.Contextual));

		Assert.IsTrue(items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(items.Any(item => item.Label == "DISABLED"));
	}

	[TestMethod]
	public void NullRequest_Throws()
	{
		ITextCompletionProvider provider = CreateProvider();

		Assert.ThrowsException<ArgumentNullException>(() => provider.GetCompletionItems(null!));
	}
}

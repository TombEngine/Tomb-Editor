using Nickelony.IDEKit.IntelliSense.Completion;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests.ClassicScript;

/// <summary>
/// Tests for the <see cref="ClassicScriptCompletionProvider"/> handling of the <c>-1</c>
/// argument-index sentinel and null-context rejection.
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
	public void MinusOneSentinel_ResolvesArgumentFromCaret()
	{
		ITextCompletionProvider provider = CreateProvider();

		// The caret sits after the comma, in the second argument, so the index resolved
		// from the caret is beyond the single-argument syntax and yields no items.
		IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
			new TextCompletionContext("Horizon= ENABLED, ", 18, TextCompletionTrigger.Contextual, -1));

		Assert.AreEqual(0, items.Count);
	}

	[TestMethod]
	public void ZeroArgumentIndex_DoesNotResolveFromCaret()
	{
		ITextCompletionProvider provider = CreateProvider();

		// A forced index of 0 treats the caret as being in the first argument even though
		// it is past a comma, so the first argument's ENABLED/DISABLED items are offered.
		IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
			new TextCompletionContext("Horizon= ENABLED, ", 18, TextCompletionTrigger.Contextual, 0));

		Assert.IsTrue(items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(items.Any(item => item.Label == "DISABLED"));
	}

	[TestMethod]
	public void NullContext_Throws()
	{
		ITextCompletionProvider provider = CreateProvider();

		Assert.ThrowsException<ArgumentNullException>(() => provider.GetCompletionItems(null!));
	}
}

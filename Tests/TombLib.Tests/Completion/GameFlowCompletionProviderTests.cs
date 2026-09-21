using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.GameFlowScript.Completion;

namespace TombLib.Tests;

[TestClass]
public class GameFlowCompletionProviderTests
{
	private static GameFlowCompletionProvider CreateProvider() => new();

	[TestMethod]
	public void GetCompletionItems_EmptyWord_ReturnsAllCatalogItems()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionRequest(string.Empty, 0));

		Assert.IsTrue(items.Count > 0);
		Assert.IsTrue(items.Any(item => item.InsertText == "LEVEL: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "DESCRIPTION: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "EXIT_TO_TITLE"));
	}

	[TestMethod]
	public void GetCompletionItems_CaretAtDocumentStart_ReturnsAllCatalogItems()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionRequest("LEVEL: ", 0));

		Assert.IsTrue(items.Count > 0);
	}

	[TestMethod]
	public void GetCompletionItems_ReturnsAllCatalogItemsRegardlessOfTypedWord()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionRequest("TITLE", 5));

		Assert.IsTrue(items.Any(item => item.InsertText == "TITLE: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "LEVEL: "));
	}

	[TestMethod]
	public void GetCompletionItems_NonMatchingWord_StillReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionRequest("QwErTyZz", 8));

		Assert.IsTrue(items.Count > 0);
		Assert.IsTrue(items.Any(item => item.InsertText == "LEVEL: "));
	}

	[TestMethod]
	public void GetCompletionItems_BundledCatalog_HasUniqueInsertionTextsIncludingOverlappingNames()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionRequest(string.Empty, 0));

		// No two items may share a case-insensitive insertion text, even when a name belongs to
		// both the constants and the properties categories (e.g. KEY1 and PICKUP1).
		Assert.AreEqual(items.Count, items.Select(item => item.InsertText).Distinct(StringComparer.OrdinalIgnoreCase).Count());

		// The overlapping constant/property names keep both their constant and property forms.
		Assert.IsTrue(items.Any(item => item.InsertText == "KEY1"));
		Assert.IsTrue(items.Any(item => item.InsertText == "KEY1: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "PICKUP1"));
		Assert.IsTrue(items.Any(item => item.InsertText == "PICKUP1: "));
	}
}

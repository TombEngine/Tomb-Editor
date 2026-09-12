using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXGameFlowCompletionServiceTests
{
	private sealed class StubSchemaService(TRXGameFlowSchemaModel? model) : ITRXGameFlowSchemaService
	{
		public TRXSchemaLoadState LoadState => model is null ? TRXSchemaLoadState.InvalidSchema : TRXSchemaLoadState.Loaded;

		public TRXGameFlowSchemaModel? Model => model;

		public TRXSchemaKeywords Keywords => model?.Keywords ?? TRXSchemaKeywords.Empty;
	}

	[TestMethod]
	public void GetCompletionItems_EmptyWord_ReturnsSchemaItems()
	{
		var model = CreateModel(new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], null));

		var service = new TRXGameFlowCompletionService(new StubSchemaService(model));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		Assert.IsTrue(items.Any(item => item.InsertText == "\"title\": "));
		Assert.IsTrue(items.Any(item => item.InsertText == "true"));
	}

	[TestMethod]
	public void GetCompletionItems_ReturnsAllSchemaItemsRegardlessOfTypedWord()
	{
		var model = CreateModel(
			new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], null),
			new TRXGameFlowProperty("level", [TRXGameFlowPropertyType.String], null));

		var service = new TRXGameFlowCompletionService(new StubSchemaService(model));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext("\"tit", 4));

		// The provider returns the full candidate set: both schema properties plus the JSON primitives.
		Assert.AreEqual(5, items.Count);
		Assert.IsTrue(items.Any(item => item.InsertText == "\"title\": "));
		Assert.IsTrue(items.Any(item => item.InsertText == "\"level\": "));
	}

	[TestMethod]
	public void GetCompletionItems_NullModel_ReturnsJsonPrimitives()
	{
		var service = new TRXGameFlowCompletionService(new StubSchemaService(null));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		Assert.IsFalse(items.Any(item => item.InsertText == "\"title\": "));
		Assert.IsTrue(items.Any(item => item.InsertText == "true"));
		Assert.IsTrue(items.Any(item => item.InsertText == "false"));
		Assert.IsTrue(items.Any(item => item.InsertText == "null"));
	}

	[TestMethod]
	public void GetCompletionItems_EmptyModel_ReturnsJsonPrimitivesOnly()
	{
		var service = new TRXGameFlowCompletionService(new StubSchemaService(new TRXGameFlowSchemaModel([], TRXSchemaKeywords.Empty)));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		Assert.AreEqual(3, items.Count);
		Assert.IsTrue(items.All(item => item.InsertText is "true" or "false" or "null"));
	}

	[TestMethod]
	public void GetCompletionItems_ArrayProperty_UsesArrayKind()
	{
		var model = CreateModel(new TRXGameFlowProperty("levels", [TRXGameFlowPropertyType.Array], null));

		var service = new TRXGameFlowCompletionService(new StubSchemaService(model));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		var item = items.First(candidate => candidate.InsertText == "\"levels\": ");
		Assert.AreEqual(TextCompletionItemKind.Array, item.Kind);
	}

	private static TRXGameFlowSchemaModel CreateModel(params TRXGameFlowProperty[] properties)
	{
		var collections = properties.Where(property => property.IsArray).Select(property => property.Name).ToArray();
		var names = properties.Where(property => !property.IsArray).Select(property => property.Name).ToArray();
		var keywords = new TRXSchemaKeywords(collections, names, []);

		return new TRXGameFlowSchemaModel(properties, keywords);
	}
}

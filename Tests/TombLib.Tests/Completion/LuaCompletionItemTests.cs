using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;

namespace TombLib.Tests;

[TestClass]
public class LuaCompletionItemTests
{
	[TestMethod]
	public async Task WithRequestContext_PreservesRequestMetadataAcrossResolve()
	{
		var item = new TextCompletionItem("spawn")
		{
			InsertText = "spawn",
			ResolveCallback = _ => Task.FromResult(new TextCompletionItem("spawn")
			{
				Detail = "function",
				InsertTextFormat = TextCompletionInsertTextFormat.Snippet
			}),
			InsertTextFormat = TextCompletionInsertTextFormat.Snippet
		}
		.WithRequestContext(4, 7);

		TextCompletionItem resolvedItem = item.WithResolvedContent(await item.ResolveAsync());

		Assert.AreEqual(4, resolvedItem.RequestDocumentVersion);
		Assert.AreEqual(7, resolvedItem.RequestGeneration);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual(TextCompletionInsertTextFormat.Snippet, resolvedItem.InsertTextFormat);
	}

	[TestMethod]
	public async Task CommitContext_DropsTextEditAndPreservesRequestStamps()
	{
		TextCompletionTextEdit textEdit = new(
			new TextRange(2, 3));

		var item = new TextCompletionItem("Color")
		{
			InsertText = "Color",
			ResolveCallback = _ => Task.FromResult(new TextCompletionItem("Color")
			{
				Detail = "enum",
				TextEdit = textEdit
			}),
			TextEdit = textEdit
		}
		.WithRequestContext(6, 2)
		.WithoutTextEdit();

		Assert.AreEqual(6, item.RequestDocumentVersion);
		Assert.AreEqual(2, item.RequestGeneration);
		Assert.IsNull(item.TextEdit);

		TextCompletionItem resolvedItem = await item.ResolveAsync();

		// The raw resolve path returns the callback result as supplied, including its edit payload;
		// a commit path that must not use resolved edit data re-applies WithoutTextEdit itself.
		Assert.AreEqual("enum", resolvedItem.Detail);
		Assert.AreEqual(textEdit, resolvedItem.TextEdit);
	}
}

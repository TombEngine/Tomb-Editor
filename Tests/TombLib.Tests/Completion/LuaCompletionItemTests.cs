using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;

namespace TombLib.Tests;

[TestClass]
public class LuaCompletionItemTests
{
	[TestMethod]
	public async Task WithRequestContext_PreservesRequestMetadataAcrossResolve()
	{
		var item = new TextCompletionItem(
			"spawn",
			insertText: "spawn",
			resolveAsync: _ => Task.FromResult(new TextCompletionItem("spawn", detail: "function", insertCaretOffset: 2)),
			insertCaretOffset: 2)
			.WithRequestContext(4, 7);

		TextCompletionItem resolvedItem = await item.ResolveAsync();

		Assert.AreEqual(4, resolvedItem.RequestDocumentVersion);
		Assert.AreEqual(7, resolvedItem.RequestGeneration);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual(2, resolvedItem.InsertCaretOffset);
	}

	[TestMethod]
	public async Task WithFilteredCommitContext_DropsTextEditAndPreservesResolveMetadata()
	{
		TextCompletionTextEdit textEdit = new(
			new TextRange(2, 3));

		var item = new TextCompletionItem(
			"Color",
			insertText: "Color",
			resolveAsync: _ => Task.FromResult(new TextCompletionItem("Color", detail: "enum", textEdit: textEdit)),
			textEdit: textEdit)
			.WithFilteredCommitContext(6, 2);

		Assert.AreEqual(6, item.RequestDocumentVersion);
		Assert.AreEqual(2, item.RequestGeneration);
		Assert.IsNull(item.TextEdit);

		TextCompletionItem resolvedItem = await item.ResolveAsync();

		Assert.AreEqual("enum", resolvedItem.Detail);
		Assert.IsNull(resolvedItem.TextEdit);
		Assert.AreEqual(6, resolvedItem.RequestDocumentVersion);
		Assert.AreEqual(2, resolvedItem.RequestGeneration);
	}
}

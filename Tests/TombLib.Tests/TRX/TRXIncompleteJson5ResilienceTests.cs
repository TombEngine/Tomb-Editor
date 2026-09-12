using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Hover;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXIncompleteJson5ResilienceTests
{
	private static readonly string[] Fragments =
	[
		"{",
		"{ levels: [",
		"{ title: \"",
		"{ levels: [{ sequence: [{ type: \"play_m",
		"{ /* unterminated comment",
		"{ key: value,",
		"{ title: \"escaped \\\" quote\", levels: [{ type: \"play_m\" },",
		"{ levels: [{ sequence: [{ type: \"play_m\", /* comment */ name: \"C:\\\\temp\" }], },"
	];

	[TestMethod]
	public void IncompleteFragments_AllCaretOffsetsKeepCompletionAndHoverSafe()
	{
		var schemaService = new StubSchemaService(new TRXGameFlowSchemaModel(
			[
				new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], "Display title."),
				new TRXGameFlowProperty("levels", [TRXGameFlowPropertyType.Array], "Level definitions."),
				new TRXGameFlowProperty("type", [TRXGameFlowPropertyType.String], "Entry type.")
			],
			new TRXSchemaKeywords(["levels"], ["title", "type"], ["play_movie"])));
		var completionService = new TRXGameFlowCompletionService(schemaService);
		var coordinator = new TRXCompletionSessionCoordinator(
			completionService,
			new TextAnalysisService(),
			new CompletionManager(new TRXLineService()));
		var hoverService = new TRXGameFlowHoverService(schemaService);

		foreach (string fragment in Fragments)
		{
			for (int offset = 0; offset <= fragment.Length; offset++)
			{
				var document = new TextDocument(fragment);
				TextCompletionSessionDecision firstCtrlSpace = coordinator.GetCtrlSpaceDecision(document, offset, false);
				TextCompletionSessionDecision secondCtrlSpace = coordinator.GetCtrlSpaceDecision(document, offset, false);
				AssertStable(firstCtrlSpace, secondCtrlSpace, fragment, offset, "Ctrl+Space");
				AssertValidRange(firstCtrlSpace, fragment, offset, "Ctrl+Space");

				TextCompletionSessionDecision firstQuoteTrigger = coordinator.GetTextEnteredDecision(document, offset, "\"", false);
				TextCompletionSessionDecision secondQuoteTrigger = coordinator.GetTextEnteredDecision(document, offset, "\"", false);
				AssertStable(firstQuoteTrigger, secondQuoteTrigger, fragment, offset, "quote trigger");
				AssertValidRange(firstQuoteTrigger, fragment, offset, "quote trigger");

				TextHoverInfo? firstHover = hoverService.GetHoverInfo(new TextHoverRequest(fragment, offset));
				TextHoverInfo? secondHover = hoverService.GetHoverInfo(new TextHoverRequest(fragment, offset));
				Assert.AreEqual(firstHover?.SymbolName, secondHover?.SymbolName, Describe(fragment, offset));
				Assert.AreEqual(firstHover?.Content, secondHover?.Content, Describe(fragment, offset));
				Assert.AreEqual(fragment, document.Text, Describe(fragment, offset));
			}
		}
	}

	[TestMethod]
	public void KnownSchemaProperty_StillProducesCompletionAndHoverInformation()
	{
		var schemaService = new StubSchemaService(new TRXGameFlowSchemaModel(
			[new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], "Display title.")],
			TRXSchemaKeywords.Empty));
		var coordinator = new TRXCompletionSessionCoordinator(
			new TRXGameFlowCompletionService(schemaService),
			new TextAnalysisService(),
			new CompletionManager(new TRXLineService()));
		var hoverService = new TRXGameFlowHoverService(schemaService);
		const string completionFragment = "{\"tit";

		TextCompletionSessionDecision decision = coordinator.GetCtrlSpaceDecision(
			new TextDocument(completionFragment),
			completionFragment.Length,
			false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.InsertText == "\"title\": "));
		Assert.AreEqual(1, decision.StartOffset);
		Assert.AreEqual(completionFragment.Length, decision.EndOffset);

		TextHoverInfo? hover = hoverService.GetHoverInfo(new TextHoverRequest("{\"title\": ", 3));

		Assert.IsNotNull(hover);
		Assert.AreEqual("title", hover.SymbolName);
		StringAssert.Contains(hover.Content, "Display title.");
	}

	private static void AssertStable(
		TextCompletionSessionDecision first,
		TextCompletionSessionDecision second,
		string fragment,
		int offset,
		string operation)
	{
		string message = $"{operation} changed at offset {offset} in '{fragment}'.";
		Assert.AreEqual(first.CloseWindow, second.CloseWindow, message);
		Assert.AreEqual(first.StartOffset, second.StartOffset, message);
		Assert.AreEqual(first.EndOffset, second.EndOffset, message);

		string[] firstItems = first.Items?.Select(item => item.InsertText).ToArray() ?? [];
		string[] secondItems = second.Items?.Select(item => item.InsertText).ToArray() ?? [];
		CollectionAssert.AreEqual(firstItems, secondItems, message);
	}

	private static void AssertValidRange(
		TextCompletionSessionDecision decision,
		string fragment,
		int offset,
		string operation)
	{
		if (decision.StartOffset is not int startOffset || decision.EndOffset is not int endOffset)
			return;

		Assert.IsTrue(startOffset >= 0, Describe(fragment, offset, operation));
		Assert.IsTrue(startOffset <= endOffset, Describe(fragment, offset, operation));
		Assert.IsTrue(endOffset <= fragment.Length, Describe(fragment, offset, operation));
	}

	private static string Describe(string fragment, int offset, string operation = "Hover")
		=> $"{operation} failed at offset {offset} in '{fragment}'.";

	private sealed class StubSchemaService(TRXGameFlowSchemaModel model) : ITRXGameFlowSchemaService
	{
		public TRXSchemaLoadState LoadState => TRXSchemaLoadState.Loaded;

		public TRXGameFlowSchemaModel? Model => model;

		public TRXSchemaKeywords Keywords => model.Keywords;
	}
}
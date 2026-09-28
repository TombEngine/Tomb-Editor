using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.Core.Identifiers;
using ICSharpCode.AvalonEdit.Document;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXCompletionManagerTests
{
	private static readonly IReadOnlyList<TextCompletionItem> Items =
	[
		new TextCompletionItem("\"title\": "),
		new TextCompletionItem("\"level\": "),
		new TextCompletionItem("true"),
		new TextCompletionItem("false"),
		new TextCompletionItem("null")
	];

	private static CompletionManager CreateManager()
		=> new(new TRXLineService());

	[TestMethod]
	public void FilterCompletions_EmptyWord_ReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, string.Empty);

		Assert.AreEqual(Items.Count, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_QuoteOnlyWord_ReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"");

		Assert.AreEqual(Items.Count, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_MatchesQuotedPrefixCaseInsensitively()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"Ti");

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("\"title\": ", result[0].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_MatchesWordWithoutQuotes()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "lev");

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("\"level\": ", result[0].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_NoMatch_ReturnsEmpty()
	{
		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "zzz");

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void FilterCompletions_IsOrdinalCaseInsensitive_NotCultureSensitive()
	{
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

		try
		{
			// Under tr-TR, "TITLE".ToLower() is "tıtle" (dotless i), so a culture-sensitive
			// filter would fail to match "title"; ordinal ignore-case matching must still match.
			IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(Items, "\"TITLE");

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("\"title\": ", result[0].InsertText);
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[TestMethod]
	public void FilterCompletions_PrefixMatchesSortBeforeContainsMatches()
	{
		IReadOnlyList<TextCompletionItem> customItems =
		[
			new TextCompletionItem("\"partition\": "),
			new TextCompletionItem("\"title\": ")
		];

		IReadOnlyList<TextCompletionItem> result = CreateManager().FilterCompletions(customItems, "\"ti");

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("\"title\": ", result[0].InsertText);
		Assert.AreEqual("\"partition\": ", result[1].InsertText);
	}

	[TestMethod]
	public void FilterCompletions_SingleStage_IsNoSlowerOrHeavierThanDoubleStage()
	{
		CompletionManager manager = CreateManager();
		var context = new TextCompletionRequest("\"ti", 3);

		// The pre-Phase-19 path filtered in the provider (TextCompletionFilter) and again in the
		// coordinator (CompletionManager); the coordinator-owned single stage must not regress it.
		const int iterations = 500;

		// Warm up both code paths so JIT and static caches are settled.
		RunDoubleStageFilter(manager, context);
		RunSingleStageFilter(manager);

		long beforeAllocated = MeasureAllocations(() => RunDoubleStageFilter(manager, context), iterations);
		long afterAllocated = MeasureAllocations(() => RunSingleStageFilter(manager), iterations);
		double beforeElapsed = MeasureElapsed(() => RunDoubleStageFilter(manager, context), iterations);
		double afterElapsed = MeasureElapsed(() => RunSingleStageFilter(manager), iterations);

				// The coordinator-owned single stage filters the full set, while the double stage's provider
		// pass first reduces it; the lean provider filter allocates only its result array, so the
		// single stage pays a small per-item overhead for the items the provider pass removed.
		Assert.IsTrue(afterAllocated <= beforeAllocated * 1.5, $"Single stage allocated {afterAllocated} B vs {beforeAllocated} B.");
		Assert.IsTrue(afterElapsed <= beforeElapsed * 2.0, $"Single stage took {afterElapsed:F1} ms vs {beforeElapsed:F1} ms.");
	}

	[TestMethod]
	public void LargeIncompleteDocumentCompletion_RemainsCorrectAndScalesWithinGenerousBounds()
	{
		var schemaService = new StubSchemaService(CreateSchemaModel());
		var coordinator = new TRXCompletionSessionCoordinator(
			new TRXGameFlowCompletionService(schemaService),
			new TextAnalysisService(),
			new CompletionManager(new TRXLineService()));
		var smallDocument = new TextDocument(CreateIncompleteDocument(64));
		var largeDocument = new TextDocument(CreateIncompleteDocument(1024));
		int smallCaretOffset = smallDocument.TextLength;
		int largeCaretOffset = largeDocument.TextLength;

		TextCompletionSessionDecision decision = coordinator.GetCtrlSpaceDecision(largeDocument, largeCaretOffset, false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.InsertText == "\"title\": "));
		Assert.IsTrue(decision.StartOffset >= 0);
		Assert.AreEqual(largeCaretOffset, decision.EndOffset);

		const int iterations = 100;
		coordinator.GetCtrlSpaceDecision(smallDocument, smallCaretOffset, false);
		coordinator.GetCtrlSpaceDecision(largeDocument, largeCaretOffset, false);

		long smallAllocated = MeasureAllocations(
			() => coordinator.GetCtrlSpaceDecision(smallDocument, smallCaretOffset, false),
			iterations);
		long largeAllocated = MeasureAllocations(
			() => coordinator.GetCtrlSpaceDecision(largeDocument, largeCaretOffset, false),
			iterations);
		double smallElapsed = MeasureElapsed(
			() => coordinator.GetCtrlSpaceDecision(smallDocument, smallCaretOffset, false),
			iterations);
		double largeElapsed = MeasureElapsed(
			() => coordinator.GetCtrlSpaceDecision(largeDocument, largeCaretOffset, false),
			iterations);

		Assert.IsTrue(largeAllocated <= smallAllocated * 20 + 1_000_000,
			$"Large document allocated {largeAllocated} B vs {smallAllocated} B for the small document.");
		Assert.IsTrue(largeElapsed <= smallElapsed * 20.0 + 100.0,
			$"Large document took {largeElapsed:F1} ms vs {smallElapsed:F1} ms for the small document.");
	}

	private static IReadOnlyList<TextCompletionItem> RunSingleStageFilter(CompletionManager manager)
		=> manager.FilterCompletions(Items, "\"ti");

	private static IReadOnlyList<TextCompletionItem> RunDoubleStageFilter(CompletionManager manager, TextCompletionRequest context)
	{
		IReadOnlyList<TextCompletionItem> providerFiltered = TextCompletionFilter.FilterByWord(Items, IdentifierOperations.GetWordEndingAt(context.DocumentText, context.CaretOffset));
		return manager.FilterCompletions(providerFiltered, "\"ti");
	}

	private static long MeasureAllocations(Action action, int iterations)
	{
		long before = GC.GetAllocatedBytesForCurrentThread();

		for (int i = 0; i < iterations; i++)
			action();

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	private static double MeasureElapsed(Action action, int iterations)
	{
		var stopwatch = Stopwatch.StartNew();

		for (int i = 0; i < iterations; i++)
			action();

		stopwatch.Stop();
		return stopwatch.Elapsed.TotalMilliseconds;
	}

	private static TRXGameFlowSchemaModel CreateSchemaModel()
	{
		var properties = Enumerable.Range(0, 64)
			.Select(index => new TRXGameFlowProperty($"property{index}", [TRXGameFlowPropertyType.String], null))
			.Append(new TRXGameFlowProperty("title", [TRXGameFlowPropertyType.String], "Display title."))
			.ToArray();

		return new TRXGameFlowSchemaModel(properties, TRXSchemaKeywords.Empty);
	}

	private static string CreateIncompleteDocument(int fillerLineCount)
		=> string.Concat(Enumerable.Repeat("{ filler: true }\n", fillerLineCount)) + "{\"tit";

	private sealed class StubSchemaService(TRXGameFlowSchemaModel model) : ITRXGameFlowSchemaService
	{
		public TRXSchemaLoadState LoadState => TRXSchemaLoadState.Loaded;

		public TRXGameFlowSchemaModel? Model => model;

		public TRXSchemaKeywords Keywords => model.Keywords;
	}
}

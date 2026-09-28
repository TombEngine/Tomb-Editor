using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXCompletionSessionCoordinatorTests
{
	private sealed class StubCompletionProvider(IReadOnlyList<TextCompletionItem> items) : ITextCompletionProvider
	{
		private readonly IReadOnlyList<TextCompletionItem> _items = items;

		public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionRequest context) => _items;
	}

	private static TRXCompletionSessionCoordinator CreateCoordinator(params TextCompletionItem[] items)
	{
		return new(
			new StubCompletionProvider(items),
			new TextAnalysisService(),
			new CompletionManager(new TRXLineService()));
	}

	[TestMethod]
	public void GetCtrlSpaceDecision_FiltersThroughCompletionManager()
	{
		TRXCompletionSessionCoordinator coordinator = CreateCoordinator(
			new TextCompletionItem("\"title\": "),
			new TextCompletionItem("\"level\": "));

		TextCompletionSessionDecision decision = coordinator.GetCtrlSpaceDecision(new TextDocument("\"ti"), 3, false);

		Assert.IsNotNull(decision.Items);
		Assert.AreEqual(1, decision.Items.Count);
		Assert.AreEqual("\"title\": ", decision.Items[0].InsertText);
		Assert.AreEqual(0, decision.StartOffset);
		Assert.AreEqual(3, decision.EndOffset);
	}

	[TestMethod]
	public void GetCtrlSpaceDecision_NoMatchingItems_ReportsNoMatches()
	{
		TRXCompletionSessionCoordinator coordinator = CreateCoordinator(new TextCompletionItem("\"title\": "));

		TextCompletionSessionDecision decision = coordinator.GetCtrlSpaceDecision(new TextDocument("\"zz"), 3, false);

		// The provider returned candidates, but the typed word filtered all of them out, so the
		// kernel reports the NoMatches state instead of the plain None.
		Assert.AreEqual(TextCompletionSessionDecision.NoMatches, decision);
	}
}

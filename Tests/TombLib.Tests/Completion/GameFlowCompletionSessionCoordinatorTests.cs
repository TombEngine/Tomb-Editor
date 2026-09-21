using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.IntelliSense.Completion;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Services;

namespace TombLib.Tests;

[TestClass]
public class GameFlowCompletionSessionCoordinatorTests
{
	private static GameFlowCompletionSessionCoordinator CreateCoordinator()
		=> new(new GameFlowCompletionProvider(), new GameFlowScriptLineService());

	[TestMethod]
	public void GetOpenDecision_EmptyDocument_ReturnsNone()
	{
		GameFlowCompletionSessionCoordinator coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = coordinator.GetOpenDecision(new TextDocument(string.Empty), 0, false);

		Assert.AreEqual(TextCompletionSessionDecision.None, decision);
	}

	[TestMethod]
	public void GetOpenDecision_CaretAtStartOfSingleCharacterLine_ReturnsNone()
	{
		GameFlowCompletionSessionCoordinator coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = coordinator.GetOpenDecision(new TextDocument("A"), 0, false);

		Assert.AreEqual(TextCompletionSessionDecision.None, decision);
	}

	[TestMethod]
	public void GetOpenDecision_FiltersItemsByTypedWord()
	{
		GameFlowCompletionSessionCoordinator coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = coordinator.GetOpenDecision(new TextDocument("T"), 1, false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.All(item => item.InsertText.Contains("T", StringComparison.OrdinalIgnoreCase)));
		Assert.IsTrue(decision.Items.Any(item => item.InsertText == "TITLE: "));
		Assert.IsFalse(decision.Items.Any(item => item.InsertText == "LEVEL: "));
	}
}

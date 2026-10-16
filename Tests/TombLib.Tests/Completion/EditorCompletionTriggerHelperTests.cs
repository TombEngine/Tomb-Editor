using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public sealed class EditorCompletionTriggerHelperTests
{
	[TestMethod]
	public void IsCtrlSpaceInput_RequiresSpaceAndCtrlModifier()
	{
		Assert.IsTrue(EditorCompletionTriggerHelper.IsCtrlSpaceInput(" ", true));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsCtrlSpaceInput(null, true));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsCtrlSpaceInput("a", true));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsCtrlSpaceInput(" ", false));
	}

	[TestMethod]
	public void IsSingleCharacterLine_ValidatesLengthAndCharacterPredicate()
	{
		Assert.IsTrue(EditorCompletionTriggerHelper.IsSingleCharacterLine("a"));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine(string.Empty));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine("ab"));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine(null));

		Assert.IsTrue(EditorCompletionTriggerHelper.IsSingleCharacterLine("a", char.IsLetter));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine("1", char.IsLetter));
	}
}

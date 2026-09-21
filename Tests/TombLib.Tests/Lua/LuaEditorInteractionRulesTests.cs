using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorInteractionRulesTests
{
	[TestMethod]
	public void IsValidCompletionContext_AllowsMemberTriggerInCode()
	{
		var document = CreateDocument("player.");

		bool result = LuaEditorInteractionRules.IsValidCompletionContext(document, document.TextLength, '.');

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsValidCompletionContext_BlocksIdentifierImmediatelyAfterDot()
	{
		var document = CreateDocument("player.a");

		bool result = LuaEditorInteractionRules.IsValidCompletionContext(document, document.TextLength, null);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidCompletionContext_BlocksLongStringContinuationOnFollowingLine()
	{
		var document = CreateDocument(
			"value = [[long string",
			"player");

		bool result = LuaEditorInteractionRules.IsValidCompletionContext(document, document.TextLength, null);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksCommentText()
	{
		var document = CreateDocument("-- player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenString()
	{
		var document = CreateDocument("print(\"player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenLongComment()
	{
		var document = CreateDocument("--[[ player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenLongString()
	{
		var document = CreateDocument("value = [[player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanShowHover_ReturnsFalseWhenCompletionWindowIsOpen()
	{
		bool result = LuaEditorInteractionRules.CanShowHover(true, false);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanShowHover_ReturnsFalseWhenSignatureHelpIsOpen()
	{
		bool result = LuaEditorInteractionRules.CanShowHover(false, true);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksLongCommentContinuationOnFollowingLine()
	{
		var document = CreateDocument(
			"--[[ comment",
			"player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetHoverOffset_ReturnsOffsetWhenPointerIsOnIdentifierText()
	{
		const string identifier = "targetValue";
		const string text = "return " + identifier;

		var document = CreateDocument(text);
		int identifierStart = text.IndexOf(identifier, StringComparison.Ordinal);
		int probeOffset = identifierStart + 2;

		bool result = LuaEditorInteractionRules.TryGetHoverOffset(document, probeOffset, out int hoverOffset);

		Assert.IsTrue(result);
		Assert.AreEqual(probeOffset, hoverOffset);
	}

	[TestMethod]
	public void TryGetHoverOffset_BlocksTrailingWhitespaceAfterIdentifier()
	{
		const string identifier = "targetValue";
		const string text = "return " + identifier;

		var document = CreateDocument(text);
		int probeOffset = document.TextLength;

		bool result = LuaEditorInteractionRules.TryGetHoverOffset(document, probeOffset, out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetHoverOffset_BlocksTextInsideLongCommentContinuation()
	{
		var document = CreateDocument(
			"--[[ comment",
			"targetValue");

		int probeOffset = document.Text.IndexOf("targetValue", StringComparison.Ordinal) + 2;

		bool result = LuaEditorInteractionRules.TryGetHoverOffset(document, probeOffset, out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetHoverOffset_RefreshesLongCommentStateAfterDocumentEdit()
	{
		var document = CreateDocument(
			"--[[ comment",
			"targetValue");

		int initialProbeOffset = document.Text.IndexOf("targetValue", StringComparison.Ordinal) + 2;

		bool initialResult = LuaEditorInteractionRules.TryGetHoverOffset(document, initialProbeOffset, out _);

		document.Text = string.Join(Environment.NewLine,
			"--[[ comment ]]",
			"targetValue");

		int updatedProbeOffset = document.Text.IndexOf("targetValue", StringComparison.Ordinal) + 2;

		bool updatedResult = LuaEditorInteractionRules.TryGetHoverOffset(document, updatedProbeOffset, out int hoverOffset);

		Assert.IsFalse(initialResult);
		Assert.IsTrue(updatedResult);
		Assert.AreEqual(updatedProbeOffset, hoverOffset);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_ReturnsWordStartFromInsideIdentifier()
	{
		const string identifier = "targetValue";
		const string text = "local " + identifier + " = 1";

		var document = CreateDocument(text);
		int identifierStart = text.IndexOf(identifier, StringComparison.Ordinal);
		int probeOffset = identifierStart + 4;

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, probeOffset, out int definitionOffset);

		Assert.IsTrue(result);
		Assert.AreEqual(identifierStart, definitionOffset);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_ReturnsWordStartWhenCaretIsAfterIdentifier()
	{
		const string identifier = "targetValue";
		const string text = "return " + identifier;

		var document = CreateDocument(text);
		int identifierStart = text.IndexOf(identifier, StringComparison.Ordinal);
		int probeOffset = identifierStart + identifier.Length;

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, probeOffset, out int definitionOffset);

		Assert.IsTrue(result);
		Assert.AreEqual(identifierStart, definitionOffset);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_BlocksCommentText()
	{
		var document = CreateDocument("-- targetValue");

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, document.TextLength, out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_BlocksLongStringContinuationOnFollowingLine()
	{
		var document = CreateDocument(
			"value = [[long string",
			"targetValue");

		int probeOffset = document.Text.IndexOf("targetValue", StringComparison.Ordinal) + 3;

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, probeOffset, out _);

		Assert.IsFalse(result);
	}

	private static TextDocument CreateDocument(params string[] lines)
		=> new(string.Join(Environment.NewLine, lines));
}

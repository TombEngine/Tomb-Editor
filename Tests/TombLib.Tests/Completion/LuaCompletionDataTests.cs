using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using System.Reflection;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class LuaCompletionDataTests
{
	[TestMethod]
	public void ResolveCompletionSegment_UsesInsertRangeByDefault()
	{
		(int offset, int length) = InvokeResolveCompletionSegment(
			new TextDocument("abcdef"),
			fallbackOffset: 1,
			fallbackLength: 2,
			new TextCompletionTextEdit(
				new TextRange(2, 1),
				new TextRange(2, 3)),
			useReplaceRange: false);

		Assert.AreEqual(2, offset);
		Assert.AreEqual(1, length);
	}

	[TestMethod]
	public void ResolveCompletionSegment_UsesReplaceRangeWhenRequested()
	{
		(int offset, int length) = InvokeResolveCompletionSegment(
			new TextDocument("abcdef"),
			fallbackOffset: 1,
			fallbackLength: 2,
			new TextCompletionTextEdit(
				new TextRange(2, 1),
				new TextRange(2, 3)),
			useReplaceRange: true);

		Assert.AreEqual(2, offset);
		Assert.AreEqual(3, length);
	}

	[TestMethod]
	public void ResolveCompletionSegment_FallsBackWhenTextEditRangeIsInvalid()
	{
		(int offset, int length) = InvokeResolveCompletionSegment(
			new TextDocument("abc"),
			fallbackOffset: 1,
			fallbackLength: 2,
			new TextCompletionTextEdit(
				new TextRange(4, 1)),
			useReplaceRange: false);

		Assert.AreEqual(1, offset);
		Assert.AreEqual(2, length);
	}

	private static (int Offset, int Length) InvokeResolveCompletionSegment(TextDocument document,
		int fallbackOffset,
		int fallbackLength,
		TextCompletionTextEdit? textEdit,
		bool useReplaceRange)
	{
		MethodInfo method = typeof(CompletionData).GetMethod(
			"ResolveCompletionSegment",
			BindingFlags.NonPublic | BindingFlags.Static,
			binder: null,
			[typeof(TextDocument), typeof(int), typeof(int), typeof(TextCompletionTextEdit?), typeof(bool)],
			modifiers: null)
			?? throw new InvalidOperationException("Private static method 'ResolveCompletionSegment' was not found.");

		return ((int Offset, int Length))(method.Invoke(null, [document, fallbackOffset, fallbackLength, textEdit, useReplaceRange])
			?? throw new InvalidOperationException("Private static method 'ResolveCompletionSegment' returned null."));
	}
}

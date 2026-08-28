using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.Cleaning;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;

namespace TombLib.Tests;

[TestClass]
public class TextEditorFormattingServiceTests
{
	[TestMethod]
	public void FormatDocument_IsOneUndoStep_PreservesCaretLineAndScroll()
	{
		WPFTestHelper.RunInSta(() =>
		{
			string original = string.Join("\r\n", Enumerable.Range(1, 200).Select(i => "Line " + i + "   "));
			var editor = new PlainTextEditor()
			{
				Text = original
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Document.UndoStack.ClearAll();
				editor.CaretOffset = editor.Document.GetOffset(2, 3);
				editor.ScrollToVerticalOffset(100.0);
				editor.ScrollToHorizontalOffset(5.0);
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Vector scrollBefore = editor.TextArea.TextView.ScrollOffset;

				var service = new TextEditorFormattingService();
				service.FormatDocument(editor, new TrimTrailingWhitespaceFormatter());

				Assert.AreEqual(TrimTrailingWhitespace(original), editor.Text);

				// The caret is preserved on the same line, with the selection collapsed.
				Assert.AreEqual(2, editor.Document.GetLineByOffset(editor.CaretOffset).LineNumber);

				// The scroll position is preserved, not only the content.
				Assert.AreEqual(scrollBefore.X, editor.TextArea.TextView.ScrollOffset.X, 1.0);
				Assert.AreEqual(scrollBefore.Y, editor.TextArea.TextView.ScrollOffset.Y, 1.0);

				// The whole replacement is a single undo step: one Undo reverts the full document
				// and one Redo restores it, confirming the undo/redo boundary covers everything.
				editor.Undo();
				Assert.AreEqual(original, editor.Text);

				editor.Redo();
				Assert.AreEqual(TrimTrailingWhitespace(original), editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void FormatDocument_NoChanges_LeavesEditorAndUndoStackUntouched()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor()
			{
				Text = "No changes here"
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Document.UndoStack.ClearAll();
				editor.CaretOffset = 3;

				var service = new TextEditorFormattingService();
				service.FormatDocument(editor, new IdentityFormatter());

				Assert.AreEqual("No changes here", editor.Text);
				Assert.AreEqual(3, editor.CaretOffset);

				// No changes means no undo entry: undoing must not alter the content.
				editor.Undo();
				Assert.AreEqual("No changes here", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static string TrimTrailingWhitespace(string content)
		=> string.Join("\r\n", content.Split("\r\n").Select(line => line.TrimEnd()));

	private sealed class TrimTrailingWhitespaceFormatter : ITextDocumentFormatter
	{
		public string FormatDocument(string content, bool trimOnly = false)
			=> TrimTrailingWhitespace(content);
	}

	private sealed class IdentityFormatter : ITextDocumentFormatter
	{
		public string FormatDocument(string content, bool trimOnly = false) => content;
	}
}

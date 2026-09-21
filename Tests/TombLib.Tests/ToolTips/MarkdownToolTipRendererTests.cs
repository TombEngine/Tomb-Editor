using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Tests;

[TestClass]
public class MarkdownToolTipRendererTests
{
	[TestMethod]
	public void CreateCodeBlockEditor_DoesNotAcceptKeyboardFocus()
	{
		WPFTestHelper.RunInSta(() =>
		{
			TextEditor editor = MarkdownToolTipRenderer.CreateCodeBlockEditor("lua", "local value = 1", Brushes.White);

			Assert.IsFalse(editor.Focusable);
			Assert.IsFalse(editor.IsTabStop);
			Assert.IsFalse(editor.TextArea.Focusable);
			Assert.IsFalse(editor.TextArea.IsTabStop);
		});
	}
}

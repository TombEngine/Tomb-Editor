using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Windows;
using System.Windows.Controls;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class CompletionWindowToolTipAccessTests
{
	[TestMethod]
	public void TryGetToolTip_ResolvesAvalonEditToolTipWithoutDirectFieldAccess()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(completionWindow);

				bool resolved = CompletionWindowToolTipAccess.TryGetToolTip(completionWindow, out ToolTip? toolTip);

				Assert.IsTrue(resolved);
				Assert.IsNotNull(toolTip);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}
}

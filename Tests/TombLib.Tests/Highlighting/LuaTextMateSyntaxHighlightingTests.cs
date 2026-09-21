using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class LuaTextMateSyntaxHighlightingTests
{
	[TestMethod]
	public void LoadFallbackHighlighting_ReturnsLuaDefinition()
	{
		IHighlightingDefinition? highlighting = LuaTextMateSyntaxHighlighting.LoadFallbackHighlighting();

		Assert.IsNotNull(highlighting);
		Assert.AreEqual("Lua", highlighting.Name);
	}

	[TestMethod]
	public void TryInstall_AttachesTransformerAndDisposeRemovesIt()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TextEditor
			{
				Text = "local value = 1"
			};

			int initialTransformerCount = editor.TextArea.TextView.LineTransformers.Count;

			bool installed = LuaTextMateSyntaxHighlighting.TryInstall(editor, out LuaTextMateInstallation? installation);

			Assert.IsTrue(installed);
			Assert.IsNotNull(installation);
			Assert.AreEqual(initialTransformerCount + 1, editor.TextArea.TextView.LineTransformers.Count);

			installation.Dispose();
			installation.Dispose();

			Assert.AreEqual(initialTransformerCount, editor.TextArea.TextView.LineTransformers.Count);
		});
	}
}

using System;
using System.IO;
using System.Windows;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.Docking;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class StudioAvalonDockHostViewTests
{
	[TestMethod]
	public void DocumentActivationToolPaneFocusAndLayoutRestorePreserveActiveContext()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var firstFile = new TemporaryFile("first.txt");
			using var secondFile = new TemporaryFile("second.txt");
			var controller = new EditorDocumentController(new Version(1, 0), string.Empty);
			controller.RegisterDocument(new ScriptingDocumentRegistration(
				EditorType.Text,
				DocumentMode.PlainText,
				static _ => false,
				static _ => false,
				_ => new PlainTextEditor(),
				ScriptingDocumentContributions.None,
				isFallback: true));
			var pane = new TestDockPane();
			var host = new StudioAvalonDockHostView(controller, [pane]);

			try
			{
				host.RestoreDefaultLayout(new DockPanelState());
				controller.OpenFile(firstFile.Path);
				IEditorControl firstEditor = controller.CurrentEditor!;
				controller.OpenFile(secondFile.Path);
				IEditorControl secondEditor = controller.CurrentEditor!;

				Assert.AreSame(secondEditor, controller.CurrentDocumentContext.Editor);
				Assert.IsTrue(host.EnsurePane(pane));
				Assert.IsTrue(host.ShowPane(pane));
				Assert.AreSame(secondEditor, controller.CurrentDocumentContext.Editor);

				controller.ActivateEditor(firstEditor);
				Assert.AreSame(firstEditor, controller.CurrentDocumentContext.Editor);

				string layout = host.SaveLayout();
				Assert.IsFalse(string.IsNullOrWhiteSpace(layout));

				host.RestoreLayout(layout, new DockPanelState());
				Assert.AreSame(firstEditor, controller.CurrentDocumentContext.Editor);
			}
			finally
			{
				host.DetachDocumentController();
				pane.Dispose();
				foreach (IEditorControl editor in controller.GetOpenEditors().ToList())
					controller.TryCloseEditor(editor);
			}
		});
	}

	private sealed class TestDockPane : StudioDockPane
	{
		public TestDockPane()
			: base("Test", "TestPane", StudioDockPaneLocation.Bottom, new System.Windows.Size(200, 100))
		{ }

		public override UIElement Content { get; } = new FrameworkElement();
	}

	private sealed class TemporaryFile : IDisposable
	{
		public TemporaryFile(string fileName)
		{
			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TombEditor-{Guid.NewGuid():N}-{fileName}");
			File.WriteAllText(Path, string.Empty);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (File.Exists(Path))
				File.Delete(Path);
		}
	}
}
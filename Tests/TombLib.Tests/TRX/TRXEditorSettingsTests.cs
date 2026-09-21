using System.Windows;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXEditorSettingsTests
{
	private static TRXLanguageServices CreateLanguageServices()
	{
		var lineService = new TRXLineService();
		var documentService = new TRXDocumentService(lineService);
		var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

		return new TRXLanguageServices(
			schemaService,
			lineService,
			documentService,
			new TRXDefinitionProvider(documentService),
			new TRXGameFlowCompletionService(schemaService),
			new TRXGameFlowHoverService(schemaService));
	}

	private sealed class IncompatibleConfiguration : TombLib.Scripting.UI.Bases.ConfigurationBase
	{
		public override string DefaultPath => string.Empty;
	}

	// ---------------------------------------------------------------------------
	// UpdateSettings — incompatible configuration policy
	// ---------------------------------------------------------------------------

	[TestMethod]
	public void UpdateSettings_IncompatibleConfiguration_IsIgnored()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				// Non-TRX configurations are ignored silently, matching the other editors.
				editor.UpdateSettings(new IncompatibleConfiguration());

				Assert.AreEqual("}", editor.BracesClosingString);
				Assert.AreEqual("]", editor.BracketsClosingString);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	// ---------------------------------------------------------------------------
	// UpdateSettings — comma settings
	// ---------------------------------------------------------------------------

	[TestMethod]
	public void UpdateSettings_AutoAddCommas_UpdatesClosingStrings()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var config = new TRXEditorConfiguration { AutoAddCommas = true };
				editor.UpdateSettings(config);

				Assert.AreEqual("},", editor.BracesClosingString);
				Assert.AreEqual("],", editor.BracketsClosingString);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateSettings_NoAutoCommas_UsesPlainClosingStrings()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var config = new TRXEditorConfiguration { AutoAddCommas = false };
				editor.UpdateSettings(config);

				Assert.AreEqual("}", editor.BracesClosingString);
				Assert.AreEqual("]", editor.BracketsClosingString);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	// ---------------------------------------------------------------------------
	// Bracket autospacing — configured indentation policy
	// ---------------------------------------------------------------------------

	[TestMethod]
	public void BracketAutospacing_TabMode_InsertsTab()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "{}"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 1;
				editor.TextArea.PerformTextInput("\n");

				Assert.AreEqual("{\r\n\r\n\t}", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void BracketAutospacing_SpacesMode_UsesConfiguredIndentationSize()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[]"
			};
			editor.Options.ConvertTabsToSpaces = true;
			editor.Options.IndentationSize = 4;
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 1;
				editor.TextArea.PerformTextInput("\n");

				Assert.AreEqual("[\r\n\r\n    ]", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

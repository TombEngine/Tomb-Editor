using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Tests;

[TestClass]
public class Tomb1MainEditorToolTipTests
{
	[TestMethod]
	public void ShowMarkdownToolTip_OpensPopupAndForceCloseClearsContent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());
			var lineService = new TRXLineService();
			var documentService = new TRXDocumentService(lineService);

			var languageServices = new TRXLanguageServices(
				schemaService,
				lineService,
				documentService,
				new TRXDefinitionProvider(documentService),
				new TRXGameFlowCompletionService(schemaService),
				new TRXGameFlowHoverService(schemaService));
			var editor = new TRXEditor(new Version(1, 0), languageServices);
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.ShowMarkdownToolTip("`Level` tooltip");
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Popup popup = WPFTestHelper.GetPrivateField<Popup>(editor, "_specialToolTip");
				ContentPresenter presenter = WPFTestHelper.GetPrivateField<EditorToolTipPresenter>(editor, "_toolTipPresenter").ContentPresenter;

				Assert.IsTrue(popup.IsOpen);
				Assert.IsNotNull(presenter.Content);

				WPFTestHelper.InvokeInstanceMethod(editor, "CloseDefinitionToolTip", [typeof(bool)], true);

				Assert.IsFalse(popup.IsOpen);
				Assert.IsNull(presenter.Content);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

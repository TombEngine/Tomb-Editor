using Nickelony.IDEKit.AvalonEdit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptDefinitionNavigationTests
{
	private static ClassicScriptLanguageServices CreateLanguageServices()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
		var indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
		var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

		return new ClassicScriptLanguageServices(
			new ClassicScriptDefinitionProvider(commandService),
			new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
			new ClassicScriptSignatureHelpProvider(commandService),
			errorDetector,
			lineService,
			commandService,
			indexService);
	}

	[TestMethod]
	public void DefinitionProvider_RequiresClassicScriptDiscriminator()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
		var definitionProvider = new ClassicScriptDefinitionProvider(commandService);
		const string document = "[Level]\nName=Level1";

		Assert.IsNull(definitionProvider.GetDefinition(new TextDefinitionRequest(document, "[Level]")));
		Assert.IsNull(definitionProvider.GetDefinition(new TextDefinitionRequest(document, "[Level]", new UnrecognizedDiscriminator())));

		TextDefinitionLocation? location = definitionProvider.GetDefinition(
			new TextDefinitionRequest(document, "[Level]", new ClassicScriptObjectDiscriminator(ObjectType.Section)));

		Assert.IsNotNull(location);
		Assert.AreEqual(1, location!.LineNumber);
	}

	private sealed record UnrecognizedDiscriminator : TextDefinitionDiscriminator;

	[TestMethod]
	public void TryHandleKeyDownAsync_F12OnSectionHeader_MovesToDefinition()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "[Level]\r\nName=Level1"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 1;

				var controller = WPFTestHelper.GetPrivateField<TextDefinitionTriggerController>(editor, "_definitionTriggerController");
				KeyEventArgs eventArgs = CreateF12KeyEventArgs(hostWindow);

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, editor.CaretOffset).GetAwaiter().GetResult();

				Assert.IsTrue(handled);
				Assert.IsTrue(eventArgs.Handled);
				Assert.AreEqual("[Level]", editor.SelectedText);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_F12OnWordWithoutDefinition_DoesNotNavigate()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "Name=Level1"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.CaretOffset = 6;

				var controller = WPFTestHelper.GetPrivateField<TextDefinitionTriggerController>(editor, "_definitionTriggerController");
				KeyEventArgs eventArgs = CreateF12KeyEventArgs(hostWindow);

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, editor.CaretOffset).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static KeyEventArgs CreateF12KeyEventArgs(Window hostWindow)
		=> new(Keyboard.PrimaryDevice, PresentationSource.FromVisual(hostWindow)!, 0, Key.F12)
		{
			RoutedEvent = Keyboard.KeyDownEvent
		};
}

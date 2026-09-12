using ICSharpCode.AvalonEdit.CodeCompletion;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Completion;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptEditorCompletionWindowTests
{
	private static readonly ITextCompletionProvider CompletionProvider = CreateCompletionProvider();

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

	private static ITextCompletionProvider CreateCompletionProvider()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

		return new ClassicScriptCompletionProvider(commandService, mnemonicCatalogService);
	}

	[TestMethod]
	public void EmptyLineCompletion_ItemsExposeKindDetailText()
	{
		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext(string.Empty, 0, TextCompletionTrigger.EmptyLine));

		Assert.AreEqual("Old Command", completionItems.First(item => item.Kind.Identifier == "OldCommand").Detail);
		Assert.AreEqual("New Command", completionItems.First(item => item.Kind.Identifier == "NewCommand").Detail);
		Assert.AreEqual("Section", completionItems.First(item => item.Kind == TextCompletionItemKind.Section).Detail);
		Assert.AreEqual("Directive", completionItems.First(item => item.Kind == TextCompletionItemKind.Directive).Detail);
	}

	[TestMethod]
	public void EmptyLineCompletion_CommandItemsComeFromCatalog()
	{
		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext(string.Empty, 0, TextCompletionTrigger.EmptyLine));

		// FMV (one of the four entries absent from the legacy new-command array) is a new command.
		Assert.IsTrue(completionItems.Any(item => item.Label.Equals("FMV", StringComparison.Ordinal)
			&& item.Kind.Identifier == "NewCommand"));

		// Legacy array-only old-command names are kept.
		Assert.IsTrue(completionItems.Any(item => item.Label.Equals("Cut", StringComparison.Ordinal)
			&& item.Kind.Identifier == "OldCommand"));

		// Directives are offered with a trailing space and are not duplicated with '='.
		Assert.IsFalse(completionItems.Any(item => item.Label.StartsWith("#DEFINE=", StringComparison.Ordinal)));
		Assert.IsTrue(completionItems.Any(item => item.Kind == TextCompletionItemKind.Directive
			&& item.Label.StartsWith("#DEFINE ", StringComparison.Ordinal)));
	}

	[TestMethod]
	public void ContextualCompletion_HorizonEquals_OffersEnabledDisabled()
	{
		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext("Horizon= ", 9, TextCompletionTrigger.Contextual, -1));

		Assert.IsTrue(completionItems.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(completionItems.Any(item => item.Label == "DISABLED"));
	}

	[TestMethod]
	public void WordCompletion_CaretAtStart_ReturnsNoItems()
	{
		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext("CUST_", 0, TextCompletionTrigger.Word));

		Assert.AreEqual(0, completionItems.Count);
	}

	[TestMethod]
	public void EmptyLineCompletion_OpensCompletionWindowAtLineOffset()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var languageServices = CreateLanguageServices();
			var editor = new ClassicScriptEditor(new Version(1, 0), languageServices)
			{
				Text = string.Empty
			};

			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				CompletionData[] completionItems = [..
					CompletionProvider.GetCompletionItems(
						new TextCompletionContext(editor.Text, editor.CaretOffset, TextCompletionTrigger.EmptyLine))
						.Select(item => new CompletionData(item))];
				int lineOffset = editor.Document.GetLineByOffset(editor.CaretOffset).Offset;

				var completionController = (TextCompletionController)WPFTestHelper.InvokeInstanceMethod(
					editor,
					"get_CompletionController",
					Type.EmptyTypes)
					?? throw new InvalidOperationException("Completion controller was not found.");

				bool opened = completionController.OpenOrRefresh(completionItems, lineOffset);

				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;

				Assert.IsTrue(opened);
				Assert.IsNotNull(completionWindow);
				Assert.IsTrue(completionWindow.CompletionList.CompletionData.Count > 0);
				Assert.AreEqual(0, completionWindow.StartOffset);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}
}

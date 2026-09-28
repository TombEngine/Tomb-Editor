using Nickelony.IDEKit.IntelliSense.Completion;
using System.Threading.Tasks;
using System.Windows;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

// Phase 1 completion-fault tests: prove a faulted completion-decision task is observed and
// logged instead of escaping the event boundary as an unobserved task exception.
[TestClass]
public class ClassicScriptCompletionFaultTests
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
	public void ApplyCompletionDecisionAsync_FaultedDecisionTask_DoesNotEscapeException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices())
			{
				Text = "Rain=ENABLED"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var faultedTask = Task.FromException<TextCompletionSessionDecision>(new InvalidOperationException("completion provider failed"));

				var decisionTask = WPFTestHelper.InvokeInstanceMethod(
					editor,
					"ApplyCompletionDecisionAsync",
					[typeof(Task<TextCompletionSessionDecision>), typeof(string), typeof(int), typeof(int)],
					faultedTask,
					editor.Text,
					editor.CaretOffset,
					0) as Task;

				Assert.IsNotNull(decisionTask);

				// The fault must be observed inside the handler; awaiting must not rethrow.
				decisionTask!.GetAwaiter().GetResult();

				// The editor remains usable after the faulted decision.
				Assert.AreEqual("Rain=ENABLED", editor.Text);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}

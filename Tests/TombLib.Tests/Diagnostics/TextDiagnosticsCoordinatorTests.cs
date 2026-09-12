using Nickelony.IDEKit.Core.Infrastructure;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Tests;

[TestClass]
public class TextDiagnosticsCoordinatorTests
{
	[TestMethod]
	public void ErrorDetectionWorker_SurfacesProviderFailureThroughCompletedEvent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(new ThrowingDiagnosticsProvider(), new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			RunWorkerCompletedEventArgs? completedArgs = null;
			worker.RunWorkerCompleted += (_, e) => completedArgs = e;

			worker.RunErrorCheck("content");

			PumpUntil(() => completedArgs is not null);

			Assert.IsNotNull(completedArgs);
			Assert.IsNotNull(completedArgs.Error);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	public void ErrorDetectionWorker_NewerRequestWhileBusy_CoalescesToLatestContent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var detector = new SlowDetector(blockFirstCalls: 1);
			var worker = new ErrorDetectionWorker(detector, new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			var published = new List<RunWorkerCompletedEventArgs>();

			worker.RunWorkerCompleted += (_, e) => published.Add(e);

			worker.RunErrorCheck("first");
			PumpUntil(() => detector.CallCount >= 1);
			Assert.IsTrue(worker.IsBusy);

			// Requests issued while a run is active must not overlap; they coalesce to the latest content.
			worker.RunErrorCheck("second");
			worker.RunErrorCheck("third");
			detector.Release();

			PumpUntil(() => !worker.IsBusy && detector.CallCount >= 2);

			Assert.AreEqual(2, detector.CallCount);
			Assert.AreEqual("third", detector.Contents[detector.Contents.Count - 1]);
			Assert.AreEqual(2, published.Count);
		});
	}

	[TestMethod]
	public void ErrorDetectionWorker_DisposeWhileBusy_CancelsInFlightAndRaisesNoCompletion()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var detector = new SlowDetector(blockFirstCalls: 1);
			var worker = new ErrorDetectionWorker(detector, new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			int completedCount = 0;

			worker.RunWorkerCompleted += (_, _) => completedCount++;

			worker.RunErrorCheck("content");
			PumpUntil(() => detector.CallCount >= 1);
			Assert.IsTrue(worker.IsBusy);

			worker.Dispose();
			detector.Release();
			PumpUntil(() => !worker.IsBusy);

			Assert.AreEqual(0, completedCount);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	public void Coordinator_ResetWhileBusy_CancelsInFlightAndRaisesNoCompletion()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("content");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				coordinator.Reset();
				detector.Release();
				PumpUntil(() => !coordinator.IsBusy);

				Assert.IsFalse(coordinator.IsBusy);
				Assert.AreEqual(0, editor.Diagnostics.Count);
				coordinator.Dispose();
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Coordinator_KeepsLastKnownDiagnostics_WhenProviderThrows()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var initialDiagnostics = new[] { new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "existing", 0, 4) };
				editor.SetDiagnostics(initialDiagnostics);

				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), new ThrowingDiagnosticsProvider());
				coordinator.RunErrorCheck("Name=Level1");

				PumpUntil(() => !coordinator.IsBusy);

				// The failing detector must not replace the editor's last-known diagnostics.
				Assert.AreEqual(1, editor.Diagnostics.Count);
				Assert.AreEqual("existing", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunErrorCheck_WhenBusy_RetainsAndRunsLatestPendingRequest()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);
				Assert.AreEqual(1, detector.CallCount);

				// A check issued while the first is active must be retained, not dropped.
				coordinator.RunErrorCheck("second");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("second", detector.Contents[detector.Contents.Count - 1]);
				Assert.AreEqual(1, editor.Diagnostics.Count);
				Assert.AreEqual("result:second", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunErrorCheck_MultipleQueuedEdits_LatestPendingRequestWins()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("second");
				coordinator.RunErrorCheck("third");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				// Only one follow-up check runs, for the latest retained content.
				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("third", detector.Contents[detector.Contents.Count - 1]);
				Assert.AreEqual("result:third", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunErrorCheck_DetectorFailure_ThenNewerSuccessfulRequestPublishes()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new FailOnceBlockingDetector();
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("second");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("result:second", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_WhileBusy_CancelsPendingAndStopsCallbacks()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("pending");
				coordinator.Dispose();

				// Post-dispose requests are no-ops and never wedge the worker.
				coordinator.RunErrorCheck("after-dispose");
				detector.Release();
				PumpUntil(() => !coordinator.IsBusy);

				Assert.IsFalse(coordinator.IsBusy);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_IsIdempotent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), new SlowDetector(blockFirstCalls: 0));

				coordinator.Dispose();
				coordinator.Dispose();
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void ProcessingSuppressed_WhileDetectionActive_IgnoresCompletedResult()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("content");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				using IDisposable suppressedScope = editor.BeginProcessingScope(EditorProcessingMode.Suppressed);
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy);

				// The in-flight request completed while silent, so its result must not replace editor diagnostics.
				Assert.IsFalse(coordinator.IsBusy);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void ProcessingSuppressed_WhileDetectionPending_DropsQueuedCheck()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				coordinator.RunErrorCheck("second");
				using IDisposable suppressedScope = editor.BeginProcessingScope(EditorProcessingMode.Suppressed);
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy);

				// The queued check must not start while silent, and the completed first result must be discarded.
				Assert.AreEqual(1, detector.CallCount);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void ProcessingSuppressed_WhileIdleTimerPending_DoesNotStartDetection()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 0);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector, idleDelayInterval: TimeSpan.FromMilliseconds(20.0));

				coordinator.RunOnIdle("content");
				using IDisposable suppressedScope = editor.BeginProcessingScope(EditorProcessingMode.Suppressed);

				PumpFor(TimeSpan.FromMilliseconds(200.0));

				// The pending idle timer must not start a check after the editor entered a silent session.
				Assert.AreEqual(0, detector.CallCount);
				Assert.IsFalse(coordinator.IsBusy);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_NormalCompletion_ClassifiesCompleted()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(new SlowDetector(blockFirstCalls: 0), new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			RunWorkerCompletedEventArgs? completedArgs = null;
			worker.RunWorkerCompleted += (_, e) => completedArgs = e;

			worker.RunErrorCheck("content");

			PumpUntil(() => completedArgs is not null);

			Assert.AreEqual(TextEditorRequestOutcome.Completed, worker.LastRequestOutcome);
			Assert.IsNotNull(completedArgs);
			Assert.IsNull(completedArgs.Error);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_ProviderFailure_ClassifiesFailed()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(new ThrowingDiagnosticsProvider(), new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			RunWorkerCompletedEventArgs? completedArgs = null;
			worker.RunWorkerCompleted += (_, e) => completedArgs = e;

			worker.RunErrorCheck("content");

			PumpUntil(() => completedArgs is not null);

			// Provider failure is classified as failed and surfaced through the completion event.
			Assert.AreEqual(TextEditorRequestOutcome.Failed, worker.LastRequestOutcome);
			Assert.IsNotNull(completedArgs?.Error);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_ResetWhileBusy_ProviderCompletes_ClassifiesSuperseded()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var detector = new SlowDetector(blockFirstCalls: 1);
			var worker = new ErrorDetectionWorker(detector, new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			int publishedCount = 0;
			worker.RunWorkerCompleted += (_, _) => publishedCount++;

			worker.RunErrorCheck("content");
			PumpUntil(() => detector.CallCount >= 1);
			Assert.IsTrue(worker.IsBusy);

			worker.Reset();
			detector.Release();
			PumpUntil(() => !worker.IsBusy);

			// The owner invalidated the request while the provider still completed; the completed
			// result is dropped and classified as superseded rather than published.
			Assert.AreEqual(TextEditorRequestOutcome.Superseded, worker.LastRequestOutcome);
			Assert.AreEqual(0, publishedCount);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_GenerationAdvancedWhileBusy_ClassifiesStaleAndDoesNotPublish()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var detector = new SlowDetector(blockFirstCalls: 1);
			int sessionGeneration = 0;
			var worker = new ErrorDetectionWorker(
				detector,
				new Version(1, 0),
				TimeSpan.FromMilliseconds(50.0),
				sessionGenerationProvider: () => sessionGeneration);
			int publishedCount = 0;
			worker.RunWorkerCompleted += (_, _) => publishedCount++;

			worker.RunErrorCheck("content");
			PumpUntil(() => detector.CallCount >= 1);
			Assert.IsTrue(worker.IsBusy);

			// A load, replace, rename, or disposal boundary advanced the session generation while
			// the run was in flight; the completed result is stale and must not publish.
			sessionGeneration = 1;
			detector.Release();
			PumpUntil(() => !worker.IsBusy);

			Assert.AreEqual(TextEditorRequestOutcome.Stale, worker.LastRequestOutcome);
			Assert.AreEqual(0, publishedCount);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_LogicalDocumentChangedWhileBusy_ClassifiesStaleAndDoesNotPublish()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var detector = new SlowDetector(blockFirstCalls: 1);
			string? logicalDocumentId = @"C:\Scripts\a.cs";
			var worker = new ErrorDetectionWorker(
				detector,
				new Version(1, 0),
				TimeSpan.FromMilliseconds(50.0),
				logicalDocumentIdProvider: () => logicalDocumentId);
			int publishedCount = 0;
			worker.RunWorkerCompleted += (_, _) => publishedCount++;

			worker.RunErrorCheck("content");
			PumpUntil(() => detector.CallCount >= 1);
			Assert.IsTrue(worker.IsBusy);

			// A rename changed the logical document identity while the run was in flight.
			logicalDocumentId = @"C:\Scripts\b.cs";
			detector.Release();
			PumpUntil(() => !worker.IsBusy);

			Assert.AreEqual(TextEditorRequestOutcome.Stale, worker.LastRequestOutcome);
			Assert.AreEqual(0, publishedCount);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void Coordinator_RenameWhileBusy_StaleResultDoesNotReplaceDiagnostics()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("content");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				// A rename advances the session generation and changes the logical document identity.
				editor.FilePath = @"C:\Scripts\renamed.cs";
				detector.Release();
				PumpUntil(() => !coordinator.IsBusy);

				// The in-flight result was computed for the previous document identity, so it must
				// not replace the editor's diagnostics.
				Assert.AreEqual(0, editor.Diagnostics.Count);
				Assert.IsFalse(coordinator.IsBusy);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void ErrorDetectionWorker_CallsAfterDisposal_ThrowObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(new SlowDetector(blockFirstCalls: 0), new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			worker.Dispose();

			// New detection work cannot be admitted after disposal; only already-admitted work
			// completes, with its normal cancellation outcome.
			Assert.ThrowsException<ObjectDisposedException>(() => worker.RunErrorCheck("content"));
			Assert.ThrowsException<ObjectDisposedException>(() => worker.RunErrorCheckOnIdle("content"));
			Assert.ThrowsException<ObjectDisposedException>(() => worker.Reset());
		});
	}

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

	private static void PumpUntil(Func<bool> condition)
	{
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		int attempts = 0;

		while (!condition() && attempts++ < 5000)
			dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
	}

	private static void PumpFor(TimeSpan duration)
	{
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		DateTime deadline = DateTime.Now + duration;

		while (DateTime.Now < deadline)
			dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
	}

	private sealed class ThrowingDiagnosticsProvider : ITextDiagnosticsProvider
	{
		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
			=> throw new InvalidOperationException("Provider failed.");
	}

	private sealed class SlowDetector : ITextDiagnosticsProvider
	{
		private readonly int _blockFirstCalls;
		private readonly ManualResetEventSlim _release = new(false);
		private int _callCount;

		public SlowDetector(int blockFirstCalls)
			=> _blockFirstCalls = blockFirstCalls;

		public List<string> Contents { get; } = new();

		public int CallCount => _callCount;

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
		{
			string editorContent = request.DocumentText;
			int call = Interlocked.Increment(ref _callCount);
			Contents.Add(editorContent);

			if (call <= _blockFirstCalls)
				_release.Wait();

			return [new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, "result:" + editorContent, 0, Math.Max(1, editorContent.Length))];
		}

		public void Release()
			=> _release.Set();
	}

	private sealed class FailOnceBlockingDetector : ITextDiagnosticsProvider
	{
		private readonly ManualResetEventSlim _release = new(false);
		private int _callCount;

		public int CallCount => _callCount;

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
		{
			string editorContent = request.DocumentText;
			int call = Interlocked.Increment(ref _callCount);
			_release.Wait();

			if (call == 1)
				throw new InvalidOperationException("Provider failed.");

			return [new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, "result:" + editorContent, 0, Math.Max(1, editorContent.Length))];
		}

		public void Release()
			=> _release.Set();
	}
}

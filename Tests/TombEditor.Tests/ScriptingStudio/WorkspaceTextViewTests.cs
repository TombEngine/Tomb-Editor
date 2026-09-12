#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using TombIDE.ScriptingStudio.FindAndReplace;
using Nickelony.IDEKit.Workspace.Views;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class WorkspaceTextViewTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	private TestContext? _testContext;

	public TestContext TestContext
	{
		get => _testContext ?? throw new InvalidOperationException("The MSTest context is not initialized.");
		set => _testContext = value;
	}

	[TestMethod]
	public void OpenedView_CreatesBackupOnEditAndRemovesItAfterUndo()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			string backupPath = filePath + ".backup";

			try
			{
				using var editor = new PlainTextEditor();
				var view = new TextEditorWorkspaceView(editor, new AvalonEditWorkspaceViewHostAdapter(editor));
				view.Open(CreateSnapshot(filePath, "initial", version: 1));

				view.Apply([new TextEditOperation(0, "initial".Length, "changed", 0)]);

				Assert.IsTrue(
					WaitForFileState(backupPath, expectedExists: true),
					"The attached view did not create its backup sidecar.");
				Assert.AreEqual("changed", File.ReadAllText(backupPath));

				editor.Undo();

				Assert.IsTrue(
					WaitForFileState(backupPath, expectedExists: false),
					"Undo did not remove the backup sidecar.");
			}
			finally
			{
				File.Delete(filePath);
				File.Delete(backupPath);
			}
		});

	[TestMethod]
	public void ControllerOpen_AttachesBeforeRegistrationAndDoesNotLoadDiskContent()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "disk");

			try
			{
			var manager = new TestDocumentBridge("canonical");
			var controller = new EditorDocumentController(
				new Version(1, 0),
				string.Empty,
				messageService: null,
				documentManager: manager);
			controller.RegisterDocument(new ScriptingDocumentRegistration(
				EditorType.Text,
				DocumentMode.PlainText,
				static _ => true,
				static _ => true,
				static _ => new PlainTextEditor(),
				ScriptingDocumentContributions.None));

			controller.OpenFile(filePath);

			var editor = (TextEditorBase)controller.CurrentEditor!;
			Assert.AreEqual(string.Empty, manager.ContentBeforeAttach);
			Assert.AreEqual("canonical", editor.Text);
			Assert.AreEqual(1, controller.GetOpenEditors().Count());
			Assert.AreEqual(1, manager.OpenCount);
			controller.TryCloseEditor(editor);
			}
			finally
			{
				File.Delete(filePath);
			}
		});

	[TestMethod]
	public void ControllerOpen_AttachFailureLeavesNoRegisteredEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "disk");

			try
			{
			var manager = new TestDocumentBridge("canonical") { FailAttach = true };
			var controller = new EditorDocumentController(
				new Version(1, 0),
				string.Empty,
				messageService: null,
				documentManager: manager);
			controller.RegisterDocument(new ScriptingDocumentRegistration(
				EditorType.Text,
				DocumentMode.PlainText,
				static _ => true,
				static _ => true,
				static _ => new PlainTextEditor(),
				ScriptingDocumentContributions.None));

			controller.OpenFile(filePath);

			Assert.IsNull(controller.CurrentEditor);
			Assert.AreEqual(0, controller.GetOpenEditors().Count());
			Assert.AreEqual(1, manager.OpenCount);
			}
			finally
			{
				File.Delete(filePath);
			}
		});

	[TestMethod]
	public void FindReplace_UsesAttachedTargetAndPublishesOneMutation()
		=> StaTestHelper.RunInSta(() =>
		{
			using var editor = new PlainTextEditor();
			var view = new TextEditorWorkspaceView(editor, new AvalonEditWorkspaceViewHostAdapter(editor));
			var requests = new List<WorkspaceDocumentReplaceRequest>();
			view.ApplyRequested += (_, args) => requests.Add(args.Request);
			WorkspaceDocumentSnapshot initial = CreateSnapshot("script.txt", "old value", version: 4);
			view.Open(initial);

			var controller = new Mock<IEditorDocumentController>();
			controller.SetupGet(value => value.CurrentEditor).Returns(editor);
			controller.Setup(value => value.GetOpenEditors()).Returns([editor]);
			var viewModel = new FindAndReplaceViewModel(
				controller.Object,
				new WeakReferenceMessenger(),
				new FindReplaceService())
			{
				FindText = "old",
				ReplaceText = "new"
			};

			viewModel.ReplaceCommand.Execute(null);

			Assert.AreEqual("new value", editor.Text);
			Assert.AreEqual(1, requests.Count);
			Assert.AreEqual(initial.DocumentKey, requests[0].ExpectedDocumentKey);
			Assert.AreEqual(initial.Version, requests[0].ExpectedVersion);
			Assert.AreEqual("new value", requests[0].Content);
		});

	[TestMethod]
	public void PublicationBenchmark_OneMiBInsertUndoCycles()
		=> StaTestHelper.RunInSta(() =>
		{
			const int documentLength = 1024 * 1024;
			const int warmupCycles = 10;
			const int measuredCycles = 100;

			using var editor = new PlainTextEditor();
			using IDisposable suppressedScope = editor.BeginProcessingScope(EditorProcessingMode.Suppressed);
			editor.Document.Text = new string('a', documentLength);
			editor.Document.UndoStack.ClearAll();
			var view = new TextEditorWorkspaceView(editor, new AvalonEditWorkspaceViewHostAdapter(editor));
			long version = 1;
			view.ApplyRequested += (_, args) =>
			{
				WorkspaceDocumentReplaceRequest request = args.Request;
				WorkspaceDocumentSnapshot snapshot = CreateSnapshot(
					"benchmark.txt",
					request.Content,
					++version,
					documentKey: request.ExpectedDocumentKey);
				view.AcknowledgeApply(new WorkspaceDocumentMutationResult(
					WorkspaceDocumentMutationStatus.Replaced,
					request.ExpectedDocumentKey,
					request.DocumentId,
					request.ExpectedVersion,
					snapshot));
			};

			view.Open(CreateSnapshot(
				"benchmark.txt",
				new string('a', documentLength),
				version));

			for (int cycle = 0; cycle < warmupCycles; cycle++)
				RunPublicationCycle(view, editor);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var elapsedTicks = new long[measuredCycles];
			long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
			for (int cycle = 0; cycle < measuredCycles; cycle++)
			{
				long start = Stopwatch.GetTimestamp();
				RunPublicationCycle(view, editor);
				elapsedTicks[cycle] = Stopwatch.GetTimestamp() - start;
			}
			long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

			Array.Sort(elapsedTicks);
			double tickFrequency = Stopwatch.Frequency;
			double medianMilliseconds = elapsedTicks[measuredCycles / 2] * 1000.0 / tickFrequency;
			double p95Milliseconds = elapsedTicks[(int)Math.Ceiling(measuredCycles * 0.95) - 1] * 1000.0 / tickFrequency;
			double allocationPerCycle = (allocatedAfter - allocatedBefore) / (double)measuredCycles;

			TestContext.WriteLine(
				$"Publication benchmark: runtime={RuntimeInformation.FrameworkDescription}; "
				+ $"os={RuntimeInformation.OSDescription}; processors={Environment.ProcessorCount}; "
				+ $"documentBytes={documentLength}; cycles={measuredCycles}; "
				+ $"medianMs={medianMilliseconds:F2}; p95Ms={p95Milliseconds:F2}; "
				+ $"managedAllocationPerCycle={allocationPerCycle / (1024 * 1024):F2}MiB");

			Assert.IsTrue(p95Milliseconds <= 50.0, $"The benchmark p95 was {p95Milliseconds:F2} ms.");
			Assert.IsTrue(allocationPerCycle <= 8.0 * 1024 * 1024,
				$"The benchmark allocated {allocationPerCycle / (1024 * 1024):F2} MiB per cycle.");
			Assert.AreEqual(documentLength, editor.Document.TextLength);
		}, TimeSpan.FromMinutes(2));

	private static WorkspaceDocumentSnapshot CreateSnapshot(
		string fileName,
		string content,
		long version,
		WorkspaceDocumentKey? documentKey = null)
	{
		return new WorkspaceDocumentSnapshot(
			documentKey ?? new WorkspaceDocumentKey(Guid.NewGuid()),
			fileName,
			fileName,
			version,
			version,
			false,
			new StringTextSnapshot(content, fileName),
			FileFormat,
			FileStamp.Missing);
	}

	private static void RunPublicationCycle(
		TextEditorWorkspaceView view,
		PlainTextEditor editor)
	{
		int insertionOffset = editor.Document.TextLength;
		view.Apply([new TextEditOperation(insertionOffset, insertionOffset, "x", 0)]);
		editor.Undo();
	}

	private static bool WaitForFileState(string filePath, bool expectedExists)
	{
		var frame = new DispatcherFrame();
		var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25.0) };
		DateTime deadline = DateTime.UtcNow.AddSeconds(5.0);
		bool result = false;
		timer.Tick += OnTimerTick;
		timer.Start();
		Dispatcher.PushFrame(frame);
		return result;

		void OnTimerTick(object? sender, EventArgs e)
		{
			bool stateMatches = File.Exists(filePath) == expectedExists;
			if (!stateMatches && DateTime.UtcNow < deadline)
				return;

			result = stateMatches;
			timer.Stop();
			timer.Tick -= OnTimerTick;
			frame.Continue = false;
		}
	}

	private sealed class TestDocumentBridge : IWorkspaceDocumentManager
	{
		private readonly string _content;

		public TestDocumentBridge(string content)
		{
			_content = content;
		}

		public int OpenCount { get; private set; }

		public bool FailAttach { get; init; }

		public string? ContentBeforeAttach { get; private set; }

		public Task<WorkspaceDocumentOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public IReadOnlyList<WorkspaceDocumentSnapshot> GetSnapshotsUnderDirectory(string directoryPath)
			=> [];

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(OpenWithView(filePath, options, view, cancellationToken));

		public WorkspaceDocumentManagerOpenResult OpenWithView(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
		{
			OpenCount++;
			ContentBeforeAttach = view.Text;
			WorkspaceDocumentViewOpenResult attach = view.Open(CreateSnapshot(
				filePath ?? string.Empty,
				_content,
				version: 1));
			if (FailAttach || attach.Status != WorkspaceDocumentViewOpenStatus.Opened)
				return new WorkspaceDocumentManagerOpenResult(WorkspaceDocumentManagerOpenStatus.OpenFailed, null);

			return new WorkspaceDocumentManagerOpenResult(
				WorkspaceDocumentManagerOpenStatus.Opened,
				CreateSnapshot(filePath ?? string.Empty, _content, version: 1));
		}

		public WorkspaceDocumentMutationResult Replace(WorkspaceDocumentReplaceRequest request)
			=> throw new NotSupportedException();

		public WorkspaceDocumentMutationResult Discard(WorkspaceDocumentDiscardRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentRenameResult> RenameAsync(
			WorkspaceDocumentRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentSaveAsResult> SaveAsAsync(
			WorkspaceDocumentSaveAsRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDeleteResult> DeleteAsync(
			WorkspaceDocumentDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryRenameResult> RenameDirectoryAsync(
			WorkspaceDocumentDirectoryRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryDeleteResult> DeleteDirectoryAsync(
			WorkspaceDocumentDirectoryDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public void UnregisterOpenView(IWorkspaceDocumentView view) { }

		public Task StopAsync() => Task.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}

}

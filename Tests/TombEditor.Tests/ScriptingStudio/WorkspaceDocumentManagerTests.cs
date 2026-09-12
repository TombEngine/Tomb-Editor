#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Views;
using TombIDE.Shared.Messaging;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class WorkspaceDocumentManagerTests
{
	private static readonly TextFileFormat DefaultFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	private static readonly WorkspaceDocumentOpenOptions OpenOptions = new(
		TextEncodingKind.Utf8,
		DefaultFormat);

	[TestMethod]
	public async Task OpenWithView_OpensUnloadedViewBeforeRegistration()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("text");

		WorkspaceDocumentManagerOpenResult result = await manager.OpenWithViewAsync(
			"script.lua",
			OpenOptions,
			view);

		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.Opened, result.Status);
		Assert.IsTrue(view.WasUnloadedWhenAttached);
		Assert.AreEqual(1, view.OpenCount);
		Assert.AreEqual(1, view.PublishSubscriberCount);
		Assert.AreEqual(1, fileSystem.ReadCount);
	}

	[TestMethod]
	public async Task OpenWithView_RejectsLoadedPendingAndAlreadyRegisteredViews()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);

		var pendingView = new TestView("pending") { HasPendingEdits = true };
		WorkspaceDocumentManagerOpenResult pending = await manager.OpenWithViewAsync(
			"pending.lua",
			OpenOptions,
			pendingView);

		var loadedView = new TestView("loaded")
		{
			InitialDocumentKey = new WorkspaceDocumentKey(Guid.NewGuid())
		};
		WorkspaceDocumentManagerOpenResult loaded = await manager.OpenWithViewAsync(
			"loaded.lua",
			OpenOptions,
			loadedView);

		var attachedView = new TestView("attached");
		WorkspaceDocumentManagerOpenResult attached = await manager.OpenWithViewAsync(
			"attached.lua",
			OpenOptions,
			attachedView);
		WorkspaceDocumentManagerOpenResult duplicate = await manager.OpenWithViewAsync(
			"attached.lua",
			OpenOptions,
			attachedView);

		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.ViewInConflictState, pending.Status);
		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.ViewInConflictState, loaded.Status);
		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.Opened, attached.Status);
		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.AlreadyOpen, duplicate.Status);
		Assert.AreEqual(1, fileSystem.ReadCount);
		Assert.AreEqual(0, pendingView.OpenCount);
		Assert.AreEqual(0, loadedView.OpenCount);
		Assert.AreEqual(1, attachedView.OpenCount);
	}

	[TestMethod]
	public async Task FailedOpen_ClosesPartialViewAndDoesNotRegisterIt()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("failed")
		{
			AttachStatus = WorkspaceDocumentViewOpenStatus.Unavailable
		};

		WorkspaceDocumentManagerOpenResult result = await manager.OpenWithViewAsync(
			"failed.lua",
			OpenOptions,
			view);

		Assert.AreEqual(WorkspaceDocumentManagerOpenStatus.OpenFailed, result.Status);
		Assert.AreEqual(1, view.DetachCount);
		Assert.AreEqual(0, view.PublishSubscriberCount);
	}

	[TestMethod]
	public async Task Replace_AcknowledgesSourceBeforePeerAndRecoversFailedPeer()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		var actions = new List<string>();
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var source = new TestView("source", actions);
		var peer = new TestView("peer", actions) { ThrowOnRefresh = true };

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync(
			"script.lua",
			OpenOptions,
			source)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, peer);

		source.RaiseApply(new WorkspaceDocumentReplaceRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			"changed",
			initial.FileFormat));

		Assert.AreEqual("source.ack", actions[0]);
		Assert.AreEqual("peer.refresh", actions[1]);

		WorkspaceDocumentSnapshot changed = GetSnapshot(store, initial.DocumentId);
		WorkspaceDocumentCommitResult blocked = await manager.CommitAsync(new WorkspaceDocumentCommitRequest(
			changed.DocumentKey,
			changed.DocumentId,
			changed.Version,
			changed.OnDiskStamp));

		Assert.AreEqual(WorkspaceDocumentCommitStatus.ViewNotSynchronized, blocked.Status);
		CollectionAssert.AreEqual(new[] { "peer" }, blocked.BlockingViewIds!.ToArray());
		Assert.AreEqual(0, fileSystem.CaptureStampCount);

		peer.ThrowOnRefresh = false;
		WorkspaceDocumentMutationResult refreshed = manager.Replace(new WorkspaceDocumentReplaceRequest(
			changed.DocumentKey,
			changed.DocumentId,
			changed.Version,
			"changed again",
			changed.FileFormat));

		Assert.AreEqual(WorkspaceDocumentMutationStatus.Replaced, refreshed.Status);
		WorkspaceDocumentSnapshot current = refreshed.Snapshot!;
		WorkspaceDocumentCommitResult committed = await manager.CommitAsync(new WorkspaceDocumentCommitRequest(
			current.DocumentKey,
			current.DocumentId,
			current.Version,
			current.OnDiskStamp));

		Assert.AreEqual(WorkspaceDocumentCommitStatus.Committed, committed.Status);
		Assert.AreEqual(1, fileSystem.CaptureStampCount);
		Assert.AreEqual(2, peer.RefreshCount);
	}

	[TestMethod]
	public async Task ThrowingAcknowledgement_BlocksCommitUntilLaterRefreshSucceeds()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var source = new TestView("source") { ThrowOnAcknowledge = true };
		var peer = new TestView("peer");

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync(
			"script.lua",
			OpenOptions,
			source)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, peer);

		source.RaiseApply(new WorkspaceDocumentReplaceRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			"changed",
			initial.FileFormat));

		WorkspaceDocumentSnapshot changed = GetSnapshot(store, initial.DocumentId);
		WorkspaceDocumentCommitResult blocked = await manager.CommitAsync(new WorkspaceDocumentCommitRequest(
			changed.DocumentKey,
			changed.DocumentId,
			changed.Version,
			changed.OnDiskStamp));

		Assert.AreEqual(WorkspaceDocumentCommitStatus.ViewNotSynchronized, blocked.Status);
		CollectionAssert.AreEqual(new[] { "source" }, blocked.BlockingViewIds!.ToArray());

		source.ThrowOnAcknowledge = false;
		WorkspaceDocumentMutationResult refreshed = manager.Replace(new WorkspaceDocumentReplaceRequest(
			changed.DocumentKey,
			changed.DocumentId,
			changed.Version,
			"changed again",
			changed.FileFormat));
		WorkspaceDocumentSnapshot current = refreshed.Snapshot!;
		WorkspaceDocumentCommitResult committed = await manager.CommitAsync(new WorkspaceDocumentCommitRequest(
			current.DocumentKey,
			current.DocumentId,
			current.Version,
			current.OnDiskStamp));

		Assert.AreEqual(WorkspaceDocumentCommitStatus.Committed, committed.Status);
	}

	[TestMethod]
	public async Task Commit_PostflightReportsViewThatBecomesPendingDuringWrite()
	{
		var fileSystem = new TestFileSystem("initial");
		var pendingReplacement = new TaskCompletionSource<WorkspaceFileReplacementResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		fileSystem.PendingReplacement = pendingReplacement.Task;
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var source = new TestView("source");
		var peer = new TestView("peer");

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync(
			"script.lua",
			OpenOptions,
			source)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, peer);
		source.RaiseApply(new WorkspaceDocumentReplaceRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			"changed",
			initial.FileFormat));

		WorkspaceDocumentSnapshot changed = GetSnapshot(store, initial.DocumentId);
		Task<WorkspaceDocumentCommitResult> commit = manager.CommitAsync(new WorkspaceDocumentCommitRequest(
			changed.DocumentKey,
			changed.DocumentId,
			changed.Version,
			changed.OnDiskStamp));
		await fileSystem.ReplacementStarted.Task;
		peer.HasPendingEdits = true;
		pendingReplacement.SetResult(new WorkspaceFileReplacementResult(
			WorkspaceFileReplacementStatus.Replaced,
			new FileStamp(true, 7, DateTime.UnixEpoch.AddMinutes(1), "committed")));

		WorkspaceDocumentCommitResult result = await commit;

		Assert.AreEqual(WorkspaceDocumentCommitStatus.CommittedWithUnsynchronizedView, result.Status);
		CollectionAssert.AreEqual(new[] { "peer" }, result.BlockingViewIds!.ToArray());
	}

	[TestMethod]
	public async Task ResolveConflict_UseDiskReportsViewThatBecomesPendingDuringRead()
	{
		var fileSystem = new TestFileSystem("initial");
		var pendingRead = new TaskCompletionSource<WorkspaceFileReadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var source = new TestView("source");
		var peer = new TestView("peer");

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync("script.lua", OpenOptions, source)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, peer);
		fileSystem.ResetReadStarted();
		fileSystem.PendingRead = pendingRead.Task;
		source.RaiseApply(new WorkspaceDocumentReplaceRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			"changed",
			initial.FileFormat));
		WorkspaceDocumentSnapshot changed = GetSnapshot(store, initial.DocumentId);
		Task<WorkspaceDocumentConflictResolutionResult> resolution = manager.ResolveExternalConflictAsync(
			new WorkspaceDocumentConflictResolutionRequest(
				changed.DocumentKey,
				changed.DocumentId,
				changed.Version,
				initial.OnDiskStamp,
				WorkspaceDocumentConflictResolutionChoice.UseDisk));
		await fileSystem.ReadStarted.Task;
		peer.HasPendingEdits = true;
		pendingRead.SetResult(new WorkspaceFileReadResult("disk", DefaultFormat, initial.OnDiskStamp));

		WorkspaceDocumentConflictResolutionResult result = await resolution;

		Assert.AreEqual(WorkspaceDocumentConflictResolutionStatus.ResolvedWithUnsynchronizedView, result.Status);
		CollectionAssert.AreEqual(new[] { "peer" }, result.BlockingViewIds!.ToArray());
	}

	[TestMethod]
	public async Task ResolveConflict_UseLogicalReportsViewThatBecomesPendingDuringWrite()
	{
		var fileSystem = new TestFileSystem("initial");
		var pendingReplacement = new TaskCompletionSource<WorkspaceFileReplacementResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		fileSystem.PendingReplacement = pendingReplacement.Task;
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var source = new TestView("source");
		var peer = new TestView("peer");

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync("script.lua", OpenOptions, source)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, peer);
		source.RaiseApply(new WorkspaceDocumentReplaceRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			"changed",
			initial.FileFormat));
		WorkspaceDocumentSnapshot changed = GetSnapshot(store, initial.DocumentId);
		Task<WorkspaceDocumentConflictResolutionResult> resolution = manager.ResolveExternalConflictAsync(
			new WorkspaceDocumentConflictResolutionRequest(
				changed.DocumentKey,
				changed.DocumentId,
				changed.Version,
				new FileStamp(true, 8, DateTime.UnixEpoch.AddMinutes(1), "external"),
				WorkspaceDocumentConflictResolutionChoice.UseLogical));
		await fileSystem.ReplacementStarted.Task;
		peer.HasPendingEdits = true;
		pendingReplacement.SetResult(new WorkspaceFileReplacementResult(
			WorkspaceFileReplacementStatus.Replaced,
			new FileStamp(true, 7, DateTime.UnixEpoch.AddMinutes(1), "committed")));

		WorkspaceDocumentConflictResolutionResult result = await resolution;

		Assert.AreEqual(WorkspaceDocumentConflictResolutionStatus.ResolvedWithUnsynchronizedView, result.Status);
		CollectionAssert.AreEqual(new[] { "peer" }, result.BlockingViewIds!.ToArray());
	}

	[TestMethod]
	public async Task Delete_BlocksViewsUntilStoreDeletionThenClosesThem()
	{
		var fileSystem = new TestFileSystem("initial");
		var pendingDelete = new TaskCompletionSource<WorkspaceFileDeleteResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		fileSystem.PendingDelete = pendingDelete.Task;
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var first = new TestView("first");
		var second = new TestView("second");

		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync("script.lua", OpenOptions, first)).Snapshot!;
		await manager.OpenWithViewAsync("script.lua", OpenOptions, second);
		Task<WorkspaceDocumentDeleteResult> delete = manager.DeleteAsync(new WorkspaceDocumentDeleteRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			initial.OnDiskStamp));
		await fileSystem.DeleteStarted.Task;

		Assert.IsTrue(first.DeleteBarrierActive);
		Assert.IsTrue(second.DeleteBarrierActive);
		pendingDelete.SetResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteStatus.Deleted));

		WorkspaceDocumentDeleteResult result = await delete;

		Assert.AreEqual(WorkspaceDocumentDeleteStatus.Deleted, result.Status);
		Assert.AreEqual(1, first.DetachCount);
		Assert.AreEqual(1, second.DetachCount);
		Assert.IsFalse(first.DeleteBarrierActive);
		Assert.IsFalse(second.DeleteBarrierActive);
	}

	[TestMethod]
	public async Task RenameDirectory_RekeysRetainedDescendantAndAcknowledgesView()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("text");
		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync(
			Path.Combine("folder", "one.lua"),
			OpenOptions,
			view)).Snapshot!;

		WorkspaceDocumentDirectoryRenameResult result = await manager.RenameDirectoryAsync(
			new WorkspaceDocumentDirectoryRenameRequest(
				"folder",
				"moved",
				[]));

		Assert.AreEqual(WorkspaceDocumentDirectoryRenameStatus.Renamed, result.Status);
		WorkspaceDocumentSnapshot renamed = result.Snapshots[0];
		Assert.AreEqual(Path.GetFullPath(Path.Combine("moved", "one.lua")), renamed.DocumentId);
		Assert.AreEqual(renamed.DocumentId, view.DocumentId);
		Assert.AreEqual(renamed.DocumentKey, view.DocumentKey);
		Assert.IsFalse(store.TryGetSnapshot(initial.DocumentId, out _));
		Assert.IsTrue(store.TryGetSnapshot(renamed.DocumentId, out _));
	}

	[TestMethod]
	public async Task RenameDirectory_AcknowledgementFailureKeepsStoreIdentityAndReportsView()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("text") { ThrowOnIdentityAcknowledge = true };
		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync(
			Path.Combine("folder", "one.lua"),
			OpenOptions,
			view)).Snapshot!;

		WorkspaceDocumentDirectoryRenameResult result = await manager.RenameDirectoryAsync(
			new WorkspaceDocumentDirectoryRenameRequest(
				"folder",
				"moved",
				[]));

		Assert.AreEqual(WorkspaceDocumentDirectoryRenameStatus.ViewUpdateFailed, result.Status);
		CollectionAssert.AreEqual(new[] { "text" }, result.FailedViewIds!.ToArray());
		WorkspaceDocumentSnapshot renamed = result.Snapshots[0];
		Assert.IsFalse(store.TryGetSnapshot(initial.DocumentId, out _));
		Assert.IsTrue(store.TryGetSnapshot(renamed.DocumentId, out _));
	}

	[TestMethod]
	public async Task Rename_AcknowledgesOpenedViewAndPreservesKey()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("text");
		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync("script.lua", OpenOptions, view)).Snapshot!;

		WorkspaceDocumentRenameResult result = await manager.RenameAsync(new WorkspaceDocumentRenameRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			initial.OnDiskStamp,
			"renamed.lua"));

		Assert.AreEqual(WorkspaceDocumentRenameStatus.Renamed, result.Status);
		Assert.AreEqual(initial.DocumentKey, result.Snapshot!.DocumentKey);
		Assert.AreEqual(result.Snapshot.DocumentId, view.DocumentId);
		Assert.AreEqual(result.Snapshot.DocumentKey, view.DocumentKey);
	}

	[TestMethod]
	public async Task SaveAs_AcknowledgesOpenedViewAndPreservesKey()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		await using var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		var view = new TestView("text");
		WorkspaceDocumentSnapshot initial = (await manager.OpenWithViewAsync("script.lua", OpenOptions, view)).Snapshot!;
		fileSystem.CapturedStamps.Enqueue(initial.OnDiskStamp);
		fileSystem.CapturedStamps.Enqueue(FileStamp.Missing);

		WorkspaceDocumentSaveAsResult result = await manager.SaveAsAsync(new WorkspaceDocumentSaveAsRequest(
			initial.DocumentKey,
			initial.DocumentId,
			initial.Version,
			initial.OnDiskStamp,
			"saved-as.lua"));

		Assert.AreEqual(WorkspaceDocumentSaveAsStatus.SavedAs, result.Status);
		Assert.AreEqual(initial.DocumentKey, result.Snapshot!.DocumentKey);
		Assert.AreEqual(result.Snapshot.DocumentId, view.DocumentId);
		Assert.AreEqual(result.Snapshot.DocumentKey, view.DocumentKey);
	}

	[TestMethod]
	public async Task StopAsync_DetachesOnceRejectsNewWorkAndPreventsLatePublication()
	{
		var fileSystem = new TestFileSystem("initial");
		await using var store = new WorkspaceDocumentStore(fileSystem);
		var view = new TestView("text");
		var manager = new WorkspaceDocumentManager(store, new ImmediateUiDispatcher().Invoke);
		WorkspaceDocumentSnapshot snapshot = (await manager.OpenWithViewAsync(
			"script.lua",
			OpenOptions,
			view)).Snapshot!;

		Task firstStop = manager.StopAsync();
		Task secondStop = manager.StopAsync();
		await firstStop;
		await secondStop;
		view.RaiseApply(new WorkspaceDocumentReplaceRequest(
			snapshot.DocumentKey,
			snapshot.DocumentId,
			snapshot.Version,
			"late",
			snapshot.FileFormat));

		Assert.AreSame(firstStop, secondStop);
		Assert.AreEqual(1, view.DetachCount);
		Assert.AreEqual(0, view.PublishSubscriberCount);
		await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => manager.OpenAsync(
			"after-stop.lua",
			OpenOptions));
		await manager.DisposeAsync();
	}

	[TestMethod]
	public async Task Composition_UsesOneScopedStoreAndDisposesViewBeforeStoreOnce()
	{
		var disposalOrder = new List<string>();
		var trackedStore = new TrackingStore(disposalOrder);
		var services = new ServiceCollection();
		services.AddScriptingStudioHostComposition();
		services.AddScoped<IUiDispatcherService>(_ => new ImmediateUiDispatcher());
		services.AddScoped<IWorkspaceDocumentStore>(_ => trackedStore);

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
		IWorkspaceDocumentManager firstBridge = scope.ServiceProvider.GetRequiredService<IWorkspaceDocumentManager>();
		IWorkspaceDocumentManager secondBridge = scope.ServiceProvider.GetRequiredService<IWorkspaceDocumentManager>();
		IWorkspaceDocumentStore firstStore = scope.ServiceProvider.GetRequiredService<IWorkspaceDocumentStore>();
		IWorkspaceDocumentStore secondStore = scope.ServiceProvider.GetRequiredService<IWorkspaceDocumentStore>();
		var view = new TestView("composition", disposalOrder);

		await firstBridge.OpenWithViewAsync("composition.lua", OpenOptions, view);
		Assert.AreSame(firstBridge, secondBridge);
		Assert.AreSame(firstStore, secondStore);
		Assert.AreSame(trackedStore, firstStore);

		await scope.DisposeAsync();
		await scope.DisposeAsync();

		CollectionAssert.AreEqual(new[] { "view.detach", "store.dispose" }, disposalOrder.ToArray());
		Assert.AreEqual(1, view.DetachCount);
		Assert.AreEqual(1, trackedStore.DisposeCount);
	}

	private static WorkspaceDocumentSnapshot GetSnapshot(
		IWorkspaceDocumentStore store,
		string documentId)
	{
		Assert.IsTrue(store.TryGetSnapshot(documentId, out WorkspaceDocumentSnapshot? snapshot));
		return snapshot!;
	}

	private sealed class ImmediateUiDispatcher : IUiDispatcherService
	{
		public bool CheckAccess() => true;

		public void Invoke(Action action)
		{
			ArgumentNullException.ThrowIfNull(action);
			action();
		}
	}

	private sealed class TestView : IWorkspaceDocumentView
	{
		private readonly List<string>? _actions;
		private WorkspaceDocumentKey? _documentKey;

		public TestView(string projectionId, List<string>? actions = null)
		{
			ViewId = projectionId;
			_actions = actions;
		}

		public event EventHandler<WorkspaceDocumentViewApplyRequestedEventArgs>? ApplyRequested;

		public string ViewId { get; }

		public string DocumentId { get; private set; } = string.Empty;

		public string Text => string.Empty;

		public void Apply(IReadOnlyList<TextEditOperation> operations) { }

		public WorkspaceDocumentKey? DocumentKey
		{
			get => _documentKey ?? InitialDocumentKey;
			private set => _documentKey = value;
		}

		public WorkspaceDocumentKey? InitialDocumentKey { get; init; }

		public bool HasPendingEdits { get; set; }

		public bool HasConflict { get; init; }

		public WorkspaceDocumentViewOpenStatus AttachStatus { get; init; } = WorkspaceDocumentViewOpenStatus.Opened;

		public bool ThrowOnRefresh { get; set; }

		public bool ThrowOnAcknowledge { get; set; }

		public bool ThrowOnIdentityAcknowledge { get; set; }

		public bool DeleteBarrierActive { get; private set; }

		public bool ThrowOnDeleteBarrier { get; set; }

		public bool WasUnloadedWhenAttached { get; private set; }

		public int OpenCount { get; private set; }

		public int RefreshCount { get; private set; }

		public int DetachCount { get; private set; }

		public int PublishSubscriberCount => ApplyRequested?.GetInvocationList().Length ?? 0;

		public WorkspaceDocumentViewOpenResult Open(WorkspaceDocumentSnapshot snapshot)
		{
			WasUnloadedWhenAttached = DocumentKey is null && !HasPendingEdits && !HasConflict;
			OpenCount++;
			if (AttachStatus != WorkspaceDocumentViewOpenStatus.Opened)
				return new WorkspaceDocumentViewOpenResult(AttachStatus);

			DocumentId = snapshot.DocumentId;
			DocumentKey = snapshot.DocumentKey;
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenStatus.Opened);
		}

		public WorkspaceDocumentViewRefreshResult Refresh(WorkspaceDocumentSnapshot snapshot)
		{
			RefreshCount++;
			_actions?.Add($"{ViewId}.refresh");
			if (ThrowOnRefresh)
				throw new InvalidOperationException("refresh failed");

			DocumentId = snapshot.DocumentId;
			DocumentKey = snapshot.DocumentKey;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshStatus.Refreshed);
		}

		public WorkspaceDocumentViewRefreshResult DiscardPendingEdits(WorkspaceDocumentSnapshot snapshot)
			=> new(WorkspaceDocumentViewRefreshStatus.Refreshed);

		public WorkspaceDocumentViewIdentityResult AcknowledgeIdentity(WorkspaceDocumentIdentityChange change)
		{
			if (ThrowOnIdentityAcknowledge)
				return new WorkspaceDocumentViewIdentityResult(
					WorkspaceDocumentViewIdentityStatus.Failed,
					new WorkspaceOperationFailure("IdentityAcknowledgementFailed", "identity acknowledgement failed"));

			DocumentId = change.Snapshot.DocumentId;
			DocumentKey = change.Snapshot.DocumentKey;
			return new WorkspaceDocumentViewIdentityResult(WorkspaceDocumentViewIdentityStatus.Updated);
		}

		public WorkspaceDocumentViewDeleteGuardResult SetDeleteGuard(bool active)
		{
			if (ThrowOnDeleteBarrier)
				return new WorkspaceDocumentViewDeleteGuardResult(
					WorkspaceDocumentViewDeleteGuardStatus.Failed,
					new WorkspaceOperationFailure("DeleteBarrierFailed", "delete guard failed"));

			DeleteBarrierActive = active;
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardStatus.Applied);
		}

		public WorkspaceDocumentViewRefreshResult AcknowledgeApply(WorkspaceDocumentMutationResult result)
		{
			_actions?.Add($"{ViewId}.ack");
			if (ThrowOnAcknowledge)
				throw new InvalidOperationException("acknowledgement failed");

			if (result.Snapshot is not null)
				DocumentKey = result.Snapshot.DocumentKey;

			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshStatus.Refreshed);
		}

		public void Close()
		{
			DetachCount++;
			if (_actions is not null)
				_actions.Add("view.detach");
		}

		public void RaiseApply(WorkspaceDocumentReplaceRequest request)
			=> ApplyRequested?.Invoke(this, new WorkspaceDocumentViewApplyRequestedEventArgs(request));
	}

	private sealed class TestFileSystem : IWorkspaceFileSystem
	{
		private readonly TextFileFormat _fileFormat = DefaultFormat;
		private readonly string _content;

		public TestFileSystem(string content)
		{
			_content = content;
		}

		public int ReadCount { get; private set; }

		public Task<WorkspaceFileReadResult>? PendingRead { get; set; }

		public TaskCompletionSource<object?> ReadStarted { get; private set; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public void ResetReadStarted()
			=> ReadStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public int CaptureStampCount { get; private set; }

		public Queue<FileStamp> CapturedStamps { get; } = new();

		public async Task<WorkspaceFileReadResult> ReadAsync(string path, CancellationToken cancellationToken)
		{
			ReadCount++;
			ReadStarted.TrySetResult(null);
			if (PendingRead is not null)
				return await PendingRead.WaitAsync(cancellationToken);

			return new WorkspaceFileReadResult(
				_content,
				_fileFormat,
				new FileStamp(true, _content.Length, DateTime.UnixEpoch, "hash"));
		}

		public Task<FileStamp> CaptureStampAsync(string path, CancellationToken cancellationToken)
		{
			CaptureStampCount++;
			if (CapturedStamps.Count > 0)
				return Task.FromResult(CapturedStamps.Dequeue());

			return Task.FromResult(new FileStamp(true, _content.Length, DateTime.UnixEpoch, "hash"));
		}

		public Task<WorkspaceTemporaryFile> WriteTemporaryAsync(
			string directory,
			ReadOnlyMemory<byte> content,
			CancellationToken cancellationToken)
			=> Task.FromResult(new WorkspaceTemporaryFile(
				Path.Combine(directory, "temporary"),
				content.Length,
				"written"));

		public async Task<WorkspaceFileReplacementResult> ReplaceAsync(
			WorkspaceTemporaryFile temporaryFile,
			string destinationPath,
			FileStamp expectedStamp,
			CancellationToken cancellationToken)
		{
			ReplacementStarted.TrySetResult(null);
			if (PendingReplacement is not null)
				return await PendingReplacement.WaitAsync(cancellationToken);

			return new WorkspaceFileReplacementResult(
				WorkspaceFileReplacementStatus.Replaced,
				new FileStamp(true, temporaryFile.Length, DateTime.UnixEpoch, temporaryFile.ContentHash));
		}

		public Task<WorkspaceFileMoveResult> MoveAsync(
			string sourcePath,
			string destinationPath,
			FileStamp expectedSourceStamp,
			CancellationToken cancellationToken)
			=> Task.FromResult(new WorkspaceFileMoveResult(
				WorkspaceFileMoveStatus.Moved,
				expectedSourceStamp));

		public Task<WorkspaceFileMoveResult> MoveDirectoryAsync(
			string sourcePath,
			string destinationPath,
			CancellationToken cancellationToken)
			=> Task.FromResult(new WorkspaceFileMoveResult(WorkspaceFileMoveStatus.Moved));

		public Task DeleteTemporaryAsync(WorkspaceTemporaryFile temporaryFile)
			=> Task.CompletedTask;

		public Task<WorkspaceFileDeleteResult>? PendingDelete { get; set; }

		public Task<WorkspaceFileReplacementResult>? PendingReplacement { get; set; }

		public TaskCompletionSource<object?> ReplacementStarted { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource<object?> DeleteStarted { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public Task<WorkspaceFileDeleteResult> DeleteAsync(
			string path,
			FileStamp expectedStamp,
			CancellationToken cancellationToken,
			bool useRecycleBin = false)
		{
			DeleteStarted.TrySetResult(null);
			return PendingDelete?.WaitAsync(cancellationToken)
				?? Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteStatus.Deleted));
		}

		public Task<WorkspaceFileDeleteResult> DeleteDirectoryAsync(
			string path,
			CancellationToken cancellationToken,
			bool useRecycleBin = false)
			=> Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteStatus.Deleted));
	}

	private sealed class TrackingStore : IWorkspaceDocumentStore
	{
		private readonly List<string> _disposalOrder;
		private readonly WorkspaceDocumentSnapshot _snapshot;

		public TrackingStore(List<string> disposalOrder)
		{
			_disposalOrder = disposalOrder;
			_snapshot = new WorkspaceDocumentSnapshot(
				new WorkspaceDocumentKey(Guid.NewGuid()),
				Path.GetFullPath("composition.lua"),
				"composition.lua",
				0,
				0,
				false,
				new StringTextSnapshot("initial", "composition.lua"),
				DefaultFormat,
				new FileStamp(true, 7, DateTime.UnixEpoch, "hash"));
		}

		public int DisposeCount { get; private set; }

		public Task<WorkspaceDocumentOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new WorkspaceDocumentOpenResult(WorkspaceDocumentOpenStatus.Opened, _snapshot));

		public bool TryGetSnapshot(string? filePath, out WorkspaceDocumentSnapshot? snapshot)
		{
			snapshot = _snapshot;
			return true;
		}

		public IReadOnlyList<WorkspaceDocumentSnapshot> GetSnapshotsUnderDirectory(string directoryPath)
			=> [_snapshot];

		public WorkspaceDocumentMutationResult TryReplace(WorkspaceDocumentReplaceRequest request)
			=> new(WorkspaceDocumentMutationStatus.NoChange, request.ExpectedDocumentKey, request.DocumentId, request.ExpectedVersion, _snapshot);

		public Task<WorkspaceDocumentCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new WorkspaceDocumentCommitResult(
				WorkspaceDocumentCommitStatus.Committed,
				request.ExpectedDocumentKey,
				request.DocumentId,
				request.ExpectedVersion,
				_snapshot));

		public Task<WorkspaceDocumentReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new WorkspaceDocumentReloadResult(
				WorkspaceDocumentReloadStatus.Unchanged,
				request.ExpectedDocumentKey,
				request.DocumentId,
				request.ExpectedVersion,
				_snapshot));

		public Task<WorkspaceDocumentConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public WorkspaceDocumentMutationResult Discard(WorkspaceDocumentDiscardRequest request)
			=> new(WorkspaceDocumentMutationStatus.NoChange, request.ExpectedDocumentKey, request.DocumentId, request.ExpectedVersion, _snapshot);

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

		public ValueTask DisposeAsync()
		{
			DisposeCount++;
			_disposalOrder.Add("store.dispose");
			return ValueTask.CompletedTask;
		}
	}
}
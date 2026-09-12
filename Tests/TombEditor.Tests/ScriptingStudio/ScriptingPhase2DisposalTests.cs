#nullable enable

using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhase2DisposalTests
{
	[TestMethod]
	[TestCategory("TextEditorBaseModernization")]
	public void FileExplorerDocumentSync_RoutesExternalObservationsThroughController()
	{
		var controller = new Mock<IEditorDocumentController>();
		var service = new StudioFileExplorerDocumentSyncService();
		const string changedPath = @"C:\Scripts\changed.lua";
		const string deletedPath = @"C:\Scripts\deleted.lua";
		const string oldPath = @"C:\Scripts\old.lua";
		const string newPath = @"C:\Scripts\new.lua";
		const string directoryPath = @"C:\Scripts";

		service.ApplyChanged(
			controller.Object,
			false,
			new FileSystemEventArgs(WatcherChangeTypes.Changed, directoryPath, "changed.lua"));
		service.ApplyChanged(
			controller.Object,
			true,
			new FileSystemEventArgs(WatcherChangeTypes.Changed, directoryPath, "changed.lua"));
		service.ApplyDeleted(
			controller.Object,
			new FileSystemEventArgs(WatcherChangeTypes.Deleted, directoryPath, "deleted.lua"));
		service.ApplyRenamed(
			controller.Object,
			new RenamedEventArgs(
				WatcherChangeTypes.Renamed,
				directoryPath,
				"new.lua",
				"old.lua"));
		service.ApplyOpened(controller.Object, new FileOpenedEventArgs(changedPath));
		service.ApplyOpened(controller.Object, FileOpenedEventArgs.CreateSourceView(newPath));
		service.ApplyWindowFocus(controller.Object, true);

		controller.Verify(item => item.AddFileToReloadQueue(changedPath), Times.Once);
		controller.Verify(item => item.AddFileToReloadQueue(deletedPath), Times.Once);
		controller.Verify(item => item.AddFileToReloadQueue(oldPath), Times.Once);
		controller.Verify(item => item.OpenFile(changedPath, EditorType.Default, default), Times.Once);
		controller.Verify(item => item.OpenSourceFile(newPath, default), Times.Once);
		controller.Verify(item => item.TryRunFileReloadQueue(), Times.Once);
	}

	[TestMethod]
	public void LayoutCoordinator_DisposesLayoutAndDockHostExactlyOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			var dockHost = new Mock<IAvalonDockHost>();
			dockHost.Setup(host => host.SaveLayout()).Returns("layout");

			var coordinator = new WorkbenchLayoutCoordinator(
				ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(),
				new Mock<IEditorDocumentController>().Object,
				dockHost.Object,
				new PaneCatalog([]),
				new Mock<IPaneHostService>().Object,
				new StudioFileExplorerDocumentSyncService());

			coordinator.Dispose();
			coordinator.Dispose();

			dockHost.Verify(host => host.SaveLayout(), Times.Once);
			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});
	}

	[TestMethod]
	public void WorkbenchDispose_IsIdempotentAndLeavesInjectedLuaServicesToOwner()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			var lifecycleService = new Mock<ILuaEditorLifecycleService>();
			var intellisenseBridge = new Mock<ILuaIntellisenseBridge>();
			ITextEditorHost textEditorHost = new Mock<ITextEditorHost>().Object;
			var trackedDocumentStateService = new LuaTrackedDocumentStateService(
				textEditorHost,
				new Mock<ILuaIntelliSenseProvider>().Object);
			var referenceSearchService = new LuaReferenceSearchService(
				textEditorHost,
				new Mock<ITextReferencesProvider>().Object,
				@"C:\Scripts");
			var workspaceCommandService = new TextWorkspaceCommandService(
				new TextWorkspaceEditApplier(textEditorHost),
				new Mock<ITextEditProvider>().Object);
			builder.WithLuaCapabilities(
				lifecycleService.Object,
				intellisenseBridge.Object,
				trackedDocumentStateService,
				referenceSearchService,
				workspaceCommandService);
			WorkbenchService workbench = builder.Build();

			workbench.Dispose();
			workbench.Dispose();

			lifecycleService.Verify(service => service.Dispose(), Times.Never);
			intellisenseBridge.Verify(service => service.Dispose(), Times.Never);

			lifecycleService.Object.Dispose();
			intellisenseBridge.Object.Dispose();

			lifecycleService.Verify(service => service.Dispose(), Times.Once);
			intellisenseBridge.Verify(service => service.Dispose(), Times.Once);
		});
	}

	[TestMethod]
	public void WorkbenchDispose_DetachesMessengerBeforeLayoutCanPublish()
	{
		StaTestHelper.RunInSta(() =>
		{
			var dockHost = new Mock<IAvalonDockHost>();
			using var builder = new WorkbenchServiceTestBuilder()
				.WithDockHost(dockHost);
			builder.Build();
			builder.DocumentController.Invocations.Clear();
			dockHost
				.Setup(host => host.SaveLayout())
				.Returns(() =>
				{
						((IMessenger)builder.Messenger).Send(new ShellUiRefreshMessage());
					return "layout";
				});

			builder.Workbench.Dispose();

			builder.DocumentController.VerifyGet(controller => controller.CurrentDocumentContext, Times.Never);
		});
	}

	[TestMethod]
	public void WorkbenchDispose_DetachesDocumentControllerEvents()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var builder = new WorkbenchServiceTestBuilder();
			builder.Build();

			builder.Workbench.Dispose();

			builder.DocumentController.VerifyRemove(
				controller => controller.CurrentEditorChanged -= It.IsAny<EventHandler<ScriptingDocumentContextChangedEventArgs>>(),
				Times.AtLeastOnce);
			builder.DocumentController.VerifyRemove(
				controller => controller.EditorClosed -= It.IsAny<EventHandler<EditorControlEventArgs>>(),
				Times.AtLeastOnce);
			builder.DocumentController.VerifyRemove(
				controller => controller.EditorTitleChanged -= It.IsAny<EventHandler<EditorControlEventArgs>>(),
				Times.AtLeastOnce);
		});
	}

	[TestMethod]
	public void WorkbenchDispose_UnregistersItsMessengerRecipientExactlyOnce()
	{
		StaTestHelper.RunInSta(() =>
		{
			var messenger = new Mock<IMessenger>();
			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithMessenger(messenger.Object);
			builder.Build();

			builder.Workbench.Dispose();
			builder.Workbench.Dispose();

			messenger.Verify(
				value => value.UnregisterAll(It.IsAny<WorkbenchService>()),
				Times.Once);
		});
	}

	[TestMethod]
	public void ShellFactory_WhenRootResolutionFails_DisposesChildScopeOnce()
	{
		var scope = new Mock<IServiceScope>();
		var scopedServiceProvider = new Mock<IServiceProvider>();
		var scopeFactory = new Mock<IServiceScopeFactory>();
		var serviceProvider = new Mock<IServiceProvider>();
		var shellContext = new ScriptingStudioShellContext();

		scope.SetupGet(value => value.ServiceProvider).Returns(scopedServiceProvider.Object);
		scopeFactory.Setup(value => value.CreateScope()).Returns(scope.Object);
		serviceProvider
			.Setup(value => value.GetService(typeof(IServiceScopeFactory)))
			.Returns(scopeFactory.Object);
		scopedServiceProvider
			.Setup(value => value.GetService(typeof(ScriptingStudioShellContext)))
			.Returns(shellContext);
		scopedServiceProvider
			.Setup(value => value.GetService(typeof(RootShellViewModel)))
			.Throws(new InvalidOperationException("Simulated shell construction failure."));

		var factory = new ScriptingStudioShellFactory(serviceProvider.Object);
		var ide = new IDE(new IDEConfiguration(), []);

		Assert.ThrowsException<InvalidOperationException>(() => factory.Create(ide));
		scope.Verify(value => value.Dispose(), Times.Once);
	}

}

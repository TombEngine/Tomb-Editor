#nullable enable

using Moq;
using CommunityToolkit.Mvvm.Messaging;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingPhase0ConstructionTests
{
	[TestMethod]
	public void LayoutRestoreFailure_CleansUpDockOwnership()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			dockHost
				.Setup(host => host.RestoreLayout(It.IsAny<string?>(), It.IsAny<DockPanelState>(), It.IsAny<Action?>()))
				.Throws(new InvalidOperationException("layout failed"));

			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});

	[TestMethod]
	public void LuaAttachFailure_CleansUpDockOwnership()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			var intellisenseBridge = new Mock<ILuaIntellisenseBridge>();
			intellisenseBridge
				.Setup(bridge => bridge.Attach())
				.Throws(new InvalidOperationException("attach failed"));

			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);
			builder.WithLuaCapabilities(
				new Mock<ILuaEditorLifecycleService>().Object,
				intellisenseBridge.Object,
				null,
				null,
				null);

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});

	[TestMethod]
	public void PreviousSessionFailure_CleansUpDockOwnership()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);
			builder.DocumentController
				.Setup(controller => controller.CheckPreviousSession())
				.Throws(new InvalidOperationException("previous session failed"));

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});

	[TestMethod]
	public void InitialFileOpenFailure_CleansUpDockOwnership()
	{
		using var scriptDirectory = new TemporaryScriptDirectory();
		StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			string initialFilePath = System.IO.Path.Combine(scriptDirectory.Path, "Script.txt");
			ScriptingWorkspaceProfile profile = ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(
				initialFilePath: initialFilePath);
			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithProfile(profile);
			builder.WithDockHost(dockHost);
			builder.DocumentController
				.Setup(controller => controller.OpenFile(It.IsAny<string>(), It.IsAny<EditorType>(), It.IsAny<DocumentLoadOptions>()))
				.Throws(new InvalidOperationException("initial file failed"));

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});
	}

	[TestMethod]
	public void SettingsApplicationFailure_CleansUpDockOwnership()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua"
			};
			var registration = new ScriptingDocumentRegistration(
				EditorType.Text,
				DocumentMode.Lua,
				_ => true,
				_ => true,
				_ => editor,
				new(null, ScriptingDocumentConfigurationKind.Lua));
			var intellisenseProvider = new Mock<ILuaLanguageServerIntelliSenseProvider>();
			intellisenseProvider
				.Setup(provider => provider.GetDiagnostics(It.IsAny<string>()))
				.Throws(new InvalidOperationException("settings failed"));
			intellisenseProvider
				.Setup(provider => provider.GetSemanticTokens(It.IsAny<string>()))
				.Returns([]);
			var textEditorHost = Mock.Of<ITextEditorHost>();
			var trackedState = new LuaTrackedDocumentStateService(textEditorHost, intellisenseProvider.Object);
			var referenceService = new LuaReferenceSearchService(
				textEditorHost,
				Mock.Of<ILanguageServerReferencesProvider>(),
				@"C:\Scripts");
			var workspaceCommandService = new TextWorkspaceCommandService(
				new TextWorkspaceEditApplier(textEditorHost),
				Mock.Of<ILanguageServerRenameProvider>());

			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);
			builder.AddEditor(editor, registration);
			builder.WithLuaCapabilities(
				new Mock<ILuaEditorLifecycleService>().Object,
				new Mock<ILuaIntellisenseBridge>().Object,
				trackedState,
				referenceService,
				workspaceCommandService);

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
		});

	[TestMethod]
	public void ComponentConstructionFailure_CleansUpPreviouslyCreatedCollaborators()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			var project = new Mock<IGameProject>();
			project
				.Setup(value => value.GetEngineRootDirectoryPath())
				.Throws(new InvalidOperationException("message service construction failed"));

			using var builder = new WorkbenchServiceTestBuilder();
			builder.WithDockHost(dockHost);
			builder.WithProject(project);

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			dockHost.Verify(host => host.DetachDocumentController(), Times.Once);
			builder.DocumentController.VerifyRemove(
				controller => controller.CurrentEditorChanged -= It.IsAny<EventHandler<ScriptingDocumentContextChangedEventArgs>>(),
				Times.Once);
		});


	[TestMethod]
	public void StartupFailure_CleansUpWorkbenchOwnedRegistrations()
		=> StaTestHelper.RunInSta(() =>
		{
			var dockHost = CreateDockHost();
			var messenger = new Mock<IMessenger>();
			using var builder = new WorkbenchServiceTestBuilder()
				.WithDockHost(dockHost)
				.WithMessenger(messenger.Object);
			builder.DocumentController
				.Setup(controller => controller.CheckPreviousSession())
				.Throws(new InvalidOperationException("previous session failed"));

			Assert.ThrowsException<InvalidOperationException>(() => builder.Build());

			builder.DocumentController.VerifyRemove(
				controller => controller.CurrentEditorChanged -= It.IsAny<EventHandler<ScriptingDocumentContextChangedEventArgs>>(),
				Times.AtLeastOnce);
			builder.DocumentController.VerifyRemove(
				controller => controller.EditorClosed -= It.IsAny<EventHandler<EditorControlEventArgs>>(),
				Times.AtLeastOnce);
			builder.DocumentController.VerifyRemove(
				controller => controller.EditorTitleChanged -= It.IsAny<EventHandler<EditorControlEventArgs>>(),
				Times.AtLeastOnce);
			messenger.Verify(
				value => value.UnregisterAll(It.IsAny<WorkbenchService>()),
				Times.Once);
		});
	private static Mock<IAvalonDockHost> CreateDockHost()
	{
		var dockHost = new Mock<IAvalonDockHost>();
		dockHost.Setup(host => host.View).Returns(Mock.Of<FrameworkElement>());
		dockHost.Setup(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
		dockHost.Setup(host => host.SaveLayout()).Returns(string.Empty);
		return dockHost;
	}

	private sealed class TemporaryScriptDirectory : IDisposable
	{
		public TemporaryScriptDirectory()
		{
			Path = Directory.CreateTempSubdirectory("TombEditor-Phase0-Construction-").FullName;
			File.WriteAllText(System.IO.Path.Combine(Path, "Script.txt"), string.Empty);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, recursive: true);
		}
	}
}
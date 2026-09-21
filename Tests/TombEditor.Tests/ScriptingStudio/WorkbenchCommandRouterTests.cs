#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class WorkbenchCommandRouterTests
{
	[DataTestMethod]
	[DataRow(false, false)]
	[DataRow(true, false)]
	[DataRow(false, true)]
	[DataRow(true, true)]
	public void LuaCapabilityCommands_ReflectProviderSupport(bool supportsReferences, bool supportsRename)
	{
		StaTestHelper.RunInSta(() =>
		{
			using var fixture = new RouterFixture(supportsReferences, supportsRename);

			Assert.AreEqual(supportsReferences, fixture.Router.CanExecuteCommand(UICommand.FindReferences));
			Assert.AreEqual(supportsRename, fixture.Router.CanExecuteCommand(UICommand.RenameSymbol));
		});
	}

	[TestMethod]
	public void LuaReferencesResults_RequiresProfileViewContribution()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var fixture = new RouterFixture(
				supportsReferences: true,
				supportsRename: true,
				viewCommands: []);

			Assert.IsFalse(fixture.Router.CanExecuteCommand(UICommand.LuaReferencesResults));
		});
	}

	[TestMethod]
	public void TryExecuteCommand_UnhandledCommand_ReturnsFalseWithoutRefreshingUi()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var fixture = new RouterFixture(false, false);

			Assert.IsFalse(fixture.Router.TryExecuteCommand(UICommand.TypeFirstAvailableId));
			Assert.AreEqual(0, fixture.UiRefreshCount);
		});
	}

	[TestMethod]
	public void TryExecuteCommand_NewFile_ReturnsTrueAndRefreshesUi()
	{
		StaTestHelper.RunInSta(() =>
		{
			using var fixture = new RouterFixture(false, false);

			Assert.IsTrue(fixture.Router.TryExecuteCommand(UICommand.NewFile));
			Assert.AreEqual(1, fixture.UiRefreshCount);
		});
	}

	private sealed class RouterFixture : IDisposable
	{
		private readonly WorkbenchDialogCoordinator _dialogCoordinator;
		private readonly WorkbenchCodeNavigationCoordinator _codeNavigationCoordinator;

		public RouterFixture(
			bool supportsReferences,
			bool supportsRename,
			IReadOnlyList<UICommand>? viewCommands = null)
		{
			Profile = ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(
				viewCommands ?? [UICommand.LuaDiagnostics, UICommand.LuaReferencesResults],
				supportsLuaActivation: true);
			DocumentController = new Mock<IEditorDocumentController>();
			DocumentController.SetupGet(controller => controller.CurrentDocumentContext)
				.Returns(ScriptingDocumentContext.Empty);
			DocumentController.Setup(controller => controller.GetOpenEditors()).Returns([]);
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\active.lua"
			};
			DocumentController.SetupGet(controller => controller.CurrentEditor).Returns(editor);

			var messenger = new WeakReferenceMessenger();
			var paneCoordinator = new WorkbenchPaneCoordinator(
				new PaneCatalog([]),
				CreateDockHost(),
				new Mock<IPaneHostService>().Object,
				null);
			_dialogCoordinator = new WorkbenchDialogCoordinator(
				new Mock<IDialogService>().Object,
				new Mock<IWin32DialogOwnerProvider>().Object,
				new FindAndReplaceViewModel(
					DocumentController.Object,
					messenger,
					new FindReplaceService()));

			var referencesProvider = new Mock<ILanguageServerReferencesProvider>();
			referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(supportsReferences);
			var editProvider = new Mock<ILanguageServerRenameProvider>();
			editProvider.SetupGet(provider => provider.SupportsRename).Returns(supportsRename);
			ITextEditorHost textEditorHost = new Mock<ITextEditorHost>().Object;
			var luaHostServices = new LuaHostServices(
				new Mock<ILuaEditorLifecycleService>().Object,
				new Mock<ILuaIntellisenseBridge>().Object,
				new LuaTrackedDocumentStateService(textEditorHost, new Mock<ILuaLanguageServerIntelliSenseProvider>().Object),
				new LuaReferenceSearchService(textEditorHost, referencesProvider.Object, @"C:\Scripts"),
				new TextWorkspaceCommandService(new TextWorkspaceEditApplier(textEditorHost), editProvider.Object));

			_codeNavigationCoordinator = new WorkbenchCodeNavigationCoordinator(
				Profile,
				DocumentController.Object,
				paneCoordinator,
				luaHostServices,
				_dialogCoordinator,
				new Mock<IMessageService>().Object,
				ScriptingLanguageServicesTestFactory.CreateClassicScript());

			Router = new WorkbenchCommandRouter(
				Profile,
				DocumentController.Object,
				paneCoordinator,
				_dialogCoordinator,
				_codeNavigationCoordinator,
				ScriptingStudioChromeTestFixture.CreateMenuServiceMock().Object,
				ScriptingStudioChromeTestFixture.CreateToolBarServiceMock().Object,
				ScriptingStudioChromeTestFixture.CreateStatusBarServiceMock().Object,
				new Mock<IMessageService>().Object,
				() => UiRefreshCount++,
				() => { },
				() => { });
		}

		public ScriptingWorkspaceProfile Profile { get; }

		public Mock<IEditorDocumentController> DocumentController { get; }

		public WorkbenchCommandRouter Router { get; }

		public int UiRefreshCount { get; private set; }

		public void Dispose()
		{
			_codeNavigationCoordinator.Dispose();
			_dialogCoordinator.Dispose();
			if (DocumentController.Object.CurrentEditor is IDisposable editor)
				editor.Dispose();
		}

		private static IAvalonDockHost CreateDockHost()
		{
			var dockHost = new Mock<IAvalonDockHost>();
			dockHost.SetupGet(host => host.View).Returns(Mock.Of<System.Windows.FrameworkElement>());
			dockHost.SetupGet(host => host.Dispatcher).Returns(System.Windows.Threading.Dispatcher.CurrentDispatcher);
			return dockHost.Object;
		}
	}
}

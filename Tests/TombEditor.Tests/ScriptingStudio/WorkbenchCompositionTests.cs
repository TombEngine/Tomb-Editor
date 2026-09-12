#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Moq;
using MvvmDialogs;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.LanguageServer.Lua;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.LevelData;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class WorkbenchCompositionTests
{
	[DataTestMethod]
	[DataRow(nameof(WorkbenchComposition.WorkspaceProfile))]
	[DataRow(nameof(WorkbenchComposition.ProjectContext))]
	[DataRow(nameof(WorkbenchComposition.Messenger))]
	[DataRow(nameof(WorkbenchComposition.MessageService))]
	[DataRow(nameof(WorkbenchComposition.KeyBindingService))]
	[DataRow(nameof(WorkbenchComposition.MenuService))]
	[DataRow(nameof(WorkbenchComposition.ToolBarService))]
	[DataRow(nameof(WorkbenchComposition.StatusBarService))]
	[DataRow(nameof(WorkbenchComposition.PaneHostService))]
	[DataRow(nameof(WorkbenchComposition.DialogOwnerProvider))]
	[DataRow(nameof(WorkbenchComposition.DocumentController))]
	[DataRow(nameof(WorkbenchComposition.DockHost))]
	[DataRow(nameof(WorkbenchComposition.PaneCatalog))]
	[DataRow(nameof(WorkbenchComposition.FindAndReplaceViewModel))]
	[DataRow(nameof(WorkbenchComposition.LuaHostServices))]
	[DataRow(nameof(WorkbenchComposition.DialogService))]
	[DataRow(nameof(WorkbenchComposition.ShowCompilerLogsAfterBuild))]
	[DataRow(nameof(WorkbenchComposition.UseNewIncludeMethod))]
	[DataRow(nameof(WorkbenchComposition.LanguageServices))]
	[DataRow(nameof(WorkbenchComposition.GameFlowLanguageServices))]
	[DataRow(nameof(WorkbenchComposition.TrxLanguageServices))]
	[DataRow(nameof(WorkbenchComposition.FileSyncService))]
	public void Constructor_WithNullDependency_ThrowsArgumentNullException(string dependencyName)
	{
		StaTestHelper.RunInSta(() =>
		{
			ArgumentNullException exception = Assert.ThrowsException<ArgumentNullException>(
				() => CreateComposition(dependencyName));

			string expectedParameterName = char.ToLowerInvariant(dependencyName[0]) + dependencyName[1..];
			Assert.AreEqual(expectedParameterName, exception.ParamName);
		});
	}

	private static WorkbenchComposition CreateComposition(string? nullDependency = null)
	{
		ScriptingWorkspaceProfile workspaceProfile = ScriptingWorkspaceProfileTestFactory.CreateLuaProfile(
			viewCommands: [UICommand.LuaDiagnostics, UICommand.LuaReferencesResults],
			supportsLuaActivation: true);
		var project = new Mock<IGameProject>();
		project.Setup(value => value.GetEngineRootDirectoryPath()).Returns(@"C:\Engine");
		project.Setup(value => value.GetEngineExecutableFilePath()).Returns(@"C:\Engine\Game.exe");
		var projectContext = new Mock<IScriptingProjectContext>();
		projectContext.Setup(value => value.Project).Returns(project.Object);
		var messenger = new WeakReferenceMessenger();
		var documentController = new Mock<IEditorDocumentController>();
		documentController.Setup(controller => controller.GetOpenEditors()).Returns([]);
		var findAndReplaceViewModel = new FindAndReplaceViewModel(
			documentController.Object,
			messenger,
			new FindReplaceService());
		var dockHost = new Mock<IAvalonDockHost>();
		dockHost.Setup(host => host.View).Returns(Mock.Of<FrameworkElement>());
		dockHost.Setup(host => host.Dispatcher).Returns(Dispatcher.CurrentDispatcher);
		var textEditorHost = new Mock<ITextEditorHost>().Object;
		var luaHostServices = new LuaHostServices(
			new Mock<ILuaEditorLifecycleService>().Object,
			new Mock<ILuaIntellisenseBridge>().Object,
			new LuaTrackedDocumentStateService(textEditorHost, new Mock<ILuaIntelliSenseProvider>().Object),
			new LuaReferenceSearchService(textEditorHost, new Mock<ITextReferencesProvider>().Object, @"C:\Scripts"),
				new TextWorkspaceCommandService(
					new TextWorkspaceEditApplier(textEditorHost),
					new Mock<ITextEditProvider>().Object));

		return new WorkbenchComposition(
			NullWhen(nameof(WorkbenchComposition.WorkspaceProfile), workspaceProfile, nullDependency),
			NullWhen(nameof(WorkbenchComposition.ProjectContext), projectContext.Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.Messenger), messenger, nullDependency),
			NullWhen(nameof(WorkbenchComposition.MessageService), new Mock<IMessageService>().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.KeyBindingService), CreateKeyBindingService(), nullDependency),
			NullWhen(nameof(WorkbenchComposition.MenuService), ScriptingStudioChromeTestFixture.CreateMenuServiceMock().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.ToolBarService), ScriptingStudioChromeTestFixture.CreateToolBarServiceMock().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.StatusBarService), ScriptingStudioChromeTestFixture.CreateStatusBarServiceMock().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.PaneHostService), new Mock<IPaneHostService>().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.DialogOwnerProvider), new Mock<IWin32DialogOwnerProvider>().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.DocumentController), documentController.Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.DockHost), dockHost.Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.PaneCatalog), new PaneCatalog([]), nullDependency),
			NullWhen(nameof(WorkbenchComposition.FindAndReplaceViewModel), findAndReplaceViewModel, nullDependency),
			NullWhen(nameof(WorkbenchComposition.LuaHostServices), luaHostServices, nullDependency),
			NullWhen(nameof(WorkbenchComposition.DialogService), new Mock<IDialogService>().Object, nullDependency),
			NullWhen(nameof(WorkbenchComposition.ShowCompilerLogsAfterBuild), (Func<bool>)(() => false), nullDependency),
			NullWhen(nameof(WorkbenchComposition.UseNewIncludeMethod), (Func<bool>)(() => false), nullDependency),
			NullWhen(nameof(WorkbenchComposition.LanguageServices), ScriptingLanguageServicesTestFactory.CreateClassicScript(), nullDependency),
			NullWhen(nameof(WorkbenchComposition.GameFlowLanguageServices), ScriptingLanguageServicesTestFactory.CreateGameFlowScript(), nullDependency),
			NullWhen(nameof(WorkbenchComposition.TrxLanguageServices), ScriptingLanguageServicesTestFactory.CreateTRX(), nullDependency),
			NullWhen(nameof(WorkbenchComposition.FileSyncService), new StudioFileExplorerDocumentSyncService(), nullDependency));
	}

	private static IKeyBindingService<UICommand> CreateKeyBindingService()
		=> new KeyBindingService<UICommand>(
			new CommandCatalog<UICommand>([]),
			new KeyBindingOverrideCollection(),
			_ => true);

	private static T NullWhen<T>(string dependencyName, T value, string? nullDependency)
		where T : class
		=> string.Equals(dependencyName, nullDependency, StringComparison.Ordinal) ? null! : value;
}

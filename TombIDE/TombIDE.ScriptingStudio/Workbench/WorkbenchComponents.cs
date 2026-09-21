#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchComponents : IDisposable
{
	private readonly ScriptingMessageService _scriptingMessageService;
	private readonly WorkbenchDialogCoordinator _dialogCoordinator;
	private readonly WorkbenchLayoutCoordinator _layoutCoordinator;
	private readonly WorkbenchPaneCoordinator _paneCoordinator;
	private readonly TextEditorDiagnosticsCoordinator _textEditorDiagnosticsCoordinator;
	private readonly WorkbenchCodeNavigationCoordinator _codeNavigationCoordinator;
	private readonly WorkbenchCommandRouter _commandRouter;
	private readonly IEditorLifecycleService _editorLifecycleService;
	private readonly LuaWorkbenchEventCoordinator? _luaWorkbenchEventCoordinator;
	private bool _disposed;

	public WorkbenchComponents(WorkbenchComposition composition, Action updateUi)
	{
		ArgumentNullException.ThrowIfNull(composition);
		ArgumentNullException.ThrowIfNull(updateUi);

		WorkbenchDialogCoordinator? dialogCoordinator = null;
		WorkbenchLayoutCoordinator? layoutCoordinator = null;
		WorkbenchPaneCoordinator? paneCoordinator = null;
		TextEditorDiagnosticsCoordinator? textEditorDiagnosticsCoordinator = null;
		WorkbenchCodeNavigationCoordinator? codeNavigationCoordinator = null;
		LuaWorkbenchEventCoordinator? luaWorkbenchEventCoordinator = null;
		ScriptingMessageService? scriptingMessageService = null;
		WorkbenchCommandRouter? commandRouter = null;
		IEditorLifecycleService? editorLifecycleService = null;

		try
		{
			dialogCoordinator = new WorkbenchDialogCoordinator(
				composition.DialogService,
				composition.DialogOwnerProvider,
				composition.FindAndReplaceViewModel);
			layoutCoordinator = new WorkbenchLayoutCoordinator(
				composition.WorkspaceProfile,
				composition.DocumentController,
				composition.DockHost,
				composition.PaneCatalog,
				composition.PaneHostService,
				composition.FileSyncService);
			paneCoordinator = new WorkbenchPaneCoordinator(
				composition.PaneCatalog,
				composition.DockHost,
				composition.PaneHostService,
				composition.LuaHostServices?.TrackedDocumentStateService);
			textEditorDiagnosticsCoordinator = new TextEditorDiagnosticsCoordinator(
				composition.DocumentController,
				paneCoordinator);
			codeNavigationCoordinator = new WorkbenchCodeNavigationCoordinator(
				composition.WorkspaceProfile,
				composition.DocumentController,
				paneCoordinator,
				composition.LuaHostServices,
				dialogCoordinator,
				composition.MessageService,
				composition.LanguageServices);
			if (composition.WorkspaceProfile.SupportsLua)
			{
				LuaHostServices? luaHostServices = composition.LuaHostServices;
				ArgumentNullException.ThrowIfNull(luaHostServices);
				luaWorkbenchEventCoordinator = new LuaWorkbenchEventCoordinator(
					composition.Messenger,
					composition.WorkspaceProfile,
					composition.DocumentController,
					paneCoordinator,
					luaHostServices,
					composition.MessageService);
			}

			scriptingMessageService = new ScriptingMessageService(
				composition.Messenger,
				composition.WorkspaceProfile,
				composition.DocumentController,
				composition.ProjectContext.Project.GetEngineRootDirectoryPath(),
				composition.ProjectContext.Project.GetEngineExecutableFilePath(),
				new ScriptingMessageServiceOptions
				{
					GetDockLayoutXml = () => composition.DockHost.SaveLayout(),
					ShowCompilerLogsPane = paneCoordinator.ShowCompilerLogsPane,
					UpdateCompilerLogs = paneCoordinator.UpdateCompilerLogs,
					ShowCompilerLogsAfterBuild = composition.ShowCompilerLogsAfterBuild,
					UseNewIncludeMethod = composition.UseNewIncludeMethod
				},
				composition.LanguageServices,
				composition.GameFlowLanguageServices,
				composition.TrxLanguageServices,
				composition.DocumentBridge);

			commandRouter = new WorkbenchCommandRouter(
				composition.WorkspaceProfile,
				composition.DocumentController,
				paneCoordinator,
				dialogCoordinator,
				codeNavigationCoordinator,
				composition.MenuService,
				composition.ToolBarService,
				composition.StatusBarService,
				composition.MessageService,
				updateUi,
				scriptingMessageService.Build,
				scriptingMessageService.ShowDocumentation);

			editorLifecycleService = new StudioEditorLifecycleCoordinator(
				composition.DocumentController,
				composition.Messenger,
				scriptingMessageService.ApplySettingsToEditor,
				commandRouter.ExecuteCommand,
				commandRouter.CanExecuteCommand,
				composition.KeyBindingService);
		}
		catch
		{
			editorLifecycleService?.Dispose();
			scriptingMessageService?.Dispose();
			luaWorkbenchEventCoordinator?.Dispose();
			codeNavigationCoordinator?.Dispose();
			textEditorDiagnosticsCoordinator?.Dispose();
			layoutCoordinator?.Dispose();
			dialogCoordinator?.Dispose();
			throw;
		}

		_dialogCoordinator = dialogCoordinator ?? throw new InvalidOperationException("Workbench dialog coordinator was not created.");
		_layoutCoordinator = layoutCoordinator ?? throw new InvalidOperationException("Workbench layout coordinator was not created.");
		_paneCoordinator = paneCoordinator ?? throw new InvalidOperationException("Workbench pane coordinator was not created.");
		_textEditorDiagnosticsCoordinator = textEditorDiagnosticsCoordinator
			?? throw new InvalidOperationException("Workbench diagnostics coordinator was not created.");
		_codeNavigationCoordinator = codeNavigationCoordinator
			?? throw new InvalidOperationException("Workbench code navigation coordinator was not created.");
		_luaWorkbenchEventCoordinator = luaWorkbenchEventCoordinator;
		_scriptingMessageService = scriptingMessageService
			?? throw new InvalidOperationException("Workbench scripting message service was not created.");
		_commandRouter = commandRouter ?? throw new InvalidOperationException("Workbench command router was not created.");
		_editorLifecycleService = editorLifecycleService
			?? throw new InvalidOperationException("Workbench editor lifecycle service was not created.");
	}

	public IEditorLifecycleService EditorLifecycleService => _editorLifecycleService;

	public ScriptingMessageService ScriptingMessageService => _scriptingMessageService;

	public WorkbenchDialogCoordinator DialogCoordinator => _dialogCoordinator;

	public WorkbenchLayoutCoordinator LayoutCoordinator => _layoutCoordinator;

	public WorkbenchPaneCoordinator PaneCoordinator => _paneCoordinator;

	public TextEditorDiagnosticsCoordinator TextEditorDiagnosticsCoordinator => _textEditorDiagnosticsCoordinator;

	public WorkbenchCodeNavigationCoordinator CodeNavigationCoordinator => _codeNavigationCoordinator;

	public WorkbenchCommandRouter CommandRouter => _commandRouter;

	public LuaWorkbenchEventCoordinator? LuaWorkbenchEventCoordinator => _luaWorkbenchEventCoordinator;

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_layoutCoordinator.Dispose();
		_codeNavigationCoordinator.Dispose();
		_luaWorkbenchEventCoordinator?.Dispose();
		_textEditorDiagnosticsCoordinator.Dispose();
		_scriptingMessageService.Dispose();
		_editorLifecycleService.Dispose();
		_dialogCoordinator.Dispose();
	}
}

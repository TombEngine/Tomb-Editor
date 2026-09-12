#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using Nickelony.IDEKit.KeyBindings;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchComposition
{
	public WorkbenchComposition(
		ScriptingWorkspaceProfile workspaceProfile,
		IScriptingProjectContext projectContext,
		IMessenger messenger,
		IMessageService messageService,
		IKeyBindingService<UICommand> keyBindingService,
		IMenuService menuService,
		IToolBarService toolBarService,
		IStatusBarService statusBarService,
		IPaneHostService paneHostService,
		IWin32DialogOwnerProvider dialogOwnerProvider,
		IEditorDocumentController documentController,
		IAvalonDockHost dockHost,
		PaneCatalog paneCatalog,
		FindAndReplaceViewModel findAndReplaceViewModel,
		LuaHostServices? luaHostServices,
		IDialogService dialogService,
		Func<bool> showCompilerLogsAfterBuild,
		Func<bool> useNewIncludeMethod,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices,
		StudioFileExplorerDocumentSyncService fileSyncService,
		IWorkspaceDocumentManager? documentManager = null)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(keyBindingService);
		ArgumentNullException.ThrowIfNull(menuService);
		ArgumentNullException.ThrowIfNull(toolBarService);
		ArgumentNullException.ThrowIfNull(statusBarService);
		ArgumentNullException.ThrowIfNull(paneHostService);
		ArgumentNullException.ThrowIfNull(dialogOwnerProvider);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneCatalog);
		ArgumentNullException.ThrowIfNull(findAndReplaceViewModel);
		if (workspaceProfile.SupportsLua)
			ArgumentNullException.ThrowIfNull(luaHostServices);
		ArgumentNullException.ThrowIfNull(dialogService);
		ArgumentNullException.ThrowIfNull(showCompilerLogsAfterBuild);
		ArgumentNullException.ThrowIfNull(useNewIncludeMethod);
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);
		ArgumentNullException.ThrowIfNull(fileSyncService);

		WorkspaceProfile = workspaceProfile;
		ProjectContext = projectContext;
		Messenger = messenger;
		MessageService = messageService;
		KeyBindingService = keyBindingService;
		MenuService = menuService;
		ToolBarService = toolBarService;
		StatusBarService = statusBarService;
		PaneHostService = paneHostService;
		DialogOwnerProvider = dialogOwnerProvider;
		DocumentController = documentController;
		DocumentBridge = documentManager;
		DockHost = dockHost;
		PaneCatalog = paneCatalog;
		FindAndReplaceViewModel = findAndReplaceViewModel;
		LuaHostServices = luaHostServices;
		DialogService = dialogService;
		ShowCompilerLogsAfterBuild = showCompilerLogsAfterBuild;
		UseNewIncludeMethod = useNewIncludeMethod;
		LanguageServices = languageServices;
		GameFlowLanguageServices = gameFlowLanguageServices;
		TrxLanguageServices = trxLanguageServices;
		FileSyncService = fileSyncService;
	}

	public ScriptingWorkspaceProfile WorkspaceProfile { get; }
	public IScriptingProjectContext ProjectContext { get; }
	public IMessenger Messenger { get; }
	public IMessageService MessageService { get; }
	public IKeyBindingService<UICommand> KeyBindingService { get; }
	public IMenuService MenuService { get; }
	public IToolBarService ToolBarService { get; }
	public IStatusBarService StatusBarService { get; }
	public IPaneHostService PaneHostService { get; }
	public IWin32DialogOwnerProvider DialogOwnerProvider { get; }
	public IEditorDocumentController DocumentController { get; }
	public IWorkspaceDocumentManager? DocumentBridge { get; }
	public IAvalonDockHost DockHost { get; }
	public PaneCatalog PaneCatalog { get; }
	public FindAndReplaceViewModel FindAndReplaceViewModel { get; }
	public LuaHostServices? LuaHostServices { get; }
	public IDialogService DialogService { get; }
	public Func<bool> ShowCompilerLogsAfterBuild { get; }
	public Func<bool> UseNewIncludeMethod { get; }
	public ClassicScriptLanguageServices LanguageServices { get; }
	public GameFlowLanguageServices GameFlowLanguageServices { get; }
	public TRXLanguageServices TrxLanguageServices { get; }
	public StudioFileExplorerDocumentSyncService FileSyncService { get; }
}

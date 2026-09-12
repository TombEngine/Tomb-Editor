#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using Nickelony.IDEKit.KeyBindings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

internal static class WorkbenchServiceTestFactory
{
	public static WorkbenchService Create(
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
		StudioFileExplorerDocumentSyncService? fileSyncService = null)
		=> new(new WorkbenchComposition(
			workspaceProfile,
			projectContext,
			messenger,
			messageService,
			keyBindingService,
			menuService,
			toolBarService,
			statusBarService,
			paneHostService,
			dialogOwnerProvider,
			documentController,
			dockHost,
			paneCatalog,
			findAndReplaceViewModel,
			luaHostServices,
			dialogService,
			showCompilerLogsAfterBuild,
			useNewIncludeMethod,
			languageServices,
			gameFlowLanguageServices,
			trxLanguageServices,
			fileSyncService ?? new StudioFileExplorerDocumentSyncService()));
}

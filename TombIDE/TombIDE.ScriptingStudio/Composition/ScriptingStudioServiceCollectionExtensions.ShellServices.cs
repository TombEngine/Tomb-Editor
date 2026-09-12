#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using Nickelony.IDEKit.KeyBindings;
using System;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Registers the shell-scoped services and the Lua host service composition.
/// </summary>
public static partial class ScriptingStudioServiceCollectionExtensions
{
	/// <summary>
	/// Registers shell-scoped services. These are resolved once per
	/// <see cref="IScriptingStudioShell"/> instance via a child scope.
	/// </summary>
	internal static void AddScriptingStudioShellServices(IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		// Scoped context and input manager.
		services.AddScoped<ScriptingStudioShellContext>();
		services.AddScoped<IScriptingProjectContext>(sp =>
			sp.GetRequiredService<ScriptingStudioShellContext>().ProjectContext);

		// Dialog owner provider (set once by Mount).
		services.AddScoped<IWin32DialogOwnerProvider, Win32DialogOwnerProvider>();

		// Settings manager for delegate parameters (populated by RootShellViewModel).
		services.AddScoped<ShellWorkbenchSettings>();

		// Text editor host adapter (bridges document controller to the editor host interface).
		services.AddScoped<ITextEditorHost>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var documentManager = sp.GetRequiredService<IWorkspaceDocumentManager>();
			return new DocumentControllerTextEditorHost(documentController, documentManager);
		});

		// Settings store.
		services.AddScoped<IScriptingStudioShellSettingsStore>(_ => new XmlScriptingStudioShellSettingsStore());

		// Workspace profile (depends on project context, settings store, and language services).
		services.AddScoped<ScriptingWorkspaceProfile>(sp =>
		{
			IScriptingProjectContext projectContext =
				sp.GetRequiredService<IScriptingProjectContext>();
			IScriptingStudioShellSettingsStore settingsStore =
				sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var classicScriptServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxServices = sp.GetRequiredService<TRXLanguageServices>();
			return ScriptingWorkspaceProfileSelector.Create(projectContext, settingsStore, classicScriptServices, gameFlowServices, trxServices);
		});

		// Shortcut binding service (shell-scoped, merges catalog defaults with persisted overrides).
		services.AddScoped<IKeyBindingService<UICommand>>(sp =>
		{
			var catalog = sp.GetRequiredService<CommandCatalog<UICommand>>();
			var settingsStore = sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();

			KeyBindingOverrideCollection overrides = settingsStore.Load(profile).ShortcutOverrides;

			return new KeyBindingService<UICommand>(catalog, overrides, newOverrides =>
				settingsStore.SaveShortcutOverrides(profile.Kind, newOverrides));
		});

		// Chrome services - each owns one builder/view.
		services.AddScoped<IMenuService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var keyBindingService = sp.GetRequiredService<IKeyBindingService<UICommand>>();
			return new MenuService(profile, keyBindingService);
		});
		services.AddScoped<IToolBarService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var keyBindingService = sp.GetRequiredService<IKeyBindingService<UICommand>>();
			return new ToolBarService(profile, keyBindingService);
		});
		services.AddScoped<IStatusBarService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var statusStripContributionService = sp.GetRequiredService<StudioStatusStripContributionService>();
			return new StatusBarService(profile, statusStripContributionService);
		});
		services.AddScoped<IPaneHostService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			return new PaneVisibilityStateService(profile, menuService, toolBarService);
		});

		// Pane infrastructure.
		services.AddScoped<StudioFileExplorerDocumentSyncService>();

		// ViewModels.
		services.AddScoped<DocumentOutlineViewModel>(sp =>
		{
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			return new DocumentOutlineViewModel(localizationService);
		});
		services.AddScoped<ReferenceBrowserViewModel>(sp =>
		{
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			return new ReferenceBrowserViewModel(messageService, localizationService);
		});
		services.AddScoped<FileExplorerViewModel>(sp =>
		{
			var dialogService = sp.GetRequiredService<IDialogService>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			var dialogOwnerProvider = sp.GetRequiredService<IWin32DialogOwnerProvider>();
			var documentManager = sp.GetRequiredService<IWorkspaceDocumentManager>();
			return new FileExplorerViewModel(dialogService, messageService, localizationService, dialogOwnerProvider, documentManager);
		});

		// Find and replace.
		services.AddScoped<FindReplaceService>();
		services.AddScoped<FindAndReplaceViewModel>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var service = sp.GetRequiredService<FindReplaceService>();
			return new FindAndReplaceViewModel(documentController, messenger, service);
		});

		// Reference info dialog.
		services.AddScoped<ReferenceInfoViewModel>(sp =>
		{
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			return new ReferenceInfoViewModel(
				workbenchSettings.GetInfoBoxAlwaysOnTop,
				workbenchSettings.SetInfoBoxAlwaysOnTop,
				workbenchSettings.GetInfoBoxCloseTabsOnClose,
				workbenchSettings.SetInfoBoxCloseTabsOnClose);
		});

		services.AddScoped<IStudioPaneContributionProvider, CompilerLogsPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider, SearchResultsPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var viewModel = sp.GetRequiredService<DocumentOutlineViewModel>();
			return new DocumentOutlinePaneProvider(documentController, viewModel);
		});
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var viewModel = sp.GetRequiredService<FileExplorerViewModel>();
			var fileSyncService = sp.GetRequiredService<StudioFileExplorerDocumentSyncService>();
			return new FileExplorerPaneProvider(profile, documentController, viewModel, fileSyncService);
		});
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var viewModel = sp.GetRequiredService<ReferenceBrowserViewModel>();
			var referenceInfoViewModel = sp.GetRequiredService<ReferenceInfoViewModel>();
			return new ReferenceBrowserPaneProvider(profile, viewModel, referenceInfoViewModel);
		});
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			return new DocumentDiagnosticsPaneProvider(
				sp.GetRequiredService<ScriptingWorkspaceProfile>(),
				sp.GetRequiredService<IEditorDocumentController>());
		});
		services.AddScoped<LuaReferencesPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			if (sp.GetRequiredService<ScriptingWorkspaceProfile>().SupportsLua)
				return sp.GetRequiredService<LuaReferencesPaneProvider>();

			return new StaticStudioPaneContributionProvider([]);
		});
		services.AddScoped<PaneCatalog>();

		// Editor document controller factory.
		services.AddScoped<IEditorDocumentControllerFactory, EditorDocumentControllerFactory>();

		// Editor document controller (created once per scope via factory).
		services.AddScoped<IEditorDocumentController>(sp =>
		{
			var factory = sp.GetRequiredService<IEditorDocumentControllerFactory>();
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var messageService = sp.GetRequiredService<IMessageService>();
			return factory.Create(profile, projectContext, messageService);
		});

		// AvalonDock host adapter.
		services.AddScoped<IAvalonDockHost>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var paneCatalog = sp.GetRequiredService<PaneCatalog>();
			var view = new StudioAvalonDockHostView(documentController, paneCatalog.Panes);
			return new AvalonDockHostAdapter(view);
		});

		// Workbench composition (receives delegates from ShellWorkbenchSettings).
		services.AddScoped<WorkbenchComposition>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var keyBindingService = sp.GetRequiredService<IKeyBindingService<UICommand>>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			var statusBarService = sp.GetRequiredService<IStatusBarService>();
			var paneHostService = sp.GetRequiredService<IPaneHostService>();
			var dialogOwnerProvider = sp.GetRequiredService<IWin32DialogOwnerProvider>();
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var documentManager = sp.GetRequiredService<IWorkspaceDocumentManager>();
			var dockHost = sp.GetRequiredService<IAvalonDockHost>();
			var paneCatalog = sp.GetRequiredService<PaneCatalog>();
			var findAndReplaceViewModel = sp.GetRequiredService<FindAndReplaceViewModel>();
			LuaHostServices? luaHostServices = CreateLuaHostServices(sp, profile);
			var dialogService = sp.GetRequiredService<IDialogService>();
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowLanguageServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxLanguageServices = sp.GetRequiredService<TRXLanguageServices>();
			var fileSyncService = sp.GetRequiredService<StudioFileExplorerDocumentSyncService>();

			return new WorkbenchComposition(
				profile,
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
				workbenchSettings.ShowCompilerLogsAfterBuild,
				workbenchSettings.UseNewIncludeMethod,
				languageServices,
				gameFlowLanguageServices,
				trxLanguageServices,
				fileSyncService,
				documentManager);
		});
		services.AddScoped<IWorkbenchService>(sp =>
			new WorkbenchService(sp.GetRequiredService<WorkbenchComposition>()));
		// Shell ViewModel (receives all interfaces via constructor injection).
		services.AddScoped<RootShellViewModel>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var settingsStore = sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			var statusBarService = sp.GetRequiredService<IStatusBarService>();
			var paneHostService = sp.GetRequiredService<IPaneHostService>();
			var workbenchService = sp.GetRequiredService<IWorkbenchService>();
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowLanguageServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxLanguageServices = sp.GetRequiredService<TRXLanguageServices>();

			return new RootShellViewModel(
				profile,
				projectContext,
				settingsStore,
				messenger,
				messageService,
				localizationService,
				menuService,
				toolBarService,
				statusBarService,
				paneHostService,
				workbenchService,
				documentController,
				workbenchSettings,
				languageServices,
				gameFlowLanguageServices,
				trxLanguageServices);
		});
	}

	internal static LuaHostServices? CreateLuaHostServices(
		IServiceProvider serviceProvider,
		ScriptingWorkspaceProfile profile)
	{
		if (!profile.SupportsLua)
			return null;

		return new LuaHostServices(
			serviceProvider.GetRequiredService<ILuaEditorLifecycleService>(),
			serviceProvider.GetRequiredService<ILuaIntellisenseBridge>(),
			serviceProvider.GetRequiredService<LuaTrackedDocumentStateService>(),
			serviceProvider.GetRequiredService<LuaReferenceSearchService>(),
			serviceProvider.GetRequiredService<TextWorkspaceCommandService>());
	}
}

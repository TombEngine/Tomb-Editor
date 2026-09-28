#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Shell;

public sealed partial class RootShellViewModel : ObservableObject, IDisposable
{
	private readonly IMenuService _menuService;
	private readonly IToolBarService _toolBarService;
	private readonly IStatusBarService _statusBarService;
	private readonly IPaneHostService _paneHostService;
	private readonly IMessenger _messenger;
	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;
	private readonly IScriptingStudioShellSettingsStore _settingsStore;
	private readonly IWorkbenchService _workbenchService;
	private readonly IEditorDocumentController _documentController;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly ClassicScriptLanguageServices _languageServices;
	private readonly GameFlowLanguageServices _gameFlowLanguageServices;
	private readonly TRXLanguageServices _trxLanguageServices;

	private string _avalonDockLayoutXml = string.Empty;
	private ScriptingStudioShellWorkspaceSettings _settings = new();
	private bool _disposed;

	internal RootShellViewModel(
		ScriptingWorkspaceProfile workspaceProfile,
		IScriptingProjectContext projectContext,
		IScriptingStudioShellSettingsStore settingsStore,
		IMessenger messenger,
		IMessageService messageService,
		ILocalizationService localizationService,
		IMenuService menuService,
		IToolBarService toolBarService,
		IStatusBarService statusBarService,
		IPaneHostService paneHostService,
		IWorkbenchService workbenchService,
		IEditorDocumentController documentController,
		ShellWorkbenchSettings workbenchSettings,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(settingsStore);
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(localizationService);
		ArgumentNullException.ThrowIfNull(menuService);
		ArgumentNullException.ThrowIfNull(toolBarService);
		ArgumentNullException.ThrowIfNull(statusBarService);
		ArgumentNullException.ThrowIfNull(paneHostService);
		ArgumentNullException.ThrowIfNull(workbenchService);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(workbenchSettings);
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);

		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
		_workspaceProfile = workspaceProfile;
		_settingsStore = settingsStore;
		_menuService = menuService;
		_toolBarService = toolBarService;
		_statusBarService = statusBarService;
		_paneHostService = paneHostService;
		_workbenchService = workbenchService;
		_documentController = documentController;
		_messenger = messenger;
		_messageService = messageService;
		_localizationService = localizationService.WithKeysFor(this);

		_menuService.CommandInvoked += HandleChromeCommandInvoked;
		_toolBarService.CommandInvoked += HandleChromeCommandInvoked;

		workbenchSettings.GetInfoBoxAlwaysOnTop = () => _settings.InfoBoxAlwaysOnTop;
		workbenchSettings.SetInfoBoxAlwaysOnTop = SetInfoBoxAlwaysOnTop;
		workbenchSettings.GetInfoBoxCloseTabsOnClose = () => _settings.InfoBoxCloseTabsOnClose;
		workbenchSettings.SetInfoBoxCloseTabsOnClose = SetInfoBoxCloseTabsOnClose;
		workbenchSettings.ShowCompilerLogsAfterBuild = () => ShowCompilerLogsAfterBuild;
		workbenchSettings.UseNewIncludeMethod = () => UseNewIncludeMethod;

		MenuView = _menuService.MenuView;
		ToolBarView = _toolBarService.ToolBarView;
		StatusBarView = _statusBarService.StatusBarView;
		WorkbenchView = _workbenchService.WorkbenchView;
		ScriptRootDirectoryPath = projectContext.ScriptRootDirectoryPath;
		ShellTitle = workspaceProfile.SettingsPages.Count > 0
			? workspaceProfile.SettingsPages[0].Title
			: workspaceProfile.Kind.ToString();
		SupportsBuild = workspaceProfile.SupportsBuild;
		SupportsUseNewIncludeMethod = workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript;

		LoadSettings();
	}

	public FrameworkElement MenuView { get; }

	public string ScriptRootDirectoryPath { get; }

	public string ShellTitle { get; }

	public FrameworkElement StatusBarView { get; }

	public bool SupportsBuild { get; }

	public bool SupportsUseNewIncludeMethod { get; }

	public FrameworkElement ToolBarView { get; }

	public FrameworkElement WorkbenchView { get; }

	[ObservableProperty]
	private bool _isStatusStripVisible;

	[ObservableProperty]
	private bool _isToolStripVisible;

	[ObservableProperty]
	private bool _reindentOnSave;

	[ObservableProperty]
	private bool _showCompilerLogsAfterBuild;

	[ObservableProperty]
	private bool _useNewIncludeMethod;

	[RelayCommand]
	private void RestoreDefaultLayout()
	{
		ScriptingStudioShellWorkspaceSettings defaults = _settingsStore.CreateDefault(_workspaceProfile);
		_settings.DockPanelState = defaults.DockPanelState;
		_avalonDockLayoutXml = string.Empty;
		IsToolStripVisible = defaults.IsToolStripVisible;
		IsStatusStripVisible = defaults.IsStatusStripVisible;
		_paneHostService.ResetPaneVisibilityStates();
		_workbenchService.RestoreDefaultLayout();

		TrySaveSettings();
	}

	[RelayCommand]
	private void SaveSettings()
		=> TrySaveSettings();

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_menuService.CommandInvoked -= HandleChromeCommandInvoked;
		_toolBarService.CommandInvoked -= HandleChromeCommandInvoked;
	}

	private ScriptingStudioShellWorkspaceSettings BuildSettingsSnapshot() => new()
	{
		AvalonDockLayoutXml = _workbenchService.CaptureLayout(),
		DockPanelState = _settings.DockPanelState,
		InfoBoxAlwaysOnTop = _settings.InfoBoxAlwaysOnTop,
		InfoBoxCloseTabsOnClose = _settings.InfoBoxCloseTabsOnClose,
		LuaEnabled = _settings.LuaEnabled,
		IsStatusStripVisible = IsStatusStripVisible,
		IsToolStripVisible = IsToolStripVisible,
		ReindentOnSave = ReindentOnSave,
		ShowCompilerLogsAfterBuild = ShowCompilerLogsAfterBuild,
		UseNewIncludeMethod = UseNewIncludeMethod
	};

	private void LoadSettings()
	{
		_settings = _settingsStore.Load(_workspaceProfile);
		_avalonDockLayoutXml = _settings.AvalonDockLayoutXml;
		IsStatusStripVisible = _settings.IsStatusStripVisible;
		IsToolStripVisible = _settings.IsToolStripVisible;
		ReindentOnSave = _settings.ReindentOnSave;
		ShowCompilerLogsAfterBuild = _settings.ShowCompilerLogsAfterBuild;
		UseNewIncludeMethod = _settings.UseNewIncludeMethod;

		ApplyShellStateToChrome();
	}

	private void ApplyShellStateToChrome()
	{
		_menuService.SetCommandChecked(UICommand.UseNewInclude, _settings.UseNewIncludeMethod);
		_menuService.SetCommandChecked(UICommand.ShowLogsAfterBuild, _settings.ShowCompilerLogsAfterBuild);
		_menuService.SetCommandChecked(UICommand.ToolStrip, _settings.IsToolStripVisible);
		_menuService.SetCommandChecked(UICommand.StatusStrip, _settings.IsStatusStripVisible);

		_toolBarService.SetCommandChecked(UICommand.UseNewInclude, _settings.UseNewIncludeMethod);
		_toolBarService.SetCommandChecked(UICommand.ShowLogsAfterBuild, _settings.ShowCompilerLogsAfterBuild);
		_toolBarService.SetCommandChecked(UICommand.ToolStrip, _settings.IsToolStripVisible);
		_toolBarService.SetCommandChecked(UICommand.StatusStrip, _settings.IsStatusStripVisible);
	}

	partial void OnIsStatusStripVisibleChanged(bool value)
	{
		_menuService.SetCommandChecked(UICommand.StatusStrip, value);
		_toolBarService.SetCommandChecked(UICommand.StatusStrip, value);
	}

	partial void OnIsToolStripVisibleChanged(bool value)
	{
		_menuService.SetCommandChecked(UICommand.ToolStrip, value);
		_toolBarService.SetCommandChecked(UICommand.ToolStrip, value);
	}

	private void HandleChromeCommandInvoked(object? sender, StudioCommandInvokedEventArgs e)
	{
		if (_workbenchService.TryExecuteCommand(e.Command))
			return;

		switch (e.Command)
		{
			case UICommand.UseNewInclude when SupportsUseNewIncludeMethod:
				UseNewIncludeMethod = !UseNewIncludeMethod;
				TrySaveSettings();
				break;

			case UICommand.ShowLogsAfterBuild when SupportsBuild:
				ShowCompilerLogsAfterBuild = !ShowCompilerLogsAfterBuild;
				TrySaveSettings();
				break;

			case UICommand.ToolStrip:
				IsToolStripVisible = !IsToolStripVisible;
				TrySaveSettings();
				break;

			case UICommand.StatusStrip:
				IsStatusStripVisible = !IsStatusStripVisible;
				TrySaveSettings();
				break;

			case UICommand.Settings:
				if (ShowSettingsDialog())
					_workbenchService.ApplyEditorSettings();
				break;

			case UICommand.Exit:
				_messenger.Send(new ScriptingRequestCloseMessage());
				break;

			case UICommand.RestoreDefaultLayout:
				RestoreDefaultLayout();
				break;

			default:
				if (_paneHostService.IsPaneVisibilityCommand(e.Command))
					_paneHostService.TogglePaneVisibility(e.Command);
				break;
		}
	}

	internal void NotifyHostTabActivated()
		=> _workbenchService.EnsureTabFileSynchronization();

	internal void NotifyMainWindowFocusChanged(bool isFocused)
		=> _workbenchService.NotifyMainWindowFocusChanged(isFocused);

	private bool TrySaveSettings()
	{
		try
		{
			_settings = BuildSettingsSnapshot();
			_settingsStore.Save(_workspaceProfile.Kind, _settings);
			ApplyShellStateToChrome();
			_messenger.Send(new ScriptingSettingsChangedMessage(_workspaceProfile.Kind.ToString()));
			return true;
		}
		catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
		{
			_messageService.ShowError(ex.Message, _localizationService["~TombIDE.Error"]);
			return false;
		}
	}

	private void SetInfoBoxAlwaysOnTop(bool value)
	{
		_settings.InfoBoxAlwaysOnTop = value;
		TrySaveSettings();
	}

	private void SetInfoBoxCloseTabsOnClose(bool value)
	{
		_settings.InfoBoxCloseTabsOnClose = value;
		TrySaveSettings();
	}

	private bool ShowSettingsDialog()
	{
		var viewModel = new ScriptingSettingsWindowViewModel(
			_workspaceProfile,
			_documentController.CurrentDocumentContext.Registration,
			_settings.LuaEnabled,
			_languageServices,
			_gameFlowLanguageServices,
			_trxLanguageServices);
		var window = new ScriptingSettingsWindow
		{
			DataContext = viewModel
		};

		if (Form.ActiveForm is Form ownerForm)
			new WindowInteropHelper(window).Owner = ownerForm.Handle;

		if (window.ShowDialog() != true)
			return false;

		_settings.LuaEnabled = viewModel.LuaEnabled;
		return TrySaveSettings();
	}
}

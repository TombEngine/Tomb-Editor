#nullable enable

using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchLayoutCoordinator : IDisposable
{
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly IEditorDocumentController _documentController;
	private readonly IAvalonDockHost _dockHost;
	private readonly PaneCatalog _paneCatalog;
	private readonly IPaneHostService _paneHostService;
	private readonly StudioFileExplorerDocumentSyncService _fileSyncService;
	private bool _disposed;

	public WorkbenchLayoutCoordinator(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		IAvalonDockHost dockHost,
		PaneCatalog paneCatalog,
		IPaneHostService paneHostService,
		StudioFileExplorerDocumentSyncService fileSyncService)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneCatalog);
		ArgumentNullException.ThrowIfNull(paneHostService);
		ArgumentNullException.ThrowIfNull(fileSyncService);

		_workspaceProfile = workspaceProfile;
		_documentController = documentController;
		_dockHost = dockHost;
		_paneCatalog = paneCatalog;
		_paneHostService = paneHostService;
		_fileSyncService = fileSyncService;
	}

	public void RestoreInitialLayout(Action updateUi)
	{
		ArgumentNullException.ThrowIfNull(updateUi);

		_dockHost.RestoreLayout(
			_workspaceProfile.LoadAvalonDockLayoutXml(),
			_workspaceProfile.LoadDockPanelState(),
			updateUi);
		_paneCatalog.EnsurePanesRegistered(_dockHost);
		_paneCatalog.UpdatePaneVisibilityChecks(_dockHost, _paneHostService);
	}

	public string CaptureLayout()
		=> _dockHost.SaveLayout();

	public void RestoreDefaultLayout(Action updateUi)
	{
		ArgumentNullException.ThrowIfNull(updateUi);

		_dockHost.RestoreDefaultLayout(_workspaceProfile.LoadDockPanelState(), updateUi);
		_paneCatalog.EnsurePanesRegistered(_dockHost);
		updateUi();
	}

	public void EnsureTabFileSynchronization()
		=> _documentController.EnsureTabFileSynchronization();

	public void NotifyMainWindowFocusChanged(bool isFocused)
		=> _fileSyncService.ApplyWindowFocus(_documentController, isFocused);

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_workspaceProfile.SaveAvalonDockLayoutXml(_dockHost.SaveLayout());
		_dockHost.DetachDocumentController();
	}
}

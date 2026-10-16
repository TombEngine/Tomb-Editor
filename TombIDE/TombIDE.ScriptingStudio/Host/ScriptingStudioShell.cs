#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared.Messaging;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.Host;

internal sealed class ScriptingStudioShell : IScriptingStudioShell
{
	private readonly AsyncServiceScope _shellScope;
	private readonly RootShellHost _host;
	private readonly RootShellViewModel _viewModel;
	private readonly IWorkspaceDocumentManager _documentManager;
	private readonly IUiDispatcherService _uiDispatcher;
	private readonly object _stateLock = new();
	private Task? _stopTask;

	public ScriptingStudioShell(
		AsyncServiceScope shellScope,
		RootShellViewModel viewModel,
		IWorkspaceDocumentManager documentManager,
		IUiDispatcherService uiDispatcher)
	{
		ArgumentNullException.ThrowIfNull(viewModel);
		ArgumentNullException.ThrowIfNull(documentManager);
		ArgumentNullException.ThrowIfNull(uiDispatcher);

		_shellScope = shellScope;
		_viewModel = viewModel;
		_documentManager = documentManager;
		_uiDispatcher = uiDispatcher;

		var view = new RootShellView
		{
			DataContext = _viewModel
		};

		_host = new RootShellHost(view);
	}

	public Task StopAsync()
	{
		lock (_stateLock)
			return _stopTask ??= StopCoreAsync();
	}

	public ValueTask DisposeAsync()
		=> new(StopAsync());

	private async Task StopCoreAsync()
	{
		try
		{
			await _documentManager.StopAsync().ConfigureAwait(false);
		}
		finally
		{
			try
			{
				_uiDispatcher.Invoke(_host.Dispose);
			}
			finally
			{
				await _shellScope.DisposeAsync().ConfigureAwait(false);
			}
		}
	}

	public void Mount(Control hostContainer, Form ownerForm)
	{
		ArgumentNullException.ThrowIfNull(hostContainer);
		ArgumentNullException.ThrowIfNull(ownerForm);

		IWin32DialogOwnerProvider dialogOwnerProvider =
			_shellScope.ServiceProvider.GetRequiredService<IWin32DialogOwnerProvider>();
		dialogOwnerProvider.SetOwner(ownerForm);

		if (!hostContainer.Controls.Contains(_host))
			hostContainer.Controls.Add(_host);

		_host.BringToFront();
	}

	public void NotifyHostTabActivated()
		=> _viewModel.NotifyHostTabActivated();

	public void NotifyMainWindowFocusChanged(bool isFocused)
		=> _viewModel.NotifyMainWindowFocusChanged(isFocused);
}

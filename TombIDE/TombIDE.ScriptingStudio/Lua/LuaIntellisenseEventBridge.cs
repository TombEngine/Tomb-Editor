#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaIntellisenseEventBridge : ILuaIntellisenseBridge
{
	private readonly IAvalonDockHost _dockHost;
	private readonly IMessenger _messenger;
	private readonly ILuaLanguageServerIntelliSenseProvider _intellisenseProvider;

	public LuaIntellisenseEventBridge(
		IAvalonDockHost dockHost,
		IMessenger messenger,
		ILuaLanguageServerIntelliSenseProvider intellisenseProvider)
	{
		_dockHost = dockHost ?? throw new ArgumentNullException(nameof(dockHost));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
		_intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));
	}

	public void Attach()
	{
		Detach();

		_intellisenseProvider.DiagnosticsUpdated += IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated += IntellisenseProvider_SemanticTokensUpdated;
		_intellisenseProvider.CapabilitiesChanged += IntellisenseProvider_CapabilitiesChanged;
		_intellisenseProvider.StartupFailed += IntellisenseProvider_StartupFailed;
		_intellisenseProvider.WorkspaceWatcherFailed += IntellisenseProvider_WorkspaceWatcherFailed;
	}

	public void Detach()
	{
		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;
		_intellisenseProvider.CapabilitiesChanged -= IntellisenseProvider_CapabilitiesChanged;
		_intellisenseProvider.StartupFailed -= IntellisenseProvider_StartupFailed;
		_intellisenseProvider.WorkspaceWatcherFailed -= IntellisenseProvider_WorkspaceWatcherFailed;
	}

	public void Dispose()
		=> Detach();

	private void IntellisenseProvider_DiagnosticsUpdated(object? sender, DiagnosticsUpdatedEventArgs eventArgs)
		=> DispatchToUi(() => _messenger.Send(new LuaDiagnosticsUpdatedMessage(new LuaDiagnosticsPayload(eventArgs.FilePath, eventArgs.Diagnostics))));

	private void IntellisenseProvider_SemanticTokensUpdated(object? sender, SemanticTokensUpdatedEventArgs eventArgs)
		=> DispatchToUi(() => _messenger.Send(new LuaSemanticTokensUpdatedMessage(new LuaSemanticTokensPayload(eventArgs.FilePath, eventArgs.SemanticTokens))));

	private void IntellisenseProvider_CapabilitiesChanged(object? sender, EventArgs e)
		=> DispatchToUi(() => _messenger.Send(new ShellUiRefreshMessage()));

	private void IntellisenseProvider_StartupFailed(object? sender, StartupFailedEventArgs eventArgs)
		=> DispatchToUi(() => _messenger.Send(new LuaStartupFailedMessage(eventArgs.Failure)));

	private void IntellisenseProvider_WorkspaceWatcherFailed(object? sender, WorkspaceWatcherFailedEventArgs eventArgs)
		=> DispatchToUi(() => _messenger.Send(new LuaWorkspaceWatcherFailedMessage(eventArgs.Failure)));

	private void DispatchToUi(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		if (_dockHost.Dispatcher.HasShutdownStarted || _dockHost.Dispatcher.HasShutdownFinished)
			return;

		if (_dockHost.Dispatcher.CheckAccess())
		{
			action();
			return;
		}

		_ = _dockHost.Dispatcher.BeginInvoke(action);
	}
}

#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.AvalonEdit.Navigation;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class LuaWorkbenchEventCoordinator : IDisposable
{
	private const string MissingLuaLanguageServerMessage =
		"The bundled Lua language server could not be found. Lua IntelliSense is unavailable.";
	private const string ProviderMissingLuaLanguageServerMessage =
		"The Lua language server executable is unavailable, so Lua IntelliSense is disabled until the application provides a valid server installation.";

	private readonly IMessenger _messenger;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly IEditorDocumentController _documentController;
	private readonly WorkbenchPaneCoordinator _paneCoordinator;
	private readonly LuaHostServices _luaHostServices;
	private readonly IMessageService _messageService;
	private bool _disposed;

	public LuaWorkbenchEventCoordinator(
		IMessenger messenger,
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		WorkbenchPaneCoordinator paneCoordinator,
		LuaHostServices luaHostServices,
		IMessageService messageService)
	{
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(paneCoordinator);
		ArgumentNullException.ThrowIfNull(luaHostServices);
		ArgumentNullException.ThrowIfNull(messageService);

		_messenger = messenger;
		_workspaceProfile = workspaceProfile;
		_documentController = documentController;
		_paneCoordinator = paneCoordinator;
		_luaHostServices = luaHostServices;
		_messageService = messageService;

		_messenger.Register<LuaDefinitionNavigationMessage>(this, (_, m) => NavigateToLuaDefinition(m.Value));
		_messenger.Register<LuaDiagnosticsUpdatedMessage>(this, (_, m) => HandleLuaDiagnosticsUpdated(m.Value));
		_messenger.Register<LuaSemanticTokensUpdatedMessage>(this, (_, m) => HandleLuaSemanticTokensUpdated(m.Value));
		_messenger.Register<LuaStartupFailedMessage>(this, (_, m) => HandleLuaStartupFailed(m.Value));
		_messenger.Register<LuaWorkspaceWatcherFailedMessage>(this, (_, m) => HandleLuaWorkspaceWatcherFailed(m.Value));
	}

	private void HandleLuaDiagnosticsUpdated(LuaDiagnosticsPayload payload)
	{
		_luaHostServices.TrackedDocumentStateService.ApplyDiagnosticsUpdate(payload.FilePath, payload.Diagnostics);

		ScriptingDocumentContext documentContext = _documentController.CurrentDocumentContext ?? ScriptingDocumentContext.Empty;
		if (documentContext.Registration?.DocumentMode != DocumentMode.Lua
			|| documentContext.Editor is not LuaEditor luaEditor
			|| !string.Equals(luaEditor.FilePath, payload.FilePath, StringComparison.OrdinalIgnoreCase))
			return;

		_paneCoordinator.ShowLuaDiagnostics(luaEditor.FilePath, luaEditor.Document, payload.Diagnostics);
	}

	private void HandleLuaSemanticTokensUpdated(LuaSemanticTokensPayload payload)
		=> _luaHostServices.TrackedDocumentStateService.ApplySemanticTokensUpdate(payload.FilePath, payload.SemanticTokens);

	private void HandleLuaStartupFailed(LanguageServerStartupFailure failure)
	{
		string message = failure.IsPersistent
			&& string.Equals(failure.Message, ProviderMissingLuaLanguageServerMessage, StringComparison.Ordinal)
			? MissingLuaLanguageServerMessage
			: failure.Message;

		if (failure.IsPersistent)
			_messageService.ShowError(message, "Lua IntelliSense");
		else
			_messageService.ShowInformation(message, "Lua IntelliSense");

		_messenger.Send(new CommandStateRefreshMessage());
	}

	private void HandleLuaWorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
		=> _messageService.ShowInformation(failure.Message, "Lua IntelliSense");

	private void NavigateToLuaDefinition(TextDefinitionLocation location)
	{
		ArgumentNullException.ThrowIfNull(location);

		string? targetFilePath = !string.IsNullOrWhiteSpace(location.FilePath)
			? location.FilePath
			: (_documentController.CurrentEditor as TextEditorBase)?.FilePath;

		if (string.IsNullOrWhiteSpace(targetFilePath))
			return;

		_documentController.OpenFile(targetFilePath);

		if (_documentController.CurrentEditor is TextEditorBase textEditor)
		{
			EditorNavigationHelper.ApplyLocation(
				textEditor,
				EditorNavigationHelper.CreateDefinitionLocation(textEditor, targetFilePath, location.LineNumber, location.ColumnNumber));
		}
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_messenger.UnregisterAll(this);
	}
}

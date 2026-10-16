#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;

namespace TombIDE.Shared.Messaging.Scripting;

/// <summary>
/// Publishes typed scripting messages from the legacy <see cref="IDE"/> event lane.
/// </summary>
public sealed class IdeScriptingMessageBridge : IDisposable
{
	private readonly IDE _ide;
	private readonly IMessenger _messenger;
	private readonly IUiDispatcherService _uiDispatcherService;

	/// <summary>
	/// Initializes a new IDE-to-messenger compatibility bridge with an explicit messenger.
	/// </summary>
	/// <param name="ide">The legacy IDE event source.</param>
	/// <param name="uiDispatcherService">The dispatcher used to preserve UI-thread delivery.</param>
	/// <param name="messenger">The messenger instance to publish to.</param>
	public IdeScriptingMessageBridge(IDE ide, IUiDispatcherService uiDispatcherService, IMessenger messenger)
	{
		_ide = ide ?? throw new ArgumentNullException(nameof(ide));
		_uiDispatcherService = uiDispatcherService ?? throw new ArgumentNullException(nameof(uiDispatcherService));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

		_ide.IDEEventRaised += OnIdeEventRaised;
	}

	/// <inheritdoc/>
	public void Dispose()
		=> _ide.IDEEventRaised -= OnIdeEventRaised;

	private void OnIdeEventRaised(IIDEEvent ideEvent)
	{
		if (ideEvent is null)
			return;

		_uiDispatcherService.Invoke(() => Publish(ideEvent));
	}

	private void Publish(IIDEEvent ideEvent)
	{
		switch (ideEvent)
		{
			case IDE.ProgramClosingEvent closingEvent:
				PublishCanCloseRequest(closingEvent);
				break;

			case IDE.RequestProgramCloseEvent:
				_messenger.Send(new ScriptingRequestCloseMessage());
				break;

			case IDE.ProjectScriptPathChangedEvent pathChangedEvent:
				_messenger.Send(new ScriptingScriptPathChangedMessage(pathChangedEvent.OldPath ?? string.Empty, pathChangedEvent.NewPath ?? string.Empty));
				break;

			case IDE.ProjectLevelsPathChangedEvent pathChangedEvent:
				_messenger.Send(new ScriptingLevelsPathChangedMessage(pathChangedEvent.OldPath ?? string.Empty, pathChangedEvent.NewPath ?? string.Empty));
				break;

			case IDE.ScriptEditor_AppendScriptEvent appendScriptEvent when appendScriptEvent.Result is not null:
				_messenger.Send(new ScriptingAppendScriptRequestedMessage(appendScriptEvent.Result));
				break;

			case IDE.ScriptEditor_AddNewLevelStringEvent addLevelStringEvent:
				_messenger.Send(new ScriptingAddLevelStringRequestedMessage(addLevelStringEvent.LevelName ?? string.Empty));
				break;

			case IDE.ScriptEditor_AddNewPluginEntryEvent addPluginEntryEvent:
				_messenger.Send(new ScriptingAddPluginEntryRequestedMessage(addPluginEntryEvent.PluginString ?? string.Empty));
				break;

			case IDE.ScriptEditor_AddNewNGStringEvent addNgStringEvent:
				_messenger.Send(new ScriptingAddNgStringRequestedMessage(addNgStringEvent.NGString ?? string.Empty));
				break;

			case IDE.ScriptEditor_ContentChangedEvent:
				_messenger.Send(new ScriptingExternalContentChangedMessage());
				break;

			case IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent:
				PublishScriptPresenceRequest(scriptPresenceEvent);
				break;

			case IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent:
				PublishStringPresenceRequest(stringPresenceEvent);
				break;

			case IDE.ScriptEditor_RenameLevelEvent renameLevelEvent:
				_messenger.Send(new ScriptingRenameLevelRequestedMessage(renameLevelEvent.OldName ?? string.Empty, renameLevelEvent.NewName ?? string.Empty));
				break;

			case IDE.ScriptEditor_ReloadSyntaxHighlightingEvent:
				_messenger.Send(new ScriptingReloadSyntaxHighlightingRequestedMessage());
				break;
		}
	}

	private void PublishCanCloseRequest(IDE.ProgramClosingEvent closingEvent)
	{
		var request = new ScriptingCanCloseRequestMessage();
		_messenger.Send(request);

		if (request.HasReceivedResponse)
			closingEvent.CanClose = closingEvent.CanClose && request.Response;
	}

	private void PublishScriptPresenceRequest(IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent)
	{
		var request = new ScriptingIsScriptDefinedRequestMessage(scriptPresenceEvent.LevelName ?? string.Empty);
		_messenger.Send(request);

		if (request.HasReceivedResponse)
			_ide.ScriptDefined = request.Response;
	}

	private void PublishStringPresenceRequest(IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent)
	{
		var request = new ScriptingIsStringDefinedRequestMessage(stringPresenceEvent.String ?? string.Empty);
		_messenger.Send(request);

		if (request.HasReceivedResponse)
			_ide.StringDefined = request.Response;
	}
}

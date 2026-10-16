#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using TombIDE.Shared.Messaging;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.Shared
{
	public interface IIDEEvent
	{ }

	public class IDE : IDisposable
	{
		private readonly IMessenger? _messenger;
		private readonly IUiDispatcherService? _uiDispatcherService;

		/* Initialization */

		public event Action<IIDEEvent> IDEEventRaised;

		public void RaiseEvent(IIDEEvent eventObj) =>
			SynchronizationContext.Current.Send(eventObj_ => IDEEventRaised?.Invoke((IIDEEvent)eventObj_), eventObj);

		public IDEConfiguration IDEConfiguration { get; }
		public List<IGameProject> AvailableProjects { get; }

		/// <summary>
		/// The currently opened TombIDE project.
		/// <para>Note: Set this before the FormMain initialization is finished. DO NOT change it afterwards.</para>
		/// </summary>
		public IGameProject Project { get; set; }

		/* Main IDE events */

		#region BeginEngineUpdate

		public class BeginEngineUpdateEvent : IIDEEvent
		{ }

		public void BeginEngineUpdate() =>
			RaiseEvent(new BeginEngineUpdateEvent());

		#endregion BeginEngineUpdate

		#region ProgramButtonsModified

		public class ProgramButtonsModifiedEvent : IIDEEvent
		{ }

		public void ProgramButtonsModified() =>
			RaiseEvent(new ProgramButtonsModifiedEvent());

		#endregion ProgramButtonsModified

		#region ProgramClosing

		public class ProgramClosingEvent : IIDEEvent
		{
			public bool CanClose { get; set; } // Result
		}

		public class RequestProgramCloseEvent : IIDEEvent
		{ }

		/// <summary>
		/// Asks the Scripting Studio if all files are saved and if the application can be safely closed.
		/// </summary>
		public bool CanClose()
		{
			ScriptingCanCloseRequestMessage? request = TrySendRequest(static messenger =>
			{
				var request = new ScriptingCanCloseRequestMessage();
				messenger.Send(request);
				return request;
			});

			if (request?.HasReceivedResponse == true)
				return request.Response;

			ProgramClosingEvent closingEvent = new ProgramClosingEvent();

			RaiseEvent(closingEvent);

			return closingEvent.CanClose;
		}

		public void RequestProgramClose()
		{
			if (TryPublish(static messenger => messenger.Send(new ScriptingRequestCloseMessage())))
				return;

			RaiseEvent(new RequestProgramCloseEvent());
		}

		#endregion ProgramClosing

		/* Project Master Events */

		#region SelectedLevelChanged

		/// <summary>
		/// The currently selected level in the "Level List" section.
		/// </summary>
		public ILevelProject SelectedLevel
		{
			get { return _selectedLevel; }
			set
			{
				if (_selectedLevel != value)
				{
					_selectedLevel = value;
					RaiseEvent(new SelectedLevelChangedEvent());
				}
			}
		}

		private ILevelProject _selectedLevel;

		public class SelectedLevelChangedEvent : IIDEEvent
		{ }

		#endregion SelectedLevelChanged

		#region SelectedLevelSettingsChanged

		public class SelectedLevelSettingsChangedEvent : IIDEEvent
		{ }

		public void SelectedLevelSettingsChanged() =>
			RaiseEvent(new SelectedLevelSettingsChangedEvent());

		#endregion SelectedLevelSettingsChanged

		#region ProjectScriptPathChanged

		public class ProjectScriptPathChangedEvent : IIDEEvent
		{
			public string OldPath { get; internal set; }
			public string NewPath { get; internal set; }
		}

		public void ChangeScriptFolder(string newPath)
		{
			string oldPath = Project.GetScriptRootDirectory();

			if (newPath != oldPath)
				RaiseEvent(new ProjectScriptPathChangedEvent { OldPath = oldPath, NewPath = newPath });
		}

		#endregion ProjectScriptPathChanged

		#region ProjectLevelsPathChanged

		public class ProjectLevelsPathChangedEvent : IIDEEvent
		{
			public string OldPath { get; internal set; }
			public string NewPath { get; internal set; }
		}

		public void ChangeLevelsFolder(string newPath)
		{
			string oldPath = Project.LevelsDirectoryPath;

			if (newPath != oldPath)
				RaiseEvent(new ProjectLevelsPathChangedEvent { OldPath = oldPath, NewPath = newPath });
		}

		#endregion ProjectLevelsPathChanged

		#region PRJ2FileDeleted

		public class PRJ2FileDeletedEvent : IIDEEvent
		{ }

		#endregion PRJ2FileDeleted

		/* Script Editor Events */

		#region ScriptEditor_AppendScript

		public class ScriptEditor_AppendScriptEvent : IIDEEvent
		{
			public ScriptGenerationResult Result { get; internal set; }
		}

		/// <summary>
		/// Sends a request to the Script Editor to append new lines of code at the end of the main script file.
		/// </summary>
		public void ScriptEditor_AppendScript(ScriptGenerationResult result)
		{
			Publish(static (messenger, payload) => messenger.Send(new ScriptingAppendScriptRequestedMessage(payload)), result);
			RaiseEvent(new ScriptEditor_AppendScriptEvent { Result = result });
		}

		#endregion ScriptEditor_AppendScript

		#region ScriptEditor_AddNewLevelString

		public class ScriptEditor_AddNewLevelStringEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public void ScriptEditor_AddNewLevelString(string levelName)
		{
			Publish(static (messenger, payload) => messenger.Send(new ScriptingAddLevelStringRequestedMessage(payload ?? string.Empty)), levelName);
			RaiseEvent(new ScriptEditor_AddNewLevelStringEvent { LevelName = levelName });
		}

		#endregion ScriptEditor_AddNewLevelString

		#region ScriptEditor_AddNewPluginEntry

		public class ScriptEditor_AddNewPluginEntryEvent : IIDEEvent
		{
			public string PluginString { get; internal set; }
		}

		public void ScriptEditor_AddNewPluginEntry(string pluginString)
		{
			Publish(static (messenger, payload) => messenger.Send(new ScriptingAddPluginEntryRequestedMessage(payload ?? string.Empty)), pluginString);
			RaiseEvent(new ScriptEditor_AddNewPluginEntryEvent { PluginString = pluginString });
		}

		#endregion ScriptEditor_AddNewPluginEntry

		#region ScriptEditor_AddNewNGString

		public class ScriptEditor_AddNewNGStringEvent : IIDEEvent
		{
			public string NGString { get; internal set; }
		}

		/// <summary>
		/// Sends a request to the Script Editor to add a new ExtraNG string at the end of the main {LANGUAGE}.txt file.
		/// <para>Note: It automatically adds index prefixes, like "0: {STRING}", "1: {STRING}" etc.</para>
		/// </summary>
		public void ScriptEditor_AddNewNGString(string ngString)
		{
			Publish(static (messenger, payload) => messenger.Send(new ScriptingAddNgStringRequestedMessage(payload ?? string.Empty)), ngString);
			RaiseEvent(new ScriptEditor_AddNewNGStringEvent { NGString = ngString });
		}

		#endregion ScriptEditor_AddNewNGString

		#region ScriptEditor_ContentChanged

		public class ScriptEditor_ContentChangedEvent : IIDEEvent
		{ }

		public void ScriptEditor_IndicateExternalChange()
		{
			if (TryPublish(static messenger => messenger.Send(new ScriptingExternalContentChangedMessage())))
				return;

			RaiseEvent(new ScriptEditor_ContentChangedEvent());
		}

		#endregion ScriptEditor_ContentChanged

		#region ScriptEditor_PresenceCheck

		public class ScriptEditor_ScriptPresenceCheckEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool ScriptEditor_IsScriptDefined(string levelName)
		{
			ScriptingIsScriptDefinedRequestMessage? request = TrySendRequest(messenger =>
			{
				var request = new ScriptingIsScriptDefinedRequestMessage(levelName ?? string.Empty);
				messenger.Send(request);
				return request;
			});

			if (request?.HasReceivedResponse == true)
				return request.Response;

			RaiseEvent(new ScriptEditor_ScriptPresenceCheckEvent { LevelName = levelName });
			return ScriptDefined;
		}

		public class ScriptEditor_StringPresenceCheckEvent : IIDEEvent
		{
			public string String { get; internal set; }
		}

		public bool ScriptEditor_IsStringDefined(string @string)
		{
			ScriptingIsStringDefinedRequestMessage? request = TrySendRequest(messenger =>
			{
				var request = new ScriptingIsStringDefinedRequestMessage(@string ?? string.Empty);
				messenger.Send(request);
				return request;
			});

			if (request?.HasReceivedResponse == true)
				return request.Response;

			RaiseEvent(new ScriptEditor_StringPresenceCheckEvent { String = @string });
			return StringDefined;
		}

		public bool ScriptDefined { get; set; }
		public bool StringDefined { get; set; }

		#endregion ScriptEditor_PresenceCheck

		#region ScriptEditor_RenameLevel

		public class ScriptEditor_RenameLevelEvent : IIDEEvent
		{
			public string OldName { get; internal set; }
			public string NewName { get; internal set; }
		}

		public void ScriptEditor_RenameLevel(string targetLevelName, string newName)
		{
			Publish(static (messenger, payload) => messenger.Send(new ScriptingRenameLevelRequestedMessage(payload.OldName ?? string.Empty, payload.NewName ?? string.Empty)), (OldName: targetLevelName, NewName: newName));
			RaiseEvent(new ScriptEditor_RenameLevelEvent { OldName = targetLevelName, NewName = newName });
		}

		#endregion ScriptEditor_RenameLevel

		#region ScriptEditor_ReloadSyntaxHighlighting

		public class ScriptEditor_ReloadSyntaxHighlightingEvent : IIDEEvent
		{ }

		public void ScriptEditor_ReloadSyntaxHighlighting()
		{
			Publish(static messenger => messenger.Send(new ScriptingReloadSyntaxHighlightingRequestedMessage()));
			RaiseEvent(new ScriptEditor_ReloadSyntaxHighlightingEvent());
		}

		#endregion ScriptEditor_ReloadSyntaxHighlighting

		// Construction and destruction
		public IDE(
			IDEConfiguration ideConfiguration,
			List<IGameProject> availableProjects,
			IMessenger? messenger = null,
			IUiDispatcherService? uiDispatcherService = null)
		{
			IDEConfiguration = ideConfiguration;
			AvailableProjects = availableProjects;
			_messenger = messenger;
			_uiDispatcherService = uiDispatcherService;
		}

		public void Dispose()
		{ }

		private static void InvokeAction(Action action)
		{
			ArgumentNullException.ThrowIfNull(action);
			action();
		}

		private void Publish(Action<IMessenger> publishAction)
		{
			ArgumentNullException.ThrowIfNull(publishAction);

			if (_messenger is null)
				return;

			InvokeOnUiThread(() => publishAction(_messenger));
		}

		private void Publish<TPayload>(Action<IMessenger, TPayload> publishAction, TPayload payload)
		{
			ArgumentNullException.ThrowIfNull(publishAction);

			if (_messenger is null)
				return;

			InvokeOnUiThread(() => publishAction(_messenger, payload));
		}

		private bool TryPublish(Action<IMessenger> publishAction)
		{
			if (_messenger is null)
				return false;

			Publish(publishAction);
			return true;
		}

		private TResult? TrySendRequest<TResult>(Func<IMessenger, TResult> sendAction)
			where TResult : class
		{
			ArgumentNullException.ThrowIfNull(sendAction);

			if (_messenger is null)
				return null;

			return InvokeOnUiThread(() => sendAction(_messenger));
		}

		private T InvokeOnUiThread<T>(Func<T> action)
		{
			ArgumentNullException.ThrowIfNull(action);

			if (_uiDispatcherService is null || _uiDispatcherService.CheckAccess())
				return action();

			(bool HasResult, T Result) state = default;
			_uiDispatcherService.Invoke(() => state = (true, action()));

			if (!state.HasResult)
				throw new InvalidOperationException("The UI dispatcher did not return a result.");

			return state.Result;
		}

		private void InvokeOnUiThread(Action action)
		{
			ArgumentNullException.ThrowIfNull(action);

			if (_uiDispatcherService is null || _uiDispatcherService.CheckAccess())
			{
				action();
				return;
			}

			_uiDispatcherService.Invoke(action);
		}

		public static IDE Instance;
	}
}

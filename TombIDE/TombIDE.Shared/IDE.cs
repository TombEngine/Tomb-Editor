using System;
using System.Collections.Generic;
using System.Threading;
using TombIDE.Shared.NewStructure;

namespace TombIDE.Shared
{
	public interface IIDEEvent
	{ }

	public class IDE : IDisposable
	{
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
			ProgramClosingEvent closingEvent = new ProgramClosingEvent();

			RaiseEvent(closingEvent);

			return closingEvent.CanClose;
		}

		public void RequestProgramClose()
			=> RaiseEvent(new RequestProgramCloseEvent());

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

		#region ScriptEditor_AppendScriptLines

		public class ScriptEditor_AppendScriptLinesEvent : IIDEEvent
		{
			public IReadOnlyList<string> Lines { get; internal set; }
		}

		/// <summary>
		/// Sends a request to the Script Editor to append new lines of code at the end of the main script file.
		/// </summary>
		public void ScriptEditor_AppendScriptLines(IReadOnlyList<string> lines) =>
			RaiseEvent(new ScriptEditor_AppendScriptLinesEvent { Lines = lines });

		#endregion ScriptEditor_AppendScriptLines

		#region ScriptEditor_AddNewLevelString

		public class ScriptEditor_AddNewLevelStringEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public void ScriptEditor_AddNewLevelString(string levelName) =>
			RaiseEvent(new ScriptEditor_AddNewLevelStringEvent { LevelName = levelName });

		#endregion ScriptEditor_AddNewLevelString

		#region ScriptEditor_AddNewPluginEntry

		public class ScriptEditor_AddNewPluginEntryEvent : IIDEEvent
		{
			public string PluginString { get; internal set; }
		}

		public void ScriptEditor_AddNewPluginEntry(string pluginString) =>
			RaiseEvent(new ScriptEditor_AddNewPluginEntryEvent { PluginString = pluginString });

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
		public void ScriptEditor_AddNewNGString(string ngString) =>
			RaiseEvent(new ScriptEditor_AddNewNGStringEvent { NGString = ngString });

		#endregion ScriptEditor_AddNewNGString

		#region ScriptEditor_ContentChanged

		public class ScriptEditor_ContentChangedEvent : IIDEEvent
		{ }

		public void ScriptEditor_IndicateExternalChange() =>
			RaiseEvent(new ScriptEditor_ContentChangedEvent());

		#endregion ScriptEditor_ContentChanged

		#region ScriptEditor_PresenceCheck

		public class ScriptEditor_ScriptPresenceCheckEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool ScriptEditor_IsScriptDefined(string levelName)
		{
			RaiseEvent(new ScriptEditor_ScriptPresenceCheckEvent { LevelName = levelName });
			return ScriptDefined;
		}

		public class ScriptEditor_StringPresenceCheckEvent : IIDEEvent
		{
			public string String { get; internal set; }
		}

		public bool ScriptEditor_IsStringDefined(string @string)
		{
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

		public void ScriptEditor_RenameLevel(string targetLevelName, string newName) =>
			RaiseEvent(new ScriptEditor_RenameLevelEvent { OldName = targetLevelName, NewName = newName });

		#endregion ScriptEditor_RenameLevel

		#region ScriptEditor_ReloadSyntaxHighlighting

		public class ScriptEditor_ReloadSyntaxHighlightingEvent : IIDEEvent
		{ }

		#endregion ScriptEditor_ReloadSyntaxHighlighting

		// Construction and destruction
		public IDE(IDEConfiguration ideConfiguration, List<IGameProject> availableProjects)
		{
			IDEConfiguration = ideConfiguration;
			AvailableProjects = availableProjects;
		}

		public void Dispose()
		{ }

		public static IDE Instance;
	}
}

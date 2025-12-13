using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Writers;

namespace TombIDE.ScriptingStudio
{
	public sealed class Tomb1MainStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.Tomb1Main;

		private TRVersion.Game _engine;

		#region Construction

		public Tomb1MainStudio(TRVersion.Game engine) : base(IDE.Instance.Project.GetScriptRootDirectory(), IDE.Instance.Project.GetEngineRootDirectoryPath())
		{
			_engine = engine;

			DockPanelState = IDE.Instance.IDEConfiguration.T1M_DockPanelState;

			FileExplorer.Filter = "*.json5";
			FileExplorer.CommentPrefix = "//";

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Instance.Project.GetScriptRootDirectory(), _engine);

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		#endregion Construction

		#region IDE Events

		protected override void OnIDEEventRaised(IIDEEvent obj)
		{
			base.OnIDEEventRaised(obj);

			IDEEvent_HandleSilentActions(obj);
		}

		private bool IsSilentAction(IIDEEvent obj) => obj
			is IDE.ScriptEditor_ScriptPresenceCheckEvent
			or IDE.ScriptEditor_RenameLevelEvent;

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (IsSilentAction(obj))
			{
				TabPage cachedTab = EditorTabControl.SelectedTab;

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, _engine));
				bool wasScriptFileAlreadyOpened = scriptFileTab != null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened && EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged;

				if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent scrpce)
				{
					IDE.Instance.ScriptDefined = IsLevelScriptDefined(scrpce.LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_RenameLevelEvent rle)
				{
					string oldName = rle.OldName;
					string newName = rle.NewName;

					RenameRequestedLevelScript(oldName, newName);
					EndSilentScriptAction(cachedTab, true, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
			}
			else if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Instance.IDEConfiguration.T1M_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Instance.IDEConfiguration.Save();
			}
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, _engine));

			if (CurrentEditor is TextEditorBase editor)
				ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, _engine));

			if (CurrentEditor is TextEditorBase editor)
				return DocumentParser.IsLevelScriptDefined(editor.Document, levelName);

			return false;
		}

		private void EndSilentScriptAction(TabPage previousTab, bool indicateChange, bool saveAffectedFile, bool closeAffectedTab)
		{
			if (indicateChange)
			{
				CurrentEditor.LastModified = DateTime.Now;
				IDE.Instance.ScriptEditor_IndicateExternalChange();
			}

			if (saveAffectedFile)
				EditorTabControl.SaveFile(EditorTabControl.SelectedTab);

			if (closeAffectedTab)
				EditorTabControl.TabPages.Remove(EditorTabControl.SelectedTab);

			EditorTabControl.EnsureTabFileSynchronization();

			if (previousTab != null)
				EditorTabControl.SelectTab(previousTab);
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.Tomb1MainLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
		}

		#endregion IDE Events

		#region Other methods

		protected override void ApplyUserSettings(IEditorControl editor)
			=> editor.UpdateSettings(Configs.Tomb1Main);

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				ApplyUserSettings(EditorTabControl.GetEditorOfTab(tab));

			UpdateSettings();
		}

		protected override void Build()
		{
			// Nothing.
		}

		protected override void ShowDocumentation() => throw new NotImplementedException();

		#endregion Other methods
	}
}

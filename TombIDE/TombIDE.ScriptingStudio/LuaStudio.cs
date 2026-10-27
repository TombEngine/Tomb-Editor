using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Lua.Services;

namespace TombIDE.ScriptingStudio
{
	public sealed class LuaStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.Lua;

		#region Fields

		private readonly TombEngineLanguageScriptService _languageScriptService = new();

		#endregion Fields

		#region Construction

		public LuaStudio() : base(IDE.Instance.Project.GetScriptRootDirectory(), IDE.Instance.Project.GetEngineRootDirectoryPath())
		{
			DockPanelState = IDE.Instance.IDEConfiguration.Lua_DockPanelState;

			FileExplorer.ExcludedDirectoryFilter = "Scripts\\Engine";
			FileExplorer.Filter = "*.lua";
			FileExplorer.CommentPrefix = "--";

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Instance.Project.GetScriptRootDirectory(), TombLib.LevelData.TRVersion.Game.TombEngine);

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		#endregion Construction

		#region IDE Events

		protected override void OnIDEEventRaised(IIDEEvent obj)
		{
			base.OnIDEEventRaised(obj);

			IDEEvent_HandleSilentActions(obj);

			if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Instance.IDEConfiguration.Lua_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Instance.IDEConfiguration.Save();
			}
		}

		private bool IsSilentAction(IIDEEvent obj)
			=> obj is IDE.ScriptEditor_AppendScriptEvent
			|| obj is IDE.ScriptEditor_ScriptPresenceCheckEvent
			|| obj is IDE.ScriptEditor_StringPresenceCheckEvent
			|| obj is IDE.ScriptEditor_RenameLevelEvent;

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (IsSilentAction(obj))
			{
				TabPage cachedTab = EditorTabControl.SelectedTab;

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
				bool wasScriptFileAlreadyOpened = scriptFileTab is not null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened && EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged;

				TabPage languageFileTab = EditorTabControl.FindTabPage(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
				bool wasLanguageFileAlreadyOpened = languageFileTab is not null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened && EditorTabControl.GetEditorOfTab(languageFileTab).IsContentChanged;

				if (obj is IDE.ScriptEditor_AppendScriptEvent asle && asle.Result.HasOutput)
				{
					AppendScript(asle.Result,
						wasScriptFileAlreadyOpened, wasScriptFileFileChanged,
						wasLanguageFileAlreadyOpened, wasLanguageFileFileChanged);

					EndSilentScriptAction(cachedTab, true, false, false);
				}
				else if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent scrpce)
				{
					IDE.Instance.ScriptDefined = true; // TEMP !!!
				}
				else if (obj is IDE.ScriptEditor_StringPresenceCheckEvent strpce)
				{
					IDE.Instance.StringDefined = IsLevelLanguageStringDefined(strpce.String);
					EndSilentScriptAction(cachedTab, false, false, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_RenameLevelEvent rle)
				{
					string oldName = rle.OldName;
					string newName = rle.NewName;

					RenameRequestedLanguageString(oldName, newName);

					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
			}
		}

		private void AppendScript(ScriptGenerationResult result,
			bool wasScriptFileAlreadyOpened, bool wasScriptFileFileChanged,
			bool wasLanguageFileAlreadyOpened, bool wasLanguageFileFileChanged)
		{
			try
			{
				if (result.GameFlowScript.Length > 0)
					AppendGameFlowScript(result.GameFlowScript, wasScriptFileAlreadyOpened, wasScriptFileFileChanged);

				if (result.LanguageScript.Length > 0)
					AppendLanguageScript(result.LanguageScript, wasLanguageFileAlreadyOpened, wasLanguageFileFileChanged);

				CreateGeneratedFiles(result.FilesToCreate);
			}
			catch
			{
				// Oh well...
			}
		}

		private void AppendGameFlowScript(string scriptText, bool wasScriptFileAlreadyOpened, bool wasScriptFileFileChanged)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
			TabPage affectedTab = EditorTabControl.SelectedTab;

			if (CurrentEditor is not TextEditorBase editor)
				return;

			editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
			editor.ScrollToLine(editor.LineCount);

			if (!wasScriptFileFileChanged && affectedTab is not null)
				EditorTabControl.SaveFile(affectedTab);

			if (!wasScriptFileAlreadyOpened && affectedTab is not null)
				EditorTabControl.TabPages.Remove(affectedTab);
		}

		private void AppendLanguageScript(string languageScript, bool wasLanguageFileAlreadyOpened, bool wasLanguageFileFileChanged)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine), EditorType.Text);
			TabPage affectedTab = EditorTabControl.SelectedTab;

			if (CurrentEditor is TextEditorBase stringsEditor)
			{
				int? insertedLineNumber = _languageScriptService.TryInsertLanguageScript(stringsEditor.Document, languageScript);

				if (insertedLineNumber is not null)
				{
					stringsEditor.ResetSelectionAt(insertedLineNumber.Value);
					stringsEditor.ScrollToLine(insertedLineNumber.Value);

					if (!wasLanguageFileFileChanged && affectedTab is not null)
						EditorTabControl.SaveFile(affectedTab);
				}
			}

			if (!wasLanguageFileAlreadyOpened && affectedTab is not null)
				EditorTabControl.TabPages.Remove(affectedTab);
		}

		private void CreateGeneratedFiles(IReadOnlyList<GeneratedScriptFile> files)
		{
			string scriptRootPath = Path.GetFullPath(ScriptRootDirectoryPath);

			if (!scriptRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
				scriptRootPath += Path.DirectorySeparatorChar;

			foreach (GeneratedScriptFile file in files)
			{
				string filePath = Path.GetFullPath(Path.Combine(scriptRootPath, file.RelativePath));

				if (!filePath.StartsWith(scriptRootPath, StringComparison.OrdinalIgnoreCase))
					continue;

				string directory = Path.GetDirectoryName(filePath);

				if (directory is not null && !Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				File.WriteAllText(filePath, file.Content);
			}
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
			{
				var regex = new Regex($"\"{Regex.Escape(levelName)}\"");
				var stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

				return stringLine is not null;
			}

			return false;
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
			{
				var regex = new Regex($"\"{Regex.Escape(oldName)}\"");
				var stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

				if (stringLine is not null)
				{
					string lineText = editor.Document.GetText(stringLine);
					editor.ReplaceLine(stringLine, regex.Replace(lineText, $"\"{newName}\""));
					editor.ScrollToLine(stringLine.LineNumber);
				}
			}
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.LuaLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
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

			if (previousTab is not null)
				EditorTabControl.SelectTab(previousTab);
		}

		#endregion IDE Events

		#region Other methods

		protected override void ApplyUserSettings(IEditorControl editor)
			=> editor.UpdateSettings(Configs.Lua);

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

		protected override void HandleDocumentCommands(UICommand command)
		{
			switch (command)
			{
				case UICommand.LuaBasics:
					string url = "https://github.com/MontyTRC89/TombEngine/wiki/Basics-of-Lua-Programming";

					var process = new ProcessStartInfo
					{
						FileName = url,
						UseShellExecute = true
					};

					Process.Start(process);
					break;
			}

			base.HandleDocumentCommands(command);
		}

		protected override void ShowDocumentation() => throw new NotImplementedException();

		#endregion Other methods
	}
}

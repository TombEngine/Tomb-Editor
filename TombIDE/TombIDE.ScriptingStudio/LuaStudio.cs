using ICSharpCode.AvalonEdit.Document;
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

namespace TombIDE.ScriptingStudio
{
	public sealed class LuaStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.Lua;

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
				bool wasScriptFileAlreadyOpened = scriptFileTab != null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened && EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged;

				TabPage languageFileTab = EditorTabControl.FindTabPage(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
				bool wasLanguageFileAlreadyOpened = languageFileTab != null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened && EditorTabControl.GetEditorOfTab(languageFileTab).IsContentChanged;

				if (obj is IDE.ScriptEditor_AppendScriptEvent asle && asle.Result.HasContent)
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
				{
					EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));

					if (CurrentEditor is TextEditorBase editor)
					{
						editor.AppendText(Environment.NewLine + result.GameFlowScript + Environment.NewLine);
						editor.ScrollToLine(editor.LineCount);

						if (!wasScriptFileFileChanged)
							EditorTabControl.SaveFile(EditorTabControl.SelectedTab);

						if (!wasScriptFileAlreadyOpened)
							EditorTabControl.TabPages.Remove(EditorTabControl.SelectedTab);
					}
				}

				if (result.LanguageScript.Length > 0)
				{
					EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine), EditorType.Text);

					if (CurrentEditor is TextEditorBase stringsEditor)
					{
						var regex = new Regex(@"TEN\.Flow\.SetStrings\((.*)\)", RegexOptions.IgnoreCase);
						var collectionNameLine = stringsEditor.Document.Lines.FirstOrDefault(line => regex.IsMatch(stringsEditor.Document.GetText(line).Replace(" ", string.Empty)));

						if (collectionNameLine == null)
							return;

						string stringsVariableName = regex.Match(stringsEditor.Document.GetText(collectionNameLine)).Groups[1].Value;

						regex = new Regex(@"local\s+" + Regex.Escape(stringsVariableName) + @"\s*=");
						var stringsStartLine = stringsEditor.Document.Lines.FirstOrDefault(line => regex.IsMatch(stringsEditor.Document.GetText(line)));

						if (stringsStartLine == null)
							return;

						var openingBrackets = new Stack<char>();
						DocumentLine stopLine = null;

						foreach (DocumentLine line in stringsEditor.Document.Lines)
						{
							string lineText = stringsEditor.Document.GetText(line);

							if (!lineText.Contains('{') && !lineText.Contains('}'))
								continue;

							foreach (char opener in lineText.Where(c => c == '{'))
								openingBrackets.Push(opener);

							foreach (char closer in lineText.Where(c => c == '}'))
								openingBrackets.Pop();

							if (openingBrackets.Count == 0)
							{
								stopLine = line;
								break;
							}
						}

						if (stopLine == null || !stringsEditor.Document.GetText(stopLine).Trim().Equals("}"))
							return;

						for (int i = stopLine.LineNumber - 1; i > 0; i--)
						{
							DocumentLine line = stringsEditor.Document.GetLineByNumber(i);
							string lineText = stringsEditor.Document.GetText(line);

							string cleanLine = Regex.Replace(lineText, "--.*$", string.Empty).TrimEnd();

							if (cleanLine.EndsWith("}") || cleanLine.EndsWith("},"))
							{
								stringsEditor.Select(line.EndOffset, 0);

								if (cleanLine.EndsWith("}"))
									stringsEditor.SelectedText += ",";

								stringsEditor.SelectedText += Environment.NewLine;

								stringsEditor.SelectedText += result.LanguageScript;
								stringsEditor.ResetSelectionAt(line.LineNumber + 1);
								stringsEditor.ScrollToLine(line.LineNumber + 1);

								break;
							}
						}

						if (!wasLanguageFileFileChanged)
							EditorTabControl.SaveFile(EditorTabControl.SelectedTab);

						if (!wasLanguageFileAlreadyOpened)
							EditorTabControl.TabPages.Remove(EditorTabControl.SelectedTab);
					}
				}

				// Create any new script files specified by the generation result
				foreach (GeneratedScriptFile file in result.FilesToCreate)
				{
					string filePath = Path.Combine(ScriptRootDirectoryPath, file.RelativePath);
					string directory = Path.GetDirectoryName(filePath);

					if (directory is not null && !Directory.Exists(directory))
						Directory.CreateDirectory(directory);

					File.WriteAllText(filePath, file.Content);
				}
			}
			catch
			{
				// Oh well...
			}
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
			{
				var regex = new Regex($"\"{Regex.Escape(levelName)}\"");
				var stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

				return stringLine != null;
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

				if (stringLine != null)
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

			if (previousTab != null)
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

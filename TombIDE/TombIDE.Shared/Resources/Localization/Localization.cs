using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TombLib.Utils;

namespace TombIDE.Shared.Local
{
	public class Localization
	{
		public string AskRestoreSession { get; set; }
		public string AskFileReload { get; set; }
		public string AskMoveFileToBin { get; set; }
		public string AskMoveFolderToBin { get; set; }
		public string AskUnsavedChanged { get; set; }

		public string AreYouSure { get; set; }
		public string RestoreSessionMBT { get; set; }
		public string FileReloadMBT { get; set; }
		public string UnsavedChangedMBT { get; set; }

		public string Error { get; set; }

		public string File { get; set; }
		public string Edit { get; set; }
		public string Document { get; set; }
		public string Options { get; set; }
		public string View { get; set; }
		public string Help { get; set; }

		public string NewFile { get; set; }
		public string Save { get; set; }
		public string SaveAs { get; set; }
		public string SaveAll { get; set; }
		public string Build { get; set; }

		public string Undo { get; set; }
		public string CantUndo { get; set; }
		public string Redo { get; set; }
		public string CantRedo { get; set; }
		public string Cut { get; set; }
		public string Copy { get; set; }
		public string Paste { get; set; }
		public string FindReplace { get; set; }
		public string SelectAll { get; set; }

		public string Convert { get; set; }
		public string TabsToSpaces { get; set; }
		public string SpacesToTabs { get; set; }

		public string Reindent { get; set; }
		public string TrimWhitespace { get; set; }
		public string ToggleComment { get; set; }
		public string CommentOut { get; set; }
		public string Uncomment { get; set; }
		public string ToggleBookmark { get; set; }
		public string NavigateBack { get; set; }
		public string NavigateForward { get; set; }
		public string GoToDefinition { get; set; }
		public string FindReferences { get; set; }
		public string RenameSymbol { get; set; }
		public string PrevBookmark { get; set; }
		public string NextBookmark { get; set; }
		public string ClearBookmarks { get; set; }

		public string PrevSection { get; set; }
		public string NextSection { get; set; }
		public string ClearString { get; set; }
		public string RemoveLastString { get; set; }

		public string UseNewInclude { get; set; }
		public string ShowLogsAfterBuild { get; set; }
		public string ReindentOnSave { get; set; }
		public string Settings { get; set; }

		public string RestoreDefaultLayout { get; set; }
		public string ContentExplorer { get; set; }
		public string FileExplorer { get; set; }
		public string ReferenceBrowser { get; set; }
		public string CompilerLogs { get; set; }
		public string SearchResults { get; set; }
		public string LuaDiagnostics { get; set; }
		public string LuaReferencesResults { get; set; }
		public string ToolStrip { get; set; }
		public string StatusStrip { get; set; }

		public string ScriptingDocumentation { get; set; }
		public string Tomb3ExtraCommands { get; set; }
		public string LuaBasics { get; set; }
		public string About { get; set; }

		public string TypeFirstAvailableId { get; set; }
		public string NewFileAtCaret { get; set; }

		public string Close { get; set; }
		public string OpenContainingFolder { get; set; }

		public string Untitled { get; set; }

		public string SearchContent { get; set; }
		public string SearchFunctions { get; set; }
		public string SearchFiles { get; set; }
		public string SearchReferences { get; set; }

		public string Sections { get; set; }
		public string Levels { get; set; }
		public string Includes { get; set; }
		public string Defines { get; set; }

		public string Row { get; set; }
		public string Column { get; set; }
		public string Line { get; set; }
		public string LineHeader { get; set; }
		public string ColumnHeader { get; set; }
		public string Selected { get; set; }
		public string Zoom { get; set; }
		public string ResetZoom { get; set; }

		public string Severity { get; set; }
		public string Source { get; set; }
		public string Message { get; set; }
		public string Errors { get; set; }
		public string Warning { get; set; }
		public string Warnings { get; set; }
		public string Information { get; set; }
		public string Hint { get; set; }
		public string Messages { get; set; }
		public string NoDiagnostics { get; set; }
		public string LuaDiagnosticsNoDocument { get; set; }
		public string LuaDiagnosticsUpdating { get; set; }
		public string NoReferencesFound { get; set; }
		public string LuaReferencesNoDocument { get; set; }
		public string LuaReferencesUnsupported { get; set; }
		public string LuaReferencesLoading { get; set; }
		public string LuaRenameNoDocument { get; set; }
		public string LuaRenameUnsupported { get; set; }
		public string LuaRenameNoSymbol { get; set; }
		public string LuaRenamePromptLabel { get; set; }
		public string LuaRenameNoChanges { get; set; }
		public string LuaReformatUnsupported { get; set; }

		public string DecimalValue { get; set; }
		public string HexadecimalValue { get; set; }
		public string Macro { get; set; }
		public string ArgumentRange { get; set; }
		public string Variable { get; set; }
		public string Description { get; set; }
		public string Sounds { get; set; }

		public string MnemonicConstants { get; set; }
		public string EnemyDamageValues { get; set; }
		public string KeyboardScancodes { get; set; }
		public string OCBList { get; set; }
		public string OldCommandsList { get; set; }
		public string NewCommandsList { get; set; }
		public string SoundIndices { get; set; }
		public string MoveableSlotIndices { get; set; }
		public string StaticObjectIndices { get; set; }
		public string VariablePlaceholders { get; set; }

		public string NewFolder { get; set; }
		public string ViewInDefaultEditor { get; set; }
		public string ViewCode { get; set; }
		public string RenameItem { get; set; }
		public string DeleteItem { get; set; }
		public string OpenItemInExplorer { get; set; }

		public string MatchSourceNodeText { get; set; }
		public string SingleMatchNodeText { get; set; }

		public static Localization GetLocalization(Language language)
		{
			string file = Path.Combine(DefaultPaths.LocalizationDirectory, "EN", "TombIDE.xml");

			Localization localization = XmlUtils.ReadXmlFile<Localization>(file);
			NormalizeFieldValues(localization);

			return localization;
		}

		private static void NormalizeFieldValues(Localization localization)
		{
			foreach (FieldInfo field in localization.GetType().GetRuntimeFields())
			{
				string fieldValue = field.GetValue(localization)?.ToString();

				if (fieldValue == null)
					continue;

				string newValue = GetNormalizedValue(fieldValue);

				field.SetValue(localization, newValue);
			}
		}

		private static string GetNormalizedValue(string fieldValue)
		{
			var lines = fieldValue.Split('\n').ToList();

			if (lines.First().Trim() == string.Empty)
				lines.RemoveAt(0);

			if (lines.Count > 0 && lines.Last().Trim() == string.Empty)
				lines.RemoveAt(lines.Count - 1);

			for (int i = 0; i < lines.Count; i++)
				lines[i] = lines[i].Trim();

			return string.Join(Environment.NewLine, lines);
		}
	}

	public enum Language
	{
		English
	}
}

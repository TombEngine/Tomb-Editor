using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster
{
	public partial class FormRenameLevel : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormRenameLevel(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			// Disable renaming external level folders (level folders which are outside of the project's /Levels/ folder)
			if (_ide.SelectedLevel.IsExternal(_ide.Project.LevelsDirectoryPath))
			{
				checkBox_RenameDirectory.Text = "Can't rename external level folders";
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;
			}

			if (_ide.Project.GameVersion == TombLib.LevelData.TRVersion.Game.TombEngine)
			{
				checkBox_RenameScriptEntry.Text = "Rename language entry as well (Recommended)";

				if (!_ide.ScriptEditor_IsStringDefined(_ide.SelectedLevel.Name))
				{
					checkBox_RenameScriptEntry.Checked = false;
					checkBox_RenameScriptEntry.Enabled = false;
					label_LanguageError.Visible = true;
				}
			}
			else if (_ide.Project.GameVersion
				is TombLib.LevelData.TRVersion.Game.TR1
				or TombLib.LevelData.TRVersion.Game.TR2X
				or TombLib.LevelData.TRVersion.Game.TR2
				or TombLib.LevelData.TRVersion.Game.TR3)
			{
				checkBox_RenameScriptEntry.Text = "Rename script entry as well (Recommended)";

				if (!_ide.ScriptEditor_IsScriptDefined(_ide.SelectedLevel.Name))
				{
					checkBox_RenameScriptEntry.Checked = false;
					checkBox_RenameScriptEntry.Enabled = false;
					label_ScriptError.Visible = true;
				}
			}
			else
			{
				// Check if there are errors in the script
				if (!_ide.ScriptEditor_IsScriptDefined(_ide.SelectedLevel.Name) || !_ide.ScriptEditor_IsStringDefined(_ide.SelectedLevel.Name))
				{
					// Disable the checkBox if so
					checkBox_RenameScriptEntry.Checked = false;
					checkBox_RenameScriptEntry.Enabled = false;

					// Display ScriptError + LanguageError
					if (!_ide.ScriptEditor_IsScriptDefined(_ide.SelectedLevel.Name) && !_ide.ScriptEditor_IsStringDefined(_ide.SelectedLevel.Name))
					{
						label_ScriptError.Visible = true;
						label_LanguageError.Visible = true;
					}
					// Display ScriptError only
					else if (!_ide.ScriptEditor_IsScriptDefined(_ide.SelectedLevel.Name) && _ide.ScriptEditor_IsStringDefined(_ide.SelectedLevel.Name))
					{
						Height = 212;
						label_ScriptError.Visible = true;
					}
					// Display LanguageError only
					else if (_ide.ScriptEditor_IsScriptDefined(_ide.SelectedLevel.Name) && !_ide.ScriptEditor_IsStringDefined(_ide.SelectedLevel.Name))
					{
						Height = 212;
						label_LanguageError.Visible = true;
					}
				}
				else // No errors
					Height = 193;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _ide.SelectedLevel.Name;
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());
				newName = LevelHandling.RemoveIllegalNameSymbols(newName);

				bool renameDirectory = checkBox_RenameDirectory.Checked;
				bool renameScriptEntry = checkBox_RenameScriptEntry.Checked;

				if (newName == _ide.SelectedLevel.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_ide.SelectedLevel.DirectoryPath) != newName && renameDirectory)
					{
						string newDirectory = Path.Combine(Path.GetDirectoryName(_ide.SelectedLevel.DirectoryPath), newName);

						if (Directory.Exists(newDirectory))
							throw new ArgumentException("A directory with the same name already exists in the parent directory.");

						_ide.SelectedLevel.Rename(newName, true);
						_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
					}
					else
						DialogResult = DialogResult.Cancel;
				}
				else
				{
					string newDirectory = Path.Combine(Path.GetDirectoryName(_ide.SelectedLevel.DirectoryPath), newName);

					if (renameDirectory && Directory.Exists(newDirectory) && !newDirectory.Equals(_ide.SelectedLevel.DirectoryPath, StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException("A directory with the same name already exists in the parent directory.");

					if (renameScriptEntry)
						_ide.ScriptEditor_RenameLevel(_ide.SelectedLevel.Name, newName);

					_ide.SelectedLevel.Rename(newName, renameDirectory);
					_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void textBox_NewName_TextChanged(object sender, EventArgs e)
		{
			string textBoxContent = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());
			textBoxContent = LevelHandling.RemoveIllegalNameSymbols(textBoxContent);

			// If the name hasn't changed, but the level folder name is different
			if (textBoxContent == _ide.SelectedLevel.Name && Path.GetFileName(_ide.SelectedLevel.DirectoryPath) != textBoxContent)
			{
				// If the level is not an external level
				if (!_ide.SelectedLevel.IsExternal(_ide.Project.LevelsDirectoryPath))
				{
					checkBox_RenameDirectory.Enabled = true;
					checkBox_RenameDirectory.Checked = true;
				}

				checkBox_RenameScriptEntry.Checked = false;
				checkBox_RenameScriptEntry.Enabled = false;
			}
			// If the name changed, but the level folder name is the same
			else if (textBoxContent != _ide.SelectedLevel.Name && Path.GetFileName(_ide.SelectedLevel.DirectoryPath) == textBoxContent)
			{
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;

				// If there are no errors in the script (in this case, if no errors are displayed)
				if (!label_ScriptError.Visible && !label_LanguageError.Visible)
				{
					checkBox_RenameScriptEntry.Enabled = true;
					checkBox_RenameScriptEntry.Checked = true;
				}
			}
			// If the name hasn't changed and the level folder name is the same
			else if (textBoxContent == _ide.SelectedLevel.Name)
			{
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;

				checkBox_RenameScriptEntry.Checked = false;
				checkBox_RenameScriptEntry.Enabled = false;
			}
			else // Basically every other scenario
			{
				// If the level is not an external level
				if (!_ide.SelectedLevel.IsExternal(_ide.Project.LevelsDirectoryPath))
				{
					checkBox_RenameDirectory.Enabled = true;
					checkBox_RenameDirectory.Checked = true;
				}

				// If there are no errors in the script (in this case, if no errors are displayed)
				if (!label_ScriptError.Visible && !label_LanguageError.Visible)
				{
					checkBox_RenameScriptEntry.Enabled = true;
					checkBox_RenameScriptEntry.Checked = true;
				}
			}

			button_Apply.Enabled = !string.IsNullOrWhiteSpace(textBoxContent);
		}

		#endregion Events
	}
}

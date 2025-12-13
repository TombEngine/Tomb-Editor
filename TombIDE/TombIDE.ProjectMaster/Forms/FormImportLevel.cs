using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class FormImportLevel : DarkForm
	{
		public ILevelProject ImportedLevel { get; private set; }
		public List<string> GeneratedScriptLines { get; private set; }

		private IGameProject _targetProject;

		#region Initialization

		public FormImportLevel(IGameProject targetProject, string prj2FilePath)
		{
			_targetProject = targetProject;

			InitializeComponent();

			// Setup some information
			textBox_Prj2Path.BackColor = Color.FromArgb(48, 48, 48); // Mark as uneditable
			textBox_Prj2Path.Text = prj2FilePath;
			textBox_Prj2Path.Tag = prj2FilePath; // Keep the full path in the Tag (because we are visually changing the text later)

			textBox_LevelName.Text = Path.GetFileNameWithoutExtension(prj2FilePath);

			if (targetProject.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TombEngine)
			{
				checkBox_GenerateSection.Checked = checkBox_GenerateSection.Visible = false;
				panel_ScriptSettings.Visible = false;
				panel_04.Visible = false;
			}
			else if (targetProject.GameVersion is not TRVersion.Game.TR4 and not TRVersion.Game.TRNG)
			{
				checkBox_EnableHorizon.Visible = false;
				panel_ScriptSettings.Height -= 30;
			}

			if (_targetProject.GameVersion == TRVersion.Game.TR2)
				numeric_SoundID.Value = 33;
			else if (_targetProject.GameVersion == TRVersion.Game.TR3)
				numeric_SoundID.Value = 28;
		}

		#endregion Initialization

		#region Level importing methods

		private void button_Import_Click(object sender, EventArgs e)
		{
			button_Import.Enabled = false;

			try
			{
				string levelName = PathHelper.RemoveIllegalPathSymbols(textBox_LevelName.Text.Trim());
				levelName = LevelHandling.RemoveIllegalNameSymbols(levelName);

				if (string.IsNullOrWhiteSpace(levelName))
					throw new ArgumentException("You must enter a valid name for the level.");

				if (radioButton_SelectedCopy.Checked && treeView.SelectedNodes.Count == 0)
					throw new ArgumentException("You must select which .prj2 files you want to import.");

				string dataFileName = textBox_CustomFileName.Text.Trim();

				if (string.IsNullOrWhiteSpace(dataFileName))
					throw new ArgumentException("You must specify the custom DATA file name.");

				if (radioButton_SpecifiedCopy.Checked || radioButton_SelectedCopy.Checked)
					ImportAndCopyFiles(levelName, dataFileName);
				else if (radioButton_FolderKeep.Checked)
					ImportButKeepFiles(levelName, dataFileName);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Import.Enabled = true;

				DialogResult = DialogResult.None;
			}
		}

		private void ImportAndCopyFiles(string levelName, string dataFileName)
		{
			string fullSpecifiedPrj2FilePath = textBox_Prj2Path.Tag.ToString();

			string levelFolderPath = Path.Combine(_targetProject.LevelsDirectoryPath, levelName); // A path inside the project's /Levels/ folder
			string specificFileName = Path.GetFileName(fullSpecifiedPrj2FilePath); // The user-specified file name

			// Create the level folder
			if (!Directory.Exists(levelFolderPath))
				Directory.CreateDirectory(levelFolderPath);

			if (Directory.EnumerateFileSystemEntries(levelFolderPath).ToArray().Length > 0) // 99% this will never accidentally happen
				throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
					"the project's /Levels/ folder and it's not empty.");

			if (radioButton_SpecifiedCopy.Checked)
			{
				// Only copy the specified file into the created level folder
				string destPath = Path.Combine(levelFolderPath, specificFileName);
				File.Copy(fullSpecifiedPrj2FilePath, destPath);
			}
			else if (radioButton_SelectedCopy.Checked)
			{
				// Check if the user-specified file was selected on the list, if not, then set the TargetPrj2FileName property to null
				bool specificFileSelected = false;

				// Copy all selected files into the created levelFolderPath
				foreach (DarkTreeNode node in treeView.SelectedNodes)
				{
					if (node.Text == specificFileName)
						specificFileSelected = true;

					string nodePrj2Path = node.Tag.ToString(); // node.Tag is the full source path of the currently processed file
					string destPath = Path.Combine(levelFolderPath, Path.GetFileName(nodePrj2Path));
					File.Copy(nodePrj2Path, destPath);
				}

				if (!specificFileSelected) // If the user-specified file was not selected on the list
					specificFileName = null;
			}

			CreateAndAddLevelToProject(levelName, levelFolderPath, dataFileName, specificFileName);
		}

		private void ImportButKeepFiles(string levelName, string dataFileName)
		{
			string fullSpecifiedPrj2FilePath = textBox_Prj2Path.Tag.ToString();

			string levelFolderPath = Path.GetDirectoryName(fullSpecifiedPrj2FilePath);
			string specificFile = Path.GetFileName(fullSpecifiedPrj2FilePath);

			CreateAndAddLevelToProject(levelName, levelFolderPath, dataFileName, specificFile);
		}

		private void CreateAndAddLevelToProject(string levelName, string levelFolderPath, string dataFileName, string specificFileName)
		{
			// Create the LevelProject instance
			var importedLevel = new LevelProject(levelName, levelFolderPath, specificFileName);

			UpdateLevelSettings(importedLevel, dataFileName);

			if (checkBox_GenerateSection.Checked)
			{
				int ambientSoundID = (int)numeric_SoundID.Value;
				bool horizon = checkBox_EnableHorizon.Checked;

				// // // //
				GeneratedScriptLines = LevelHandling.GenerateScriptLines(levelName, dataFileName, _targetProject.GameVersion, ambientSoundID, horizon);
				// // // //
			}

			importedLevel.Save();

			// // // //
			ImportedLevel = importedLevel;
			// // // //
		}

		private void UpdateLevelSettings(ILevelProject importedLevel, string dataFileName)
		{
			if (radioButton_SpecifiedCopy.Checked)
			{
				string specifiedFileName = Path.GetFileName(textBox_Prj2Path.Tag.ToString());
				string internalFilePath = Path.Combine(importedLevel.DirectoryPath, specifiedFileName);

				LevelHandling.UpdatePrj2GameSettings(internalFilePath, _targetProject, dataFileName);
			}
			else if (radioButton_SelectedCopy.Checked)
			{
				UpdateAllPrj2FilesInLevelDirectory(importedLevel, dataFileName);
			}
			else if (radioButton_FolderKeep.Checked)
			{
				DialogResult result = DarkMessageBox.Show(this, "Do you want to update the \"Game\" settings of all the .prj2 files in the\n" +
					"specified folder to match the project settings?", "Update settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					UpdateAllPrj2FilesInLevelDirectory(importedLevel, dataFileName);
			}
		}

		private void UpdateAllPrj2FilesInLevelDirectory(ILevelProject importedLevel, string dataFileName)
		{
			string[] files = Directory.GetFiles(importedLevel.DirectoryPath, "*.prj2", SearchOption.TopDirectoryOnly);

			progressBar.Visible = true;
			progressBar.BringToFront();
			progressBar.Maximum = files.Length;

			foreach (string file in files)
			{
				if (!Prj2Helper.IsBackupFile(file))
					LevelHandling.UpdatePrj2GameSettings(file, _targetProject, dataFileName);

				progressBar.Increment(1);
			}
		}

		#endregion Level importing methods

		#region Other level importing events / methods

		private void textBox_LevelName_TextChanged(object sender, EventArgs e)
		{
			if (!checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
		}

		private void checkBox_CustomFileName_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Enabled = true;
			else
			{
				textBox_CustomFileName.Enabled = false;
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
			}
		}

		private void textBox_CustomFileName_TextChanged(object sender, EventArgs e)
		{
			int cachedCaretPosition = textBox_CustomFileName.SelectionStart;

			textBox_CustomFileName.Text = textBox_CustomFileName.Text.Replace(' ', '_');
			textBox_CustomFileName.SelectionStart = cachedCaretPosition;
		}

		private void radioButton_SpecifiedCopy_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_SpecifiedCopy.Checked)
				return;

			textBox_Prj2Path.Text = textBox_Prj2Path.Tag.ToString(); // Switch back to the full path
			textBox_Prj2Path.BackColor = Color.FromArgb(48, 48, 48); // Reset the BackColor
			ClearAndDisableTreeView();
		}

		private void radioButton_SelectedCopy_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_SelectedCopy.Checked)
				return;

			textBox_Prj2Path.Text = Path.GetDirectoryName(textBox_Prj2Path.Tag.ToString()); // Switch to just the folder path
			textBox_Prj2Path.BackColor = Color.FromArgb(64, 80, 96); // Change the BackColor to indicate the change
			EnableAndFillTreeView();
		}

		private void radioButton_SpecificKeep_CheckedChanged(object sender, EventArgs e)
		{
			if (!radioButton_FolderKeep.Checked)
				return;

			textBox_Prj2Path.Text = Path.GetDirectoryName(textBox_Prj2Path.Tag.ToString()); // Switch to just the folder path
			textBox_Prj2Path.BackColor = Color.FromArgb(64, 80, 96); // Change the BackColor to indicate the change
			ClearAndDisableTreeView();
		}

		private void button_SelectAll_Click(object sender, EventArgs e)
		{
			treeView.SelectNodes(treeView.Nodes);
			treeView.Focus();
		}

		private void button_DeselectAll_Click(object sender, EventArgs e)
		{
			treeView.SelectedNodes.Clear();
			treeView.Invalidate();
		}

		private void ClearAndDisableTreeView()
		{
			treeView.SelectedNodes.Clear();
			treeView.Nodes.Clear();
			treeView.Invalidate();

			treeView.Enabled = false;
			button_SelectAll.Enabled = false;
			button_DeselectAll.Enabled = false;
		}

		private void EnableAndFillTreeView()
		{
			treeView.Enabled = true;
			button_SelectAll.Enabled = true;
			button_DeselectAll.Enabled = true;

			treeView.Nodes.Clear();

			foreach (string file in Directory.GetFiles(textBox_Prj2Path.Text, "*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (Prj2Helper.IsBackupFile(file))
					continue;

				var node = new DarkTreeNode
				{
					Text = Path.GetFileName(file),
					Tag = file,
				};

				treeView.Nodes.Add(node);
			}
		}

		#endregion Other level importing events / methods

		#region Script section generating

		private void checkBox_GenerateSection_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_GenerateSection.Checked)
			{
				panel_ScriptSettings.Visible = true;
				panel_04.Height = 108;
				Height = 626;
			}
			else
			{
				panel_ScriptSettings.Visible = false;
				panel_04.Height = 35;
				Height = 553;
			}
		}

		private void button_OpenAudioFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(Path.Combine(_targetProject.GetEngineRootDirectoryPath(), "audio"));

		#endregion Script section generating
	}
}

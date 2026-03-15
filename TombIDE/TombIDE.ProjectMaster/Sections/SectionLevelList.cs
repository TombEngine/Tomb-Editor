using DarkUI.Controls;
using DarkUI.Forms;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.LevelCompile;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;
using TombIDE.Shared.SharedForms;

namespace TombIDE.ProjectMaster
{
	public partial class SectionLevelList : UserControl
	{
		private IDE _ide = null!;
		private ILevelCompileService _levelCompileService = null!;

		#region Initialization

		public SectionLevelList()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide, ILevelCompileService levelCompileService)
		{
			_ide = ide;
			_levelCompileService = levelCompileService;
			_ide.IDEEventRaised += OnIDEEventRaised;

			FillLevelList(); // With levels taken from the .trproj file (current _ide.Project)
		}

		private void FillLevelList()
		{
			treeView.Nodes.Clear();

			// Add nodes into the treeView for each level
			foreach (ILevelProject level in _ide.Project.GetAllValidLevelProjects())
			{
				if (level.IsValid(out _))
					AddLevelToList(level);
			}

			// Update the .trproj file with only valid levels
			ReserializeTRPROJ();
		}

		private void AddLevelToList(ILevelProject level, bool reserialize = false)
		{
			var node = new DarkTreeNodeEx(level.Name, "~/" + level.GetMostRecentlyModifiedPrj2FileName()) { Tag = level };

			// Mark external levels
			if (level.IsExternal(_ide.Project.LevelsDirectoryPath))
				node.Text = _ide.IDEConfiguration.ExternalLevelPrefix + node.Text;

			// Add the node to the list
			treeView.Nodes.Add(node);

			if (reserialize)
				ReserializeTRPROJ();
		}

		#endregion Initialization

		#region Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedLevelSettingsChangedEvent)
			{
				// Update the text of the node (if the level name was changed ofc)
				if (_ide.SelectedLevel.TargetPrj2FileName is null)
					((DarkTreeNodeEx)treeView.SelectedNodes[0]).SubText = "~/" + _ide.SelectedLevel.GetMostRecentlyModifiedPrj2FileName();
				else
					((DarkTreeNodeEx)treeView.SelectedNodes[0]).SubText = "~/" + _ide.SelectedLevel.TargetPrj2FileName;

				treeView.SelectedNodes[0].Text = _ide.SelectedLevel.Name;

				// Mark external levels
				if (_ide.SelectedLevel.IsExternal(_ide.Project.LevelsDirectoryPath))
					treeView.SelectedNodes[0].Text = _ide.IDEConfiguration.ExternalLevelPrefix + treeView.SelectedNodes[0].Text;

				// Update the node's Tag with the updated level
				treeView.SelectedNodes[0].Tag = _ide.SelectedLevel;

				// Send the new list (with the modified node) into the .trproj file
				ReserializeTRPROJ();
			}
			else if (obj is IDE.PRJ2FileDeletedEvent)
			{
				// Clear the selection immediately, otherwise many exceptions will happen!
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				RefreshLevelList();
			}
		}

		private void label_Hint_Click(object sender, EventArgs e) => ShowLevelSetupForm();

		private void button_New_Click(object sender, EventArgs e) => ShowLevelSetupForm();
		private void button_Import_Click(object sender, EventArgs e) => ImportLevel();
		private void button_Rename_Click(object sender, EventArgs e) => ShowRenameLevelForm();
		private void button_Delete_Click(object sender, EventArgs e) => DeleteLevel();

		private void button_MoveUp_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeUp();
			ReserializeTRPROJ();
		}

		private void button_MoveDown_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeDown();
			ReserializeTRPROJ();
		}

		private void button_OpenInExplorer_Click(object sender, EventArgs e)
			=> SharedMethods.OpenInExplorer(((ILevelProject)treeView.SelectedNodes[0].Tag).DirectoryPath);

		private void button_Refresh_Click(object sender, EventArgs e) => RefreshLevelList();

		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => CheckItemSelection();

		private void treeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.Button == MouseButtons.Right)
				contextMenu.Show(Cursor.Position);

			CheckItemSelection();
		}

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OpenSelectedLevel();
		}

		private void treeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.KeyCode == Keys.F2)
				ShowRenameLevelForm();

			if (e.KeyCode == Keys.Delete)
				DeleteLevel();
		}

		private void treeView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode is Keys.Up or Keys.Down)
				CheckItemSelection();
		}

		private void menuItem_OpenLevel_Click(object sender, EventArgs e) => OpenSelectedLevel();

		private void ShowLevelSetupForm()
		{
			using var form = new FormLevelSetup(_ide.Project);

			if (form.ShowDialog(this) == DialogResult.OK && form.CreatedLevel is not null)
				OnLevelAdded(form.CreatedLevel, form.GeneratedScriptLines);
		}

		private void ImportLevel()
		{
			using var dialog = new OpenFileDialog();
			dialog.Title = "Choose the .prj2 file you want to import";
			dialog.Filter = "Tomb Editor Levels|*.prj2";

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					if (dialog.FileName.StartsWith(_ide.Project.LevelsDirectoryPath, StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException("You cannot import levels which are already inside the project's /Levels/ folder.");

					if (Prj2Helper.IsBackupFile(dialog.FileName))
						throw new ArgumentException("You cannot import backup files.");

					using var form = new FormImportLevel(_ide.Project, dialog.FileName);

					if (form.ShowDialog(this) == DialogResult.OK)
						OnLevelAdded(form.ImportedLevel, form.GeneratedScriptLines);
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void ShowRenameLevelForm()
		{
			if (!IsValidLevel(_ide.SelectedLevel))
				return;

			using var form = new FormRenameLevel(_ide);
			form.ShowDialog(this); // After the form is done, it will trigger IDE.SelectedLevelSettingsChangedEvent
		}

		private void DeleteLevel()
		{
			var affectedLevel = (ILevelProject)treeView.SelectedNodes[0].Tag;

			if (!IsValidLevel(affectedLevel))
				return;

			// We can't allow deleting directories of external levels, so we must handle them differently

			// Internal level paths always start with the project's LevelsPath
			bool isInternalLevel = affectedLevel.DirectoryPath.StartsWith(_ide.Project.LevelsDirectoryPath, StringComparison.OrdinalIgnoreCase);
			string message;

			if (isInternalLevel)
			{
				message = "Are you sure you want to delete the \"" + affectedLevel.Name + "\" level?\n" +
					"This will send the level folder with all its files into the recycle bin.";
			}
			else
			{
				message = "Are you sure you want to delete the \"" + affectedLevel.Name + "\" level from the list?\n" +
					"Since this is an external level, this process will NOT affect the level folder nor its files.";
			}

			DialogResult result = DarkMessageBox.Show(this, message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (isInternalLevel) // Move the level folder into the recycle bin in this case
					FileSystem.DeleteDirectory(affectedLevel.DirectoryPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

				// Remove the node and clear selection
				treeView.SelectedNodes[0].Remove();
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				// Send the new list (without the removed node) into the .trproj file
				ReserializeTRPROJ();
			}
		}

		private void RefreshLevelList()
		{
			treeView.SelectedNodes.Clear();
			CheckItemSelection();

			string cachedNodeText = string.Empty;

			if (treeView.SelectedNodes.Count > 0)
			{
				cachedNodeText = treeView.SelectedNodes[0].Text;

				// Reinitialize the selected level (That's for treeView_Resources in SectionLevelProperties)
				_ide.SelectedLevel = null;
				_ide.SelectedLevel = (ILevelProject)treeView.SelectedNodes[0].Tag;
			}

			// Scan the project's /Levels/ folder using FormLoading
			using var form = new FormLoading(_ide);

			if (form.ShowDialog(this) == DialogResult.OK)
			{
				// Refresh the level list
				FillLevelList();

				// If a node was selected, reselect it after refreshing
				if (!string.IsNullOrEmpty(cachedNodeText))
				{
					foreach (DarkTreeNode node in treeView.Nodes)
					{
						if (node.Text.Equals(cachedNodeText, StringComparison.OrdinalIgnoreCase))
						{
							treeView.SelectNode(node);
							treeView.ScrollTo(node.FullArea.Location);

							break;
						}
					}
				}

				CheckItemSelection();
			}
		}

		private void CheckItemSelection()
		{
			// Enable / Disable node specific buttons
			button_OpenInTE.Enabled = treeView.SelectedNodes.Count > 0;
			button_Rebuild.Enabled = treeView.SelectedNodes.Count > 0;
			button_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			button_Delete.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			button_OpenInExplorer.Enabled = treeView.SelectedNodes.Count > 0;

			menuItem_OpenLevel.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_Rebuild.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_Delete.Enabled = treeView.SelectedNodes.Count > 0;

			// Set the SelectedLevel variable if a node is selected
			// Triggers IDE.SelectedLevelChangedEvent
			if (treeView.SelectedNodes.Count > 0)
				_ide.SelectedLevel = (ILevelProject)treeView.SelectedNodes[0].Tag;
			else
				_ide.SelectedLevel = null;
		}

		private void OpenSelectedLevel()
		{
			if (!IsValidLevel(_ide.SelectedLevel))
				return;

			string prj2Path;

			if (_ide.SelectedLevel.TargetPrj2FileName is null)
				prj2Path = Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.GetMostRecentlyModifiedPrj2FileName());
			else
				prj2Path = Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.TargetPrj2FileName);

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
				Arguments = "\"" + prj2Path + "\"",
				WorkingDirectory = DefaultPaths.ProgramDirectory,
				UseShellExecute = true
			};

			Process.Start(startInfo);
		}

		#endregion Events

		#region Methods

		private void OnLevelAdded(ILevelProject addedLevel, List<string> scriptLines)
		{
			AddLevelToList(addedLevel, true);

			// Select the new level node
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				if (((ILevelProject)node.Tag).Name.Equals(addedLevel.Name, StringComparison.OrdinalIgnoreCase))
				{
					treeView.SelectNode(node);
					CheckItemSelection();

					break;
				}
			}

			if (scriptLines != null && scriptLines.Count > 0)
			{
				_ide.ScriptEditor_AppendScriptLines(scriptLines);
				_ide.ScriptEditor_AddNewLevelString(addedLevel.Name);
			}
		}

		public void ReserializeTRPROJ()
		{
			treeView.Invalidate();

			_ide.Project.KnownLevelProjectFilePaths.Clear();

			foreach (DarkTreeNode node in treeView.Nodes)
			{
				var level = (ILevelProject)node.Tag;
				_ide.Project.KnownLevelProjectFilePaths.Add(level.GetTrlvlFilePath());
				level.Save();
			}

			_ide.Project.Save();

			label_Hint.Visible = treeView.Nodes.Count == 0;
		}

		#endregion Methods

		private void button_Rebuild_Click(object sender, EventArgs e)
		{
			if (!IsValidLevel(_ide.SelectedLevel))
				return;

			_levelCompileService.RebuildLevel(_ide.SelectedLevel);
		}

		private bool IsValidLevel(ILevelProject level)
		{
			bool isValid;
			string errorMessage;

			if (level is null)
			{
				isValid = false;
				errorMessage = "The selected level is null.";
			}
			else
				isValid = level.IsValid(out errorMessage);

			if (!isValid)
			{
				string message =
					"The selected level is invalid.\n" +
					$"Error message: {errorMessage}\n" +
					"Would you like to refresh the level list?";

				DialogResult result = DarkMessageBox.Show(this, message, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

				if (result is DialogResult.Yes)
					RefreshLevelList();

				return false;
			}

			return true;
		}
	}
}

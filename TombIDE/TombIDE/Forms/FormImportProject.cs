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
using TombLib.Utils;

namespace TombIDE
{
	public partial class FormImportProject : DarkForm
	{
		public IGameProject ImportedProject { get; private set; }
		public GameProjectDTO ProjectDTO { get; private set; }

		#region Initialization

		public FormImportProject(GameProjectDTO projectDTO)
		{
			InitializeComponent();

			ProjectDTO = projectDTO;

			textBox_ExePath.Text = projectDTO.EngineExecutableFilePath;
			textBox_LauncherPath.Text = projectDTO.LauncherExecutableFilePath;

			textBox_ProjectName.Text = projectDTO.ProjectName;

			textBox_ScriptPath.Text = projectDTO.ScriptDirectoryPath;
			textBox_LevelsPath.Text = projectDTO.LevelsDirectoryPath;

			if (string.IsNullOrEmpty(textBox_LevelsPath.Text)) // If still no /Levels/ or /Maps/ folder was found
			{
				// Suggest a path
				textBox_LevelsPath.Text = Path.Combine(Path.GetDirectoryName(projectDTO.LauncherExecutableFilePath), "Levels");

				// Highlight the textBox and add a toolTip for it to indicate that the pre-set path is just a suggestion
				textBox_LevelsPath.BackColor = Color.FromArgb(48, 96, 64);
				toolTip.SetToolTip(textBox_LevelsPath, "Suggested path");
			}

			button_Import.Text = "Import " + projectDTO.GameVersion + " Project";

			if (projectDTO.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TombEngine) // Hardcoded script paths
			{
				textBox_ScriptPath.ReadOnly = true;
				button_BrowseScript.Enabled = false;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			textBox_ProjectName.Focus();
		}

		#endregion Initialization

		#region Events

		private void textBox_LevelsPath_TextChanged(object sender, EventArgs e)
		{
			// Reset the suggestion indication
			if (textBox_LevelsPath.BackColor == Color.FromArgb(48, 96, 64))
			{
				textBox_LevelsPath.BackColor = Color.FromArgb(69, 73, 74); // Default DarkTextBox color
				toolTip.RemoveAll();
			}
		}

		private void button_BrowseScript_Click(object sender, EventArgs e)
		{
			using var dialog = new BrowseFolderDialog();
			dialog.Title = "Select an existing script directory for the project...";

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_ScriptPath.Text = dialog.Folder;
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			using var dialog = new BrowseFolderDialog();
			dialog.Title = "Choose a /Levels/ directory for the project...";

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_LevelsPath.Text = dialog.Folder;
		}

		private void button_Import_Click(object sender, EventArgs e)
		{
			button_Import.Enabled = false;

			try
			{
				string projectName = PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());
				string projectDirectory = Path.GetDirectoryName(ProjectDTO.LauncherExecutableFilePath);

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for the project.");

				if (projectName.Equals("Engine", StringComparison.OrdinalIgnoreCase)) // Safety
					throw new ArgumentException("Illegal project name.");

				if (string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the Script directory path of the project.");

				if (string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the Levels directory path for the project.");

				string scriptDirectoryPath = textBox_ScriptPath.Text.Trim();
				string levelsDirectoryPath = textBox_LevelsPath.Text.Trim();

				// Check if the levelsPath directory exists, if so, check if it contains any valid .prj2 files
				if (Directory.Exists(levelsDirectoryPath))
				{
					// Check if the directory contains non-backup .prj2 files
					List<string> prj2Files = LevelHandling.GetValidPrj2FilesFromDirectory(levelsDirectoryPath);

					if (prj2Files.Count > 0)
					{
						DialogResult result = DarkMessageBox.Show(this,
							"TombIDE will change the \"Game\" settings of all the .prj2 files\n" +
							"in the specified /Levels/ folder to match the imported project settings.\n" +
							"Would you like to create a backup of the folder first?", "Create backup?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
							CreateLevelsBackup(levelsDirectoryPath);
					}
				}
				else
					Directory.CreateDirectory(levelsDirectoryPath);

				IGameProject importedProject = ProjectDTO.GameVersion switch
				{
					TRVersion.Game.TR1 => new TR1XGameProject(projectName, projectDirectory, levelsDirectoryPath),
					TRVersion.Game.TR2X => new TR2XGameProject(projectName, projectDirectory, levelsDirectoryPath),
					TRVersion.Game.TR2 => new TR2GameProject(projectName, projectDirectory, levelsDirectoryPath, scriptDirectoryPath),
					TRVersion.Game.TR3 => new TR3GameProject(projectName, projectDirectory, levelsDirectoryPath, scriptDirectoryPath),
					TRVersion.Game.TR4 => new TR4GameProject(projectName, projectDirectory, levelsDirectoryPath, scriptDirectoryPath),
					TRVersion.Game.TRNG => new TRNGGameProject(projectName, projectDirectory, levelsDirectoryPath, scriptDirectoryPath, ProjectDTO.PluginsDirectoryPath),
					TRVersion.Game.TombEngine => new TENGameProject(projectName, projectDirectory, levelsDirectoryPath),
					_ => throw new NotImplementedException("Detected game version doesn't match a supported version.")
				};

				importedProject.Save();
				ImportedProject = importedProject;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Import.Enabled = true;
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		#region Methods

		private void CreateLevelsBackup(string levelsPath)
		{
			try
			{
				// Check if there are any existing backup folders of the /Levels/ folder in the parent directory of the levelsPath
				string[] existingBackupFolders = Directory.GetDirectories(
					Path.GetDirectoryName(levelsPath), Path.GetFileName(levelsPath) + "_BACKUP*", SearchOption.TopDirectoryOnly);

				string newBackupFolderPath;

				// Generate a name for the new backup folder

				if (existingBackupFolders.Length == 0)
					newBackupFolderPath = levelsPath + "_BACKUP";
				else if (existingBackupFolders.Length == 1)
					newBackupFolderPath = levelsPath + "_BACKUP_2";
				else
				{
					var existingNumbers = new List<int>();

					foreach (string existingBackupFolder in existingBackupFolders)
					{
						if (int.TryParse(Path.GetFileNameWithoutExtension(existingBackupFolder).Split('_')[2], out int result))
							existingNumbers.Add(result);
					}

					int nextFolderNumber = existingNumbers.Max() + 1;

					newBackupFolderPath = levelsPath + "_BACKUP_" + nextFolderNumber;
				}

				Directory.CreateDirectory(newBackupFolderPath);

				// Create all of the subdirectories
				foreach (string directory in Directory.GetDirectories(levelsPath, "*", SearchOption.AllDirectories))
					Directory.CreateDirectory(directory.Replace(levelsPath, newBackupFolderPath));

				// Copy all the .prj2 files
				foreach (string file in Directory.GetFiles(levelsPath, "*.prj2", SearchOption.AllDirectories))
				{
					if (!Prj2Helper.IsBackupFile(file))
						File.Copy(file, file.Replace(levelsPath, newBackupFolderPath));
				}

				DarkMessageBox.Show(this, "Backup successfully created in:\n" + newBackupFolderPath, "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "Failed to create backup.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion Methods
	}
}

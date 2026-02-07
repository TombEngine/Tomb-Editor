using DarkUI.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;

namespace TombIDE.Shared.SharedForms
{
	public partial class FormLoading : DarkForm
	{
		private IDE _ide;

		public FormLoading(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		private const int CP_NOCLOSE_BUTTON = 0x200;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CP_NOCLOSE_BUTTON;

				return cp;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// We must delay the ScanLevelsDirectory() method to prevent graphical glitches
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			ScanLevelsDirectory();
		}

		private void ScanLevelsDirectory()
		{
			var directoryInfo = new DirectoryInfo(_ide.Project.LevelsDirectoryPath);

			// Get all subdirectories of the project's /Levels/ folder
			DirectoryInfo[] levelDirectories = directoryInfo.GetDirectories();

			// Setup the progress bar
			progressBar.Maximum = levelDirectories.Length;

			foreach (DirectoryInfo directory in levelDirectories)
			{
				// Check if the folder we're currently scanning is already known for the project
				if (IsFolderKnownForProject(directory))
				{
					progressBar.Increment(1);
					continue;
				}

				// Scan all .prj2 files in the current directory (if there are any)
				if (directory.GetFiles("*.prj2", SearchOption.TopDirectoryOnly).Length > 0)
				{
					// Check if the folder isn't just made up of backup files
					if (FolderOnlyContainsBackupFiles(directory))
					{
						progressBar.Increment(1);
						continue;
					}

					// Create a new ProjectLevel instance from the folder
					var detectedLevel = new LevelProject(directory.Name, directory.FullName);

					// Update the settings of all the .prj2 files in the folder to match the project settings
					UpdateAllPrj2GameSettings(detectedLevel);

					detectedLevel.Save();
				}

				progressBar.Increment(1);
			}

			CheckForFloatingPrj2Files(directoryInfo);

			DialogResult = DialogResult.OK;
		}

		private void CheckForFloatingPrj2Files(DirectoryInfo directoryInfo)
		{
			if (directoryInfo.GetFiles("*.prj2", SearchOption.TopDirectoryOnly).Length > 0)
			{
				DarkMessageBox.Show(this, "Some .prj2 files were found directly placed in the project's /Levels/ folder.\n" +
					"Please make sure to create separate sub-folders for them as having floating .prj2 files is not allowed.",
					"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private bool IsFolderKnownForProject(DirectoryInfo directory)
			=> _ide.Project.KnownLevelProjectFilePaths.Any(level => Path.GetDirectoryName(level).Equals(directory.FullName, StringComparison.OrdinalIgnoreCase));

		private bool FolderOnlyContainsBackupFiles(DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles("*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!Prj2Helper.IsBackupFile(file.FullName))
					return false; // We got a non-backup file
			}

			return true;
		}

		private void UpdateAllPrj2GameSettings(ILevelProject levelProject)
		{
			foreach (string filePath in Directory.GetFiles(levelProject.DirectoryPath, "*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!Prj2Helper.IsBackupFile(filePath))
					Prj2Helper.UpdateGameSettings(filePath, _ide.Project);
			}
		}
	}
}

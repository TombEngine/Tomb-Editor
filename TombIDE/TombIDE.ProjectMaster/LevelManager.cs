using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Forms;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class LevelManager : UserControl
	{
		private IDE _ide;

		public LevelManager()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += IDE_IDEEventRaised;

			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine: panel_GameLabel.BackgroundImage = Properties.Resources.TEN_LVL; break;
				case TRVersion.Game.TRNG: panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL; break;
				case TRVersion.Game.TR4: panel_GameLabel.BackgroundImage = Properties.Resources.TR4_LVL; break;
				case TRVersion.Game.TR3: panel_GameLabel.BackgroundImage = Properties.Resources.TR3_LVL; break;
				case TRVersion.Game.TR2 or TRVersion.Game.TR2X: panel_GameLabel.BackgroundImage = Properties.Resources.TR2_LVL; break;
				case TRVersion.Game.TR1 or TRVersion.Game.TR1X: panel_GameLabel.BackgroundImage = Properties.Resources.TR1_LVL; break;
			}

			UpdateVersionLabel();

			section_LevelList.Initialize(ide);
			section_LevelProperties.Initialize(ide);
		}

		private void IDE_IDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.BeginEngineUpdateEvent)
				BeginEngineUpdate();
		}

		private void UpdateVersionLabel()
		{
			button_Update.Visible = false;

			if (_ide.Project.GameVersion is TRVersion.Game.TR4)
			{
				label_OutdatedState.Visible = false;
				label_EngineVersion.Text = "Engine Version: TRLE";
				return;
			}

			Version engineVersion = _ide.Project.GetCurrentEngineVersion();
			string engineVersionString = engineVersion == new Version(0, 0) ? "Unknown" : engineVersion.ToString();
			label_EngineVersion.Text = $"Engine Version: {engineVersionString}";

			Version latestVersion = _ide.Project.GetLatestEngineVersion();

			if (engineVersion is null || latestVersion is null)
				label_OutdatedState.Visible = false;
			else
			{
				label_OutdatedState.Visible = true;

				if (engineVersion < latestVersion)
				{
					label_OutdatedState.Text = $"(Outdated, Latest version is: {latestVersion})";
					label_OutdatedState.ForeColor = Color.LightPink;

					if (_ide.Project.GameVersion is TRVersion.Game.TombEngine)
					{
						button_Update.Visible = true;

						// 1.0.9 is the first version that supports auto-updating
						if (engineVersion.Major <= 1 && engineVersion.Minor <= 0 && engineVersion.Build <= 8)
						{
							button_Update.Enabled = false;
							button_Update.Text = "Cannot Auto-Update engine. Current version is too old.";
							button_Update.Width = 300;
						}
					}

					if (_ide.Project.GameVersion is TRVersion.Game.TR1)
					{
						button_Update.Visible = true;

						// Auto-update ability resets at TRX 1.0
						if (engineVersion.Major < 1)
						{
							button_Update.Text = "Cannot Auto-Update engine. TRX 1.0 introduced breaking changes, which require manual migration.";
							button_Update.Enabled = false;
							button_Update.Width = 300;
						}
					}

					if (_ide.Project.GameVersion is TRVersion.Game.TR2X)
					{
						button_Update.Visible = true;

						// Auto-update ability resets at TRX 1.0
						if (engineVersion.Major < 1)
						{
							button_Update.Text = "Cannot Auto-Update engine. TRX 1.0 introduced breaking changes, which require manual migration.";
							button_Update.Enabled = false;
							button_Update.Width = 300;
						}
					}
				}
				else
				{
					label_OutdatedState.Text = "(Latest)";
					label_OutdatedState.ForeColor = Color.LightGreen;
				}
			}
		}

		private void button_RebuildAll_Click(object sender, EventArgs e)
		{
			LevelProject[] levels = _ide.Project.GetAllValidLevelProjects();

			if (levels.Length == 0)
			{
				DarkMessageBox.Show(this,
					"There are no levels in the current project.",
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

				return;
			}

			var batchList = new BatchCompileList();

			foreach (ILevelProject level in levels)
			{
				string prj2Path;

				if (level.TargetPrj2FileName is null)
					prj2Path = Path.Combine(level.DirectoryPath, level.GetMostRecentlyModifiedPrj2FileName());
				else
					prj2Path = Path.Combine(level.DirectoryPath, level.TargetPrj2FileName);

				batchList.Files.Add(prj2Path);
			}

			string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
			BatchCompileList.SaveToXml(batchListFilePath, batchList);

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
				Arguments = "\"" + batchListFilePath + "\"",
				WorkingDirectory = DefaultPaths.ProgramDirectory,
				UseShellExecute = true
			};

			Process.Start(startInfo);
		}

		private void button_Update_Click(object sender, EventArgs e)
			=> BeginEngineUpdate();

		private void BeginEngineUpdate()
		{
			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine:
					UpdateTEN();
					break;

				case TRVersion.Game.TR1:
					UpdateTR1X();
					break;

				case TRVersion.Game.TR2X:
					UpdateTR2X();
					break;
			}
		}

		private void UpdateTEN()
		{
			var newVersion = _ide.Project.GetLatestEngineVersion();
			var prevVersion = _ide.Project.GetCurrentEngineVersion();

			// 1.0.9 is the first version that supports auto-updating
			if (prevVersion.Major <= 1 && prevVersion.Minor <= 0 && prevVersion.Build <= 8)
			{
				MessageBox.Show(this, "Cannot Auto-Update engine. Current version is too old.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			// In 1.6 onwards we need to upgrade settings file.
			var settingsUpdate16 = newVersion.Major == 1 && newVersion.Minor >= 6 && prevVersion.Major == 1 && prevVersion.Minor <= 6 && prevVersion <= newVersion;

			var message =
				@"This update will replace the following directories and files:

				- Engine/Bin/
				- Engine/Shaders/
				- Engine/Scripts/Engine/
				- Engine/Scripts/SystemStrings.lua" +

				(settingsUpdate16 ? "\n- Engine/Scripts/Settings.lua" : "") +

				"\n\n" +
				@"If any of these directories or files are important to you, please update the engine manually or create a copy of these files before performing this update.

				Are you sure you want to continue?
				This action cannot be reverted.";

			DialogResult result = MessageBox.Show(this, message.TrimIndentation(),
				"Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

			if (result is not DialogResult.Yes)
				return;

			string engineDirectoryPath = Path.Combine(_ide.Project.DirectoryPath, "Engine");

			if (!Directory.Exists(engineDirectoryPath))
			{
				DarkMessageBox.Show(this, "Couldn't locate \"Engine\" directory. Updating is not supported for your project structure.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TEN.zip");
				string libsPath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TEN External DLLs.zip");
				string resourcesPath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TEN Resources.zip");
				using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));
				using var libsArchive = new ZipArchive(File.OpenRead(libsPath));
				using var resourcesArchive = new ZipArchive(File.OpenRead(resourcesPath));

				var bin = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Bin")).ToList();
				bin.AddRange(libsArchive.Entries);
				ExtractEntries(bin, _ide.Project);

				// Delete the "Engine/Shaders/Bin" directory before extracting new shaders
				string compiledShadersPath = Path.Combine(_ide.Project.DirectoryPath, "Engine/Shaders/Bin");

				if (Directory.Exists(compiledShadersPath))
					Directory.Delete(compiledShadersPath, true);

				var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Shaders")).ToList();
				ExtractEntries(shaders, _ide.Project);

				// Delete the "Engine/Scripts/Engine" directory before extracting new scripts
				string engineScriptsPath = Path.Combine(_ide.Project.DirectoryPath, "Engine/Scripts/Engine");

				if (Directory.Exists(engineScriptsPath))
					Directory.Delete(engineScriptsPath, true);

				var scriptsEngine = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Scripts/Engine")).ToList();
				ExtractEntries(scriptsEngine, _ide.Project);

				ZipArchiveEntry systemStrings = engineArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("SystemStrings.lua"));
				systemStrings?.ExtractToFile(Path.Combine(_ide.Project.DirectoryPath, systemStrings.FullName), true);

				// Version-specific file updates.
				if (settingsUpdate16)
				{
					ZipArchiveEntry settings = engineArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("Settings.lua"));
					settings?.ExtractToFile(Path.Combine(_ide.Project.DirectoryPath, settings.FullName), true);
				}

				// Extract resources, but don't overwrite
				ExtractEntries(resourcesArchive.Entries, _ide.Project, false);

				UpdateTENApi(newVersion);
				UpdateVersionLabel();

				DarkMessageBox.Show(this, "Engine has been updated successfully!", "Done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the engine:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateTR1X()
		{
			var prevVersion = _ide.Project.GetCurrentEngineVersion();

			// Auto-update ability resets at TRX 1.0
			if (prevVersion.Major < 1)
			{
				MessageBox.Show(this, "Cannot Auto-Update engine. TRX 1.0 introduced breaking changes, which require manual migration.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			DialogResult result = MessageBox.Show(this,
				"This update will replace the following directories and files:\n\n" +

				"- Engine/shaders/\n" +
				"- Engine/TRX.exe\n\n" +

				"If any of these directories / files are important to you, please update the engine manually or create a copy of these files before performing this update.\n\n" +

				"Are you sure you want to continue?\n" +
				"This action cannot be reverted.",
				"Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

			if (result is not DialogResult.Yes)
				return;

			string engineDirectoryPath = Path.Combine(_ide.Project.DirectoryPath, "Engine");

			if (!Directory.Exists(engineDirectoryPath))
			{
				DarkMessageBox.Show(this, "Couldn't locate \"Engine\" directory. Updating is not supported for your project structure.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR1.zip");
				using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

				var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/shaders")).ToList();
				ExtractEntries(shaders, _ide.Project);

				var executables = engineArchive.Entries.Where(entry => entry.FullName.EndsWith(".exe")).ToList();
				ExtractEntries(executables, _ide.Project);

				UpdateVersionLabel();

				DarkMessageBox.Show(this, "Engine has been updated successfully!", "Done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the engine:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateTR2X()
		{
			var prevVersion = _ide.Project.GetCurrentEngineVersion();

			// Auto-update ability resets at TRX 1.0
			if (prevVersion.Major < 1)
			{
				MessageBox.Show(this, "Cannot Auto-Update engine. TRX 1.0 introduced breaking changes, which require manual migration.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			DialogResult result = MessageBox.Show(this,
				"This update will replace the following directories and files:\n\n" +

				"- Engine/shaders/\n" +
				"- Engine/TRX.exe\n\n" +

				"If any of these directories / files are important to you, please update the engine manually or create a copy of these files before performing this update.\n\n" +

				"Are you sure you want to continue?\n" +
				"This action cannot be reverted.",
				"Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

			if (result is not DialogResult.Yes)
				return;

			string engineDirectoryPath = Path.Combine(_ide.Project.DirectoryPath, "Engine");

			if (!Directory.Exists(engineDirectoryPath))
			{
				DarkMessageBox.Show(this, "Couldn't locate \"Engine\" directory. Updating is not supported for your project structure.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR2X.zip");
				using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

				var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/shaders")).ToList();
				ExtractEntries(shaders, _ide.Project);

				var executables = engineArchive.Entries.Where(entry => entry.FullName.EndsWith(".exe")).ToList();
				ExtractEntries(executables, _ide.Project);

				UpdateVersionLabel();

				DarkMessageBox.Show(this, "Engine has been updated successfully!", "Done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the engine:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static void ExtractEntries(IEnumerable<ZipArchiveEntry> entries, IGameProject targetProject, bool overwrite = true)
		{
			foreach (ZipArchiveEntry entry in entries)
			{
				if (entry.FullName.EndsWith("/"))
				{
					string dirPath = Path.Combine(targetProject.DirectoryPath, entry.FullName);

					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);
				}
				else
				{
					string targetPath = Path.Combine(targetProject.DirectoryPath, entry.FullName);

					if (overwrite)
						entry.ExtractToFile(targetPath, true);
					else if (!File.Exists(targetPath))
						entry.ExtractToFile(targetPath, false);
				}
			}
		}

		private void button_Publish_Click(object sender, EventArgs e)
		{
			using var form = new FormGameArchive(_ide);
			form.ShowDialog();
		}

		private void UpdateTENApi(Version currentEngineVersion)
		{
			try
			{
				TENApiService.InjectTENApi(_ide.Project, currentEngineVersion);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the API:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}

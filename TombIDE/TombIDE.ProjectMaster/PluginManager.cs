using DarkUI.Controls;
using DarkUI.Forms;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class PluginManager : UserControl
	{
		private IDE _ide;
		private DirectoryInfo _pluginsDirectory;

		#region Initialization

		public PluginManager()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			if (!ide.Project.SupportsPlugins)
				return;

			_ide = ide;

			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine: panel_GameLabel.BackgroundImage = Properties.Resources.TEN_LVL; break;
				case TRVersion.Game.TRNG: panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL; break;
				case TRVersion.Game.TR4: panel_GameLabel.BackgroundImage = Properties.Resources.TR4_LVL; break;
				case TRVersion.Game.TR3: panel_GameLabel.BackgroundImage = Properties.Resources.TR3_LVL; break;
				case TRVersion.Game.TR2 or TRVersion.Game.TR2X: panel_GameLabel.BackgroundImage = Properties.Resources.TR2_LVL; break;
				case TRVersion.Game.TR1 or TRVersion.Game.TR1X: panel_GameLabel.BackgroundImage = Properties.Resources.TR1_LVL; break;
			}

			string pluginsPath = _ide.Project.PluginsDirectoryPath;
			_pluginsDirectory = new DirectoryInfo(pluginsPath);

			if (!_pluginsDirectory.Exists || !_pluginsDirectory.EnumerateFiles("*", System.IO.SearchOption.AllDirectories).Any())
			{
				if (!_pluginsDirectory.Exists)
				{
					_pluginsDirectory.Create();
					_pluginsDirectory = new DirectoryInfo(_pluginsDirectory.FullName);
				}

				string parcPath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), "plugins.parc");

				if (File.Exists(parcPath))
					CopyPluginsFromPARCToProject(parcPath);

				if (Directory.Exists(DefaultPaths.TRNGPluginsDirectory)) // Priority
					CopyPluginsFromTIDEToProject();
			}

			UpdatePlugins();
		}

		private void CopyPluginsFromTIDEToProject()
		{
			string[] internalPluginDirs = Directory.GetDirectories(DefaultPaths.TRNGPluginsDirectory, "*", System.IO.SearchOption.TopDirectoryOnly);

			foreach (string pluginFile in Directory.GetFiles(_ide.Project.GetEngineRootDirectoryPath(), "plugin_*.dll", System.IO.SearchOption.TopDirectoryOnly))
			{
				string tidePluginDir = internalPluginDirs.FirstOrDefault(x =>
					Path.GetFileName(x).Equals(Path.GetFileNameWithoutExtension(pluginFile)));

				if (tidePluginDir != null)
					SharedMethods.CopyFilesRecursively(tidePluginDir, Path.Combine(_pluginsDirectory.FullName, Path.GetFileName(tidePluginDir)));
			}
		}

		private void CopyPluginsFromPARCToProject(string parcPath)
		{
			using (ZipArchive parc = ZipFile.OpenRead(parcPath))
			{
				foreach (string pluginFile in Directory.GetFiles(_ide.Project.GetEngineRootDirectoryPath(), "plugin_*.dll", System.IO.SearchOption.TopDirectoryOnly))
				{
					foreach (ZipArchiveEntry entry in parc.Entries)
					{
						if (entry.FullName.Split('\\')[0].Equals(Path.GetFileNameWithoutExtension(pluginFile), StringComparison.OrdinalIgnoreCase))
						{
							string destPath = Path.Combine(_pluginsDirectory.FullName, entry.FullName);
							string dirName = Path.GetDirectoryName(destPath);

							if (!Directory.Exists(dirName))
								Directory.CreateDirectory(dirName);

							entry.ExtractToFile(destPath, true);
						}
					}
				}
			}
		}

		#endregion Initialization

		#region Events

		private void button_OpenArchive_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the archive of the plugin you want to install";
				dialog.Filter = "ZIP Archives|*.zip";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					InstallPluginFromArchive(dialog.FileName);
			}
		}

		private void button_OpenFolder_Click(object sender, EventArgs e)
		{
			using (var dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Select the folder of the plugin you want to install";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					InstallPluginFromFolder(dialog.Folder);
			}
		}

		private void button_Remove_Click(object sender, EventArgs e)
		{
			var selectedPluginFile = (FileInfo)treeView.SelectedNodes[0].Tag;

			if (!File.Exists(selectedPluginFile.FullName))
			{
				UpdatePlugins();
				return;
			}

			string pluginName = treeView.SelectedNodes[0].Text;

			DialogResult result = DarkMessageBox.Show(this,
				$"Are you sure you want to remove the following plugin from your project:\n" +
				$"\"{pluginName}\" ?\n" +
				"This action will move the plugin directory into the recycle bin.",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					string engineDllFilePath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), ((FileInfo)treeView.SelectedNodes[0].Tag).Name);

					if (File.Exists(engineDllFilePath))
						FileSystem.DeleteFile(engineDllFilePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

					string pluginDirectoryPath = ((FileInfo)treeView.SelectedNodes[0].Tag).DirectoryName;

					if (Directory.Exists(pluginDirectoryPath))
						FileSystem.DeleteDirectory(pluginDirectoryPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

					UpdatePlugins();
				}
				catch { }
			}
		}

		private void button_OpenDirectory_Click(object sender, EventArgs e)
		{
			var selectedPluginFile = (FileInfo)treeView.SelectedNodes[0].Tag;

			if (!File.Exists(selectedPluginFile.FullName))
			{
				UpdatePlugins();
				return;
			}

			SharedMethods.OpenInExplorer(Path.GetDirectoryName(selectedPluginFile.FullName));
		}

		private void button_Download_Click(object sender, EventArgs e)
			=> Process.Start(new ProcessStartInfo()
			{
				FileName = "https://www.tombraiderforums.com/showpost.php?p=7636390",
				UseShellExecute = true
			});

		private void button_Refresh_Click(object sender, EventArgs e)
			=> UpdatePlugins();

		private void treeView_SelectedNodesChanged(object sender, EventArgs e)
		{
			button_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
			button_Remove.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
			menuItem_Remove.Enabled = treeView.SelectedNodes.Count > 0;

			if (treeView.SelectedNodes.Count > 0)
				UpdatePluginInfoOverview();
		}

		#endregion Events

		#region Methods

		private void UpdatePlugins()
		{
			treeView.SelectedNodes.Clear();
			treeView.Nodes.Clear();
			treeView.Invalidate();

			if (_pluginsDirectory.Exists)
			{
				foreach (DirectoryInfo subDirectory in _pluginsDirectory.GetDirectories())
				{
					FileInfo dllFile = subDirectory.GetFiles("plugin_*.dll", System.IO.SearchOption.TopDirectoryOnly).FirstOrDefault();

					if (dllFile == null)
						continue;

					string pluginName = Path.GetFileNameWithoutExtension(dllFile.Name);
					string btnFilePath = Path.Combine(subDirectory.FullName, pluginName + ".btn");

					if (File.Exists(btnFilePath))
					{
						string nameLine = File.ReadAllLines(btnFilePath)
							.FirstOrDefault(line => line.StartsWith("NAME#"));

						if (nameLine != null)
							pluginName = nameLine.Replace("NAME#", string.Empty).Trim();
					}

					var node = new DarkTreeNodeEx(pluginName, "~/" + dllFile.Name) { Tag = dllFile };
					treeView.Nodes.Add(node);
				}
			}

			CopyDLLFilesToEngineDirectory();
			HandleScriptReferenceFiles();

			MnemonicData.SetupConstants(DefaultPaths.InternalNGCDirectory);

			_ide.RaiseEvent(new IDE.ScriptEditor_ReloadSyntaxHighlightingEvent());

			UpdatePluginInfoOverview();
			treeView.Invalidate();
		}

		private void InstallPluginFromArchive(string archivePath)
		{
			try
			{
				if (!_pluginsDirectory.Exists)
				{
					_pluginsDirectory.Create();
					_pluginsDirectory = new DirectoryInfo(_pluginsDirectory.FullName);
				}

				using (var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
				using (var archive = new ZipArchive(fileStream))
				{
					IEnumerable<ZipArchiveEntry> dllFileEntries = archive.Entries.Where(entry =>
						Regex.IsMatch(entry.Name, @"plugin_.*\.dll", RegexOptions.IgnoreCase));

					if (dllFileEntries.Count() == 0)
						throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
					else if (dllFileEntries.Count() > 1)
						throw new ArgumentException("Selected archive contains more than 1 valid plugin .dll file.");

					ZipArchiveEntry dllFileEntry = dllFileEntries.First();

					string dllFileName = dllFileEntry.Name;
					string dllSubPath = dllFileEntry.FullName.Remove(dllFileEntry.FullName.Length - dllFileName.Length);

					string unzipDirectoryPath = Path.Combine(_pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

					if (!Directory.Exists(unzipDirectoryPath))
						Directory.CreateDirectory(unzipDirectoryPath);

					IEnumerable<ZipArchiveEntry> entriesToExtract = archive.Entries.Where(entry =>
						entry.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

					foreach (ZipArchiveEntry entry in entriesToExtract)
					{
						string unzipFilePath = Path.Combine(unzipDirectoryPath, entry.FullName.Remove(0, dllSubPath.Length));

						if (string.IsNullOrWhiteSpace(Path.GetExtension(unzipFilePath)))
						{
							if (!Directory.Exists(unzipFilePath))
								Directory.CreateDirectory(unzipFilePath);

							continue;
						}

						string fileSubDirectory = Path.GetDirectoryName(unzipFilePath);

						if (!Directory.Exists(fileSubDirectory))
							Directory.CreateDirectory(fileSubDirectory);

						entry.ExtractToFile(unzipFilePath, true);
					}

					string pluginString = Path.GetFileNameWithoutExtension(dllFileEntry.FullName);

					_ide.ScriptEditor_AddNewPluginEntry(pluginString);
					_ide.ScriptEditor_AddNewNGString(pluginString);
				}

				UpdatePlugins();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void InstallPluginFromFolder(string folderPath)
		{
			try
			{
				if (!_pluginsDirectory.Exists)
				{
					_pluginsDirectory.Create();
					_pluginsDirectory = new DirectoryInfo(_pluginsDirectory.FullName);
				}

				var selectedDir = new DirectoryInfo(folderPath);

				FileInfo[] dllFiles = selectedDir.GetFiles("plugin_*.dll", System.IO.SearchOption.TopDirectoryOnly);

				if (dllFiles.Length == 0)
					throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");
				else if (dllFiles.Length > 1)
					throw new ArgumentException("Selected folder contains more than 1 valid plugin .dll file.");

				FileInfo dllFile = dllFiles.First();

				string dllFileName = dllFile.Name;
				string dllSubPath = dllFile.FullName.Remove(dllFile.FullName.Length - dllFileName.Length);

				string copyDirectoryPath = Path.Combine(_pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

				if (!Directory.Exists(copyDirectoryPath))
					Directory.CreateDirectory(copyDirectoryPath);

				IEnumerable<FileInfo> filesToCopy = selectedDir.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly).Where(file =>
					file.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

				foreach (FileInfo file in filesToCopy)
				{
					string destFilePath = Path.Combine(copyDirectoryPath, file.FullName.Remove(0, dllSubPath.Length));
					file.CopyTo(destFilePath, true);
				}

				string pluginString = Path.GetFileNameWithoutExtension(dllFile.FullName);

				_ide.ScriptEditor_AddNewPluginEntry(pluginString);
				_ide.ScriptEditor_AddNewNGString(pluginString);

				UpdatePlugins();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdatePluginInfoOverview()
		{
			if (treeView.SelectedNodes.Count == 0)
			{
				panel_Logo.BackgroundImage = null;
				label_NoInfo.Visible = true;

				textBox_Title.Text = string.Empty;
				textBox_DLLName.Text = string.Empty;

				richTextBox_Description.Text = string.Empty;

				return;
			}

			label_NoInfo.Visible = false;

			var selectedPluginFile = (FileInfo)treeView.SelectedNodes[0].Tag;

			if (!File.Exists(selectedPluginFile.FullName))
			{
				UpdatePlugins();
				return;
			}

			textBox_Title.Text = treeView.SelectedNodes[0].Text;
			textBox_DLLName.Text = Path.GetFileName(selectedPluginFile.Name);

			try
			{
				bool logoFound = false;

				foreach (string file in Directory.GetFiles(Path.GetDirectoryName(selectedPluginFile.FullName)))
				{
					string extension = Path.GetExtension(file).ToLower();

					if (extension == ".jpg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
					{
						using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
						{
							panel_Logo.BackgroundImage = Image.FromStream(stream);
							logoFound = true;
						}

						break;
					}
				}

				if (!logoFound)
				{
					panel_Logo.BackgroundImage = null;
					label_NoLogo.Visible = true;
				}
				else
					label_NoLogo.Visible = false;

				string descriptionFilePath = Path.Combine(
					Path.GetDirectoryName(selectedPluginFile.FullName),
					Path.GetFileNameWithoutExtension(selectedPluginFile.FullName) + ".txt");

				if (File.Exists(descriptionFilePath))
					richTextBox_Description.Text = File.ReadAllText(descriptionFilePath);
				else
					richTextBox_Description.Text = string.Empty;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void HandleScriptReferenceFiles()
		{
			// Delete all .script files from the internal /NGC/ folder
			foreach (string file in Directory.GetFiles(DefaultPaths.InternalNGCDirectory, "plugin_*.script", System.IO.SearchOption.TopDirectoryOnly))
				File.Delete(file);

			foreach (FileInfo scriptFile in _pluginsDirectory.GetFiles("plugin_*.script", System.IO.SearchOption.AllDirectories))
			{
				string destPath = Path.Combine(DefaultPaths.InternalNGCDirectory, scriptFile.Name);
				scriptFile.CopyTo(destPath, true);
			}
		}

		private void CopyDLLFilesToEngineDirectory()
		{
			foreach (FileInfo dllFile in _pluginsDirectory.GetFiles("plugin_*.dll", System.IO.SearchOption.AllDirectories))
			{
				string destPath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), dllFile.Name);
				dllFile.CopyTo(destPath, true);
			}
		}

		#endregion Methods
	}
}

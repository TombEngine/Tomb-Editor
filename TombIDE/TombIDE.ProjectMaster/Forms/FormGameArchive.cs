using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Forms
{
	public partial class FormGameArchive : DarkForm
	{
		private IDE _ide;
		private FormPleaseWait _waitForm = new FormPleaseWait();

		public FormGameArchive(IDE ide)
		{
			InitializeComponent();

			_ide = ide;
		}

		private string _fileName;

		private void button_Generate_Click(object sender, EventArgs e)
		{
			using (var dialog = new SaveFileDialog())
			{
				dialog.InitialDirectory = _ide.Project.DirectoryPath;
				dialog.FileName = $"{_ide.Project.Name.Replace(' ', '_')}.zip";
				dialog.Filter = "Zip Archive (*.zip)|*.zip";
				dialog.DefaultExt = "zip";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					_fileName = dialog.FileName;
					_waitForm.Show(this);
					timer.Start();
				}
			}
		}

		public void GenerateArchive(string filePath, string readmeText)
		{
			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TR1:
					GenerateTR1Archive(filePath, readmeText);
					break;

				case TRVersion.Game.TR2X:
					GenerateTR2XArchive(filePath, readmeText);
					break;

				case TRVersion.Game.TR2:
					GenerateTR2Archive(filePath, readmeText);
					break;

				case TRVersion.Game.TR3:
					GenerateTR3Archive(filePath, readmeText);
					break;

				case TRVersion.Game.TR4 or TRVersion.Game.TRNG:
					GenerateTR4Archive(filePath, readmeText);
					break;

				case TRVersion.Game.TombEngine:
					GenerateTENArchive(filePath, readmeText);
					break;
			}
		}

		public void GenerateTR1Archive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "music"),
				Path.Combine(engineDirectory, "cfg"),
				Path.Combine(engineDirectory, "data"),
				Path.Combine(engineDirectory, "shaders")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "TR1X.exe"),
				Path.Combine(engineDirectory, "Tomb1Main.exe"),
				Path.Combine(engineDirectory, "TR1X_ConfigTool.exe"),
				Path.Combine(engineDirectory, "Tomb1Main_ConfigTool.exe"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			CreateArchive(importantFolders, importantFiles, filePath, readmeText);
		}

		public void GenerateTR2XArchive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "music"),
				Path.Combine(engineDirectory, "cfg"),
				Path.Combine(engineDirectory, "data"),
				Path.Combine(engineDirectory, "shaders")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "TR2X.exe"),
				Path.Combine(engineDirectory, "TR2X_ConfigTool.exe"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			CreateArchive(importantFolders, importantFiles, filePath, readmeText);
		}

		public void GenerateTR2Archive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "audio"),
				Path.Combine(engineDirectory, "data"),
				Path.Combine(engineDirectory, "ExtraOptions"),
				Path.Combine(engineDirectory, "music"),
				Path.Combine(engineDirectory, "pix")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "COPYING.txt"),
				Path.Combine(engineDirectory, "Tomb2.exe"),
				Path.Combine(engineDirectory, "TR2Main.json"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			var allImportantFiles = new List<string>();
			allImportantFiles.AddRange(importantFiles);
			allImportantFiles.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

			CreateArchive(importantFolders, allImportantFiles, filePath, readmeText);
		}

		public void GenerateTR3Archive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "data"),
				Path.Combine(engineDirectory, "pix")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "audio", "cdaudio.wad"),
				Path.Combine(engineDirectory, "data.bin"),
				Path.Combine(engineDirectory, "tomb3.exe"),
				Path.Combine(engineDirectory, "tomb3_ConfigTool.exe"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			var allImportantFiles = new List<string>();
			allImportantFiles.AddRange(importantFiles);
			allImportantFiles.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

			CreateArchive(importantFolders, allImportantFiles, filePath, readmeText, true);
		}

		public void GenerateTR4Archive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "audio"),
				Path.Combine(engineDirectory, "data"),
				Path.Combine(engineDirectory, "pix"),
				Path.Combine(engineDirectory, "patches")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "load.bmp"),
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "patches.bin"),
				Path.Combine(engineDirectory, "patches.fpd"),
				Path.Combine(engineDirectory, "tomb4.exe"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			var allImportantFiles = new List<string>();
			allImportantFiles.AddRange(importantFiles);
			allImportantFiles.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));
			allImportantFiles.AddRange(Directory.GetFiles(engineDirectory, "*.dat", SearchOption.TopDirectoryOnly));

			CreateArchive(importantFolders, allImportantFiles, filePath, readmeText);
		}

		public void GenerateTENArchive(string filePath, string readmeText)
		{
			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();

			string[] importantFolders = new string[]
			{
				Path.Combine(engineDirectory, "Audio"),
				Path.Combine(engineDirectory, "Bin"),
				Path.Combine(engineDirectory, "Data"),
				Path.Combine(engineDirectory, "FMV"),
				Path.Combine(engineDirectory, "Screens"),
				Path.Combine(engineDirectory, "Scripts"),
				Path.Combine(engineDirectory, "Shaders"),
				Path.Combine(engineDirectory, "Textures")
			};

			string[] importantFiles = new string[]
			{
				Path.Combine(engineDirectory, "splash.bmp"),
				Path.Combine(engineDirectory, "TombEngine.exe"),
				Path.Combine(engineDirectory, "splash.xml")
			};

			var allImportantFiles = new List<string>();
			allImportantFiles.AddRange(importantFiles);
			allImportantFiles.AddRange(Directory.GetFiles(engineDirectory, "*.dll", SearchOption.TopDirectoryOnly));

			CreateArchive(importantFolders, allImportantFiles, filePath, readmeText);
		}

		private void CreateArchive(IEnumerable<string> importantFolders, IEnumerable<string> importantFiles,
			string filePath, string readmeText, bool createSavesFolder = false)
		{
			string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			if (!Directory.Exists(tempDirectory))
				Directory.CreateDirectory(tempDirectory);

			string engineDirectory = _ide.Project.GetEngineRootDirectoryPath();
			string targetTempEngineDirectory = Path.Combine(tempDirectory, "Engine");

			foreach (string folder in importantFolders)
			{
				if (!Directory.Exists(folder))
					continue;

				string pathPart = folder.Remove(0, engineDirectory.Length);
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				SharedMethods.CopyFilesRecursively(folder, targetPath);
			}

			if (createSavesFolder)
				Directory.CreateDirectory(Path.Combine(targetTempEngineDirectory, "saves"));

			foreach (string file in importantFiles)
			{
				if (!File.Exists(file))
					continue;

				string pathPart = file.Remove(0, engineDirectory.Length);
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				string targetDirectory = Path.GetDirectoryName(targetPath);

				if (!Directory.Exists(targetDirectory))
					Directory.CreateDirectory(targetDirectory);

				File.Copy(file, targetPath);
			}

			// Copy launch.exe
			string launchFilePath = _ide.Project.GetLauncherFilePath();

			if (File.Exists(launchFilePath))
				File.Copy(launchFilePath, Path.Combine(tempDirectory, Path.GetFileName(launchFilePath)), true);

			if (!string.IsNullOrWhiteSpace(readmeText))
				File.WriteAllText(Path.Combine(tempDirectory, "README.txt"), readmeText);

			if (File.Exists(filePath))
				File.Delete(filePath);

			ZipFile.CreateFromDirectory(tempDirectory, filePath);

			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();

			GenerateArchive(_fileName, richTextBox.Text);
			_waitForm.Close();

			DarkMessageBox.Show(this, "Archive creation completed.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);

			DialogResult = DialogResult.OK;
		}
	}
}

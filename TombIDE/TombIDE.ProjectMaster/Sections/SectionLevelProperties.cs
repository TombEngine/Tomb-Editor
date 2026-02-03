using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombIDE.ProjectMaster
{
	public partial class SectionLevelProperties : UserControl
	{
		private IDE _ide = null!;

		#region Initialization

		public SectionLevelProperties()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			AddPinnedProgramsToContextMenu();
		}

		private void AddPinnedProgramsToContextMenu()
		{
			foreach (string programPath in _ide.IDEConfiguration.PinnedProgramPaths)
			{
				string exeFileName = Path.GetFileName(programPath).ToLower();

				// Exclude these programs from the list
				if (exeFileName is "tombeditor.exe" or "wadtool.exe" or "soundtool.exe" or "tombide.exe"
					or "ng_center.exe" or "tomb4.exe" or "pctomb5.exe")
					continue;

				// Exclude batch files
				if (exeFileName.EndsWith(".bat"))
					continue;

				// Get the ProductName and the icon of the program
				string? programName = FileVersionInfo.GetVersionInfo(programPath).ProductName;

				Image? image = Icon.ExtractAssociatedIcon(programPath)?.ToBitmap();
				image = image is not null ? ImageHandling.ResizeImage(image, 16, 16) : null;

				if (string.IsNullOrEmpty(programName))
					programName = Path.GetFileNameWithoutExtension(programPath);

				// Create the menu item
				var item = new ToolStripMenuItem
				{
					Image = image,
					Text = "Open with " + programName,
					Tag = programPath
				};

				// Bind the OnContextMenuProgramClicked event method to the item and add it to the list
				item.Click += OnContextMenuProgramClicked;
				contextMenu.Items.Add(item);
			}
		}

		#endregion Initialization

		#region Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedLevelChangedEvent)
			{
				// Update the whole control depending on the level selection
				if (_ide.SelectedLevel != null)
				{
					tabControl.Enabled = true;

					if (tabControl.SelectedIndex == 0)
						UpdateSettings();
					else if (tabControl.SelectedIndex == 1)
						InitializeResourceListRefresh();
				}
				else
				{
					ClearEverything();
					tabControl.Enabled = false;
				}
			}
			else if (obj is IDE.ProgramButtonsModifiedEvent)
			{
				// Cache the first 3 items, because they are important
				var cachedItems = new List<ToolStripItem>
				{
					contextMenu.Items[0],
					contextMenu.Items[1],
					contextMenu.Items[2]
				};

				// Clear the whole list
				contextMenu.Items.Clear();

				// Re-Add the cached items
				contextMenu.Items.AddRange(cachedItems.ToArray());

				AddPinnedProgramsToContextMenu();
			}
		}

		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl.SelectedIndex == 0)
				UpdateSettings();
			else if (tabControl.SelectedIndex == 1)
				InitializeResourceListRefresh();
		}

		private void radioButton_LatestFile_CheckedChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			// Check if the setting actually changed
			if (radioButton_LatestFile.Checked && _ide.SelectedLevel.TargetPrj2FileName is not null)
			{
				ClearAndDisablePrj2FileList();

				_ide.SelectedLevel.TargetPrj2FileName = null;
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
		}

		private void radioButton_SpecificFile_CheckedChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			// Check if the setting actually changed
			if (radioButton_SpecificFile.Checked && _ide.SelectedLevel.TargetPrj2FileName is null)
			{
				EnableAndFillPrj2FileList();

				_ide.SelectedLevel.TargetPrj2FileName = treeView_AllPrjFiles.SelectedNodes[0].Text;
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
		}

		private void treeView_AllPrjFiles_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			if (treeView_AllPrjFiles.SelectedNodes.Count > 0)
			{
				// Check if the setting actually changed
				if (_ide.SelectedLevel.TargetPrj2FileName != treeView_AllPrjFiles.SelectedNodes[0].Text)
				{
					_ide.SelectedLevel.TargetPrj2FileName = treeView_AllPrjFiles.SelectedNodes[0].Text;
					_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
				}
			}
		}

		private void checkBox_ShowAllFiles_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_LatestFile.Checked)
				return;

			UpdatePrj2FileList();

			// If the user unchecked the checkBox and the SpecificFile was a backup file
			if (!checkBox_ShowAllFiles.Checked && Prj2Helper.IsBackupFile(Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.TargetPrj2FileName)))
				treeView_AllPrjFiles.SelectNode(treeView_AllPrjFiles.Nodes[0]); // Select something else since the item is no longer on the list
		}

		private void timer_ResourceRefreshDelay_Tick(object sender, EventArgs e)
		{
			timer_ResourceRefreshDelay.Stop();
			UpdateResourceList();
		}

		private void treeView_Resources_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				ShowResourceContextMenu();
		}

		private void treeView_Resources_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OpenSelectedResource();
		}

		private void menuItem_Open_Click(object sender, EventArgs e) => OpenSelectedResource();

		private void menuItem_OpenFolder_Click(object sender, EventArgs e)
			=> SharedMethods.OpenInExplorer(Path.GetDirectoryName(treeView_Resources.SelectedNodes[0].Text));

		private void OpenSelectedResource()
		{
			if (treeView_Resources.SelectedNodes.Count == 0)
				return;

			// Check if the clicked node is not a default node
			if (treeView_Resources.Nodes.Contains(treeView_Resources.SelectedNodes[0]))
				return;

			try
			{
				string filePath = treeView_Resources.SelectedNodes[0].Text;
				var startInfo = new ProcessStartInfo();

				if (treeView_Resources.SelectedNodes[0].ParentNode == treeView_Resources.Nodes[1]) // Wad handling
				{
					startInfo.FileName = Path.Combine(DefaultPaths.ProgramDirectory, "WadTool.exe");
					startInfo.Arguments = "\"" + filePath + "\"";
				}
				else
					startInfo.FileName = filePath;

				startInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
				startInfo.UseShellExecute = true;

				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ShowResourceContextMenu()
		{
			if (treeView_Resources.SelectedNodes.Count == 0)
				return;

			// Check if the clicked node is not a default node
			if (treeView_Resources.Nodes.Contains(treeView_Resources.SelectedNodes[0]))
				return;

			contextMenu.Show(Cursor.Position);
		}

		private void OnContextMenuProgramClicked(object? sender, EventArgs e)
		{
			string? programPath = (sender as ToolStripMenuItem)?.Tag?.ToString();

			if (string.IsNullOrWhiteSpace(programPath) || treeView_Resources.SelectedNodes.Count == 0)
				return;

			string selectedFilePath = treeView_Resources.SelectedNodes[0].Text;

			var startInfo = new ProcessStartInfo
			{
				FileName = programPath,
				Arguments = "\"" + selectedFilePath + "\"",
				WorkingDirectory = Path.GetDirectoryName(selectedFilePath),
				UseShellExecute = true
			};

			Process.Start(startInfo);
		}

		#endregion Events

		#region Methods

		private void UpdateSettings()
		{
			// Update the radio buttons
			radioButton_LatestFile.Checked = _ide.SelectedLevel.TargetPrj2FileName == null;
			radioButton_SpecificFile.Checked = _ide.SelectedLevel.TargetPrj2FileName != null;

			// Update the .prj2 file list depending on which radio button was checked
			if (radioButton_LatestFile.Checked)
				ClearAndDisablePrj2FileList();
			else
				EnableAndFillPrj2FileList();
		}

		private void InitializeResourceListRefresh()
		{
			tabControl.Invalidate();

			treeView_Resources.SelectedNodes.Clear();
			treeView_Resources.Nodes.Clear();

			treeView_Resources.Invalidate();

			if (_ide.SelectedLevel == null)
				return;

			label_Loading.Visible = true;

			timer_ResourceRefreshDelay.Start();
		}

		private void UpdateResourceList()
		{
			AddDefaultResourceNodes();

			string prj2Path = string.Empty;

			if (_ide.SelectedLevel.TargetPrj2FileName is null)
				prj2Path = Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.GetMostRecentlyModifiedPrj2FileName());
			else
				prj2Path = Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.TargetPrj2FileName);

			try
			{
				LevelSettings settings;

				using (var stream = new FileStream(prj2Path, FileMode.Open, FileAccess.Read, FileShare.Read))
					settings = Prj2Loader.LoadFromPrj2OnlySettings(prj2Path, stream);

				AddTextureFileNodes(settings);
				AddWadFileNodes(settings);
				AddGeometryFileNodes(settings);
			}
			catch { } // Whatever

			label_Loading.Visible = false;
			treeView_Resources.Invalidate();
		}

		private void ClearEverything() // Used when _ide.SelectedLevel is null
		{
			radioButton_LatestFile.Checked = false;
			radioButton_SpecificFile.Checked = false;

			treeView_AllPrjFiles.SelectedNodes.Clear();
			treeView_AllPrjFiles.Nodes.Clear();
			treeView_AllPrjFiles.Invalidate();

			checkBox_ShowAllFiles.Checked = false;

			treeView_Resources.SelectedNodes.Clear();
			treeView_Resources.Nodes.Clear();
			treeView_Resources.Invalidate();
		}

		private void EnableAndFillPrj2FileList()
		{
			treeView_AllPrjFiles.Enabled = true;
			checkBox_ShowAllFiles.Enabled = true;

			// When the user switched the _ide.SelectedLevel and the current _ide.SelectedLevel 's SpecificFile is a backup file,
			// then check this checkbox, otherwise it will reset the SpecificFile to a non-backup file and we don't want that
			if (_ide.SelectedLevel.TargetPrj2FileName is not null)
				checkBox_ShowAllFiles.Checked = Prj2Helper.IsBackupFile(Path.Combine(_ide.SelectedLevel.DirectoryPath, _ide.SelectedLevel.TargetPrj2FileName));

			UpdatePrj2FileList();
		}

		private void ClearAndDisablePrj2FileList()
		{
			treeView_AllPrjFiles.SelectedNodes.Clear();
			treeView_AllPrjFiles.Nodes.Clear();

			checkBox_ShowAllFiles.Checked = false;

			treeView_AllPrjFiles.Enabled = false;
			checkBox_ShowAllFiles.Enabled = false;
		}

		private void UpdatePrj2FileList()
		{
			treeView_AllPrjFiles.Nodes.Clear();

			foreach (string file in Directory.GetFiles(_ide.SelectedLevel.DirectoryPath, "*.prj2", SearchOption.TopDirectoryOnly))
			{
				// Don't show backup files if checkBox_ShowAllFiles is unchecked
				if (!checkBox_ShowAllFiles.Checked && Prj2Helper.IsBackupFile(file))
					continue;

				// Create the .prj2 file node
				var node = new DarkTreeNode
				{
					Text = Path.GetFileName(file),
					Tag = file
				};

				// Add the node to the .prj2 file list
				treeView_AllPrjFiles.Nodes.Add(node);
			}

			// Select the SpecificFile node (if the file exists on the list)
			bool nodeFound = false;

			if (_ide.SelectedLevel.TargetPrj2FileName is not null)
			{
				foreach (DarkTreeNode node in treeView_AllPrjFiles.Nodes)
				{
					if (node.Text.Equals(_ide.SelectedLevel.TargetPrj2FileName, StringComparison.OrdinalIgnoreCase))
					{
						treeView_AllPrjFiles.SelectNode(node);
						nodeFound = true;

						break;
					}
				}
			}

			if (!nodeFound)
				treeView_AllPrjFiles.SelectNode(treeView_AllPrjFiles.Nodes[0]); // Select the first node if no file was found

			treeView_AllPrjFiles.Invalidate();
		}

		private void AddDefaultResourceNodes()
		{
			var texturesNode = new DarkTreeNode
			{
				Icon = Properties.Resources.image_file.ToBitmap(),
				Text = "Textures",
			};

			var wadFilesNode = new DarkTreeNode
			{
				Icon = Properties.Resources.wad_file.ToBitmap(),
				Text = "Wad Files",
			};

			var geometryNode = new DarkTreeNode
			{
				Icon = Properties.Resources.obj_file.ToBitmap(),
				Text = "Imported Geometry",
			};

			treeView_Resources.Nodes.Add(texturesNode);
			treeView_Resources.Nodes.Add(wadFilesNode);
			treeView_Resources.Nodes.Add(geometryNode);
		}

		private void AddTextureFileNodes(LevelSettings settings)
		{
			for (int i = 0; i < settings.Textures.Count; i++)
			{
				LevelTexture texture = settings.Textures[i];
				string textureFilePath = GetFullFilePath(texture.Path, settings);

				if (!File.Exists(textureFilePath))
					continue;

				var node = new DarkTreeNode
				{
					Icon = Properties.Resources.image_file.ToBitmap(),
					Text = textureFilePath,
				};

				treeView_Resources.Nodes[0].Nodes.Add(node);
				treeView_Resources.Nodes[0].Expanded = true;
			}
		}

		private void AddWadFileNodes(LevelSettings settings)
		{
			for (int i = 0; i < settings.Wads.Count; i++)
			{
				ReferencedWad wad = settings.Wads[i];
				string wadFilePath = GetFullFilePath(wad.Path, settings);

				if (!File.Exists(wadFilePath))
					continue;

				var node = new DarkTreeNode
				{
					Icon = Properties.Resources.wad_file.ToBitmap(),
					Text = wadFilePath,
				};

				treeView_Resources.Nodes[1].Nodes.Add(node);
				treeView_Resources.Nodes[1].Expanded = true;
			}
		}

		private void AddGeometryFileNodes(LevelSettings settings)
		{
			for (int i = 0; i < settings.ImportedGeometries.Count; i++)
			{
				ImportedGeometry geometry = settings.ImportedGeometries[i];
				string geometryFilePath = GetFullFilePath(geometry.Info.Path, settings);

				if (!File.Exists(geometryFilePath))
					continue;

				var node = new DarkTreeNode
				{
					Icon = Properties.Resources.obj_file.ToBitmap(),
					Text = geometryFilePath,
				};

				treeView_Resources.Nodes[2].Nodes.Add(node);
				treeView_Resources.Nodes[2].Expanded = true;
			}
		}

		private string GetFullFilePath(string filePath, LevelSettings settings)
		{
			string fullPath = string.Empty;

			if (filePath.Contains("$(LevelDirectory)"))
			{
				int foldersToGoBackCount = Regex.Matches(filePath, @"\\\.\.").Count;

				string partialPath = filePath.Replace("$(LevelDirectory)", string.Empty).Replace(@"\..", string.Empty);
				string? missingPart = settings.LevelFilePath;

				for (int i = 0; i <= foldersToGoBackCount; i++)
					missingPart = Path.GetDirectoryName(missingPart);

				fullPath = missingPart + partialPath;
			}
			else
				fullPath = filePath;

			return fullPath;
		}

		#endregion Methods
	}
}

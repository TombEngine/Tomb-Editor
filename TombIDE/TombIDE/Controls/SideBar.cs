using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.Controls
{
	public partial class SideBar : UserControl
	{
		private IDE _ide;

		public event EventHandler<IDETab> SelectedIDETabChanged;

		#region Initialization

		public SideBar()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			if (_ide.Project.GameVersion == TRVersion.Game.TombEngine)
			{
				panel_Icon_Plugins.BackgroundImage = Properties.Resources.ide_vscode_30;
				label_Plugins.Text = "VS Code";

				const string tip = "Open project in Visual Studio Code...";

				toolTip.SetToolTip(panel_Icon_Plugins, tip);
				toolTip.SetToolTip(label_Plugins, tip);
			}
			else if (_ide.Project.GameVersion != TRVersion.Game.TRNG)
			{
				panel_Icon_Plugins.BackgroundImage = Properties.Resources.ide_plugin_30_disabled;
				label_Plugins.ForeColor = Color.FromArgb(128, 128, 128);

				const string tip = "Plugins are not supported by the current game engine.";

				toolTip.SetToolTip(panel_Icon_Plugins, tip);
				toolTip.SetToolTip(label_Plugins, tip);
			}

			button_LaunchGame.Image = Icon.ExtractAssociatedIcon(_ide.Project.GetLauncherFilePath()).ToBitmap();

			InitializeFLEP();
			AddPinnedPrograms();

			if (_ide.Project.GameVersion is not TRVersion.Game.TR1 and not TRVersion.Game.TR2X and not TRVersion.Game.TombEngine)
				button_Update.Enabled = false;
		}

		private void InitializeFLEP()
		{
			string flepExePath = Path.Combine(_ide.Project.GetEngineRootDirectoryPath(), "flep.exe");

			if (File.Exists(flepExePath))
			{
				button_Special.Image = Icon.ExtractAssociatedIcon(flepExePath).ToBitmap();
				toolTip.SetToolTip(button_Special, "Launch FLEP");
				button_Special.Click += Special_LaunchFLEP;
			}
			else
				button_Special.Dispose();
		}

		private void AddPinnedPrograms()
		{
			if (_ide.IDEConfiguration.PinnedProgramPaths.Count > 0)
			{
				foreach (string programPath in _ide.IDEConfiguration.PinnedProgramPaths)
					AddProgramButton(programPath, false);
			}
			else
			{
				// Add the default buttons
				AddProgramButton(DefaultPaths.TombEditorExecutable, false);
				AddProgramButton(DefaultPaths.WadToolExecutable, false);
				AddProgramButton(DefaultPaths.SoundToolExecutable, false);
			}

			SavePinnedPrograms();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ScriptEditor_ContentChangedEvent)
			{
				// Indicate changes inside the Scripting Studio
				panelButton_ScriptingStudio.BackColor = Color.FromArgb(180, 100, 0);
				timer_ScriptButtonBlinking.Interval = 1;
				timer_ScriptButtonBlinking.Start();
			}
		}

		#endregion Initialization

		#region Program button Drag 'n' Drop

		private const int DRAGGING_THRESHOLD = 5;

		private Point _clickPosition;
		private DarkButton _draggedButton;
		private bool _isDragging;

		private void ProgramButton_MouseDown(object sender, MouseEventArgs e)
		{
			if (_draggedButton == null)
				_clickPosition = e.Location; // Get click position for dragging threshold

			_draggedButton = (DarkButton)sender;
		}

		private void ProgramButton_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || _draggedButton == null)
				return;

			int dragDistance = Math.Abs(e.Y - _clickPosition.Y);

			if (dragDistance < DRAGGING_THRESHOLD)
				return;

			flowLayoutPanel_Programs.Capture = true; // While dragging the button, always capture the panel, even if the cursor is out of bounds
			Cursor.Current = Cursors.SizeAll;

			_isDragging = true;
		}

		private void panel_Programs_MouseUp(object sender, MouseEventArgs e)
		{
			_draggedButton = null;

			flowLayoutPanel_Programs.Capture = false;
			Cursor.Current = Cursors.Default;

			_isDragging = false;
		}

		// We move from ProgramButton_MouseMove to this method if panel_Programs.Capture is true
		private void panel_Programs_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isDragging)
				return;

			// Check if we can swap the button

			DarkButton hoveredButton = GetHoveredButton();

			if (hoveredButton == null)
				return;

			if (hoveredButton == _draggedButton)
				return;

			SwapButtons(_draggedButton, hoveredButton);
			SavePinnedPrograms();
		}

		private DarkButton GetHoveredButton()
		{
			foreach (DarkButton button in flowLayoutPanel_Programs.Controls.OfType<DarkButton>())
			{
				int allScreenWidth = GetAllScreenWidth();
				var rect = new Rectangle(-allScreenWidth, button.Location.Y, allScreenWidth * 4, button.Size.Height);
				// "allScreenWidth * 4" because someone would eventually break it

				if (rect.Contains(flowLayoutPanel_Programs.PointToClient(Cursor.Position)))
					return button;
			}

			return null;
		}

		private int GetAllScreenWidth()
			=> Screen.AllScreens.Select(x => x.Bounds.Width).Sum();

		private void SwapButtons(DarkButton src, DarkButton dst)
		{
			int srcChildIndex = flowLayoutPanel_Programs.Controls.GetChildIndex(src);
			int dstChildIndex = flowLayoutPanel_Programs.Controls.GetChildIndex(dst);

			flowLayoutPanel_Programs.Controls.SetChildIndex(src, dstChildIndex);
			flowLayoutPanel_Programs.Controls.SetChildIndex(dst, srcChildIndex);
		}

		#endregion Program button Drag 'n' Drop

		#region Program buttons

		private void button_AddProgram_Click(object sender, EventArgs e)
		{
			using var dialog = new OpenFileDialog();
			dialog.Title = "Select the .exe file of the program you want to add.";
			dialog.Filter = "Executable Files|*.exe|Batch Files|*.bat";

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				bool alreadyExists = flowLayoutPanel_Programs.Controls.OfType<DarkButton>().Any(button
					=> button.Name.Equals(dialog.FileName, StringComparison.OrdinalIgnoreCase));

				if (alreadyExists)
				{
					DarkMessageBox.Show(this,
						"Program shortcut already exists.",
						"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					return;
				}

				AddProgramButton(dialog.FileName, true);
			}
		}

		private void menuItem_DeleteButton_Click(object sender, EventArgs e)
		{
			var source = (ToolStripMenuItem)sender;
			var owner = (DarkContextMenu)source.GetCurrentParent();
			var disposedButton = (DarkButton)owner.SourceControl;

			disposedButton.Dispose();

			SavePinnedPrograms();
		}

		private void AddProgramButton(string filePath, bool saveList)
		{
			if (!File.Exists(filePath))
				return;

			Image image = Icon.ExtractAssociatedIcon(filePath).ToBitmap();

			var button = new DarkButton
			{
				Name = filePath,
				Image = image,
				Size = new Size(42, 42)
			};

			button.Click += ProgramButton_Click;
			button.MouseDown += ProgramButton_MouseDown;
			button.MouseMove += ProgramButton_MouseMove;

			button.ContextMenuStrip = contextMenu_ProgramButton;

			string programName = FileVersionInfo.GetVersionInfo(filePath).ProductName;

			// Handle batch files and programs without ProductNames
			if (Path.GetExtension(filePath).Equals(".bat", StringComparison.OrdinalIgnoreCase))
				programName = Path.GetFileNameWithoutExtension(filePath) + " (Batch File)";
			else if (string.IsNullOrWhiteSpace(programName))
				programName = Path.GetFileNameWithoutExtension(filePath);

			toolTip.SetToolTip(button, programName);

			flowLayoutPanel_Programs.Controls.Add(button);
			button_AddProgram.SendToBack();

			if (saveList)
				SavePinnedPrograms();
		}

		private void SavePinnedPrograms()
		{
			_ide.IDEConfiguration.PinnedProgramPaths.Clear();

			IEnumerable<string> programPaths = flowLayoutPanel_Programs.Controls.OfType<DarkButton>().Select(x => x.Name);

			_ide.IDEConfiguration.PinnedProgramPaths.AddRange(programPaths);
			_ide.IDEConfiguration.Save();

			_ide.ProgramButtonsModified();
		}

		private void ProgramButton_Click(object sender, EventArgs e)
		{
			if (_isDragging)
				return;

			try
			{
				string programFilePath = ((Button)sender).Name;

				var startInfo = new ProcessStartInfo
				{
					FileName = programFilePath,
					WorkingDirectory = Path.GetDirectoryName(programFilePath),
					UseShellExecute = true
				};

				Process.Start(startInfo);
			}
			catch { }
		}

		#endregion Program buttons

		#region Events

		private void button_ExitProject_MouseUp(object sender, MouseEventArgs e)
			=> contextMenu_TIDE.Show(Cursor.Position);

		private void button_BackToStart_Click(object sender, EventArgs e)
			=> FindForm().DialogResult = DialogResult.OK;

		private void button_Publish_Click(object sender, EventArgs e)
		{
			using var form = new FormGameArchive(_ide);
			form.ShowDialog();
		}

		private void button_Update_Click(object sender, EventArgs e)
			=> _ide.BeginEngineUpdate();

		private void button_Exit_Click(object sender, EventArgs e)
			=> FindForm().DialogResult = DialogResult.Cancel;

		private void panelButton_LevelManager_Click(object sender, EventArgs e) => SelectIDETab(IDETab.LevelManager);
		private void panelButton_ScriptingStudio_Click(object sender, EventArgs e) => SelectIDETab(IDETab.ScriptingStudio);
		private void panelButton_PluginManager_Click(object sender, EventArgs e) => SelectIDETab(IDETab.PluginManager);
		private void panelButton_Miscellaneous_Click(object sender, EventArgs e) => SelectIDETab(IDETab.Miscellaneous);

		private void button_LaunchGame_Click(object sender, EventArgs e) => LaunchGame();
		private void button_OpenDirectory_Click(object sender, EventArgs e) => SharedMethods.OpenInExplorer(_ide.Project.DirectoryPath);

		private void Special_LaunchFLEP(object sender, EventArgs e) => LaunchFLEP();

		private void timer_ScriptButtonBlinking_Tick(object sender, EventArgs e)
		{
			int red = panelButton_ScriptingStudio.BackColor.R;
			int green = panelButton_ScriptingStudio.BackColor.G;
			int blue = panelButton_ScriptingStudio.BackColor.B;

			if (red > 48)
				red--;

			if (green > 48)
				green--;

			if (blue < 48)
				blue++;

			panelButton_ScriptingStudio.BackColor = Color.FromArgb(red, green, blue);

			if (red == 48 && green == 48 && blue == 48)
				timer_ScriptButtonBlinking.Stop();
		}

		#endregion Events

		#region Methods

		public void LaunchFLEP()
		{
			string engineRootDirectory = _ide.Project.GetEngineRootDirectoryPath();
			string flepExePath = Path.Combine(engineRootDirectory, "flep.exe");

			if (!File.Exists(flepExePath))
				return;

			try
			{
				var startInfo = new ProcessStartInfo
				{
					FileName = flepExePath,
					WorkingDirectory = engineRootDirectory,
					UseShellExecute = true
				};

				Process.Start(startInfo);
			}
			catch { }
		}

		public void LaunchGame()
		{
			string engineRootDirectory = _ide.Project.GetEngineRootDirectoryPath();

			if (_ide.Project.GameVersion is not TRVersion.Game.TR1 and not TRVersion.Game.TR2X and not TRVersion.Game.TombEngine)
			{
				string scriptDatFilePath = string.Empty;

				if (_ide.Project.GameVersion is TRVersion.Game.TR4 or TRVersion.Game.TRNG)
					scriptDatFilePath = Path.Combine(engineRootDirectory, "script.dat");
				else if (_ide.Project.GameVersion is TRVersion.Game.TR2 or TRVersion.Game.TR3)
					scriptDatFilePath = Path.Combine(engineRootDirectory, "data", "tombpc.dat");

				if (!File.Exists(scriptDatFilePath))
				{
					DarkMessageBox.Show(this,
						"Before launching the game, you must compile\n" +
						"your first script using the Scripting Studio.",
						"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

					SelectIDETab(IDETab.ScriptingStudio);
					return;
				}
			}

			bool supportsDebug = _ide.Project.GameVersion is TRVersion.Game.TombEngine;

			try
			{
				string launcherFilePath = _ide.Project.GetLauncherFilePath();

				var startInfo = new ProcessStartInfo
				{
					FileName = launcherFilePath,
					WorkingDirectory = Path.GetDirectoryName(launcherFilePath),
					Arguments = supportsDebug ? "-debug" : string.Empty,
					UseShellExecute = true
				};

				Process.Start(startInfo);
			}
			catch { }
		}

		public void SelectIDETab(IDETab tab)
		{
			var selectionColor = Color.FromArgb(128, 128, 128);
			var neutralColor = Color.FromArgb(48, 48, 48);

			switch (tab)
			{
				case IDETab.LevelManager:
					panelButton_LevelManager.BackColor = selectionColor;
					panelButton_ScriptingStudio.BackColor = neutralColor;
					panelButton_PluginManager.BackColor = neutralColor;
					panelButton_Miscellaneous.BackColor = neutralColor;
					break;

				case IDETab.ScriptingStudio:
					if (timer_ScriptButtonBlinking.Enabled)
						timer_ScriptButtonBlinking.Stop();

					panelButton_LevelManager.BackColor = neutralColor;
					panelButton_ScriptingStudio.BackColor = selectionColor;
					panelButton_PluginManager.BackColor = neutralColor;
					panelButton_Miscellaneous.BackColor = neutralColor;
					break;

				case IDETab.PluginManager:
					if (_ide.Project.GameVersion == TRVersion.Game.TRNG)
					{
						panelButton_LevelManager.BackColor = neutralColor;
						panelButton_ScriptingStudio.BackColor = neutralColor;
						panelButton_PluginManager.BackColor = selectionColor;
						panelButton_Miscellaneous.BackColor = neutralColor;
					}
					else if (_ide.Project.GameVersion == TRVersion.Game.TombEngine)
					{
						VSCodeUtils.OpenDirectoryInVSCode(FindForm(), _ide.IDEConfiguration, _ide.Project.GetScriptRootDirectory());
						return;
					}

					break;

				case IDETab.Miscellaneous:
					panelButton_LevelManager.BackColor = neutralColor;
					panelButton_ScriptingStudio.BackColor = neutralColor;
					panelButton_PluginManager.BackColor = neutralColor;
					panelButton_Miscellaneous.BackColor = selectionColor;
					break;
			}

			SelectedIDETabChanged?.Invoke(this, tab);
		}

		#endregion Methods
	}
}

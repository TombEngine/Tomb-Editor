using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Settings.ProjectInfo;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.ProjectMaster;

public partial class SettingsProjectInfo : UserControl
{
	private IDE _ide = null!;

	private readonly IProjectInfoService _projectInfoService;

	#region Initialization

	public SettingsProjectInfo() : this(new ProjectInfoService())
	{ }

	public SettingsProjectInfo(IProjectInfoService projectInfoService)
	{
		InitializeComponent();

		_projectInfoService = projectInfoService;
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;

		checkBox_FullPaths.Checked = _ide.IDEConfiguration.ViewFullFolderPaths;

		if (_ide.Project.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TombEngine)
		{
			button_ChangeScriptPath.Visible = false;
			textBox_ScriptPath.Width = 589;
		}

		UpdateProjectInfo();
	}

	#endregion Initialization

	#region Events

	private void checkBox_FullPaths_CheckedChanged(object sender, EventArgs e)
	{
		_ide.IDEConfiguration.ViewFullFolderPaths = checkBox_FullPaths.Checked;
		_ide.IDEConfiguration.Save();

		UpdateProjectInfo();
	}

	private void button_OpenProjectFolder_Click(object sender, EventArgs e)
		=> SharedMethods.OpenInExplorer(_ide.Project.DirectoryPath);

	private void button_OpenEngineFolder_Click(object sender, EventArgs e)
		=> SharedMethods.OpenInExplorer(_ide.Project.GetEngineRootDirectoryPath());

	private void button_OpenScriptFolder_Click(object sender, EventArgs e)
		=> SharedMethods.OpenInExplorer(_ide.Project.GetScriptRootDirectory());

	private void button_OpenLevelsFolder_Click(object sender, EventArgs e)
		=> SharedMethods.OpenInExplorer(_ide.Project.LevelsDirectoryPath);

	private void button_ChangeScriptPath_Click(object sender, EventArgs e)
	{
		using var dialog = new BrowseFolderDialog
		{
			Title = "Choose a new /Script/ folder for the project"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			try
			{
				if (!File.Exists(Path.Combine(dialog.Folder, "script.txt")))
					throw new ArgumentException("Selected /Script/ folder does not contain a Script.txt file.");

				_ide.ChangeScriptFolder(dialog.Folder);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void button_ChangeLevelsPath_Click(object sender, EventArgs e)
	{
		using var dialog = new BrowseFolderDialog
		{
			Title = "Choose a new /Levels/ folder for the project"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
			_ide.ChangeLevelsFolder(dialog.Folder);
	}

	#endregion Events

	#region Methods

	private void UpdateProjectInfo()
	{
		textBox_ProjectName.Text = _ide.Project.Name;
		textBox_ProjectPath.Text = _ide.Project.DirectoryPath;

		bool showFullPaths = checkBox_FullPaths.Checked;

		textBox_EnginePath.Text = _projectInfoService.FormatPathForDisplay(
			_ide.Project.GetEngineRootDirectoryPath(),
			_ide.Project.DirectoryPath,
			showFullPaths);

		textBox_ScriptPath.Text = _projectInfoService.GetScriptDirectoryDisplay(
			_ide.Project,
			showFullPaths);

		textBox_LevelsPath.Text = _projectInfoService.GetLevelsDirectoryDisplay(
			_ide.Project,
			showFullPaths);
	}

	#endregion Methods
}

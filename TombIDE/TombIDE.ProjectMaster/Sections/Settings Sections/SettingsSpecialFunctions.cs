using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Forms;
using TombIDE.ProjectMaster.Services.Settings.Launcher;
using TombIDE.ProjectMaster.Services.Settings.LogCleaning;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster;

public partial class SettingsSpecialFunctions : UserControl
{
	private IDE _ide = null!;

	private readonly ILauncherManagementService _launcherService;
	private readonly ILogCleaningService _logCleaningService;

	public SettingsSpecialFunctions()
		: this(new LauncherManagementService(), new LogCleaningService())
	{ }

	public SettingsSpecialFunctions(
		ILauncherManagementService launcherService,
		ILogCleaningService logCleaningService)
	{
		InitializeComponent();

		_launcherService = launcherService;
		_logCleaningService = logCleaningService;
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;

		if (_launcherService.CanRenameLauncher(_ide.Project))
		{
			textBox_LauncherName.Text = _launcherService.GetLauncherName(_ide.Project);
		}
		else
		{
			button_RenameLauncher.Enabled = false;

			textBox_LauncherName.ForeColor = Color.Gray;
			textBox_LauncherName.Text = "Unavailable for legacy projects";
		}
	}

	private void button_DeleteLogs_Click(object sender, EventArgs e)
	{
		try
		{
			int deletedCount = _logCleaningService.DeleteAllLogFiles(_ide.Project);

			if (deletedCount > 0)
			{
				DarkMessageBox.Show(this, $"Successfully deleted {deletedCount} log files\n" +
					"and error dumps from the project folder.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				DarkMessageBox.Show(this, "No log files or error dumps were found.", "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void button_RenameLauncher_Click(object sender, EventArgs e)
	{
		string launcherExecutable = _ide.Project.GetLauncherFilePath();

		if (!File.Exists(launcherExecutable))
		{
			DarkMessageBox.Show(this, "Couldn't find the launcher executable of the project.\n" +
				"Please restart TombIDE to resolve any issues.", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

			return;
		}

		using (var form = new FormRenameLauncher(_ide))
			form.ShowDialog(this);

		textBox_LauncherName.Text = _launcherService.GetLauncherName(_ide.Project);
	}

	private void button_BuildArchive_Click(object sender, EventArgs e)
	{
		using var form = new FormGameArchive(_ide);
		form.ShowDialog();
	}
}

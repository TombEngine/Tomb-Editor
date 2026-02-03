using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Settings.Launcher;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster;

public partial class FormRenameLauncher : DarkForm
{
	private readonly IDE _ide;

	private readonly ILauncherManagementService _launcherService;

	#region Initialization

	public FormRenameLauncher(IDE ide) : this(ide, new LauncherManagementService())
	{ }

	public FormRenameLauncher(IDE ide, ILauncherManagementService launcherService)
	{
		_ide = ide;
		_launcherService = launcherService;

		InitializeComponent();
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);

		textBox_NewName.Text = _launcherService.GetLauncherName(_ide.Project);
		textBox_NewName.SelectAll();
	}

	#endregion Initialization

	#region Events

	private void button_Apply_Click(object sender, EventArgs e)
	{
		try
		{
			string newName = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());

			if (string.IsNullOrWhiteSpace(newName))
				throw new ArgumentException("Invalid file name.");

			string currentName = _launcherService.GetLauncherName(_ide.Project);

			if (newName == currentName)
			{
				DialogResult = DialogResult.Cancel;
			}
			else
			{
				_launcherService.RenameLauncher(_ide.Project, newName);
				DialogResult = DialogResult.OK;
			}
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			DialogResult = DialogResult.None;
		}
	}

	#endregion Events
}

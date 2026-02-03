using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Settings.InGameLogo;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster;

public partial class SettingsLogo : UserControl
{
	private IDE _ide = null!;

	private readonly ILogoManagementService _logoService;

	#region Initialization

	public SettingsLogo() : this(new LogoManagementService())
	{ }

	public SettingsLogo(ILogoManagementService logoService)
	{
		InitializeComponent();

		_logoService = logoService;
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;

		UpdatePreview();
	}

	#endregion Initialization

	#region Events

	private void button_Change_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Filter = "All Supported Files|*.bmp;*.png|Bitmap Files|*.bmp|PNG Files|*.png"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			try
			{
				_logoService.ApplyLogoImage(_ide.Project, dialog.FileName);
				UpdatePreview();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void button_UseBlank_Click(object sender, EventArgs e)
	{
		DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to apply a blank image?", "Are you sure?",
			MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result == DialogResult.Yes)
		{
			try
			{
				_logoService.ApplyBlankLogo(_ide.Project);
				UpdatePreview();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void button_Reset_Click(object sender, EventArgs e)
	{
		DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default image?", "Are you sure?",
			MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result == DialogResult.Yes)
		{
			try
			{
				_logoService.RestoreDefaultLogo(_ide.Project);
				UpdatePreview();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	#endregion Events

	#region Methods

	private void UpdatePreview()
	{
		try
		{
			var logoImage = _logoService.GetLogoImage(_ide.Project);

			if (logoImage is not null)
			{
				panel_Preview.BackgroundImage = logoImage;
				label_Blank.Visible = _logoService.IsLogoBlank(_ide.Project);
			}
			else
			{
				panel_Preview.BackgroundImage = null;
				label_Blank.Visible = true;
			}
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	#endregion Methods
}

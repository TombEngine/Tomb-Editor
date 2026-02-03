using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Forms;
using TombIDE.ProjectMaster.Services.Settings.SplashScreen;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster;

public partial class SettingsSplashScreen : UserControl
{
	private IDE _ide = null!;

	private readonly ISplashScreenService _splashScreenService;

	#region Initialization

	public SettingsSplashScreen() : this(new SplashScreenService())
	{ }

	public SettingsSplashScreen(ISplashScreenService splashScreenService)
	{
		InitializeComponent();

		_splashScreenService = splashScreenService;
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;

		if (_splashScreenService.IsSupported(_ide.Project))
		{
			UpdatePreview();
		}
		else
		{
			label_NotSupported.Visible = true;

			button_Preview.Enabled = false;
			button_Change.Enabled = false;
			button_Remove.Enabled = false;
		}
	}

	#endregion Initialization

	#region Events

	private void button_Preview_Click(object sender, EventArgs e)
	{
		if (!_splashScreenService.IsPreviewSupported(_ide.Project))
		{
			DarkMessageBox.Show(this, "Project not supported.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		string launcherFilePath = _ide.Project.GetLauncherFilePath();

		var startInfo = new ProcessStartInfo
		{
			FileName = launcherFilePath,
			Arguments = "-p",
			WorkingDirectory = Path.GetDirectoryName(launcherFilePath),
			UseShellExecute = true
		};

		Process.Start(startInfo);
	}

	private void button_Change_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Filter = "Bitmap Files|*.bmp"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			try
			{
				_splashScreenService.ApplySplashScreenImage(_ide.Project, dialog.FileName);
				UpdatePreview();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void button_Remove_Click(object sender, EventArgs e)
	{
		DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to remove the splash screen image?", "Are you sure?",
			MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result == DialogResult.Yes)
		{
			try
			{
				_splashScreenService.RemoveSplashScreen(_ide.Project);
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
			var image = _splashScreenService.GetSplashScreenImage(_ide.Project);

			if (image is not null && _splashScreenService.IsValidResolution(image))
			{
				panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 460, 230);
				label_Blank.Visible = false;
				image.Dispose();
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

	private void button_AdjustFrame_Click(object sender, EventArgs e)
	{
		using var form = new FormAdjustFrame(_ide);
		form.ShowDialog(this);
	}
}

using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Settings.StartupImage;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster;

public partial class SettingsStartupImage : UserControl
{
	private IDE _ide = null!;

	private readonly IStartupImageService _startupImageService;

	#region Initialization

	public SettingsStartupImage() : this(new StartupImageService())
	{ }

	public SettingsStartupImage(IStartupImageService startupImageService)
	{
		InitializeComponent();

		_startupImageService = startupImageService;
	}

	public void Initialize(IDE ide)
	{
		_ide = ide;

		radioButton_Wide.Checked = !_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled;
		radioButton_Standard.Checked = _ide.IDEConfiguration.StandardAspectRatioPreviewEnabled;

		UpdatePreview();
	}

	#endregion Initialization

	#region Events

	private void radioButton_Wide_CheckedChanged(object sender, EventArgs e)
	{
		if (radioButton_Wide.Checked)
		{
			try
			{
				var image = _startupImageService.GetStartupImage(_ide.Project);

				if (image is not null)
				{
					panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 426, 240);
					image.Dispose();
				}

				_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled = false;
				_ide.IDEConfiguration.Save();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void radioButton_Standard_CheckedChanged(object sender, EventArgs e)
	{
		if (radioButton_Standard.Checked)
		{
			try
			{
				var image = _startupImageService.GetStartupImage(_ide.Project);

				if (image is not null)
				{
					panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 320, 240);
					image.Dispose();
				}

				_ide.IDEConfiguration.StandardAspectRatioPreviewEnabled = true;
				_ide.IDEConfiguration.Save();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
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
				_startupImageService.ApplyStartupImage(_ide.Project, dialog.FileName);
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
				_startupImageService.ApplyBlankImage(_ide.Project);
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
				_startupImageService.RestoreDefaultImage(_ide.Project);
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
			var image = _startupImageService.GetStartupImage(_ide.Project);

			if (image is not null)
			{
				if (radioButton_Wide.Checked)
					panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 426, 240);
				else if (radioButton_Standard.Checked)
					panel_Preview.BackgroundImage = ImageHandling.ResizeImage(image, 320, 240);

				label_Blank.Visible = _startupImageService.IsImageBlank(_ide.Project);
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
}

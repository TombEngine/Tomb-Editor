using DarkUI.Forms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.Settings.ExecutableIcon;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster;

public partial class SettingsIcon : UserControl
{
	private IDE _ide = null!;

	private readonly IIconManagementService _iconService;

	#region Initialization

	public SettingsIcon() : this(new IconManagementService())
	{ }

	public SettingsIcon(IIconManagementService iconService)
	{
		InitializeComponent();

		_iconService = iconService;
	}

	public async void Initialize(IDE ide)
	{
		_ide = ide;

		radioButton_Dark.Checked = !_ide.IDEConfiguration.LightModePreviewEnabled;
		radioButton_Light.Checked = _ide.IDEConfiguration.LightModePreviewEnabled;

		if (!_iconService.IsSupported(_ide.Project))
		{
			label_Unavailable.Visible = true;

			button_Change.Enabled = false;
			button_Reset.Enabled = false;
		}
		else
		{
			await UpdateIcons();
		}
	}

	#endregion Initialization

	#region Events

	private void radioButton_Dark_CheckedChanged(object sender, EventArgs e)
	{
		if (radioButton_Dark.Checked)
		{
			panel_PreviewBackground.BackColor = Color.FromArgb(48, 48, 48);

			_ide.IDEConfiguration.LightModePreviewEnabled = false;
			_ide.IDEConfiguration.Save();
		}
	}

	private void radioButton_Light_CheckedChanged(object sender, EventArgs e)
	{
		if (radioButton_Light.Checked)
		{
			panel_PreviewBackground.BackColor = Color.White;

			_ide.IDEConfiguration.LightModePreviewEnabled = true;
			_ide.IDEConfiguration.Save();
		}
	}

	private async void button_Change_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog
		{
			Title = "Choose the .ico file you want to inject into your game's .exe file",
			Filter = "Icon Files|*.ico"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			try
			{
				await _iconService.InjectIconAsync(_ide.Project, dialog.FileName);
				await UpdateIcons();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private async void button_Reset_Click(object sender, EventArgs e)
	{
		DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default icon?", "Are you sure?",
			MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		if (result == DialogResult.Yes)
		{
			try
			{
				await _iconService.RestoreDefaultIconAsync(_ide.Project);
				await UpdateIcons();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	#endregion Events

	#region Methods

	private async Task UpdateIcons()
	{
		try
		{
			var iconSet = await _iconService.ExtractIconPreviewsAsync(_ide.Project)
				?? throw new Exception("Failed to extract icon previews from the launcher executable.");

			if (iconSet.IsLowResolution)
			{
				panel_256.BorderStyle = BorderStyle.FixedSingle;
				panel_128.BorderStyle = BorderStyle.FixedSingle;

				panel_256.BackgroundImage = iconSet.Icon48;
				panel_128.BackgroundImage = iconSet.Icon48;
				panel_48.BackgroundImage = iconSet.Icon48;
				panel_16.BackgroundImage = iconSet.Icon16;
			}
			else
			{
				panel_256.BorderStyle = BorderStyle.None;
				panel_128.BorderStyle = BorderStyle.None;

				panel_256.BackgroundImage = iconSet.Icon256;
				panel_128.BackgroundImage = iconSet.Icon128;
				panel_48.BackgroundImage = iconSet.Icon48;
				panel_16.BackgroundImage = iconSet.Icon16;
			}
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	#endregion Methods
}

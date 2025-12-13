using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsIcon : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsIcon()
		{
			InitializeComponent();
		}

		public async void Initialize(IDE ide)
		{
			_ide = ide;

			radioButton_Dark.Checked = !_ide.IDEConfiguration.LightModePreviewEnabled;
			radioButton_Light.Checked = _ide.IDEConfiguration.LightModePreviewEnabled;

			if (_ide.Project.DirectoryPath.Equals(_ide.Project.GetEngineRootDirectoryPath(), StringComparison.OrdinalIgnoreCase))
			{
				label_Unavailable.Visible = true;

				button_Change.Enabled = false;
				button_Reset.Enabled = false;
			}
			else
				await UpdateIcons();
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
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Choose the .ico file you want to inject into your game's .exe file";
				dialog.Filter = "Icon Files|*.ico";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					await ApplyIconToExe(dialog.FileName);
			}
		}

		private async void button_Reset_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default icon?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				string icoFilePath = Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "Defaults", "Game Icons", _ide.Project.GameVersion + ".ico");
				await ApplyIconToExe(icoFilePath);
			}
		}

		#endregion Events

		#region Methods

		private async Task ApplyIconToExe(string iconPath)
		{
			try
			{
				IconUtilities.InjectIcon(_ide.Project.GetLauncherFilePath(), iconPath);
				await UpdateIcons();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async Task UpdateIcons()
		{
			string launcherFilePath = _ide.Project.GetLauncherFilePath();
			string launcherDirectoryPath = Path.GetDirectoryName(launcherFilePath);

			// Create a temporary .exe file to make sure the icon cache is up to date
			string tempFilePath = Path.Combine(launcherDirectoryPath, Path.GetRandomFileName() + ".exe");

			try
			{
				// Try copying the file with retry logic
				bool fileCopied = await FileUtils.TryCopyFileWithRetryAsync(launcherFilePath, tempFilePath);

				if (!fileCopied)
					return; // File copy failed, exit early

				var ico_256 = IconUtilities.ExtractIcon(tempFilePath, IconSize.Jumbo).ToBitmap();

				// Windows doesn't seem to have a name for 128x128 px icons, therefore we must resize the Jumbo one
				var ico_128 = ImageHandling.ResizeImage(ico_256, 128, 128) as Bitmap;

				var ico_48 = IconUtilities.ExtractIcon(tempFilePath, IconSize.ExtraLarge).ToBitmap();
				var ico_16 = IconUtilities.ExtractIcon(tempFilePath, IconSize.Small).ToBitmap();

				if (ico_256.Width == ico_48.Width && ico_256.Height == ico_48.Height)
				{
					panel_256.BorderStyle = BorderStyle.FixedSingle;
					panel_128.BorderStyle = BorderStyle.FixedSingle;

					panel_256.BackgroundImage = ico_48;
					panel_128.BackgroundImage = ico_48;
					panel_48.BackgroundImage = ico_48;
					panel_16.BackgroundImage = ico_16;
				}
				else
				{
					panel_256.BorderStyle = BorderStyle.None;
					panel_128.BorderStyle = BorderStyle.None;

					panel_256.BackgroundImage = ico_256;
					panel_128.BackgroundImage = ico_128;
					panel_48.BackgroundImage = ico_48;
					panel_16.BackgroundImage = ico_16;
				}
			}
			finally
			{
				// Now delete the temporary .exe file with retry logic
				await FileUtils.TryDeleteFileWithRetryAsync(tempFilePath);
			}
		}

		#endregion Methods
	}
}

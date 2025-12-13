using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombIDE
{
	public partial class FormFindMusic : DarkForm
	{
		public string MusicArchiveFilePath { get; set; }

		private string _downloadURL;

		public FormFindMusic(TRVersion.Game gameVersion)
		{
			InitializeComponent();

			_downloadURL = gameVersion switch  // Hardcoded links, yay!
			{
				TRVersion.Game.TR1 => "https://lostartefacts.dev/aux/tr1x/music.zip",
				TRVersion.Game.TR2X => "https://lostartefacts.dev/aux/tr2x/music.zip",
				_ => throw new NotSupportedException("Unsupported game version for music archive.")
			};
		}

		private void button_Download_Click(object sender, EventArgs e)
			=> Process.Start(new ProcessStartInfo()
			{
				FileName = _downloadURL,
				UseShellExecute = true
			});

		private void button_Browse_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the music.zip file.";
				dialog.Filter = "ZIP Files (.zip)|*.zip";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ArchivePath.Text = dialog.FileName;
			}
		}

		private void button_Continue_Click(object sender, EventArgs e)
		{
			try
			{
				string archivePath = textBox_ArchivePath.Text.Trim();

				if (!Path.GetExtension(archivePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
					throw new ArgumentException("Selected file is not a ZIP archive.");

				MusicArchiveFilePath = archivePath;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void textBox_ArchivePath_TextChanged(object sender, EventArgs e)
			=> button_Continue.Enabled = !string.IsNullOrWhiteSpace(textBox_ArchivePath.Text);
	}
}

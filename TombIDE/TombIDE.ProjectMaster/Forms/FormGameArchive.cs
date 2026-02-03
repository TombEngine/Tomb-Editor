using DarkUI.Forms;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.ArchiveCreation;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster.Forms;

public partial class FormGameArchive : DarkForm
{
	private readonly IDE _ide;

	private readonly IGameArchiveServiceFactory _archiveServiceFactory;

	public FormGameArchive(IDE ide) : this(ide, new GameArchiveServiceFactory())
	{ }

	public FormGameArchive(IDE ide, IGameArchiveServiceFactory archiveServiceFactory)
	{
		InitializeComponent();

		_ide = ide;
		_archiveServiceFactory = archiveServiceFactory;
	}

	private async void button_Generate_Click(object sender, EventArgs e)
	{
		using var dialog = new SaveFileDialog
		{
			InitialDirectory = _ide.Project.DirectoryPath,
			FileName = $"{_ide.Project.Name.Replace(' ', '_')}.zip",
			Filter = "Zip Archive (*.zip)|*.zip",
			DefaultExt = "zip"
		};

		if (dialog.ShowDialog(this) == DialogResult.OK)
			await CreateArchiveAsync(dialog.FileName);
	}

	private async Task CreateArchiveAsync(string fileName)
	{
		using var cts = new CancellationTokenSource();
		using var waitForm = new FormPleaseWait();

		waitForm.CancellationTokenSource = cts;
		waitForm.SetCancelButtonVisible(true);
		waitForm.UpdateProgressAndStatus(0, "Initializing archive creation...");
		waitForm.Show(this);

		var archiveService = _archiveServiceFactory.GetArchiveService(_ide.Project.GameVersion);

		// Define the event handler so we can unsubscribe later
		void progressHandler(object? sender, ArchiveProgressEventArgs e)
			=> waitForm.UpdateProgressAndStatus(e.PercentComplete, e.CurrentOperation);

		try
		{
			// Wire up progress reporting
			archiveService.ProgressChanged += progressHandler;

			// Execute async archive creation with cancellation support
			await archiveService.CreateGameArchiveAsync(_ide.Project, fileName, richTextBox.Text, cts.Token);

			waitForm.Close();

			DarkMessageBox.Show(this, "Archive creation completed.", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);

			DialogResult = DialogResult.OK;
		}
		catch (OperationCanceledException)
		{
			waitForm.Close();

			// Delete partial archive if it exists
			if (File.Exists(fileName))
			{
				try
				{
					File.Delete(fileName);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}

			DarkMessageBox.Show(this, "Archive creation was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);

			DialogResult = DialogResult.None;
		}
		catch (Exception ex)
		{
			waitForm.Close();

			DarkMessageBox.Show(this, $"Archive creation failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			DialogResult = DialogResult.None;
		}
		finally
		{
			// Always unsubscribe to prevent memory leaks
			archiveService.ProgressChanged -= progressHandler;
			waitForm.CancellationTokenSource = null;
		}
	}
}

using DarkUI.Forms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace TombIDE.ProjectMaster.Forms;

public partial class FormPleaseWait : DarkForm
{
	private const int CP_NOCLOSE_BUTTON = 0x200;

	public CancellationTokenSource? CancellationTokenSource { get; set; }

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams cp = base.CreateParams;
			cp.ClassStyle |= CP_NOCLOSE_BUTTON;

			return cp;
		}
	}

	public FormPleaseWait()
		=> InitializeComponent();

	/// <summary>
	/// Updates the progress bar value (0-100).
	/// </summary>
	public void UpdateProgress(int percentComplete)
	{
		if (InvokeRequired)
		{
			Invoke(new Action<int>(UpdateProgress), percentComplete);
			return;
		}

		progressBar.Value = Math.Min(100, Math.Max(0, percentComplete));
	}

	/// <summary>
	/// Updates the status message displayed above the progress bar.
	/// </summary>
	public void UpdateStatus(string status)
	{
		if (InvokeRequired)
		{
			Invoke(new Action<string>(UpdateStatus), status);
			return;
		}

		labelStatus.Text = status;
	}

	/// <summary>
	/// Updates both progress and status in one call.
	/// </summary>
	public void UpdateProgressAndStatus(int percentComplete, string status)
	{
		if (InvokeRequired)
		{
			Invoke(new Action<int, string>(UpdateProgressAndStatus), percentComplete, status);
			return;
		}

		progressBar.Value = Math.Min(100, Math.Max(0, percentComplete));
		labelStatus.Text = status;
	}

	/// <summary>
	/// Sets whether the cancel button is visible.
	/// </summary>
	public void SetCancelButtonVisible(bool visible)
	{
		if (InvokeRequired)
		{
			Invoke(new Action<bool>(SetCancelButtonVisible), visible);
			return;
		}

		buttonCancel.Visible = visible;
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		CancellationTokenSource?.Cancel();

		buttonCancel.Enabled = false;
		buttonCancel.Text = "Cancelling...";
	}
}

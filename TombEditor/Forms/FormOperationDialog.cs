using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.Utils;
using TombLib;
using DarkUI.Extensions;
using DarkUI.Config;
using System.Threading.Tasks;

namespace TombEditor.Forms
{
	public partial class FormOperationDialog : DarkForm, IProgressReporter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private readonly Action<IProgressReporter, CancellationToken> _operation;
		private readonly bool _autoCloseWhenDone;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private Task _task;

		public FormOperationDialog(string operationName, bool autoCloseWhenDone, bool noProgressBar, Action<IProgressReporter, CancellationToken> operation)
		{
			_autoCloseWhenDone = autoCloseWhenDone;
			_operation = operation;
			InitializeComponent();

			Text = operationName;
			lstLog.SelectionHangingIndent = 50;

			this.SetActualSize();

			panelProgressBar.Visible = !noProgressBar;
			if (!noProgressBar && Application.OpenForms.Count > 0)
				TaskbarProgress.SetState(WinFormsUtils.TryGetMainWindowHandle(), TaskbarProgress.TaskbarStates.Normal);

			lstLog.BackColor = lstLog.BackColor.Multiply(Colors.Brightness);
			_cancellationTokenSource = new CancellationTokenSource();

		}

		private void FormOperationDialog_Shown(object sender, EventArgs e)
		{
			Run();
		}

		private void OnOperationFailure(Exception ex)
		{
			if (ex is not OperationCanceledException)
				TaskbarProgress.SetState(WinFormsUtils.TryGetMainWindowHandle(), TaskbarProgress.TaskbarStates.Error);
			else
				TaskbarProgress.SetState(WinFormsUtils.TryGetMainWindowHandle(), TaskbarProgress.TaskbarStates.NoProgress);

			TaskbarProgress.FlashWindow();
			pbStato.Value = 0;
			butCancel.Enabled = true;
			butOk.Enabled = false;
			lstLog.BackColor = lstLog.BackColor = Color.LightPink;

		}

		private void OnOperationSuccess()
		{
			TaskbarProgress.SetState(WinFormsUtils.TryGetMainWindowHandle(), TaskbarProgress.TaskbarStates.NoProgress);
			TaskbarProgress.FlashWindow();

			pbStato.Value = 100;

			butOk.Enabled = true;
			butCancel.Enabled = false;
			butOk.Focus();

			if (_autoCloseWhenDone)
			{
				DialogResult = DialogResult.OK;
				Close();
			}

			lstLog.BackColor = Color.LightGreen.Multiply(Colors.Brightness);
		}

		private void FormOperationDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Paranoid checks in case of system-specific null reference exceptions
			if (_task == null)
				return;

			// We shouldn't close the window when the task is still running
			if (_task.Status <= TaskStatus.Running)
			{
				// So we cancel the closing
				e.Cancel = true;

				// If the task isn't canceled yet, cancel it.
				if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
					EndThread();
			}

			TaskbarProgress.SetState(WinFormsUtils.TryGetMainWindowHandle(), TaskbarProgress.TaskbarStates.NoProgress);
		}

		private async void Run()
		{
			try
			{
				butCancel.Enabled = true;
				butOk.Enabled = false;
				_task = Task.Factory.StartNew(() =>
				{
					_operation(this, _cancellationTokenSource.Token);
					_cancellationTokenSource.Token.ThrowIfCancellationRequested();
				});

				// Syntax with await is much easier to handle because it propagates Exceptions nicely.
				await _task;

				// Done
				OnOperationSuccess();
			}
			catch (Exception ex)
			{
				OnOperationFailure(ex);
				logger.Error(ex, "Operation failed: " + Text);

				string message = "There was an error. Message: " + ex.Message;

				if (ex.InnerException != null)
					message += " : " + ex.InnerException?.Message;

				lstLog.SelectionBackColor = Color.Tomato;
				lstLog.AppendText(message + "\n");
				lstLog.ScrollToCaret();

#if DEBUG		// Keep Debug and Release the same, but in Debug we rethrow to get the proper exception and it's stacktrace.
				throw;
#endif
			}
		}

		void AddMessage(float? progress, string message, bool isWarning)
		{
			if (_cancellationTokenSource.IsCancellationRequested)
				return;

			BeginInvoke(() =>
			{
				if (progress.HasValue)
				{
					pbStato.Value = (int)Math.Round(MathC.Clamp(progress.Value, 0, 100), 0);
					TaskbarProgress.SetValue(WinFormsUtils.TryGetMainWindowHandle(), progress.Value, 100);
				}

				if (!string.IsNullOrEmpty(message))
				{
					lstLog.SelectionBackColor = isWarning ? Color.Yellow.Multiply(Colors.Brightness) : Color.Empty;
					lstLog.AppendText(message + "\n");
					lstLog.ScrollToCaret();
				}
			});
		}

		void IProgressReporter.ReportWarn(string message)
		{
			logger.Warn(message);
			AddMessage(null, message, true);

		}

		void IProgressReporter.ReportInfo(string message)
		{
			logger.Info(message);
			AddMessage(null, message, false);
		}

		void IProgressReporter.ReportProgress(float progress, string message)
		{
			logger.Info(progress + " - " + message);
			AddMessage(progress, message, false);
		}

		void IDialogHandler.RaiseDialog(IDialogDescription description)
		{
			GraphicalDialogHandler.HandleDialog(description, this);
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;

			if(_task.Status >= TaskStatus.RanToCompletion)
				Close();
			else 
				EndThread();
		}

		private void butOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void EndThread()
		{
			Invoke(() =>
			{
				_cancellationTokenSource.Cancel();
				DialogResult = DialogResult.Cancel;
				// Try shutting down the thread softly
				lstLog.SelectionBackColor = Color.Tomato;
				lstLog.AppendText("Stopping the process...\n");
				lstLog.ScrollToCaret();
			});

		}
	}
}

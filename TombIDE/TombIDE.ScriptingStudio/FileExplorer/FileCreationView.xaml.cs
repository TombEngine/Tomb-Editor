#nullable enable

using System;
using System.Windows;

namespace TombIDE.ScriptingStudio.FileExplorer;

/// <summary>
/// WPF window for the File Creation / Save As dialog.
/// Modal dialog that returns a file path result.
/// </summary>
public partial class FileCreationView : Window
{
	private readonly FileCreationViewModel _viewModel;
	private bool? _dialogResult;

	public FileCreationView(FileCreationViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		DataContext = viewModel;

		_viewModel.RequestAccept += OnRequestAccept;
		_viewModel.RequestCancel += OnRequestCancel;

		InitializeComponent();
	}

	/// <summary>
	/// Shows the dialog modally and returns the created file path, or null if cancelled.
	/// </summary>
	public string? ShowDialogAndGetResult()
	{
		_dialogResult = null;
		ShowDialog();
		return _dialogResult == true ? _viewModel.NewFilePath : null;
	}

	protected override void OnClosed(EventArgs e)
	{
		_viewModel.RequestAccept -= OnRequestAccept;
		_viewModel.RequestCancel -= OnRequestCancel;
		base.OnClosed(e);
	}

	private void OnRequestAccept()
	{
		_dialogResult = true;
		DialogResult = true;
		Close();
	}

	private void OnRequestCancel()
	{
		_dialogResult = false;
		DialogResult = false;
		Close();
	}
}

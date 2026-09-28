#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// WPF window for the Find and Replace dialog.
/// Follows the same hide-on-close pattern as the legacy WinForms dialog
/// to support modeless reuse across the workbench lifetime.
/// </summary>
public partial class FindAndReplaceView : Window
{
	private readonly FindAndReplaceViewModel _viewModel;
	private bool _isActuallyClosing;

	public FindAndReplaceView(FindAndReplaceViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		DataContext = viewModel;

		InitializeComponent();
	}

	/// <summary>
	/// Shows the dialog modelessly with the given owner handle and initial find text.
	/// </summary>
	public void Show(IntPtr ownerHandle, string initialFindText)
	{
		if (ownerHandle != IntPtr.Zero)
			new WindowInteropHelper(this).Owner = ownerHandle;

		_viewModel.Initialize(initialFindText);

		if (!IsVisible)
			Show();
		else
			Activate();
	}

	/// <summary>
	/// Hides the window instead of closing it, matching the legacy WinForms behavior.
	/// Call <see cref="ClosePermanently"/> to actually close the window.
	/// </summary>
	protected override void OnClosing(CancelEventArgs e)
	{
		if (!_isActuallyClosing)
		{
			Hide();
			e.Cancel = true;
		}

		base.OnClosing(e);
	}

	/// <summary>
	/// Closes the window permanently, bypassing the hide-on-close behavior.
	/// </summary>
	public void ClosePermanently()
	{
		_isActuallyClosing = true;
		Close();
	}
}

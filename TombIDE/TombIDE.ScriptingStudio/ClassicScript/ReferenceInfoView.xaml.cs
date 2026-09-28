#nullable enable

using System;
using System.ComponentModel;
using System.Windows;

namespace TombIDE.ScriptingStudio.ClassicScript;

/// <summary>
/// WPF window for the ClassicScript Reference Info dialog.
/// Follows the same hide-on-close pattern for modeless reuse.
/// </summary>
public partial class ReferenceInfoView : Window
{
	private readonly ReferenceInfoViewModel _viewModel;
	private bool _isActuallyClosing;

	public ReferenceInfoView(ReferenceInfoViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		DataContext = viewModel;

		_viewModel.RequestHide += OnRequestHide;

		InitializeComponent();
	}

	/// <summary>
	/// Shows the dialog and navigates to the given reference info.
	/// </summary>
	public void Show(string keyword, string description)
	{
		_viewModel.ApplySettings();

		if (!IsVisible)
			Show();

		_viewModel.ShowReferenceInfo(keyword, description);

		Activate();

		if (IsVisible)
			_viewModel.WasAlreadyOpened = true;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		if (!_isActuallyClosing)
		{
			_viewModel.OnHidden();
			Hide();
			e.Cancel = true;
		}

		base.OnClosing(e);
	}

	public void ClosePermanently()
	{
		_viewModel.RequestHide -= OnRequestHide;
		_isActuallyClosing = true;
		Close();
	}

	private void OnRequestHide()
	{
		Hide();
	}
}

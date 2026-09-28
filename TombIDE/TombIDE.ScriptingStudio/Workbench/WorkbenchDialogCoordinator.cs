#nullable enable

using System;
using System.ComponentModel;
using System.Windows.Forms;
using MvvmDialogs;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchDialogCoordinator : IDisposable
{
	private readonly IDialogService _dialogService;
	private readonly IWin32DialogOwnerProvider _dialogOwnerProvider;
	private readonly FindAndReplaceView _findAndReplaceView;
	private bool _disposed;

	public WorkbenchDialogCoordinator(
		IDialogService dialogService,
		IWin32DialogOwnerProvider dialogOwnerProvider,
		FindAndReplaceViewModel findAndReplaceViewModel)
	{
		ArgumentNullException.ThrowIfNull(dialogService);
		ArgumentNullException.ThrowIfNull(dialogOwnerProvider);
		ArgumentNullException.ThrowIfNull(findAndReplaceViewModel);

		_dialogService = dialogService;
		_dialogOwnerProvider = dialogOwnerProvider;
		_findAndReplaceView = new FindAndReplaceView(findAndReplaceViewModel);
	}

	public void ShowFindReplace(IEditorControl? currentEditor)
	{
		IWin32Window? owner = _dialogOwnerProvider.GetOwner() ?? Form.ActiveForm;

		_findAndReplaceView.Show(
			owner?.Handle ?? IntPtr.Zero,
			currentEditor?.SelectedContent?.ToString() ?? string.Empty);
	}

	public bool? ShowInputBox(InputBoxWindowViewModel inputBox)
	{
		ArgumentNullException.ThrowIfNull(inputBox);

		try
		{
			return _dialogService.ShowDialog(inputBox, inputBox);
		}
		catch (ViewNotRegisteredException)
		{
			var window = new InputBoxWindow { DataContext = inputBox };
			PropertyChangedEventHandler? propertyChangedHandler = null;
			propertyChangedHandler = (_, e) =>
			{
				if (e.PropertyName == nameof(InputBoxWindowViewModel.DialogResult) && inputBox.DialogResult.HasValue)
					window.DialogResult = inputBox.DialogResult;
			};

			inputBox.PropertyChanged += propertyChangedHandler;

			try
			{
				if (_dialogOwnerProvider.GetOwner() is { } owner)
					new System.Windows.Interop.WindowInteropHelper(window).Owner = owner.Handle;

				return window.ShowDialog();
			}
			finally
			{
				inputBox.PropertyChanged -= propertyChangedHandler;
			}
		}
	}

	public void ShowAbout()
	{
		using var form = new FormAbout(Resources.AboutScreen_800);
		IWin32Window? owner = _dialogOwnerProvider.GetOwner() ?? Form.ActiveForm;

		if (owner is not null)
			form.ShowDialog(owner);
		else
			form.ShowDialog();
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_findAndReplaceView.ClosePermanently();
	}
}

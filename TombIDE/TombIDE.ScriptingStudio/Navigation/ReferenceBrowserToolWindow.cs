#nullable enable

using System;
using System.Windows;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Navigation;

public sealed class ReferenceBrowserToolWindow : StudioDockPane
{
	private readonly ReferenceBrowserView _view;
	private readonly ReferenceBrowserViewModel _viewModel;

	public ReferenceBrowserToolWindow(ReferenceBrowserViewModel viewModel)
		: base(Strings.Default.ReferenceBrowser, "ReferenceBrowser", StudioDockPaneLocation.Bottom, new Size(520, 240))
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		_view = new ReferenceBrowserView
		{
			DataContext = _viewModel
		};

		_viewModel.ReferenceDefinitionRequested += ViewModel_ReferenceDefinitionRequested;
	}

	public event TombIDE.ScriptingStudio.ReferenceDefinitionRequestedEventHandler? ReferenceDefinitionRequested;

	public override UIElement Content => _view;

	public override void Dispose()
	{
		_viewModel.ReferenceDefinitionRequested -= ViewModel_ReferenceDefinitionRequested;
		ReferenceDefinitionRequested = null;
	}

	private void ViewModel_ReferenceDefinitionRequested(object? sender, ReferenceDefinitionEventArgs e)
		=> ReferenceDefinitionRequested?.Invoke(this, e);
}

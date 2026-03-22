using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.LightingPanel;

public partial class LightingView : UserControl
{
	private readonly LightingViewModel _viewModel;

	public LightingView()
	{
		InitializeComponent();
		_viewModel = new LightingViewModel(Editor.Instance);
		DataContext = _viewModel;
	}

	public void Cleanup()
	{
		_viewModel.Cleanup();
	}
}

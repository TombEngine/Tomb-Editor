using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.RoomOptionsPanel;

public partial class RoomOptionsView : UserControl
{
	private readonly RoomOptionsViewModel _viewModel;

	public RoomOptionsView()
	{
		InitializeComponent();
		_viewModel = new RoomOptionsViewModel(Editor.Instance);
		DataContext = _viewModel;
	}

	public void Cleanup()
	{
		_viewModel.Cleanup();
	}
}

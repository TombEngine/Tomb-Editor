using System.Windows.Controls;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

public partial class RoomOptions : UserControl
{
	private readonly RoomOptionsViewModel _viewModel;

	public RoomOptions()
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

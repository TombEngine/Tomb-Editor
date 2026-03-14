using System.Windows.Controls;
using TombEditor.ViewModels.ToolWindows;

namespace TombEditor.Views.ToolWindows;

public partial class Lighting : UserControl
{
	private readonly LightingViewModel _viewModel;

	public Lighting()
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

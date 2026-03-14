using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

public partial class Lighting : System.Windows.Controls.UserControl
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

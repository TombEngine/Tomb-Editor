#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.StatusBar;

public partial class StatusBarView : UserControl
{
    private readonly StatusBarViewModel _viewModel;

    public StatusBarView()
    {
        InitializeComponent();
        _viewModel = new StatusBarViewModel(Editor.Instance);
        DataContext = _viewModel;
    }

    public void Cleanup() => _viewModel.Cleanup();
}

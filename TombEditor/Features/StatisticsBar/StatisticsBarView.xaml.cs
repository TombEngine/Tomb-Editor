#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.StatisticsBar;

public partial class StatisticsBarView : UserControl
{
    private readonly StatisticsBarViewModel _viewModel;

    public StatisticsBarView()
    {
        InitializeComponent();
        _viewModel = new StatisticsBarViewModel(Editor.Instance);
        DataContext = _viewModel;
    }

    public void Cleanup() => _viewModel.Cleanup();
}

#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.RoomsPanel;

public partial class RoomsView : UserControl
{
    private readonly RoomsViewModel _viewModel;

    public RoomsView()
    {
        InitializeComponent();
        _viewModel = new RoomsViewModel(Editor.Instance);
        DataContext = _viewModel;
    }

    public void Cleanup() => _viewModel.Cleanup();

    private void RoomsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoomsList.SelectedItem is { } selected)
            RoomsList.ScrollIntoView(selected);
    }
}

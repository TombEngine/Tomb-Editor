#nullable enable

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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

    public void FocusRoomSearch() => RoomSearchComboBox.OpenAndFocusSearch();

    public void Cleanup() => _viewModel.Cleanup();
}

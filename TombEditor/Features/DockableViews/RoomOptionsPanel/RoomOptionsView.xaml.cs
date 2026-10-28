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

    public void FocusRoomSearch()
    {
        Dispatcher.BeginInvoke(() => {
            Focus();
            RoomSearchComboBox.IsDropDownOpen = true;
            RoomSearchComboBox.ApplyTemplate();
            RoomSearchComboBox.UpdateLayout();

            Dispatcher.BeginInvoke(() => {
                if (RoomSearchComboBox.SearchTextBox is { } searchTextBox)
                {
                    FocusManager.SetFocusedElement(this, searchTextBox);
                    Keyboard.Focus(searchTextBox);
                    searchTextBox.Focus();
                    searchTextBox.SelectAll();
                }
            }, DispatcherPriority.Input);
        }, DispatcherPriority.Input);
    }

    public void Cleanup() => _viewModel.Cleanup();
}

#nullable enable

using System.Windows.Controls;
using System.Windows.Input;

namespace TombEditor.Features.DockableViews.TriggerList;

public partial class TriggerListView : UserControl
{
    private readonly TriggerListViewModel _viewModel;

    public TriggerListView()
    {
        InitializeComponent();
        _viewModel = new TriggerListViewModel(Editor.Instance);
        DataContext = _viewModel;
    }

    public void Cleanup() => _viewModel.Cleanup();

    private void TriggersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Double-click on a row -> open the trigger editor, mirroring the previous panel behaviour.
        if (_viewModel.EditTriggerCommand.CanExecute(null))
            _viewModel.EditTriggerCommand.Execute(null);
    }
}

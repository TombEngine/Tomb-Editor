using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ToolBarLayout;

public partial class ToolBarLayoutWindow : Window
{
    public ToolBarLayoutWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }

    // Like the previous dialog, double-clicking a button moves it to the other list.
    private void AvailableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        => ExecuteForClickedItem(sender, e, (viewModel) => viewModel.AddCommand);

    private void CurrentList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        => ExecuteForClickedItem(sender, e, (viewModel) => viewModel.RemoveCommand);

    private void ExecuteForClickedItem(object sender, MouseButtonEventArgs e, System.Func<ToolBarLayoutWindowViewModel, ICommand> command)
    {
        if (DataContext is not ToolBarLayoutWindowViewModel viewModel || sender is not ListBox list)
            return;

        // Only react to double-clicks on an item, not on the empty area of the list.
        if (list.InputHitTest(e.GetPosition(list)) is not DependencyObject hit || ItemsControl.ContainerFromElement(list, hit) is null)
            return;

        var action = command(viewModel);
        if (action.CanExecute(null))
            action.Execute(null);
    }
}
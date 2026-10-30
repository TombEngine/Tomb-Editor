#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
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
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>Gets the panel title (the panel is called Classic Triggers for TombEngine levels).</summary>
    public string Title => _viewModel.Title;

    /// <summary>Raised when the panel title changes.</summary>
    public event EventHandler? TitleChanged;

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TriggerListViewModel.Title))
            TitleChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TriggersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedItems = [.. TriggersList.SelectedItems.OfType<TriggerEntry>()];
        _viewModel.NotifySelectionChanged();
    }

    private void TriggersList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Delete or Key.Back) || !_viewModel.DeleteTriggersCommand.CanExecute(null))
            return;

        _viewModel.DeleteTriggersCommand.Execute(null);
        e.Handled = true;
    }

    public void Cleanup() => _viewModel.Cleanup();

    private void TriggersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Double-click on a row -> open the trigger editor, mirroring the previous panel behaviour.
        if (_viewModel.EditTriggerCommand.CanExecute(null))
            _viewModel.EditTriggerCommand.Execute(null);
    }
}

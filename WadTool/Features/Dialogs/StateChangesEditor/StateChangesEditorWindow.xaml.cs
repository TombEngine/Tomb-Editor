#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.WPF;
using WadTool.Features.Dialogs.BlendCurveEditor;

namespace WadTool.Features.Dialogs.StateChangesEditor;

public partial class StateChangesEditorWindow : Window
{
    private StateChangesEditorWindowViewModel? _vm;

    public StateChangesEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Closed += (_, _) => _vm?.Detach();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.BlendCurveEditRequested -= OnBlendCurveEditRequested;
            _vm.RowAdded -= OnRowAdded;
        }

        _vm = e.NewValue as StateChangesEditorWindowViewModel;
        if (_vm is null)
            return;

        _vm.BlendCurveEditRequested += OnBlendCurveEditRequested;
        _vm.RowAdded += OnRowAdded;

        // TEN-only columns, like the legacy form.
        var tenColumnVisibility = _vm.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
        columnNextHighFrame.Visibility = tenColumnVisibility;
        columnBlendFrames.Visibility = tenColumnVisibility;
        columnBlendCurve.Visibility = tenColumnVisibility;
    }

    // Legacy CellMouseDoubleClick: play the state change on a cell double click, except in the blend curve column.
    private void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_vm is null)
            return;

        var source = e.OriginalSource as DependencyObject;
        var cell = source as DataGridCell ?? source?.FindAncestor<DataGridCell>();
        if (cell is null || ReferenceEquals(cell.Column, columnBlendCurve))
            return;

        if (_vm.PlayStateChangeCommand.CanExecute(null))
            _vm.PlayStateChangeCommand.Execute(null);
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_vm is null)
            return;

        // Same persisted placement slots as the legacy form.
        WindowConfiguration.ConfigureWindow(this, _vm.Tool.Configuration, key: "FormStateChangesEditor");

        // Legacy OnShown: select and reveal the state change appended by the ctor.
        if (_vm.CreatedNew && dataGrid.Items.Count > 0)
        {
            object lastRow = dataGrid.Items[dataGrid.Items.Count - 1];
            dataGrid.SelectedItems.Clear();
            dataGrid.SelectedItem = lastRow;
            dataGrid.ScrollIntoView(lastRow);
        }
    }

    private void OnRowAdded(StateChangeRow row) => dataGrid.ScrollIntoView(row);

    private void OnBlendCurveEditRequested(StateChangeRow row)
    {
        // Legacy ShowBlendCurveEditor, now via the already-migrated WPF BlendCurveEditor dialog.
        var viewModel = new BlendCurveEditorWindowViewModel(row.BlendCurve);
        var dialog = new BlendCurveEditorWindow { DataContext = viewModel, Owner = this };
        dialog.ShowDialog();

        if (viewModel.DialogResult == true)
            _vm?.ApplyBlendCurve(row, viewModel.ResultCurve);
    }
}

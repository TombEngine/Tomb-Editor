#nullable enable

using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ResizeRoom;

public partial class ResizeRoomWindow : Window
{
    public ResizeRoomWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        DataContextChanged += OnDataContextChanged;
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is ResizeRoomWindowViewModel vm)
        {
            vm.AreaChanged -= OnAreaChanged;
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ResizeRoomWindowViewModel oldVm)
        {
            oldVm.AreaChanged -= OnAreaChanged;
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is ResizeRoomWindowViewModel newVm)
        {
            gridControl.Room = newVm.Room;
            gridControl.ColorScheme = newVm.Editor.Configuration.UI_ColorScheme;
            SyncFromViewModel(newVm);
            gridControl.InvalidateVisual();

            newVm.AreaChanged += OnAreaChanged;
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnAreaChanged()
    {
        if (DataContext is ResizeRoomWindowViewModel vm)
        {
            SyncFromViewModel(vm);
            gridControl.InvalidateVisual();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResizeRoomWindowViewModel.UseFloor))
            OnAreaChanged();
    }

    private void SyncFromViewModel(ResizeRoomWindowViewModel vm)
    {
        gridControl.NewArea = vm.NewArea;
        gridControl.UseFloor = vm.UseFloor;
    }
}

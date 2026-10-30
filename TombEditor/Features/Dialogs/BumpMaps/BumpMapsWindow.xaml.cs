#nullable enable

using System;
using System.Windows;
using System.Windows.Threading;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.BumpMaps;

public partial class BumpMapsWindow : Window
{
    public BumpMapsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) =>
        {
            WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormBumpMaps");

            // The map needs its final size to place the top of the texture at the top of the view.
            Dispatcher.BeginInvoke(new Action(ResetMap), DispatcherPriority.Loaded);
        };
        DataContextChanged += OnDataContextChanged;
    }

    private void ResetMap()
    {
        if (DataContext is BumpMapsWindowViewModel vm)
            mapView.ResetVisibleTexture(vm.SelectedTexture);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BumpMapsWindowViewModel old)
        {
            old.RequestResetVisibleTexture -= OnResetVisible;
            old.RequestInvalidate -= OnInvalidate;
            old.MapSelectionProvider = null;
        }

        if (e.NewValue is BumpMapsWindowViewModel vm)
        {
            vm.RequestResetVisibleTexture += OnResetVisible;
            vm.RequestInvalidate += OnInvalidate;
            vm.MapSelectionProvider = () => mapView.SelectedTexture;
            mapView.ResetVisibleTexture(vm.SelectedTexture);
        }
    }

    private void CustomPathText_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Like the previous dialog, clicking the path label toggles the custom file too.
        if (DataContext is BumpMapsWindowViewModel { HasSelectedTexture: true } vm && vm.ToggleCustomFileCommand.CanExecute(null))
            vm.ToggleCustomFileCommand.Execute(null);
    }

    private void OnResetVisible(object? sender, TombLib.LevelData.LevelTexture? texture)
        => mapView.ResetVisibleTexture(texture);

    private void OnInvalidate(object? sender, System.EventArgs e)
        => mapView.InvalidateVisual();
}

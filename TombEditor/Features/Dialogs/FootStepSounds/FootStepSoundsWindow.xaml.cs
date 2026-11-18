#nullable enable

using System;
using System.Windows;
using System.Windows.Threading;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FootStepSounds;

public partial class FootStepSoundsWindow : Window
{
    public FootStepSoundsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) =>
        {
            WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormFootStepSounds");

            // The map needs its final size to place the top of the texture at the top of the view.
            Dispatcher.BeginInvoke(new Action(ResetMap), DispatcherPriority.Loaded);
        };
        DataContextChanged += OnDataContextChanged;
    }

    private void ResetMap()
    {
        if (DataContext is FootStepSoundsWindowViewModel vm)
            mapView.ResetVisibleTexture(vm.SelectedTexture);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is FootStepSoundsWindowViewModel old)
        {
            old.RequestResetVisibleTexture -= OnResetVisible;
            old.RequestInvalidate -= OnInvalidate;
            old.MapSelectionProvider = null;
        }

        if (e.NewValue is FootStepSoundsWindowViewModel vm)
        {
            vm.RequestResetVisibleTexture += OnResetVisible;
            vm.RequestInvalidate += OnInvalidate;
            vm.MapSelectionProvider = () => mapView.SelectedTexture;
            mapView.ResetVisibleTexture(vm.SelectedTexture);
        }
    }

    private void OnResetVisible(object? sender, TombLib.LevelData.LevelTexture? texture)
    {
        mapView.ResetVisibleTexture(texture);

        // The previous form cleared the selection when the texture changed.
        mapView.SelectedTexture = TombLib.Utils.TextureArea.None;
    }

    private void OnInvalidate(object? sender, System.EventArgs e)
        => mapView.InvalidateVisual();
}

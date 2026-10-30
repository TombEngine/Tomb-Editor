using System;
#nullable enable

using System.Windows;
using System.Windows.Threading;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.TextureRemap;

public partial class TextureRemapWindow : Window
{
    public TextureRemapWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) =>
        {
            WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormTextureRemap");

            // The maps need their final size to place the top of the texture at the top of the view.
            Dispatcher.BeginInvoke(new Action(ResetMaps), DispatcherPriority.Loaded);
        };
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TextureRemapWindowViewModel old)
        {
            old.RemapRectanglesChanged -= OnRectanglesChanged;
            old.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is TextureRemapWindowViewModel vm)
        {
            SourceMapView.DataContext = vm;
            DestinationMapView.DataContext = vm;
            SourceMapView.ResetVisibleTexture(vm.SourceTexture);
            DestinationMapView.ResetVisibleTexture(vm.DestinationTexture);

            vm.RemapRectanglesChanged += OnRectanglesChanged;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void ResetMaps()
    {
        if (DataContext is not TextureRemapWindowViewModel vm)
            return;

        SourceMapView.ResetVisibleTexture(vm.SourceTexture);
        DestinationMapView.ResetVisibleTexture(vm.DestinationTexture);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not TextureRemapWindowViewModel vm)
            return;

        if (e.PropertyName == nameof(vm.SourceTexture))
            SourceMapView.ResetVisibleTexture(vm.SourceTexture);
        else if (e.PropertyName == nameof(vm.DestinationTexture))
            DestinationMapView.ResetVisibleTexture(vm.DestinationTexture);
    }

    private void OnRectanglesChanged(object? sender, System.EventArgs e)
    {
        SourceMapView.InvalidateVisual();
        DestinationMapView.InvalidateVisual();
    }
}

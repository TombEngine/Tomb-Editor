#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FootStepSounds;

public partial class FootStepSoundsWindow : Window
{
    public FootStepSoundsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormFootStepSounds");
        DataContextChanged += OnDataContextChanged;
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
        => mapView.ResetVisibleTexture(texture);

    private void OnInvalidate(object? sender, System.EventArgs e)
        => mapView.InvalidateVisual();
}

#nullable enable

using System.Windows;
using System.Windows.Interop;
using TombLib.LevelData;

namespace TombEditor.Features.Dialogs.TexturePreview;

/// <summary>
/// Borderless pop-up that previews a <see cref="LevelTexture"/>. Closes when it
/// loses focus, mirroring the previous <c>FormPreviewTexture</c> behaviour.
/// </summary>
public partial class TexturePreviewWindow : Window
{
    public TexturePreviewWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Deactivated += (_, _) => Close();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TexturePreviewWindowViewModel old)
            old.PropertyChanged -= OnViewModelPropertyChanged;

        if (e.NewValue is TexturePreviewWindowViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            MapView.VisibleTexture = vm.VisibleTexture;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TexturePreviewWindowViewModel.VisibleTexture)
            && sender is TexturePreviewWindowViewModel vm)
            MapView.VisibleTexture = vm.VisibleTexture;
    }

    /// <summary>Convenience helper for previous callers - sets the WPF owner from an HWND.</summary>
    public void SetOwnerFromHwnd(System.IntPtr hwnd)
    {
        if (hwnd == System.IntPtr.Zero)
            return;

        new WindowInteropHelper(this).Owner = hwnd;
    }
}

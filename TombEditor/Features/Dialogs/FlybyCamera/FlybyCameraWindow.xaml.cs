#nullable enable

using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FlybyCamera;

public partial class FlybyCameraWindow : Window
{
    public FlybyCameraWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        Loaded += OnLoaded;
        Closing += OnClosing;
        Closed += OnClosedHandler;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormFlybyCamera");

        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.BeginPreview();
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.OnWindowClosing();
    }

    private void OnClosedHandler(object sender, System.EventArgs e)
    {
        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.EndPreview();
    }
}

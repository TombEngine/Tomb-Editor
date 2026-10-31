#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Transform;

public partial class TransformWindow : Window
{
    public TransformWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Closing += (_, _) => (DataContext as TransformWindowViewModel)?.OnClosing();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormTransform");
    }
}

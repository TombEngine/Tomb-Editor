#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Camera;

public partial class CameraWindow : Window
{
    public CameraWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

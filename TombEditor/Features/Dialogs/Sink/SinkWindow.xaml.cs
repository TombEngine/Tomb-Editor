#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Sink;

public partial class SinkWindow : Window
{
    public SinkWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

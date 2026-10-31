#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Portal;

public partial class PortalWindow : Window
{
    public PortalWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

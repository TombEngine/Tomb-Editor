#nullable enable

using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.Options;

public partial class OptionsWindow : Window
{
    public OptionsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

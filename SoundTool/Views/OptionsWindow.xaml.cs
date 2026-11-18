#nullable enable

using System.Windows;
using TombLib.WPF;

namespace SoundTool.Views;

public partial class OptionsWindow : Window
{
    public OptionsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

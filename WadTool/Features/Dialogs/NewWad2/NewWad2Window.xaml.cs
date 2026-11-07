#nullable enable

using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.NewWad2;

public partial class NewWad2Window : Window
{
    public NewWad2Window()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

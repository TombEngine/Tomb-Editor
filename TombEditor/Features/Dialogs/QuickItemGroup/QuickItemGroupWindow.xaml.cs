#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.QuickItemGroup;

public partial class QuickItemGroupWindow : Window
{
    public QuickItemGroupWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

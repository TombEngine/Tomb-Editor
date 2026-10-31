#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ImportPrj;

public partial class ImportPrjWindow : Window
{
    public ImportPrjWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

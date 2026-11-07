using System.Windows;

namespace TombLib.WPF.Features.ColorPicker;

public partial class ColorPickerWindow : Window
{
    public ColorPickerWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

using System.Windows;
using System.Windows.Input;
using TombLib.Input;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.KeyboardLayout;

public partial class KeyboardLayoutWindow : Window
{
    public KeyboardLayoutWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        PreviewKeyDown += OnPreviewKeyDown;
        PreviewKeyUp += OnPreviewKeyUp;
    }

    private KeyboardLayoutWindowViewModel? Vm => DataContext as KeyboardLayoutWindowViewModel;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Vm is not { IsListening: true } vm)
        {
            if (e.Key == Key.Delete && Vm?.SelectedRow is { } row)
                Vm?.DeleteHotkeysCommand.Execute(row);
            return;
        }

        var winFormsKey = (EditorKey)KeyInterop.VirtualKeyFromKey(e.Key);
        var modifiers = EditorKey.None;

        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            modifiers |= EditorKey.Control;

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            modifiers |= EditorKey.Shift;

        if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            modifiers |= EditorKey.Alt;

        vm.OnListenKeyDown(winFormsKey | modifiers);
        e.Handled = true;
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (Vm is { IsListening: true } vm)
        {
            vm.OnListenKeyUp();
            e.Handled = true;
        }
    }
}

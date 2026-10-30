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

    // A found or newly selected row is scrolled into view, like the previous grid did.
    private void HotkeysGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.DataGrid { SelectedItem: { } selected } grid)
            grid.ScrollIntoView(selected);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Vm is not { IsListening: true } vm)
        {
            if (e.Key == Key.Delete && Vm?.SelectedRow is { } row)
                Vm?.DeleteHotkeysCommand.Execute(row);
            return;
        }

        // With Alt held WPF reports Key.System and carries the real key in SystemKey (likewise for IME and dead keys).
        var pressedKey = e.Key switch
        {
            Key.System => e.SystemKey,
            Key.ImeProcessed => e.ImeProcessedKey,
            Key.DeadCharProcessed => e.DeadCharProcessedKey,
            _ => e.Key
        };

        var winFormsKey = (EditorKey)KeyInterop.VirtualKeyFromKey(pressedKey);
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

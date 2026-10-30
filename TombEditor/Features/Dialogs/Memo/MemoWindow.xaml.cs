#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Memo;

public partial class MemoWindow : Window
{
    public MemoWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        // The multiline box consumes Enter, so Ctrl+Enter (save) is handled before it gets there.
        if (e.Key == System.Windows.Input.Key.Enter
            && System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control)
            && DataContext is MemoWindowViewModel viewModel
            && viewModel.ConfirmCommand.CanExecute(null))
        {
            viewModel.ConfirmCommand.Execute(null);
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    // "FormMemo" key preserves existing user-config Window_FormMemo_Position/Size/Maximized.
    private void OnLoaded(object sender, RoutedEventArgs e)
        => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormMemo");
}

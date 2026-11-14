using System;
#nullable enable

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Moveable;

public partial class MoveableWindow : Window
{
    public MoveableWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) =>
        {
            WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormMoveable");

            // The previous form selected the OCB text when it opened.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ocbBox.Focus();
                ocbBox.SelectAll();
            }), DispatcherPriority.ApplicationIdle);
        };
    }

    // Like the previous dialog, the OCB field accepts digits, plus a minus sign only as the first character.
    private void OcbTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        string remaining = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);

        e.Handled = !e.Text.All(character => char.IsDigit(character)
            || character == '-' && textBox.SelectionStart == 0 && !remaining.Contains('-'));
    }
}
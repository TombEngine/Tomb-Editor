#nullable enable

using System.Windows;

namespace TombLib.WPF.Views;

public partial class InputBoxWindow : Window
{
    public InputBoxWindow()
    {
        InitializeComponent();
        Loaded += InputBoxWindow_Loaded;
    }

    private void InputBoxWindow_Loaded(object sender, RoutedEventArgs e)
    {
        textBox_Value.Focus();
        Dispatcher.BeginInvoke(textBox_Value.SelectAll);
    }
}

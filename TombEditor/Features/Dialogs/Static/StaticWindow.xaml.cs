#nullable enable

using System.Linq;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Static;

public partial class StaticWindow : Window
{
    public StaticWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormStatic");
    }

    // Like the previous dialog, the OCB field accepts digits only.
    private void OcbTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = !e.Text.All(char.IsDigit);
}

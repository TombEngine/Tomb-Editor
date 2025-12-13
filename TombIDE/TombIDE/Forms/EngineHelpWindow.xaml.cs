using System.Windows;

namespace TombIDE.Forms;

public partial class EngineHelpWindow : Window
{
	public EngineHelpWindow()
		=> InitializeComponent();

	private void Button_Click(object sender, RoutedEventArgs e)
		=> Close();
}

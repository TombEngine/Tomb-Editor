using TombEditor.WPF.ViewModels;
using TombEditor.WPF.Views;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ViewWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}
}
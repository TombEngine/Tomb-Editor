using DarkUI.Controls;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for TriggerList.xaml
/// </summary>
public partial class TriggerList : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.TriggerList WinFormsLayer { get; }

	public TriggerList()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.TriggerList();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		TreeViewHost.Child = new DarkListView();
	}
}

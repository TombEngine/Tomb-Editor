using DarkUI.Controls;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for ObjectList.xaml
/// </summary>
public partial class ObjectList : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.ObjectList WinFormsLayer { get; }

	public ObjectList()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.ObjectList();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		TreeViewHost.Child = new DarkListView();
	}
}

using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombEditor.Controls;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for ItemBrowser.xaml
/// </summary>
public partial class ItemBrowser : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.ItemBrowser WinFormsLayer { get; }

	public ItemBrowser()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.ItemBrowser();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		//ItemPreviewHost.Child = new PanelRenderingItem();
	}
}

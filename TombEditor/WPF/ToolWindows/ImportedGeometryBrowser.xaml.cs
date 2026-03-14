using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombEditor.Controls;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for ImportedGeometryBrowser.xaml
/// </summary>
public partial class ImportedGeometryBrowser : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.ImportedGeometryBrowser WinFormsLayer { get; }

	public ImportedGeometryBrowser()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.ImportedGeometryBrowser();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		//ItemPreviewHost.Child = new PanelRenderingImportedGeometry();
	}
}

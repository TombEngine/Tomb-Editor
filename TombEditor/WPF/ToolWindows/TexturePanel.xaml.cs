using TombEditor.Controls;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for TexturePanel.xaml
/// </summary>
public partial class TexturePanel : System.Windows.Controls.UserControl
{
	public PanelTextureMap WinFormsLayer { get; }

	public TexturePanel()
	{
		InitializeComponent();
		WinFormsLayer = new PanelTextureMap();

		ViewportHost.Child = WinFormsLayer;
	}
}

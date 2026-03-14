using DarkUI.Docking;

namespace TombEditor.Features.DockableViews.PalettePanel;

public partial class Palette : DarkToolWindow
{
	public Palette()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_paletteView?.Cleanup();
			components?.Dispose();
		}

		base.Dispose(disposing);
	}
}

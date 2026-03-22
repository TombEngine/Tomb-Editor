using DarkUI.Docking;

namespace TombEditor.Features.DockableViews.LightingPanel;

public partial class Lighting : DarkToolWindow
{
	public Lighting()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_lightingView?.Cleanup();
			components?.Dispose();
		}

		base.Dispose(disposing);
	}
}

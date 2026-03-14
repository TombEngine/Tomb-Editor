using DarkUI.Docking;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

public partial class SectorOptions : DarkToolWindow
{
	public SectorOptions()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_sectorOptionsView?.Cleanup();
			components?.Dispose();
		}

		base.Dispose(disposing);
	}
}

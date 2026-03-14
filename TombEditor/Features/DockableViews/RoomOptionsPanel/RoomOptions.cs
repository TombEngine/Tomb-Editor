using DarkUI.Docking;

namespace TombEditor.Features.DockableViews.RoomOptionsPanel;

public partial class RoomOptions : DarkToolWindow
{
	public RoomOptions()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_roomOptionsView?.Cleanup();
			components?.Dispose();
		}

		base.Dispose(disposing);
	}
}

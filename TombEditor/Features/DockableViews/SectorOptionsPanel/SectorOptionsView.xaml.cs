using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

public partial class SectorOptionsView : UserControl
{
	private readonly Editor _editor;
	private readonly SectorOptionsViewModel _viewModel;

	public SectorOptionsView()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_viewModel = new SectorOptionsViewModel(_editor);
		DataContext = _viewModel;

		_editor.EditorEventRaised += EditorEventRaised;
		Panel2DGridControl.Room = _editor.SelectedRoom;
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= EditorEventRaised;
		_viewModel.Cleanup();
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.SelectedRoomChangedEvent selectedRoomChangedEvent)
			Panel2DGridControl.Room = selectedRoomChangedEvent.Current;
	}
}

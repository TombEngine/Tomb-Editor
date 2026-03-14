using System.Windows.Controls;
using TombEditor.ViewModels.ToolWindows;

namespace TombEditor.Views.ToolWindows;

public partial class SectorOptions : UserControl
{
	private readonly Editor _editor;
	private readonly SectorOptionsViewModel _viewModel;

	public SectorOptions()
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

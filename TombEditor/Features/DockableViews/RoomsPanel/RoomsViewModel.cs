#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TombLib.LevelData;

namespace TombEditor.Features.DockableViews.RoomsPanel;

/// <summary>
/// Backs the rooms dockable panel: a flat list of every existing room in the level, with
/// two-way selection synchronised to <see cref="Editor.SelectedRoom"/>.
/// </summary>
public partial class RoomsViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;

    public ObservableCollection<Room> Rooms { get; } = new();

    private Room? _selectedRoom;
    public Room? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (!SetProperty(ref _selectedRoom, value))
                return;

            if (_suppressEditorSync || value is null)
                return;

            _editor.SelectRoom(value);
        }
    }

    public RoomsViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        RebuildRooms();
    }

    public void Cleanup()
    {
        if (_disposed)
            return;
        _disposed = true;
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.LevelChangedEvent || obj is Editor.RoomListChangedEvent)
            RebuildRooms();

        if (obj is Editor.SelectedRoomChangedEvent)
        {
            _suppressEditorSync = true;
            try
            {
                SelectedRoom = _editor.SelectedRoom;
            }
            finally
            {
                _suppressEditorSync = false;
            }
        }

        if (obj is Editor.RoomPropertiesChangedEvent propsChanged)
        {
            // Refresh the affected entry so a renamed room's label updates in place.
            int index = Rooms.IndexOf(propsChanged.Room);
            if (index >= 0)
            {
                Rooms.RemoveAt(index);
                Rooms.Insert(index, propsChanged.Room);
            }
        }
    }

    private void RebuildRooms()
    {
        if (_editor.Level is null)
        {
            Rooms.Clear();
            return;
        }

        _suppressEditorSync = true;
        try
        {
            Rooms.Clear();
            foreach (Room room in _editor.Level.ExistingRooms)
                Rooms.Add(room);

            SelectedRoom = _editor.SelectedRoom;
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }
}

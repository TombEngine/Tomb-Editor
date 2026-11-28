#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using TombLib.LevelData;

namespace TombEditor.Features.StatusBar;

/// <summary>
/// WPF port of the previous <c>FormMain</c> <c>DarkStatusStrip</c> (the status bar shown at the very
/// bottom of the window). Mirrors its four fields: the selected room summary, the world-space
/// selection area, the local selection size and the autosave status. Texts and formatting are kept
/// identical to <c>FormMain.EditorEventRaised</c> so behaviour matches the legacy editor.
/// </summary>
public partial class StatusBarViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _disposed;

    [ObservableProperty] private string _selectedRoomText = "Selected room: None";
    [ObservableProperty] private string _selectionAreaText = "Area: None";
    [ObservableProperty] private string _selectionSizeText = "Size: None";
    [ObservableProperty] private string _autosaveText = string.Empty;
    [ObservableProperty] private bool _autosaveFailed;

    public StatusBarViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        UpdateRoom();
        UpdateSelection();
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
        // Clear autosave information
        if (obj is Editor.LevelChangedEvent or Editor.LevelFileNameChangedEvent)
        {
            AutosaveText = string.Empty;
            AutosaveFailed = false;
        }

        // Update autosave status
        if (obj is Editor.AutosaveEvent autosave)
        {
            bool success = autosave.Exception == null;
            AutosaveText = success ? "Autosave OK: " + autosave.Time : "Autosave failed!";
            AutosaveFailed = !success;
        }

        // Update room information
        if (obj is Editor.InitEvent
            or Editor.LevelChangedEvent
            or Editor.SelectedRoomChangedEvent
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent)
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomPositionChangedEvent)
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent)
            || obj is Editor.RoomPropertiesChangedEvent)
        {
            UpdateRoom();
        }

        // Update selection information
        if (obj is Editor.SelectedRoomChangedEvent
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent)
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomPositionChangedEvent)
            || _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent)
            || obj is Editor.SelectedSectorsChangedEvent)
        {
            UpdateSelection();
        }
    }

    private void UpdateRoom()
    {
        var room = _editor.SelectedRoom;
        if (room == null)
        {
            SelectedRoomText = "Selected room: None";
            return;
        }

        float
            posY = room.Position.Y / (float)Level.FullClickHeight,
            lowestCorner = room.GetLowestCorner() / (float)Level.FullClickHeight,
            highestCorner = room.GetHighestCorner() / (float)Level.FullClickHeight;

        SelectedRoomText = "Selected room: " +
            "Name = " + room + " | " +
            "Size = " + (room.NumXSectors - 2) + " x " + (room.NumZSectors - 2) + " | " +
            "Pos = (" + room.Position.X + ", " + posY + ", " + room.Position.Z + ") | " +
            "Floor = " + (posY + lowestCorner) + " | " +
            "Ceiling = " + (posY + highestCorner);
    }

    private void UpdateSelection()
    {
        var room = _editor.SelectedRoom;
        if (room == null || !_editor.SelectedSectors.Valid)
        {
            SelectionAreaText = "Area: None";
            SelectionSizeText = "Size: None";
            return;
        }

        float
            posY = room.Position.Y / (float)Level.FullClickHeight,
            minHeight = room.GetLowestCorner(_editor.SelectedSectors.Area) / (float)Level.FullClickHeight,
            maxHeight = room.GetHighestCorner(_editor.SelectedSectors.Area) / (float)Level.FullClickHeight;

        SelectionAreaText = "Area = " +
            "(" + (room.Position.X + _editor.SelectedSectors.Area.X0) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y0) + ") \u2192 " +
            "(" + (room.Position.X + _editor.SelectedSectors.Area.X1) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y1) + ")" +
            " | y = [" + (minHeight == int.MaxValue || maxHeight == int.MinValue ? "N/A" : posY + minHeight + ", " + (posY + maxHeight)) + "]";

        SelectionSizeText = "Size = " + (1 + Math.Abs(_editor.SelectedSectors.Size.X)) +
            " x " + (1 + Math.Abs(_editor.SelectedSectors.Size.Y));
    }
}

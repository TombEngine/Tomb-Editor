#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

/// <summary>
/// View model for a single flyby camera entry in the camera list.
/// </summary>
public partial class FlybyCameraItemViewModel(FlybyCameraInstance camera) : ObservableObject
{
    /// <summary>
    /// Gets the underlying flyby camera instance represented by this item.
    /// </summary>
    public FlybyCameraInstance Camera { get; } = camera;

    /// <summary>
    /// Stores the display number of the camera entry.
    /// </summary>
    [ObservableProperty]
    private int _number = camera.Number;

    /// <summary>
    /// Stores the formatted timeline timecode shown for the camera.
    /// </summary>
    [ObservableProperty]
    private string _timecode = "00:00.00";

    /// <summary>
    /// Stores whether this camera item is currently selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Stores whether this camera shares its number with another camera.
    /// </summary>
    [ObservableProperty]
    private bool _isDuplicateIndex;

    /// <summary>
    /// Stores the room name shown for the camera entry.
    /// </summary>
    [ObservableProperty]
    private string _roomName = camera.Room?.ToString() ?? "NULL";
}

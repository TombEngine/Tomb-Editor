#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombLib.Wad.Catalog;

namespace TombEditor.Features.StatisticsBar;

/// <summary>A single coloured text fragment of the statistics bar (highlighted when a limit is exceeded).</summary>
public sealed record StatRun(string Text, bool Highlight);

/// <summary>
/// WPF port of the previous <c>MainView.UpdateStatistics</c> / <c>tbStats</c> bar.
/// Reads the editor's live <see cref="StatisticSummary"/> and rebuilds two coloured lines plus a
/// limit-warning tooltip whenever statistics, the level, version or selected room change.
/// </summary>
public partial class StatisticsBarViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _disposed;

    public ObservableCollection<StatRun> Runs { get; } = new();

    /// <summary>Mirrors the previous <c>comboStepHeight</c> items (indices map to 32/64/128/256).</summary>
    public IReadOnlyList<string> StepHeightOptions { get; } = new[] { "32 (Eighth)", "64 (Quarter)", "128 (Half)", "256 (Full)" };

    [ObservableProperty] private string? _limitWarning;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isStatsVisible;
    [ObservableProperty] private bool _isStepHeightVisible;
    [ObservableProperty] private int _stepHeightIndex;

    public StatisticsBarViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        UpdateVisibility();
        SyncStepHeight();
        Rebuild();
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
        // IsPreciseGeometryAllowed depends on the game version and a config flag, so visibility
        // must follow level/version changes too (mirrors MainView.UpdateBottomPanelVisibility).
        if (obj is Editor.ConfigurationChangedEvent
            or Editor.InitEvent
            or Editor.LevelChangedEvent
            or Editor.GameVersionChangedEvent)
        {
            UpdateVisibility();
        }

        if (obj is Editor.StepHeightChangedEvent
            or Editor.InitEvent
            or Editor.LevelChangedEvent)
        {
            SyncStepHeight();
        }

        if (obj is Editor.StatisticsChangedEvent
            or Editor.InitEvent
            or Editor.LevelChangedEvent
            or Editor.GameVersionChangedEvent
            or Editor.SelectedRoomChangedEvent
            or Editor.ConfigurationChangedEvent)
        {
            Rebuild();
        }
    }

    private void UpdateVisibility()
    {
        IsStatsVisible = _editor.Configuration.Window_Layout.ShowStats;
        IsStepHeightVisible = _editor.Level is not null && _editor.IsPreciseGeometryAllowed;
        IsVisible = IsStatsVisible || IsStepHeightVisible;
    }

    private void SyncStepHeight()
    {
        StepHeightIndex = _editor.Configuration.Editor_StepHeight switch
        {
            32 => 0,
            64 => 1,
            128 => 2,
            256 => 3,
            _ => -1
        };
    }

    partial void OnStepHeightIndexChanged(int value)
    {
        // Mirrors comboStepHeight_SelectedIndexChanged: the 3D panel reads IncrementReference
        // live from the configuration, so writing the value is enough (no event needed).
        // A non-standard configured step height maps to -1 and must not overwrite the configuration.
        if (value < 0)
            return;

        _editor.Configuration.Editor_StepHeight = value switch
        {
            0 => 32,
            1 => 64,
            2 => 128,
            _ => 256
        };
    }

    private void Rebuild()
    {
        Runs.Clear();

        if (_editor.Level is null)
        {
            LimitWarning = null;
            return;
        }

        StatisticSummary summary = _editor.Stats;
        var settings = _editor.Level.Settings;
        Statistics lStats = summary.LevelStats;
        Statistics rStats = summary.RoomStats;

        string warning = string.Empty;

        void AddWarning(string message)
            => warning = warning.Length == 0 ? message : warning + "\n" + message;

        int Limit(Limit limit) => TrCatalog.GetLimit(settings.GameVersion, limit);

        // Rooms

        bool roomsExceeded = false;
        if (_editor.Level.VerticallyConnectedRooms.Count > Limit(TombLib.Wad.Catalog.Limit.RoomSafeCount))
        {
            roomsExceeded = true;
            AddWarning("Vertically connected room count is exceeded.");
        }
        else if (_editor.Level.ExistingRooms.Count > Limit(TombLib.Wad.Catalog.Limit.RoomMaxCount))
        {
            roomsExceeded = true;
            AddWarning("Maximum room count is exceeded.");
        }

        Runs.Add(new StatRun("Rooms: ", false));
        Runs.Add(new StatRun(summary.RoomCount + "  ", roomsExceeded));

        // Objects

        Runs.Add(new StatRun("Objects: " + rStats.MoveableCount + " / ", false));

        bool objectsExceeded = false;
        if (lStats.MoveableCount > Limit(TombLib.Wad.Catalog.Limit.ItemSafeCount))
        {
            objectsExceeded = true;
            AddWarning("Safe object count is exceeded.");
        }
        else if (lStats.MoveableCount > Limit(TombLib.Wad.Catalog.Limit.ItemMaxCount))
        {
            objectsExceeded = true;
            AddWarning("Maximum object count is exceeded.");
        }

        Runs.Add(new StatRun(lStats.MoveableCount + "  ", objectsExceeded));

        // Statics / triggers

        Runs.Add(new StatRun("Statics: " + rStats.StaticCount + " / " + lStats.StaticCount + "  ", false));
        Runs.Add(new StatRun("Triggers: " + rStats.TriggerCount + " / " + lStats.TriggerCount + "  ", false));

        // Lights (dynamic only - static lights are not real lights)

        Runs.Add(new StatRun("Lights: ", false));

        bool lightsExceeded = rStats.DynLightCount > Limit(TombLib.Wad.Catalog.Limit.RoomLightCount);
        if (lightsExceeded)
            AddWarning("Maximum light count in current room is exceeded.");

        Runs.Add(new StatRun(rStats.DynLightCount.ToString(), lightsExceeded));
        Runs.Add(new StatRun(" / " + lStats.DynLightCount + "  ", false));

        // Misc

        Runs.Add(new StatRun("Cameras: " + rStats.CameraCount + " / " + lStats.CameraCount + "  ", false));
        Runs.Add(new StatRun("Flybys: " + rStats.FlybyCount + " / " + lStats.FlybyCount + "  ", false));

        // Room geometry

        Runs.Add(new StatRun("Room vertices / faces: ", false));

        bool verticesExceeded = rStats.VertexCount > Limit(TombLib.Wad.Catalog.Limit.RoomVertexCount);
        if (verticesExceeded)
            AddWarning("Room vertex count is exceeded.");
        Runs.Add(new StatRun(rStats.VertexCount + " / ", verticesExceeded));

        bool facesExceeded = rStats.FaceCount > Limit(TombLib.Wad.Catalog.Limit.RoomFaceCount);
        if (facesExceeded)
            AddWarning("Room face count is exceeded.");
        Runs.Add(new StatRun(rStats.FaceCount + "  ", facesExceeded));

        // Last level output (only known after a compile)

        Runs.Add(new StatRun("Last level output: ", false));

        if (summary.BoxCount.HasValue)
        {
            bool boxesExceeded = summary.BoxCount >= Limit(TombLib.Wad.Catalog.Limit.BoxLimit);
            if (boxesExceeded)
                AddWarning("Box count is exceeded. Reduce level complexity.");
            Runs.Add(new StatRun(summary.BoxCount.Value + " boxes, ", boxesExceeded));

            bool overlapsExceeded = summary.OverlapCount >= Limit(TombLib.Wad.Catalog.Limit.OverlapLimit) - 2;
            if (overlapsExceeded)
                AddWarning("Overlap count is exceeded. Reduce level complexity.");
            Runs.Add(new StatRun((summary.OverlapCount.HasValue ? summary.OverlapCount.Value.ToString() : "?") + " overlaps, ", overlapsExceeded));

            bool texInfosExceeded = summary.TextureCount > Limit(TombLib.Wad.Catalog.Limit.TexInfos);
            if (texInfosExceeded)
                AddWarning("TexInfo count is exceeded. Simplify level texturing.");
            Runs.Add(new StatRun((summary.TextureCount.HasValue ? summary.TextureCount.Value.ToString() : "?") + " texinfos", texInfosExceeded));
        }
        else
            Runs.Add(new StatRun("compile level", false));

        LimitWarning = warning.Length == 0 ? null : warning;
    }
}

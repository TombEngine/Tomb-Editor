#nullable enable

using System;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

// Camera property editing with undo support.
public partial class FlybyTimelineViewModel
{
    /// <summary>
    /// Captures the flyby camera properties tracked by the per-edit undo guard.
    /// </summary>
    private readonly struct FlybyCameraPropertySnapshot
    {
        public ushort Sequence { get; init; }
        public ushort Number { get; init; }
        public short Timer { get; init; }
        public ushort Flags { get; init; }
        public float Speed { get; init; }
        public float Fov { get; init; }
        public float Roll { get; init; }
        public float RotationX { get; init; }
        public float RotationY { get; init; }

        /// <summary>
        /// Captures the current tracked property values from a flyby camera.
        /// </summary>
        /// <param name="camera">Camera whose tracked editable properties should be snapshotted.</param>
        /// <returns>A snapshot containing the camera state used to detect no-op edits.</returns>
        public static FlybyCameraPropertySnapshot Capture(FlybyCameraInstance camera) => new()
        {
            Sequence = camera.Sequence,
            Number = camera.Number,
            Timer = camera.Timer,
            Flags = camera.Flags,
            Speed = camera.Speed,
            Fov = camera.Fov,
            Roll = camera.Roll,
            RotationX = camera.RotationX,
            RotationY = camera.RotationY
        };
    }

    /// <summary>
    /// Applies a speed edit and refreshes timing-dependent timeline state.
    /// </summary>
    partial void OnCameraSpeedChanged(float value)
        => ApplyPropertyToCamera(c => c.Speed = value, invalidateSequenceTiming: true, refreshTimeline: true);

    /// <summary>
    /// Applies a field-of-view edit to the selected camera.
    /// </summary>
    partial void OnCameraFovChanged(float value)
        => ApplyPropertyToCamera(c => c.Fov = FlybyHelpers.ClampFlybyFieldOfViewDegrees(value), invalidateSequenceTiming: false, refreshTimeline: false);

    /// <summary>
    /// Applies a roll edit to the selected camera.
    /// </summary>
    partial void OnCameraRollChanged(float value)
        => ApplyPropertyToCamera(c => c.Roll = value, invalidateSequenceTiming: false, refreshTimeline: false);

    /// <summary>
    /// Applies an X rotation edit to the selected camera.
    /// </summary>
    partial void OnCameraRotationXChanged(float value)
        => ApplyPropertyToCamera(c => c.RotationX = value, invalidateSequenceTiming: false, refreshTimeline: false);

    /// <summary>
    /// Applies a Y rotation edit to the selected camera.
    /// </summary>
    partial void OnCameraRotationYChanged(float value)
        => ApplyPropertyToCamera(c => c.RotationY = value, invalidateSequenceTiming: false, refreshTimeline: false);

    /// <summary>
    /// Applies a timer edit to the selected camera.
    /// </summary>
    partial void OnCameraTimerChanged(short value)
        => ApplyPropertyToCamera(c => c.Timer = value, invalidateSequenceTiming: true, refreshTimeline: true);

    /// <summary>
    /// Applies a flag edit to the selected camera.
    /// </summary>
    partial void OnCameraFlagsChanged(ushort value)
        => ApplyPropertyToCamera(c => c.Flags = value, invalidateSequenceTiming: true, refreshTimeline: true);

    /// <summary>
    /// Applies a property change to the selected camera and records undo state.
    /// </summary>
    /// <param name="setter">Action that applies the property change to the camera.</param>
    /// <param name="invalidateSequenceTiming">Whether to invalidate the sequence timing after the change.</param>
    /// <param name="refreshTimeline">Whether to refresh the timeline after the change.</param>
    private void ApplyPropertyToCamera(Action<FlybyCameraInstance> setter,
        bool invalidateSequenceTiming, bool refreshTimeline)
    {
        if (_isUpdating || SelectedCamera is null)
            return;

        var camera = SelectedCamera.Camera;
        var originalState = FlybyCameraPropertySnapshot.Capture(camera);
        var undoInstance = new ChangeObjectPropertyUndoInstance(_editor.UndoManager, camera);

        _isApplyingProperty = true;

        try
        {
            setter(camera);
        }
        finally
        {
            _isApplyingProperty = false;
        }

        if (!HasTrackedPropertyChanges(camera, originalState))
            return;

        _editor.ObjectChange(camera, ObjectChangeType.Change);

        _preview.InvalidateCache();

        if (invalidateSequenceTiming)
            InvalidateSequenceTiming();

        QueueTimelineRefresh(refreshCameraList: false, refreshTimeline: refreshTimeline);
        PushUndoIfAny([undoInstance]);
    }

    /// <summary>
    /// Returns whether the camera changed in any property tracked by the edit helper.
    /// </summary>
    /// <param name="camera">Camera after the attempted property edit.</param>
    /// <param name="originalState">Snapshot captured before the edit was applied.</param>
    /// <returns><see langword="true"/> when any tracked property changed; otherwise <see langword="false"/>.</returns>
    private static bool HasTrackedPropertyChanges(FlybyCameraInstance camera, FlybyCameraPropertySnapshot originalState)
    {
        return camera.Sequence != originalState.Sequence ||
            camera.Number != originalState.Number ||
            camera.Timer != originalState.Timer ||
            camera.Flags != originalState.Flags ||
            camera.Speed != originalState.Speed ||
            camera.Fov != originalState.Fov ||
            camera.Roll != originalState.Roll ||
            camera.RotationX != originalState.RotationX ||
            camera.RotationY != originalState.RotationY;
    }
}

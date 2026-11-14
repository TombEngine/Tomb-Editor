using System;
using TombLib;

namespace TombEditor.Features.FlybyTimeline;

public static class FlybyHelpers
{
    /// <summary>
    /// Clamps a flyby camera field-of-view value expressed in degrees to the valid persisted range.
    /// </summary>
    public static float ClampFlybyFieldOfViewDegrees(float fovDegrees)
    {
        if (!float.IsFinite(fovDegrees))
            return FlybyConstants.DefaultPreviewFieldOfViewDegrees;

        return Math.Clamp(fovDegrees, 0.0f, FlybyConstants.MaxFlybyFieldOfViewDegrees);
    }

    /// <summary>
    /// Clamps a preview field-of-view value expressed in radians to the valid projection range.
    /// Invalid or too-small values fall back to the default preview field of view.
    /// </summary>
    public static float ClampPreviewFieldOfViewRadians(float fovRadians)
    {
        if (!float.IsFinite(fovRadians) || fovRadians < FlybyConstants.PreviewMinFieldOfViewRadians)
            return MathC.DegToRad(FlybyConstants.DefaultPreviewFieldOfViewDegrees);

        return Math.Min(fovRadians, FlybyConstants.MaxPreviewFieldOfViewRadians);
    }
}

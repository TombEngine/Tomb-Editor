using System;
using System.Numerics;

namespace TombLib.Rendering
{
    public class RenderingState
    {
		public Matrix4x4 TransformMatrix = Matrix4x4.Identity;
        public float RoomGridLineWidth = 10.0f;
        public bool RoomGridForce = false;
        public bool RoomDisableVertexColors = false;
        public bool ShowExtraBlendingModes = true;
        public bool ShowLightingWhiteTextureOnly = true;
        public int LightMode = 0;

        // Object brush overlay (0 = disabled, 1 = circle, 2 = square)
        public int BrushShape = 0;
        public Vector4 BrushCenter = Vector4.Zero; // xyz = world center, w = radius
        public Vector4 BrushColor = Vector4.One;
        public float BrushRotation = 0.0f; // Degrees, for rotation indicator line

        // Flyby DOF overlay. DofColorStrength.w stores packed DOF mode.
        public Vector4 DofCenterRange = Vector4.Zero; // xyz = camera origin, w = range
        public Vector4 DofDirectionDistance = Vector4.Zero; // xyz = normalized view direction, w = focus distance
        public Vector4 DofColorStrength = Vector4.Zero;
    }

    public abstract class RenderingStateBuffer : IDisposable
    {
        public abstract void Dispose();
        public abstract void Set(RenderingState State);
    }
}

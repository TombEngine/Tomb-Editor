using System.Collections.Generic;
using TombLib.Types;

namespace TombLib.Wad
{
    public struct WadAnimRootMotionSettings
    {
        public bool PositionX { get; set; }
        public bool PositionY { get; set; }
        public bool PositionZ { get; set; }
        public bool RotationY { get; set; }

        public int GetBitmask()
        {
            int bitmask = 0;
            if (PositionX) bitmask |= 1 << 0;
            if (PositionY) bitmask |= 1 << 1;
            if (PositionZ) bitmask |= 1 << 2;
            if (RotationY) bitmask |= 1 << 3;

            return bitmask;
        }
    }

    public class WadAnimation
    {
        public byte FrameRate { get; set; } = 1;
        public ushort StateId { get; set; }
        public ushort EndFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
        public string Name { get; set; } = "Animation";

        // New velocities. Originally Core's AnimEdit had Start Velocity and End Velocity pairs and
        // acceleration is obtained used the equations of motion: v = v0 + a * t where in our case
        // t is (Number of KeyFrames + 1) * FrameRate
        public float StartVelocity { get; set; }
        public float EndVelocity { get; set; }
        public float StartLateralVelocity { get; set; }
        public float EndLateralVelocity { get; set; }

        // New parameters for animation blending (TEN only).
        public ushort BlendFrameCount { get; set; }
        public BezierCurve2 BlendCurve { get; set; } = BezierCurve2.Linear.Clone();

        // Root motion settings (TEN only).
        public WadAnimRootMotionSettings RootMotion { get; set; }

        public List<WadKeyFrame> KeyFrames { get; private set; } = new List<WadKeyFrame>();
        public List<WadStateChange> StateChanges { get; private set; } = new List<WadStateChange>();
        public List<WadAnimCommand> AnimCommands { get; private set; } = new List<WadAnimCommand>();

        public WadAnimation Clone()
        {
            var animation = (WadAnimation)MemberwiseClone();
            animation.KeyFrames = KeyFrames.ConvertAll(keyFrame => keyFrame.Clone());

            animation.AnimCommands = new List<WadAnimCommand>();
            foreach (var ac in AnimCommands)
                animation.AnimCommands.Add(ac.Clone());

            animation.StateChanges = new List<WadStateChange>();
            foreach (var sc in StateChanges)
                animation.StateChanges.Add(sc.Clone());

            animation.BlendCurve = BlendCurve.Clone();

            return animation;
        }

        public int GetRealNumberOfFrames(int keyFrameCount = -1)
        {
            if (keyFrameCount < 0) keyFrameCount = KeyFrames.Count;
            if (keyFrameCount == 0) return 0;

            return FrameRate * (keyFrameCount - 1) + 1;
        }
    }
}

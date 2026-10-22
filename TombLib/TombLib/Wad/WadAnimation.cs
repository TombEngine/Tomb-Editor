using System;
using System.Collections.Generic;
using TombLib.Types;

namespace TombLib.Wad
{
    [Flags]
    public enum WadAnimRootMotionFlags
    {
        None            = 0,
        TranslationX    = 1 << 0,
        TranslationY    = 1 << 1,
        TranslationZ    = 1 << 2,
        RotationX       = 1 << 3,
        RotationY       = 1 << 4,
        RotationZ       = 1 << 5,
        RootMotionCycle = 1 << 6  // Internally set in TEN.
}

    public struct WadAnimRootMotionSettings
    {
		public WadAnimRootMotionFlags Flags;

        public bool TranslationX { get => Flags.HasFlag(WadAnimRootMotionFlags.TranslationX); set => SetFlag(WadAnimRootMotionFlags.TranslationX, value); }
        public bool TranslationY { get => Flags.HasFlag(WadAnimRootMotionFlags.TranslationY); set => SetFlag(WadAnimRootMotionFlags.TranslationY, value); }
        public bool TranslationZ { get => Flags.HasFlag(WadAnimRootMotionFlags.TranslationZ); set => SetFlag(WadAnimRootMotionFlags.TranslationZ, value); }
        public bool RotationX    { get => Flags.HasFlag(WadAnimRootMotionFlags.RotationX);    set => SetFlag(WadAnimRootMotionFlags.RotationX, value); }
        public bool RotationY    { get => Flags.HasFlag(WadAnimRootMotionFlags.RotationY);    set => SetFlag(WadAnimRootMotionFlags.RotationY, value); }
        public bool RotationZ    { get => Flags.HasFlag(WadAnimRootMotionFlags.RotationZ);    set => SetFlag(WadAnimRootMotionFlags.RotationZ, value); }

        private void SetFlag(WadAnimRootMotionFlags flag, bool enabled)
        {
            if (enabled)
				Flags |= flag;
            else
				Flags &= ~flag;
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

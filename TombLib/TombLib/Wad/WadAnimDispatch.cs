using TombLib.Types;

namespace TombLib.Wad
{
    public class WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextLowFrame { get; set; }

        // New parameter for frame range mapping (TEN only).
        public ushort NextHighFrame { get; set; }

        // New parameters for animation blending (TEN only).
        public ushort BlendFrames { get; set; }
        public BezierCurve2 BlendCurve { get; set; } = BezierCurve2.Linear.Clone();

        public WadAnimDispatch() { }

        public WadAnimDispatch(ushort inFrame, ushort outFrame, ushort nextAnimation, ushort nextLowFrame)
        {
            InFrame = inFrame;
            OutFrame = outFrame;
            NextAnimation = nextAnimation;
            NextLowFrame = nextLowFrame;
            NextHighFrame = nextLowFrame;
        }
    }
}

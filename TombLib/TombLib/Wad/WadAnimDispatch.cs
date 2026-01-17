using TombLib.Types;

namespace TombLib.Wad
{
    public class WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrameLow { get; set; }

        // New parameter for frame range mapping (TEN only).
        public ushort NextFrameHigh { get; set; }

        // New parameters for animation blending (TEN only).
        public ushort BlendFrameCount { get; set; }
        public BezierCurve2 BlendCurve { get; set; } = BezierCurve2.Linear.Clone();

        public WadAnimDispatch() { }

        public WadAnimDispatch(ushort inFrame, ushort outFrame, ushort nextAnimation, ushort nextFrameLow)
        {
            InFrame = inFrame;
            OutFrame = outFrame;
            NextAnimation = nextAnimation;
            NextFrameLow = nextFrameLow;
            //NextFrameHigh = NextFrameHigh; // TODO: This belongs here.
        }
    }
}

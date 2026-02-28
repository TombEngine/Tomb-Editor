using TombLib.Graphics.Dx11Toolkit;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SolidVertex : IVertex
    {
        [VertexElement("POSITION", 0, DxgiFormat.R32G32B32_Float, 0)]
        public Vector3 Position;
        private readonly float _unusedPadding;
        [VertexElement("COLOR", 0, DxgiFormat.R32G32B32A32_Float, 16)]
        public Vector4 Color;

        Vector3 IVertex.Position => Position;

        public SolidVertex(Vector3 pos)
        {
            Position = pos;
            _unusedPadding = 0;
            Color = Vector4.One;
        }
    }
}

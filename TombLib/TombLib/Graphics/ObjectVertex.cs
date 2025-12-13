using SharpDX.Toolkit.Graphics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObjectVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, -1)]
        public Vector3 Position;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32B32_Float, -1)]
        public Vector3 UVW;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, -1)]
        public Vector3 Normal;
        [VertexElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32_Float, -1)]
        public Vector3 Color;
        [VertexElement("BLENDINDICES", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, -1)]
        public Vector4 Indices; 
        [VertexElement("BLENDWEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, -1)]
        public Vector4 Weights;

        Vector3 IVertex.Position => Position;
    }
}

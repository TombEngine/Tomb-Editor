using System;

namespace TombLib.Graphics.Dx11Toolkit
{
    // ── Toolkit-level flags (unchanged from Phase 2) ──────────────────

    [Flags]
    public enum TextureFlags
    {
        None = 0,
        ShaderResource = 1,
        RenderTarget = 2,
        UnorderedAccess = 4,
    }

    [Flags]
    public enum BufferFlags
    {
        None = 0,
        VertexBuffer = 1,
        IndexBuffer = 2,
        ConstantBuffer = 4,
        ShaderResource = 8,
        UnorderedAccess = 16,
        StructuredBuffer = 32,
    }

    /// <summary>Primitive topology. Values match D3D_PRIMITIVE_TOPOLOGY.</summary>
    public enum PrimitiveType
    {
        Undefined = 0,
        PointList = 1,
        LineList = 2,
        LineStrip = 3,
        TriangleList = 4,
        TriangleStrip = 5,
    }

    // ── Façade types replacing SharpDX.Direct3D11 / DXGI enums ───────

    /// <summary>GPU resource usage. Values match D3D11_USAGE.</summary>
    public enum ResourceUsage
    {
        Default = 0,
        Immutable = 1,
        Dynamic = 2,
        Staging = 3,
    }

    /// <summary>Rasterizer cull mode. Values match D3D11_CULL_MODE.</summary>
    public enum CullMode
    {
        None = 1,
        Front = 2,
        Back = 3,
    }

    /// <summary>Rasterizer fill mode. Values match D3D11_FILL_MODE.</summary>
    public enum FillMode
    {
        Wireframe = 2,
        Solid = 3,
    }

    /// <summary>
    /// Subset of DXGI_FORMAT values actually used in the codebase.
    /// Numeric values match native DXGI_FORMAT so we can cast directly to Silk.NET.DXGI.Format.
    /// </summary>
    public enum DxgiFormat
    {
        Unknown = 0,
        R32G32B32A32_Float = 2,
        R32G32B32A32_UInt = 3,
        R32G32B32A32_SInt = 4,
        R32G32B32_Float = 6,
        R32G32B32_UInt = 7,
        R32G32B32_SInt = 8,
        R16G16B16A16_Float = 10,
        R32G32_Float = 16,
        R32G32_UInt = 17,
        R32G32_SInt = 18,
        R8G8B8A8_UNorm = 28,
        R16G16_Float = 34,
        R16G16_UNorm = 35,
        R32_Float = 41,
        R32_UInt = 42,
        R32_SInt = 43,
        R16_UInt = 57,
        B8G8R8A8_UNorm = 87,
    }

    /// <summary>Simplified rasterizer state description for consumer code.</summary>
    public struct RasterizerStateDescription
    {
        public CullMode CullMode;
        public FillMode FillMode;
        public bool IsDepthClipEnabled;
        public int DepthBias;
        public float DepthBiasClamp;
        public float SlopeScaledDepthBias;
        public bool IsAntialiasedLineEnabled;
        public bool IsFrontCounterClockwise;
        public bool IsMultisampleEnabled;
        public bool IsScissorEnabled;
    }

    /// <summary>Initial data for texture / buffer creation. Matches D3D11_SUBRESOURCE_DATA layout.</summary>
    public struct DataBox
    {
        public IntPtr DataPointer;
        public int RowPitch;
        public int SlicePitch;

        public DataBox(IntPtr dataPointer, int rowPitch, int slicePitch)
        {
            DataPointer = dataPointer;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }
    }

    /// <summary>Pointer + size pair for texture updates.</summary>
    public struct DataPointer
    {
        public IntPtr Pointer;
        public int Size;

        public DataPointer(IntPtr pointer, int size)
        {
            Pointer = pointer;
            Size = size;
        }
    }

    /// <summary>Axis-aligned sub-region of a resource. Matches D3D11_BOX layout.</summary>
    public struct ResourceRegion
    {
        public int Left;
        public int Top;
        public int Front;
        public int Right;
        public int Bottom;
        public int Back;
    }
}

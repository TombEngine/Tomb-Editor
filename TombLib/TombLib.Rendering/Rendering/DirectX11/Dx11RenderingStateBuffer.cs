using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingStateBuffer : RenderingStateBuffer
    {
        // Microsoft reference for "Packing Rules for Constant Variables":
        // https://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx
        [StructLayout(LayoutKind.Explicit)]
        public struct ConstantBufferLayout
        {
            [FieldOffset(0)]
            public Matrix4x4 TransformMatrix;
            [FieldOffset(64)]
            public float RoomGridLineWidth;
            [FieldOffset(68)]
            public int RoomGridForce;
            [FieldOffset(72)]
            public int RoomDisableVertexColors;
            [FieldOffset(76)]
            public int ShowExtraBlendingModes;
            [FieldOffset(80)]
            public int ShowLightingWhiteTextureOnly;
            [FieldOffset(84)]
            public int LightMode;
        };
        public static readonly int Size = ((Marshal.SizeOf(typeof(ConstantBufferLayout)) + 15) / 16) * 16;

        public readonly ID3D11DeviceContext* Context;
        public ComPtr<ID3D11Buffer> ConstantBuffer;

        public Dx11RenderingStateBuffer(Dx11RenderingDevice device)
        {
            Context = device.Context.Handle;

            BufferDesc desc = new BufferDesc
            {
                ByteWidth = (uint)Size,
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0
            };

            ID3D11Buffer* pBuffer = null;
            SilkMarshal.ThrowHResult(
                device.Device.Handle->CreateBuffer(&desc, (SubresourceData*)null, &pBuffer));
            ConstantBuffer = new ComPtr<ID3D11Buffer>(pBuffer);
        }

        public override void Dispose()
        {
            ConstantBuffer.Dispose();
        }

        public override void Set(RenderingState State)
        {
            ConstantBufferLayout buffer;
            buffer.TransformMatrix = State.TransformMatrix;
            buffer.RoomGridLineWidth = State.RoomGridLineWidth;
            buffer.RoomGridForce = State.RoomGridForce ? 1 : 0;
            buffer.RoomDisableVertexColors = State.RoomDisableVertexColors ? 1 : 0;
            buffer.ShowExtraBlendingModes = State.ShowExtraBlendingModes ? 1 : 0;
            buffer.ShowLightingWhiteTextureOnly = State.ShowLightingWhiteTextureOnly ? 1 : 0;
            buffer.LightMode = State.LightMode;
            Context->UpdateSubresource((ID3D11Resource*)ConstantBuffer.Handle, 0, (Box*)null, &buffer, 0, 0);
        }
    }
}

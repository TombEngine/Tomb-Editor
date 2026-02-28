using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Numerics;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingDrawingTest : RenderingDrawingTest
    {
        public readonly Dx11RenderingDevice Device;
        public ID3D11Buffer* VertexBuffer;
        private uint[] _strides;
        private uint[] _offsets;

        public Dx11RenderingDrawingTest(Dx11RenderingDevice device, Description description)
        {
            Device = device;

            // Create buffer
            const int vertexCount = 3;
            int size = vertexCount * (sizeof(Vector3) + sizeof(uint));
            fixed (byte* data = new byte[size])
            {
                Vector3* positions = (Vector3*)(data);
                uint* colors = (uint*)(data + vertexCount * sizeof(Vector3));

                // Setup vertices
                positions[0] = new Vector3(0.0f, 0.0f, 0.0f);
                colors[0] = 0xff000080;
                positions[1] = new Vector3(0.0f, 1.0f, 0.0f);
                colors[1] = 0xff008000;
                positions[2] = new Vector3(1.0f, 0.0f, 0.0f);
                colors[2] = 0xff800000;

                // Create GPU resources
                BufferDesc bufferDesc = new BufferDesc
                {
                    ByteWidth = (uint)size,
                    Usage = Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0
                };
                SubresourceData initData = new SubresourceData { PSysMem = data, SysMemPitch = 0, SysMemSlicePitch = 0 };

                ID3D11Buffer* pVB = null;
                SilkMarshal.ThrowHResult(
                    device.Device.Handle->CreateBuffer(&bufferDesc, &initData, &pVB));
                VertexBuffer = pVB;

                _strides = new uint[] { (uint)sizeof(Vector3), sizeof(uint) };
                _offsets = new uint[]
                {
                    (uint)((byte*)positions - data),
                    (uint)((byte*)colors - data)
                };
            }
        }

        public override void Dispose()
        {
            if (VertexBuffer != null)
            {
                VertexBuffer->Release();
                VertexBuffer = null;
            }
        }

        public override void Render(RenderArgs arg)
        {
            /*var pContext = Device.Context.Handle;

            // Setup state
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();
            Device.TestShader.Apply(pContext, arg.StateBuffer);

            ID3D11Buffer** ppBuffers = stackalloc ID3D11Buffer*[2] { VertexBuffer, VertexBuffer };
            fixed (uint* pStrides = _strides)
            fixed (uint* pOffsets = _offsets)
            {
                pContext->IASetVertexBuffers(0, 2, ppBuffers, pStrides, pOffsets);
            }

            // Render
            pContext->Draw(3, 0);*/
        }
    }
}

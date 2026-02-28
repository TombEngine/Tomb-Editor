using Silk.NET.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Non-generic GPU buffer wrapping a native ID3D11Buffer.
    /// </summary>
    public unsafe class Buffer : IDisposable
    {
        internal ID3D11Buffer* NativeBuffer { get; private set; }
        public int ElementCount { get; }
        public int StructureByteStride { get; }

        internal Buffer(ID3D11Buffer* nativeBuffer, int elementCount, int stride)
        {
            NativeBuffer = nativeBuffer;
            ElementCount = elementCount;
            StructureByteStride = stride;
        }

        public void Dispose()
        {
            if (NativeBuffer != null)
            {
                NativeBuffer->Release();
                NativeBuffer = null;
            }
        }

        // ── Static generic factory ───────────────────────────────────

        public static Buffer<T> New<T>(GraphicsDevice device, T[] data, BufferFlags flags,
            ResourceUsage usage = ResourceUsage.Default) where T : struct
        {
            int stride = Marshal.SizeOf<T>();
            uint bindFlags = 0;
            if (flags.HasFlag(BufferFlags.VertexBuffer)) bindFlags |= (uint)BindFlag.VertexBuffer;
            if (flags.HasFlag(BufferFlags.IndexBuffer)) bindFlags |= (uint)BindFlag.IndexBuffer;
            if (flags.HasFlag(BufferFlags.ConstantBuffer)) bindFlags |= (uint)BindFlag.ConstantBuffer;
            if (flags.HasFlag(BufferFlags.ShaderResource)) bindFlags |= (uint)BindFlag.ShaderResource;

            return CreateBuffer<T>(device, data, stride, (Usage)(int)usage, bindFlags);
        }

        // ── Nested Vertex factory ────────────────────────────────────

        public static class Vertex
        {
            public static Buffer<T> New<T>(GraphicsDevice device, T[] data,
                ResourceUsage usage = ResourceUsage.Default) where T : struct
            {
                return CreateBuffer<T>(device, data, Marshal.SizeOf<T>(),
                    (Usage)(int)usage, (uint)BindFlag.VertexBuffer);
            }

            /// <summary>Creates an empty dynamic vertex buffer.</summary>
            public static Buffer<T> New<T>(GraphicsDevice device, int elementCount) where T : struct
            {
                int stride = Marshal.SizeOf<T>();
                var desc = new BufferDesc
                {
                    ByteWidth = (uint)(stride * elementCount),
                    Usage = Usage.Dynamic,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = (uint)CpuAccessFlag.Write,
                    StructureByteStride = (uint)stride,
                };

                ID3D11Buffer* native;
                int hr = device.NativeDevice->CreateBuffer(&desc, null, &native);
                Marshal.ThrowExceptionForHR(hr);
                return new Buffer<T>(native, elementCount, stride);
            }
        }

        // ── Nested Index factory ─────────────────────────────────────

        public static class Index
        {
            public static Buffer New(GraphicsDevice device, int[] data,
                ResourceUsage usage = ResourceUsage.Default)
                => CreateIndexBuffer(device, data, sizeof(int), usage);

            public static Buffer New(GraphicsDevice device, short[] data)
                => CreateIndexBuffer(device, data, sizeof(short), ResourceUsage.Default);

            public static Buffer New(GraphicsDevice device, ushort[] data)
                => CreateIndexBuffer(device, data, sizeof(ushort), ResourceUsage.Default);

            private static Buffer CreateIndexBuffer<T>(GraphicsDevice device, T[] data,
                int elementSize, ResourceUsage usage) where T : struct
            {
                var desc = new BufferDesc
                {
                    ByteWidth = (uint)(elementSize * data.Length),
                    Usage = (Usage)(int)usage,
                    BindFlags = (uint)BindFlag.IndexBuffer,
                    CPUAccessFlags = usage == ResourceUsage.Dynamic ? (uint)CpuAccessFlag.Write : 0,
                    StructureByteStride = (uint)elementSize,
                };

                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var initData = new SubresourceData { PSysMem = (void*)handle.AddrOfPinnedObject() };
                    ID3D11Buffer* native;
                    int hr = device.NativeDevice->CreateBuffer(&desc, &initData, &native);
                    Marshal.ThrowExceptionForHR(hr);
                    return new Buffer(native, data.Length, elementSize);
                }
                finally { handle.Free(); }
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static Buffer<T> CreateBuffer<T>(GraphicsDevice device, T[] data,
            int stride, Usage usage, uint bindFlags) where T : struct
        {
            var desc = new BufferDesc
            {
                ByteWidth = (uint)(stride * data.Length),
                Usage = usage,
                BindFlags = bindFlags,
                CPUAccessFlags = usage == Usage.Dynamic ? (uint)CpuAccessFlag.Write : 0,
                StructureByteStride = (uint)stride,
            };

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var initData = new SubresourceData { PSysMem = (void*)handle.AddrOfPinnedObject() };
                ID3D11Buffer* native;
                int hr = device.NativeDevice->CreateBuffer(&desc, &initData, &native);
                Marshal.ThrowExceptionForHR(hr);
                return new Buffer<T>(native, data.Length, stride);
            }
            finally { handle.Free(); }
        }
    }

    /// <summary>
    /// Typed GPU buffer.
    /// </summary>
    public unsafe class Buffer<T> : Buffer where T : struct
    {
        internal Buffer(ID3D11Buffer* nativeBuffer, int elementCount, int stride)
            : base(nativeBuffer, elementCount, stride) { }

        /// <summary>Uploads new data to a dynamic buffer via Map/Unmap.</summary>
        public void SetData(GraphicsDevice device, T[] data)
        {
            MappedSubresource mapped;
            int hr = device.NativeContext->Map(
                (ID3D11Resource*)NativeBuffer, 0, Map.WriteDiscard, 0, &mapped);
            Marshal.ThrowExceptionForHR(hr);

            try
            {
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    int maxBytes = StructureByteStride * ElementCount;
                    int bytes = Math.Min(Marshal.SizeOf<T>() * data.Length, maxBytes);
                    System.Buffer.MemoryCopy(
                        (void*)handle.AddrOfPinnedObject(),
                        mapped.PData, maxBytes, bytes);
                }
                finally { handle.Free(); }
            }
            finally
            {
                device.NativeContext->Unmap((ID3D11Resource*)NativeBuffer, 0);
            }
        }
    }
}

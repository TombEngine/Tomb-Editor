using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// A named shader parameter backed by a constant buffer variable or resource binding.
    /// </summary>
    public class EffectParameter
    {
        internal int CBufferIndex { get; set; } = -1;
        internal int ByteOffset { get; set; }
        internal int ByteSize { get; set; }
        internal int ResourceSlot { get; set; } = -1;
        internal int SamplerSlot { get; set; } = -1;
        internal bool IsResource { get; set; }
        internal bool IsSampler { get; set; }

        // Bound SRV stored as native pointer (ID3D11ShaderResourceView*)
        internal unsafe ID3D11ShaderResourceView* BoundSRV { get; set; }

        // Bound sampler state stored as native pointer (ID3D11SamplerState*)
        internal unsafe ID3D11SamplerState* BoundSampler { get; set; }

        private Effect _owner;
        internal EffectParameter(Effect owner) { _owner = owner; }
        internal void SetOwner(Effect effect) { _owner = effect; }

        public void SetValue(Matrix4x4 value)
        {
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref value, 64);
        }

        public void SetValue(Vector4 value)
        {
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref value, 16);
        }

        public void SetValue(Vector3 value)
        {
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref value, 12);
        }

        public void SetValue(Vector2 value)
        {
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref value, 8);
        }

        public void SetValue(float value)
        {
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref value, 4);
        }

        public void SetValue(bool value)
        {
            int intVal = value ? 1 : 0;
            if (CBufferIndex >= 0)
                _owner?.WriteToCBuffer(CBufferIndex, ByteOffset, ref intVal, 4);
        }

        public void SetValue(Matrix4x4[] value)
        {
            if (CBufferIndex >= 0 && value != null)
            {
                int totalBytes = value.Length * 64;
                if (totalBytes > ByteSize) totalBytes = ByteSize;
                _owner?.WriteToCBufferArray(CBufferIndex, ByteOffset, value, totalBytes);
            }
        }

        /// <summary>Bind a texture's SRV to this resource slot.</summary>
        public unsafe void SetResource(Texture2D texture)
        {
            BoundSRV = texture != null ? texture.NativeSRV : null;
        }

        /// <summary>Bind a sampler state to this sampler slot.</summary>
        public unsafe void SetResource(SamplerState sampler)
        {
            BoundSampler = sampler != null ? sampler.NativeState : null;
        }
    }

    /// <summary>Named collection of effect parameters.</summary>
    public class EffectParameterCollection
    {
        private readonly Dictionary<string, EffectParameter> _parameters;
        internal EffectParameterCollection(Dictionary<string, EffectParameter> parameters) { _parameters = parameters; }

        public EffectParameter this[string name]
        {
            get
            {
                if (_parameters.TryGetValue(name, out var p)) return p;
                System.Diagnostics.Debug.WriteLine($"[Effect] Parameter '{name}' not found in shader. Returning no-op stub.");
                return new EffectParameter(null); // no-op stub
            }
        }
    }

    /// <summary>A single rendering pass within a technique.</summary>
    public unsafe class EffectPass
    {
        private Effect _effect;
        private bool _disposed;
        internal ID3D11VertexShader* VertexShader { get; }
        internal ID3D11PixelShader* PixelShader { get; }
        internal byte[] VSBytecode { get; }
        internal int VSBytecodeHash { get; }

        internal EffectPass(Effect effect, ID3D11VertexShader* vs, ID3D11PixelShader* ps, byte[] vsBytecode)
        {
            _effect = effect;
            VertexShader = vs;
            PixelShader = ps;
            VSBytecode = vsBytecode;

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < vsBytecode.Length; i++)
                    hash = hash * 31 + vsBytecode[i];
                VSBytecodeHash = hash;
            }
        }

        internal void SetOwner(Effect effect) { _effect = effect; }

        /// <summary>Sets shaders, uploads cbuffers, binds SRVs, creates/binds InputLayout.</summary>
        public void Apply()
        {
            var device = _effect.Device;
            var ctx = device.NativeContext;

            ctx->VSSetShader(VertexShader, null, 0);
            ctx->PSSetShader(PixelShader, null, 0);

            // Upload dirty constant buffers
            _effect.FlushConstantBuffers();

            // Bind constant buffers
            for (int i = 0; i < _effect.ConstantBuffers.Length; i++)
            {
                var buf = _effect.ConstantBuffers[i].NativeBuffer;
                ctx->VSSetConstantBuffers((uint)i, 1, &buf);
                ctx->PSSetConstantBuffers((uint)i, 1, &buf);
            }

            // Bind SRVs
            foreach (var kv in _effect.ParameterMap)
            {
                if (kv.Value.ResourceSlot >= 0)
                {
                    var srv = kv.Value.BoundSRV;  // may be null — explicitly unbinds the slot
                    ctx->PSSetShaderResources((uint)kv.Value.ResourceSlot, 1, &srv);
                    ctx->VSSetShaderResources((uint)kv.Value.ResourceSlot, 1, &srv);
                }
            }

            // Bind sampler states
            foreach (var kv in _effect.ParameterMap)
            {
                if (kv.Value.SamplerSlot >= 0)
                {
                    var samp = kv.Value.BoundSampler;  // may be null — explicitly unbinds the slot
                    ctx->PSSetSamplers((uint)kv.Value.SamplerSlot, 1, &samp);
                    ctx->VSSetSamplers((uint)kv.Value.SamplerSlot, 1, &samp);
                }
            }

            // Create / set InputLayout
            var layout = device.CurrentVertexInputLayout;
            if (layout != null)
            {
                var key = (VSBytecodeHash, layout.LayoutHash);
                if (!device.InputLayoutCache.TryGetValue(key, out var layoutPtr))
                {
                    var nativeElements = VertexInputLayout.CreateNativeElements(layout.Elements, out var handles);
                    try
                    {
                        ID3D11InputLayout* il;
                        fixed (byte* pBytecode = VSBytecode)
                        {
                            int hr = device.NativeDevice->CreateInputLayout(
                                nativeElements, (uint)layout.Elements.Length,
                                pBytecode, (nuint)VSBytecode.Length, &il);
                            Marshal.ThrowExceptionForHR(hr);
                        }
                        layoutPtr = (nint)il;
                        device.InputLayoutCache[key] = layoutPtr;
                    }
                    finally
                    {
                        VertexInputLayout.FreeNativeElements(nativeElements, handles);
                    }
                }
                ctx->IASetInputLayout((ID3D11InputLayout*)layoutPtr);
            }
        }

        internal void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (VertexShader != null) VertexShader->Release();
            if (PixelShader != null) PixelShader->Release();
        }
    }

    /// <summary>Named technique containing one or more passes.</summary>
    public class EffectTechnique
    {
        public string Name { get; }
        public EffectPassCollection Passes { get; }
        internal EffectTechnique(string name, EffectPass[] passes)
        {
            Name = name;
            Passes = new EffectPassCollection(passes);
        }
    }

    public class EffectPassCollection
    {
        internal readonly EffectPass[] _passes;
        public int Count => _passes.Length;
        public EffectPass this[int index] => _passes[index];
        internal EffectPassCollection(EffectPass[] passes) { _passes = passes; }
    }

    /// <summary>Internal constant buffer backing storage.</summary>
    internal unsafe class CBufferData
    {
        public ID3D11Buffer* NativeBuffer;
        public byte[] Data;
        public bool Dirty;
        public string Name;

        public CBufferData(ID3D11Device* device, int size, string name)
        {
            // D3D11 requires constant buffer size to be a multiple of 16 bytes.
            size = (size + 15) & ~15;
            Data = new byte[size];
            Name = name;
            var desc = new BufferDesc
            {
                ByteWidth = (uint)size,
                Usage = Usage.Dynamic,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
            };
            ID3D11Buffer* buf;
            int hr = device->CreateBuffer(&desc, null, &buf);
            Marshal.ThrowExceptionForHR(hr);
            NativeBuffer = buf;
        }

        public void Dispose()
        {
            if (NativeBuffer != null)
            {
                NativeBuffer->Release();
                NativeBuffer = null;
            }
        }
    }

    /// <summary>Compiled effect containing techniques, passes, and named parameters.</summary>
    public unsafe class Effect : IDisposable
    {
        public GraphicsDevice Device { get; }
        public EffectParameterCollection Parameters { get; }
        public EffectTechnique CurrentTechnique { get; set; }
        public EffectTechnique[] Techniques { get; }
        internal CBufferData[] ConstantBuffers { get; }
        internal Dictionary<string, EffectParameter> ParameterMap { get; }

        internal Effect(GraphicsDevice device, EffectTechnique[] techniques,
            CBufferData[] cbuffers, Dictionary<string, EffectParameter> parameterMap)
        {
            Device = device;
            Techniques = techniques;
            CurrentTechnique = techniques.Length > 0 ? techniques[0] : null;
            ConstantBuffers = cbuffers;
            ParameterMap = parameterMap;
            Parameters = new EffectParameterCollection(parameterMap);
        }

        internal void WriteToCBuffer<T>(int cbIndex, int offset, ref T value, int size) where T : unmanaged
        {
            var cb = ConstantBuffers[cbIndex];
            fixed (byte* dst = cb.Data)
            {
                T local = value;
                System.Buffer.MemoryCopy(&local, dst + offset, cb.Data.Length - offset, size);
            }
            cb.Dirty = true;
        }

        internal void WriteToCBufferArray(int cbIndex, int offset, Matrix4x4[] matrices, int totalBytes)
        {
            var cb = ConstantBuffers[cbIndex];
            var handle = GCHandle.Alloc(matrices, GCHandleType.Pinned);
            try
            {
                fixed (byte* dst = cb.Data)
                {
                    System.Buffer.MemoryCopy(
                        (void*)handle.AddrOfPinnedObject(),
                        dst + offset,
                        cb.Data.Length - offset, totalBytes);
                }
            }
            finally { handle.Free(); }
            cb.Dirty = true;
        }

        internal void FlushConstantBuffers()
        {
            var ctx = Device.NativeContext;
            for (int i = 0; i < ConstantBuffers.Length; i++)
            {
                var cb = ConstantBuffers[i];
                if (!cb.Dirty) continue;

                MappedSubresource mapped;
                int hr = ctx->Map((ID3D11Resource*)cb.NativeBuffer, 0, Map.WriteDiscard, 0, &mapped);
                if (hr >= 0)
                {
                    Marshal.Copy(cb.Data, 0, (IntPtr)mapped.PData, cb.Data.Length);
                    ctx->Unmap((ID3D11Resource*)cb.NativeBuffer, 0);
                    cb.Dirty = false;
                }
            }
        }

        public void Dispose()
        {
            if (Techniques != null)
                foreach (var t in Techniques)
                    if (t.Passes?._passes != null)
                        foreach (var p in t.Passes._passes)
                            p.Dispose();

            if (ConstantBuffers != null)
                foreach (var cb in ConstantBuffers)
                    cb.Dispose();
        }
    }
}

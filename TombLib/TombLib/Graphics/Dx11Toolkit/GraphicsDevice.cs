using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Wraps a D3D11 device and immediate context using Silk.NET COM pointers.
    /// Replaces the SharpDX-based GraphicsDevice.
    /// </summary>
    public unsafe class GraphicsDevice : IDisposable
    {
        internal ID3D11Device* NativeDevice { get; private set; }
        internal ID3D11DeviceContext* NativeContext { get; private set; }

        public RasterizerStates RasterizerStates { get; }
        public BlendStates BlendStates { get; }
        public SamplerStates SamplerStates { get; }
        public DepthStencilStates DepthStencilStates { get; }
        public Features Features { get; }

        // Current vertex input layout (stored for deferred InputLayout creation in Effect.Apply)
        internal VertexInputLayout CurrentVertexInputLayout { get; private set; }

        // Cache for ID3D11InputLayout keyed by (VS bytecode hash, layout hash)
        internal Dictionary<(int, int), nint> InputLayoutCache { get; } = new();

        private GraphicsDevice(ID3D11Device* device)
        {
            NativeDevice = device;

            ID3D11DeviceContext* ctx;
            device->GetImmediateContext(&ctx);
            NativeContext = ctx;

            RasterizerStates = new RasterizerStates(this);
            BlendStates = new BlendStates(this);
            SamplerStates = new SamplerStates(this);
            DepthStencilStates = new DepthStencilStates(this);

            D3DFeatureLevel level = device->GetFeatureLevel();
            Features = new Features((int)level);
        }

        /// <summary>Creates a GraphicsDevice from a native ID3D11Device pointer (IntPtr).</summary>
        public static GraphicsDevice New(IntPtr nativeDevicePtr)
        {
            return new GraphicsDevice((ID3D11Device*)nativeDevicePtr);
        }

        // ── Vertex / Index buffer binding ────────────────────────────

        public void SetVertexBuffer<T>(int slot, Buffer<T> buffer) where T : struct
        {
            uint stride = (uint)buffer.StructureByteStride;
            uint offset = 0;
            var buf = buffer.NativeBuffer;
            NativeContext->IASetVertexBuffers((uint)slot, 1, &buf, &stride, &offset);
        }

        public void SetVertexBuffer<T>(Buffer<T> buffer) where T : struct
            => SetVertexBuffer(0, buffer);

        public void SetVertexBuffer(int slot, Buffer buffer)
        {
            uint stride = (uint)buffer.StructureByteStride;
            uint offset = 0;
            var buf = buffer.NativeBuffer;
            NativeContext->IASetVertexBuffers((uint)slot, 1, &buf, &stride, &offset);
        }

        public void SetVertexBuffer(Buffer buffer) => SetVertexBuffer(0, buffer);

        // ── Input layout ─────────────────────────────────────────────

        public void SetVertexInputLayout(VertexInputLayout layout)
        {
            CurrentVertexInputLayout = layout;
        }

        // ── Index buffer ─────────────────────────────────────────────

        public void SetIndexBuffer(Buffer buffer, bool is32Bit)
        {
            NativeContext->IASetIndexBuffer(
                buffer != null ? buffer.NativeBuffer : null,
                is32Bit ? Format.FormatR32Uint : Format.FormatR16Uint,
                0);
        }

        // ── Draw calls ───────────────────────────────────────────────

        public void DrawIndexed(PrimitiveType primitiveType, int indexCount, int startIndexLocation = 0)
        {
            NativeContext->IASetPrimitiveTopology((D3DPrimitiveTopology)(int)primitiveType);
            NativeContext->DrawIndexed((uint)indexCount, (uint)startIndexLocation, 0);
        }

        public void Draw(PrimitiveType primitiveType, int vertexCount, int startVertexLocation = 0)
        {
            NativeContext->IASetPrimitiveTopology((D3DPrimitiveTopology)(int)primitiveType);
            NativeContext->Draw((uint)vertexCount, (uint)startVertexLocation);
        }

        // ── Pipeline states ──────────────────────────────────────────

        public void SetRasterizerState(RasterizerState state)
        {
            NativeContext->RSSetState(state != null ? state.NativeState : null);
        }

        public void SetBlendState(BlendState state)
        {
            float* blendFactor = null;
            NativeContext->OMSetBlendState(state != null ? state.NativeState : null, blendFactor, 0xFFFFFFFF);
        }

        public void SetDepthStencilState(DepthStencilState state)
        {
            NativeContext->OMSetDepthStencilState(state != null ? state.NativeState : null, 0);
        }

        // ── Resource copy ────────────────────────────────────────────

        public void Copy(Texture2D source, int sourceSubresource, Texture2D destination, int destinationSubresource)
        {
            NativeContext->CopySubresourceRegion(
                (ID3D11Resource*)destination.NativeTexture, (uint)destinationSubresource,
                0, 0, 0,
                (ID3D11Resource*)source.NativeTexture, (uint)sourceSubresource,
                null);
        }

        // ── Disposal ─────────────────────────────────────────────────

        public void Dispose()
        {
            foreach (var kv in InputLayoutCache)
            {
                if (kv.Value != 0)
                    ((ID3D11InputLayout*)kv.Value)->Release();
            }
            InputLayoutCache.Clear();

            RasterizerStates?.Dispose();
            BlendStates?.Dispose();
            SamplerStates?.Dispose();
            DepthStencilStates?.Dispose();

            // Release the context we obtained (GetImmediateContext adds a ref).
            if (NativeContext != null)
            {
                NativeContext->Release();
                NativeContext = null;
            }

            // We do NOT release NativeDevice — it is owned by Dx11RenderingDevice (Silk.NET).
            NativeDevice = null;
        }
    }

    // ── Pre-built rasterizer states ──────────────────────────────────

    public class RasterizerStates : IDisposable
    {
        public RasterizerState CullNone { get; }
        public RasterizerState CullBack { get; }
        public RasterizerState Default { get; }

        internal RasterizerStates(GraphicsDevice device)
        {
            CullNone = RasterizerState.New(device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsDepthClipEnabled = true,
            });

            CullBack = RasterizerState.New(device, new RasterizerStateDescription
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                IsDepthClipEnabled = true,
            });

            Default = CullBack;
        }

        public void Dispose()
        {
            CullNone?.Dispose();
            CullBack?.Dispose();
        }
    }

    // ── Pre-built blend states ───────────────────────────────────────

    public unsafe class BlendStates : IDisposable
    {
        public BlendState Opaque { get; }
        public BlendState Additive { get; }
        public BlendState NonPremultiplied { get; }
        public BlendState AlphaBlend { get; }

        internal BlendStates(GraphicsDevice device)
        {
            // Opaque
            {
                var desc = new BlendDesc();
                desc.RenderTarget[0] = new RenderTargetBlendDesc
                {
                    BlendEnable = 0,
                    RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                };
                Opaque = BlendState.New(device, desc);
            }
            // Additive
            {
                var desc = new BlendDesc();
                desc.RenderTarget[0] = new RenderTargetBlendDesc
                {
                    BlendEnable = 1,
                    SrcBlend = Blend.SrcAlpha,
                    DestBlend = Blend.One,
                    BlendOp = BlendOp.Add,
                    SrcBlendAlpha = Blend.SrcAlpha,
                    DestBlendAlpha = Blend.One,
                    BlendOpAlpha = BlendOp.Add,
                    RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                };
                Additive = BlendState.New(device, desc);
            }
            // NonPremultiplied (traditional alpha blending)
            {
                var desc = new BlendDesc();
                desc.RenderTarget[0] = new RenderTargetBlendDesc
                {
                    BlendEnable = 1,
                    SrcBlend = Blend.SrcAlpha,
                    DestBlend = Blend.InvSrcAlpha,
                    BlendOp = BlendOp.Add,
                    SrcBlendAlpha = Blend.SrcAlpha,
                    DestBlendAlpha = Blend.InvSrcAlpha,
                    BlendOpAlpha = BlendOp.Add,
                    RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                };
                NonPremultiplied = BlendState.New(device, desc);
            }
            // AlphaBlend (premultiplied alpha)
            {
                var desc = new BlendDesc();
                desc.RenderTarget[0] = new RenderTargetBlendDesc
                {
                    BlendEnable = 1,
                    SrcBlend = Blend.One,
                    DestBlend = Blend.InvSrcAlpha,
                    BlendOp = BlendOp.Add,
                    SrcBlendAlpha = Blend.One,
                    DestBlendAlpha = Blend.InvSrcAlpha,
                    BlendOpAlpha = BlendOp.Add,
                    RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                };
                AlphaBlend = BlendState.New(device, desc);
            }
        }

        public void Dispose()
        {
            Opaque?.Dispose();
            Additive?.Dispose();
            NonPremultiplied?.Dispose();
            AlphaBlend?.Dispose();
        }
    }

    // ── Feature level ────────────────────────────────────────────────

    // ── Pre-built sampler states ─────────────────────────────────────

    public unsafe class SamplerStates : IDisposable
    {
        /// <summary>Linear filtering, Wrap addressing (MinMagMipLinear).</summary>
        public SamplerState Default { get; }

        /// <summary>Anisotropic filtering, Wrap addressing.</summary>
        public SamplerState AnisotropicWrap { get; }

        /// <summary>Point filtering, Clamp addressing.</summary>
        public SamplerState PointClamp { get; }

        /// <summary>Point filtering, Wrap addressing.</summary>
        public SamplerState PointWrap { get; }

        internal SamplerStates(GraphicsDevice device)
        {
            Default = SamplerState.New(device, new SamplerDesc
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxAnisotropy = 1,
                ComparisonFunc = ComparisonFunc.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            });

            AnisotropicWrap = SamplerState.New(device, new SamplerDesc
            {
                Filter = Filter.Anisotropic,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxAnisotropy = 16,
                ComparisonFunc = ComparisonFunc.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            });

            PointClamp = SamplerState.New(device, new SamplerDesc
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                MaxAnisotropy = 1,
                ComparisonFunc = ComparisonFunc.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            });

            PointWrap = SamplerState.New(device, new SamplerDesc
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxAnisotropy = 1,
                ComparisonFunc = ComparisonFunc.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            });
        }

        public void Dispose()
        {
            Default?.Dispose();
            AnisotropicWrap?.Dispose();
            PointClamp?.Dispose();
            PointWrap?.Dispose();
        }
    }

    // ── Feature level (continued) ────────────────────────────────────

    // ── Pre-built depth stencil states ───────────────────────────────

    public unsafe class DepthStencilStates : IDisposable
    {
        /// <summary>Depth test enabled, depth write enabled, comparison LessEqual.</summary>
        public DepthStencilState Default { get; }

        /// <summary>Depth test enabled, depth write disabled (read-only depth).</summary>
        public DepthStencilState DepthRead { get; }

        /// <summary>Depth test disabled, depth write disabled.</summary>
        public DepthStencilState None { get; }

        internal DepthStencilStates(GraphicsDevice device)
        {
            Default = DepthStencilState.New(device, new DepthStencilDesc
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunc.LessEqual,
                StencilEnable = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
            });

            DepthRead = DepthStencilState.New(device, new DepthStencilDesc
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunc.LessEqual,
                StencilEnable = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
            });

            None = DepthStencilState.New(device, new DepthStencilDesc
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunc.LessEqual,
                StencilEnable = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
            });
        }

        public void Dispose()
        {
            Default?.Dispose();
            DepthRead?.Dispose();
            None?.Dispose();
        }
    }

    // ── Feature level ────────────────────────────────────────────────

    public class Features
    {
        /// <summary>Raw D3D feature level as an integer (e.g. 0xb000 = 11.0).</summary>
        public int Level { get; }
        internal Features(int level) { Level = level; }
    }

    /// <summary>D3D_FEATURE_LEVEL constants. Values match the native enum.</summary>
    public static class FeatureLevel
    {
        public const int Level_9_1 = 0x9100;
        public const int Level_9_2 = 0x9200;
        public const int Level_9_3 = 0x9300;
        public const int Level_10_0 = 0xa000;
        public const int Level_10_1 = 0xa100;
        public const int Level_11_0 = 0xb000;
        public const int Level_11_1 = 0xb100;
    }
}

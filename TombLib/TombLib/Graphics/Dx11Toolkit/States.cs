using Silk.NET.Direct3D11;
using System;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>Wraps an ID3D11RasterizerState.</summary>
    public unsafe class RasterizerState : IDisposable
    {
        internal ID3D11RasterizerState* NativeState { get; private set; }

        private RasterizerState(ID3D11RasterizerState* nativeState)
        {
            NativeState = nativeState;
        }

        public static RasterizerState New(GraphicsDevice device, RasterizerStateDescription description)
        {
            var desc = new RasterizerDesc
            {
                CullMode = (Silk.NET.Direct3D11.CullMode)(int)description.CullMode,
                FillMode = (Silk.NET.Direct3D11.FillMode)(int)description.FillMode,
                DepthClipEnable = description.IsDepthClipEnabled,
                FrontCounterClockwise = description.IsFrontCounterClockwise,
                DepthBias = description.DepthBias,
                DepthBiasClamp = description.DepthBiasClamp,
                SlopeScaledDepthBias = description.SlopeScaledDepthBias,
                ScissorEnable = description.IsScissorEnabled,
                MultisampleEnable = description.IsMultisampleEnabled,
                AntialiasedLineEnable = description.IsAntialiasedLineEnabled,
            };

            ID3D11RasterizerState* native;
            int hr = device.NativeDevice->CreateRasterizerState(&desc, &native);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            return new RasterizerState(native);
        }

        public void Dispose()
        {
            if (NativeState != null)
            {
                NativeState->Release();
                NativeState = null;
            }
        }
    }

    /// <summary>Wraps an ID3D11BlendState.</summary>
    public unsafe class BlendState : IDisposable
    {
        internal ID3D11BlendState* NativeState { get; private set; }

        private BlendState(ID3D11BlendState* nativeState)
        {
            NativeState = nativeState;
        }

        internal static BlendState New(GraphicsDevice device, BlendDesc description)
        {
            ID3D11BlendState* native;
            int hr = device.NativeDevice->CreateBlendState(&description, &native);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            return new BlendState(native);
        }

        public void Dispose()
        {
            if (NativeState != null)
            {
                NativeState->Release();
                NativeState = null;
            }
        }
    }

    /// <summary>Wraps an ID3D11DepthStencilState.</summary>
    public unsafe class DepthStencilState : IDisposable
    {
        internal ID3D11DepthStencilState* NativeState { get; private set; }

        private DepthStencilState(ID3D11DepthStencilState* nativeState)
        {
            NativeState = nativeState;
        }

        internal static DepthStencilState New(GraphicsDevice device, DepthStencilDesc description)
        {
            ID3D11DepthStencilState* native;
            int hr = device.NativeDevice->CreateDepthStencilState(&description, &native);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            return new DepthStencilState(native);
        }

        public void Dispose()
        {
            if (NativeState != null)
            {
                NativeState->Release();
                NativeState = null;
            }
        }
    }

    /// <summary>Wraps an ID3D11SamplerState.</summary>
    public unsafe class SamplerState : IDisposable
    {
        internal ID3D11SamplerState* NativeState { get; private set; }

        private SamplerState(ID3D11SamplerState* nativeState)
        {
            NativeState = nativeState;
        }

        internal static SamplerState New(GraphicsDevice device, SamplerDesc description)
        {
            ID3D11SamplerState* native;
            int hr = device.NativeDevice->CreateSamplerState(&description, &native);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            return new SamplerState(native);
        }

        public void Dispose()
        {
            if (NativeState != null)
            {
                NativeState->Release();
                NativeState = null;
            }
        }
    }
}

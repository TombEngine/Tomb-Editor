using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingSwapChain : RenderingSwapChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Dx11RenderingDevice Device;
        public ComPtr<IDXGISwapChain> SwapChain;
        public ID3D11Texture2D* BackBuffer;
        public ID3D11RenderTargetView* BackBufferView;
        public ID3D11Texture2D* DepthBuffer;
        public ID3D11DepthStencilView* DepthBufferView;

        public static readonly Rational RefreshRate = new Rational(60, 1);
        public static readonly Format ColorFormat = Format.FormatR8G8B8A8Unorm;
        public static readonly Format DepthFormat = Format.FormatD32Float;
        public const int BufferCount = 2;

        private int _sampleCount;

        public Dx11RenderingSwapChain(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            Size = description.Size;
            RenderException = null;

            _sampleCount = GetAntialiasQuality(description.Antialias ? 4 : 1);

            SwapChainDesc swapChainDesc = new SwapChainDesc
            {
                BufferCount = BufferCount,
                BufferDesc = new ModeDesc
                {
                    Width = (uint)Size.X,
                    Height = (uint)Size.Y,
                    RefreshRate = RefreshRate,
                    Format = ColorFormat
                },
                Windowed = 1,
                OutputWindow = description.WindowHandle,
                SampleDesc = new SampleDesc((uint)_sampleCount, 0),
                SwapEffect = SwapEffect.Sequential,
                BufferUsage = DXGI.UsageRenderTargetOutput
            };

            IDXGISwapChain* pSwapChain = null;
            SilkMarshal.ThrowHResult(
                device.Factory.Handle->CreateSwapChain((IUnknown*)device.Device.Handle, &swapChainDesc, &pSwapChain));
            SwapChain = new ComPtr<IDXGISwapChain>(pSwapChain);

            device.Factory.Handle->MakeWindowAssociation(description.WindowHandle, 1 | 2 | 4); // IgnoreAll = DXGI_MWA_NO_WINDOW_CHANGES | NO_ALT_ENTER | NO_PRINT_SCREEN

            CreateBuffersAndViews();
        }

        private void CreateBuffersAndViews()
        {
            // Get back buffer
            Guid tex2DGuid = ID3D11Texture2D.Guid;
            void* pBackBuffer = null;
            SilkMarshal.ThrowHResult(
                SwapChain.Handle->GetBuffer(0, &tex2DGuid, &pBackBuffer));
            BackBuffer = (ID3D11Texture2D*)pBackBuffer;

            // Create render target view
            ID3D11RenderTargetView* pRTV = null;
            SilkMarshal.ThrowHResult(
                Device.Device.Handle->CreateRenderTargetView((ID3D11Resource*)BackBuffer, (RenderTargetViewDesc*)null, &pRTV));
            BackBufferView = pRTV;

            // Create depth buffer
            Texture2DDesc depthDesc = new Texture2DDesc
            {
                Format = DepthFormat,
                ArraySize = 1,
                MipLevels = 1,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                SampleDesc = new SampleDesc((uint)_sampleCount, 0),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.DepthStencil,
                CPUAccessFlags = 0,
                MiscFlags = 0
            };
            ID3D11Texture2D* pDepth = null;
            SilkMarshal.ThrowHResult(
                Device.Device.Handle->CreateTexture2D(&depthDesc, (SubresourceData*)null, &pDepth));
            DepthBuffer = pDepth;

            // Create depth stencil view
            ID3D11DepthStencilView* pDSV = null;
            SilkMarshal.ThrowHResult(
                Device.Device.Handle->CreateDepthStencilView((ID3D11Resource*)DepthBuffer, (DepthStencilViewDesc*)null, &pDSV));
            DepthBufferView = pDSV;
        }

        private int GetAntialiasQuality(int maxQuality)
        {
            int antialiasQuality = maxQuality;
            while (antialiasQuality > 1)
            {
                uint numQualityLevels = 0;
                Device.Device.Handle->CheckMultisampleQualityLevels(Format.FormatR8G8B8A8Unorm, (uint)antialiasQuality, &numQualityLevels);
                if (numQualityLevels != 0)
                    break;
                else
                    antialiasQuality /= 2;
            }
            return antialiasQuality;
        }

        public override void Dispose()
        {
            if (BackBufferView != null) { BackBufferView->Release(); BackBufferView = null; }
            if (BackBuffer != null) { BackBuffer->Release(); BackBuffer = null; }
            if (DepthBufferView != null) { DepthBufferView->Release(); DepthBufferView = null; }
            if (DepthBuffer != null) { DepthBuffer->Release(); DepthBuffer = null; }
            SwapChain.Dispose();
        }

        public void Bind()
        {
            if (Device.CurrentRenderTarget == this)
                return;
            BindForce();
        }

        public void BindForce()
        {
            var pContext = Device.Context.Handle;
            Viewport viewport = new Viewport(0, 0, Size.X, Size.Y, 0.0f, 1.0f);
            pContext->RSSetViewports(1, &viewport);
            var bbView = BackBufferView;
            pContext->OMSetRenderTargets(1, &bbView, DepthBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            var pContext = Device.Context.Handle;
            float* clearColor = stackalloc float[4] { color.X, color.Y, color.Z, color.W };
            pContext->ClearDepthStencilView(DepthBufferView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
            pContext->ClearRenderTargetView(BackBufferView, clearColor);
        }

        public override void ClearDepth()
        {
            var pContext = Device.Context.Handle;
            pContext->ClearDepthStencilView(DepthBufferView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
        }

        public override void Present()
        {
            try
            {
                int hr = SwapChain.Handle->Present(0, 0);
                if (hr < 0) throw Marshal.GetExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                if (RenderException == null)
                {
                    string message = string.Empty;

                    switch (unchecked((uint)ex.HResult))
                    {
                        case 0x887A0005:
                            {
                                int reason = Device.Device.Handle->GetDeviceRemovedReason();
                                message = "Renderer unexpectedly stopped due to DXGI_ERROR_DEVICE_REMOVED exception. Error code: " + reason;
                            }
                            break;

                        case 0x887A0020:
                            {
                                int reason = Device.Device.Handle->GetDeviceRemovedReason();
                                message = "Rendering device was lost due to DXGI_ERROR_DRIVER_INTERNAL_ERROR exception. Error code: " + reason;
                            }
                            break;

                        case 0x887A0006:
                            message = "Rendering device stopped responding due to DXGI_ERROR_DEVICE_HUNG exception.";
                            break;

                        case 0x887A0007:
                            message = "Rendering device was lost due to DXGI_ERROR_DEVICE_RESET exception.";
                            break;

                        case 0x887A0001:
                            message = "Rendering device received invalid call. Possibly one of the shaders was badly modified.";
                            break;

                        default:
                            message = "Unknown error was encountered while rendering.";
                            break;
                    }

                    message += "\n" + "Additional message: " + ex.Message;
                    logger.Error(message);
                    RenderException = ex;
                }
            }
        }

        public override void Resize(VectorInt2 newSize)
        {
            if (newSize.X <= 0 || newSize.Y <= 0)
                return;

            if (Device.CurrentRenderTarget == this)
            {
                Device.CurrentRenderTarget = null;
                Device.Context.Handle->OMSetRenderTargets(0, (ID3D11RenderTargetView**)null, (ID3D11DepthStencilView*)null);
            }

            Size = newSize;
            if (BackBufferView != null) { BackBufferView->Release(); BackBufferView = null; }
            if (BackBuffer != null) { BackBuffer->Release(); BackBuffer = null; }
            if (DepthBufferView != null) { DepthBufferView->Release(); DepthBufferView = null; }
            if (DepthBuffer != null) { DepthBuffer->Release(); DepthBuffer = null; }
            SilkMarshal.ThrowHResult(
                SwapChain.Handle->ResizeBuffers(BufferCount, (uint)newSize.X, (uint)newSize.Y, ColorFormat, 0));
            CreateBuffersAndViews();
        }

        public override void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = sprites.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector3) + sizeof(Vector4) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector3* positions = (Vector3*)(data);
                Vector4* colours = (Vector4*)(data + vertexCount * sizeof(Vector3));
                ulong* uvws = (ulong*)(data + vertexCount * (sizeof(Vector3) + sizeof(Vector4)));

                // Setup vertices
                int count = sprites.Count;
                for (int i = 0; i < count; ++i)
                {
                    Sprite sprite = sprites[i];
                    VectorInt3 texPos = textureAllocator.Get(sprite.Texture);
                    VectorInt2 texSize = sprite.Texture.To - sprite.Texture.From;
                    float depth = sprite.Depth.HasValue ? sprite.Depth.Value : 1.0f;

                    positions[i * 6 + 0] = new Vector3(sprite.Pos00.X, sprite.Pos00.Y, depth);
                    positions[i * 6 + 2] = positions[i * 6 + 3] = new Vector3(sprite.Pos10.X, sprite.Pos10.Y, depth);
                    positions[i * 6 + 1] = positions[i * 6 + 4] = new Vector3(sprite.Pos01.X, sprite.Pos01.Y, depth);
                    positions[i * 6 + 5] = new Vector3(sprite.Pos11.X, sprite.Pos11.Y, depth);
                    uvws[i * 6 + 1] = uvws[i * 6 + 4] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                    uvws[i * 6 + 5] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                    uvws[i * 6 + 0] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                    uvws[i * 6 + 2] = uvws[i * 6 + 3] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));

                    for (int j = 0; j < 6; j++)
                        colours[i * 6 + j] = sprite.Tint;
                }

                // Create GPU resources
                BufferDesc bufferDesc = new BufferDesc
                {
                    ByteWidth = (uint)bufferSize,
                    Usage = Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0
                };
                SubresourceData initData = new SubresourceData { PSysMem = data, SysMemPitch = 0, SysMemSlicePitch = 0 };

                ID3D11Buffer* pVB = null;
                SilkMarshal.ThrowHResult(
                    Device.Device.Handle->CreateBuffer(&bufferDesc, &initData, &pVB));
                try
                {
                    ID3D11Buffer** ppBuffers = stackalloc ID3D11Buffer*[3] { pVB, pVB, pVB };
                    uint* pStrides = stackalloc uint[3] { (uint)sizeof(Vector3), (uint)sizeof(Vector4), (uint)sizeof(ulong) };
                    uint* pOffsets = stackalloc uint[3]
                    {
                        (uint)((byte*)positions - data),
                        (uint)((byte*)colours - data),
                        (uint)((byte*)uvws - data)
                    };

                    var pContext = Device.Context.Handle;

                    // Render
                    Bind();
                    Device.SpriteShader.Apply(pContext);

                    ID3D11SamplerState* pSampler = linearFilter ? Device.SamplerDefault.Handle : Device.SamplerRoundToNearest.Handle;
                    pContext->PSSetSamplers(0, 1, &pSampler);

                    ID3D11ShaderResourceView* pSRV = ((Dx11RenderingTextureAllocator)textureAllocator).TextureView.Handle;
                    pContext->PSSetShaderResources(0, 1, &pSRV);

                    pContext->IASetVertexBuffers(0, 3, ppBuffers, pStrides, pOffsets);

                    if (noZ)
                        pContext->OMSetDepthStencilState(Device.DepthStencilNoZBuffer.Handle, 0);
                    else
                        pContext->OMSetDepthStencilState(Device.DepthStencilDefault.Handle, 0);

                    pContext->Draw((uint)vertexCount, 0);

                    // Reset state
                    pContext->OMSetDepthStencilState(Device.DepthStencilDefault.Handle, 0);
                }
                finally
                {
                    pVB->Release();
                }
            }
        }

        public override void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2);
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector2) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector2* positions = (Vector2*)(data);
                ulong* uvws = (ulong*)(data + vertexCount * sizeof(Vector2));

                // Setup vertices
                int c = 0;
                for (int i = 0; i < overlays.Count; ++i, ++c)
                {
                    var overlay = overlays[i];
                    Vector2 posStart = overlay.Start * posScaling + posOffset;
                    Vector2 posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

                    positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[c * 6 + 2] = uvws[c * 6 + 3] =
                    uvws[c * 6 + 1] = uvws[c * 6 + 4] =
                    uvws[c * 6 + 5] = uvws[c * 6 + 0] = Dx11RenderingDevice.CompressUvw(VectorInt3.Zero, Vector2.Zero, Vector2.Zero, 1);
                }

                for (int i = 0; i < glyphRenderInfos.Count; ++i, ++c)
                {
                    RenderingFont.GlyphRenderInfo info = glyphRenderInfos[i];
                    Vector2 posStart = info.PosStart * posScaling + posOffset;
                    Vector2 posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                    positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[c * 6 + 0] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, Vector2.Zero);
                    uvws[c * 6 + 2] = uvws[c * 6 + 3] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, 0));
                    uvws[c * 6 + 1] = uvws[c * 6 + 4] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(0, info.TexSize.Y - 1));
                    uvws[c * 6 + 5] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, info.TexSize.Y - 1));
                }

                // Create GPU resources
                BufferDesc bufferDesc = new BufferDesc
                {
                    ByteWidth = (uint)bufferSize,
                    Usage = Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0
                };
                SubresourceData initData = new SubresourceData { PSysMem = data, SysMemPitch = 0, SysMemSlicePitch = 0 };

                ID3D11Buffer* pVB = null;
                SilkMarshal.ThrowHResult(
                    Device.Device.Handle->CreateBuffer(&bufferDesc, &initData, &pVB));
                try
                {
                    ID3D11Buffer** ppBuffers = stackalloc ID3D11Buffer*[2] { pVB, pVB };
                    uint* pStrides = stackalloc uint[2] { (uint)sizeof(Vector2), (uint)sizeof(ulong) };
                    uint* pOffsets = stackalloc uint[2]
                    {
                        (uint)((byte*)positions - data),
                        (uint)((byte*)uvws - data)
                    };

                    var pContext = Device.Context.Handle;

                    // Render
                    Bind();
                    Device.TextShader.Apply(pContext);

                    ID3D11SamplerState* pSampler = Device.SamplerRoundToNearest.Handle;
                    pContext->PSSetSamplers(0, 1, &pSampler);

                    ID3D11ShaderResourceView* pSRV = ((Dx11RenderingTextureAllocator)textureAllocator).TextureView.Handle;
                    pContext->PSSetShaderResources(0, 1, &pSRV);

                    pContext->IASetVertexBuffers(0, 2, ppBuffers, pStrides, pOffsets);
                    pContext->OMSetDepthStencilState(Device.DepthStencilNoZBuffer.Handle, 0);

                    pContext->Draw((uint)vertexCount, 0);

                    // Reset state
                    pContext->OMSetDepthStencilState(Device.DepthStencilDefault.Handle, 0);
                }
                finally
                {
                    pVB->Release();
                }
            }
        }
    }
}

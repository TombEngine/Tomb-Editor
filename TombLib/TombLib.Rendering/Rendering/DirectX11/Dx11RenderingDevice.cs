using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TombLib.Utils;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int SectorTextureSize = 256;
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public static ImageC TextureUnavailable = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_unavailable.png"));
        public static ImageC TextureCoordOutOfBounds = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_coord_out_of_bounds.png"));

        // API entry points
        public readonly D3D11 D3D11Api;
        public readonly DXGI DXGIApi;

        // Core D3D11 objects
        public ComPtr<ID3D11Device> Device;
        public ComPtr<IDXGIFactory1> Factory;
        public ComPtr<ID3D11DeviceContext> Context;

        // Pipeline states / shaders
        public readonly Dx11PipelineState TextShader;
        public readonly Dx11PipelineState SpriteShader;
        public readonly Dx11PipelineState RoomShader;

        // Rendering states
        public ComPtr<ID3D11RasterizerState> RasterizerBackCulling;
        public ComPtr<ID3D11SamplerState> SamplerDefault;
        public ComPtr<ID3D11SamplerState> SamplerRoundToNearest;
        public ComPtr<ID3D11DepthStencilState> DepthStencilDefault;
        public ComPtr<ID3D11DepthStencilState> DepthStencilNoZBuffer;
        public ComPtr<ID3D11BlendState> BlendingDisabled;
        public ComPtr<ID3D11BlendState> BlendingPremultipliedAlpha;

        // Sector textures
        public ComPtr<ID3D11Texture2D> SectorTextureArray;
        public ComPtr<ID3D11ShaderResourceView> SectorTextureArrayView;

        public Dx11RenderingSwapChain CurrentRenderTarget = null;

        /// <summary>Returns the native ID3D11Device pointer (for interop with legacy systems).</summary>
        public IntPtr NativeDevicePointer => (IntPtr)Device.Handle;

        public Dx11RenderingDevice()
        {
            logger.Info("Dx11 rendering device creating.");

            D3D11Api = D3D11.GetApi();
            DXGIApi = DXGI.GetApi();

#if DEBUG
            const uint DebugFlags = (uint)CreateDeviceFlag.Debug;
#else
            const uint DebugFlags = 0;
#endif

            IDXGIAdapter* pAdapter = null;
            try
            {
                // Create DXGI factory
                IDXGIFactory1* pFactory = null;
                SilkMarshal.ThrowHResult(
                    DXGIApi.CreateDXGIFactory1(SilkMarshal.GuidPtrOf<IDXGIFactory1>(), (void**)&pFactory));
                Factory = new ComPtr<IDXGIFactory1>(pFactory);

                // Enumerate adapters
                int adapterHr = pFactory->EnumAdapters(0, &pAdapter);
                if (adapterHr < 0 || pAdapter == null)
                {
                    MessageBox.Show("Your system have no video adapters. Try to install video adapter.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("There are no valid video adapters in system.");
                }

                // Get adapter description
                AdapterDesc adapterDesc;
                pAdapter->GetDesc(&adapterDesc);
                string adapterName = new string((char*)adapterDesc.Description);
                long vramMB = (long)(ulong)adapterDesc.DedicatedVideoMemory / 1024 / 1024;

                // Check for outputs
                IDXGIOutput* pOutput = null;
                int outputHr = pAdapter->EnumOutputs(0, &pOutput);
                if (outputHr < 0 || pOutput == null)
                {
                    MessageBox.Show("There are no video displays connected to your system. Try to connect a display.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("No connected displays found.");
                }
                if (pOutput != null) pOutput->Release();

                logger.Info("Creating D3D device: " + adapterName + ", " + vramMB + " MB GPU RAM.");

                D3DFeatureLevel requestedLevel = D3DFeatureLevel.Level100;
                D3DFeatureLevel actualLevel;
                ID3D11Device* pDevice = null;
                ID3D11DeviceContext* pContext = null;

                SilkMarshal.ThrowHResult(
                    D3D11Api.CreateDevice(
                        pAdapter,
                        D3DDriverType.Unknown, // Using specific adapter
                        0,
                        DebugFlags | (uint)CreateDeviceFlag.Singlethreaded,
                        &requestedLevel, 1,
                        D3D11.SdkVersion,
                        &pDevice,
                        &actualLevel,
                        &pContext));

                Device = new ComPtr<ID3D11Device>(pDevice);
                Context = new ComPtr<ID3D11DeviceContext>(pContext);
            }
            catch (Exception exc)
            {
                // Clean up Factory on failure — constructor throws, so Dispose() won't be called.
                Factory.Dispose();

                uint hresult = unchecked((uint)exc.HResult);
                switch (hresult)
                {
                    case 0x887A0004:
                        MessageBox.Show("Your DirectX version, videocard or drivers are out of date.\nDirectX 11 installation and videocard with DirectX 10 support is required.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 0x887A002D:
                        MessageBox.Show("Warning: provided build is a debug build.\nPlease install DirectX SDK or request release build from QA team.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 0x887A0005:
                    case 0x887A0020:
                        MessageBox.Show("There was a serious video system error while initializing Direct3D device.\nTry to restart your system.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        if (hresult >= 0x88700000 && hresult <= 0x887FFFFF) // DXGI/D3D error range
                            MessageBox.Show("Unknown error while creating Direct3D device!\nShutting down now.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                throw new Exception("Can't create Direct3D 11 device! Exception: " + exc);
            }
            finally
            {
                if (pAdapter != null) pAdapter->Release();
            }

#if DEBUG
            try
            {
                ID3D11InfoQueue* pInfoQueue = null;
                Guid infoQueueGuid = ID3D11InfoQueue.Guid;
                int hr = Device.Handle->QueryInterface(&infoQueueGuid, (void**)&pInfoQueue);
                if (hr >= 0 && pInfoQueue != null)
                {
                    pInfoQueue->SetBreakOnSeverity(MessageSeverity.Warning, 1);
                    pInfoQueue->Release();
                }
            }
            catch { /* Info queue may not be available */ }
#endif

            try
            {
                var pDevice = Device.Handle;
                var pContext = Context.Handle;

                // Text shader
                byte[] positionSemantic = Encoding.ASCII.GetBytes("POSITION\0");
                byte[] uvwSemantic = Encoding.ASCII.GetBytes("UVW\0");
                byte[] colorSemantic = Encoding.ASCII.GetBytes("COLOR\0");
                byte[] overlaySemantic = Encoding.ASCII.GetBytes("OVERLAY\0");
                byte[] uvwBlendSemantic = Encoding.ASCII.GetBytes("UVWANDBLENDMODE\0");
                byte[] editorUvSemantic = Encoding.ASCII.GetBytes("EDITORUVANDSECTORTEXTURE\0");

                fixed (byte* pPosition = positionSemantic)
                fixed (byte* pUvw = uvwSemantic)
                fixed (byte* pColor = colorSemantic)
                fixed (byte* pOverlay = overlaySemantic)
                fixed (byte* pUvwBlend = uvwBlendSemantic)
                fixed (byte* pEditorUv = editorUvSemantic)
                {
                    TextShader = new Dx11PipelineState(this, "TextShader", new InputElementDesc[]
                    {
                        new InputElementDesc { SemanticName = pPosition, SemanticIndex = 0, Format = Format.FormatR32G32Float, InputSlot = 0, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pUvw, SemanticIndex = 0, Format = Format.FormatR32G32Uint, InputSlot = 1, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 }
                    });
                    SpriteShader = new Dx11PipelineState(this, "SpriteShader", new InputElementDesc[]
                    {
                        new InputElementDesc { SemanticName = pPosition, SemanticIndex = 0, Format = Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pColor, SemanticIndex = 0, Format = Format.FormatR32G32B32A32Float, InputSlot = 1, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pUvw, SemanticIndex = 0, Format = Format.FormatR32G32Uint, InputSlot = 2, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 }
                    });
                    RoomShader = new Dx11PipelineState(this, "RoomShader", new InputElementDesc[]
                    {
                        new InputElementDesc { SemanticName = pPosition, SemanticIndex = 0, Format = Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pColor, SemanticIndex = 0, Format = Format.FormatR8G8B8A8Unorm, InputSlot = 1, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pOverlay, SemanticIndex = 0, Format = Format.FormatR8G8B8A8Unorm, InputSlot = 2, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pUvwBlend, SemanticIndex = 0, Format = Format.FormatR32G32Uint, InputSlot = 3, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
                        new InputElementDesc { SemanticName = pEditorUv, SemanticIndex = 0, Format = Format.FormatR32Uint, InputSlot = 4, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 }
                    });
                }

                // Rasterizer state
                {
                    RasterizerDesc desc = new RasterizerDesc
                    {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                        FrontCounterClockwise = 0,
                        DepthBias = 0,
                        DepthBiasClamp = 0,
                        SlopeScaledDepthBias = 0,
                        DepthClipEnable = 1,
                        ScissorEnable = 0,
                        MultisampleEnable = 0,
                        AntialiasedLineEnable = 0
                    };
                    ID3D11RasterizerState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateRasterizerState(&desc, &pState));
                    RasterizerBackCulling = new ComPtr<ID3D11RasterizerState>(pState);
                }

                // Sampler states
                {
                    SamplerDesc desc = new SamplerDesc
                    {
                        Filter = Filter.Anisotropic,
                        AddressU = TextureAddressMode.Mirror,
                        AddressV = TextureAddressMode.Mirror,
                        AddressW = TextureAddressMode.Wrap,
                        MaxAnisotropy = 4,
                        MinLOD = 0,
                        MaxLOD = float.MaxValue,
                        ComparisonFunc = ComparisonFunc.Never,
                        MipLODBias = 0
                    };
                    ID3D11SamplerState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateSamplerState(&desc, &pState));
                    SamplerDefault = new ComPtr<ID3D11SamplerState>(pState);
                }
                {
                    SamplerDesc desc = new SamplerDesc
                    {
                        Filter = Filter.MinMagMipPoint,
                        AddressU = TextureAddressMode.Mirror,
                        AddressV = TextureAddressMode.Mirror,
                        AddressW = TextureAddressMode.Wrap,
                        MaxAnisotropy = 4,
                        MinLOD = 0,
                        MaxLOD = float.MaxValue,
                        ComparisonFunc = ComparisonFunc.Never,
                        MipLODBias = 0
                    };
                    ID3D11SamplerState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateSamplerState(&desc, &pState));
                    SamplerRoundToNearest = new ComPtr<ID3D11SamplerState>(pState);
                }

                // Depth stencil states
                {
                    DepthStencilDesc desc = new DepthStencilDesc
                    {
                        DepthEnable = 1,
                        DepthWriteMask = DepthWriteMask.All,
                        DepthFunc = ComparisonFunc.LessEqual,
                        StencilEnable = 0,
                        StencilReadMask = 0xFF,
                        StencilWriteMask = 0xFF,
                    };
                    ID3D11DepthStencilState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateDepthStencilState(&desc, &pState));
                    DepthStencilDefault = new ComPtr<ID3D11DepthStencilState>(pState);
                }
                {
                    DepthStencilDesc desc = new DepthStencilDesc
                    {
                        DepthEnable = 0,
                        DepthWriteMask = DepthWriteMask.Zero,
                        DepthFunc = ComparisonFunc.Always,
                        StencilEnable = 0,
                        StencilReadMask = 0xFF,
                        StencilWriteMask = 0xFF,
                    };
                    ID3D11DepthStencilState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateDepthStencilState(&desc, &pState));
                    DepthStencilNoZBuffer = new ComPtr<ID3D11DepthStencilState>(pState);
                }

                // Blend states
                {
                    BlendDesc desc = new BlendDesc();
                    desc.AlphaToCoverageEnable = 0;
                    desc.IndependentBlendEnable = 0;
                    desc.RenderTarget[0] = new RenderTargetBlendDesc
                    {
                        BlendEnable = 0,
                        SrcBlend = Blend.One,
                        DestBlend = Blend.Zero,
                        BlendOp = BlendOp.Add,
                        SrcBlendAlpha = Blend.One,
                        DestBlendAlpha = Blend.Zero,
                        BlendOpAlpha = BlendOp.Add,
                        RenderTargetWriteMask = (byte)ColorWriteEnable.All
                    };
                    ID3D11BlendState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateBlendState(&desc, &pState));
                    BlendingDisabled = new ComPtr<ID3D11BlendState>(pState);
                }
                {
                    BlendDesc desc = new BlendDesc();
                    desc.AlphaToCoverageEnable = 0;
                    desc.IndependentBlendEnable = 0;
                    desc.RenderTarget[0] = new RenderTargetBlendDesc
                    {
                        BlendEnable = 1,
                        SrcBlend = Blend.One,
                        DestBlend = Blend.InvSrcAlpha,
                        BlendOp = BlendOp.Add,
                        SrcBlendAlpha = Blend.One,
                        DestBlendAlpha = Blend.InvSrcAlpha,
                        BlendOpAlpha = BlendOp.Add,
                        RenderTargetWriteMask = (byte)ColorWriteEnable.All
                    };
                    ID3D11BlendState* pState = null;
                    SilkMarshal.ThrowHResult(pDevice->CreateBlendState(&desc, &pState));
                    BlendingPremultipliedAlpha = new ComPtr<ID3D11BlendState>(pState);
                }
            }
            catch (Exception exc)
            {
                // Clean up already-created COM objects on partial construction failure.
                Context.Dispose();
                Device.Dispose();
                Factory.Dispose();
                throw new Exception("Can't assign needed Direct3D parameters! Exception: " + exc);
            }

            // Sector textures
            {
                uint formatSupport = 0;
                Device.Handle->CheckFormatSupport(Format.FormatB5G5R5A1Unorm, &formatSupport);
                bool support16BitTexture = ((FormatSupport)formatSupport).HasFlag(FormatSupport.Texture2D);

                string[] sectorTextureNames = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
                GCHandle[] handles = new GCHandle[sectorTextureNames.Length];
                try
                {
                    SubresourceData[] dataBoxes = new SubresourceData[sectorTextureNames.Length];
                    for (int i = 0; i < sectorTextureNames.Length; ++i)
                    {
                        string name = nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures." + sectorTextureNames[i] + ".png";
                        using (Stream stream = ThisAssembly.GetManifestResourceStream(name))
                        {
                            ImageC image = ImageC.FromStream(stream);
                            if ((image.Width != SectorTextureSize) || (image.Height != SectorTextureSize))
                                throw new ArgumentOutOfRangeException("The embedded resource '" + name + "' is not of a valid size.");

                            if (support16BitTexture)
                            {
                                ushort[] sectorTextureData = new ushort[SectorTextureSize * SectorTextureSize];
                                for (int j = 0; j < (SectorTextureSize * SectorTextureSize); ++j)
                                {
                                    ColorC Color = image.Get(j);
                                    sectorTextureData[j] = (ushort)(
                                        ((Color.B >> 3) << 0) |
                                        ((Color.G >> 3) << 5) |
                                        ((Color.R >> 3) << 10) |
                                        ((Color.A >> 7) << 15));
                                }
                                handles[i] = GCHandle.Alloc(sectorTextureData, GCHandleType.Pinned);
                                dataBoxes[i] = new SubresourceData
                                {
                                    PSysMem = (void*)handles[i].AddrOfPinnedObject(),
                                    SysMemPitch = (uint)(sizeof(ushort) * SectorTextureSize),
                                    SysMemSlicePitch = 0
                                };
                            }
                            else
                            {
                                handles[i] = GCHandle.Alloc(image.ToByteArray(), GCHandleType.Pinned);
                                dataBoxes[i] = new SubresourceData
                                {
                                    PSysMem = (void*)handles[i].AddrOfPinnedObject(),
                                    SysMemPitch = (uint)(sizeof(uint) * SectorTextureSize),
                                    SysMemSlicePitch = 0
                                };
                            }
                        }
                    }

                    Texture2DDesc texDesc = new Texture2DDesc
                    {
                        Width = SectorTextureSize,
                        Height = SectorTextureSize,
                        MipLevels = 1,
                        ArraySize = (uint)sectorTextureNames.Length,
                        Format = support16BitTexture ? Format.FormatB5G5R5A1Unorm : Format.FormatB8G8R8A8Unorm,
                        SampleDesc = new SampleDesc(1, 0),
                        Usage = Usage.Immutable,
                        BindFlags = (uint)BindFlag.ShaderResource,
                        CPUAccessFlags = 0,
                        MiscFlags = 0
                    };

                    fixed (SubresourceData* pData = dataBoxes)
                    {
                        ID3D11Texture2D* pTex = null;
                        SilkMarshal.ThrowHResult(Device.Handle->CreateTexture2D(&texDesc, pData, &pTex));
                        SectorTextureArray = new ComPtr<ID3D11Texture2D>(pTex);
                    }
                }
                finally
                {
                    foreach (GCHandle handle in handles)
                        if (handle.IsAllocated)
                            handle.Free();
                }

                {
                    ID3D11ShaderResourceView* pSRV = null;
                    SilkMarshal.ThrowHResult(
                        Device.Handle->CreateShaderResourceView((ID3D11Resource*)SectorTextureArray.Handle, (ShaderResourceViewDesc*)null, &pSRV));
                    SectorTextureArrayView = new ComPtr<ID3D11ShaderResourceView>(pSRV);
                }
            }

            // Set omni present state
            ResetState();

            logger.Info("Dx11 rendering device created.");
        }

        public void ResetState()
        {
            var pContext = Context.Handle;
            pContext->RSSetState(RasterizerBackCulling.Handle);
            pContext->OMSetDepthStencilState(DepthStencilDefault.Handle, 0);
            float* blendFactor = stackalloc float[4] { 1f, 1f, 1f, 1f };
            pContext->OMSetBlendState(BlendingPremultipliedAlpha.Handle, blendFactor, 0xFFFFFFFF);
        }

        public override void Dispose()
        {
            try
            {
                var pContext = Context.Handle;
                pContext->ClearState();
                pContext->Flush();
            }
            finally
            {
                SectorTextureArrayView.Dispose();
                SectorTextureArray.Dispose();
                DepthStencilDefault.Dispose();
                DepthStencilNoZBuffer.Dispose();
                BlendingDisabled.Dispose();
                BlendingPremultipliedAlpha.Dispose();
                SamplerDefault.Dispose();
                SamplerRoundToNearest.Dispose();
                RasterizerBackCulling.Dispose();
                RoomShader.Dispose();
                SpriteShader.Dispose();
                TextShader.Dispose();
                Context.Dispose();
                Device.Dispose();
                Factory.Dispose();
                DXGIApi.Dispose();
                D3D11Api.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CompressColor(Vector3 color, float alpha = 1.0f, bool average = true)
        {
            float multiplier = average ? 128.0f : 255.0f;
            color = Vector3.Max(new Vector3(), Vector3.Min(new Vector3(255.0f), color * multiplier + new Vector3(0.5f)));
            return ((uint)color.X) | (((uint)color.Y) << 8) | (((uint)color.Z) << 16) | ((uint)(MathC.Clamp(alpha, 0, 1) * 255.0f) << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CompressUvw(VectorInt3 position, Vector2 textureScaling, Vector2 uv, uint highestBits = 0)
        {
            uint blendMode2 = Math.Min(highestBits, 15);
            uint x = (uint)((position.X + uv.X) * textureScaling.X);
            uint y = (uint)((position.Y + uv.Y) * textureScaling.Y);
            return x | ((ulong)y << 24) | ((ulong)position.Z << 48) | ((ulong)blendMode2 << 60);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorInt3 UncompressUvw(ulong value, Vector2 textureScaling)
        {
            Vector2 uv = new Vector2(value & 0xFFFFFF, (value >> 24) & 0xFFFFFF) / textureScaling;
            int w = (int)((value >> 48) & 0x3FF);
            return new VectorInt3((int)uv.X, (int)uv.Y, w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UncompressUvw(ulong value, VectorInt3 position, Vector2 textureScaling, out Vector2 uv, out uint highestBits)
        {
            uv = new Vector2(value & 0xFFFFFF, (value >> 24) & 0xFFFFFF) / textureScaling.X - new Vector2(position.X, position.Y);
            highestBits = (uint)(value >> 60);
        }

        ///<summary>Works even on immutable buffers</summary>
        public byte[] ReadBuffer(ID3D11Buffer* buffer, int size)
        {
            var pDevice = Device.Handle;
            var pContext = Context.Handle;

            BufferDesc tempDesc = new BufferDesc
            {
                ByteWidth = (uint)size,
                Usage = Usage.Staging,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                MiscFlags = 0,
                StructureByteStride = 0
            };

            ID3D11Buffer* pTempBuffer = null;
            SilkMarshal.ThrowHResult(pDevice->CreateBuffer(&tempDesc, (SubresourceData*)null, &pTempBuffer));
            try
            {
                pContext->CopyResource((ID3D11Resource*)pTempBuffer, (ID3D11Resource*)buffer);
                MappedSubresource mapped;
                SilkMarshal.ThrowHResult(pContext->Map((ID3D11Resource*)pTempBuffer, 0, Map.Read, 0, &mapped));
                try
                {
                    byte[] result = new byte[size];
                    Marshal.Copy((IntPtr)mapped.PData, result, 0, size);
                    return result;
                }
                finally
                {
                    pContext->Unmap((ID3D11Resource*)pTempBuffer, 0);
                }
            }
            finally
            {
                pTempBuffer->Release();
            }
        }

        /// <summary>
        /// Creates a texture description from a VectorInt3 where the convention is:
        /// X = page height, Y = page width, Z = array size (page count).
        /// Note: default pages are square (2048x2048) so the X/Y mapping is interchangeable.
        /// </summary>
        public Texture2DDesc CreateTextureDescription(VectorInt3 size)
        {
            return new Texture2DDesc
            {
                Height = (uint)size.X,
                Width = (uint)size.Y,
                ArraySize = (uint)size.Z,
                BindFlags = (uint)BindFlag.ShaderResource,
                CPUAccessFlags = 0,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Default
            };
        }

        public VectorInt3 GetAvailableTextureAllocatorSize(VectorInt3 size)
        {
            if (size.Z < RenderingTextureAllocator.MinimumPageCount)
                return size;

            logger.Info("Trying to reserve " + size.Z + " " + size.X + "x" + size.Y + " pages of texture memory...");

            var dx11Description = CreateTextureDescription(size);

            while ((int)dx11Description.ArraySize >= RenderingTextureAllocator.MinimumPageCount)
            {
                try
                {
                    ID3D11Texture2D* pTest = null;
                    int hr = Device.Handle->CreateTexture2D(&dx11Description, (SubresourceData*)null, &pTest);
                    if (hr >= 0 && pTest != null)
                    {
                        pTest->Release();
                        logger.Info(dx11Description.ArraySize + " texture pages were successfully reserved.");
                        return new VectorInt3(size.X, size.Y, (int)dx11Description.ArraySize);
                    }
                    else
                    {
                        logger.Warn("Not enough memory to allocate " + dx11Description.ArraySize + " texture pages. Trying to reduce page count...");
                        dx11Description.ArraySize -= 2;
                    }
                }
                catch
                {
                    logger.Warn("Not enough memory to allocate " + dx11Description.ArraySize + " texture pages. Trying to reduce page count...");
                    dx11Description.ArraySize -= 2;
                }
            }

            throw new NotSupportedException("Video system does not have enough video memory to create texture array. Please upgrade your graphics adapter.");
        }

        public override RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description)
        {
            return new Dx11RenderingSwapChain(this, description);
        }

        public override RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description)
        {
            return new Dx11RenderingDrawingTest(this, description);
        }

        public override RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description)
        {
            return new Dx11RenderingDrawingRoom(this, description);
        }

        public override RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description)
        {
            description.Size = GetAvailableTextureAllocatorSize(description.Size);
            return new Dx11RenderingTextureAllocator(this, description);
        }

        public override RenderingFont CreateFont(RenderingFont.Description description)
        {
            return new RenderingFont(description);
        }

        public override RenderingStateBuffer CreateStateBuffer()
        {
            return new Dx11RenderingStateBuffer(this);
        }
    }

    public static class Dx11RenderingDeviceDebugging
    {
        private static readonly Guid WKPDID_D3DDebugObjectName = new Guid("429b8c22-9188-4b0c-8742-acb0bf85c200");

        public static unsafe void SetDebugName(ID3D11DeviceChild* child, string debugName)
        {
            if (child == null) return;
            byte[] debugNameBytes = Encoding.ASCII.GetBytes(debugName);
            fixed (byte* pName = debugNameBytes)
            {
                Guid guid = WKPDID_D3DDebugObjectName;
                child->SetPrivateData(&guid, (uint)debugNameBytes.Length, pName);
            }
        }
    }
}

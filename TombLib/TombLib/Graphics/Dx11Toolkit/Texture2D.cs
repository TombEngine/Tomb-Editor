using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Wraps a D3D11 Texture2D + ShaderResourceView using Silk.NET COM pointers.
    /// </summary>
    public unsafe class Texture2D : IDisposable
    {
        internal ID3D11Texture2D* NativeTexture { get; private set; }
        internal ID3D11ShaderResourceView* NativeSRV { get; private set; }

        public int Width { get; }
        public int Height { get; }
        public int MipLevels { get; }
        public int ArraySize { get; }
        public DxgiFormat Format { get; }

        private Texture2D(ID3D11Texture2D* texture, ID3D11ShaderResourceView* srv,
            int width, int height, int mipLevels, int arraySize, DxgiFormat format)
        {
            NativeTexture = texture;
            NativeSRV = srv;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArraySize = arraySize;
            Format = format;
        }

        /// <summary>Creates a Texture2D with optional initial data.</summary>
        public static Texture2D New(GraphicsDevice device, int width, int height, int mipLevels,
            DxgiFormat format, DataBox[] data, TextureFlags flags, int arraySize = 1,
            ResourceUsage usage = ResourceUsage.Default)
        {
            var silkFormat = (Format)(int)format;
            uint bindFlags = 0;
            if (flags.HasFlag(TextureFlags.ShaderResource)) bindFlags |= (uint)BindFlag.ShaderResource;
            if (flags.HasFlag(TextureFlags.RenderTarget)) bindFlags |= (uint)BindFlag.RenderTarget;
            if (flags.HasFlag(TextureFlags.UnorderedAccess)) bindFlags |= (uint)BindFlag.UnorderedAccess;

            var desc = new Texture2DDesc
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = (uint)mipLevels,
                ArraySize = (uint)arraySize,
                Format = silkFormat,
                SampleDesc = new SampleDesc(1, 0),
                Usage = (Usage)(int)usage,
                BindFlags = bindFlags,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            ID3D11Texture2D* texture;
            if (data != null && data.Length > 0)
            {
                // Convert our DataBox[] to Silk.NET SubresourceData[]
                var initData = stackalloc SubresourceData[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    initData[i] = new SubresourceData
                    {
                        PSysMem = (void*)data[i].DataPointer,
                        SysMemPitch = (uint)data[i].RowPitch,
                        SysMemSlicePitch = (uint)data[i].SlicePitch,
                    };
                }
                int hr = device.NativeDevice->CreateTexture2D(&desc, initData, &texture);
                Marshal.ThrowExceptionForHR(hr);
            }
            else
            {
                int hr = device.NativeDevice->CreateTexture2D(&desc, null, &texture);
                Marshal.ThrowExceptionForHR(hr);
            }

            ID3D11ShaderResourceView* srv = null;
            if (flags.HasFlag(TextureFlags.ShaderResource))
            {
                ShaderResourceViewDesc srvDesc = default;
                srvDesc.Format = silkFormat;

                if (arraySize > 1)
                {
                    srvDesc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2Darray;
                    srvDesc.Texture2DArray.ArraySize = (uint)arraySize;
                    srvDesc.Texture2DArray.FirstArraySlice = 0;
                    srvDesc.Texture2DArray.MipLevels = mipLevels == 0 ? unchecked((uint)-1) : (uint)mipLevels;
                    srvDesc.Texture2DArray.MostDetailedMip = 0;
                }
                else
                {
                    srvDesc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D;
                    srvDesc.Texture2D.MipLevels = mipLevels == 0 ? unchecked((uint)-1) : (uint)mipLevels;
                    srvDesc.Texture2D.MostDetailedMip = 0;
                }

                int hr = device.NativeDevice->CreateShaderResourceView(
                    (ID3D11Resource*)texture, &srvDesc, &srv);
                Marshal.ThrowExceptionForHR(hr);
            }

            return new Texture2D(texture, srv, width, height, mipLevels, arraySize, format);
        }

        /// <summary>Creates a Texture2D without initial data.</summary>
        public static Texture2D New(GraphicsDevice device, int width, int height,
            DxgiFormat format, TextureFlags flags, int arraySize = 1,
            ResourceUsage usage = ResourceUsage.Default)
        {
            return New(device, width, height, 1, format, null, flags, arraySize, usage);
        }

        /// <summary>Gets the subresource index for a given array slice and mip level.</summary>
        public int GetSubResourceIndex(int arraySlice, int mipLevel)
        {
            return mipLevel + arraySlice * MipLevels;
        }

        /// <summary>Updates a region of the texture via UpdateSubresource.</summary>
        public void SetData(GraphicsDevice device, DataPointer data, int arraySlice, int mipSlice, ResourceRegion region)
        {
            int subresource = GetSubResourceIndex(arraySlice, mipSlice);
            int rowPitch = (region.Right - region.Left) * FormatBytesPerPixel(Format);

            var box = new Box
            {
                Left = (uint)region.Left,
                Top = (uint)region.Top,
                Front = (uint)region.Front,
                Right = (uint)region.Right,
                Bottom = (uint)region.Bottom,
                Back = (uint)region.Back,
            };

            device.NativeContext->UpdateSubresource(
                (ID3D11Resource*)NativeTexture, (uint)subresource,
                &box, (void*)data.Pointer, (uint)rowPitch, 0);
        }

        internal static int FormatBytesPerPixel(DxgiFormat format)
        {
            switch (format)
            {
                case DxgiFormat.B8G8R8A8_UNorm:
                case DxgiFormat.R8G8B8A8_UNorm:
                case DxgiFormat.R32_Float:
                    return 4;
                case DxgiFormat.R32G32_Float:
                case DxgiFormat.R16G16B16A16_Float:
                    return 8;
                case DxgiFormat.R32G32B32_Float:
                    return 12;
                case DxgiFormat.R32G32B32A32_Float:
                    return 16;
                case DxgiFormat.R16_UInt:
                    return 2;
                default:
                    return 4;
            }
        }

        public void Dispose()
        {
            if (NativeSRV != null)
            {
                NativeSRV->Release();
                NativeSRV = null;
            }
            if (NativeTexture != null)
            {
                NativeTexture->Release();
                NativeTexture = null;
            }
        }
    }
}

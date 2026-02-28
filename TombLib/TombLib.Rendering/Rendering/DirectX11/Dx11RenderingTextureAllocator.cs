using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingTextureAllocator : RenderingTextureAllocator
    {
        public readonly ID3D11DeviceContext* Context;
        public ComPtr<ID3D11Texture2D> Texture;
        public ComPtr<ID3D11ShaderResourceView> TextureView;

        public Dx11RenderingTextureAllocator(Dx11RenderingDevice device, Description description)
            : base(device, description)
        {
            Context = device.Context.Handle;

            Texture2DDesc texDesc = device.CreateTextureDescription(description.Size);

            ID3D11Texture2D* pTex = null;
            SilkMarshal.ThrowHResult(
                device.Device.Handle->CreateTexture2D(&texDesc, (SubresourceData*)null, &pTex));
            Texture = new ComPtr<ID3D11Texture2D>(pTex);

            ID3D11ShaderResourceView* pSRV = null;
            SilkMarshal.ThrowHResult(
                device.Device.Handle->CreateShaderResourceView((ID3D11Resource*)pTex, (ShaderResourceViewDesc*)null, &pSRV));
            TextureView = new ComPtr<ID3D11ShaderResourceView>(pSRV);
        }

        public override void Dispose()
        {
            TextureView.Dispose();
            Texture.Dispose();
        }

        protected override void UploadTexture(RenderingTexture texture, VectorInt3 pos)
        {
            var width  = texture.To.X - texture.From.X;
            var height = texture.To.Y - texture.From.Y;

            // Copy original region to new image
            var originalImage = ImageC.CreateNew(width + 2, height + 2);
            originalImage.CopyFrom(1, 1, texture.Image, texture.From.X, texture.From.Y, width, height);

            // Add 1px padding to prevent border bleeding
            originalImage.SetPixel(0, 0, originalImage.GetPixel(1, 1));
            originalImage.SetPixel(width + 1, 0, originalImage.GetPixel(width, 1));
            originalImage.SetPixel(0, height + 1, originalImage.GetPixel(1, height));
            originalImage.SetPixel(width + 1, height + 1, originalImage.GetPixel(width, height));
            originalImage.CopyFrom(0, 1, originalImage, 1, 1, 1, height);
            originalImage.CopyFrom(width + 1, 1, originalImage, width, 1, 1, height);
            originalImage.CopyFrom(1, 0, originalImage, 1, 1, width, 1);
            originalImage.CopyFrom(1, height + 1, originalImage, 1, height, width, 1);

            originalImage.GetIntPtr(ptr =>
            {
                const int mipLevelToUpload = 0;
                int subresourceIndex = pos.Z + mipLevelToUpload;

                int left = Math.Max(pos.X, 0);
                int right = Math.Min(pos.X + originalImage.Width, Size.X);
                int top = Math.Max(pos.Y, 0);
                int bottom = Math.Min(pos.Y + originalImage.Height, Size.Y);

                if (0 > left || left >= right || right > Size.X ||
                    0 > top || top >= bottom || bottom > Size.Y)
                {
                    throw new ArgumentOutOfRangeException("texture.From.X = " + texture.From.X + ", " +
                                                          "texture.From.Y = " + texture.From.Y + ", " +
                                                          "texture.To.X = " + texture.To.X + ", " +
                                                          "texture.To.Y = " + texture.To.Y + ", " +
                                                          "pos.X = " + pos.X + ", " +
                                                          "pos.Y = " + pos.Y + ", " +
                                                          "region.Left = " + left + ", " +
                                                          "region.Right = " + right + ", " +
                                                          "region.Top = " + top + ", " +
                                                          "region.Bottom = " + bottom);
                }

                Box region = new Box
                {
                    Left = (uint)left,
                    Right = (uint)right,
                    Top = (uint)top,
                    Bottom = (uint)bottom,
                    Front = 0,
                    Back = 1
                };

                Context->UpdateSubresource(
                    (ID3D11Resource*)Texture.Handle,
                    (uint)subresourceIndex,
                    &region,
                    (void*)ptr,
                    (uint)(originalImage.Width * ImageC.PixelSize),
                    0);
            });
        }

        public override ImageC RetrieveTestImage()
        {
            const int mipLevelToRetrieve = 0;

            Texture2DDesc dx11Description = new Texture2DDesc
            {
                ArraySize = 1,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                Format = Format.FormatB8G8R8A8Unorm,
                Height = (uint)(Size.X >> mipLevelToRetrieve),
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Staging,
                Width = (uint)(Size.Y >> mipLevelToRetrieve)
            };

            ID3D11Device* pDevice;
            Context->GetDevice(&pDevice);

            ID3D11Texture2D* pTempTexture = null;
            try
            {
                SilkMarshal.ThrowHResult(pDevice->CreateTexture2D(&dx11Description, (SubresourceData*)null, &pTempTexture));
            }
            finally
            {
                pDevice->Release();
            }

            try
            {
                int width = Size.Y >> mipLevelToRetrieve;   // Size convention: X=height, Y=width
                int height = Size.X >> mipLevelToRetrieve;
                int rowBytes = width * ImageC.PixelSize;
                int bytesPerSlice = rowBytes * height;
                byte[] result = new byte[bytesPerSlice * Size.Z];
                for (int z = 0; z < Size.Z; ++z)
                {
                    int subresourceIndex = z + mipLevelToRetrieve;
                    Context->CopySubresourceRegion(
                        (ID3D11Resource*)pTempTexture, 0, 0, 0, 0,
                        (ID3D11Resource*)Texture.Handle, (uint)subresourceIndex, (Box*)null);

                    MappedSubresource mapped;
                    SilkMarshal.ThrowHResult(
                        Context->Map((ID3D11Resource*)pTempTexture, 0, Silk.NET.Direct3D11.Map.Read, 0, &mapped));
                    try
                    {
                        // Copy row-by-row to handle GPU row pitch padding.
                        int dstOffset = bytesPerSlice * z;
                        if (mapped.RowPitch == (uint)rowBytes)
                        {
                            Marshal.Copy((IntPtr)mapped.PData, result, dstOffset, bytesPerSlice);
                        }
                        else
                        {
                            byte* src = (byte*)mapped.PData;
                            for (int row = 0; row < height; row++)
                            {
                                Marshal.Copy((IntPtr)(src + row * mapped.RowPitch), result, dstOffset + row * rowBytes, rowBytes);
                            }
                        }
                    }
                    finally
                    {
                        Context->Unmap((ID3D11Resource*)pTempTexture, 0);
                    }
                }
                return ImageC.FromByteArray(result, Size.X >> mipLevelToRetrieve, (Size.Y >> mipLevelToRetrieve) * Size.Z);
            }
            finally
            {
                pTempTexture->Release();
            }
        }
    }
}

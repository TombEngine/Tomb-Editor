using Assimp;
using bzPSD;
using ColorThiefDotNet;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Xml.Linq;
using TombLib.LevelData;

namespace TombLib.Utils
{
    public enum SobelFilterType
    {
        Sobel,
        Scharr
    }

    public enum ImageChannel : int
    {
        R,
        G,
        B,
        A
    }

    public struct ColorC
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;
        public ColorC(byte r, byte g, byte b, byte a = 255)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public static bool operator ==(ColorC first, ColorC second) => first.R == second.R && first.G == second.G && first.B == second.B && first.A == second.A;
        public static bool operator !=(ColorC first, ColorC second) => first.R != second.R || first.G != second.G || first.B != second.B || first.A != second.A;

        public static implicit operator System.Drawing.Color(ColorC this_)
        {
            return System.Drawing.Color.FromArgb(255, this_.R, this_.G, this_.B);
        }

        public static implicit operator ColorC(System.Drawing.Color this_)
        {
            return new ColorC(this_.R, this_.G, this_.B);
        }

        public static implicit operator Vector4(ColorC this_)
        {
            const float _floatFactor = 1.0f / 255.0f;
            return new Vector4(this_.R * _floatFactor, this_.G * _floatFactor, this_.B * _floatFactor, this_.A * _floatFactor);
        }

        public static explicit operator ColorC(Vector4 this_)
        {
            this_ = Vector4.Min(Vector4.Max(this_ * 255.99998f, new Vector4()), new Vector4(255.0f));
            return new ColorC((byte)this_.X, (byte)this_.Y, (byte)this_.Z, (byte)this_.W);
        }

        public static implicit operator Vector3(ColorC this_)
        {
            const float _floatFactor = 1.0f / 255.0f;
            return new Vector3(this_.R * _floatFactor, this_.G * _floatFactor, this_.B * _floatFactor);
        }

        public static explicit operator ColorC(Vector3 this_)
        {
            this_ = Vector3.Min(Vector3.Max(this_ * 255.99998f, new Vector3()), new Vector3(255.0f));
            return new ColorC((byte)this_.X, (byte)this_.Y, (byte)this_.Z);
        }

        public static Vector4 Mix(Vector4 background, Vector4 foreground)
        {
            // https://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
            float backgroundTotal = background.W * (1.0f - foreground.W);
            float alpha = foreground.W + backgroundTotal;
            Vector4 output = foreground * foreground.W + background * backgroundTotal;
            if (alpha > float.Epsilon)
                output /= alpha;
            output.W = alpha;
            return output;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    // A very simple but very efficient image that is independent of GDI+.
    // This structure is under the control of the garbage collector and therefore does not need a Dispose() call.
    // Pixel format
    //   [0]: Blue
    //   [1]: Green
    //   [2]: Red
    //   [3]: Alpha
    public struct ImageC : IEquatable<ImageC>
    {
        public static ImageC Magenta { get; } = new ImageC(1, 1, new byte[] { 0xFF, 0, 0xFF, 0xFF });
        public static ImageC Red { get; } = new ImageC(1, 1, new byte[] { 0, 0, 0xFF, 0xFF });
        public static ImageC Transparent { get; } = new ImageC(1, 1, new byte[] { 0, 0, 0, 0 });
        public static ImageC Black { get; } = new ImageC(1, 1, new byte[] { 0, 0, 0, 0xFF }) { FileName = "Black colour" };
        public static uint AlphaBits = ColorToUint(new ColorC(0, 0, 0, 255));
        public const int PixelSize = 4;

        public string FileName { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<ColorC> Palette { get; private set; }
        private byte[] _data { get; set; }

        private ImageC(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            _data = data;
            FileName = "";
            Palette = new List<ColorC>();
        }

        public static bool operator ==(ImageC first, ImageC second) =>
            first.Width == second.Width && first.Height == second.Height && first._data == second._data;
        public static bool operator !=(ImageC first, ImageC second) => !(first == second);
        public bool Equals(ImageC other) => this == other;
        public override bool Equals(object other) => other is ImageC && this == (ImageC)other;
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => "Image (Width=" + Width + ", Height=" + Height + ")";

        public static ImageC CreateNew(int width, int height)
        {
            return new ImageC(width, height, new byte[width * height * PixelSize]);
        }

        public Rectangle2 GetRect()
        {
            return new Rectangle2(0, 0, Width - 1, Height - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ColorC Get(int i)
        {
            int index = i * PixelSize;
            if ((uint)(index + 3) >= (uint)_data.Length)
                return new ColorC(255, 0, 0);

            uint value = Unsafe.ReadUnaligned<uint>(ref _data[index]);

            return new ColorC
            {
                B = (byte)(value & 0xFF),
                G = (byte)((value >> 8) & 0xFF),
                R = (byte)((value >> 16) & 0xFF),
                A = (byte)((value >> 24) & 0xFF)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int i, byte r, byte g, byte b, byte a = 255)
        {
            int index = i * PixelSize;
            if ((uint)(index + 3) >= (uint)_data.Length)
                return;

            // BGRA in little-endian
            uint value = (uint)(b | (g << 8) | (r << 16) | (a << 24));

            Unsafe.WriteUnaligned(ref _data[index], value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int i, ColorC color)
        {
            Set(i, color.R, color.G, color.B, color.A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ColorC GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Width - 1);
            y = Math.Clamp(y, 0, Height - 1);

            int index = ((y * Width) + x) * PixelSize;

            uint value = Unsafe.ReadUnaligned<uint>(ref _data[index]);

            return new ColorC
            {
                B = (byte)value,
                G = (byte)(value >> 8),
                R = (byte)(value >> 16),
                A = (byte)(value >> 24)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
                return;

            int index = (y * Width + x) * 4;
            if ((uint)(index + 3) >= (uint)_data.Length)
                return;

            uint packed = (uint)(b | (g << 8) | (r << 16) | (a << 24));
            Unsafe.WriteUnaligned(ref _data[index], packed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, ColorC color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(ColorC color)
        {
            uint packed = (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));

            // JIT runtime uses SIMD operations here
            var span = MemoryMarshal.Cast<byte, uint>(_data);
            span.Fill(packed);
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe void SetColorDataForTransparentPixels(ColorC color)
		{
			// Build RGB with alpha = 0 (BGRA packing)
			uint packedRgb = (uint)(color.B | (color.G << 8) | (color.R << 16)); // A=0
			int pixelCount = _data.Length >> 2; // bytes -> pixels

			fixed (byte* pBase8 = _data)
			{
				uint* p = (uint*)pBase8;

				int i = 0;

				if (Avx2.IsSupported)
				{
					var vRgb = Vector256.Create(packedRgb);
					var vAlphaMask = Vector256.Create(0xFF000000u);
					var vZero = Vector256<uint>.Zero;

					// 8 pixels per iteration
					int n8 = pixelCount & ~7;
					for (; i < n8; i += 8)
					{
						var block = Avx.LoadVector256(p + i);
						var a = Avx2.And(block, vAlphaMask);

						// mask = (alpha==0) ? 0xFFFFFFFF : 0x00000000
						var mask = Avx2.CompareEqual(a, vZero);

						// Quick skip if no transparent pixel in this block
						if (Avx2.MoveMask(mask.AsByte()) == 0)
							continue;

						// result = (mask & vRgb) | (~mask & block)
						var repl = Avx2.Or(Avx2.And(mask, vRgb), Avx2.AndNot(mask, block));
						Avx.Store(p + i, repl);
					}

					// SSE2 tail (4 pixels)
					if (Sse2.IsSupported)
					{
						var vRgb128 = Vector128.Create(packedRgb);
						var vAlphaMask128 = Vector128.Create(0xFF000000u);
						var vZero128 = Vector128<uint>.Zero;

						int n4 = i + ((pixelCount - i) & ~3);
						for (; i < n4; i += 4)
						{
							var block = Sse2.LoadVector128(p + i);
							var a = Sse2.And(block, vAlphaMask128);
							var mask = Sse2.CompareEqual(a, vZero128);

							if (Sse2.MoveMask(mask.AsByte()) == 0)
								continue;

							var repl = Sse2.Or(Sse2.And(mask, vRgb128), Sse2.AndNot(mask, block));
							Sse2.Store(p + i, repl);
						}
					}
				}

				// Scalar tail (<=3 pixels or no AVX2/SSE2)
				for (; i < pixelCount; ++i)
				{
					uint px = p[i];
					if ((px & 0xFF000000u) == 0) // alpha == 0
						p[i] = packedRgb;        // replace RGB, keep A=0
				}
			}
		}

		public void CalculatePalette(int colorCount = 256)
        {
            if (colorCount > 256) colorCount = 256; // For some reason it fails with more...
            var colorThief = new ColorThief();

            var palette = new List<QuantizedColor>();
            using (var image = ToBitmap())
                palette = colorThief.GetPalette(image, colorCount, 10, false);

            // Sort colours by YIQ luma so they align nicely
            var sortedPalette = palette.OrderBy(entry => entry.CalculateYiqLuma(entry.Color));

            Palette.Clear();
            foreach (var color in sortedPalette)
                if (color.Color.ToHsl().L > 0.009) // Filter out dark values
                {
                    var newColor = new ColorC(color.Color.R, color.Color.G, color.Color.B);
                    if (!Palette.Contains(newColor)) // Filter out duplicates
                        Palette.Add(newColor);
                }
        }

        public void ApplyKernel(int xStart, int yStart, int width, int height, int weight, int[,] kernel)
        {
            // Avoid potential divide by zero errors which shouldn't happen but some TE users experienced them anyway
            weight = weight == 0 ? -2 : weight;

            ImageC oldImage = new ImageC(width, height, new byte[width * height * 4]);
            oldImage.CopyFrom(0, 0, this, xStart, yStart, width, height);

            int kernel_width = kernel.GetUpperBound(0) + 1;
            int kernel_height = kernel.GetUpperBound(1) + 1;

            for (int x = 0, xReal = xStart; x < width; x++, xReal++)
                for (int y = 0, yReal = yStart; y < height; y++, yReal++)
                {
                    int r = 0, g = 0, b = 0;
                    for (int dx = 0; dx < kernel_width; dx++)
                        for (int dy = 0; dy < kernel_height; dy++)
                        {
                            int sourceX = MathC.Clamp(x + dx, 0, width - 1);
                            int sourceY = MathC.Clamp(y + dy, 0, height - 1);
                            ColorC clr = oldImage.GetPixel(sourceX, sourceY);
                            r += (int)clr.R * kernel[dx, dy];
                            g += (int)clr.G * kernel[dx, dy];
                            b += (int)clr.B * kernel[dx, dy];
                        }
                    r = MathC.Clamp((int)(127 + r / weight), 0, 255);
                    g = MathC.Clamp((int)(127 + g / weight), 0, 255);
                    b = MathC.Clamp((int)(127 + b / weight), 0, 255);
                    SetPixel(xReal, yReal, new ColorC((byte)r, (byte)g, (byte)b));
                }

            // Restore alpha
            for (int x = 0, xReal = xStart; x < width; x++, xReal++)
                for (int y = 0, yReal = yStart; y < height; y++, yReal++)
                {
                    var alpha = oldImage.GetPixel(xReal, yReal).A;
                    var color = GetPixel(xReal, yReal);
                    color.A = alpha;
                    SetPixel(xReal, yReal, color);
                }
        }

        public void Emboss(int xStart, int yStart, int width, int height, int weight, int size)
        {
            size = MathC.Clamp(size, 2, 8);
            int[,] kernel = new int[size, size];
            kernel[0, 0] = -1;
            kernel[size - 1, size - 1] = 1;

            ApplyKernel(xStart, yStart, width, height, weight, kernel);
        }

        public VectorInt2 Size => new VectorInt2(Width, Height);

        public int DataSize => Width * Height * PixelSize;

        private static readonly byte[] Tga2_Signature = new byte[18] { 84, 82, 85, 69, 86, 73, 83, 73, 79, 78, 45, 88, 70, 73, 76, 69, 46, 0 };

        private static bool IsTga(byte[] startBytes)
        {
            // Inspired by the FreeType tga image validation routine
            // "Validate" in PluginTARGE.cpp

            if (startBytes.SequenceEqual(Tga2_Signature))
                return true;

            byte colorMapType = startBytes[1];
            byte imageType = startBytes[2];
            ushort colorMapFirstEntry = BitConverter.ToUInt16(startBytes, 3);
            ushort colorMapLength = BitConverter.ToUInt16(startBytes, 5);
            byte colorMapSize = startBytes[7];
            ushort width = BitConverter.ToUInt16(startBytes, 12);
            ushort height = BitConverter.ToUInt16(startBytes, 14);
            byte pixelDepth = startBytes[16];

            if (colorMapType != 0 && colorMapType != 1)
                return false;
            if (colorMapType == 1)
            {
                if (colorMapFirstEntry >= colorMapLength)
                    return false;
                if (colorMapSize == 0 || colorMapSize > 32)
                    return false;
            }
            if (width == 0 || height == 0)
                return false;

            switch (imageType)
            {
                case 1: // Cmap
                case 2: // RGB
                case 3: // Mono
                case 9: // RLE Cmap
                case 10: // RLE RGB
                case 11: // RLE Mono
                    break;
                default:
                    return false;
            }

            switch (pixelDepth)
            {
                case 8:
                case 16:
                case 24:
                case 32:
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static ImageC FromPfimImage(Pfim.IImage image)
        {
            switch (image.Format)
            {
                case Pfim.ImageFormat.Rgba32:
                    return new ImageC(image.Width, image.Height, image.Data);
                case Pfim.ImageFormat.Rgb24:
                    byte[] data = image.Data;
                    int stride = image.Stride;
                    ImageC result = CreateNew(image.Width, image.Height);
                    for (int y = 0; y < result.Height; ++y)
                    {
                        int inputIndex = y * stride;
                        int outputIndex = y * result.Width * PixelSize;

                        for (int x = 0; x < result.Width; ++x)
                        {
                            result._data[outputIndex + 0] = data[inputIndex + 0];
                            result._data[outputIndex + 1] = data[inputIndex + 1];
                            result._data[outputIndex + 2] = data[inputIndex + 2];
                            result._data[outputIndex + 3] = 0xff;

                            inputIndex += 3;
                            outputIndex += 4;
                        }
                    }
                    return result;
                default:
                    throw new NotImplementedException("Pfim image library type " + image.Format + " not handled!");
            }
        }

        public static ImageC FromStream(Stream stream)
        {
            long PreviousPosition = stream.Position;

            // Read some start bytes
            long startPos = stream.Position;
            byte[] startBytes = new byte[18];
            stream.Read(startBytes, 0, 18);
            stream.Position = startPos;

            // Detect special image types
            if (startBytes[0] == 0x44 && startBytes[1] == 0x44 && startBytes[2] == 0x53 && startBytes[3] == 0x20)
            { // dds image
                return FromPfimImage(Pfim.Dds.Create(stream, new Pfim.PfimConfig()));
            }
            else if (startBytes[0] == 0x38 && startBytes[1] == 0x42 && startBytes[2] == 0x50 && startBytes[3] == 0x53)
            { // psd image
                PsdFile image = new PsdFile();
                image.Load(stream);
                using (Image image2 = ImageDecoder.DecodeImage(image))
                    return FromSystemDrawingImage(image2);
            }
            else if (IsTga(startBytes))
            { // tga image
                return FromPfimImage(Pfim.Targa.Create(stream, new Pfim.PfimConfig()));
            }
            else
            { // other image
                using (Image image = Image.FromStream(stream))
                    return FromSystemDrawingImage(image);
            }
        }

        public static ImageC FromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Image file " + path + " not found!");

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var result = FromStream(stream);
                result.FileName = path;
                return result;
            }
        }

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Portable Network Graphics", "png"),
            new FileFormat("Truevision Targa", "tga"),
            new FileFormat("Windows Bitmap", "bmp", "dib"),
            new FileFormat("Jpeg Image", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi"),
            new FileFormat("Graphics Interchange Format", "gif"),
            new FileFormat("Photoshop File", "psd"),
            new FileFormat("Windows Meta File", "wmf", "emf")
        };

        public static IReadOnlyList<FileFormat> SaveFileFileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Portable Network Graphics", "png"),
            new FileFormat("Windows Bitmap", "bmp", "dib"),
            new FileFormat("Jpeg Image", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi"),
            new FileFormat("Graphics Interchange Format", "gif")
        };

        private static ImageC FromSystemDrawingBitmapMatchingPixelFormat(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                ImageC result = CreateNew(bitmap.Width, bitmap.Height);
                Marshal.Copy(bitmapData.Scan0, result._data, 0, bitmap.Width * bitmap.Height * PixelSize);
                return result;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static ImageC FromSystemDrawingImage(Image image)
        {
            Bitmap imageAsBitmap = image as Bitmap;
            if (imageAsBitmap != null && imageAsBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                return FromSystemDrawingBitmapMatchingPixelFormat(imageAsBitmap);

            using (var convertedBitmap = new Bitmap(image.Width, image.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(convertedBitmap))
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                return FromSystemDrawingBitmapMatchingPixelFormat(convertedBitmap);
            }
        }

        public static ImageC FromStreamRaw(Stream stream, int width, int height)
        {
            ImageC result = CreateNew(width, height);
            stream.Read(result._data, 0, width * height * PixelSize);
            return result;
        }

        public static ImageC FromByteArray(byte[] data, int width, int height)
        {
            ImageC result = CreateNew(width, height);
            Array.Copy(data, result._data, data.Length);
            return result;
        }

        public void WriteToStreamRaw(Stream stream)
        {
            stream.Write(_data, 0, Width * Height * PixelSize);
        }

        public void SaveToFile(string fileName)
        {
            // Figure out image format
            string extension = Path.GetExtension(fileName).Remove(0, 1).ToLowerInvariant();
            switch (extension)
            {
                case "png":
                    PngWriter.SaveToFile(fileName, _data, Width, Height);
                    break;
                case "bmp":
                case "dib":
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, ImageFormat.Bmp));
                    break;
                case "jpg":
                case "jpeg":
                case "jpe":
                case "jif":
                case "jfif":
                case "jfi":
                    using (EncoderParameters encoderParameters = new EncoderParameters(1))
                    using (EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, 95L))
                    {
                        ImageCodecInfo codecInfo = ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                        encoderParameters.Param[0] = encoderParameter;
                        GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, codecInfo, encoderParameters));
                    }
                    break;
                case "gif":
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, ImageFormat.Gif));
                    break;
                default:
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName));
                    break;
            }
        }

        public void SaveToStream(Stream stream, ImageFormat format)
        {
            GetTempSystemDrawingBitmap(bitmap => bitmap.Save(stream, ImageFormat.Png));
        }

        public void SavePngToStreamFast(Stream stream)
        {
            PngWriter.SaveToStream(stream, _data, Width, Height);
        }

        // Try to use 'GetTempSystemDrawingBitmap' instead if possible to avoid unnecessary data allocation
        // The returned Bitmap must be Dispose()ed.
        public Bitmap ToBitmap()
        {
            // Create bitmap
            Bitmap result = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData resultData = result.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    Marshal.Copy(_data, 0, resultData.Scan0, resultData.Height * resultData.Stride);
                }
                finally
                {
                    result.UnlockBits(resultData);
                }
                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }

        // Bitmap has the pixel format 'Format32bppArgb'
        public unsafe void GetTempSystemDrawingBitmap(Action<Bitmap> bitmapAction)
        {
            fixed (void* dataPtr = _data)
                using (var bitmap = new Bitmap(Width, Height, Width * PixelSize, PixelFormat.Format32bppArgb, new IntPtr(dataPtr))) // Temporary bitmap
                    bitmapAction(bitmap);
        }

        public unsafe void GetIntPtr(Action<IntPtr> intPtrAction)
        {
            fixed (void* dataPtr = _data)
                intPtrAction(new IntPtr(dataPtr));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void CopyFrom(int toX, int toY, ImageC fromImage, int fromX, int fromY, int width, int height)
        {
            if (toX < 0 || toY < 0 || fromX < 0 || fromY < 0 ||
                width <= 0 || height <= 0 ||
                toX + width > Width || toY + height > Height ||
                fromX + width > fromImage.Width || fromY + height > fromImage.Height)
                return;

            fixed (byte* toBase = _data)
            fixed (byte* fromBase = fromImage._data)
            {
                int bpp = 4; // 32 bpp
                int destStride = Width * bpp;
                int srcStride = fromImage.Width * bpp;
                nuint rowSize = (nuint)(width * bpp);

                byte* destTop = toBase + (toY * Width + toX) * bpp;
                byte* srcTop = fromBase + (fromY * fromImage.Width + fromX) * bpp;

                // ========= LINEAR FAST PATH: a single block =========
                // byte-contiguous rectangle on both images

                if (destStride == (int)rowSize && srcStride == (int)rowSize)
                {
                    nuint total = (nuint)(height) * rowSize;

                    // Overlap? Choose direction (memmove)
                    if (srcTop < destTop && destTop < srcTop + total)
                    {
                        // copy backwards
                        byte* s = srcTop + (long)total;
                        byte* d = destTop + (long)total;
                        CopyBlockBackward(d, s, total);
                    }
                    else
                    {
                        // copy forewards
                        CopyBlockForward(destTop, srcTop, total);
                    }
                    return;
                }

                // ========= COPY BY ROW =========
                for (int y = 0; y < height; ++y)
                {
                    byte* destRow = destTop + (long)y * destStride;
                    byte* srcRow = srcTop + (long)y * srcStride;

                    // Overlap for single row?
                    bool overlap = srcRow < destRow && destRow < srcRow + rowSize;

                    if (overlap)
                    {
                        // copy backwards on this line
                        CopyBlockBackward(destRow + (long)rowSize, srcRow + (long)rowSize, rowSize);
                    }
                    else
                    {
                        // copy forewards on this line
                        CopyBlockForward(destRow, srcRow, rowSize);
                    }
                }
            }
        }

        /// <summary>
        /// Forward copy(src -> dst) of 'count' bytes, using AVX2/SSE2 and 8/4/2/1 queues.
        /// dst and src must not increase toward low in overlap; to overlap, use backward.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void CopyBlockForward(byte* dst, byte* src, nuint count)
        {
            // AVX2: 32B
            if (Avx2.IsSupported)
            {
                while (count >= 32)
                {
                    var v = Avx.LoadVector256(src);
                    Avx.Store(dst, v);
                    src += 32; dst += 32; count -= 32;
                }
            }

            // SSE2: 16B
            if (Sse2.IsSupported)
            {
                while (count >= 16)
                {
                    var v = Sse2.LoadVector128(src);
                    Sse2.Store(dst, v);
                    src += 16; dst += 16; count -= 16;
                }
            }

            // Scalar queues
            while (count >= 8)
            {
                *(ulong*)dst = *(ulong*)src;
                src += 8; dst += 8; count -= 8;
            }
            while (count >= 4)
            {
                *(uint*)dst = *(uint*)src;
                src += 4; dst += 4; count -= 4;
            }
            while (count >= 2)
            {
                *(ushort*)dst = *(ushort*)src;
                src += 2; dst += 2; count -= 2;
            }
            if (count != 0)
            {
                *dst = *src;
            }
        }

        /// <summary>
        /// Backward copy (srcEnd -> dstEnd) of 'count' bytes (like memmove left),
        /// using AVX2/SSE2 and code 8/4/2/1. srcEnd and dstEnd point **one byte past** the end.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void CopyBlockBackward(byte* dstEnd, byte* srcEnd, nuint count)
        {
            // End pointers
            byte* dst = dstEnd;
            byte* src = srcEnd;

            // AVX2: 32B at a time, going backwards
            if (Avx2.IsSupported)
            {
                while (count >= 32)
                {
                    dst -= 32; src -= 32; count -= 32;
                    var v = Avx.LoadVector256(src);
                    Avx.Store(dst, v);
                }
            }

            // SSE2: 16B
            if (Sse2.IsSupported)
            {
                while (count >= 16)
                {
                    dst -= 16; src -= 16; count -= 16;
                    var v = Sse2.LoadVector128(src);
                    Sse2.Store(dst, v);
                }
            }

            // Scalar queues
            while (count >= 8)
            {
                dst -= 8; src -= 8; count -= 8;
                *(ulong*)dst = *(ulong*)src;
            }
            while (count >= 4)
            {
                dst -= 4; src -= 4; count -= 4;
                *(uint*)dst = *(uint*)src;
            }
            while (count >= 2)
            {
                dst -= 2; src -= 2; count -= 2;
                *(ushort*)dst = *(ushort*)src;
            }
            if (count != 0)
            {
                dst -= 1; src -= 1;
                *dst = *src;
            }
        }

        /// <summary>
        /// uint's are platform dependet representation of the color.
        /// They should stay private inside ImageC to prevent abuse.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint ColorToUint(ColorC color)
        {
            byte* byteArray = stackalloc byte[4];
            byteArray[0] = color.B;
            byteArray[1] = color.G;
            byteArray[2] = color.R;
            byteArray[3] = color.A;
            return *(uint*)byteArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void ReplaceColor(ColorC from, ColorC to)
        {
            uint fromUint = ColorToUint(from);
            uint toUint = ColorToUint(to);

            var span = MemoryMarshal.Cast<byte, uint>(_data);

            var vFrom = new Vector<uint>(fromUint);
            var vTo = new Vector<uint>(toUint);

            int i = 0;
            int simdCount = Vector<uint>.Count;
            int length = span.Length;

            // Process SIMD blocks
            for (; i <= length - simdCount; i += simdCount)
            {
                var block = new Vector<uint>(span.Slice(i, simdCount));
                var mask = Vector.Equals(block, vFrom);
                if (!Vector.EqualsAll(mask, Vector<uint>.Zero))
                {
                    var replaced = Vector.ConditionalSelect(mask, vTo, block);
                    replaced.CopyTo(span.Slice(i, simdCount));
                }
            }

            // Remaining pixels
            for (; i < length; i++)
            {
                if (span[i] == fromUint)
                    span[i] = toUint;
            }
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe BlendMode HasAlpha(TRVersion.Game version, int X, int Y, int width, int height)
		{
			var result = BlendMode.Normal;

			// Region bounds check
			if ((uint)X >= Width || (uint)Y >= Height ||
				width <= 0 || height <= 0 ||
				X + width > Width || Y + height > Height)
				return result;

			fixed (byte* basePtr8 = _data)
			{
				uint* basePtr = (uint*)basePtr8;
				uint* ptr = basePtr + (Y * Width + X); // region top-left
				int stride = Width;                    // image stride in pixels

				if (version == TRVersion.Game.TombEngine)
				{
					bool hasZeroAlpha = false;

					if (Avx2.IsSupported)
					{
						var vZero = Vector256.Create(0u);
						var vFF = Vector256.Create(255u);

						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							int x = 0;

							// 8-pixel blocks (8 x uint = 32 bytes)
							int n8 = width & ~7;
							for (; x < n8; x += 8)
							{
								var v = Avx.LoadVector256(p + x);          // 8 * u32 BGRA
								var a = Avx2.ShiftRightLogical(v, 24);     // per-lane alpha (0..255)

								var m0 = Avx2.CompareEqual(a, vZero); // a == 0?
								var m255 = Avx2.CompareEqual(a, vFF);   // a == 255?
								var nonPartial = Avx2.Or(m0, m255);          // true if (0 or 255)

								// If NOT all lanes are (0 or 255) => at least one partial alpha => AlphaBlend
								if (Avx2.MoveMask(nonPartial.AsByte()) != -1)
									return BlendMode.AlphaBlend;

								// Track presence of alpha == 0
								if (!hasZeroAlpha && Avx2.MoveMask(m0.AsByte()) != 0)
									hasZeroAlpha = true;
							}

							// SSE2 tail (4 px)
							if (Sse2.IsSupported)
							{
								var vZero128 = Vector128.Create(0u);
								var vFF128 = Vector128.Create(255u);
								int n4 = x + ((width - x) & ~3);
								for (; x < n4; x += 4)
								{
									var v128 = Sse2.LoadVector128(p + x);
									var a128 = Sse2.ShiftRightLogical(v128, 24);

									var m0128 = Sse2.CompareEqual(a128, vZero128);
									var m255_128 = Sse2.CompareEqual(a128, vFF128);
									var nonPartial128 = Sse2.Or(m0128, m255_128);

									if (Sse2.MoveMask(nonPartial128.AsByte()) != 0xFFFF)
										return BlendMode.AlphaBlend;

									if (!hasZeroAlpha && Sse2.MoveMask(m0128.AsByte()) != 0)
										hasZeroAlpha = true;
								}
							}

							// Scalar tail (<= 3 px)
							for (; x < width; ++x)
							{
								uint a = (p[x] >> 24) & 0xFFu; // A is the highest byte (BGRA)
								if (a != 0 && a != 255) return BlendMode.AlphaBlend;
								if (a == 0) hasZeroAlpha = true;
							}
						}

						return hasZeroAlpha ? BlendMode.AlphaTest : BlendMode.Normal;
					}
					else if (Sse2.IsSupported)
					{
						var vZero128 = Vector128.Create(0u);
						var vFF128 = Vector128.Create(255u);

						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							int x = 0;
							bool rowHasZero = false;

							int n4 = width & ~3;
							for (; x < n4; x += 4)
							{
								var v128 = Sse2.LoadVector128(p + x);
								var a128 = Sse2.ShiftRightLogical(v128, 24);

								var m0128 = Sse2.CompareEqual(a128, vZero128);
								var m255_128 = Sse2.CompareEqual(a128, vFF128);
								var nonPartial128 = Sse2.Or(m0128, m255_128);

								if (Sse2.MoveMask(nonPartial128.AsByte()) != 0xFFFF)
									return BlendMode.AlphaBlend;

								if (!rowHasZero && Sse2.MoveMask(m0128.AsByte()) != 0)
									rowHasZero = true;
							}

							for (; x < width; ++x)
							{
								uint a = (p[x] >> 24) & 0xFFu;
								if (a != 0 && a != 255) return BlendMode.AlphaBlend;
								if (a == 0) rowHasZero = true;
							}

							if (rowHasZero) result = BlendMode.AlphaTest;
						}

						return result;
					}
					else
					{
						// Scalar path
						bool hasZero = false;
						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							for (int col = 0; col < width; ++col)
							{
								uint a = (p[col] >> 24) & 0xFFu;
								if (a != 0 && a != 255) return BlendMode.AlphaBlend;
								if (a == 0) hasZero = true;
							}
						}
						return hasZero ? BlendMode.AlphaTest : BlendMode.Normal;
					}
				}
				else
				{
					// Non-TEN: any alpha != 255 => AlphaTest
					if (Avx2.IsSupported)
					{
						var vFF = Vector256.Create(255u);

						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							int x = 0;
							int n8 = width & ~7;

							for (; x < n8; x += 8)
							{
								var v = Avx.LoadVector256(p + x);
								var a = Avx2.ShiftRightLogical(v, 24);
								var m255 = Avx2.CompareEqual(a, vFF);

								if (Avx2.MoveMask(m255.AsByte()) != -1)
									return BlendMode.AlphaTest;
							}

							if (Sse2.IsSupported)
							{
								var vFF128 = Vector128.Create(255u);
								int n4 = x + ((width - x) & ~3);
								for (; x < n4; x += 4)
								{
									var v128 = Sse2.LoadVector128(p + x);
									var a128 = Sse2.ShiftRightLogical(v128, 24);
									var m255_128 = Sse2.CompareEqual(a128, vFF128);

									if (Sse2.MoveMask(m255_128.AsByte()) != 0xFFFF)
										return BlendMode.AlphaTest;
								}
							}

							for (; x < width; ++x)
								if (((p[x] >> 24) & 0xFFu) != 255)
									return BlendMode.AlphaTest;
						}

						return BlendMode.Normal;
					}
					else if (Sse2.IsSupported)
					{
						var vFF128 = Vector128.Create(255u);

						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							int x = 0;
							int n4 = width & ~3;

							for (; x < n4; x += 4)
							{
								var v128 = Sse2.LoadVector128(p + x);
								var a128 = Sse2.ShiftRightLogical(v128, 24);
								var m255_128 = Sse2.CompareEqual(a128, vFF128);

								if (Sse2.MoveMask(m255_128.AsByte()) != 0xFFFF)
									return BlendMode.AlphaTest;
							}

							for (; x < width; ++x)
								if (((p[x] >> 24) & 0xFFu) != 255)
									return BlendMode.AlphaTest;
						}

						return BlendMode.Normal;
					}
					else
					{
						// Scalar path
						for (int row = 0; row < height; ++row)
						{
							uint* p = ptr + row * stride;
							for (int col = 0; col < width; ++col)
								if (((p[col] >> 24) & 0xFFu) != 255)
									return BlendMode.AlphaTest;
						}
						return BlendMode.Normal;
					}
				}
			}
		}

		public unsafe BlendMode HasAlpha(TRVersion.Game version)
        {
            return HasAlpha(version, 0, 0, Width, Height);
        }

        public BlendMode HasAlpha(TRVersion.Game version, Rectangle2 rect)
        {
            return HasAlpha(version, (int)rect.Start.X, (int)rect.Start.Y, (int)rect.Width, (int)rect.Height);
        }

        public void CopyFrom(int toX, int toY, ImageC fromImage)
        {
            CopyFrom(toX, toY, fromImage, 0, 0, fromImage.Width, fromImage.Height);
        }

        public Stream ToRawStream()
        {
            return new MemoryStream(_data);
        }

        public byte[] ToByteArray()
        {
            return _data;
        }

        public unsafe byte[] ToByteArray(Rectangle2 rect) =>
            ToByteArray((int)rect.X0, (int)rect.Y0, (int)rect.Width, (int)rect.Height);

        public unsafe byte[] ToByteArray(int fromX, int fromY, int width, int height)
        {
            if (fromX < 0 || fromY < 0 || width <= 0 || height <= 0 ||
                fromX + width > Width || fromY + height > Height)
                return Array.Empty<byte>();

            var size = width * height * 4;
            var result = new byte[size];

            fixed (byte* toPtr = result)
            fixed (byte* fromPtr = _data)
            {
                for (int y = 0; y < height; ++y)
                {
                    byte* src = fromPtr + ((fromY + y) * Width + fromX) * 4;
                    byte* dst = toPtr + (y * width) * 4;
                    Buffer.MemoryCopy(src, dst, width * 4, width * 4);
                }
            }

            return result;
        }

        public unsafe Hash GetHashOfAreaFast(Rectangle2 area)
        {
            const int Bpp = 4;

            int x = (int)area.TopLeft.X;
            int y = (int)area.TopLeft.Y;
            int width = (int)area.Width;
            int height = (int)area.Height;

            if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
                x + width > Width || y + height > Height)
                return Hash.Zero;

            int srcStride = Width * Bpp;
            int rowBytes = width * Bpp;
            long total = (long)rowBytes * height;

            using var hasher = Blake3.Hasher.New();

            fixed (byte* basePtr = _data)
            {
                // FAST PATH: contiguous rectangle(only one Update)

                if (width == Width && x == 0)
                {
                    byte* start = basePtr + y * srcStride;
                    hasher.Update(new ReadOnlySpan<byte>(start, (int)total));
                }
                else
                {
                    const int ChunkSize = 1 << 20; // 1 MiB
                    byte[] rented = ArrayPool<byte>.Shared.Rent(ChunkSize);
                    try
                    {
                        int row = 0;
                        while (row < height)
                        {
                            int rowsPerChunk = Math.Max(1, Math.Min(height - row, ChunkSize / rowBytes));
                            int bytesThisChunk = rowsPerChunk * rowBytes;

                            fixed (byte* dstFixed = rented)
                            {
                                byte* dstBase = dstFixed;
                                byte* srcBase0 = basePtr + (y + row) * srcStride + x * Bpp;

                                bool doParallel = rowsPerChunk >= 64 && rowBytes >= 1024;

                                if (doParallel)
                                {
                                    Parallel.For(0, rowsPerChunk, r =>
                                    {
                                        byte* src = srcBase0 + (long)r * srcStride;
                                        byte* dst = dstBase + (long)r * rowBytes;
                                        Unsafe.CopyBlockUnaligned(dst, src, (uint)rowBytes);
                                    });
                                }
                                else
                                {
                                    for (int r = 0; r < rowsPerChunk; r++)
                                    {
                                        byte* src = srcBase0 + (long)r * srcStride;
                                        byte* dst = dstBase + (long)r * rowBytes;
                                        Unsafe.CopyBlockUnaligned(dst, src, (uint)rowBytes);
                                    }
                                }
                            }

                            hasher.Update(new ReadOnlySpan<byte>(rented, 0, bytesThisChunk));
                            row += rowsPerChunk;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rented);
                    }
                }
            }

            Span<byte> digest = stackalloc byte[32];
            hasher.Finalize(digest);

            ulong low = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(0, 8));
            ulong high = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(8, 8));
            return new Hash { HashLow = low, HashHigh = high };
        }

        public Stream ToRawStream(int yStart, int Height)
        {
            return new MemoryStream(_data, yStart * Width * PixelSize, Height * Width * PixelSize);
        }

        public ulong HashImageData(System.Security.Cryptography.HashAlgorithm hashAlgorithm)
        {
            ulong metaHash = unchecked((ulong)Width * 4551534108298448059ul + (ulong)Height * 7310107420406914801ul); // two random primes
            byte[] dataHashArr = hashAlgorithm.ComputeHash(_data);
            ulong dataHash = BitConverter.ToUInt64(dataHashArr, 0);
            return dataHash ^ metaHash;
        }

        public void RawCopyTo(byte[] destination, int offset)
        {
            Array.Copy(_data, 0, destination, offset, _data.GetLength(0));
        }

        public static ImageC SobelFilter(ImageC source, double strength, double level, SobelFilterType type, int posX, int posY, int w, int h)
        {
            ImageC result = ImageC.CreateNew(w, h);

            strength = Math.Max(strength, 0.0001f);
            double dX = 0;
            double dY = 0;
            double dZ = 1.0f / strength * (1.0f + Math.Pow(2.0f, level));

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float tl = source.GetPixel(posX + x - 1, posY + y - 1).R;
                    float l = source.GetPixel(posX + x - 1, posY + y).R;
                    float bl = source.GetPixel(posX + x - 1, posY + y + 1).R;
                    float t = source.GetPixel(posX + x, posY + y - 1).R;
                    float b = source.GetPixel(posX + x, posY + y + 1).R;
                    float tr = source.GetPixel(posX + x + 1, posY + y - 1).R;
                    float r = source.GetPixel(posX + x + 1, posY + y).R;
                    float br = source.GetPixel(posX + x + 1, posY + y + 1).R;

                    if (type == SobelFilterType.Sobel)
                    {
                        dX = tl + l * 2.0f + bl - tr - r * 2.0f - br;
                        dY = tl + t * 2.0f + tr - bl - b * 2.0f - br;
                    }
                    else if (type == SobelFilterType.Scharr)
                    {
                        dX = tl * 3.0f + l * 10.0f + bl * 3.0f - tr * 3.0f - r * 10.0f - br * 3.0f;
                        dY = tl * 3.0f + t * 10.0f + tr * 3.0f - bl * 3.0f - b * 10.0f - br * 3.0f;
                    }

                    Vector3 normal = Vector3.Normalize(new Vector3((float)dX, (float)dY, (float)dZ));

                    byte red = (byte)((normal.X * 0.5f + 0.5f) * 255.0f);
                    byte green = (byte)((normal.Y * 0.5f + 0.5f) * 255.0f);
                    byte blue = (byte)(normal.Z * 255.0f);
                    byte alpha = source.GetPixel(posX + x, posY + y).A;

                    result.SetPixel(x, y, red, green, blue, alpha);
                }
            }

            return result;
        }

        public static ImageC Resize(in ImageC Original, int newWidth, int newHeight)
        {
            var bitmap = Original.ToBitmap();
            var resizedBitmap = new Bitmap(bitmap, newWidth, newHeight);
            return FromSystemDrawingBitmapMatchingPixelFormat(resizedBitmap);
        }

        public static ImageC GrayScaleFilter(ImageC source, bool invert, int posX, int posY, int w, int h)
        {
            ImageC result = ImageC.CreateNew(w, h);

            byte[] srcData = source.ToByteArray();
            byte[] dstData = result.ToByteArray();

            int srcStride = source.Width * 4;
            int dstStride = w * 4;

            for (int y = 0; y < h; y++)
            {
                int srcRow = ((posY + y) * source.Width + posX) * 4;
                int dstRow = y * dstStride;

                for (int x = 0; x < w; x++)
                {
                    byte b = srcData[srcRow + 0];
                    byte g = srcData[srcRow + 1];
                    byte r = srcData[srcRow + 2];
                    byte a = srcData[srcRow + 3];

                    double gray = 0.2126 * r + 0.7152 * g + 0.0722 * b;
                    if (invert) gray = 255 - gray;
                    byte g8 = (byte)gray;

                    dstData[dstRow + 0] = g8;
                    dstData[dstRow + 1] = g8;
                    dstData[dstRow + 2] = g8;
                    dstData[dstRow + 3] = a;

                    srcRow += 4;
                    dstRow += 4;
                }
            }

            return result;
        }

        public static ImageC GaussianBlur(ImageC source, float radius = 1.0f)
        {
            int size = (int)Math.Ceiling(radius * 3.0);
            float[] kernel = new float[size * 2 + 1];
            float sigma = radius;
            float norm = 0;

            for (int i = -size; i <= size; i++)
            {
                float val = (float)Math.Exp(-(i * i) / (2 * sigma * sigma));
                kernel[i + size] = val;
                norm += val;
            }
            for (int i = 0; i < kernel.Length; i++)
                kernel[i] /= norm;

            ImageC temp = CreateNew(source.Width, source.Height);
            ImageC result = CreateNew(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int k = -size; k <= size; k++)
                    {
                        int ix = MathC.Clamp(x + k, 0, source.Width - 1);
                        ColorC c = source.GetPixel(ix, y);
                        float weight = kernel[k + size];
                        r += c.R * weight;
                        g += c.G * weight;
                        b += c.B * weight;
                        a += c.A * weight;
                    }
                    temp.SetPixel(x, y, (byte)r, (byte)g, (byte)b, (byte)a);
                }
            }

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int k = -size; k <= size; k++)
                    {
                        int iy = MathC.Clamp(y + k, 0, source.Height - 1);
                        ColorC c = temp.GetPixel(x, iy);
                        float weight = kernel[k + size];
                        r += c.R * weight;
                        g += c.G * weight;
                        b += c.B * weight;
                        a += c.A * weight;
                    }
                    result.SetPixel(x, y, (byte)r, (byte)g, (byte)b, (byte)a);
                }
            }

            return result;
        }

        public static ImageC NormalizeContrast(ImageC source)
        {
            byte min = 255, max = 0;
            for (int i = 0; i < source.Width * source.Height; i++)
            {
                byte v = source.Get(i).R;
                if (v < min) min = v;
                if (v > max) max = v;
            }

            float scale = 255.0f / Math.Max(1, max - min);
            ImageC result = CreateNew(source.Width, source.Height);
            for (int i = 0; i < source.Width * source.Height; i++)
            {
                ColorC c = source.Get(i);
                byte newVal = (byte)MathC.Clamp((int)((c.R - min) * scale), 0, 255);
                result.Set(i, new ColorC(newVal, newVal, newVal, c.A));
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void CopySingleChannelFrom(
            int toX, int toY, ImageC fromImage, int fromX, int fromY,
            int width, int height, ImageChannel channel)
        {
            if (toX < 0 || toY < 0 || fromX < 0 || fromY < 0 ||
            width <= 0 || height <= 0 ||
                toX + width > Width || toY + height > Height ||
                fromX + width > fromImage.Width || fromY + height > fromImage.Height)
                return;

            // ARGB in uint (in memory little-endian = BGRA):
            // R=16, G=8, B=0, A=24
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int ShiftOf(ImageChannel ch) => ch switch
            {
                ImageChannel.R => 16,
                ImageChannel.G => 8,
                ImageChannel.B => 0,
                _ => 24
            };

            int shift = ShiftOf(channel);
            uint mask = 0xFFu << shift;
            uint invMask = ~mask;

            fixed (byte* pDstBase = _data)
            fixed (byte* pSrcBase = fromImage._data)
            {
                int dstStrideBytes = Width * 4;
                int srcStrideBytes = fromImage.Width * 4;

                byte* dstRow = pDstBase + (toY * Width + toX) * 4;
                byte* srcRow = pSrcBase + (fromY * fromImage.Width + fromX) * 4;

                // -------- AVX2: 8 px / iterazione --------
                if (Avx2.IsSupported)
                {
                    var vMask = Vector256.Create(mask);
                    var vInvMask = Vector256.Create(invMask);

                    for (int y = 0; y < height; ++y)
                    {
                        byte* dst = dstRow;
                        byte* src = srcRow;

                        int n8 = width & ~7;
                        for (int x = 0; x < n8; x += 8)
                        {
                            var vSrc = Avx.LoadVector256((uint*)(src + x * 4));
                            var vDst = Avx.LoadVector256((uint*)(dst + x * 4));

                            // dst = (dst & ~mask) | (src & mask)
                            var vOut = Avx2.Or(Avx2.And(vDst, vInvMask), Avx2.And(vSrc, vMask));
                            Avx.Store((uint*)(dst + x * 4), vOut);
                        }

                        int rem = width - n8;

                        // Coda 4 px con SSE2 se disponibile
                        if (Sse2.IsSupported && rem >= 4)
                        {
                            int x4 = n8;
                            var vSrc128 = Sse2.LoadVector128((uint*)(src + x4 * 4));
                            var vDst128 = Sse2.LoadVector128((uint*)(dst + x4 * 4));

                            var vMask128 = Vector128.Create(mask);
                            var vInvMask128 = Vector128.Create(invMask);

                            var vOut128 = Sse2.Or(Sse2.And(vDst128, vInvMask128), Sse2.And(vSrc128, vMask128));
                            Sse2.Store((uint*)(dst + x4 * 4), vOut128);

                            n8 += 4;
                            rem -= 4;
                        }

                        // Residui (0..3) scalar
                        for (int x = n8; x < width; ++x)
                        {
                            uint sPx = ((uint*)src)[x];
                            uint dPx = ((uint*)dst)[x];
                            ((uint*)dst)[x] = (dPx & invMask) | (sPx & mask);
                        }

                        srcRow += srcStrideBytes;
                        dstRow += dstStrideBytes;
                    }

                    return;
                }

                // -------- SSE2: 4 px / iteration --------
                if (Sse2.IsSupported)
                {
                    var vMask = Vector128.Create(mask);
                    var vInvMask = Vector128.Create(invMask);

                    for (int y = 0; y < height; ++y)
                    {
                        byte* dst = dstRow;
                        byte* src = srcRow;

                        int n4 = width & ~3;
                        for (int x = 0; x < n4; x += 4)
                        {
                            var vSrc = Sse2.LoadVector128((uint*)(src + x * 4));
                            var vDst = Sse2.LoadVector128((uint*)(dst + x * 4));

                            var vOut = Sse2.Or(Sse2.And(vDst, vInvMask), Sse2.And(vSrc, vMask));
                            Sse2.Store((uint*)(dst + x * 4), vOut);
                        }

                        for (int x = n4; x < width; ++x)
                        {
                            uint sPx = ((uint*)src)[x];
                            uint dPx = ((uint*)dst)[x];
                            ((uint*)dst)[x] = (dPx & invMask) | (sPx & mask);
                        }

                        srcRow += srcStrideBytes;
                        dstRow += dstStrideBytes;
                    }

                    return;
                }

                // -------- Scalar fallback --------
                for (int y = 0; y < height; ++y)
                {
                    uint* dst = (uint*)dstRow;
                    uint* src = (uint*)srcRow;

                    for (int x = 0; x < width; ++x)
                    {
                        uint sPx = src[x];
                        uint dPx = dst[x];
                        dst[x] = (dPx & invMask) | (sPx & mask);
                    }

                    srcRow += srcStrideBytes;
                    dstRow += dstStrideBytes;
                }
            }
        }
    }
}

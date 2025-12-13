using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace TombLib.Utils
{
	public static class PngWriter
	{
		private static readonly uint[] _crcTable = GenerateCrcTable();
		private static readonly Vector256<byte> _avx2Mask = Vector256.Create(
			(byte)2, 1, 0, 3, 6, 5, 4, 7,
			10, 9, 8, 11, 14, 13, 12, 15,
			18, 17, 16, 19, 22, 21, 20, 23,
			26, 25, 24, 27, 30, 29, 28, 31);

		private static readonly Vector128<byte> _ssse3Mask = Vector128.Create(
			(byte)2, 1, 0, 3,
			6, 5, 4, 7,
			10, 9, 8, 11,
			14, 13, 12, 15);

		public static void SaveToFile(string filePath, ReadOnlySpan<byte> rgba, int width, int height)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			SaveToStream(fs, rgba, width, height);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void SaveToStream(Stream stream, ReadOnlySpan<byte> rgba, int width, int height)
		{
			int stride = width * 4;
			if (rgba.Length != stride * height)
				throw new ArgumentException("Invalid data size for given dimensions.");

			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

			// PNG signature
			writer.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

			// IHDR
			Span<byte> ihdr = stackalloc byte[13];
			BinaryPrimitives.WriteInt32BigEndian(ihdr.Slice(0, 4), width);
			BinaryPrimitives.WriteInt32BigEndian(ihdr.Slice(4, 4), height);
			ihdr[8] = 8;  // Bit depth
			ihdr[9] = 6;  // RGBA
			ihdr[10] = 0; // Compression
			ihdr[11] = 0; // Filter
			ihdr[12] = 0; // Interlace
			WriteChunk(writer, "IHDR", ihdr);

			// IDAT
			WriteIdatChunkBgraToRgbaSubFilterSimd(writer, rgba, width, height);

			// IEND
			WriteChunk(writer, "IEND", ReadOnlySpan<byte>.Empty);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static unsafe void WriteIdatChunkBgraToRgbaSubFilterSimd(BinaryWriter writer, ReadOnlySpan<byte> bgra, int width, int height)
		{
			int stride = width * 4;

			Span<byte> row = stackalloc byte[stride + 1];
			Span<byte> rgba = stackalloc byte[stride];

			using var ms = new MemoryStream();
			using (var deflate = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: false))
			{
				for (int y = 0; y < height; y++)
				{
					row[0] = 1; // Sub filter
					var src = bgra.Slice(y * stride, stride);

					if (Avx2.IsSupported)
					{
						fixed (byte* pSrc = src)
						fixed (byte* pDst = rgba)
						{
							int i = 0;
							for (; i <= stride - 32; i += 32)
							{
								var v = Avx.LoadVector256(pSrc + i);
								var shuffled = Avx2.Shuffle(v, _avx2Mask);
								Avx.Store(pDst + i, shuffled);
							}
							for (; i < stride; i += 4)
							{
								pDst[i + 0] = pSrc[i + 2];
								pDst[i + 1] = pSrc[i + 1];
								pDst[i + 2] = pSrc[i + 0];
								pDst[i + 3] = pSrc[i + 3];
							}
						}
					}
					else if (Ssse3.IsSupported)
					{
						fixed (byte* pSrc = src)
						fixed (byte* pDst = rgba)
						{
							int i = 0;
							for (; i <= stride - 16; i += 16)
							{
								var v = Unsafe.ReadUnaligned<Vector128<byte>>(pSrc + i);
								var shuffled = Ssse3.Shuffle(v, _ssse3Mask);
								Unsafe.WriteUnaligned(pDst + i, shuffled);
							}
							for (; i < stride; i += 4)
							{
								pDst[i + 0] = pSrc[i + 2];
								pDst[i + 1] = pSrc[i + 1];
								pDst[i + 2] = pSrc[i + 0];
								pDst[i + 3] = pSrc[i + 3];
							}
						}
					}
					else
					{
						for (int i = 0; i < stride; i += 4)
						{
							rgba[i + 0] = src[i + 2];
							rgba[i + 1] = src[i + 1];
							rgba[i + 2] = src[i + 0];
							rgba[i + 3] = src[i + 3];
						}
					}

					// Filter Sub (on rgba → row[1..])
					for (int i = 0; i < stride; i++)
					{
						byte prev = (i >= 4) ? rgba[i - 4] : (byte)0;
						row[i + 1] = (byte)(rgba[i] - prev);
					}

					deflate.Write(row);
				}
			}

			WriteChunk(writer, "IDAT", ms.ToArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static void WriteChunk(BinaryWriter writer, string type, ReadOnlySpan<byte> data)
		{
			Span<byte> typeBytes = stackalloc byte[4];
			Encoding.ASCII.GetBytes(type, typeBytes);

			writer.Write(BinaryPrimitives.ReverseEndianness(data.Length));
			writer.Write(typeBytes);
			writer.Write(data);
			uint crc = Crc32(typeBytes, data);
			writer.Write(BinaryPrimitives.ReverseEndianness((int)crc));
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
		{
			uint crc = 0xffffffff;

			foreach (byte b in type)
				crc = _crcTable[(crc ^ b) & 0xff] ^ (crc >> 8);

			foreach (byte b in data)
				crc = _crcTable[(crc ^ b) & 0xff] ^ (crc >> 8);

			return crc ^ 0xffffffff;
		}

		private static uint[] GenerateCrcTable()
		{
			const uint poly = 0xEDB88320;
			var table = new uint[256];

			for (uint i = 0; i < 256; i++)
			{
				uint c = i;
				for (int k = 0; k < 8; k++)
					c = (c & 1) != 0 ? poly ^ (c >> 1) : c >> 1;
				table[i] = c;
			}

			return table;
		}
	}
}

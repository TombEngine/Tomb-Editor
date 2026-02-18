using K4os.Compression.LZ4;
using System;
using System.IO;
using System.IO.Compression;

namespace TombLib.Utils
{
	public class LZ4
	{
		private const int ChunkSize = 256 * 1024 * 1024; // 256 MB

		/// <summary>
		/// Compresses data from input stream to output stream using chunked raw LZ4 blocks.
		/// Supports data larger than 2 GB. Returns the number of compressed bytes written.
		/// </summary>
		public static long CompressData(Stream inStream, Stream outStream, CompressionLevel compressionLevel)
		{
			long totalSize = inStream.Length;
			inStream.Position = 0;

			long startPos = outStream.Position;
			var bw = new BinaryWriter(outStream, System.Text.Encoding.Default, true);

			int numChunks = Math.Max(1, (int)((totalSize + ChunkSize - 1) / ChunkSize));
			bw.Write((uint)numChunks);

			var readBuffer = new byte[(int)Math.Min(ChunkSize, totalSize)];
			byte[] compressBuffer = null;
			var level = GetCompressionLevel(compressionLevel);

			for (int i = 0; i < numChunks; i++)
			{
				int bytesToRead = (int)Math.Min(ChunkSize, totalSize - inStream.Position);
				int bytesRead = 0;

				while (bytesRead < bytesToRead)
				{
					int read = inStream.Read(readBuffer, bytesRead, bytesToRead - bytesRead);
					if (read == 0)
						break;
					bytesRead += read;
				}

				int maxOutput = LZ4Codec.MaximumOutputSize(bytesRead);
				if (compressBuffer == null || compressBuffer.Length < maxOutput)
					compressBuffer = new byte[maxOutput];

				int compressedLen = LZ4Codec.Encode(
					readBuffer, 0, bytesRead,
					compressBuffer, 0, compressBuffer.Length,
					level);

				bw.Write((uint)bytesRead);
				bw.Write((uint)compressedLen);
				outStream.Write(compressBuffer, 0, compressedLen);
			}

			return outStream.Position - startPos;
		}

		/// <summary>
		/// Compresses a byte array to an output stream using chunked raw LZ4 blocks.
		/// Returns the number of compressed bytes written.
		/// </summary>
		public static long CompressData(byte[] inData, Stream outStream, CompressionLevel compressionLevel)
		{
			long startPos = outStream.Position;
			var bw = new BinaryWriter(outStream, System.Text.Encoding.Default, true);

			int numChunks = Math.Max(1, (inData.Length + ChunkSize - 1) / ChunkSize);
			bw.Write((uint)numChunks);

			byte[] compressBuffer = null;
			var level = GetCompressionLevel(compressionLevel);
			int offset = 0;

			for (int i = 0; i < numChunks; i++)
			{
				int chunkLen = Math.Min(ChunkSize, inData.Length - offset);

				int maxOutput = LZ4Codec.MaximumOutputSize(chunkLen);
				if (compressBuffer == null || compressBuffer.Length < maxOutput)
					compressBuffer = new byte[maxOutput];

				int compressedLen = LZ4Codec.Encode(
					inData, offset, chunkLen,
					compressBuffer, 0, compressBuffer.Length,
					level);

				bw.Write((uint)chunkLen);
				bw.Write((uint)compressedLen);
				outStream.Write(compressBuffer, 0, compressedLen);

				offset += chunkLen;
			}

			return outStream.Position - startPos;
		}

		private static LZ4Level GetCompressionLevel(CompressionLevel compressionLevel)
		{
			return compressionLevel switch
			{
				CompressionLevel.SmallestSize => LZ4Level.L12_MAX,
				CompressionLevel.Optimal => LZ4Level.L03_HC,
				CompressionLevel.Fastest => LZ4Level.L00_FAST,
				_ => LZ4Level.L11_OPT
			};
		}
	}
}

using K4os.Compression.LZ4;
using System.IO;
using System.IO.Compression;

namespace TombLib.Utils
{
	public class LZ4
	{
		public static byte[] CompressData(Stream inStream, CompressionLevel compressionLevel)
		{
			long length = inStream.Length;

			if (length > int.MaxValue)
				throw new System.InvalidOperationException(
					$"Stream data size ({length / (1024 * 1024)} MB) exceeds the 2 GB compression limit. " +
					"Try reducing the number of texture pages or disabling texture compression.");

			if (inStream is MemoryStream ms)
				return CompressRaw(ms.ToArray(), compressionLevel);

			var buffer = new byte[(int)length];
			inStream.Position = 0;

			int offset = 0;
			while (offset < buffer.Length)
			{
				int read = inStream.Read(buffer, offset, buffer.Length - offset);
				if (read == 0)
					break;
				offset += read;
			}

			return CompressRaw(buffer, compressionLevel);
		}

		public static byte[] CompressData(byte[] inData, CompressionLevel compressionLevel)
		{
			return CompressRaw(inData, compressionLevel);
		}

		private static byte[] CompressRaw(byte[] inData, CompressionLevel compressionLevel)
		{
			int maxOutputSize = LZ4Codec.MaximumOutputSize(inData.Length);
			byte[] output = new byte[maxOutputSize];

			int compressedLength = LZ4Codec.Encode(
				inData, 0, inData.Length,
				output, 0, output.Length,
				GetCompressionLevel(compressionLevel)
			);

			if (compressedLength != output.Length)
				System.Array.Resize(ref output, compressedLength);

			return output;
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

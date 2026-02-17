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
			int capacity = length > 0 && length <= int.MaxValue ? (int)length : 0;

			using var ms = capacity > 0 ? new MemoryStream(capacity) : new MemoryStream();
			inStream.CopyTo(ms);

			return CompressRaw(ms.ToArray(), compressionLevel);
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

using System;
using System.IO;
using System.Linq;
using TombLib.Utils;

namespace TombIDE.ProjectMaster.Services.PakFile;

public sealed class PakFileService : IPakFileService
{
	private static readonly byte[] PakFilePrefix = [0x00, 0x00, 0x06, 0x00]; // Required prefix for TR4

	public byte[] GetDecompressedData(string pakFilePath)
	{
		if (!File.Exists(pakFilePath))
			throw new FileNotFoundException($"PAK file not found: {pakFilePath}");

		try
		{
			using var stream = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read);

			if (stream.Length < 4)
				throw new InvalidOperationException($"PAK file is too small (less than 4 bytes): {pakFilePath}");

			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, (int)stream.Length);

			// Skip the 4-byte prefix before decompression
			bytes = [.. bytes.Skip(4)];

			return ZLib.DecompressData(bytes);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to decompress PAK file: {pakFilePath}", ex);
		}
	}

	public void SavePakFile(string pakFilePath, byte[] rawData)
	{
		try
		{
			byte[] pakFileData = CompressDataAndAddPrefix(rawData);
			File.WriteAllBytes(pakFilePath, pakFileData);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to save PAK file: {pakFilePath}", ex);
		}
	}

	private static byte[] CompressDataAndAddPrefix(byte[] data)
	{
		byte[] compressedData = ZLib.CompressData(data);
		return [.. PakFilePrefix, .. compressedData];
	}
}

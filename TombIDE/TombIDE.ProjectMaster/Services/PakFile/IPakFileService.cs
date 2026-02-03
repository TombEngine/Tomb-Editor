using System;
using System.IO;

namespace TombIDE.ProjectMaster.Services.PakFile;

/// <summary>
/// Provides functionality for reading and writing PAK files used by TR4.
/// <para>PAK files are compressed data files with a 4-byte prefix used for game resources like logos.</para>
/// </summary>
public interface IPakFileService
{
	/// <summary>
	/// Decompresses and returns the raw data from a PAK file.
	/// </summary>
	/// <param name="pakFilePath">The path to the PAK file to read.</param>
	/// <returns>The decompressed byte array.</returns>
	/// <exception cref="FileNotFoundException">Thrown when the PAK file doesn't exist.</exception>
	/// <exception cref="InvalidOperationException">Thrown when decompression fails.</exception>
	byte[] GetDecompressedData(string pakFilePath);

	/// <summary>
	/// Compresses raw data and saves it as a PAK file.
	/// </summary>
	/// <param name="pakFilePath">The path where the PAK file will be saved.</param>
	/// <param name="rawData">The raw data to compress and save.</param>
	/// <exception cref="InvalidOperationException">Thrown when compression fails.</exception>
	void SavePakFile(string pakFilePath, byte[] rawData);
}

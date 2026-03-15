using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace TombIDE.ProjectMaster.Services.FileExtraction;

public sealed class FileExtractionService : IFileExtractionService
{
	public void ExtractEntries(IEnumerable<ZipArchiveEntry> entries, string targetDirectoryPath, bool overwrite = true, string? subPathToTrim = null)
	{
		Directory.CreateDirectory(targetDirectoryPath);

		foreach (ZipArchiveEntry entry in entries)
		{
			string relativePath = GetRelativePath(entry.FullName, subPathToTrim);

			if (string.IsNullOrEmpty(relativePath))
				continue;

			string targetPath = Path.Combine(targetDirectoryPath, relativePath);

			// Check if this is a directory entry
			if (entry.FullName.EndsWith('/'))
			{
				Directory.CreateDirectory(targetPath);
				continue;
			}

			// Ensure the file's parent directory exists
			string? fileDirectory = Path.GetDirectoryName(targetPath);

			if (!string.IsNullOrEmpty(fileDirectory))
				Directory.CreateDirectory(fileDirectory);

			if (overwrite || !File.Exists(targetPath))
				entry.ExtractToFile(targetPath, overwrite);
		}
	}

	public void CopyFilesToDirectory(IEnumerable<FileInfo> files, string targetDirectoryPath, bool overwrite = true, string? subPathToTrim = null)
	{
		Directory.CreateDirectory(targetDirectoryPath);

		foreach (FileInfo file in files)
		{
			string relativePath = GetRelativePath(file.FullName, subPathToTrim);

			if (string.IsNullOrEmpty(relativePath))
				continue;

			string targetPath = Path.Combine(targetDirectoryPath, relativePath);

			// Ensure the file's parent directory exists
			string? fileDirectory = Path.GetDirectoryName(targetPath);

			if (!string.IsNullOrEmpty(fileDirectory))
				Directory.CreateDirectory(fileDirectory);

			file.CopyTo(targetPath, overwrite);
		}
	}

	/// <summary>
	/// Gets the relative path by trimming the specified sub-path from the full path.
	/// </summary>
	/// <param name="fullPath">The full path to process.</param>
	/// <param name="subPathToTrim">The sub-path to trim from the full path.</param>
	/// <returns>
	/// The relative path after trimming the sub-path.
	/// <para>For example, <c>"Engine/bin/x64/TombEngine.exe"</c> with sub-path <c>"Engine/bin/"</c> becomes <c>"x64/TombEngine.exe"</c>.</para>
	/// </returns>
	private static string GetRelativePath(string fullPath, string? subPathToTrim)
	{
		if (string.IsNullOrEmpty(subPathToTrim))
			return fullPath;

		if (fullPath.StartsWith(subPathToTrim, StringComparison.OrdinalIgnoreCase))
			return fullPath[subPathToTrim.Length..];

		// Fallback: return just the file name if the sub-path doesn't match
		return Path.GetFileName(fullPath);
	}
}

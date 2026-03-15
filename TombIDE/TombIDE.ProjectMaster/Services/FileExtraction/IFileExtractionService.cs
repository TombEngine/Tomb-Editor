using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace TombIDE.ProjectMaster.Services.FileExtraction;

/// <summary>
/// Provides functionality for extracting files from ZIP archives and copying files.
/// </summary>
public interface IFileExtractionService
{
	/// <summary>
	/// Extracts ZIP archive entries to a target directory, optionally trimming a sub-path prefix from entry names.
	/// </summary>
	/// <param name="entries">The ZIP entries to extract.</param>
	/// <param name="targetDirectoryPath">The target directory where files should be extracted.</param>
	/// <param name="overwrite">Whether to overwrite existing files.</param>
	/// <param name="subPathToTrim">
	/// Optional sub-path prefix to trim from entry names.
	/// <para>For example, <c>"Engine/bin/x64/TombEngine.exe"</c> with sub-path <c>"Engine/bin/"</c> becomes <c>"x64/TombEngine.exe"</c>.</para>
	/// </param>
	void ExtractEntries(IEnumerable<ZipArchiveEntry> entries, string targetDirectoryPath, bool overwrite = true, string? subPathToTrim = null);

	/// <summary>
	/// Copies files from a source directory to a target directory, optionally trimming a sub-path prefix from file paths.
	/// </summary>
	/// <param name="files">The files to copy.</param>
	/// <param name="targetDirectoryPath">The target directory where files should be copied.</param>
	/// <param name="overwrite">Whether to overwrite existing files.</param>
	/// <param name="subPathToTrim">
	/// Optional sub-path prefix to trim from file paths. Providing this will allow maintaining the directory structure relative to the trimmed sub-path.
	/// <para>For example, <c>"Engine/bin/x64/TombEngine.exe"</c> with sub-path <c>"Engine/bin/"</c> becomes <c>"x64/TombEngine.exe"</c>.</para>
	/// </param>
	void CopyFilesToDirectory(IEnumerable<FileInfo> files, string targetDirectoryPath, bool overwrite = true, string? subPathToTrim = null);
}

using System;
using System.IO;
using System.Linq;

namespace TombLib.Scripting.UI.IO;

/// <summary>
/// Copies script directories between a project and a compiler staging area,
/// skipping backup files and optionally reformatting text files.
/// </summary>
public static class ScriptDirectoryCopier
{
	/// <summary>
	/// Recursively copies <paramref name="sourcePath"/> into <paramref name="targetPath"/>,
	/// skipping <c>.backup</c> files. When <paramref name="copyFile"/> is provided it replaces
	/// the default copy for each file, allowing callers to reformat text files.
	/// </summary>
	/// <param name="sourcePath">The source script directory.</param>
	/// <param name="targetPath">The target staging directory.</param>
	/// <param name="clearTarget">Whether to delete an existing target directory first.</param>
	/// <param name="copyFile">Optional per-file copy callback receiving (sourcePath, targetPath).</param>
	public static void CopyScriptDirectory(string sourcePath, string targetPath, bool clearTarget, Action<string, string>? copyFile = null)
	{
		if (clearTarget && Directory.Exists(targetPath))
			Directory.Delete(targetPath, true);

		Directory.CreateDirectory(targetPath);

		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(GetMappedPath(sourcePath, targetPath, dirPath));

		foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
			.Where(x => !Path.GetExtension(x).Equals(".backup", StringComparison.OrdinalIgnoreCase)))
		{
			string newPath = GetMappedPath(sourcePath, targetPath, file);

			if (copyFile is not null)
				copyFile(file, newPath);
			else
				File.Copy(file, newPath, true);
		}
	}

	private static string GetMappedPath(string sourcePath, string targetPath, string path)
		=> Path.Combine(targetPath, Path.GetRelativePath(sourcePath, path));

	/// <summary>
	/// Deletes all entries in the directory whose names do not satisfy <paramref name="keep"/>.
	/// </summary>
	/// <param name="directoryPath">The directory to clean.</param>
	/// <param name="keep">Predicate deciding whether an entry name is preserved.</param>
	public static void ClearDirectoryExcept(string directoryPath, Func<string, bool> keep)
	{
		var directory = new DirectoryInfo(directoryPath);

		foreach (FileSystemInfo fileSystemInfo in directory.EnumerateFileSystemInfos().Where(x => !keep(x.Name)))
		{
			if (fileSystemInfo is DirectoryInfo dir)
				dir.Delete(true);
			else
				fileSystemInfo.Delete();
		}
	}
}

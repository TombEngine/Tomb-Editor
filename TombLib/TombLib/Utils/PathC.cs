using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TombLib.Utils
{
    public static class PathC
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static string GetRelativePath(string baseDir, string fileName)
        {
            if (string.IsNullOrEmpty(baseDir))
                return Path.GetFullPath(fileName);

            // https://stackoverflow.com/questions/9042861/how-to-make-an-absolute-path-relative-to-a-particular-folder
            //
            // Roughly based on (slightly improved)
            //   https://sourceforge.net/p/syncproj/code/HEAD/tree/syncProj.cs#l976
            //   makeRelative
            baseDir = Path.GetFullPath(baseDir);
            fileName = Path.GetFullPath(Path.Combine(baseDir, fileName));

            var dictionarySeperators = new[] { Path.DirectorySeparatorChar.ToString(), Path.AltDirectorySeparatorChar.ToString() };
            string[] baseDirArr = baseDir.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);
            string[] fileNameArr = fileName.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            for (; i < baseDirArr.Length && i < fileNameArr.Length; i++)
                if (string.Compare(baseDirArr[i], fileNameArr[i], true) != 0) // Case insensitive match
                    break;
            if (i == 0) // Cannot make relative path, for example if resides on different drive
                return null;

            var resultFolders = Enumerable.Repeat("..", Math.Max(0, baseDirArr.Length - i)).Concat(fileNameArr.Skip(i));
            string result = string.Join(Path.DirectorySeparatorChar.ToString(), resultFolders);
            return result;
        }

        public static string TryFindFile(string fullBasePath, string filename, int maxFilePathCheckDepth, int maxLevelPathCheckDepth)
        {
            try
            {
                // Is the file easily found?
                if (File.Exists(filename))
                    return filename;

                string[] filePathComponents = filename.Split(new[] { '\\', '/' });
                string[] levelPathComponents = fullBasePath.Split(new[] { '\\', '/' });

                // Try to go up 2 directories to find file (works in original levels)
                // If it turns out that many people have directory structures incompatible to this assumptions
                // we can add more suffisticated options here in the future.
                int filePathCheckDepth = Math.Min(maxFilePathCheckDepth, filePathComponents.GetLength(0) - 1);
                int levelPathCheckDepth = Math.Min(maxLevelPathCheckDepth, levelPathComponents.GetLength(0) - 1);
                for (int levelPathUntil = 0; levelPathUntil <= levelPathCheckDepth; ++levelPathUntil)
                    for (int filePathAfter = 1; filePathAfter <= filePathCheckDepth; ++filePathAfter)
                    {
                        var basePath = levelPathComponents.Take(levelPathComponents.GetLength(0) - levelPathUntil);
                        var filePath = filePathComponents.Skip(filePathComponents.GetLength(0) - filePathAfter);
                        string filepathSuggestion = string.Join(Path.DirectorySeparatorChar.ToString(), basePath.Union(filePath));
                        if (File.Exists(filepathSuggestion))
                            return filepathSuggestion;
                    }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "TryFindFile failed for '" + filename + "' in the directory '" + fullBasePath + "'.");
                // In cas of an error we can just give up to find the absolute path already
                // and prompt the user for the file path.
            }
            return filename;
        }

        public static bool IsFileNotFoundException(Exception exc)
        {
            return exc is FileNotFoundException || exc is DirectoryNotFoundException || exc is DriveNotFoundException;
        }

        public static string GetFileNameWithoutExtensionTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                return path;
            }
        }

        public static string GetFileNameTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetFileName(path);
            }
            catch
            {
                return path;
            }
        }

        public static string GetDirectoryNameTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return path;
            }
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            var result = new List<string>(GetDirectories(path, searchPattern));

            if (searchOption == SearchOption.AllDirectories)
            {
                for (int i = 0; i < result.Count; i++)
                    result.AddRange(GetDirectories(result[i], searchPattern));
            }

            result.Add(path);
            return result;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

		public static bool IsTrulyAbsolutePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			try
			{
				return Path.IsPathRooted(path) && !string.IsNullOrWhiteSpace(Path.GetPathRoot(path));
			}
			catch
			{
				return false;
			}
		}
	}
}

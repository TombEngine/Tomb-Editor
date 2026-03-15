using System;
using System.IO;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Settings.LogCleaning;

public sealed class LogCleaningService : ILogCleaningService
{
	public int DeleteAllLogFiles(IGameProject project)
	{
		string engineDirectory = project.GetEngineRootDirectoryPath();

		if (!Directory.Exists(engineDirectory))
			return 0;

		int deletedCount = 0;

		// Delete all files in the engine root directory matching log/error patterns
		foreach (string file in Directory.GetFiles(engineDirectory))
		{
			string fileName = Path.GetFileName(file);

			if (ShouldDeleteFile(fileName))
			{
				try
				{
					File.Delete(file);
					deletedCount++;
				}
				catch
				{
					// Ignore files that can't be deleted (might be in use)
				}
			}
		}

		// Delete all .log files recursively
		foreach (string logFile in Directory.GetFiles(engineDirectory, "*.log", SearchOption.AllDirectories))
		{
			try
			{
				File.Delete(logFile);
				deletedCount++;
			}
			catch
			{
				// Ignore files that can't be deleted (might be in use)
			}
		}

		// Delete the logs directory if it exists
		string logsDirectory = Path.Combine(engineDirectory, "logs");

		if (Directory.Exists(logsDirectory))
		{
			try
			{
				Directory.Delete(logsDirectory, true);
				deletedCount++; // Count the directory as one item
			}
			catch
			{
				// Ignore if directory can't be deleted
			}
		}

		return deletedCount;
	}

	private static bool ShouldDeleteFile(string fileName)
	{
		// Check for specific file names
		if (fileName
			is "db_patches_crash.bin"
			or "DETECTED CRASH.txt"
			or "LastExtraction.lst")
		{
			return true;
		}

		string extension = Path.GetExtension(fileName);

		// Check for crash dump patterns
		bool isLastCrashDump = fileName.StartsWith("Last_Crash_", StringComparison.OrdinalIgnoreCase);

		bool isValidLastCrashDumpExtension = extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
			|| extension.Equals(".mem", StringComparison.OrdinalIgnoreCase);

		if (isLastCrashDump && isValidLastCrashDumpExtension)
			return true;

		// Check for warm up log pattern
		if (fileName.EndsWith("_warm_up_log.txt", StringComparison.OrdinalIgnoreCase))
			return true;

		// Check for .log extension
		if (extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}
}

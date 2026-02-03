using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

/// <summary>
/// Base class for game archive creation services.
/// Provides common functionality for creating distribution archives.
/// </summary>
public abstract class GameArchiveServiceBase : IGameArchiveService
{
	public event EventHandler<ArchiveProgressEventArgs>? ProgressChanged;

	protected void OnProgressChanged(string operation, int filesProcessed, int totalFiles)
	{
		ProgressChanged?.Invoke(this, new ArchiveProgressEventArgs
		{
			CurrentOperation = operation,
			FilesProcessed = filesProcessed,
			TotalFiles = totalFiles
		});
	}

	public abstract bool SupportsGameVersion(IGameProject project);

	public async Task CreateGameArchiveAsync(
		IGameProject project,
		string outputPath,
		string? readmeText = null,
		CancellationToken cancellationToken = default)
	{
		if (!SupportsGameVersion(project))
			throw new InvalidOperationException($"This service does not support {project.GameVersion}.");

		// Delete existing archive if it exists
		if (File.Exists(outputPath))
			File.Delete(outputPath);

		string engineDirectory = project.GetEngineRootDirectoryPath();
		string launchFilePath = project.GetLauncherFilePath();

		bool hasLauncherFile = File.Exists(launchFilePath);
		bool hasReadmeContent = !string.IsNullOrWhiteSpace(readmeText);
		bool hasSavesFolder = CreateSavesFolder();

		IReadOnlyList<string> importantFolders = GetImportantFolders(engineDirectory);
		IReadOnlyList<string> importantFiles = GetImportantFiles(engineDirectory);

		int totalFileCount = CalculateTotalFileCount(
			importantFolders, importantFiles, hasLauncherFile, hasReadmeContent, hasSavesFolder);

		OnProgressChanged("Creating archive...", 0, totalFileCount);

		int processedFiles = 0;

		await Task.Run(() =>
		{
			using var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create);

			// Add files from folders
			foreach (string folder in importantFolders.Where(Directory.Exists))
			{
				cancellationToken.ThrowIfCancellationRequested();

				foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
				{
					cancellationToken.ThrowIfCancellationRequested();

					AddFileToArchive(archive, file, engineDirectory, "Engine/");
					OnProgressChanged($"Added: {Path.GetFileName(file)}", ++processedFiles, totalFileCount);
				}
			}

			// Create saves folder if needed
			if (CreateSavesFolder())
			{
				cancellationToken.ThrowIfCancellationRequested();

				archive.CreateEntry("Engine/saves/keep.me", CompressionLevel.Optimal);
				OnProgressChanged("Created: /saves/ folder", ++processedFiles, totalFileCount);
			}

			// Add individual files
			foreach (string file in importantFiles.Where(File.Exists))
			{
				cancellationToken.ThrowIfCancellationRequested();

				AddFileToArchive(archive, file, engineDirectory, "Engine/");
				OnProgressChanged($"Added: {Path.GetFileName(file)}", ++processedFiles, totalFileCount);
			}

			// Add launcher
			if (hasLauncherFile)
			{
				cancellationToken.ThrowIfCancellationRequested();

				string launcherFileName = Path.GetFileName(launchFilePath);
				archive.CreateEntryFromFile(launchFilePath, launcherFileName, CompressionLevel.Optimal);

				OnProgressChanged($"Added: {launcherFileName}", ++processedFiles, totalFileCount);
			}

			// Add README if provided
			if (hasReadmeContent)
			{
				cancellationToken.ThrowIfCancellationRequested();

				AddReadmeToArchive(archive, readmeText!);
				OnProgressChanged("Added: README.txt", ++processedFiles, totalFileCount);
			}
		}, cancellationToken);

		OnProgressChanged("Archive creation complete!", totalFileCount, totalFileCount);
	}

	private static int CalculateTotalFileCount(
		IReadOnlyList<string> importantFolders,
		IReadOnlyList<string> importantFiles,
		bool hasLauncherFile,
		bool hasReadmeContent,
		bool hasSavesFolder)
	{
		int count = importantFiles.Count +
			(hasLauncherFile ? 1 : 0) +
			(hasReadmeContent ? 1 : 0) +
			(hasSavesFolder ? 1 : 0);

		// Count files in folders
		count += importantFolders
			.Where(Directory.Exists)
			.Sum(folder => Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length);

		return count;
	}

	private static void AddFileToArchive(ZipArchive archive, string filePath, string baseDirectory, string archivePrefix)
	{
		string relativePath = filePath[baseDirectory.Length..].Trim('\\');
		string entryName = archivePrefix + relativePath.Replace('\\', '/');
		archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
	}

	private static void AddReadmeToArchive(ZipArchive archive, string readmeText)
	{
		ZipArchiveEntry readmeEntry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
		using var writer = new StreamWriter(readmeEntry.Open());
		writer.Write(readmeText);
	}

	protected abstract IReadOnlyList<string> GetImportantFolders(string engineDirectory);
	protected abstract IReadOnlyList<string> GetImportantFiles(string engineDirectory);

	/// <summary>
	/// Gets the common files that are present in all game versions.
	/// Derived classes should call this and add their specific files.
	/// </summary>
	protected virtual IReadOnlyList<string> GetCommonFiles(string engineDirectory) => new List<string>
	{
		Path.Combine(engineDirectory, "splash.bmp"),
		Path.Combine(engineDirectory, "splash.xml")
	};

	protected virtual bool CreateSavesFolder() => false;
}

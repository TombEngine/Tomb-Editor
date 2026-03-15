using System;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

/// <summary>
/// Event args for archive creation progress updates.
/// </summary>
public class ArchiveProgressEventArgs : EventArgs
{
	public string CurrentOperation { get; set; } = string.Empty;
	public int FilesProcessed { get; set; }
	public int TotalFiles { get; set; }

	public int PercentComplete => TotalFiles > 0 ? FilesProcessed * 100 / TotalFiles : 0;
}

/// <summary>
/// Provides functionality for creating game archives for distribution.
/// </summary>
public interface IGameArchiveService
{
	/// <summary>
	/// Event raised when progress is updated during archive creation.
	/// </summary>
	event EventHandler<ArchiveProgressEventArgs>? ProgressChanged;

	/// <summary>
	/// Creates a game archive containing all necessary files for distribution.
	/// </summary>
	/// <param name="project">The project to archive.</param>
	/// <param name="outputPath">The destination path for the archive (.zip).</param>
	/// <param name="readmeText">Optional README.txt content to include in the archive.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
	Task CreateGameArchiveAsync(
		IGameProject project,
		string outputPath,
		string? readmeText = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks if this service supports the given game version.
	/// </summary>
	/// <param name="project">The project to check.</param>
	/// <returns><see langword="true"/> if supported; otherwise, <see langword="false"/>.</returns>
	bool SupportsGameVersion(IGameProject project);
}

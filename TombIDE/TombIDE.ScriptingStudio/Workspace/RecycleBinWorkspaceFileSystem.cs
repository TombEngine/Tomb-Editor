#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Documents.FileSystem;

namespace TombIDE.ScriptingStudio.Workspace;

/// <summary>
/// Decorates an <see cref="IWorkspaceFileSystem"/> with Windows shell recycle-bin deletion so the
/// workspace document authority removes files and directories through the recycle bin instead of
/// deleting them permanently. The workspace library itself is host-neutral and deletes permanently.
/// </summary>
internal sealed class RecycleBinWorkspaceFileSystem : IWorkspaceFileSystem
{
	private readonly IWorkspaceFileSystem _inner;
	private readonly Action<string> _sendFileToRecycleBin;
	private readonly Action<string> _sendDirectoryToRecycleBin;

	public RecycleBinWorkspaceFileSystem(IWorkspaceFileSystem inner)
		: this(inner, SendFileToRecycleBin, SendDirectoryToRecycleBin)
	{ }

	internal RecycleBinWorkspaceFileSystem(
		IWorkspaceFileSystem inner,
		Action<string> sendFileToRecycleBin,
		Action<string> sendDirectoryToRecycleBin)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_sendFileToRecycleBin = sendFileToRecycleBin ?? throw new ArgumentNullException(nameof(sendFileToRecycleBin));
		_sendDirectoryToRecycleBin = sendDirectoryToRecycleBin ?? throw new ArgumentNullException(nameof(sendDirectoryToRecycleBin));
	}

	public Task<WorkspaceFileReadResult> ReadAsync(string path, CancellationToken cancellationToken)
		=> _inner.ReadAsync(path, cancellationToken);

	public Task<FileStamp> CaptureStampAsync(string path, CancellationToken cancellationToken)
		=> _inner.CaptureStampAsync(path, cancellationToken);

	public Task<WorkspaceTemporaryFile> WriteTemporaryAsync(
		string destinationPath,
		ReadOnlyMemory<byte> content,
		CancellationToken cancellationToken)
		=> _inner.WriteTemporaryAsync(destinationPath, content, cancellationToken);

	public Task<WorkspaceFileReplacementResult> ReplaceFileAsync(
		WorkspaceTemporaryFile temporaryFile,
		string destinationPath,
		FileStamp expectedStamp,
		CancellationToken cancellationToken)
		=> _inner.ReplaceFileAsync(temporaryFile, destinationPath, expectedStamp, cancellationToken);

	public Task<WorkspaceFileMoveResult> MoveAsync(
		string sourcePath,
		string destinationPath,
		FileStamp expectedSourceStamp,
		CancellationToken cancellationToken)
		=> _inner.MoveAsync(sourcePath, destinationPath, expectedSourceStamp, cancellationToken);

	public Task<WorkspaceFileMoveResult> MoveDirectoryAsync(
		string sourcePath,
		string destinationPath,
		CancellationToken cancellationToken)
		=> _inner.MoveDirectoryAsync(sourcePath, destinationPath, cancellationToken);

	public async Task<WorkspaceFileDeleteResult> DeleteAsync(
		string path,
		FileStamp expectedStamp,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(path);

		// The shell recycle bin exists only on Windows; elsewhere the inner file system deletes
		// permanently so the decorator stays usable in cross-platform test runs.
		if (!OperatingSystem.IsWindows())
			return await _inner.DeleteAsync(path, expectedStamp, cancellationToken).ConfigureAwait(false);

		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			FileStamp actualStamp = await _inner.CaptureStampAsync(path, cancellationToken).ConfigureAwait(false);
			if (actualStamp != expectedStamp)
				return new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.ExternalFileConflict, actualStamp);

			if (File.Exists(path))
				_sendFileToRecycleBin(path);

			return new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Deleted);
		}
		catch (OperationCanceledException)
		{
			return new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Canceled);
		}
		catch (Exception exception)
		{
			return new WorkspaceFileDeleteResult(
				WorkspaceFileDeleteOutcome.DeleteFailed,
				Failure: new WorkspaceOperationFailure("DeleteFailed", exception.Message, exception));
		}
	}

	public Task<WorkspaceFileDeleteResult> DeleteDirectoryAsync(string path, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(path);

		if (!OperatingSystem.IsWindows())
			return _inner.DeleteDirectoryAsync(path, cancellationToken);

		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (Directory.Exists(path))
				_sendDirectoryToRecycleBin(path);

			return Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Deleted));
		}
		catch (OperationCanceledException)
		{
			return Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Canceled));
		}
		catch (Exception exception)
		{
			return Task.FromResult(new WorkspaceFileDeleteResult(
				WorkspaceFileDeleteOutcome.DeleteFailed,
				Failure: new WorkspaceOperationFailure("DeleteFailed", exception.Message, exception)));
		}
	}

	public Task DeleteTemporaryAsync(WorkspaceTemporaryFile temporaryFile)
		=> _inner.DeleteTemporaryAsync(temporaryFile);

	// UIOption.OnlyErrorDialogs maps to FOF_SILENT | FOF_NOCONFIRMATION in the shell layer, so the
	// recycle-bin operation runs silently and shows no user interface.
	private static void SendFileToRecycleBin(string path)
		=> FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

	private static void SendDirectoryToRecycleBin(string path)
		=> FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
}

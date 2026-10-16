#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nickelony.IDEKit.Workspace;
using Nickelony.IDEKit.Workspace.Documents;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Documents.FileSystem;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class RecycleBinWorkspaceFileSystemTests
{
	private static readonly FileStamp s_stamp = new(true, 4, DateTime.UnixEpoch, "hash");

	[TestMethod]
	public async Task DeleteAsync_ExistingFile_SendsToRecycleBinAfterStampCheck()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}.txt");
		File.WriteAllText(path, "data");
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem { CurrentStamp = s_stamp },
			recycled.Add,
			_ => { });

		try
		{
			WorkspaceFileDeleteResult result = await fileSystem.DeleteAsync(path, s_stamp, CancellationToken.None);

			Assert.AreEqual(WorkspaceFileDeleteOutcome.Deleted, result.Outcome);
			CollectionAssert.AreEqual(new[] { path }, recycled);
			Assert.IsTrue(File.Exists(path), "The shell callback is a test stub and must not delete the file.");
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public async Task DeleteAsync_StampMismatch_ReportsConflictWithoutShellCall()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}.txt");
		File.WriteAllText(path, "data");
		FileStamp observedStamp = new(true, 9, DateTime.UnixEpoch.AddMinutes(1), "changed");
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem { CurrentStamp = observedStamp },
			recycled.Add,
			_ => { });

		try
		{
			WorkspaceFileDeleteResult result = await fileSystem.DeleteAsync(path, s_stamp, CancellationToken.None);

			Assert.AreEqual(WorkspaceFileDeleteOutcome.ExternalFileConflict, result.Outcome);
			Assert.AreEqual(observedStamp, result.ObservedOnDiskStamp);
			Assert.AreEqual(0, recycled.Count);
			Assert.IsTrue(File.Exists(path));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public async Task DeleteAsync_MissingFile_IsNoOp()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}.txt");
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem { CurrentStamp = s_stamp },
			recycled.Add,
			_ => { });

		WorkspaceFileDeleteResult result = await fileSystem.DeleteAsync(path, s_stamp, CancellationToken.None);

		Assert.AreEqual(WorkspaceFileDeleteOutcome.Deleted, result.Outcome);
		Assert.AreEqual(0, recycled.Count);
	}

	[TestMethod]
	public async Task DeleteAsync_ShellFailure_ReportsDeleteFailed()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}.txt");
		File.WriteAllText(path, "data");
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem { CurrentStamp = s_stamp },
			_ => throw new IOException("The shell operation failed."),
			_ => { });

		try
		{
			WorkspaceFileDeleteResult result = await fileSystem.DeleteAsync(path, s_stamp, CancellationToken.None);

			Assert.AreEqual(WorkspaceFileDeleteOutcome.DeleteFailed, result.Outcome);
			Assert.AreEqual(WorkspaceOperationFailureCodes.DeleteFailed, result.Failure!.Code);
			Assert.IsTrue(File.Exists(path));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public async Task DeleteAsync_CancelledToken_ReportsCanceledWithoutShellCall()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}.txt");
		File.WriteAllText(path, "data");
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem { CurrentStamp = s_stamp },
			recycled.Add,
			_ => { });
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		try
		{
			WorkspaceFileDeleteResult result = await fileSystem.DeleteAsync(path, s_stamp, cancellation.Token);

			Assert.AreEqual(WorkspaceFileDeleteOutcome.Canceled, result.Outcome);
			Assert.AreEqual(0, recycled.Count);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[TestMethod]
	public async Task DeleteDirectoryAsync_ExistingDirectory_SendsToRecycleBin()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}");
		Directory.CreateDirectory(path);
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem(),
			_ => { },
			recycled.Add);

		try
		{
			WorkspaceFileDeleteResult result = await fileSystem.DeleteDirectoryAsync(path, CancellationToken.None);

			Assert.AreEqual(WorkspaceFileDeleteOutcome.Deleted, result.Outcome);
			CollectionAssert.AreEqual(new[] { path }, recycled);
			Assert.IsTrue(Directory.Exists(path), "The shell callback is a test stub and must not delete the directory.");
		}
		finally
		{
			Directory.Delete(path, recursive: true);
		}
	}

	[TestMethod]
	public async Task DeleteDirectoryAsync_MissingDirectory_IsNoOp()
	{
		string path = Path.Combine(Path.GetTempPath(), $"idekit-workspace-{Guid.NewGuid():N}");
		var recycled = new List<string>();
		var fileSystem = new RecycleBinWorkspaceFileSystem(
			new StubFileSystem(),
			_ => { },
			recycled.Add);

		WorkspaceFileDeleteResult result = await fileSystem.DeleteDirectoryAsync(path, CancellationToken.None);

		Assert.AreEqual(WorkspaceFileDeleteOutcome.Deleted, result.Outcome);
		Assert.AreEqual(0, recycled.Count);
	}

	[TestMethod]
	public void HostComposition_ResolvesRecycleBinFileSystemDecorator()
	{
		var services = new ServiceCollection();
		services.AddScriptingStudioHostComposition();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		using IServiceScope scope = serviceProvider.CreateScope();

		IWorkspaceFileSystem fileSystem = scope.ServiceProvider.GetRequiredService<IWorkspaceFileSystem>();

		Assert.IsInstanceOfType(fileSystem, typeof(RecycleBinWorkspaceFileSystem));
	}

	private sealed class StubFileSystem : IWorkspaceFileSystem
	{
		public FileStamp CurrentStamp { get; set; } = FileStamp.Missing;

		public Task<WorkspaceFileReadResult> ReadAsync(string path, CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public Task<FileStamp> CaptureStampAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(CurrentStamp);
		}

		public Task<WorkspaceTemporaryFile> WriteTemporaryAsync(
			string destinationPath,
			ReadOnlyMemory<byte> content,
			CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public Task<WorkspaceFileReplacementResult> ReplaceFileAsync(
			WorkspaceTemporaryFile temporaryFile,
			string destinationPath,
			FileStamp expectedStamp,
			CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public Task<WorkspaceFileMoveResult> MoveAsync(
			string sourcePath,
			string destinationPath,
			FileStamp expectedSourceStamp,
			CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public Task<WorkspaceFileMoveResult> MoveDirectoryAsync(
			string sourcePath,
			string destinationPath,
			CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		public Task<WorkspaceFileDeleteResult> DeleteAsync(
			string path,
			FileStamp expectedStamp,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Deleted));
		}

		public Task<WorkspaceFileDeleteResult> DeleteDirectoryAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(new WorkspaceFileDeleteResult(WorkspaceFileDeleteOutcome.Deleted));
		}

		public Task DeleteTemporaryAsync(WorkspaceTemporaryFile temporaryFile)
			=> Task.CompletedTask;
	}
}

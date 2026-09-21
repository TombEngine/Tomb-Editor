using Nickelony.IDEKit.Processes;
using System;
using System.IO;
using System.Threading.Tasks;
using TombLib.Scripting.GameFlowScript.Compilers;

namespace TombLib.Tests.GameFlow;

/// <summary>
/// Direct tests for <see cref="ScriptCompiler"/> process orchestration through the injected
/// <see cref="IProcessRunner"/> seam: no-handle outcomes, paused and timed waits, start configuration,
/// and staging-directory cleanup.
/// </summary>
[TestClass]
public class ScriptCompilerProcessTests
{
	[TestMethod]
	public void RunCompileWorkflow_NoProcessHandle_ReturnsFalseAndCleansStagingDirectory()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeProcessRunner(_ => new ProcessRunResult { Outcome = ProcessRunOutcome.NoProcessHandle }));

			Assert.IsFalse(result);
			Assert.IsFalse(File.Exists(Path.Combine(gameflowDirectory, "compile.bat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_Paused_ProcessRunsToCompletionAndCopiesOutput()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var runner = new FakeProcessRunner(_ =>
			{
				File.WriteAllText(Path.Combine(gameflowDirectory, "tombpc.dat"), "compiled data");
				return new ProcessRunResult { Outcome = ProcessRunOutcome.Exited };
			});

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: true),
				"tombpc.dat",
				"gameFlow.exe",
				pause: true,
				runner);

			Assert.IsTrue(result);
			Assert.IsTrue(runner.RunCalled);
			Assert.IsNotNull(runner.LastRequest);
			Assert.IsNull(runner.LastRequest.Timeout);
			Assert.AreEqual("compiled data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_TimedWait_ProcessExitsInTimeAndCopiesOutput()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var runner = new FakeProcessRunner(_ =>
			{
				File.WriteAllText(Path.Combine(gameflowDirectory, "tombpc.dat"), "compiled data");
				return new ProcessRunResult { Outcome = ProcessRunOutcome.Exited };
			});

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				runner);

			Assert.IsTrue(result);
			Assert.IsTrue(runner.RunCalled);
			Assert.IsNotNull(runner.LastRequest);
			Assert.AreEqual(TimeSpan.FromMilliseconds(300000), runner.LastRequest.Timeout);
			Assert.AreEqual("compiled data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_StaleStagingOutput_IsNotCopiedWhenCompilerProducesNothing()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			File.WriteAllText(Path.Combine(gameflowDirectory, "tombpc.dat"), "stale data");
			File.WriteAllText(Path.Combine(outputDirectory, "tombpc.dat"), "existing project data");
			var runner = new FakeProcessRunner(_ => new ProcessRunResult { Outcome = ProcessRunOutcome.Exited });

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				runner);

			Assert.IsFalse(result);
			Assert.AreEqual("existing project data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
			Assert.IsFalse(File.Exists(Path.Combine(gameflowDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_ProcessFailure_CleansStagingDirectoryAndDisposesProcess()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var runner = new FakeProcessRunner(_ => throw new InvalidOperationException("process wait failed"));

			Assert.ThrowsException<InvalidOperationException>(() => ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				runner));

			Assert.IsTrue(runner.RunCalled);
			Assert.IsFalse(File.Exists(Path.Combine(gameflowDirectory, "compile.bat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_StartsBatchWithStagingDirectory()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var runner = new FakeProcessRunner(_ => new ProcessRunResult { Outcome = ProcessRunOutcome.Exited });

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				runner);

			Assert.IsFalse(result);

			Assert.IsTrue(runner.RunCalled);
			Assert.IsNotNull(runner.LastRequest);
			Assert.IsNotNull(runner.LastRequest.FileName);
			Assert.IsNotNull(runner.LastRequest.WorkingDirectory);
			Assert.AreEqual(Path.Combine(gameflowDirectory, "compile.bat"), runner.LastRequest.FileName);
			Assert.AreEqual(gameflowDirectory, runner.LastRequest.WorkingDirectory);
			Assert.IsTrue(runner.LastRequest.UseShellExecute);
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	private static (string BaseDirectory, string InputDirectory, string GameflowDirectory, string OutputDirectory) CreateWorkflowDirectories()
	{
		string baseDirectory = CreateTempDirectory();
		string inputDirectory = CreateTempDirectory(baseDirectory);
		string gameflowDirectory = CreateTempDirectory(baseDirectory);
		string outputDirectory = CreateTempDirectory(baseDirectory);

		File.WriteAllText(Path.Combine(inputDirectory, "Script.txt"), "script content");

		return (baseDirectory, inputDirectory, gameflowDirectory, outputDirectory);
	}

	private static string CreateTempDirectory(string? parent = null)
	{
		string path = Path.Combine(parent ?? Path.GetTempPath(), "ScriptCompilerProcessTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(path);
		return path;
	}

	private sealed class FakeProcessRunner : IProcessRunner
	{
		private readonly Func<ProcessRunRequest, ProcessRunResult> _run;

		public FakeProcessRunner(Func<ProcessRunRequest, ProcessRunResult> run)
			=> _run = run;

		public ProcessRunRequest? LastRequest { get; private set; }

		public bool RunCalled { get; private set; }

		public ProcessRunResult Run(ProcessRunRequest request, CancellationToken cancellationToken = default)
			=> RunAsync(request, cancellationToken).GetAwaiter().GetResult();

		public Task<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken = default)
		{
			RunCalled = true;
			LastRequest = request;
			return Task.FromResult(_run(request));
		}

		public IProcessHandle Start(ProcessRunRequest request)
			=> throw new NotSupportedException("The fake runner only supports Run.");
	}
}

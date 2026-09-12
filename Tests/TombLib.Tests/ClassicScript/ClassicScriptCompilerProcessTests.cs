using Nickelony.IDEKit.Tooling;
using System;
using System.IO;
using System.Text;
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptCompilerProcessTests
{
	static ClassicScriptCompilerProcessTests()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	[TestMethod]
	public void NGCompiler_StartFailure_PropagatesWithoutLaunchingARealProcess()
	{
		using var directories = CompilerDirectories.Create();

		Assert.ThrowsException<InvalidOperationException>(() => NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			new FakeProcessRunner(_ => throw new InvalidOperationException("process start failed"))));
	}

	[TestMethod]
	public void NGCompiler_SuccessfulProcess_CopiesCompiledFilesAndCapturesStartInfo()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt"), "Compilation complete");
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "Script.dat"), "script data");
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "English.dat"), "english data");
			return new ProcessRunResult { Started = true };
		});

		bool result = NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			runner);

		Assert.IsTrue(result);
		Assert.IsTrue(runner.RunCalled);
		Assert.IsNotNull(runner.LastRequest);
		Assert.AreEqual(directories.Paths.NGCExecutable, runner.LastRequest.FileName);
		Assert.AreEqual("\"" + Path.Combine(directories.Paths.VGEScriptDirectory, "Script.txt") + "\" -Log -NoMsgBox -NoWait -Concise", runner.LastRequest.Arguments);
		Assert.AreEqual("script data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
		Assert.AreEqual("english data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "English.dat")));
	}

	[TestMethod]
	public void NGCompiler_ErrorLog_ReturnsFalseAndStillCopiesCompiledFiles()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt"), "ERROR: compiler failure");
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "Script.dat"), "script data");
			return new ProcessRunResult { Started = true };
		});

		bool result = NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			runner);

		Assert.IsFalse(result);
		Assert.AreEqual("script data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
	}

	[TestMethod]
	public void NGCompiler_MissingPrimaryOutput_ReturnsFalseAndDoesNotTreatStaleProjectOutputAsSuccess()
	{
		using var directories = CompilerDirectories.Create();
		File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "Script.dat"), "stale compiler data");
		File.WriteAllText(Path.Combine(directories.OutputDirectory, "Script.dat"), "stale data");
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt"), "Compilation complete");
			return new ProcessRunResult { Started = true };
		});

		bool result = NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			runner);

		Assert.IsFalse(result);
		Assert.AreEqual("stale data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
		Assert.IsFalse(File.Exists(Path.Combine(directories.Paths.VGEDirectory, "Script.dat")));
	}

	[TestMethod]
	public void NGCompiler_MissingOptionalEnglishOutput_ReturnsTrueAndCopiesPrimaryOutput()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt"), "Compilation complete");
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "Script.dat"), "script data");
			return new ProcessRunResult { Started = true };
		});

		bool result = NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			runner);

		Assert.IsTrue(result);
		Assert.AreEqual("script data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
		Assert.IsFalse(File.Exists(Path.Combine(directories.OutputDirectory, "English.dat")));
	}

	[TestMethod]
	public void NGCompiler_NonAsciiLog_UsesWindows1252()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllBytes(
				Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt"),
				Encoding.GetEncoding(1252).GetBytes("Compilation caf\u00E9"));
			File.WriteAllText(Path.Combine(directories.Paths.VGEDirectory, "Script.dat"), "script data");
			return new ProcessRunResult { Started = true };
		});

		bool result = NGCompiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			newIncludeMethod: false,
			directories.Paths,
			runner);

		Assert.IsTrue(result);
		Assert.AreEqual("Compilation caf\u00E9", File.ReadAllText(Path.Combine(directories.Paths.VGEDirectory, "LastCompilerLog.txt")));
	}

	[TestMethod]
	public void TR4Compiler_StartFailure_PropagatesWithoutLaunchingARealProcess()
	{
		using var directories = CompilerDirectories.Create();
		File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "stale-output.tmp"), "stale output");

		InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
			() => TR4Compiler.CompileCore(
				directories.InputDirectory,
				directories.OutputDirectory,
				directories.Paths,
				new FakeProcessRunner(_ => throw new InvalidOperationException("process start failed"))));

		StringAssert.Contains(exception.Message, "process start failed");
		Assert.IsFalse(File.Exists(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "stale-output.tmp")));
	}

	[TestMethod]
	public void TR4Compiler_SuccessfulProcess_CopiesCompiledFilesAndPreservesArguments()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "logs.txt"), "Compilation complete");
			File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "Script.dat"), "script data");
			File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "English.dat"), "english data");
			return new ProcessRunResult { Started = true };
		});

		string result = TR4Compiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			directories.Paths,
			runner);

		Assert.AreEqual("Compilation complete", result);
		Assert.IsTrue(runner.RunCalled);
		Assert.IsNotNull(runner.LastRequest);
		Assert.AreEqual(directories.Paths.DOSBoxExecutable, runner.LastRequest.FileName);
		Assert.AreEqual(directories.Paths.DOSDirectory, runner.LastRequest.WorkingDirectory);
		StringAssert.Contains(runner.LastRequest.Arguments, directories.Paths.TR4ScriptCompilerDirectory);
		Assert.AreEqual("script data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
		Assert.AreEqual("english data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "English.dat")));
	}

	[TestMethod]
	public void TR4Compiler_MissingOutput_ReturnsLogWithoutCopyingFiles()
	{
		using var directories = CompilerDirectories.Create();
		File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "Script.dat"), "stale script data");
		File.WriteAllText(Path.Combine(directories.OutputDirectory, "Script.dat"), "existing project data");
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "logs.txt"), "Compilation complete");
			return new ProcessRunResult { Started = true };
		});

		string result = TR4Compiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			directories.Paths,
			runner);

		Assert.AreEqual("Compilation complete", result);
		Assert.AreEqual("existing project data", File.ReadAllText(Path.Combine(directories.OutputDirectory, "Script.dat")));
		Assert.IsFalse(File.Exists(Path.Combine(directories.OutputDirectory, "English.dat")));
		Assert.IsFalse(File.Exists(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "Script.dat")));
	}

	[TestMethod]
	public void TR4Compiler_MissingLog_CleansStagingDirectoryBeforePropagatingFailure()
	{
		using var directories = CompilerDirectories.Create();
		File.WriteAllText(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "stale-output.tmp"), "stale output");

		Assert.ThrowsException<FileNotFoundException>(() => TR4Compiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			directories.Paths,
			new FakeProcessRunner(_ => new ProcessRunResult { Started = true })));

		Assert.IsFalse(File.Exists(Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "stale-output.tmp")));
	}

	[TestMethod]
	public void TR4Compiler_NonAsciiLog_UsesWindows1252()
	{
		using var directories = CompilerDirectories.Create();
		var runner = new FakeProcessRunner(_ =>
		{
			File.WriteAllBytes(
				Path.Combine(directories.Paths.TR4ScriptCompilerDirectory, "logs.txt"),
				Encoding.GetEncoding(1252).GetBytes("Compilation caf\u00E9"));
			return new ProcessRunResult { Started = true };
		});

		string result = TR4Compiler.CompileCore(
			directories.InputDirectory,
			directories.OutputDirectory,
			directories.Paths,
			runner);

		Assert.AreEqual("Compilation caf\u00E9", result);
	}

	private sealed class CompilerDirectories : IDisposable
	{
		private CompilerDirectories(string baseDirectory)
		{
			BaseDirectory = baseDirectory;
			InputDirectory = CreateDirectory("Input");
			OutputDirectory = CreateDirectory("Output");
			Paths = new ClassicScriptCompilerPaths(CreateDirectory("Application"));
			File.WriteAllText(Path.Combine(InputDirectory, "Script.txt"), "script content");
			Directory.CreateDirectory(Paths.VGEScriptDirectory);
			Directory.CreateDirectory(Paths.VGEDirectory);
			Directory.CreateDirectory(Paths.TR4ScriptCompilerDirectory);
		}

		public string BaseDirectory { get; }

		public string InputDirectory { get; }

		public string OutputDirectory { get; }

		public ClassicScriptCompilerPaths Paths { get; }

		public static CompilerDirectories Create()
			=> new(Path.Combine(Path.GetTempPath(), "ClassicScriptCompilerProcessTests_" + Guid.NewGuid().ToString("N")));

		private string CreateDirectory(string name)
		{
			string path = Path.Combine(BaseDirectory, name);
			Directory.CreateDirectory(path);
			return path;
		}

		public void Dispose()
		{
			if (Directory.Exists(BaseDirectory))
				Directory.Delete(BaseDirectory, recursive: true);
		}
	}

	private sealed class FakeProcessRunner : IProcessRunner
	{
		private readonly Func<ProcessRunRequest, ProcessRunResult> _run;

		public FakeProcessRunner(Func<ProcessRunRequest, ProcessRunResult> run)
			=> _run = run;

		public ProcessRunRequest? LastRequest { get; private set; }

		public bool RunCalled { get; private set; }

		public ProcessRunResult Run(ProcessRunRequest request, CancellationToken cancellationToken = default)
		{
			RunCalled = true;
			LastRequest = request;
			return _run(request);
		}

		public IProcessHandle Start(ProcessRunRequest request)
			=> throw new NotSupportedException("The fake runner only supports Run.");
	}
}
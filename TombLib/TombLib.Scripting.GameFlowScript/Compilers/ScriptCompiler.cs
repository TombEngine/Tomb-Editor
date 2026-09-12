using Nickelony.IDEKit.Tooling;
using NLog;
using System;
using System.IO;
using TombLib.Scripting.UI.IO;

namespace TombLib.Scripting.GameFlowScript.Compilers;

/// <summary>
/// Compiles GameFlow scripts by driving the external GameFlow compiler.
/// </summary>
public static class ScriptCompiler
{
	private const int ProcessTimeoutMilliseconds = 300000;

	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly IProcessRunner s_processRunner = new ProcessRunner();

	/// <summary>
	/// Compiles a classic GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="isTR3">Whether the script targets Tomb Raider 3.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
	public static bool ClassicCompile(string inputDirectory, string outputDirectory, bool isTR3, bool pause = true)
	{
		return RunCompileWorkflow(
			inputDirectory,
			outputDirectory,
			GameFlowCompilerPaths.Default.GameFlow2Directory,
			BuildClassicBatchContent(isTR3, pause),
			"tombpc.dat",
			"gameFlow.exe",
			pause,
			s_processRunner);
	}

	/// <summary>
	/// Compiles a TR3 version 2+ GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
	public static bool CompileTR3Version2Plus(string inputDirectory, string outputDirectory, bool pause = true)
	{
		return RunCompileWorkflow(
			inputDirectory,
			outputDirectory,
			GameFlowCompilerPaths.Default.GameFlow3Directory,
			BuildTR3Version2PlusBatchContent(pause),
			"Script.dat",
			"TRGameFlow.exe",
			pause,
			s_processRunner);
	}

	/// <summary>
	/// Builds the compiler batch content for a classic GameFlow compile.
	/// </summary>
	/// <param name="isTR3">Whether the script targets Tomb Raider 3.</param>
	/// <param name="pause">Whether the batch should pause when it finishes.</param>
	/// <returns>The batch file content.</returns>
	internal static string BuildClassicBatchContent(bool isTR3, bool pause)
		=> "gameflow -Game " + (isTR3 ? 3 : 2) + "\n" + (pause ? "@pause" : string.Empty);

	/// <summary>
	/// Builds the compiler batch content for a TR3 version 2+ compile.
	/// </summary>
	/// <param name="pause">Whether the batch should pause when it finishes.</param>
	/// <returns>The batch file content.</returns>
	internal static string BuildTR3Version2PlusBatchContent(bool pause)
		=> "TRGameFlow Script.txt\n" + (pause ? "@pause" : string.Empty);

	/// <summary>
	/// Detects and copies the compiled data file into the output directory.
	/// </summary>
	/// <param name="gameflowDirectory">The directory that contains the compiled data file.</param>
	/// <param name="outputDirectory">The directory that receives the copied data file.</param>
	/// <param name="compiledScriptFileName">The name of the compiled data file.</param>
	/// <returns><c>true</c> when the compiled data file existed and was copied; otherwise, <c>false</c>.</returns>
	internal static bool FinalizeCompileResult(string gameflowDirectory, string outputDirectory, string compiledScriptFileName)
	{
		string compiledScriptFilePath = Path.Combine(gameflowDirectory, compiledScriptFileName);

		if (!File.Exists(compiledScriptFilePath))
			return false;

		File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
		return true;
	}

	/// <summary>
	/// Runs the full compile workflow: copies the script directory, writes and starts the compiler
	/// batch, waits for the compiler (bounding non-paused runs by a timeout), and finalizes the
	/// compiled data file.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="gameflowDirectory">The staging directory that hosts the compiler batch.</param>
	/// <param name="batchFileContent">The compiler batch content to write.</param>
	/// <param name="compiledScriptFileName">The name of the compiled data file.</param>
	/// <param name="keptExecutableName">The executable name preserved when the staging directory is cleaned.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <param name="processRunner">The process runner used to start the compiler batch.</param>
	/// <returns><c>true</c> when the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
	internal static bool RunCompileWorkflow(
		string inputDirectory,
		string outputDirectory,
		string gameflowDirectory,
		string batchFileContent,
		string compiledScriptFileName,
		string keptExecutableName,
		bool pause,
		IProcessRunner processRunner)
	{
		ScriptDirectoryCopier.CopyScriptDirectory(inputDirectory, gameflowDirectory, clearTarget: false);

		string batchFilePath = Path.Combine(gameflowDirectory, "compile.bat");
		File.WriteAllText(batchFilePath, batchFileContent);
		File.Delete(Path.Combine(gameflowDirectory, compiledScriptFileName));

		// In interactive (paused) mode the batch waits for the user to dismiss the compiler
		// window, so it runs to completion; otherwise a runaway compiler is bounded by a
		// timeout and the runner kills the whole process tree so no child compiler keeps running.
		var request = new ProcessRunRequest
		{
			FileName = batchFilePath,
			WorkingDirectory = gameflowDirectory,
			UseShellExecute = true,
			Timeout = pause ? null : TimeSpan.FromMilliseconds(ProcessTimeoutMilliseconds)
		};

		try
		{
			ProcessRunResult result = processRunner.Run(request);

			if (!result.Started || result.TimedOut || result.Cancelled)
				return false;

			return FinalizeCompileResult(gameflowDirectory, outputDirectory, compiledScriptFileName);
		}
		finally
		{
			ScriptDirectoryCopier.ClearDirectoryExcept(gameflowDirectory, name => name.Equals(keptExecutableName, StringComparison.OrdinalIgnoreCase));
		}
	}
}

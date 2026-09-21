using System;
using System.IO;
using TombLib.Scripting.GameFlowScript.Compilers;

namespace TombLib.Tests.GameFlow;

/// <summary>
/// Direct tests for <see cref="ScriptCompiler"/> command construction and
/// compile-result output detection/copying through the extracted seams.
/// </summary>
[TestClass]
public class ScriptCompilerTests
{
	// Batch command construction

	[TestMethod]
	public void BuildClassicBatchContent_TR2_NoPause_ReturnsExpectedCommand()
	{
		Assert.AreEqual("gameflow -Game 2\n", ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false));
	}

	[TestMethod]
	public void BuildClassicBatchContent_TR2_Paused_AppendsPause()
	{
		Assert.AreEqual("gameflow -Game 2\n@pause", ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: true));
	}

	[TestMethod]
	public void BuildClassicBatchContent_TR3_ReturnsGame3()
	{
		Assert.AreEqual("gameflow -Game 3\n", ScriptCompiler.BuildClassicBatchContent(isTR3: true, pause: false));
	}

	[TestMethod]
	public void BuildTR3Version2PlusBatchContent_NoPause_ReturnsExpectedCommand()
	{
		Assert.AreEqual("TRGameFlow Script.txt\n", ScriptCompiler.BuildTR3Version2PlusBatchContent(pause: false));
	}

	[TestMethod]
	public void BuildTR3Version2PlusBatchContent_Paused_AppendsPause()
	{
		Assert.AreEqual("TRGameFlow Script.txt\n@pause", ScriptCompiler.BuildTR3Version2PlusBatchContent(pause: true));
	}

	// Compile result output detection and copying

	[TestMethod]
	public void FinalizeCompileResult_MissingOutput_ReturnsFalse()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string gameflowDirectory = CreateTempDirectory(baseDirectory);
			string outputDirectory = CreateTempDirectory(baseDirectory);

			Assert.IsFalse(ScriptCompiler.FinalizeCompileResult(gameflowDirectory, outputDirectory, "Script.dat"));
			Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void FinalizeCompileResult_PresentOutput_CopiesToTombpcDat()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string gameflowDirectory = CreateTempDirectory(baseDirectory);
			string outputDirectory = CreateTempDirectory(baseDirectory);

			string compiledPath = Path.Combine(gameflowDirectory, "Script.dat");
			File.WriteAllText(compiledPath, "compiled data");

			Assert.IsTrue(ScriptCompiler.FinalizeCompileResult(gameflowDirectory, outputDirectory, "Script.dat"));
			Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "tombpc.dat")));
			Assert.AreEqual("compiled data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	private static string CreateTempDirectory(string? parent = null)
	{
		string path = Path.Combine(parent ?? Path.GetTempPath(), "ScriptCompilerTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(path);
		return path;
	}
}

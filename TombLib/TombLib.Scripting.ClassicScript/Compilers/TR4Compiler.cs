using Nickelony.IDEKit.Tooling;
using System;
using System.IO;
using System.Text;
using TombLib.Scripting.UI.IO;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Compiles ClassicScript using the external TR4 compiler.
/// </summary>
public static class TR4Compiler
{
	private static readonly IProcessRunner s_processRunner = new ProcessRunner();
	/// <summary>
	/// Throws when the TR4 script compiler directory contains a character that is invalid for
	/// the DOSBox mount command.
	/// </summary>
	/// <param name="compilerDirectory">The path to the TR4 script compiler directory.</param>
	/// <exception cref="ArgumentException">The path contains an invalid path character.</exception>
	internal static void ThrowIfInvalidCompilerPath(string compilerDirectory)
	{
		if (compilerDirectory.Contains('\''))
			throw new ArgumentException(
				"The path to the TR4 script compiler contains an invalid path character. " +
				$"' is not a valid path character for this operation: '{compilerDirectory}'.",
				nameof(compilerDirectory));
	}

	/// <summary>
	/// Compiles the script at the given path with the TR4 compiler.
	/// </summary>
	/// <param name="projectScriptPath">The path of the project script directory.</param>
	/// <param name="projectEnginePath">The path of the project engine directory.</param>
	/// <returns>The compiler log content.</returns>
	public static string Compile(string projectScriptPath, string projectEnginePath)
	{
		return CompileCore(projectScriptPath, projectEnginePath, ClassicScriptCompilerPaths.Default, s_processRunner);
	}

	internal static string CompileCore(
		string projectScriptPath,
		string projectEnginePath,
		ClassicScriptCompilerPaths compilerPaths,
		IProcessRunner processRunner)
	{
		ArgumentNullException.ThrowIfNull(compilerPaths);
		ArgumentNullException.ThrowIfNull(processRunner);

		ThrowIfInvalidCompilerPath(compilerPaths.TR4ScriptCompilerDirectory);

		ScriptDirectoryCopier.CopyScriptDirectory(projectScriptPath, compilerPaths.TR4ScriptCompilerDirectory, clearTarget: false, CompilerFileCopy.CopyTextFormatted);
		File.Delete(Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "logs.txt"));
		File.Delete(Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "Script.dat"));
		File.Delete(Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "English.dat"));

		try
		{
			var request = new ProcessRunRequest
			{
				FileName = compilerPaths.DOSBoxExecutable,
				WorkingDirectory = compilerPaths.DOSDirectory,
				Arguments =
					$"-c \"mount C '{compilerPaths.TR4ScriptCompilerDirectory}'\" " +
					"-c \"C:\" " +
					"-c \"script script.txt >> logs.txt\" " +
					"-c \"exit\" " +
					"-noconsole",
				UseShellExecute = true
			};

			processRunner.Run(request);

			string logFilePath = Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "logs.txt");
			string logFileContent = File.ReadAllText(logFilePath, Encoding.GetEncoding(1252));

			string compiledScriptFilePath = Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "Script.dat");
			string compiledEnglishFilePath = Path.Combine(compilerPaths.TR4ScriptCompilerDirectory, "English.dat");

			if (File.Exists(compiledScriptFilePath))
				File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

			if (File.Exists(compiledEnglishFilePath))
				File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);

			return logFileContent;
		}
		finally
		{
			ScriptDirectoryCopier.ClearDirectoryExcept(
				compilerPaths.TR4ScriptCompilerDirectory,
				name => name.Equals("SCRIPT.EXE", StringComparison.OrdinalIgnoreCase)
					|| name.Equals("DOS4GW.EXE", StringComparison.OrdinalIgnoreCase));
		}
	}
}

using System;
using System.IO;

namespace TombLib.Scripting.GameFlowScript.Compilers;

/// <summary>
/// Resolves the GameFlow compiler installation paths relative to the supplied application root,
/// which defaults to the application base directory.
/// </summary>
internal sealed class GameFlowCompilerPaths
{
	/// <summary>
	/// Gets the default compiler path layout rooted at the application base directory.
	/// </summary>
	public static GameFlowCompilerPaths Default { get; } = new(AppContext.BaseDirectory);

	private readonly string _programDirectory;

	/// <summary>
	/// Initializes a new instance rooted at the supplied program directory.
	/// </summary>
	/// <param name="programDirectory">The application root directory.</param>
	public GameFlowCompilerPaths(string programDirectory) => _programDirectory = programDirectory;

	/// <summary>
	/// Gets the application root directory.
	/// </summary>
	public string ProgramDirectory => _programDirectory;

	/// <summary>
	/// Gets the TIDE subdirectory that hosts the GameFlow toolchains.
	/// </summary>
	public string TIDEDirectory => Path.Combine(_programDirectory, "TIDE");

	/// <summary>
	/// Gets the classic GameFlow 2 compiler directory.
	/// </summary>
	public string GameFlow2Directory => Path.Combine(TIDEDirectory, "GFL");

	/// <summary>
	/// Gets the TR3 GameFlow 3 compiler directory.
	/// </summary>
	public string GameFlow3Directory => Path.Combine(TIDEDirectory, "GF3");
}

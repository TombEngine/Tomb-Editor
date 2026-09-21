using System;
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombLib.Tests;

// Phase 1 compiler-precondition tests: prove the compiler guards throw specific exception types
// with useful messages without launching an external compiler.
[TestClass]
public class CompilerPreconditionTests
{
	[TestMethod]
	public void NGCompiler_ThrowIfLibrariesNotRegistered_ThrowsInvalidOperationException()
	{
		InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
			() => NGCompiler.ThrowIfLibrariesNotRegistered(librariesRegistered: false));

		Assert.IsTrue(exception.Message.Contains("required libraries", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void NGCompiler_ThrowIfLibrariesNotRegistered_RegisteredDoesNotThrow()
	{
		NGCompiler.ThrowIfLibrariesNotRegistered(librariesRegistered: true);
	}

	[TestMethod]
	public void TR4Compiler_ThrowIfInvalidCompilerPath_PathWithApostrophe_ThrowsArgumentException()
	{
		const string invalidPath = @"C:\Compiler's Directory\TR4";

		ArgumentException exception = Assert.ThrowsException<ArgumentException>(
			() => TR4Compiler.ThrowIfInvalidCompilerPath(invalidPath));

		Assert.IsTrue(exception.Message.Contains(invalidPath, StringComparison.Ordinal));
		Assert.IsTrue(exception.Message.Contains("invalid path character", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void TR4Compiler_ThrowIfInvalidCompilerPath_ValidPathDoesNotThrow()
	{
		TR4Compiler.ThrowIfInvalidCompilerPath(@"C:\CompilerDirectory\TR4");
	}
}

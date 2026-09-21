using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombLib.Tests;

[TestClass]
public class NGCompilerIncludeMergeTests
{
	[TestMethod]
	public void ReplaceIncludesWithFileContents_ExpandsNestedIncludesWithMarkers()
	{
		string directory = CreateTempDirectory();

		try
		{
			File.WriteAllText(Path.Combine(directory, "sub.txt"), "SUB_LINE\r\n#include \"sub2.txt\"");
			File.WriteAllText(Path.Combine(directory, "sub2.txt"), "SUB2_LINE");

			var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine(directory, "main.txt") };

			string[] result = NGCompiler.ReplaceIncludesWithFileContents(
				[
					"#include \"sub.txt\"",
					"AFTER_INCLUDE"
				],
				visitedFiles,
				directory);

			Assert.AreEqual(7, result.Length);
			CollectionAssert.Contains(result, "; // // // // <SUB.TXT> // // // //");
			CollectionAssert.Contains(result, "SUB_LINE");
			CollectionAssert.Contains(result, "; // // // // <SUB2.TXT> // // // //");
			CollectionAssert.Contains(result, "SUB2_LINE");
			CollectionAssert.Contains(result, "; // // // // </SUB2.TXT> // // // //");
			CollectionAssert.Contains(result, "; // // // // </SUB.TXT> // // // //");
			CollectionAssert.Contains(result, "AFTER_INCLUDE");
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[TestMethod]
	public void ReplaceIncludesWithFileContents_CircularIncludes_DoNotLoop()
	{
		string directory = CreateTempDirectory();

		try
		{
			File.WriteAllText(Path.Combine(directory, "b.txt"), "B_LINE\r\n#include \"a.txt\"");

			var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine(directory, "a.txt") };

			string[] result = NGCompiler.ReplaceIncludesWithFileContents(
				[
					"#include \"b.txt\"",
					"A_TAIL"
				],
				visitedFiles,
				directory);

			// The circular include is dropped instead of recursing forever, and the visited
			// content is expanded exactly once.
			Assert.AreEqual(4, result.Length);
			Assert.AreEqual(1, result.Count(line => line.Contains("B_LINE")));
			Assert.AreEqual(0, result.Count(line => line.Contains("<A.TXT>")));
			CollectionAssert.Contains(result, "A_TAIL");
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[TestMethod]
	public void ReplaceIncludesWithFileContents_MissingInclude_IsDropped()
	{
		string directory = CreateTempDirectory();

		try
		{
			var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			string[] result = NGCompiler.ReplaceIncludesWithFileContents(
				[
					"#include \"missing.txt\""
				],
				visitedFiles,
				directory);

			Assert.AreEqual(0, result.Length);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	private static string CreateTempDirectory()
		=> Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"ng-include-test-{Guid.NewGuid():N}")).FullName;
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.UI.IO;

namespace TombLib.Tests.IO;

/// <summary>
/// Direct tests for <see cref="ScriptDirectoryCopier"/> path mapping, backup
/// skipping, callback routing, and target clearing.
/// </summary>
[TestClass]
public class ScriptDirectoryCopierTests
{
	[TestMethod]
	public void CopyScriptDirectory_FlatFiles_AreCopiedToTarget()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(source);
			File.WriteAllText(Path.Combine(source, "a.txt"), "A");
			File.WriteAllText(Path.Combine(source, "b.txt"), "B");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false);

			Assert.AreEqual("A", File.ReadAllText(Path.Combine(target, "a.txt")));
			Assert.AreEqual("B", File.ReadAllText(Path.Combine(target, "b.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_NestedDirectories_PreserveRelativeLayout()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(Path.Combine(source, "sub", "inner"));
			File.WriteAllText(Path.Combine(source, "sub", "inner", "c.txt"), "C");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false);

			Assert.IsTrue(File.Exists(Path.Combine(target, "sub", "inner", "c.txt")));
			Assert.AreEqual("C", File.ReadAllText(Path.Combine(target, "sub", "inner", "c.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_NestedFolderMatchingSourceName_DoesNotDoubleReplace()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "scripts");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(Path.Combine(source, "scripts"));
			File.WriteAllText(Path.Combine(source, "scripts", "foo.txt"), "F");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false);

			Assert.IsTrue(File.Exists(Path.Combine(target, "scripts", "foo.txt")));
			Assert.IsFalse(File.Exists(Path.Combine(target, "target", "foo.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_BackupFiles_AreSkipped()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(source);
			File.WriteAllText(Path.Combine(source, "data.txt"), "D");
			File.WriteAllText(Path.Combine(source, "data.txt.backup"), "backup");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false);

			Assert.IsTrue(File.Exists(Path.Combine(target, "data.txt")));
			Assert.IsFalse(File.Exists(Path.Combine(target, "data.txt.backup")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_CopyFileCallback_ReceivesMappedPaths()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(Path.Combine(source, "sub"));
			File.WriteAllText(Path.Combine(source, "root.txt"), "R");
			File.WriteAllText(Path.Combine(source, "sub", "nested.txt"), "N");

			var pairs = new List<(string Source, string Target)>();
			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false,
				(sourcePath, targetPath) => pairs.Add((sourcePath, targetPath)));

			Assert.AreEqual(2, pairs.Count);
			Assert.IsTrue(pairs.Any(pair => pair.Target == Path.Combine(target, "root.txt")));
			Assert.IsTrue(pairs.Any(pair => pair.Target == Path.Combine(target, "sub", "nested.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_ClearTarget_RemovesExistingContents()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(source);
			Directory.CreateDirectory(target);
			File.WriteAllText(Path.Combine(source, "new.txt"), "new");
			File.WriteAllText(Path.Combine(target, "stale.txt"), "stale");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: true);

			Assert.IsTrue(File.Exists(Path.Combine(target, "new.txt")));
			Assert.IsFalse(File.Exists(Path.Combine(target, "stale.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void CopyScriptDirectory_NoClearTarget_KeepsExistingContents()
	{
		string baseDirectory = CreateTempDirectory();

		try
		{
			string source = Path.Combine(baseDirectory, "source");
			string target = Path.Combine(baseDirectory, "target");
			Directory.CreateDirectory(source);
			Directory.CreateDirectory(target);
			File.WriteAllText(Path.Combine(source, "new.txt"), "new");
			File.WriteAllText(Path.Combine(target, "kept.txt"), "kept");

			ScriptDirectoryCopier.CopyScriptDirectory(source, target, clearTarget: false);

			Assert.IsTrue(File.Exists(Path.Combine(target, "new.txt")));
			Assert.IsTrue(File.Exists(Path.Combine(target, "kept.txt")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	private static string CreateTempDirectory()
	{
		string path = Path.Combine(Path.GetTempPath(), "ScriptDirectoryCopierTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(path);
		return path;
	}
}

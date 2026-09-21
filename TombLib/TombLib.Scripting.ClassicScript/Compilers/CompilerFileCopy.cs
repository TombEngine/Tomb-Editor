using System;
using System.IO;
using System.Text;
using TombLib.Scripting.ClassicScript.Cleaning;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Shared per-file copy behavior for Classic Script compilers that reformats
/// <c>.txt</c> files before writing them to the staging directory.
/// </summary>
internal static class CompilerFileCopy
{
	public static void CopyTextFormatted(string sourceFilePath, string targetFilePath)
	{
		if (sourceFilePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
		{
			string fileContent = File.ReadAllText(sourceFilePath);
			fileContent = ClassicScriptDocumentFormatter.FormatCompilerOutput(fileContent);
			File.WriteAllText(targetFilePath, fileContent, Encoding.GetEncoding(1252));
		}
		else
		{
			File.Copy(sourceFilePath, targetFilePath);
		}
	}
}

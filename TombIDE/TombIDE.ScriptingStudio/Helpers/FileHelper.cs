using System;
using System.IO;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Services;

namespace TombIDE.ScriptingStudio.Helpers;

internal static class FileHelper
{
	public static ClassicScriptFileKind GetClassicScriptFileKind(string filePath, IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);

		var classificationService = new ClassicScriptFileClassificationService(lineService);
		return classificationService.GetFileKind(filePath);
	}

	public static bool IsStringFile(string filePath, IClassicScriptLineService lineService)
		=> GetClassicScriptFileKind(filePath, lineService) == ClassicScriptFileKind.Strings;

	public static bool IsClassicScriptFile(string filePath, IClassicScriptLineService lineService)
		=> GetClassicScriptFileKind(filePath, lineService) == ClassicScriptFileKind.Script;

	public static bool IsLuaFile(string filePath)
		=> Path.GetExtension(filePath).Equals(SupportedFormats.Lua, StringComparison.OrdinalIgnoreCase);

	public static bool IsTextFile(string filePath)
		=> Path.GetExtension(filePath).Equals(SupportedFormats.Text, StringComparison.OrdinalIgnoreCase);

	public static bool IsJson5File(string filePath)
		=> Path.GetExtension(filePath).Equals(SupportedFormats.Json5, StringComparison.OrdinalIgnoreCase);

	public static string GetOriginalFilePathFromBackupFile(string backupFilePath)
		=> Path.Combine(Path.GetDirectoryName(backupFilePath), Path.GetFileNameWithoutExtension(backupFilePath));
}

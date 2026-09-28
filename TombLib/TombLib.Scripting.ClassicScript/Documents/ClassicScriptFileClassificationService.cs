using System;
using System.Collections.Generic;
using System.IO;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.Documents;

/// <summary>
/// Classifies ClassicScript files by inspecting their section headers.
/// </summary>
public sealed class ClassicScriptFileClassificationService
{
	private readonly IClassicScriptLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptFileClassificationService"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to identify section headers.</param>
	public ClassicScriptFileClassificationService(IClassicScriptLineService lineService)
		=> _lineService = lineService;

	private static readonly HashSet<string> ScriptSections = new(StringComparer.OrdinalIgnoreCase)
	{
		"PSXExtensions",
		"PCExtensions",
		"Language",
		"Options",
		"Title",
		"Level"
	};

	/// <summary>
	/// Gets the kind of the file at the given path.
	/// </summary>
	/// <param name="filePath">The path of the file to classify.</param>
	/// <returns>The classified file kind.</returns>
	public ClassicScriptFileKind GetFileKind(string filePath)
	{
		foreach (string line in File.ReadLines(filePath))
		{
			if (!_lineService.IsSectionHeaderLine(line))
				continue;

			string? sectionName = _lineService.GetSectionHeaderText(line);

			if (string.IsNullOrWhiteSpace(sectionName))
				continue;

			if (_lineService.IsStringSectionName(sectionName))
				return ClassicScriptFileKind.Strings;

			if (ScriptSections.Contains(sectionName))
				return ClassicScriptFileKind.Script;
		}

		return ClassicScriptFileKind.Unknown;
	}
}

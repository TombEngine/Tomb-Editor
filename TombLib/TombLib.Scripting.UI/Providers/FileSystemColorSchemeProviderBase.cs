using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Providers;

/// <summary>
/// Provides a file-system-backed color scheme listing for languages whose schemes are
/// stored as one JSON file per scheme in a dedicated directory.
/// </summary>
public abstract class FileSystemColorSchemeProviderBase : ITextEditorColorProvider
{
	private readonly string _colorSchemesDirectory;

	/// <summary>
	/// Initializes a new instance of the <see cref="FileSystemColorSchemeProviderBase"/> class.
	/// </summary>
	/// <param name="colorSchemesDirectory">The directory that contains the color scheme files.</param>
	protected FileSystemColorSchemeProviderBase(string colorSchemesDirectory) => _colorSchemesDirectory = colorSchemesDirectory;

	/// <inheritdoc/>
	public virtual IReadOnlyList<string> GetAvailableNames()
	{
		if (!Directory.Exists(_colorSchemesDirectory))
			return [];

		return FilterNames(Directory.GetFiles(_colorSchemesDirectory, "*" + ScriptingDefaults.ColorSchemeFileExtension, SearchOption.TopDirectoryOnly));
	}

	/// <inheritdoc/>
	public abstract string GetSelectedName(TextEditorConfigBase config);

	/// <inheritdoc/>
	public abstract void SetSelectedName(TextEditorConfigBase config, string name);

	/// <summary>
	/// Reduces a set of color scheme file paths to their sorted, de-duplicated display names.
	/// </summary>
	protected static IReadOnlyList<string> FilterNames(IEnumerable<string> filePaths)
	{
		return filePaths
			.Select(static path => Path.GetFileNameWithoutExtension(path) ?? string.Empty)
			.Where(static name => !string.IsNullOrWhiteSpace(name))
			.OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}
}

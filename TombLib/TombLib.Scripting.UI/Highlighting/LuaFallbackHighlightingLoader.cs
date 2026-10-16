using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Xml;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Loads and caches the Lua highlighting definition used as a fallback when the
/// TextMate grammar is unavailable.
/// </summary>
public static class LuaFallbackHighlightingLoader
{
	private static readonly Lazy<IHighlightingDefinition?> FallbackHighlightingState = new(LoadFallbackHighlightingCore);

	/// <summary>
	/// Gets the cached Lua highlighting definition, or <see langword="null"/> if the definition file is missing.
	/// </summary>
	public static IHighlightingDefinition? Load() => FallbackHighlightingState.Value;

	private static IHighlightingDefinition? LoadFallbackHighlightingCore()
	{
		string fallbackFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "ColorSchemes", "Lua", "Default.xml");

		if (!File.Exists(fallbackFilePath))
			return null;

		using var stream = File.OpenRead(fallbackFilePath);
		using var reader = XmlReader.Create(stream);
		return HighlightingLoader.Load(reader, HighlightingManager.Instance);
	}
}

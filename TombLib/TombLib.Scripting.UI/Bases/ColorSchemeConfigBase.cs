using System.IO;
using System.Xml.Serialization;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Base for editor configurations that load a per-language color scheme from the
/// file system whenever the selected scheme name changes.
/// </summary>
/// <typeparam name="TColorScheme">The language-specific color scheme type.</typeparam>
public abstract class ColorSchemeConfigBase<TColorScheme> : TextEditorConfigBase, IColorSchemeConfig where TColorScheme : new()
{
	private string _selectedColorSchemeName = string.Empty;

	/// <summary>
	/// Gets or sets the name of the selected color scheme, reloading the scheme on change.
	/// </summary>
	public string SelectedColorSchemeName
	{
		get => _selectedColorSchemeName;
		set
		{
			_selectedColorSchemeName = value;
			ColorScheme = LoadColorScheme(value);
		}
	}

	/// <summary>
	/// Gets the color scheme for the currently selected scheme name.
	/// Derived from <see cref="SelectedColorSchemeName"/>, therefore not persisted.
	/// </summary>
	[XmlIgnore]
	public TColorScheme ColorScheme { get; private set; } = new();

	/// <summary>
	/// Resolves the file path of the color scheme file for the given scheme name.
	/// </summary>
	protected abstract string GetColorSchemeFilePath(string colorSchemeName);

	/// <summary>
	/// Reads a color scheme from the file at <paramref name="colorSchemeFilePath"/>.
	/// </summary>
	protected abstract TColorScheme ReadColorSchemeFile(string colorSchemeFilePath);

	private TColorScheme LoadColorScheme(string colorSchemeName)
	{
		string colorSchemeFilePath = GetColorSchemeFilePath(colorSchemeName);

		if (!File.Exists(colorSchemeFilePath))
			return new TColorScheme();

		return ReadColorSchemeFile(colorSchemeFilePath);
	}
}

using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Serves as the common base for scripting editor color schemes and themes.
/// </summary>
public abstract class ColorSchemeBase
{
	private IReadOnlyList<string> _aliases = [];

	/// <summary>
	/// Gets or sets the display name of the color scheme.
	/// </summary>
	public string Name { get; set; } = "Untitled Color Scheme";

	/// <summary>
	/// Gets or sets additional names that can be used to resolve this color scheme.
	/// </summary>
	public IReadOnlyList<string> Aliases
	{
		get => _aliases;
		set => _aliases = value is null ? [] : Array.AsReadOnly([.. value]);
	}

	/// <summary>
	/// Gets or sets the editor background color.
	/// </summary>
	public string Background { get; set; } = "Black";

	/// <summary>
	/// Gets or sets the editor foreground color.
	/// </summary>
	public string Foreground { get; set; } = "White";

	/// <summary>
	/// Replaces missing metadata with safe defaults so the scheme can be used at runtime.
	/// </summary>
	/// <param name="fallbackName">The scheme name to use when no explicit name was provided.</param>
	/// <returns>The current scheme instance.</returns>
	public virtual ColorSchemeBase Normalize(string fallbackName)
	{
		if (string.IsNullOrWhiteSpace(Name))
			Name = fallbackName;

		return this;
	}
}

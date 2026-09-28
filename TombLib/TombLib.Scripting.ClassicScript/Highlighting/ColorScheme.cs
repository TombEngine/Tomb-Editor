using System;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.ClassicScript.Highlighting;

/// <summary>
/// The color scheme used by the ClassicScript editor highlighting.
/// </summary>
public sealed class ColorScheme : ColorSchemeBase
{
	/// <summary>
	/// Gets or sets the highlighting object for sections.
	/// </summary>
	public HighlightingObject Sections { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for values.
	/// </summary>
	public HighlightingObject Values { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for references.
	/// </summary>
	public HighlightingObject References { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for standard commands.
	/// </summary>
	public HighlightingObject StandardCommands { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for new commands.
	/// </summary>
	public HighlightingObject NewCommands { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for comments.
	/// </summary>
	public HighlightingObject Comments { get; set; } = new();

	// Operators

	/// <summary>
	/// Determines whether two color schemes are equal.
	/// </summary>
	public static bool operator ==(ColorScheme? a, ColorScheme? b) => Equals(a, b);

	/// <summary>
	/// Determines whether two color schemes are not equal.
	/// </summary>
	public static bool operator !=(ColorScheme? a, ColorScheme? b) => !Equals(a, b);

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is not ColorScheme objectToCompare)
			return false;

		return Sections == objectToCompare.Sections
			&& Values == objectToCompare.Values
			&& References == objectToCompare.References
			&& StandardCommands == objectToCompare.StandardCommands
			&& NewCommands == objectToCompare.NewCommands
			&& Comments == objectToCompare.Comments
			&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
			&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Sections.GetHashCode();
			hashCode = (hashCode * 397) ^ Values.GetHashCode();
			hashCode = (hashCode * 397) ^ References.GetHashCode();
			hashCode = (hashCode * 397) ^ StandardCommands.GetHashCode();
			hashCode = (hashCode * 397) ^ NewCommands.GetHashCode();
			hashCode = (hashCode * 397) ^ Comments.GetHashCode();
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Background);
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Foreground);
			return hashCode;
		}
	}
}

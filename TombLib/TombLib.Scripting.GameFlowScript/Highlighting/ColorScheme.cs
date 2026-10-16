using System;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.GameFlowScript.Highlighting;

/// <summary>
/// The color scheme used by the GameFlow editor highlighting.
/// </summary>
public sealed class ColorScheme : ColorSchemeBase
{
	/// <summary>
	/// Gets or sets the highlighting object for comments.
	/// </summary>
	public HighlightingObject Comments { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for sections.
	/// </summary>
	public HighlightingObject Sections { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for special properties.
	/// </summary>
	public HighlightingObject SpecialProperties { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for properties.
	/// </summary>
	public HighlightingObject Properties { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for constants.
	/// </summary>
	public HighlightingObject Constants { get; set; } = new();

	/// <summary>
	/// Gets or sets the highlighting object for values.
	/// </summary>
	public HighlightingObject Values { get; set; } = new();

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

		return Comments == objectToCompare.Comments
			&& Sections == objectToCompare.Sections
			&& SpecialProperties == objectToCompare.SpecialProperties
			&& Properties == objectToCompare.Properties
			&& Constants == objectToCompare.Constants
			&& Values == objectToCompare.Values
			&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
			&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Comments.GetHashCode();
			hashCode = (hashCode * 397) ^ Sections.GetHashCode();
			hashCode = (hashCode * 397) ^ SpecialProperties.GetHashCode();
			hashCode = (hashCode * 397) ^ Properties.GetHashCode();
			hashCode = (hashCode * 397) ^ Constants.GetHashCode();
			hashCode = (hashCode * 397) ^ Values.GetHashCode();
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Background);
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Foreground);
			return hashCode;
		}
	}
}

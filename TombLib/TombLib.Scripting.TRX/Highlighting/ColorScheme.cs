using System;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.TRX.Highlighting;

/// <summary>
/// Defines the highlighting colors used for the TRX language.
/// </summary>
public sealed class ColorScheme : ColorSchemeBase
{
	/// <summary>
	/// Gets or sets the highlighting for comments.
	/// </summary>
	public HighlightingObject Comments { get; set; } = new HighlightingObject();

	/// <summary>
	/// Gets or sets the highlighting for constants.
	/// </summary>
	public HighlightingObject Constants { get; set; } = new HighlightingObject();

	/// <summary>
	/// Gets or sets the highlighting for collections.
	/// </summary>
	public HighlightingObject Collections { get; set; } = new HighlightingObject();

	/// <summary>
	/// Gets or sets the highlighting for properties.
	/// </summary>
	public HighlightingObject Properties { get; set; } = new HighlightingObject();

	/// <summary>
	/// Gets or sets the highlighting for values.
	/// </summary>
	public HighlightingObject Values { get; set; } = new HighlightingObject();

	/// <summary>
	/// Gets or sets the highlighting for strings.
	/// </summary>
	public HighlightingObject Strings { get; set; } = new HighlightingObject();

	// Operators

	/// <summary>
	/// Determines whether two color schemes are equal.
	/// </summary>
	public static bool operator ==(ColorScheme? a, ColorScheme? b) => Equals(a, b);

	/// <summary>
	/// Determines whether two color schemes are not equal.
	/// </summary>
	public static bool operator !=(ColorScheme? a, ColorScheme? b) => !Equals(a, b);

	/// <summary>
	/// Determines whether this color scheme equals another object.
	/// </summary>
	public override bool Equals(object? obj)
	{
		if (obj is not ColorScheme other)
			return false;

		return Comments == other.Comments
			&& Constants == other.Constants
			&& Collections == other.Collections
			&& Properties == other.Properties
			&& Values == other.Values
			&& Strings == other.Strings
			&& Background.Equals(other.Background, StringComparison.OrdinalIgnoreCase)
			&& Foreground.Equals(other.Foreground, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Computes a hash code for this color scheme.
	/// </summary>
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Comments.GetHashCode();
			hashCode = (hashCode * 397) ^ Constants.GetHashCode();
			hashCode = (hashCode * 397) ^ Collections.GetHashCode();
			hashCode = (hashCode * 397) ^ Properties.GetHashCode();
			hashCode = (hashCode * 397) ^ Values.GetHashCode();
			hashCode = (hashCode * 397) ^ Strings.GetHashCode();
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Background);
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Foreground);
			return hashCode;
		}
	}
}

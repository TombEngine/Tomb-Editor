using System;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Describes the highlighting style applied to a group of syntax elements.
/// </summary>
public sealed class HighlightingObject
{
	/// <summary>
	/// Gets or sets the HTML color of the highlighted text.
	/// </summary>
	public string HtmlColor { get; set; } = "White";

	/// <summary>
	/// Gets or sets whether the highlighted text is bold.
	/// </summary>
	public bool IsBold { get; set; }

	/// <summary>
	/// Gets or sets whether the highlighted text is italic.
	/// </summary>
	public bool IsItalic { get; set; }

	// Operators

	/// <summary>
	/// Determines whether two highlighting objects are equal.
	/// </summary>
	public static bool operator ==(HighlightingObject? a, HighlightingObject? b) => Equals(a, b);

	/// <summary>
	/// Determines whether two highlighting objects are not equal.
	/// </summary>
	public static bool operator !=(HighlightingObject? a, HighlightingObject? b) => !Equals(a, b);

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is not HighlightingObject objectToCompare)
			return false;

		return HtmlColor.Equals(objectToCompare.HtmlColor, StringComparison.OrdinalIgnoreCase)
			&& IsBold == objectToCompare.IsBold
			&& IsItalic == objectToCompare.IsItalic;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(HtmlColor);
			hashCode = (hashCode * 397) ^ IsBold.GetHashCode();
			hashCode = (hashCode * 397) ^ IsItalic.GetHashCode();
			return hashCode;
		}
	}
}

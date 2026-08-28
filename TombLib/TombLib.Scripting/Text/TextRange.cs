using System;

namespace TombLib.Scripting.Text;

/// <summary>
/// Represents a contiguous range of text identified by a zero-based offset and length.
/// The range is relative to the text supplied to the method unless explicitly documented otherwise.
/// </summary>
public readonly struct TextRange : IEquatable<TextRange>
{
	/// <summary>
	/// Gets the zero-based offset of the start of the range.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// Gets the length of the range in characters.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// Gets the zero-based offset of the first character after the range,
	/// equal to <c>Offset + Length</c>.
	/// </summary>
	public int EndOffset => Offset + Length;

	/// <summary>
	/// Gets a value indicating whether this range is empty (Length is zero).
	/// </summary>
	public bool IsEmpty => Length == 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextRange"/> struct.
	/// </summary>
	/// <param name="offset">The zero-based start offset.</param>
	/// <param name="length">The length in characters.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> or <paramref name="length"/> is negative.
	/// </exception>
	public TextRange(int offset, int length)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(offset);
		ArgumentOutOfRangeException.ThrowIfNegative(length);

		Offset = offset;
		Length = length;
	}

	/// <summary>
	/// Returns the text represented by this range from the given source text.
	/// </summary>
	/// <param name="source">The source text to slice.</param>
	/// <returns>The text within this range.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The range extends beyond the length of <paramref name="source"/>.
	/// </exception>
	public string GetText(string source)
	{
		// Overflow-safe bounds check: the subtraction cannot overflow because
		// Offset is verified to be within the source length first.
		if (Offset > source.Length || Length > source.Length - Offset)
			throw new ArgumentOutOfRangeException(nameof(source));

		return source.Substring(Offset, Length);
	}

	/// <inheritdoc/>
	public bool Equals(TextRange other)
		=> Offset == other.Offset && Length == other.Length;

	/// <inheritdoc/>
	public override bool Equals(object? obj)
		=> obj is TextRange other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode()
		=> HashCode.Combine(Offset, Length);

	/// <inheritdoc/>
	public override string ToString() => $"[{Offset}..{EndOffset})";

	/// <summary>
	/// Returns a value indicating whether two <see cref="TextRange"/> values are equal.
	/// </summary>
	public static bool operator ==(TextRange left, TextRange right) => left.Equals(right);

	/// <summary>
	/// Returns a value indicating whether two <see cref="TextRange"/> values are not equal.
	/// </summary>
	public static bool operator !=(TextRange left, TextRange right) => !left.Equals(right);
}

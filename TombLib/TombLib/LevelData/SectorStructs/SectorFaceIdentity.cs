using System;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// A unique, one of a kind identifier for a sector face, which includes the position of the sector that the face belongs to.
/// </summary>
public readonly struct SectorFaceIdentity : IEquatable<SectorFaceIdentity>, IComparable, IComparable<SectorFaceIdentity>
{
	public readonly VectorInt2 Position;
	public readonly SectorFace Face;

	public SectorFaceIdentity(int x, int z, SectorFace face)
	{
		Position = new VectorInt2(x, z);
		Face = face;
	}

	public override readonly bool Equals(object other) => other is SectorFaceIdentity identity && Equals(identity);
	public readonly bool Equals(SectorFaceIdentity other) => Position == other.Position && Face == other.Face;
	public override int GetHashCode() => Position.GetHashCode() ^ (1200049507 * (int)Face); // Random prime

	readonly int IComparable.CompareTo(object other) => CompareTo((SectorFaceIdentity)other);
	public readonly int CompareTo(SectorFaceIdentity other)
	{
		if (Position.X != other.Position.X)
			return Position.X > other.Position.X ? 1 : -1;

		if (Position.Y != other.Position.Y)
			return Position.Y > other.Position.Y ? 1 : -1;

		if (Face != other.Face)
			return Face > other.Face ? 1 : -1;

		return 0;
	}

	public static bool operator ==(SectorFaceIdentity left, SectorFaceIdentity right) => left.Equals(right);
	public static bool operator !=(SectorFaceIdentity left, SectorFaceIdentity right) => !(left == right);
	public static bool operator <(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) < 0;
	public static bool operator <=(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) <= 0;
	public static bool operator >(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) > 0;
	public static bool operator >=(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) >= 0;
}

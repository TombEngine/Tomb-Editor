using System;
using System.Numerics;

namespace TombLib
{
	[Serializable]
	public struct VectorInt4 : IEquatable<VectorInt4>
	{
		public int X;
		public int Y;
		public int Z;
		public int W;

		public VectorInt4(int all)
		{
			X = all; Y = all; Z = all; W = all;
		}

		public VectorInt4(int x, int y, int z, int w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static VectorInt4 Max(VectorInt4 first, VectorInt4 second) =>
			new VectorInt4(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y), Math.Max(first.Z, second.Z), Math.Max(first.W, second.W));

		public static VectorInt4 Min(VectorInt4 first, VectorInt4 second) =>
			new VectorInt4(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Min(first.Z, second.Z), Math.Min(first.W, second.W));

		public static VectorInt4 operator +(VectorInt4 first, VectorInt4 second) =>
			new VectorInt4(first.X + second.X, first.Y + second.Y, first.Z + second.Z, first.W + second.W);

		public static VectorInt4 operator -(VectorInt4 first, VectorInt4 second) =>
			new VectorInt4(first.X - second.X, first.Y - second.Y, first.Z - second.Z, first.W - second.W);

		public static VectorInt4 operator -(VectorInt4 value) =>
			new VectorInt4(-value.X, -value.Y, -value.Z, -value.W);

		public static VectorInt4 operator *(VectorInt4 value, int scale) =>
			new VectorInt4(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);

		public static VectorInt4 operator /(VectorInt4 value, int scale) =>
			new VectorInt4(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);

		public static bool operator ==(VectorInt4 first, VectorInt4 second) =>
			first.X == second.X && first.Y == second.Y && first.Z == second.Z && first.W == second.W;

		public static bool operator !=(VectorInt4 first, VectorInt4 second) =>
			!(first == second);

		public static implicit operator Vector4(VectorInt4 value) =>
			new Vector4(value.X, value.Y, value.Z, value.W);

		public static explicit operator VectorInt4(Vector4 value) => FromRounded(value);

		public static VectorInt4 Zero => new VectorInt4(0, 0, 0, 0);
		public static VectorInt4 One => new VectorInt4(1, 1, 1, 1);

		public static VectorInt4 FromRounded(Vector4 value) => new VectorInt4(
			(int)Math.Min(Math.Max(Math.Round(value.X), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Round(value.Y), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Round(value.Z), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Round(value.W), int.MinValue), int.MaxValue));

		public static VectorInt4 FromFloor(Vector4 value) => new VectorInt4(
			(int)Math.Min(Math.Max(Math.Floor(value.X), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Floor(value.Y), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Floor(value.Z), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Floor(value.W), int.MinValue), int.MaxValue));

		public static VectorInt4 FromCeiling(Vector4 value) => new VectorInt4(
			(int)Math.Min(Math.Max(Math.Ceiling(value.X), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Ceiling(value.Y), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Ceiling(value.Z), int.MinValue), int.MaxValue),
			(int)Math.Min(Math.Max(Math.Ceiling(value.W), int.MinValue), int.MaxValue));

		public override string ToString() => $"<{X}, {Y}, {Z}, {W}>";

		public override bool Equals(object obj) => obj is VectorInt4 other && this == other;

		public bool Equals(VectorInt4 other) => this == other;

		public override int GetHashCode() => unchecked(
			X + 249669787 * Y + 195115517 * Z + 136399731 * W); // Random primes
	}
}
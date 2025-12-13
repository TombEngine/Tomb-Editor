using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib
{
    [Serializable]
    public struct Rectangle2
    {
        public static readonly Rectangle2 MaxMin = new Rectangle2(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        public static readonly Rectangle2 Zero = new Rectangle2(0, 0, 0, 0);

        public Vector2 Start;
        public Vector2 End;

        public Rectangle2(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public Rectangle2(float x0, float y0, float x1, float y1)
        {
            Start = new Vector2(x0, y0);
            End = new Vector2(x1, y1);
        }

        public float X0
        {
            get { return Start.X; }
            set { Start.X = value; }
        }
        public float Y0
        {
            get { return Start.Y; }
            set { Start.Y = value; }
        }
        public float X1
        {
            get { return End.X; }
            set { End.X = value; }
        }
        public float Y1
        {
            get { return End.Y; }
            set { End.Y = value; }
        }

        public Vector2 Size => End - Start;
        public float Width => End.X - Start.X;
        public float Height => End.Y - Start.Y;

        public Vector2 TopLeft => new Vector2(X0, Y0);
        public Vector2 TopRight => new Vector2(X1, Y0);
        public Vector2 BottomLeft => new Vector2(X0, Y1);
        public Vector2 BottomRight => new Vector2(X1, Y1);

        public bool IsZero => MathC.IsZero(Size.X) || MathC.IsZero(Size.Y);

        public Rectangle2 Intersect(Rectangle2 other) =>
            new Rectangle2(Vector2.Max(Start, other.Start), Vector2.Min(End, other.End));

        public Rectangle2 Union(Rectangle2 other) =>
            new Rectangle2(Vector2.Min(Start, other.Start), Vector2.Max(End, other.End));

        public Vector2 GetMid() => (Start + End) * 0.5f;

        public bool Contains(Rectangle2 other) =>
            Start.X <= other.Start.X && End.X >= other.End.X && Start.Y <= other.Start.Y && End.Y >= other.End.Y;

        public bool Contains(Vector2 point) =>
            Start.X <= point.X && End.X >= point.X && Start.Y <= point.Y && End.Y >= point.Y;

        public bool Intersects(Rectangle2 other) =>
            Start.X < other.End.X && End.X > other.Start.X &&
            Start.Y < other.End.Y && End.Y > other.Start.Y;

        public Rectangle2 Inflate(float width) =>
            new Rectangle2(Start - new Vector2(width), End + new Vector2(width));

        public Rectangle2 Inflate(Vector2 width) =>
            new Rectangle2(Start - width, End + width);

        public Rectangle2 Inflate(float width, float height) => Inflate(new Vector2(width, height));

        public Rectangle2 Round()
        {
            Rectangle2 result = this;
            result.Start = new Vector2((float)Math.Floor(result.Start.X), (float)Math.Floor(result.Start.Y));
            result.End = new Vector2((float)Math.Ceiling(result.End.X), (float)Math.Ceiling(result.End.Y));
            return result;
        }

        public static Rectangle2 operator +(Rectangle2 area, Vector2 offset) => new Rectangle2(area.Start + offset, area.End + offset);
        public static Rectangle2 operator -(Rectangle2 area, Vector2 offset) => new Rectangle2(area.Start - offset, area.End - offset);
        public static Rectangle2 operator +(Vector2 offset, Rectangle2 area) => new Rectangle2(offset + area.Start, offset + area.End);
        public static Rectangle2 operator -(Vector2 offset, Rectangle2 area) => new Rectangle2(offset - area.Start, offset - area.End);
        public static Rectangle2 operator -(Rectangle2 area) => new Rectangle2(-area.Start, -area.End);

        public static bool operator ==(Rectangle2 first, Rectangle2 second) => first.Start == second.Start && first.End == second.End;
        public static bool operator !=(Rectangle2 first, Rectangle2 second) => first.Start != second.Start || first.End != second.End;
        public static Rectangle2 FromLTRB(Vector2 start, Vector2 size) => new Rectangle2(start, start + size);
        public static Rectangle2 FromLTRB(float x0, float y0, float width, float height) => new Rectangle2(x0, y0, x0 + width, y0 + height);

        public static Rectangle2 FromCoordinates(Vector2[] coordinates)
        {
            if (coordinates.Length == 4)
                return FromCoordinates(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
            else
                return FromCoordinates(coordinates[0], coordinates[1], coordinates[2]);
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public static Rectangle2 FromCoordinates(in Vector2 a, in Vector2 b, in Vector2 c)
        {
			var min = Vector2.Min(Vector2.Min(a, b), c);
			var max = Vector2.Max(Vector2.Max(a, b), c);
			return new Rectangle2(min, max);
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public static Rectangle2 FromCoordinates(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d)
		{
			var min = Vector2.Min(Vector2.Min(a, b), Vector2.Min(c, d));
			var max = Vector2.Max(Vector2.Max(a, b), Vector2.Max(c, d));
			return new Rectangle2(min, max);
		}

		public bool Equals(Rectangle2 other) => this == other;
        public override string ToString() => Start + " to " + End;
        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle2))
                return false;
            return this == (Rectangle2)obj;
        }
        public override int GetHashCode() => unchecked(Start.GetHashCode() + End.GetHashCode() * -460299429); // Random prime
    }
}

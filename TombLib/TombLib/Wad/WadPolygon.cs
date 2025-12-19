using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TombLib.Utils;

namespace TombLib.Wad
{
    public enum WadPolygonShape : ushort
    {
        Quad = 0,
        Triangle = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WadPolygon : IEquatable<WadPolygon>
    {
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;

        public WadPolygonShape Shape;
        public TextureArea Texture;
        public byte ShineStrength;

        public bool IsTriangle => Shape == WadPolygonShape.Triangle;

        public Vector2[] CorrectTexCoords(float margin = 0.5f) =>
            MathC.CorrectTexCoords(Texture.TexCoords, IsTriangle, margin);

        public void Rotate(int iter = 1, bool isTriangle = false)
        {
            for (int i = 0; i < iter; i++)
            {
                if (!isTriangle)
                {
                    int tempIndex = Index3;
                    Index3 = Index2;
                    Index2 = Index1;
                    Index1 = Index0;
                    Index0 = tempIndex;
                }
                else
                {
                    int tempIndex = Index2;
                    Index2 = Index1;
                    Index1 = Index0;
                    Index0 = tempIndex;
                    Index3 = Index2;
                }
            }
        }

        public void Flip(bool isTriangle = false)
        {
            if (!isTriangle)
            {
                int tempIndex = Index0;
                Index0 = Index3;
                Index3 = tempIndex;

                tempIndex = Index1;
                Index1 = Index2;
                Index2 = tempIndex;
            }
            else
            {
                int tempIndex = Index0;
                Index0 = Index2;
                Index2 = tempIndex;
                Index3 = Index2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Equals(WadPolygon other)
        {
            if (Shape != other.Shape || ShineStrength != other.ShineStrength)
                return false;

            if (!EqualsIndices(in this, in other))
                return false;

            return Texture.Equals(other.Texture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool EqualsIndices(in WadPolygon a, in WadPolygon b)
        {
            if (Sse2.IsSupported)
            {
                // 4 int = 128 bit
                ref readonly var aVec = ref Unsafe.As<int, Vector128<int>>(ref Unsafe.AsRef(in a.Index0));
                ref readonly var bVec = ref Unsafe.As<int, Vector128<int>>(ref Unsafe.AsRef(in b.Index0));

                var cmp = Sse2.CompareEqual(aVec, bVec);
                return Sse2.MoveMask(cmp.AsByte()) == 0xFFFF;
            }

            return a.Index0 == b.Index0 &&
                   a.Index1 == b.Index1 &&
                   a.Index2 == b.Index2 &&
                   a.Index3 == b.Index3;
        }

        public override bool Equals(object obj) => obj is WadPolygon other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(Shape, Index0, Index1, Texture.Texture, Texture.BlendMode);
        }
    }
}

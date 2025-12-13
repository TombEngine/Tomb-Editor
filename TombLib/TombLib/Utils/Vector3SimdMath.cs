using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace TombLib.Utils
{
	internal static class Vector3SimdMath
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float Dot(in Vector3 a, in Vector3 b)
		{
			if (Sse41.IsSupported)
			{
				Vector128<float> va = Vector128.Create(a.X, a.Y, a.Z, 0f);
				Vector128<float> vb = Vector128.Create(b.X, b.Y, b.Z, 0f);

				// Sum 3 components
				Vector128<float> dp = Sse41.DotProduct(va, vb, 0x7F); 

				return dp.ToScalar();
			}
			else if (Sse3.IsSupported)
			{
				Vector128<float> va = Vector128.Create(a.X * b.X, a.Y * b.Y, a.Z * b.Z, 0f);
				
				// hadd 2 times for summing x + y + z 
				Vector128<float> s1 = Sse3.HorizontalAdd(va, va);
				Vector128<float> s2 = Sse3.HorizontalAdd(s1, s1);
				
				return s2.ToScalar();
			}
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector3 Normalize(in Vector3 v)
		{
			// Use rsqrt, it's faster
			if (Sse.IsSupported)
			{
				Vector128<float> x = Vector128.Create(v.X, v.Y, v.Z, 0f);
				Vector128<float> len2;
				if (Sse41.IsSupported)
				{
					len2 = Sse41.DotProduct(x, x, 0x7F);
				}
				else if (Sse3.IsSupported)
				{
					Vector128<float> mul = Sse.Multiply(x, x);
					Vector128<float> h1 = Sse3.HorizontalAdd(mul, mul);
					len2 = Sse3.HorizontalAdd(h1, h1);
				}
				else
				{
					Vector128<float> mul = Sse.Multiply(x, x);
					Vector128<float> sh = Sse.Shuffle(mul, mul, 0b_01_01_10_00); // z y x x
					Vector128<float> add = Sse.Add(mul, sh);
					sh = Sse.Shuffle(add, add, 0b_00_00_00_01);
					len2 = Sse.Add(add, sh);
				}

				Vector128<float> invLen = Sse.ReciprocalSqrtScalar(len2);        // 1/sqrt(len2)
				invLen = Sse.Shuffle(invLen, invLen, 0x00);         // splat
				Vector128<float> n = Sse.Multiply(x, invLen);
				return new Vector3(n.GetElement(0), n.GetElement(1), n.GetElement(2));
			}
			return Vector3.Normalize(v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector3 NormalizeFromDxDy(in Vector3 d, float dz)
		{
			// normaliza (dx, dy, dz) with intrinsics
			if (Sse.IsSupported)
			{
				Vector128<float> v = Vector128.Create(d.X, d.Y, dz, 0f);
				Vector128<float> len2;
				if (Sse41.IsSupported)
				{
					len2 = Sse41.DotProduct(v, v, 0x7F);
				}
				else if (Sse3.IsSupported)
				{
					Vector128<float> mul = Sse.Multiply(v, v);
					Vector128<float> h1 = Sse3.HorizontalAdd(mul, mul);
					len2 = Sse3.HorizontalAdd(h1, h1);
				}
				else
				{
					Vector128<float> mul = Sse.Multiply(v, v);
					Vector128<float> sh = Sse.Shuffle(mul, mul, 0b_01_01_10_00);
					Vector128<float> add = Sse.Add(mul, sh);
					sh = Sse.Shuffle(add, add, 0b_00_00_00_01);
					len2 = Sse.Add(add, sh);
				}
				Vector128<float> invLen = Sse.ReciprocalSqrtScalar(len2);
				invLen = Sse.Shuffle(invLen, invLen, 0x00);
				Vector128<float> n = Sse.Multiply(v, invLen);
				return new Vector3(n.GetElement(0), n.GetElement(1), n.GetElement(2));
			}
			// fallback
			float len = MathF.Sqrt(d.X * d.X + d.Y * d.Y + dz * dz);
			return new Vector3(d.X / len, d.Y / len, dz / len);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector3 CrossFast(in Vector3 a, in Vector3 b) =>
			new Vector3(a.Y * b.Z - a.Z * b.Y,
						a.Z * b.X - a.X * b.Z,
						a.X * b.Y - a.Y * b.X);
	}
}

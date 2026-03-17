using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Utils
{
    /// <summary>
    /// Catmull-Rom spline evaluation matching TEN's Spline() function (spotcam.cpp).
    /// Used by both the flyby preview and the 3D viewport camera path ribbon.
    /// </summary>
    public static class CatmullRomSpline
    {
        /// <summary>
        /// Evaluates a Catmull-Rom spline at parameter t.
        /// <para>
        /// The knots array must be padded with endpoint duplication:
        ///   [first_dup, cam0, cam1, ..., camN-1, last_dup]
        /// </para>
        /// <para>
        /// t ∈ [0, numSegments] where numSegments = knots.Length - 3.
        /// At integer values of t, the result equals the corresponding camera value exactly.
        /// </para>
        /// </summary>
        public static float Evaluate(float t, float[] knots)
        {
            if (knots == null || knots.Length < 4)
                throw new ArgumentException("Knots array must have at least 4 elements.", nameof(knots));

            int segments = knots.Length - 3;
            int span = (int)t;

            if (span >= segments)
                span = segments - 1;

            if (span < 0)
                span = 0;

            float u = t - span;

            float p0 = knots[span];
            float p1 = knots[span + 1];
            float p2 = knots[span + 2];
            float p3 = knots[span + 3];

            // Standard Catmull-Rom coefficients (tau = 0.5), Horner form:
            float a = 0.5f * (-p0 + (3.0f * p1) - (3.0f * p2) + p3);
            float b = 0.5f * ((2.0f * p0) - (5.0f * p1) + (4.0f * p2) - p3);
            float c = 0.5f * (-p0 + p2);
            float d = p1;

            return (((((a * u) + b) * u) + c) * u) + d;
        }

        /// <summary>
        /// Pads an array by duplicating the first and last elements, matching how
        /// TEN sets up spline knots with endpoint clamping.
        /// Result: [values[0], values[0], values[1], ..., values[N-1], values[N-1]]
        /// </summary>
        public static float[] PadKnots(float[] values)
        {
            if (values == null || values.Length == 0)
                return Array.Empty<float>();

            int n = values.Length;
            var padded = new float[n + 2];
            padded[0] = values[0];

            for (int i = 0; i < n; i++)
                padded[i + 1] = values[i];

            padded[n + 1] = values[n - 1];
            return padded;
        }

        /// <summary>
        /// Generates a smooth list of 3D positions along a Catmull-Rom spline defined
        /// by the given control points, using the specified number of subdivisions per segment.
        /// </summary>
        /// <param name="positions">The ordered control point positions (at least 2).</param>
        /// <param name="subdivisionsPerSegment">How many sample points per segment between control points.</param>
        /// <returns>A list of interpolated positions along the spline, ending at the last control point.</returns>
        public static List<Vector3> EvaluatePositions(IList<Vector3> positions, int subdivisionsPerSegment)
        {
            if (positions == null || positions.Count < 2)
                return new List<Vector3>(positions ?? Array.Empty<Vector3>());

            if (subdivisionsPerSegment <= 0)
                throw new ArgumentOutOfRangeException(nameof(subdivisionsPerSegment), "Subdivisions per segment must be greater than zero.");

            int n = positions.Count;
            var xKnots = new float[n];
            var yKnots = new float[n];
            var zKnots = new float[n];

            for (int i = 0; i < n; i++)
            {
                xKnots[i] = positions[i].X;
                yKnots[i] = positions[i].Y;
                zKnots[i] = positions[i].Z;
            }

            var px = PadKnots(xKnots);
            var py = PadKnots(yKnots);
            var pz = PadKnots(zKnots);

            int numSegments = n - 1;
            int totalSamples = numSegments * subdivisionsPerSegment;
            var result = new List<Vector3>(totalSamples + 1);

            for (int i = 0; i <= totalSamples; i++)
            {
                float t = (float)i / subdivisionsPerSegment;
                result.Add(new Vector3(Evaluate(t, px), Evaluate(t, py), Evaluate(t, pz)));
            }

            return result;
        }
    }
}

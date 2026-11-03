using System.Globalization;
using System.Numerics;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IconPipeline;

/// <summary>
/// Traces a legacy bitmap into smooth vector outlines: each color layer is upsampled with a bicubic filter, its iso-contour
/// is extracted with marching squares and fitted with cubic Beziers, keeping real corners sharp.
/// </summary>
internal static class LegacyTracer
{
    private const int Upsample = 8;
    private const int Padding = 2;

    internal sealed record Layer(string Color, double[,] Alpha);

    internal static (int Width, int Height, byte[] Bgra) ReadStraightBgra(string path)
    {
        var decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
        var converted = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Bgra32, null, 0);
        var bytes = new byte[converted.PixelWidth * converted.PixelHeight * 4];
        converted.CopyPixels(bytes, converted.PixelWidth * 4, 0);
        return (converted.PixelWidth, converted.PixelHeight, bytes);
    }

    /// <summary>Splits the bitmap into color layers (greedy clustering by RGB distance) with their alpha coverage.</summary>
    internal static List<Layer> SplitLayers(int width, int height, byte[] bgra)
    {
        var centers = new List<(double R, double G, double B, double Weight)>();
        var assignment = new int[width * height];
        Array.Fill(assignment, -1);

        for (var i = 0; i < width * height; i++)
        {
            var a = bgra[i * 4 + 3];
            if (a < 8)
                continue;

            double b = bgra[i * 4], g = bgra[i * 4 + 1], r = bgra[i * 4 + 2];
            var best = -1;
            var bestDistance = 44.0;
            for (var k = 0; k < centers.Count; k++)
            {
                var distance = Math.Sqrt(Math.Pow(r - centers[k].R, 2) + Math.Pow(g - centers[k].G, 2) + Math.Pow(b - centers[k].B, 2));
                if (distance < bestDistance)
                {
                    best = k;
                    bestDistance = distance;
                }
            }

            if (best < 0)
            {
                centers.Add((r, g, b, a));
                best = centers.Count - 1;
            }
            else
            {
                var c = centers[best];
                var w = c.Weight + a;
                centers[best] = ((c.R * c.Weight + r * a) / w, (c.G * c.Weight + g * a) / w, (c.B * c.Weight + b * a) / w, w);
            }

            assignment[i] = best;
        }

        var layers = new List<Layer>();
        for (var k = 0; k < centers.Count; k++)
        {
            var alpha = new double[height, width];
            var count = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var i = y * width + x;
                    if (assignment[i] == k)
                    {
                        alpha[y, x] = bgra[i * 4 + 3] / 255.0;
                        count++;
                    }
                }
            }

            if (count > 0)
                layers.Add(new Layer($"#{(int)Math.Round(centers[k].R):x2}{(int)Math.Round(centers[k].G):x2}{(int)Math.Round(centers[k].B):x2}", alpha));
        }

        // Larger layers first so small accents are drawn on top.
        return layers.OrderByDescending(layer => Coverage(layer.Alpha)).ToList();
    }

    private static double Coverage(double[,] alpha)
    {
        var sum = 0.0;
        foreach (var value in alpha)
            sum += value;
        return sum;
    }

    internal static string BuildSvg(int width, int height, List<Layer> layers, double threshold)
    {
        var builder = new StringBuilder();
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">\r\n");
        foreach (var layer in layers)
        {
            var data = TraceLayer(layer.Alpha, width, height, threshold);
            if (data.Length > 0)
                builder.Append(CultureInfo.InvariantCulture, $"  <path fill=\"{layer.Color}\" fill-rule=\"evenodd\" d=\"{data}\"/>\r\n");
        }

        builder.Append("</svg>\r\n");
        return builder.ToString();
    }

    private static string TraceLayer(double[,] alpha, int width, int height, double threshold)
    {
        var nx = (width + 2 * Padding) * Upsample;
        var ny = (height + 2 * Padding) * Upsample;
        var field = new float[ny, nx];
        for (var gy = 0; gy < ny; gy++)
        {
            var fy = (gy + 0.5) / Upsample - Padding - 0.5;
            for (var gx = 0; gx < nx; gx++)
            {
                var fx = (gx + 0.5) / Upsample - Padding - 0.5;
                field[gy, gx] = (float)SampleCubic(alpha, width, height, fx, fy);
            }
        }

        var loops = MarchingSquares(field, nx, ny, (float)threshold);
        var builder = new StringBuilder();
        foreach (var loop in loops)
        {
            // Convert grid coordinates to SVG units.
            var points = loop.Select(p => new Vector2((float)((p.X + 0.5) / Upsample - Padding), (float)((p.Y + 0.5) / Upsample - Padding))).ToList();
            if (points.Count < 4)
                continue;

            AppendFittedLoop(builder, points);
        }

        return builder.ToString().Trim();
    }

    private static double SampleCubic(double[,] source, int width, int height, double x, double y)
    {
        var ix = (int)Math.Floor(x);
        var iy = (int)Math.Floor(y);
        var tx = x - ix;
        var ty = y - iy;
        var rows = new double[4];
        for (var j = 0; j < 4; j++)
        {
            var yy = iy - 1 + j;
            var p = new double[4];
            for (var i = 0; i < 4; i++)
            {
                var xx = ix - 1 + i;
                p[i] = xx < 0 || yy < 0 || xx >= width || yy >= height ? 0.0 : source[yy, xx];
            }

            rows[j] = CatmullRom(p[0], p[1], p[2], p[3], tx);
        }

        return Math.Clamp(CatmullRom(rows[0], rows[1], rows[2], rows[3], ty), -0.25, 1.25);
    }

    private static double CatmullRom(double p0, double p1, double p2, double p3, double t)
        => 0.5 * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t);

    // ---- Marching squares ----

    private static List<List<Vector2>> MarchingSquares(float[,] field, int nx, int ny, float level)
    {
        // Edge id: horizontal edge (x,y)-(x+1,y) = 2*(y*nx+x), vertical edge (x,y)-(x,y+1) = 2*(y*nx+x)+1.
        var links = new Dictionary<int, List<int>>();

        void Link(int a, int b)
        {
            if (!links.TryGetValue(a, out var la))
                links[a] = la = new List<int>(2);
            if (!links.TryGetValue(b, out var lb))
                links[b] = lb = new List<int>(2);
            la.Add(b);
            lb.Add(a);
        }

        for (var y = 0; y < ny - 1; y++)
        {
            for (var x = 0; x < nx - 1; x++)
            {
                float a = field[y, x], b = field[y, x + 1], c = field[y + 1, x + 1], d = field[y + 1, x];
                var index = (a >= level ? 1 : 0) | (b >= level ? 2 : 0) | (c >= level ? 4 : 0) | (d >= level ? 8 : 0);
                if (index is 0 or 15)
                    continue;

                var top = 2 * (y * nx + x);
                var bottom = 2 * ((y + 1) * nx + x);
                var left = 2 * (y * nx + x) + 1;
                var right = 2 * (y * nx + x + 1) + 1;
                switch (index)
                {
                    case 1: case 14: Link(top, left); break;
                    case 2: case 13: Link(top, right); break;
                    case 3: case 12: Link(left, right); break;
                    case 4: case 11: Link(right, bottom); break;
                    case 6: case 9: Link(top, bottom); break;
                    case 7: case 8: Link(left, bottom); break;
                    case 5:
                    case 10:
                        var center = (a + b + c + d) / 4;
                        var connectDiagonal = (center >= level) == (index == 5);
                        if (connectDiagonal)
                        {
                            Link(top, right);
                            Link(left, bottom);
                        }
                        else
                        {
                            Link(top, left);
                            Link(right, bottom);
                        }

                        break;
                }
            }
        }

        Vector2 EdgePoint(int id)
        {
            var vertical = (id & 1) == 1;
            var cell = id >> 1;
            var y = cell / nx;
            var x = cell % nx;
            if (!vertical)
            {
                float v0 = field[y, x], v1 = field[y, x + 1];
                var t = Math.Abs(v1 - v0) < 1e-6f ? 0.5f : (level - v0) / (v1 - v0);
                return new Vector2(x + t, y);
            }
            else
            {
                float v0 = field[y, x], v1 = field[y + 1, x];
                var t = Math.Abs(v1 - v0) < 1e-6f ? 0.5f : (level - v0) / (v1 - v0);
                return new Vector2(x, y + t);
            }
        }

        var visited = new HashSet<int>();
        var result = new List<List<Vector2>>();
        foreach (var start in links.Keys.ToList())
        {
            if (visited.Contains(start))
                continue;

            var loop = new List<Vector2>();
            var previous = -1;
            var current = start;
            while (!visited.Contains(current))
            {
                visited.Add(current);
                loop.Add(EdgePoint(current));
                var next = links[current].FirstOrDefault(candidate => candidate != previous && !visited.Contains(candidate), -1);
                if (next == -1)
                    break;
                previous = current;
                current = next;
            }

            if (loop.Count >= 4)
                result.Add(loop);
        }

        return result;
    }

    // ---- Curve fitting (Schneider) ----

    private static void AppendFittedLoop(StringBuilder builder, List<Vector2> loop)
    {
        // Smooth the dense contour slightly, then find corners with a wide window so staircase noise is not seen as a corner.
        var count = loop.Count;
        var corners = new List<int>();
        const int window = 6;
        for (var i = 0; i < count; i++)
        {
            var before = loop[i] - loop[(((i - window) % count) + count) % count];
            var after = loop[(i + window) % count] - loop[i];
            if (before.Length() < 1e-4f || after.Length() < 1e-4f)
                continue;

            var angle = Math.Acos(Math.Clamp(Vector2.Dot(Vector2.Normalize(before), Vector2.Normalize(after)), -1f, 1f)) * 180 / Math.PI;
            if (angle > 38)
                corners.Add(i);
        }

        // Keep only the sharpest point of every group of neighboring corner candidates.
        var picked = new List<int>();
        var used = new bool[count];
        foreach (var i in corners.OrderByDescending(i => CornerAngle(loop, i, window)))
        {
            var tooClose = false;
            for (var d = -window; d <= window; d++)
            {
                if (used[(((i + d) % count) + count) % count])
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            picked.Add(i);
            used[i] = true;
        }

        picked.Sort();
        var segments = new List<List<Vector2>>();
        if (picked.Count == 0)
        {
            // Smooth closed curve: split in two halves at arbitrary points so each fits well.
            var half = count / 2;
            segments.Add(Slice(loop, 0, half));
            segments.Add(Slice(loop, half, count));
        }
        else
        {
            for (var s = 0; s < picked.Count; s++)
            {
                var from = picked[s];
                var to = picked[(s + 1) % picked.Count];
                segments.Add(Slice(loop, from, to <= from ? to + count : to));
            }
        }

        var first = segments[0][0];
        builder.Append(CultureInfo.InvariantCulture, $"M{F(first.X)} {F(first.Y)}");
        foreach (var segment in segments)
        {
            foreach (var curve in FitCubic(segment, 0.045f))
                builder.Append(CultureInfo.InvariantCulture, $"C{F(curve[1].X)} {F(curve[1].Y)} {F(curve[2].X)} {F(curve[2].Y)} {F(curve[3].X)} {F(curve[3].Y)}");
        }

        builder.Append("Z");
    }

    private static double CornerAngle(List<Vector2> loop, int i, int window)
    {
        var count = loop.Count;
        var before = Vector2.Normalize(loop[i] - loop[(((i - window) % count) + count) % count]);
        var after = Vector2.Normalize(loop[(i + window) % count] - loop[i]);
        return Math.Acos(Math.Clamp(Vector2.Dot(before, after), -1f, 1f));
    }

    private static List<Vector2> Slice(List<Vector2> loop, int from, int to)
    {
        var result = new List<Vector2>();
        for (var i = from; i <= to; i++)
            result.Add(loop[i % loop.Count]);
        return result;
    }

    private static string F(float value) => Math.Round(value, 3).ToString("0.###", CultureInfo.InvariantCulture);

    private static List<Vector2[]> FitCubic(List<Vector2> points, float error)
    {
        var curves = new List<Vector2[]>();
        if (points.Count < 2)
            return curves;

        var cleaned = new List<Vector2> { points[0] };
        foreach (var p in points.Skip(1))
        {
            if ((p - cleaned[^1]).Length() > 1e-4f)
                cleaned.Add(p);
        }

        if (cleaned.Count < 2)
            return curves;

        if (cleaned.Count == 2)
        {
            var third = (cleaned[1] - cleaned[0]) / 3;
            curves.Add([cleaned[0], cleaned[0] + third, cleaned[1] - third, cleaned[1]]);
            return curves;
        }

        var leftTangent = Normalize(cleaned[Math.Min(3, cleaned.Count - 1)] - cleaned[0]);
        var rightTangent = Normalize(cleaned[Math.Max(0, cleaned.Count - 4)] - cleaned[^1]);
        FitRecursive(cleaned, 0, cleaned.Count - 1, leftTangent, rightTangent, error, curves);
        return curves;
    }

    private static Vector2 Normalize(Vector2 v) => v.Length() < 1e-6f ? Vector2.Zero : Vector2.Normalize(v);

    private static void FitRecursive(List<Vector2> d, int first, int last, Vector2 tHat1, Vector2 tHat2, float error, List<Vector2[]> output)
    {
        var count = last - first + 1;
        if (count == 2)
        {
            var dist = (d[last] - d[first]).Length() / 3;
            output.Add([d[first], d[first] + tHat1 * dist, d[last] + tHat2 * dist, d[last]]);
            return;
        }

        var u = ChordLengthParameterize(d, first, last);
        var bez = GenerateBezier(d, first, last, u, tHat1, tHat2);
        var maxError = ComputeMaxError(d, first, last, bez, u, out var splitPoint);
        if (maxError < error * error)
        {
            output.Add(bez);
            return;
        }

        if (maxError < error * error * 16)
        {
            for (var i = 0; i < 4; i++)
            {
                u = Reparameterize(d, first, last, u, bez);
                bez = GenerateBezier(d, first, last, u, tHat1, tHat2);
                maxError = ComputeMaxError(d, first, last, bez, u, out splitPoint);
                if (maxError < error * error)
                {
                    output.Add(bez);
                    return;
                }
            }
        }

        splitPoint = Math.Clamp(splitPoint, first + 1, last - 1);
        var tHatCenter = Normalize(d[splitPoint - 1] - d[splitPoint + 1]);
        FitRecursive(d, first, splitPoint, tHat1, tHatCenter, error, output);
        FitRecursive(d, splitPoint, last, -tHatCenter, tHat2, error, output);
    }

    private static double[] ChordLengthParameterize(List<Vector2> d, int first, int last)
    {
        var u = new double[last - first + 1];
        for (var i = first + 1; i <= last; i++)
            u[i - first] = u[i - first - 1] + (d[i] - d[i - 1]).Length();
        var total = u[last - first];
        if (total < 1e-9)
            return u;
        for (var i = 1; i <= last - first; i++)
            u[i] /= total;
        return u;
    }

    private static Vector2[] GenerateBezier(List<Vector2> d, int first, int last, double[] uPrime, Vector2 tHat1, Vector2 tHat2)
    {
        var count = last - first + 1;
        var c = new double[2, 2];
        var x = new double[2];
        for (var i = 0; i < count; i++)
        {
            var u = uPrime[i];
            var a1 = tHat1 * (float)B1(u);
            var a2 = tHat2 * (float)B2(u);
            c[0, 0] += Vector2.Dot(a1, a1);
            c[0, 1] += Vector2.Dot(a1, a2);
            c[1, 1] += Vector2.Dot(a2, a2);
            var tmp = d[first + i] - (d[first] * (float)(B0(u) + B1(u)) + d[last] * (float)(B2(u) + B3(u)));
            x[0] += Vector2.Dot(a1, tmp);
            x[1] += Vector2.Dot(a2, tmp);
        }

        c[1, 0] = c[0, 1];
        var det01 = c[0, 0] * c[1, 1] - c[1, 0] * c[0, 1];
        var det0X = c[0, 0] * x[1] - c[1, 0] * x[0];
        var detX1 = x[0] * c[1, 1] - x[1] * c[0, 1];
        var alphaL = Math.Abs(det01) < 1e-12 ? 0 : detX1 / det01;
        var alphaR = Math.Abs(det01) < 1e-12 ? 0 : det0X / det01;
        var segLength = (d[last] - d[first]).Length();
        var epsilon = 1e-6 * segLength;
        if (alphaL < epsilon || alphaR < epsilon)
        {
            var dist = segLength / 3;
            return [d[first], d[first] + tHat1 * dist, d[last] + tHat2 * dist, d[last]];
        }

        return [d[first], d[first] + tHat1 * (float)alphaL, d[last] + tHat2 * (float)alphaR, d[last]];
    }

    private static double[] Reparameterize(List<Vector2> d, int first, int last, double[] u, Vector2[] bez)
    {
        var result = new double[last - first + 1];
        for (var i = first; i <= last; i++)
            result[i - first] = NewtonRaphson(bez, d[i], u[i - first]);
        return result;
    }

    private static double NewtonRaphson(Vector2[] q, Vector2 p, double u)
    {
        var d = Evaluate(q, u) - p;
        var q1 = new Vector2[3];
        var q2 = new Vector2[2];
        for (var i = 0; i < 3; i++)
            q1[i] = (q[i + 1] - q[i]) * 3;
        for (var i = 0; i < 2; i++)
            q2[i] = (q1[i + 1] - q1[i]) * 2;
        var q1u = EvaluateQuad(q1, u);
        var q2u = EvaluateLinear(q2, u);
        var numerator = Vector2.Dot(d, q1u);
        var denominator = Vector2.Dot(q1u, q1u) + Vector2.Dot(d, q2u);
        return Math.Abs(denominator) < 1e-12 ? u : Math.Clamp(u - numerator / denominator, 0, 1);
    }

    private static double ComputeMaxError(List<Vector2> d, int first, int last, Vector2[] bez, double[] u, out int splitPoint)
    {
        splitPoint = (last - first + 1) / 2 + first;
        var maxDist = 0.0;
        for (var i = first + 1; i < last; i++)
        {
            var dist = (Evaluate(bez, u[i - first]) - d[i]).LengthSquared();
            if (dist >= maxDist)
            {
                maxDist = dist;
                splitPoint = i;
            }
        }

        return maxDist;
    }

    private static Vector2 Evaluate(Vector2[] v, double t)
        => v[0] * (float)B0(t) + v[1] * (float)B1(t) + v[2] * (float)B2(t) + v[3] * (float)B3(t);

    private static Vector2 EvaluateQuad(Vector2[] v, double t)
    {
        var s = 1 - t;
        return v[0] * (float)(s * s) + v[1] * (float)(2 * s * t) + v[2] * (float)(t * t);
    }

    private static Vector2 EvaluateLinear(Vector2[] v, double t) => v[0] * (float)(1 - t) + v[1] * (float)t;

    private static double B0(double u) => Math.Pow(1 - u, 3);

    private static double B1(double u) => 3 * u * Math.Pow(1 - u, 2);

    private static double B2(double u) => 3 * u * u * (1 - u);

    private static double B3(double u) => u * u * u;
}

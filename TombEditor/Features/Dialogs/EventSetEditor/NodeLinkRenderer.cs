#nullable enable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Builds the geometry and colors of node links (ribbons or ropes), like the previous node editor drew them.</summary>
    public static class NodeLinkRenderer
    {
        public const double ConnectedOpacity = 0.8;
        public const double PendingOpacity = 0.6;

        /// <summary>A horizontal span (left, right, y) that a link starts or ends at.</summary>
        public readonly record struct Span(double Left, double Right, double Y);

        /// <summary>Creates the link shape: a filled quad between the two grips, or a bezier rope between their centers.</summary>
        public static Geometry CreateGeometry(Span from, Span to, bool asRope)
        {
            var figure = new PathFigure();
            bool filled = !asRope;

            if (asRope)
            {
                double startX = (from.Left + from.Right) / 2.0;
                double endX = (to.Left + to.Right) / 2.0;
                double midY = (from.Y + to.Y) / 2.0;

                figure.StartPoint = new Point(startX, from.Y);
                figure.Segments.Add(new BezierSegment(new Point(startX, midY), new Point(endX, midY), new Point(endX, to.Y), true));
            }
            else
            {
                figure.StartPoint = new Point(from.Left, from.Y);
                figure.Segments.Add(new LineSegment(new Point(from.Right, from.Y), true));
                figure.Segments.Add(new LineSegment(new Point(to.Right, to.Y), true));
                figure.Segments.Add(new LineSegment(new Point(to.Left, to.Y), true));
                figure.IsClosed = true;
            }

            figure.IsFilled = filled;

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();
            return geometry;
        }

        /// <summary>Gets the rope color (normalized, but kept dark for dark nodes) or the translucent ribbon color of a link.</summary>
        public static Brush CreateBrush(Vector3 color, bool isElse, bool asRope, double opacity)
        {
            if (isElse)
                color = FlipColorBias(color);

            Color result;
            if (asRope)
            {
                var normalized = color == Vector3.Zero ? color : Vector3.Normalize(color);
                float luma = color.GetLuma();
                if (luma < 0.33f)
                    normalized = MathC.SmoothStep(color, normalized, luma * 3.0f);

                result = ToMediaColor(normalized, 1.0);
            }
            else
            {
                result = ToMediaColor(color, opacity);
            }

            var brush = new SolidColorBrush(result);
            brush.Freeze();
            return brush;
        }

        /// <summary>Gets the rope thickness; ropes are thin, ribbons have no outline.</summary>
        public static double GetThickness(bool asRope) => asRope ? 2.0 : 0.0;

        /// <summary>Reverses the color bias of an Else link, so it can be told apart from a Next link of the same node.</summary>
        public static Vector3 FlipColorBias(Vector3 source)
        {
            double hue = GetHue(source);
            var flipYX = new Vector3(source.Y, source.X, source.Z);
            var flipZX = new Vector3(source.X, source.Z, source.Y);

            return Math.Abs(GetHue(flipYX) - hue) > 90.0 ? flipYX : flipZX;
        }

        private static double GetHue(Vector3 color)
        {
            float max = Math.Max(color.X, Math.Max(color.Y, color.Z));
            float min = Math.Min(color.X, Math.Min(color.Y, color.Z));
            float delta = max - min;

            if (delta <= 0.0f)
                return 0.0;

            double hue;
            if (max == color.X)
                hue = (color.Y - color.Z) / delta;
            else if (max == color.Y)
                hue = 2.0 + (color.Z - color.X) / delta;
            else
                hue = 4.0 + (color.X - color.Y) / delta;

            hue *= 60.0;
            return hue < 0.0 ? hue + 360.0 : hue;
        }

        private static Color ToMediaColor(Vector3 color, double opacity) => Color.FromArgb(
            (byte)Math.Clamp(Math.Round(opacity * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.X * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Y * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Z * 255.0), 0.0, 255.0));
    }
}

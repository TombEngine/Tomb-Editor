#nullable disable

using System;
using System.Numerics;
using System.Windows;
using TombLib;

namespace TombEditor.Features.Map2D
{
    // Viewport <-> world coordinate conversions.
    public partial class Panel2DMap
    {
        public Vector2 FromVisualCoord(Point pos)
        {
            return new Vector2(
                (float)(pos.X - ActualWidth * 0.5) / _viewScale + ViewPosition.X,
                (float)(ActualHeight * 0.5 - pos.Y) / _viewScale + ViewPosition.Y);
        }

        public Rectangle2 FromVisualCoord(Rect area)
        {
            Vector2 visibleAreaStart = FromVisualCoord(new Point(area.Left, area.Top));
            Vector2 visibleAreaEnd = FromVisualCoord(new Point(area.Right, area.Bottom));
            return new Rectangle2(Vector2.Min(visibleAreaStart, visibleAreaEnd), Vector2.Max(visibleAreaStart, visibleAreaEnd));
        }

        public Point ToVisualCoord(Vector2 pos)
        {
            return new Point(
                (pos.X - ViewPosition.X) * _viewScale + ActualWidth * 0.5,
                ActualHeight * 0.5 - (pos.Y - ViewPosition.Y) * _viewScale);
        }

        public Rect ToVisualCoord(Rectangle2 area)
        {
            Point start = ToVisualCoord(area.Start);
            Point end = ToVisualCoord(area.End);
            return new Rect(
                new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y)),
                new Point(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y)));
        }

        private Size ControlSize => new(ActualWidth, ActualHeight);
    }
}

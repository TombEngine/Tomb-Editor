#nullable enable

using System;
using System.Windows;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

// Zoom, pan, and viewport clamping.
public partial class Panel2DGrid
{
    private bool PanBy(double deltaX, double deltaY)
    {
        if (!CanNavigateView())
            return false;

        double previousOffsetX = _viewOffsetX;
        double previousOffsetY = _viewOffsetY;

        _viewOffsetX += deltaX;
        _viewOffsetY += deltaY;
        CoerceViewOffset();

        if (_viewOffsetX == previousOffsetX && _viewOffsetY == previousOffsetY)
            return false;

        InvalidateVisual();
        return true;
    }

    private bool PanToFollowDrag(Point position)
    {
        double deltaX = GetDragPanDelta(position.X, ActualWidth);
        double deltaY = GetDragPanDelta(position.Y, ActualHeight);

        if (deltaX != 0.0 || deltaY != 0.0)
            return PanBy(deltaX, deltaY);

        return false;
    }

    private static double GetDragPanDelta(double position, double viewportSize)
    {
        if (viewportSize <= DragPanEdgeMargin * 2.0)
            return 0.0;

        if (position < DragPanEdgeMargin)
            return Math.Clamp(DragPanEdgeMargin - position, 0.0, DragPanMaxStep);

        double lowerEdge = viewportSize - DragPanEdgeMargin;

        if (position > lowerEdge)
            return -Math.Clamp(position - lowerEdge, 0.0, DragPanMaxStep);

        return 0.0;
    }

    private void ZoomBy(double factor, Point anchor)
    {
        if (!CanNavigateView())
            return;

        double previousGridStep = GetGridStep();

        if (previousGridStep <= 0.0)
            return;

        var previousArea = GetVisualAreaTotal();
        double anchorGridX = (anchor.X - previousArea.X) / previousGridStep;
        double anchorGridY = (anchor.Y - previousArea.Y) / previousGridStep;
        double previousScale = _viewScale;

        _viewScale = Math.Clamp(_viewScale * factor, MinZoom, MaxZoom);

        if (Math.Abs(_viewScale - previousScale) < double.Epsilon)
            return;

        double newGridStep = GetGridStep();
        var centeredArea = GetCenteredVisualAreaTotal(newGridStep);

        _viewOffsetX = anchor.X - (anchorGridX * newGridStep) - centeredArea.X;
        _viewOffsetY = anchor.Y - (anchorGridY * newGridStep) - centeredArea.Y;

        CoerceViewOffset();
        InvalidateVisual();
    }

    private void ResetView()
    {
        _viewScale = 1.0;
        _viewOffsetX = 0.0;
        _viewOffsetY = 0.0;
        InvalidateVisual();
    }

    private bool CanNavigateView()
        => Room is not null && ActualWidth > 0.0 && ActualHeight > 0.0 && GetFitGridStep() > 0.0;

    private bool CanPanHorizontally(double delta)
    {
        var centeredArea = GetCenteredVisualAreaTotal(GetGridStep());
        double newOffset = CoerceViewOffset(_viewOffsetX + delta, ActualWidth, centeredArea.X, centeredArea.Width);
        return newOffset != _viewOffsetX;
    }

    private bool CanPanVertically(double delta)
    {
        var centeredArea = GetCenteredVisualAreaTotal(GetGridStep());
        double newOffset = CoerceViewOffset(_viewOffsetY + delta, ActualHeight, centeredArea.Y, centeredArea.Height);
        return newOffset != _viewOffsetY;
    }

    private void CoerceViewOffset()
    {
        var centeredArea = GetCenteredVisualAreaTotal(GetGridStep());
        _viewOffsetX = CoerceViewOffset(_viewOffsetX, ActualWidth, centeredArea.X, centeredArea.Width);
        _viewOffsetY = CoerceViewOffset(_viewOffsetY, ActualHeight, centeredArea.Y, centeredArea.Height);
    }

    private static double CoerceViewOffset(double offset, double viewportSize, double centeredStart, double contentSize)
    {
        if (viewportSize <= 0.0 || contentSize <= 0.0 || contentSize <= viewportSize)
            return 0.0;

        double minOffset = viewportSize - centeredStart - contentSize;
        double maxOffset = -centeredStart;
        return Math.Clamp(offset, minOffset, maxOffset);
    }

    private Point GetViewCenter()
        => new(ActualWidth * 0.5, ActualHeight * 0.5);
}

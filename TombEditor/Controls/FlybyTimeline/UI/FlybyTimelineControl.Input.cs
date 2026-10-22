#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TombEditor.Controls.FlybyTimeline.UI;

// Mouse and keyboard input handling for the timeline control.
public partial class FlybyTimelineControl
{
    /// <summary>
    /// Starts scrubbing, dragging, repositioning, or track click / range selection from a left click.
    /// </summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();

        if (e.ClickCount >= 2)
            return;

        var pos = e.GetPosition(this);

        if (pos.Y < FlybyConstants.TimelineRulerHeight)
        {
            BeginScrub((float)pos.X);
            return;
        }

        int hitIndex = HitTestMarker(pos);

        if (hitIndex >= 0)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                BeginReposition(hitIndex, (float)pos.X);
            else
                BeginMarkerDrag(hitIndex, pos);

            return;
        }

        BeginRangeSelection(pos);
    }

    /// <summary>
    /// Updates the active interaction while the mouse moves over the control.
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var pos = e.GetPosition(this);
        float mouseX = (float)pos.X;

        UpdateMouseTracking(mouseX);

        if (_interactionMode == InteractionMode.Panning)
            UpdatePan(pos);
        else if (_interactionMode == InteractionMode.Repositioning && e.LeftButton == MouseButtonState.Pressed)
            UpdateReposition(mouseX);
        else if (_interactionMode == InteractionMode.MarkerDrag && _dragIndex >= 0 && e.LeftButton == MouseButtonState.Pressed)
            UpdateMarkerDrag(pos);
        else if (_interactionMode == InteractionMode.Scrubbing && e.LeftButton == MouseButtonState.Pressed)
            UpdateScrub(mouseX);
        else if (_interactionMode == InteractionMode.RangeSelecting && e.LeftButton == MouseButtonState.Pressed)
            UpdateRangeSelection(pos);

        InvalidateVisual();
    }

    /// <summary>
    /// Completes the current left-button interaction.
    /// </summary>
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        EndLeftMouseInteraction(e.GetPosition(this));
    }

    /// <summary>
    /// Hides hover visuals when the pointer leaves the control.
    /// </summary>
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        _isMouseOver = false;
        _mouseX = -1.0f;

        InvalidateVisual();
    }

    /// <summary>
    /// Enables hover visuals when the pointer enters the control.
    /// </summary>
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        UpdateMouseTracking((float)e.GetPosition(this).X);
        InvalidateVisual();
    }

    /// <summary>
    /// Zooms or pans the timeline viewport in response to the mouse wheel.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
        {
            if (e.Delta > 0)
                PanLeft(FlybyConstants.TimelineSmoothPanEnabled);
            else if (e.Delta < 0)
                PanRight(FlybyConstants.TimelineSmoothPanEnabled);

            e.Handled = true;
            return;
        }

        var pos = e.GetPosition(this);
        GetInteractiveViewport(out float baseStart, out float baseEnd);
        float pivotTime = PixelToTime((float)pos.X, (float)ActualWidth, baseStart, baseEnd);
        float zoomFactor = e.Delta > 0 ? 0.8f : 1.25f;

        float newStart = pivotTime - ((pivotTime - baseStart) * zoomFactor);
        float newEnd = pivotTime + ((baseEnd - pivotTime) * zoomFactor);

        ClampViewportToBounds(ref newStart, ref newEnd);

        if (newEnd - newStart < FlybyConstants.TimelineMinViewportRange)
            return;

        ApplyViewport(newStart, newEnd, FlybyConstants.TimelineSmoothZoomEnabled);
    }

    /// <summary>
    /// Starts middle-button panning.
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton == MouseButton.Middle)
            BeginPan(e.GetPosition(this), false);
    }

    /// <summary>
    /// Starts right-button panning and tracks whether it becomes a drag.
    /// </summary>
    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);
        _rightButtonPanned = false;

        BeginPan(e.GetPosition(this), true);
    }

    /// <summary>
    /// Ends middle-button panning when active.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.ChangedButton == MouseButton.Middle && _interactionMode == InteractionMode.Panning)
            EndPan();
    }

    /// <summary>
    /// Ends right-button panning, moving the playhead on a click and suppressing the context menu for both click and drag.
    /// </summary>
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);

        if (_interactionMode != InteractionMode.Panning)
            return;

        if (!_rightButtonPanned)
        {
            CommitTrackClick((float)e.GetPosition(this).X);
            e.Handled = true;
        }

        EndPan();

        if (_rightButtonPanned)
            e.Handled = true;
    }

    /// <summary>
    /// Opens marker editing or zooms the viewport out on double-click.
    /// </summary>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        var pos = e.GetPosition(this);
        int hitIndex = HitTestMarker(pos);

        if (hitIndex >= 0)
        {
            MarkerDoubleClicked?.Invoke(hitIndex);
            return;
        }

        ZoomToFit();
    }

    /// <summary>
    /// Handles keyboard shortcuts for the timeline.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        e.Handled = HandleKeyShortcut(Keyboard.Modifiers, e.Key);
    }

    /// <summary>
    /// Handles keyboard shortcuts with modifiers (e.g. Ctrl+A) and without (e.g. Space for Play/Stop).
    /// Shortcut handling is separated to avoid conflicts between timeline-specific shortcuts and global ones.
    /// </summary>
    /// <returns><see langword="true"/> if the key was handled; otherwise, <see langword="false"/>.</returns>
    private bool HandleKeyShortcut(ModifierKeys modifiers, Key key) => modifiers switch
    {
        ModifierKeys.None => TryHandleTimelineShortcut(key),
        ModifierKeys.Control => TryHandleCtrlShortcut(key),
        _ => false,
    };

    /// <summary>
    /// Handles Ctrl+ shortcuts for the timeline, such as Select All.
    /// </summary>
    private bool TryHandleCtrlShortcut(Key key)
    {
        switch (key)
        {
            case Key.A:
                SelectAllRequested?.Invoke();
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Handles shortcuts without modifiers, such as Play/Stop and Delete.
    /// </summary>
    private bool TryHandleTimelineShortcut(Key key)
    {
        switch (key)
        {
            case Key.Space:
                PlayStopRequested?.Invoke();
                return true;

            case Key.Delete or Key.Back:
                DeleteRequested?.Invoke();
                return true;

            case Key.Left:
                PanLeft();
                return true;

            case Key.Right:
                PanRight();
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Expands the viewport to show the full sequence range.
    /// </summary>
    public void ZoomToFit(bool smooth = FlybyConstants.TimelineSmoothZoomEnabled)
        => ApplyViewport(0.0f, _totalDurationSeconds * FlybyConstants.TimelineZoomOutScale, smooth);

    /// <summary>
    /// Pans the viewport left by one configured step.
    /// </summary>
    public void PanLeft(bool smooth = false)
    {
        GetInteractiveViewport(out float startSeconds, out float endSeconds);
        PanBy(-(endSeconds - startSeconds) * FlybyConstants.TimelinePanStepFraction, smooth);
    }

    /// <summary>
    /// Pans the viewport right by one configured step.
    /// </summary>
    public void PanRight(bool smooth = false)
    {
        GetInteractiveViewport(out float startSeconds, out float endSeconds);
        PanBy((endSeconds - startSeconds) * FlybyConstants.TimelinePanStepFraction, smooth);
    }

    /// <summary>
    /// Updates mouse position state used for cursor-line rendering.
    /// </summary>
    private void UpdateMouseTracking(float mouseX)
    {
        _mouseX = mouseX;
        _isMouseOver = true;
    }

    /// <summary>
    /// Begins timeline scrubbing from the ruler.
    /// </summary>
    private void BeginScrub(float mouseX)
    {
        _interactionMode = InteractionMode.Scrubbing;

        CaptureMouse();
        UpdateScrub(mouseX);
    }

    /// <summary>
    /// Begins marker reposition mode using Alt-drag.
    /// </summary>
    private void BeginReposition(int hitIndex, float mouseX)
    {
        MarkerClicked?.Invoke(hitIndex);

        _interactionMode = InteractionMode.Repositioning;
        _repositionGhostX = mouseX;
        _repositionFromIndex = hitIndex;
        _repositionTargetIndex = hitIndex;
        CaptureMouse();
    }

    /// <summary>
    /// Begins dragging a marker to adjust its timeline position.
    /// </summary>
    private void BeginMarkerDrag(int hitIndex, Point mousePosition)
    {
        _interactionMode = InteractionMode.MarkerDrag;
        _dragIndex = hitIndex;
        _isDragging = false;
        _dragStartPoint = mousePosition;
        _dragMouseOffsetSeconds = PixelToTime((float)mousePosition.X, (float)ActualWidth) - _markers[hitIndex].TimeSeconds;
        CaptureMouse();
    }

    /// <summary>
    /// Begins a pending track click that becomes marquee selection only after a real drag.
    /// </summary>
    private void BeginRangeSelection(Point mousePosition)
    {
        _interactionMode = InteractionMode.RangeSelecting;
        _rangeStartPoint = mousePosition;
        _rangeEndPoint = mousePosition;
        CaptureMouse();
        InvalidateVisual();
    }

    /// <summary>
    /// Starts viewport panning from the given mouse position.
    /// </summary>
    private void BeginPan(Point startPosition, bool requireThreshold)
    {
        if (HasActiveLeftMouseInteraction())
            return;

        _interactionMode = InteractionMode.Panning;
        _panThresholdStartPoint = startPosition;
        _panAnchorPixelX = (float)startPosition.X;
        _panAnchorViewRange = 0.0f;
        _panThresholdPending = requireThreshold;

        if (!requireThreshold)
            StartPanDrag(_panAnchorPixelX);

        CaptureMouse();
        Focus();
    }

    /// <summary>
    /// Initializes active viewport panning once any required drag threshold has been crossed.
    /// </summary>
    private void StartPanDrag(float currentPixelX)
    {
        SnapViewportToInteractiveState();

        _panThresholdPending = false;
        _panWarpPending = false;

        Cursor = Cursors.SizeWE;
        ResetPanAnchor(currentPixelX);
    }

    /// <summary>
    /// Updates the viewport while a pan drag is in progress.
    /// </summary>
    private void UpdatePan(Point currentPosition)
    {
        if (_panThresholdPending)
        {
            if (!HasExceededDragThreshold(_panThresholdStartPoint, currentPosition))
                return;

            StartPanDrag((float)currentPosition.X);
            _rightButtonPanned = true;
            return;
        }

        float w = (float)ActualWidth;

        if (w <= 0)
            return;

        float currentPixelX = (float)currentPosition.X;

        if (TryConsumePendingPanWarp(currentPixelX, w))
            return;

        if (_panAnchorViewRange <= 0)
            return;

        float deltaPixels = currentPixelX - _panAnchorPixelX;
        float deltaSeconds = -(deltaPixels / w) * _panAnchorViewRange;
        float newStart = ClampVisibleStart(_panAnchorViewSeconds + deltaSeconds, _panAnchorViewRange);

        SetViewport(newStart, newStart + _panAnchorViewRange, false);

        if (TryWarpPanCursor(currentPixelX, newStart, w, out float warpedPixelX))
        {
            UpdateMouseTracking(warpedPixelX);
            ResetPanAnchor(warpedPixelX);

            _panWarpPending = true;
            _panWarpTargetPixelX = warpedPixelX;
            return;
        }

        ResetPanAnchor(currentPixelX);
    }

    /// <summary>
    /// Re-anchors the pan drag origin to the current cursor position and viewport state.
    /// </summary>
    private void ResetPanAnchor(float currentPixelX)
    {
        GetInteractiveViewport(out float startSeconds, out float endSeconds);

        _panAnchorPixelX = currentPixelX;
        _panAnchorViewSeconds = startSeconds;
        _panAnchorViewRange = endSeconds - startSeconds;
    }

    /// <summary>
    /// Ignores stale opposite-edge mouse-move events until the post-warp cursor position is observed.
    /// </summary>
    private bool TryConsumePendingPanWarp(float currentPixelX, float width)
    {
        if (!_panWarpPending)
            return false;

        bool targetOnLeftHalf = _panWarpTargetPixelX <= width * 0.5f;
        bool currentOnLeftHalf = currentPixelX <= width * 0.5f;

        if (targetOnLeftHalf != currentOnLeftHalf)
            return true;

        _panWarpPending = false;
        return false;
    }

    /// <summary>
    /// Warps the OS cursor to the opposite edge when it reaches the control boundary during a pan drag.
    /// Returns <see langword="true"/> and the new local pixel X when a warp occurred.
    /// </summary>
    private bool TryWarpPanCursor(float currentPixelX, float newStart, float width, out float warpedPixelX)
    {
        const float edgeThreshold = 1.0f;
        const float edgeInset = 2.0f;

        warpedPixelX = currentPixelX;

        if (!IsMouseCaptured || width <= edgeInset * 2.0f)
            return false;

        if (currentPixelX <= edgeThreshold)
        {
            if (newStart >= GetMaxViewportStart(_panAnchorViewRange))
                return false;

            warpedPixelX = width - edgeInset;
        }
        else if (currentPixelX >= width - edgeThreshold)
        {
            if (newStart <= 0.0f)
                return false;

            warpedPixelX = edgeInset;
        }
        else
        {
            return false;
        }

        var localY = Math.Clamp(Mouse.GetPosition(this).Y, 0.0, Math.Max(0.0, ActualHeight - 1.0));
        var screenPoint = PointToScreen(new Point(warpedPixelX, localY));

        System.Windows.Forms.Cursor.Position = new System.Drawing.Point( // TODO: Remove dependency on WinForms for cursor warping if possible.
            (int)Math.Round(screenPoint.X),
            (int)Math.Round(screenPoint.Y));

        return true;
    }

    /// <summary>
    /// Updates the reorder target while a marker is being repositioned.
    /// </summary>
    private void UpdateReposition(float mouseX)
    {
        _repositionGhostX = mouseX;
        _repositionTargetIndex = ComputeReorderTargetIndex(mouseX, (float)ActualWidth);
    }

    /// <summary>
    /// Updates a dragged marker and emits its requested target time.
    /// </summary>
    private void UpdateMarkerDrag(Point mousePosition)
    {
        if (!_isDragging && HasExceededDragThreshold(_dragStartPoint, mousePosition))
        {
            _isDragging = true;
            MarkerClicked?.Invoke(_dragIndex);
        }

        if (!_isDragging)
            return;

        float newTime = PixelToTime((float)mousePosition.X, (float)ActualWidth) - _dragMouseOffsetSeconds;
        MarkerDragged?.Invoke(_dragIndex, Math.Max(0.0f, newTime));
    }

    /// <summary>
    /// Updates scrub playback time from the current mouse position.
    /// </summary>
    private void UpdateScrub(float mouseX)
    {
        float scrubTime = PixelToTime(mouseX, (float)ActualWidth);
        ScrubRequested?.Invoke(Math.Max(0.0f, scrubTime));
    }

    /// <summary>
    /// Updates pending track click or marquee selection visuals during drag.
    /// </summary>
    private void UpdateRangeSelection(Point mousePosition) => _rangeEndPoint = mousePosition;

    /// <summary>
    /// Ends the current viewport pan operation.
    /// </summary>
    private void EndPan()
    {
        _interactionMode = InteractionMode.None;
        _panThresholdPending = false;
        _panWarpPending = false;

        Cursor = null;
        ReleaseMouseCapture();
    }

    /// <summary>
    /// Ends any active left-button interaction and clears temporary state.
    /// </summary>
    private void EndLeftMouseInteraction(Point mousePosition)
    {
        int clickedMarkerIndex = _interactionMode == InteractionMode.MarkerDrag && !_isDragging ? _dragIndex : -1;

        if (_interactionMode == InteractionMode.Repositioning)
        {
            _interactionMode = InteractionMode.None;
            CommitRepositioning();
        }
        else if (_interactionMode == InteractionMode.RangeSelecting)
        {
            _interactionMode = InteractionMode.None;
            CommitRangeSelection(mousePosition);
        }

        if (_isDragging && _dragIndex >= 0)
            MarkerDragCompleted?.Invoke(_dragIndex);
        else if (clickedMarkerIndex >= 0)
            MarkerClicked?.Invoke(clickedMarkerIndex);

        if (_interactionMode != InteractionMode.Panning)
            _interactionMode = InteractionMode.None;

        _dragIndex = -1;
        _isDragging = false;
        _dragMouseOffsetSeconds = 0;

        if (IsMouseCaptured && _interactionMode != InteractionMode.Panning)
            ReleaseMouseCapture();

        InvalidateVisual();
    }

    /// <summary>
    /// Finalizes marquee selection or commits a track click to the released playhead position.
    /// </summary>
    private void CommitRangeSelection(Point mousePosition)
    {
        _rangeEndPoint = mousePosition;

        if (!HasExceededSelectionThreshold(_rangeEndPoint))
        {
            RangeSelected?.Invoke([]);
            CommitTrackClick((float)_rangeEndPoint.X);
            return;
        }

        RangeSelected?.Invoke(GetRangeSelection());
    }

    /// <summary>
    /// Moves the playhead to the clicked track position without entering continuous scrub mode.
    /// </summary>
    private void CommitTrackClick(float mouseX)
    {
        float clickTime = PixelToTime(mouseX, (float)ActualWidth);
        PlayheadRequested?.Invoke(Math.Max(0.0f, clickTime));
    }

    /// <summary>
    /// Commits a marker reorder operation when the target index changed.
    /// </summary>
    private void CommitRepositioning()
    {
        if (_repositionFromIndex < 0 || _repositionTargetIndex < 0 || _repositionFromIndex == _repositionTargetIndex)
        {
            _repositionFromIndex = -1;
            _repositionTargetIndex = -1;
            return;
        }

        MarkerReordered?.Invoke(_repositionFromIndex, _repositionTargetIndex);

        _repositionFromIndex = -1;
        _repositionTargetIndex = -1;
    }

    /// <summary>
    /// Returns marker indices inside the current marquee selection.
    /// </summary>
    private IReadOnlyList<int> GetRangeSelection()
    {
        GetRangeSelectionBounds(out float leftX, out float rightX);

        List<int> selected = [];

        for (int i = 0; i < _markers.Count; i++)
        {
            if (!TryGetMarkerPixel(_markers[i], (float)ActualWidth, out float x))
                continue;

            if (x >= leftX && x <= rightX)
                selected.Add(i);
        }

        return selected;
    }

    /// <summary>
    /// Returns the current marquee selection bounds in local pixel coordinates.
    /// </summary>
    private void GetRangeSelectionBounds(out float leftX, out float rightX)
    {
        float startX = (float)_rangeStartPoint.X;
        float endX = (float)_rangeEndPoint.X;

        leftX = Math.Min(startX, endX);
        rightX = Math.Max(startX, endX);
    }

    /// <summary>
    /// Returns the closest marker under the given mouse position.
    /// </summary>
    private int HitTestMarker(Point pos)
    {
        const float trackY = FlybyConstants.TimelineRulerHeight;

        if (pos.Y < trackY - 4 || pos.Y > ActualHeight)
            return -1;

        float closestDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < _markers.Count; i++)
        {
            if (!TryGetMarkerPixel(_markers[i], (float)ActualWidth, out float x))
                continue;

            float dist = Math.Abs((float)pos.X - x);

            if (dist < FlybyConstants.TimelineMarkerRadius + 4 && dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Calculates the marker index that a reposition drag should target.
    /// </summary>
    private int ComputeReorderTargetIndex(float mouseX, float width)
    {
        bool hasValidMarker = false;
        int lastBefore = -1;

        for (int i = 0; i < _markers.Count; i++)
        {
            if (!TryGetMarkerPixel(_markers[i], width, out float markerX))
                continue;

            hasValidMarker = true;

            if (markerX <= mouseX)
                lastBefore = i;
            else
                break;
        }

        if (!hasValidMarker)
            return _repositionFromIndex;

        if (_repositionFromIndex <= lastBefore)
            return lastBefore;

        return Math.Min(lastBefore + 1, _markers.Count - 1);
    }

    /// <summary>
    /// Returns whether a pointer drag has exceeded the system drag threshold.
    /// </summary>
    private static bool HasExceededDragThreshold(Point startPoint, Point currentPoint)
        => Math.Abs(currentPoint.X - startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance
        || Math.Abs(currentPoint.Y - startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;

    /// <summary>
    /// Returns whether a marquee selection drag is large enough to count.
    /// </summary>
    private bool HasExceededSelectionThreshold(Point currentPoint)
        => Math.Abs(currentPoint.X - _rangeStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance;

    /// <summary>
    /// Returns whether a left-button-driven interaction is currently active or pending.
    /// </summary>
    private bool HasActiveLeftMouseInteraction()
        => _interactionMode is InteractionMode.MarkerDrag or InteractionMode.Scrubbing
            or InteractionMode.RangeSelecting or InteractionMode.Repositioning;
}

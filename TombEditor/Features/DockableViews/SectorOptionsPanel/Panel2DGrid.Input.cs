#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

// Mouse and keyboard interaction.
public partial class Panel2DGrid
{
    private bool IsPanning => _panButton is not null;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();

        var position = e.GetPosition(this);

        if (Keyboard.IsKeyDown(Key.Space))
        {
            BeginPan(position, MouseButton.Left);
            e.Handled = true;
            return;
        }

        if (e.ClickCount == 2 && Keyboard.Modifiers == ModifierKeys.None)
        {
            HandleMouseDown(position, isRightButton: true);
            e.Handled = true;
            return;
        }

        _doSectorSelection = false;
        HandleMouseDown(position, isRightButton: false);

        if (_doSectorSelection && CaptureMouse())
            StartDragPan(position);

        e.Handled = true;
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);
        Focus();

        if (e.ClickCount == 2 && Keyboard.Modifiers == ModifierKeys.None)
        {
            ResetView();
            e.Handled = true;
            return;
        }

        var position = e.GetPosition(this);

        if (!BeginPan(position, MouseButton.Right, requireThreshold: true))
            HandleMouseDown(position, isRightButton: true);

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (IsPanning && _panButton == MouseButton.Left)
        {
            EndPan();
            e.Handled = true;
            return;
        }

        _doSectorSelection = false;
        StopDragPan();

        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton != MouseButton.Middle)
            return;

        Focus();
        BeginPan(e.GetPosition(this), MouseButton.Middle);
        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.ChangedButton != MouseButton.Middle)
            return;

        if (IsPanning && _panButton == MouseButton.Middle)
        {
            EndPan();
            e.Handled = true;
        }
    }

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);

        if (IsPanning && _panButton == MouseButton.Right)
        {
            bool shouldHandleClick = _panThresholdPending;
            var clickPosition = _lastPanPosition;

            EndPan();

            if (shouldHandleClick)
                HandleMouseDown(clickPosition, isRightButton: true);

            e.Handled = true;
            return;
        }

        _doSectorSelection = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (IsPanning)
        {
            if (!IsPanButtonPressed(e))
            {
                EndPan();
                e.Handled = true;
                return;
            }

            var position = e.GetPosition(this);

            if (_panThresholdPending)
            {
                if (!HasExceededDragThreshold(_lastPanPosition, position))
                {
                    e.Handled = true;
                    return;
                }

                _panThresholdPending = false;
                _panWarpPending = false;
                _lastPanPosition = position;

                UpdateCursor();
                e.Handled = true;
                return;
            }

            if (TryConsumePendingPanWarp(position))
            {
                e.Handled = true;
                return;
            }

            bool panned = PanBy(position.X - _lastPanPosition.X, position.Y - _lastPanPosition.Y);

            if (panned && TryWarpPanCursor(position, out var warpResult))
            {
                _lastPanPosition = warpResult.Position;
                _panWarpTarget = warpResult;
                _panWarpPending = true;
                e.Handled = true;
                return;
            }

            _lastPanPosition = position;
            e.Handled = true;
            return;
        }

        var editor = _editor;

        if (editor?.SelectedRoom is null || editor.Action is EditorActionRelocateCamera)
            return;

        if (e.LeftButton == MouseButtonState.Pressed && _doSectorSelection)
        {
            _lastDragPosition = e.GetPosition(this);
            UpdateDragSelection(_lastDragPosition);
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        _doSectorSelection = false;
        StopDragPan();
        CancelPan();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        CloseSelectionToolTip();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        Focus();

        double factor = Math.Pow(MouseWheelZoomBase, e.Delta / 120.0);
        ZoomBy(factor, e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (HandleNavigationKey(e.Key, Keyboard.Modifiers))
            e.Handled = true;
    }

    private bool HandleNavigationKey(Key key, ModifierKeys modifiers)
    {
        if (Room is null || (modifiers & (ModifierKeys.Alt | ModifierKeys.Windows)) != 0)
            return false;

        bool isControlDown = modifiers.HasFlag(ModifierKeys.Control);

        if (isControlDown)
        {
            if (IsPlusKey(key))
            {
                ZoomBy(KeyboardZoomBase, GetViewCenter());
                return true;
            }

            if (IsMinusKey(key))
            {
                ZoomBy(1.0 / KeyboardZoomBase, GetViewCenter());
                return true;
            }

            if (IsResetKey(key))
            {
                ResetView();
                return true;
            }

            return false;
        }

        double panStep = modifiers.HasFlag(ModifierKeys.Shift) ? KeyboardPanStep * 3.0 : KeyboardPanStep;

        switch (key)
        {
            case Key.Left:
                PanBy(panStep, 0.0);
                return true;

            case Key.Right:
                PanBy(-panStep, 0.0);
                return true;

            case Key.Up:
                PanBy(0.0, panStep);
                return true;

            case Key.Down:
                PanBy(0.0, -panStep);
                return true;

            case Key.PageUp:
                ZoomBy(KeyboardZoomBase, GetViewCenter());
                return true;

            case Key.PageDown:
                ZoomBy(1.0 / KeyboardZoomBase, GetViewCenter());
                return true;

            case Key.Home:
                ResetView();
                return true;

            default:
                return false;
        }
    }

    private bool BeginPan(Point position, MouseButton button, bool requireThreshold = false)
    {
        CloseSelectionToolTip();

        _doSectorSelection = false;
        _panButton = null;
        _panThresholdPending = false;
        _panWarpPending = false;

        if (!CaptureMouse())
        {
            UpdateCursor();
            return false;
        }

        _lastPanPosition = position;
        _panButton = button;
        _panThresholdPending = requireThreshold;
        UpdateCursor();
        return true;
    }

    private void EndPan()
    {
        CancelPan();

        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }

    private void CancelPan()
    {
        if (_panButton is null)
            return;

        _panButton = null;
        _panThresholdPending = false;
        _panWarpPending = false;
        UpdateCursor();
    }

    private bool IsPanButtonPressed(MouseEventArgs e)
    {
        return _panButton switch
        {
            MouseButton.Left => e.LeftButton == MouseButtonState.Pressed,
            MouseButton.Middle => e.MiddleButton == MouseButtonState.Pressed,
            MouseButton.Right => e.RightButton == MouseButtonState.Pressed,
            _ => false
        };
    }

    private bool TryConsumePendingPanWarp(Point position)
    {
        if (!_panWarpPending)
            return false;

        if (CursorWarpHelper.IsPendingWarpStale(position, _panWarpTarget, new Size(ActualWidth, ActualHeight)))
            return true;

        _panWarpPending = false;
        return false;
    }

    private bool TryWarpPanCursor(Point position, out CursorWarpResult result)
    {
        return CursorWarpHelper.TryWarp(
            this,
            position,
            allowLeftEdgeWarp: CanPanHorizontally(-1.0),
            allowRightEdgeWarp: CanPanHorizontally(1.0),
            allowTopEdgeWarp: CanPanVertically(-1.0),
            allowBottomEdgeWarp: CanPanVertically(1.0),
            out result);
    }

    private void StartDragPan(Point position)
    {
        _lastDragPosition = position;
        _dragPanTimer.Start();
    }

    private void StopDragPan()
        => _dragPanTimer.Stop();

    private void DragPanTimerTick(object? sender, EventArgs e)
    {
        if (!_doSectorSelection || !IsMouseCaptured || Mouse.LeftButton != MouseButtonState.Pressed)
        {
            _doSectorSelection = false;
            StopDragPan();

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            return;
        }

        if (PanToFollowDrag(_lastDragPosition))
            UpdateSelectionEnd(_lastDragPosition);
    }

    private void UpdateDragSelection(Point position)
    {
        PanToFollowDrag(position);
        UpdateSelectionEnd(position);
    }

    private void UpdateSelectionEnd(Point position)
    {
        var editor = _editor;

        if (editor is null)
            return;

        if (editor.SelectedRoom is null || editor.Action is EditorActionRelocateCamera)
            return;

        if (TryGetSectorFromVisualCoord(position, out var sectorPos))
            editor.SelectedSectors = new SectorSelection { Start = editor.SelectedSectors.Start, End = sectorPos };
    }

    private void UpdateCursor(bool? isCameraRelocate = null)
    {
        if (IsPanning && !_panThresholdPending)
        {
            Cursor = Cursors.SizeAll;
            return;
        }

        Cursor = (isCameraRelocate ?? _editor?.Action is EditorActionRelocateCamera) ? Cursors.Cross : Cursors.Arrow;
    }

    private static bool IsPlusKey(Key key)
        => key is Key.Add or Key.OemPlus;

    private static bool IsMinusKey(Key key)
        => key is Key.Subtract or Key.OemMinus;

    private static bool IsResetKey(Key key)
        => key is Key.D0 or Key.NumPad0;

    private static bool HasExceededDragThreshold(Point startPoint, Point currentPoint)
        => Math.Abs(currentPoint.X - startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance
        || Math.Abs(currentPoint.Y - startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;

    private void HandleMouseDown(Point position, bool isRightButton)
    {
        CloseSelectionToolTip();

        var editor = _editor;
        var room = Room;

        if (editor is null || room is null)
            return;

        if (!TryGetSectorFromVisualCoord(position, out var sectorPos))
            return;

        if (editor.Action is EditorActionRelocateCamera)
        {
            editor.MoveCameraToSector(sectorPos);
            return;
        }

        var selectedSectorObject = editor.SelectedObject as SectorBasedObjectInstance;
        bool isAltDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

        if (!isRightButton && !isAltDown)
        {
            if (selectedSectorObject is not null &&
                selectedSectorObject.Room == room &&
                selectedSectorObject.Area.Contains(sectorPos))
            {
                HandleSectorObjectClick(editor, room, selectedSectorObject);
            }
            else
            {
                editor.SelectedSectors = new SectorSelection { Start = sectorPos, End = sectorPos };

                if (selectedSectorObject is not null)
                    editor.SelectedObject = null;

                _doSectorSelection = true;
            }
        }
        else
        {
            SelectNextObjectAtSector(editor, room, sectorPos, selectedSectorObject, position);
        }
    }

    private static void HandleSectorObjectClick(Editor editor, Room room, SectorBasedObjectInstance selectedSectorObject)
    {
        if (selectedSectorObject is PortalInstance portal)
        {
            if (room.AlternateBaseRoom is not null && portal.AdjoiningRoom.Alternated)
            {
                editor.SelectRoom(portal.AdjoiningRoom.AlternateRoom);
                editor.SelectedObject = portal.FindOppositePortal(room).FindAlternatePortal(portal.AdjoiningRoom.AlternateRoom);
            }
            else
            {
                editor.SelectRoom(portal.AdjoiningRoom);
                editor.SelectedObject = portal.FindOppositePortal(room);
            }
        }
        else if (selectedSectorObject is TriggerInstance)
        {
            EditorActions.EditObject(selectedSectorObject, WPFUtils.GetWindowOwner());
        }
    }

    /// <summary>
    /// Cycles through portals and triggers that overlap the given sector.
    /// </summary>
    private void SelectNextObjectAtSector(
        Editor editor,
        Room room,
        VectorInt2 sectorPos,
        SectorBasedObjectInstance? selectedSectorObject,
        Point tooltipPosition)
    {
        var portalsInRoom = room.Portals.Cast<SectorBasedObjectInstance>();
        var triggersInRoom = room.Triggers.Cast<SectorBasedObjectInstance>();
        var relevantObjects = portalsInRoom.Concat(triggersInRoom)
            .Where(obj => obj.Area.Contains(sectorPos));

        var nextObject = relevantObjects
            .FindFirstAfterWithWrapAround(obj => obj == selectedSectorObject, obj => true);

        if (nextObject is not null)
        {
            editor.SelectedObject = nextObject;
            ShowSelectionToolTip(nextObject, tooltipPosition);
        }
    }

    private void ShowSelectionToolTip(SectorBasedObjectInstance selectedObject, Point position)
    {
        CloseSelectionToolTip();

        _selectionToolTip = new ToolTip
        {
            Content = selectedObject.ToString(),
            HorizontalOffset = position.X + 5.0,
            Placement = PlacementMode.Relative,
            PlacementTarget = this,
            StaysOpen = false,
            VerticalOffset = position.Y + 5.0
        };

        ToolTip = _selectionToolTip;
        _selectionToolTip.IsOpen = true;
    }

    private void CloseSelectionToolTip()
    {
        if (_selectionToolTip is null)
            return;

        _selectionToolTip.IsOpen = false;
        _selectionToolTip = null;
        ToolTip = null;
    }
}

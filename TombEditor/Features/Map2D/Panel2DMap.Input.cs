using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using TombEditor.Features.ContextMenus;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.WPF.Input;

#nullable disable


namespace TombEditor.Features.Map2D
{
    // Mouse, keyboard and drag-drop interaction.
    public partial class Panel2DMap
    {
        private static bool IsAltDown => Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
        private static bool IsControlDown => Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            var position = e.GetPosition(this);
            var clickPos = FromVisualCoord(position);

            if (e.ClickCount == 2)
            {
                HandleDoubleClick(e.ChangedButton, clickPos);
                e.Handled = true;
                return;
            }

            CaptureMouse();

            if (!_depthBar.MouseDown(position, e.ChangedButton, ControlSize, clickPos))
            {
                e.Handled = true;
                return;
            }

            _lastMousePosition = position;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    // Check for depth probe
                    int? currentProbeIndex = FindClosestProbe(clickPos);

                    if (IsAltDown)
                    {
                        if (currentProbeIndex.HasValue)
                        {
                            // Remove depth probe closest to mouse pointer
                            _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        }
                        else
                        {
                            // Add depth probe under mouse pointer
                            _currentlyEditedDepthProbeIndex = _depthBar.DepthProbes.Count;
                            _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                        }
                        InvalidateVisual();
                    }
                    else if (currentProbeIndex.HasValue)
                    {
                        // Depth probe normally selected
                        _currentlyEditedDepthProbeIndex = FindClosestProbe(clickPos);
                    }
                    else
                    {
                        _overallDelta = VectorInt3.Zero;
                        _roomMouseClicked = DoPicking(clickPos);
                        if (_roomMouseClicked == null)
                        {
                            _editor.SelectedRooms = new[] { _editor.SelectedRoom };
                            _selectionArea = new SelectionArea { _area = new Rectangle2(clickPos, clickPos) };
                        }
                        else
                        {
                            if (Keyboard.Modifiers != ModifierKeys.None || !_editor.SelectedRooms.Contains(_roomMouseClicked))
                            {
                                _editor.SelectRoomsAndResetCamera(BoolCombine(_editor.SelectedRooms,
                                    new[] { _roomMouseClicked }));
                            }
                            _roomMouseOffset = clickPos - _roomMouseClicked.SectorPos;
                        }
                    }
                    break;

                case MouseButton.Right:
                    _startMousePosition = position;

                    // Move view with mouse cursor
                    // Mouse cursor is a fixed point
                    _viewMoveMouseWorldCoord = clickPos;
                    break;

                case MouseButton.Middle:
                    _selectionArea = new SelectionArea { _area = new Rectangle2(clickPos, clickPos) };
                    break;

                case MouseButton.XButton1:
                    // Remove depth probe closest to mouse pointer
                    currentProbeIndex = FindClosestProbe(clickPos);
                    if (currentProbeIndex.HasValue)
                    {
                        _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        InvalidateVisual();
                    }
                    break;

                case MouseButton.XButton2:
                    // Add depth probe under mouse pointer
                    _currentlyEditedDepthProbeIndex = _depthBar.DepthProbes.Count;
                    _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                    InvalidateVisual();
                    break;
            }

            e.Handled = true;
        }

        private void HandleDoubleClick(MouseButton button, Vector2 clickPos)
        {
            switch (button)
            {
                case MouseButton.Left:
                    if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        int? currentProbeIndex = FindClosestProbe(clickPos);
                        if (currentProbeIndex.HasValue)
                            _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        else
                            _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                        InvalidateVisual();
                    }
                    break;

                case MouseButton.Middle:
                    Room clickedRoom = DoPicking(clickPos);
                    if (clickedRoom != null)
                    {
                        IEnumerable<Room> connectedRooms = BoolCombine(_editor.SelectedRooms, _editor.Level.GetConnectedRooms(clickedRoom));
                        connectedRooms = // Don't use the currently clicked room because it was already processed with the previous single click.
                            _editor.SelectedRooms.Where(room => room == clickedRoom || room == clickedRoom.AlternateOpposite)
                            .Concat(
                                connectedRooms.Where(room => room != clickedRoom && room != clickedRoom.AlternateOpposite));
                        _editor.SelectRoomsAndResetCamera(connectedRooms);
                    }
                    _selectionArea = null;
                    break;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsFocused)
                Focus(); // Enable keyboard interaction
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);

            // Update depth bar... (redraws the cursor depth column that follows the pointer)
            _depthBar.MouseMove(position, ControlSize);
            InvalidateVisual();

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_currentlyEditedDepthProbeIndex.HasValue)
                {
                    // Move depth probe around
                    _depthBar.DepthProbes[_currentlyEditedDepthProbeIndex.Value].Position = FromVisualCoord(position);
                    InvalidateVisual();
                }
                else if (_roomMouseClicked != null)
                {
                    bool copyMode = IsControlDown;

                    if (copyMode && ((Vector3)_overallDelta).Length() >= 1 * ViewScale)
                    {
                        _roomsToMove = null;
                        var dataObject = new DataObject();
                        dataObject.SetData(typeof(RoomClipboardData), new RoomClipboardData(_editor, FromVisualCoord(position)));
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                        _lastMousePosition = position;
                        return;
                    }

                    if (_roomsToMove == null)
                        _roomsToMove = _editor.Level.GetConnectedRooms(_editor.SelectedRooms.Concat(new[] { _roomMouseClicked }));

                    if (_roomsToMove != null && UpdateRoomPosition(FromVisualCoord(position) - _roomMouseOffset, _roomMouseClicked, !copyMode))
                    {
                        // Move rooms around
                        foreach (Room room in _roomsToMove)
                            _editor.RoomPositionChange(room);
                        _editor.ResetCamera();
                        InvalidateVisual();
                    }
                }
                else if (_selectionArea != null)
                    UpdateSelectionArea(position);
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                UpdateSelectionArea(position);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_viewMoveMouseWorldCoord != null)
                    if (IsControlDown)
                    { // Zoom
                        double relativeDeltaY = (position.Y - _lastMousePosition.Y) / ActualHeight;
                        ViewScale *= (float)Math.Exp(_editor.Configuration.Map2D_NavigationSpeedMouseZoom * relativeDeltaY);
                        InvalidateVisual();
                    }
                    else
                    { // Panning
                        MoveToFixedPoint(position, _viewMoveMouseWorldCoord.Value, true);
                    }
            }

            _lastMousePosition = position;
        }

        private void UpdateSelectionArea(Point position)
        {
            if (_selectionArea == null)
                return;

            _selectionArea._area.End = FromVisualCoord(position);
            _selectionArea._roomSelectionCache = null;
            InvalidateVisual();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsMouseCaptured)
                ReleaseMouseCapture();
            _depthBar.MouseUp();

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (_roomMouseClicked != null)
                    {
                        if (_roomsToMove != null)
                        {
                            _editor.UndoManager.PushRoomsMoved(_roomsToMove.ToList(), _overallDelta);
                            _roomsToMove = null;
                        }

                        _roomMouseClicked = null;
                        InvalidateVisual();
                    }
                    break;
                case MouseButton.Right:
                    var distance = new Vector2((float)_startMousePosition.X, (float)_startMousePosition.Y) - new Vector2((float)e.GetPosition(this).X, (float)e.GetPosition(this).Y);
                    if (distance.Length() < 4.0f)
                    {
                        if (_currentContextMenu != null)
                            _currentContextMenu.IsOpen = false;
                        Vector2 clickPos = FromVisualCoord(e.GetPosition(this));
                        var screenPoint = PointToScreen(e.GetPosition(this));
                        var screen = new System.Drawing.Point((int)screenPoint.X, (int)screenPoint.Y);
                        var owner = WPFUtils.GetWindowOwner();
                        if (_editor.SelectedRooms.Contains(DoPicking(clickPos)))
                            _currentContextMenu = SelectedRoomWpfContextMenu.Show(_editor, owner, clickPos, screen);
                        else
                            _currentContextMenu = Space2DMapWpfContextMenu.Show(_editor, owner, clickPos, screen);
                    }

                    _viewMoveMouseWorldCoord = null;
                    break;
            }
            _currentlyEditedDepthProbeIndex = null;
            _roomMouseClicked = null;
            _roomsToMove = null;

            if (_selectionArea != null)
            { // Change room selection
                _editor.SelectRoomsAndResetCamera(_selectionArea.GetRoomSelection(this));
                _selectionArea = null;
                InvalidateVisual();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2 fixedPointInWorld = FromVisualCoord(e.GetPosition(this));
            ViewScale *= (float)Math.Exp(e.Delta * _editor.Configuration.Map2D_NavigationSpeedMouseWheelZoom);
            MoveToFixedPoint(e.GetPosition(this), fixedPointInWorld);
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)) == ModifierKeys.None)
                EngageMovement(e.Key);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            KeyUpdateSelectionAreaPreview(e.Key);
            if (e.Key == Key.Escape)
            {
                _selectionArea = null;
                InvalidateVisual();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            StopMovement();
            KeyUpdateSelectionAreaPreview(e.Key);
        }

        private void KeyUpdateSelectionAreaPreview(Key key)
        {
            if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl)
            {
                if (_selectionArea != null)
                {
                    _selectionArea._roomSelectionCache = null;
                    InvalidateVisual();
                }
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            StopMovement();
            _insertionContourLineData = null;
        }

        private bool UpdateRoomPosition(Vector2 newRoomPos, Room roomReference, bool moveRooms)
        {
            VectorInt2 newRoomPosInt = VectorInt2.FromRounded(newRoomPos);
            VectorInt2 roomMovement = newRoomPosInt - roomReference.SectorPos;

            if (roomMovement != new VectorInt2())
            {
                if (EditorActions.CheckForLockedRooms(WPFUtils.GetWindowOwner(), _roomsToMove))
                    _roomMouseClicked = null;
                else
                {
                    var delta = new VectorInt3(roomMovement.X, 0, roomMovement.Y);

                    if (roomMovement.X != 0 || roomMovement.Y != 0)
                    {
                        _overallDelta += delta;
                        if (moveRooms)
                        {
                            EditorActions.MoveRooms(delta, _roomsToMove, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Room DoPicking(Vector2 pos)
        {
            IEnumerable<Room> roomList = _editor.Level.GetVerticallyAscendingRoomList(room =>
            {
                int roomLocalX = (int)pos.X - room.SectorPos.X;
                int roomLocalZ = (int)pos.Y - room.SectorPos.Y;
                if (roomLocalX < 1 || roomLocalZ < 1 || roomLocalX >= room.NumXSectors - 1 || roomLocalZ >= room.NumZSectors - 1)
                    return false;

                if (room.Sectors[roomLocalX, roomLocalZ].IsAnyWall)
                    return false;

                // Check if the room fits the depth bar criterion
                return _depthBar.CheckRoom(room);
            });

            return roomList.LastOrDefault();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            RoomClipboardData clipboardData = e.Data.GetData(typeof(RoomClipboardData)) as RoomClipboardData;
            if (clipboardData != null)
            {
                e.Effects = DragDropEffects.Copy;
                _insertionContourLineData = clipboardData.ContourLines;
                _insertionDropPosition = clipboardData.DropPosition;
                _insertionCurrentOffset = GetDragDropOffset(e);
                InvalidateVisual();
            }
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            if (_insertionContourLineData != null)
            {
                e.Effects = DragDropEffects.Copy;
                VectorInt2 newCurrentOffset = GetDragDropOffset(e);
                if (newCurrentOffset != _insertionCurrentOffset)
                {
                    _insertionCurrentOffset = newCurrentOffset;
                    InvalidateVisual();
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop) && EditorActions.DragDropFileSupported(e.ToPointerDragEventArgs(), true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

            if (_insertionContourLineData != null)
            {
                _insertionContourLineData = null;
                InvalidateVisual();
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            _insertionContourLineData = null;
            RoomClipboardData clipboardData = e.Data.GetData(typeof(RoomClipboardData)) as RoomClipboardData;
            if (clipboardData != null)
                clipboardData.MergeInto(_editor, GetDragDropOffset(e));
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                EditorActions.DragDropCommonFiles(e.ToPointerDragEventArgs(), WPFUtils.GetWindowOwner());
            InvalidateVisual();
        }

        private VectorInt2 GetDragDropOffset(DragEventArgs e)
        {
            var newPos = FromVisualCoord(e.GetPosition(this));
            var result = newPos - _insertionDropPosition;
            return VectorInt2.FromRounded(result);
        }
    }
}

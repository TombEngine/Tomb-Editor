using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private bool OnMouseMovedRight(Point location, bool shift)
        {
            // Don't do anything while camera is animating!
            if (_movementTimer.Animating)
                return false;

            // Warp cursor
            var delta = _editor.Configuration.Rendering3D_CursorWarping ?
                WarpMouseCursor(location, _lastMousePosition) : Delta(location, _lastMousePosition);

            if (shift)
                Camera.MoveCameraPlane(new Vector3(delta.X, delta.Y, 0) *
                    _editor.Configuration.Rendering3D_NavigationSpeedMouseTranslate);
            else if (ModifierKeys.HasFlag(Keys.Control))
                Camera.Zoom((_editor.Configuration.Rendering3D_InvertMouseZoom ? delta.Y : -delta.Y) * _editor.Configuration.Rendering3D_NavigationSpeedMouseZoom);
            else
                Camera.Rotate(
                    delta.X * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                   -delta.Y * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

            _gizmo.MouseMoved(_viewProjection, GetRay(location.X, location.Y)); // Update gizmo
            return true;
        }

        private bool OnMouseMovedLeft(Point location)
        {
            if (_gizmo.MouseMoved(_viewProjection, GetRay(location.X, location.Y)))
            {
                // Process gizmo
                return true;
            }
            else if (_editor.Tool.Tool >= EditorToolType.Drag && _toolHandler.Engaged && !_doSectorSelection)
            {
                if (_editor.Tool.Tool != EditorToolType.PortalDigger && !_toolHandler.Dragged && _toolHandler.PositionDiffers(location.X, location.Y))
                {
                    Room room = _editor.SelectedRoom;
                    RectangleInt2 area = _editor.SelectedSectors.Area;
                    HashSet<Room> affectedRooms = room.GetAdjoiningRoomsFromArea(area);
                    affectedRooms.Add(room);

                    _editor.UndoManager.PushGeometryChanged(affectedRooms);
                }

                var dragValue = _toolHandler.UpdateDragState(location.X, location.Y,
                    _editor.Tool.Tool == EditorToolType.Drag || _editor.Tool.Tool == EditorToolType.PortalDigger,
                    _editor.Tool.Tool != EditorToolType.PortalDigger);

                if (dragValue.HasValue)
                {
                    if (_editor.Tool.Tool == EditorToolType.PortalDigger)
                    {
                        VectorInt3 move = VectorInt3.Zero;
                        VectorInt2 drag = VectorInt2.Zero;
                        Point invertedDragValue = new Point(dragValue.Value.X, -dragValue.Value.Y);
                        var currRoom = _toolHandler.ReferenceRoom;
                        RectangleInt2 resizeArea = new RectangleInt2(currRoom.LocalArea.Start, currRoom.LocalArea.End);
                        int[] resizeHeight = { currRoom.GetLowestCorner(), currRoom.GetHighestCorner() };
                        PortalDirection portalDirection;
                        int verticalPrecision = ModifierKeys.HasFlag(Keys.Shift) ? _editor.IncrementReference : _editor.IncrementReference * 4;

                        if (_toolHandler.ReferencePicking.IsVerticalPlane)
                            portalDirection = PortalInstance.GetOppositeDirection
                                             (PortalInstance.GetDirection
                                             (_toolHandler.ReferencePicking.Face.GetDirection()));
                        else
                        {
                            portalDirection = _toolHandler.ReferencePicking.BelongsToFloor ? PortalDirection.Floor : PortalDirection.Ceiling;
                            move = new VectorInt3(0, dragValue.Value.Y * verticalPrecision, 0);
                        }

                        switch (portalDirection)
                        {
                            case PortalDirection.Floor:
                            case PortalDirection.Ceiling:
                                var newHeight = -dragValue.Value.Y * verticalPrecision;
                                if (resizeHeight[1] - resizeHeight[0] + (portalDirection == PortalDirection.Floor ? newHeight : -newHeight) <= 0)
                                    return false;  // Limit inward dragging

                                resizeHeight[0] = portalDirection == PortalDirection.Floor ? 0 : newHeight;
                                resizeHeight[1] = portalDirection == PortalDirection.Floor ? newHeight : 0;
                                break;

                            case PortalDirection.WallNegativeX:
                            case PortalDirection.WallPositiveX:
                                drag = new VectorInt2(TranslateCameraMouseMovement(invertedDragValue, true), 0);
                                if (portalDirection == PortalDirection.WallNegativeX)
                                    resizeArea.Start += drag;
                                else
                                    resizeArea.End += drag;
                                break;

                            case PortalDirection.WallNegativeZ:
                            case PortalDirection.WallPositiveZ:
                                drag = new VectorInt2(0, TranslateCameraMouseMovement(invertedDragValue));
                                if (portalDirection == PortalDirection.WallNegativeZ)
                                    resizeArea.Start += drag;
                                else
                                    resizeArea.End += drag;
                                break;
                        }

                        // Only resize if any dimension is bigger than 3 and less than 32
                        if (resizeArea.Size.X > 1 && resizeArea.Size.Y > 1 && resizeArea.Size.X < 32 && resizeArea.Size.Y < 32)
                        {
                            bool? operateOnFloor = null;
                            if (_toolHandler.ReferencePicking.IsVerticalPlane) operateOnFloor = true;
                            currRoom.Resize(_editor.Level, resizeArea, resizeHeight[0], resizeHeight[1], operateOnFloor);
                            EditorActions.MoveRooms(move, currRoom.Versions, true);
                            if (_toolHandler.ReferenceRoom == _editor.SelectedRoom)
                                _editor.HighlightedSectors = new SectorSelection() { Area = _toolHandler.ReferenceRoom.LocalArea };
                        }
                    }
                    else if (_editor.SelectedSectors.Valid)
                    {
                        SectorVerticalPart splitToEdit;

                        if (_toolHandler.ReferencePicking.BelongsToFloor)
                        {
                            if (ModifierKeys.HasFlag(Keys.Control))
                                splitToEdit = SectorVerticalPart.Floor2;
                            else if (_editor.HighlightedSplit <= 1)
                                splitToEdit = SectorVerticalPart.QA;
                            else
                                splitToEdit = SectorVerticalPartExtensions.GetExtraFloorSplit(_editor.HighlightedSplit - 2);
                        }
                        else
                        {
                            if (ModifierKeys.HasFlag(Keys.Control))
                                splitToEdit = SectorVerticalPart.Ceiling2;
                            else if (_editor.HighlightedSplit <= 1)
                                splitToEdit = SectorVerticalPart.WS;
                            else
                                splitToEdit = SectorVerticalPartExtensions.GetExtraCeilingSplit(_editor.HighlightedSplit - 2);
                        }

                        switch (_editor.Tool.Tool)
                        {
                            case EditorToolType.Drag:
                                EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                    _editor.SelectedSectors.Area,
                                    _editor.SelectedSectors.Arrow,
                                    splitToEdit,
                                    Math.Sign(dragValue.Value.Y) * _editor.IncrementReference,
                                    ModifierKeys.HasFlag(Keys.Alt),
                                    _toolHandler.ReferenceIsOppositeDiagonalStep, true, true, true);
                                break;
                            case EditorToolType.Terrain:
                                _toolHandler.DiscardEditedGeometry();
                                EditorActions.ApplyHeightmap(_editor.SelectedRoom,
                                    _editor.SelectedSectors.Area,
                                    _editor.SelectedSectors.Arrow,
                                    splitToEdit,
                                    _toolHandler.RandomHeightMap,
                                    dragValue.Value.Y,
                                    ModifierKeys.HasFlag(Keys.Shift),
                                    ModifierKeys.HasFlag(Keys.Alt));
                                break;
                            default:
                                _toolHandler.DiscardEditedGeometry();
                                EditorActions.ShapeGroup(_editor.SelectedRoom,
                                    _editor.SelectedSectors.Area,
                                    _editor.SelectedSectors.Arrow,
                                    _editor.Tool.Tool,
                                    splitToEdit,
                                    dragValue.Value.Y,
                                    ModifierKeys.HasFlag(Keys.Shift),
                                    ModifierKeys.HasFlag(Keys.Alt));
                                break;
                        }
                    }

                    return true;
                }
            }
            else
            {
                if (HandleBrushMouseMove(location))
                    return true;

                var newSectorPicking = DoPicking(GetRay(location.X, location.Y)) as PickingResultSector;

                if (newSectorPicking != null)
                {
                    VectorInt2 pos = newSectorPicking.Pos;
                    bool belongsToFloor = newSectorPicking.BelongsToFloor;

                    if ((_editor.Tool.Tool == EditorToolType.Selection || _editor.Tool.Tool == EditorToolType.Group || _editor.Tool.Tool >= EditorToolType.Drag) && _doSectorSelection)
                    {
                        var objectSelectionMode = _editor.Tool.Tool == EditorToolType.Selection && _toolHandler.Engaged;

                        var newArea = new SectorSelection
                        {
                            Start = objectSelectionMode ? _editor.HighlightedSectors.Start : _editor.SelectedSectors.Start,
                            End = new VectorInt2(pos.X, pos.Y)
                        };

                        if (objectSelectionMode && _editor.HighlightedSectors != newArea)
                        {
                            _editor.HighlightedSectors = newArea;
                            return true;
                        }
                        else if (!objectSelectionMode && _editor.SelectedSectors != newArea)
                        {
                            _editor.SelectedSectors = newArea;
                            return true;
                        }
                    }
                    else if (_editor.Mode == EditorMode.Geometry && _toolHandler.Engaged && !ModifierKeys.HasFlag(Keys.Alt | Keys.Shift))
                    {
                        if (!ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift) && _toolHandler.Process(pos.X, pos.Y))
                        {
                            if (_editor.SelectedRoom.Sectors[pos.X, pos.Y].IsAnyWall == _toolHandler.ReferenceSector.IsAnyWall)
                            {
                                switch (_editor.Tool.Tool)
                                {
                                    case EditorToolType.Flatten:
                                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                        {
                                            if (belongsToFloor && _toolHandler.ReferencePicking.BelongsToFloor)
                                            {
                                                _editor.SelectedRoom.Sectors[pos.X, pos.Y].Floor.SetHeight(edge, _toolHandler.ReferenceSector.Floor.Min);
                                                _editor.SelectedRoom.Sectors[pos.X, pos.Y].ExtraFloorSplits.Clear();

                                                for (int i = 0; i < _toolHandler.ReferenceSector.ExtraFloorSplits.Count; i++)
                                                    _editor.SelectedRoom.Sectors[pos.X, pos.Y].ExtraFloorSplits.Add(new SectorSplit(_toolHandler.ReferenceSector.ExtraFloorSplits[i].Min));
                                            }
                                            else if (!belongsToFloor && !_toolHandler.ReferencePicking.BelongsToFloor)
                                            {
                                                _editor.SelectedRoom.Sectors[pos.X, pos.Y].Ceiling.SetHeight(edge, _toolHandler.ReferenceSector.Ceiling.Min);
                                                _editor.SelectedRoom.Sectors[pos.X, pos.Y].ExtraCeilingSplits.Clear();

                                                for (int i = 0; i < _editor.SelectedRoom.Sectors[pos.X, pos.Y].ExtraCeilingSplits.Count; i++)
                                                    _editor.SelectedRoom.Sectors[pos.X, pos.Y].ExtraCeilingSplits.Add(new SectorSplit(_toolHandler.ReferenceSector.ExtraCeilingSplits[i].Min));
                                            }
                                        }
                                        EditorActions.SmartBuildGeometry(_editor.SelectedRoom, new RectangleInt2(pos, pos));
                                        break;

                                    case EditorToolType.Smooth:
                                        if (belongsToFloor != _toolHandler.ReferencePicking.BelongsToFloor)
                                            break;

                                        EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, belongsToFloor ? SectorVerticalPart.QA : SectorVerticalPart.WS, _editor.IncrementReference, true);
                                        break;

                                    case EditorToolType.Drag:
                                    case EditorToolType.Terrain:
                                        break;

                                    default:
                                        if (belongsToFloor != _toolHandler.ReferencePicking.BelongsToFloor)
                                            break;

                                        int increment =
                                            (_editor.Tool.Tool == EditorToolType.Shovel || (_editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control))) ^ belongsToFloor
                                            ? _editor.IncrementReference : -_editor.IncrementReference;

                                        EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                            new RectangleInt2(pos, pos),
                                            ArrowType.EntireFace,
                                            belongsToFloor ? SectorVerticalPart.QA : SectorVerticalPart.WS,
                                            increment,
                                            _editor.Tool.Tool is EditorToolType.Brush or EditorToolType.Shovel,
                                            false, false, true, true);
                                        break;
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // Disable texturing in lighting mode, if option is set
                        if (_editor.Mode == EditorMode.Lighting &&
                            !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                        {
                            return false;
                        }
                        else if ((_editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.Lighting) && _editor.Action == null && ModifierKeys == Keys.None && !_objectPlaced)
                        {
                            if (_editor.Tool.Tool == EditorToolType.Brush && _toolHandler.Engaged)
                            {
                                if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos) ||
                                    _editor.SelectedSectors.Empty)
                                    return EditorActions.ApplyTexture(_editor.SelectedRoom, pos, newSectorPicking.Face, _editor.SelectedTexture, true);
                            }
                            else if (_editor.Tool.Tool == EditorToolType.GridPaint && _toolHandler.Engaged)
                            {
                                int factor = 2;
                                if (_editor.Tool.GridSize == PaintGridSize.Grid3x3) factor = 3;
                                if (_editor.Tool.GridSize == PaintGridSize.Grid4x4) factor = 4;

                                var point = new VectorInt2();

                                point.X = _toolHandler.ReferencePicking.Pos.X + (int)Math.Floor((float)(pos.X - _toolHandler.ReferencePicking.Pos.X) / factor) * factor;
                                point.Y = _toolHandler.ReferencePicking.Pos.Y + (int)Math.Floor((float)(pos.Y - _toolHandler.ReferencePicking.Pos.Y) / factor) * factor;

                                var newSelection = new SectorSelection { Start = point, End = point + VectorInt2.One * (factor - 1) };

                                if (_editor.HighlightedSectors != newSelection)
                                {
                                    _editor.HighlightedSectors = newSelection;
                                    EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                        _editor.HighlightedSectors,
                                        _editor.SelectedSectors,
                                        _editor.SelectedTexture,
                                        _toolHandler.ReferencePicking.Face,
                                        ModifierKeys.HasFlag(Keys.Control),
                                        !ModifierKeys.HasFlag(Keys.Shift),
                                        true);

                                    return true;
                                }
                            }
                        }
                    }
                }
                else if (_dragObjectPicked && !_dragObjectMoved && _editor.SelectedObject != null)
                {
                    // Do drag-n-drop tasks, if any
                    Update();
                    DoDragDrop(_editor.SelectedObject, DragDropEffects.Copy);
                    return true;
                }
            }

            return false;
        }

        private bool OnMouseMovedNone(Point location)
        {
            HandleBrushMouseMove(location);

            if (_editor.Tool.Tool != EditorToolType.GridPaint)
                return false;

            // Disable highlight in lighting mode, if option is set
            if (_editor.Mode == EditorMode.Lighting &&
                !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                return false;

            int addToSelection = 1;
            if (_editor.Tool.GridSize == PaintGridSize.Grid3x3) addToSelection = 2;
            if (_editor.Tool.GridSize == PaintGridSize.Grid4x4) addToSelection = 3;

            PickingResultSector newSectorPicking = DoPicking(GetRay(location.X, location.Y)) as PickingResultSector;
            if (newSectorPicking != null)
            {
                VectorInt2 pos = newSectorPicking.Pos;
                var newSelection = new SectorSelection
                {
                    Start = new VectorInt2(pos.X, pos.Y),
                    End = new VectorInt2(pos.X, pos.Y) + VectorInt2.One * addToSelection
                };

                if (_editor.HighlightedSectors != newSelection)
                {
                    _editor.HighlightedSectors = newSelection;
                    return true;
                }
            }
            else if (_editor.HighlightedSectors != SectorSelection.None)
            {
                _editor.HighlightedSectors = SectorSelection.None;
            }

            return false;
        }
    }
}

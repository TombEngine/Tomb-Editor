#nullable enable

using Keys = TombLib.Input.EditorKey;
using System.Drawing;
using System.Numerics;
using TombEditor.Features.ContextMenus;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void OnMouseButtonUpLeft(Point location)
        {
            if (_editor.Mode == EditorMode.Geometry && !_gizmoEnabled && !_objectPlaced)
            {
                var newSectorPicking = DoPicking(GetRay(location.X, location.Y)) as PickingResultSector;
                if (newSectorPicking != null && !_toolHandler.Dragged)
                {
                    var pos = newSectorPicking.Pos;
                    var zone = _editor.SelectedSectors.Empty ? new RectangleInt2(pos, pos) : _editor.SelectedSectors.Area;
                    bool belongsToFloor = newSectorPicking.BelongsToFloor;

                    if (ModifierKeys.HasFlag(Keys.Alt) && zone.Contains(pos))
                    {
                        // Split the faces
                        if (belongsToFloor)
                            EditorActions.FlipFloorSplit(_editor.SelectedRoom, zone);
                        else
                            EditorActions.FlipCeilingSplit(_editor.SelectedRoom, zone);
                        return;
                    }
                    else if (ModifierKeys.HasFlag(Keys.Shift) && zone.Contains(pos))
                    {
                        // Rotate sector
                        EditorActions.RotateSectors(_editor.SelectedRoom, zone, belongsToFloor);
                        return;
                    }
                    else if (_editor.Tool.Tool == EditorToolType.Selection ||
                            (_editor.Tool.Tool >= EditorToolType.Drag && _editor.Tool.Tool < EditorToolType.PortalDigger))
                    {
                        if (!_doSectorSelection && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                        {
                            // Rotate the arrows
                            if (ModifierKeys.HasFlag(Keys.Control))
                            {
                                if (_editor.SelectedSectors.Arrow == ArrowType.CornerSW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerSE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSW);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSE);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNE);
                                else
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNW);
                            }
                            else
                            {
                                if (_editor.SelectedSectors.Arrow == ArrowType.EdgeW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeS)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeW);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeS);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeN)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeE);
                                else
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeN);
                            }
                        }
                    }
                }
            }

            // Handle gizmo manipulation with ghostblock (to update room properties in 2D grid)
            if (_gizmoEnabled && _editor.SelectedObject is GhostBlockInstance)
                _editor.RoomSectorPropertiesChange(_editor.SelectedRoom);

            // Handle multiple object selection
            if (_editor.Tool.Tool == EditorToolType.Selection && _toolHandler.Engaged && ModifierKeys.HasFlag(Keys.Control))
                EditorActions.SelectObjectsInArea(this, _editor.HighlightedSectors, false);
        }

        private void OnMouseButtonUpRight(Point location)
        {
            var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(location.X, location.Y);
            if (distance.Length() < 4.0f)
            {
                if (_currentWpfContextMenu != null)
                {
                    _currentWpfContextMenu.IsOpen = false;
                    _currentWpfContextMenu = null;
                }

                PickingResult newPicking = DoPicking(GetRay(location.X, location.Y), true);
                var screen = PointToScreen(location);
                if (newPicking is PickingResultObject)
                {
                    ObjectInstance target = ((PickingResultObject)newPicking).ObjectInstance;
                    if (target is ISpatial)
                        _currentWpfContextMenu = MaterialObjectWpfContextMenu.Show(_editor, this, target, screen);
                }
                else if (newPicking is PickingResultSector pickedSector)
                {
                    if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pickedSector.Pos))
                        _currentWpfContextMenu = SelectedGeometryWpfContextMenu.Show(
                            _editor, this, pickedSector.Room, _editor.SelectedSectors.Area, pickedSector.Pos, screen);
                    else
                        _currentWpfContextMenu = SectorWpfContextMenu.Show(
                            _editor, this, pickedSector.Room, pickedSector.Pos, screen);
                }
            }
        }
    }
}

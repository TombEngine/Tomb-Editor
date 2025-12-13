using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void OnMouseButtonUp(MouseButtons button, Point location)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    OnMouseButtonUpLeft(location);
                    break;

                case MouseButtons.Right:
                    OnMouseButtonUpRight(location);
                    break;
            }

            // Click outside room
            if (_noSelectionConfirm)
            {
                _editor.SelectedSectors = SectorSelection.None;
                _editor.SelectedObject = null;

                // It gets already set on MouseMove, but it's better
                // to prevent obscure errors and unwanted behavior later on.

                _noSelectionConfirm = false;    
            }

            _toolHandler.Disengage();
            _doSectorSelection = false;
            _gizmoEnabled = false;
            _dragObjectMoved = false;
            _dragObjectPicked = false;

            if (_gizmo.MouseUp())
                Invalidate();

            Capture = false;
            Invalidate();
        }

        private void OnMouseButtonDown(MouseButtons button, Point location)
        {
            if (_editor.FlyMode)
                return; // Selecting in FlyMode is not allowed

            _lastMousePosition = location;
            _doSectorSelection = false;
            _objectPlaced = false;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            switch (button)
            {
                case MouseButtons.Left:
                    OnMouseButtonDownLeft(location);
                    break;

                case MouseButtons.Right:
                    OnMouseButtonDownRight(location);
                    break;
            }
        }

        private void OnMouseMoved(MouseButtons button, Point location)
        {
            if (_editor.FlyMode)
                return;

            // Reset internal bool for deselection
            var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(location.X, location.Y);
            if (distance.Length() > 8.0f)
                _noSelectionConfirm = false;

            bool redrawWindow = false;

            switch (button)
            {
                case MouseButtons.Left:
                    redrawWindow = OnMouseMovedLeft(location);
                    break;

                case MouseButtons.Right:
                    redrawWindow = OnMouseMovedRight(location, ModifierKeys.HasFlag(Keys.Shift));
                    break;

                case MouseButtons.Middle:
                    redrawWindow = OnMouseMovedRight(location, true);
                    break;

                default:
                    redrawWindow = OnMouseMovedNone(location);
                    break;
            }

            if (!redrawWindow)
            {
                // Hover effect on gizmo
                redrawWindow = _gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(location.X, location.Y)));
            }

            if (redrawWindow)
            {
                Invalidate();
                Update(); // Magic fix for gizmo stiffness!
            }

            _lastMousePosition = location;
        }

        private void OnMouseDoubleClicked(MouseButtons button, Point location)
        {
            _objectPlaced = false;

            switch (button)
            {
                case MouseButtons.Left:
                    OnMouseDoubleClickedLeft(location);
                    break;

                case MouseButtons.Right:
                    OnMouseDoubleClickedRight(location);
                    break;
            }
        }

        private void OnMouseWheelScroll(int delta)
        {
            if (!_movementTimer.Animating)
            {
                Console.WriteLine("Delta: " + delta);
                Camera.Zoom(-delta * _editor.Configuration.Rendering3D_NavigationSpeedMouseWheelZoom);
                Invalidate();
            }
        }

        private void OnMouseEntered()
        {
            if (!Focused && Form.ActiveForm == FindForm())
            {
                Focus(); // Enable keyboard interaction
                _editor.ToggleHiddenSelection(false); // Restore hidden selection, if any
            }
        }

        private void OnMouseDragAndDrop(DragEventArgs e)
        {
            // Check if we are done with all common file tasks
            var filesToProcess = EditorActions.DragDropCommonFiles(e, FindForm());

            if (filesToProcess == 0)
                return;

            // Now try to put data on pointed sector
            Point loc = PointToClient(new Point(e.X, e.Y));
            PickingResult newPicking = DoPicking(GetRay(loc.X, loc.Y), _editor.Configuration.Rendering3D_AutoswitchCurrentRoom);

            if (newPicking is PickingResultSector)
            {
                var newSectorPicking = (PickingResultSector)newPicking;

                // Switch room if needed
                if (newSectorPicking.Room != _editor.SelectedRoom)
                    _editor.SelectedRoom = newSectorPicking.Room;

                var obj = e.Data.GetData(e.Data.GetFormats()[0]) as IWadObject;

                if (obj != null)
                {
                    PositionBasedObjectInstance instance = null;

                    if (obj is ImportedGeometry)
                        instance = new ImportedGeometryInstance { Model = (ImportedGeometry)obj };
                    else if (obj is WadMoveable)
                        instance = ItemInstance.FromItemType(new ItemType(((WadMoveable)obj).Id, _editor?.Level?.Settings));
                    else if (obj is WadStatic)
                        instance = ItemInstance.FromItemType(new ItemType(((WadStatic)obj).Id, _editor?.Level?.Settings));

                    // Put item from object browser
                    if (instance != null)
                        EditorActions.PlaceObject(_editor.SelectedRoom, newSectorPicking.Pos, instance);
                }
                else if (filesToProcess != -1)
                {
                    // Try to put custom geometry files, if any
                    List<string> files = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();

                    foreach (var file in files)
                    {
                        if (!BaseGeometryImporter.FileExtensions.Matches(file))
                            continue;

                        if (!file.IsANSI())
                        {
                            MessageBoxes.NonANSIFilePathError(FindForm());
                            continue;
                        }

                        EditorActions.AddAndPlaceImportedGeometry(this, newSectorPicking.Pos, file);
                    }
                }
            }
        }

        private void OnMouseDragEntered(DragEventArgs e)
        {
            if (e.Data.GetData(e.Data.GetFormats()[0]) as IWadObject != null)
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(DarkFloatingToolboxContainer)))
                e.Effect = DragDropEffects.Move;
            else if (EditorActions.DragDropFileSupported(e, true))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
    }
}

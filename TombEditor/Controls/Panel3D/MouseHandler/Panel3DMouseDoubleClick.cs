#nullable enable

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void OnMouseDoubleClickedLeft(Point location)
        {
            PickingResult newPicking = DoPicking(GetRay(location.X, location.Y), true);
            if (newPicking is PickingResultObject)
            {
                if (ModifierKeys == Keys.None)
                {
                    var pickedObject = ((PickingResultObject)newPicking).ObjectInstance;
                    EditorActions.EditObject(pickedObject, this);
                }
            }
            else if (newPicking is PickingResultSector)
            {
                var sector = (PickingResultSector)newPicking;
                Room pickedRoom = sector.Room;
                if (pickedRoom != _editor.SelectedRoom)
                {
                    if (ModifierKeys == Keys.Shift)
                    {
                        List<Room> newlySelectedRooms = _editor.SelectedRooms.ToList();
                        if (newlySelectedRooms.Contains(pickedRoom))
                            newlySelectedRooms.Remove(pickedRoom);
                        else
                            newlySelectedRooms.Add(pickedRoom);

                        _editor.SelectRooms(newlySelectedRooms);

                    }
                    else
                    {
                        _editor.SelectedRoom = pickedRoom;
                        if (_editor.Configuration.Rendering3D_AnimateCameraOnDoubleClickRoomSwitch && ModifierKeys == Keys.None)
                        {
                            Vector3 center = sector.Room.GetLocalCenter();
                            var nextPos = new Vector3(sector.Pos.X * Level.SectorSizeUnit + Level.HalfSectorSizeUnit, center.Y, sector.Pos.Y * Level.SectorSizeUnit + Level.HalfSectorSizeUnit) + sector.Room.WorldPos;
                            AnimateCamera(nextPos);
                        }
                    }

                }
            }
        }

        private void OnMouseDoubleClickedRight(Point location)
        {
            _editor.ResetCamera();
        }
    }
}

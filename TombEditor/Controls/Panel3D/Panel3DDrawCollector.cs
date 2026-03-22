using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private List<Room> CollectRoomsToDraw(Room baseRoom)
        {
            List<Room> result = new List<Room>();

            bool isFlipped = baseRoom.Alternated && baseRoom.AlternateBaseRoom != null;

            if (ShowAllRooms)
            {
                foreach (var room in _editor.Level.Rooms)
                {
                    if (room == null)
                        continue;

                    if (isFlipped)
                    {
                        if (!room.Alternated)
                        {
                            result.Add(room);
                        }
                        else
                        {
                            if (room.AlternateRoom != null)
                            {
                                result.Add(room.AlternateRoom);
                            }
                            else
                            {
                                result.Add(room);
                            }
                        }
                    }
                    else
                    {
                        if (!room.Alternated || room.Alternated && room.AlternateBaseRoom == null)
                        {
                            result.Add(room);
                        }
                    }
                }

                return result;
            }
            else if (!ShowPortals)
                return new List<Room>(new[] { baseRoom });

            // New iterative version of the function
            Vector3 cameraPosition = Camera.GetPosition();
            Stack<Room> stackRooms = new Stack<Room>();
            Stack<int> stackLimits = new Stack<int>();
            HashSet<Room> visitedRooms = new HashSet<Room>();

            stackRooms.Push(baseRoom);
            stackLimits.Push(0);

            while (stackRooms.Count > 0)
            {
                var theRoom = stackRooms.Pop();
                int theLimit = stackLimits.Pop();

                if (theLimit > _editor.Configuration.Rendering3D_DrawRoomsMaxDepth)
                    continue;

                if (isFlipped)
                {
                    if (!theRoom.Alternated)
                    {
                        visitedRooms.Add(theRoom);
                        if (!result.Contains(theRoom))
                            result.Add(theRoom);
                    }
                    else
                    {
                        if (theRoom.AlternateRoom != null)
                        {
                            visitedRooms.Add(theRoom);
                            if (!result.Contains(theRoom.AlternateRoom))
                                result.Add(theRoom.AlternateRoom);
                        }
                        else
                        {
                            visitedRooms.Add(theRoom);
                            if (!result.Contains(theRoom))
                                result.Add(theRoom);
                        }
                    }
                }
                else
                {
                    if (!theRoom.Alternated || theRoom.Alternated && theRoom.AlternateBaseRoom == null)
                    {
                        visitedRooms.Add(theRoom);
                        if (!result.Contains(theRoom))
                            result.Add(theRoom);
                    }
                }

                foreach (var portal in theRoom.Portals)
                {
                    Vector3 normal = Vector3.Zero;

                    if (portal.Direction == PortalDirection.WallPositiveZ)
                        normal = -Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.WallPositiveX)
                        normal = -Vector3.UnitX;
                    if (portal.Direction == PortalDirection.WallNegativeZ)
                        normal = Vector3.UnitZ;
                    if (portal.Direction == PortalDirection.WallNegativeX)
                        normal = Vector3.UnitX;
                    if (portal.Direction == PortalDirection.Floor)
                        normal = Vector3.UnitY;
                    if (portal.Direction == PortalDirection.Ceiling)
                        normal = -Vector3.UnitY;

                    Vector3 cameraDirection = cameraPosition - Camera.Target;

                    if (Vector3.Dot(normal, cameraDirection) < -0.1f && theLimit > 1)
                        continue;

                    if (!visitedRooms.Contains(portal.AdjoiningRoom) &&
                        !stackRooms.Contains(portal.AdjoiningRoom))
                    {
                        stackRooms.Push(portal.AdjoiningRoom);
                        stackLimits.Push(theLimit + 1);
                    }
                }
            }
            return result;
        }

        Room[] CollectRoomsToDraw()
        {
            // Collect rooms to draw
            var camPos = Camera.GetPosition();
            var roomsToDraw = CollectRoomsToDraw(_editor.SelectedRoom).ToArray();

            if (_roomsDistanceCache == null || _roomsDistanceCache.Length < roomsToDraw.Length)
                _roomsDistanceCache = new float[roomsToDraw.Length];

            for (int i = 0; i < roomsToDraw.Length; ++i)
                _roomsDistanceCache[i] = Vector3.DistanceSquared(camPos, roomsToDraw[i].WorldPos + roomsToDraw[i].GetLocalCenter());

            Array.Sort(_roomsDistanceCache, roomsToDraw, 0, roomsToDraw.Length);
            Array.Reverse(roomsToDraw);

            return roomsToDraw;
        }

        List<MoveableInstance> CollectMoveablesToDraw(Room[] roomsToDraw)
        {
            var moveablesToDraw = new List<MoveableInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
                moveablesToDraw.AddRange(roomsToDraw[i].Objects.OfType<MoveableInstance>());
            return moveablesToDraw;
        }

        List<StaticInstance> CollectStaticsToDraw(Room[] roomsToDraw)
        {
            var staticsToDraw = new List<StaticInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
                staticsToDraw.AddRange(roomsToDraw[i].Objects.OfType<StaticInstance>());
            return staticsToDraw;
        }

        List<ImportedGeometryInstance> CollectImportedGeometryToDraw(Room[] roomsToDraw)
        {
            var importedGeometryToDraw = new List<ImportedGeometryInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
                importedGeometryToDraw.AddRange(roomsToDraw[i].Objects.OfType<ImportedGeometryInstance>().Where(ig => ig.Model?.DirectXModel != null));
            return importedGeometryToDraw;
        }

        List<VolumeInstance> CollectVolumesToDraw(Room[] roomsToDraw)
        {
            var volumesToDraw = new List<VolumeInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
                volumesToDraw.AddRange(roomsToDraw[i].Objects.OfType<VolumeInstance>());
            return volumesToDraw.OrderBy(v => v.Shape()).ToList();
        }

        List<GhostBlockInstance> CollectGhostBlocksToDraw(Room[] roomsToDraw)
        {
            var ghostBlocksToDraw = new List<GhostBlockInstance>();
            for (int i = 0; i < roomsToDraw.Length; i++)
                ghostBlocksToDraw.AddRange(roomsToDraw[i].GhostBlocks);
            return ghostBlocksToDraw;
        }
    }
}

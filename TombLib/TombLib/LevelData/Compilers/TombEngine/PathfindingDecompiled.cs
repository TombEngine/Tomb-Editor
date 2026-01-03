using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public class dec_TombEngine_box_aux
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public int TrueFloor;
        public int OverlapIndex;
        public bool IsolatedBox;
        public bool NotWalkableBox;
        public bool Monkey;
        public bool Jump;
        public Room Room;
        public bool Water;
        public bool Flag0x04;
        public bool Flag0x02;
        public bool Slope;
    }

    public sealed partial class LevelCompilerTombEngine
    {
        private bool dec_graybox;
        private bool dec_water = true;
        private bool dec_monkey;
        private bool dec_flipped;
        private bool dec_jump;
        private Room dec_currentRoom;
        private int dec_q0 = Clicks.ToWorld(-1);
        private int dec_q1 = Clicks.ToWorld(-1);
        private int dec_q2 = Clicks.ToWorld(-1);
        private int dec_q3 = Clicks.ToWorld(-1);
        private List<dec_TombEngine_box_aux> dec_boxes;
        private List<TombEngineOverlap> dec_overlaps;
        private bool dec_boxExtendsInAnotherRoom;

        private const int _noHeight = int.MinValue + byte.MaxValue;

        private void Dec_BuildBoxesAndOverlaps()
        {
            dec_currentRoom = _level.Rooms[0];
            dec_boxes = new List<dec_TombEngine_box_aux>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int flipped = 0; flipped < 2; flipped++)
            {
                for (int i = 0; i < _level.Rooms.Length; i++)
                {
                    Room room = _level.Rooms[i];

                    // Room must be defined and also must be base room or the flipped version
                    if (room != null && (flipped == 0 && room.AlternateBaseRoom == null || flipped == 1 && room.AlternateBaseRoom != null))
                    {
                        TombEngineRoom tempRoom = _tempRooms[room];
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                int boxIndex = -1;

                                if (!room.Properties.FlagExcludeFromPathFinding)
                                {
                                    dec_TombEngine_box_aux box = new dec_TombEngine_box_aux();

                                    // First create the box...
                                    if (x != 0 &&
                                        z != 0 &&
                                        x != room.NumXSectors - 1 &&
                                        z != room.NumZSectors - 1 &&
                                        Dec_CreateNewBox(box, x, z, room))
                                    {
                                        // ...then try to add it to the box array
                                        boxIndex = Dec_AddBox(box);
                                        if (boxIndex < 0) return;
                                    }
                                }

                                // Assign the box index to the sector
                                tempRoom.Sectors[tempRoom.NumZSectors * x + z].BoxIndex = boxIndex;
                            }
                        }

                        _tempRooms[room] = tempRoom;
                    }
                }

                // Originally a FlipAllRooms() function was called. This function swap all base rooms with flipped room.
                // I've decided to simply set this global variable to true and, in each case, take the correct room.
                dec_flipped = true;
            }

            dec_flipped = false;

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build boxes: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_boxes.Count);

            watch.Restart();

            // Build the overlaps
            Dec_BuildOverlaps();

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build overlaps: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_overlaps.Count);
        }

        private bool Dec_BuildOverlaps()
        {
            dec_overlaps = new List<TombEngineOverlap>();

            int i = 0;
            int j = 0;

            // All boxes beloging to non flipped rooms have both flags set so they can be linked with
            // boxes belonging to both rooms of normal-alternate pair.
            // Instead box of rooms that are member of a normal-alternate pair have just only one flag set,
            // 0x04 is for normal rooms and 0x02 is for flipped rooms.
            // This is an hack, because probably both flags were set in this case with the FlipAllRooms() function
            // That I could not implement because the way Tomb Editor is designed.
            for (int k = 0; k < dec_boxes.Count; k++)
            {
                if (!_tempRooms[dec_boxes[k].Room].Flipped)
                {
                    dec_boxes[k].Flag0x04 = true;
                    dec_boxes[k].Flag0x02 = true;
                }
            }

            do
            {
                var box1 = dec_boxes[i];
                dec_boxes[i].OverlapIndex = -1;

                int numOverlapsAdded = 0;

                if (box1.Flag0x04)
                {
                    if (dec_flipped)
                    {
                        dec_flipped = false;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)
                        {
                            var box2 = dec_boxes[j];

                            if (box2.Flag0x04)
                            {
                                if (Dec_BoxesOverlap(box1, box2))
                                {
                                    if (dec_boxes[i].OverlapIndex == -1)
                                        dec_boxes[i].OverlapIndex = dec_overlaps.Count;

                                    var overlap = new TombEngineOverlap
                                    {
                                        Box = j
                                    };

                                    if (dec_jump)
                                        overlap.Flags |= 0x800;
                                    if (dec_monkey)
                                        overlap.Flags |= 0x2000;

                                    dec_overlaps.Add(overlap);
                                    numOverlapsAdded++;
                                }
                            }
                        }

                        j++;
                    }
                    while (j < dec_boxes.Count);
                }

                if (box1.Flag0x02)
                {
                    if (!dec_flipped)
                    {
                        dec_flipped = true;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)
                        {
                            var box2 = dec_boxes[j];

                            if (box2.Flag0x02)
                            {
                                if (!(box1.Flag0x04 && box2.Flag0x04))
                                {
                                    if (Dec_BoxesOverlap(box1, box2))
                                    {
                                        if (dec_boxes[i].OverlapIndex == -1)
                                            dec_boxes[i].OverlapIndex = dec_overlaps.Count;

                                        var overlap = new TombEngineOverlap
                                        {
                                            Box = j
                                        };

                                        if (dec_jump)
                                            overlap.Flags |= 0x800;
                                        if (dec_monkey)
                                            overlap.Flags |= 0x2000;

                                        dec_overlaps.Add(overlap);
                                        numOverlapsAdded++;
                                    }
                                }
                            }
                        }

                        j++;
                    }
                    while (j < dec_boxes.Count);
                }

                i++;

                if (numOverlapsAdded != 0)
                    dec_overlaps[dec_overlaps.Count - 1].Flags |= 0x8000;
            }
            while (i < dec_boxes.Count);

            dec_flipped = false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int Dec_AddBox(dec_TombEngine_box_aux box)
        {
            int boxIndex = -1;

            // Pack search values
            int boxWaterInt = box.Water ? 1 : 0;

            if (Sse2.IsSupported)
            {
                var searchBounds = Vector128.Create(box.Xmin, box.Xmax, box.Zmin, box.Zmax);
                var searchExtra = Vector128.Create(box.TrueFloor, boxWaterInt, 0, 0);

                for (int i = 0; i < dec_boxes.Count; i++)
                {
                    var candidate = dec_boxes[i];

                    var candBounds = Vector128.Create(candidate.Xmin, candidate.Xmax, candidate.Zmin, candidate.Zmax);
                    var candExtra = Vector128.Create(candidate.TrueFloor, candidate.Water ? 1 : 0, 0, 0);

                    // Compare all 4 bounds at once
                    var cmpBounds = Sse2.CompareEqual(searchBounds, candBounds);
                    var cmpExtra = Sse2.CompareEqual(searchExtra, candExtra);

                    int maskBounds = Sse2.MoveMask(cmpBounds.AsByte());
                    int maskExtra = Sse2.MoveMask(cmpExtra.AsByte());

                    // All 4 bounds must match (0xFFFF) and first 2 extra must match (0x00FF)
                    if (maskBounds == 0xFFFF && (maskExtra & 0x00FF) == 0x00FF)
                    {
                        boxIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // Scalar fallback
                for (int i = 0; i < dec_boxes.Count; i++)
                {
                    if (dec_boxes[i].Xmin == box.Xmin &&
                        dec_boxes[i].Xmax == box.Xmax &&
                        dec_boxes[i].Zmin == box.Zmin &&
                        dec_boxes[i].Zmax == box.Zmax &&
                        dec_boxes[i].TrueFloor == box.TrueFloor &&
                        dec_boxes[i].Water == box.Water)
                    {
                        boxIndex = i;
                        break;
                    }
                }
            }

            if (boxIndex == -1)
            {
                boxIndex = dec_boxes.Count;
                box.OverlapIndex = -1;
                dec_boxes.Add(box);
            }
            else
            {
                if (dec_flipped)
                    dec_boxes[boxIndex].Flag0x02 = true;
            }

            return boxIndex;
        }

        private bool Dec_CreateNewBox(dec_TombEngine_box_aux box, int x, int z, Room theRoom)
        {
            bool monkey = false;

            Room room = theRoom;
            Sector sector = room.Sectors[x, z];

            // Check if current sector is not walkable
            if ((sector.Flags & SectorFlags.NotWalkableFloor) != 0) return false;

            if (sector.Type == SectorType.Wall ||
                sector.Type == SectorType.BorderWall ||
                sector.WallPortal != null && sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
            {
                return false;
            }

            dec_q0 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZp : sector.Floor.XnZp;
            dec_q1 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZp : sector.Floor.XpZp;
            dec_q2 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZn : sector.Floor.XpZn;
            dec_q3 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZn : sector.Floor.XnZn;

            int currentX = room.Position.X + x;
            int currentZ = room.Position.Z + z;

            dec_currentRoom = theRoom;

            Dec_CanSectorBeReachedAndIsSolid(currentX, currentZ);

            dec_graybox = false;
            dec_water = true;
            dec_monkey = false;

            bool slope;
            int floor = Dec_GetBoxFloorHeight(currentX, currentZ, out slope);
            box.TrueFloor = floor;
            box.Slope = slope;

            if (floor == _noHeight) return false;

            box.Room = dec_currentRoom;
            box.Water = room.Properties.Type == RoomType.Water;

            if (dec_flipped)
            {
                box.Flag0x02 = true;
            }
            else
            {
                box.Flag0x04 = true;
            }

            if (dec_monkey)
            {
                box.Monkey = true;
                monkey = true;
            }

            if (!dec_water)
            {
                box.Water = false;
            }

            if (dec_graybox)
            {
                box.Xmin = currentX;
                box.Zmin = currentZ;
                box.Xmax = currentX + 1;
                box.Zmax = currentZ + 1;
                box.IsolatedBox = true;

                return true;
            }
            else
            {
                dec_graybox = true;

                int direction = 0x0f;
                int directionBase = 0x0f;

                int xMin = currentX;
                int xMax = currentX;
                int zMin = currentZ;
                int zMax = currentZ;

                Room currentRoom = theRoom;
                Room currentRoom1 = theRoom;
                Room currentRoom2 = theRoom;
                Room currentRoom3 = theRoom;
                Room currentRoom4 = theRoom;

                int searchX = xMin;
                int searchZ = zMin;

                while (true)
                {
                    if ((directionBase & 0x04) == 0x04)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom1;
                        currentRoom = currentRoom1;

                        searchX = xMin;

                        if (xMin <= xMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMin) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMin - 1) &&
                                   dec_monkey == monkey)
                            {
                                if (searchX == xMin) currentRoom1 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting sector is != floor of sector (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMin - 1)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
                                }

                                searchX++;

                                if (searchX > xMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x04;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x04) == 0x04) zMin--;
                    }

                    if ((directionBase & 0x02) == 0x02)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom2;
                        currentRoom = currentRoom2;

                        searchZ = zMin;

                        if (zMin <= zMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMax, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMax + 1, searchZ) &&
                                   dec_monkey == monkey)
                            {
                                if (searchZ == zMin) currentRoom2 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting sector is != floor of sector (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMax + 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
                                }

                                searchZ++;

                                if (searchZ > zMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x02;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x02) == 0x02) xMax++;
                    }

                    if ((directionBase & 0x08) == 0x08)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom3;
                        currentRoom = currentRoom3;

                        searchX = xMax;

                        if (xMax >= xMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMax) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMax + 1) &&
                                   dec_monkey == monkey)
                            {
                                if (searchX == xMax) currentRoom3 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting sector is != floor of sector (X, Z + 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMax + 1)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
                                }

                                searchX--;

                                if (searchX < xMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x08;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x08) == 0x08) zMax++;
                    }

                    if ((directionBase & 0x01) == 0x01)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom4;
                        currentRoom = currentRoom4;

                        searchZ = zMax;

                        if (zMax >= zMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMin, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMin - 1, searchZ) &&
                                   dec_monkey == monkey)
                            {
                                if (searchZ == zMax) currentRoom4 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting sector is != floor of sector (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMin - 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
                                }

                                searchZ--;

                                if (searchZ < zMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x01;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x01) == 0x01) xMin--;
                    }

                    if (directionBase == 0x00) break;

                    currentX = xMin;
                }

                box.Xmin = xMin;
                box.Zmin = zMin;
                box.Xmax = xMax + 1;
                box.Zmax = zMax + 1;

                return true;
            }
        }

        private bool Dec_CanSectorBeReachedAndIsSolid(int x, int z)
        {
            bool borderOrOutside = Dec_ClampRoom(x, z);

            Room theRoom = dec_currentRoom;
            Room startRoom = dec_currentRoom;

            int xInRoom = 0;
            int zInRoom = 0;

            Room room;
            Sector sector;

            if (borderOrOutside)
            {
                while (true)
                {
                    room = theRoom;

                    xInRoom = x - room.Position.X;
                    zInRoom = z - room.Position.Z;

                    if (xInRoom >= 0)
                    {
                        if (xInRoom >= room.NumXSectors)
                            xInRoom = room.NumXSectors - 1;
                    }
                    else
                    {
                        xInRoom = 0;
                    }

                    if (zInRoom >= 0)
                    {
                        if (zInRoom >= room.NumZSectors)
                            zInRoom = room.NumZSectors - 1;
                    }
                    else
                    {
                        zInRoom = 0;
                    }

                    sector = room.Sectors[xInRoom, zInRoom];

                    // HACK: this code was not inside the original functions but the procedure fails if xInRoom and zInRoom are one of the 4 corners.
                    // This happen for example when there are 3 room connected together and the corner is inside the box.
                    // In this case, there are portals but the function can't travel to neighbour rooms because is stuck in the corner.
                    // For now I assume that the dest X, Z can't be reached.
                    if (xInRoom == 0 && zInRoom == 0 ||
                        xInRoom == 0 && zInRoom == room.NumZSectors - 1 ||
                        xInRoom == room.NumXSectors - 1 && zInRoom == 0 ||
                        xInRoom == room.NumXSectors - 1 && zInRoom == room.NumZSectors - 1)
                        return false;

                    if (sector.WallPortal == null)
                        break;

                    Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

                    dec_currentRoom = adjoiningRoom;
                    theRoom = adjoiningRoom;

                    if (sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
                        return false;

                    if (!Dec_ClampRoom(x, z))
                        break;
                }

                room = dec_currentRoom;

                xInRoom = x - room.Position.X;
                zInRoom = z - room.Position.Z;

                // HACK: this is a very ultra rare case, I adopt a conservative approach here
                if (xInRoom < 0 || zInRoom < 0 || xInRoom >= room.NumXSectors || zInRoom >= room.NumZSectors)
                    return false;

                sector = room.Sectors[xInRoom, zInRoom];

                // After having probed that we can reach X, Z from the original room, do the following
                while (room.GetFloorRoomConnectionInfo(new VectorInt2(xInRoom, zInRoom), true).TraversableType == Room.RoomConnectionType.FullPortal)
                {
                    Room adjoiningRoom = sector.FloorPortal.AdjoiningRoom;
                    if (room.Properties.Type == RoomType.Water != (adjoiningRoom.Properties.Type == RoomType.Water))
                        break;

                    dec_currentRoom = adjoiningRoom;

                    room = dec_currentRoom;

                    xInRoom = x - room.Position.X;
                    zInRoom = z - room.Position.Z;

                    sector = room.Sectors[xInRoom, zInRoom];
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_ClampRoom(int x, int z)
        {
            Room room = dec_currentRoom;
            return x < 0 || z < 0 || x > room.NumXSectors - 1 || z > room.NumZSectors - 1;
        }

        private int Dec_GetBoxFloorHeight(int x, int z)
        {
            bool slope;
            return Dec_GetBoxFloorHeight(x, z, out slope);
        }

        private int Dec_GetBoxFloorHeight(int x, int z, out bool slope)
        {
            slope = false;

            Room adjoiningRoom = dec_currentRoom;
            Room room = dec_currentRoom;

            // Ignore pathfinding for current room?
            if (dec_currentRoom.Properties.FlagExcludeFromPathFinding) return _noHeight;

            int posXsectors = room.Position.X;
            int posZsectors = room.Position.Z;

            int xInRoom = x - posXsectors;
            int zInRoom = z - posZsectors;

            if (xInRoom < 0 ||
                xInRoom > room.NumXSectors - 1 ||
                zInRoom < 0 ||
                zInRoom > room.NumZSectors - 1)
            {
                return _noHeight;
            }

            Sector sector = room.Sectors[xInRoom, zInRoom];

            // If sector is a wall or is a vertical toggle opacity 1
            // Note that is & 8 because wall and border wall are the only sectors with bit 4 (0x08) set
            if ((sector.Type == SectorType.Wall ||
                 sector.Type == SectorType.BorderWall) && (sector.WallPortal == null ||
                sector.WallPortal != null && sector.WallPortal.Opacity == PortalOpacity.SolidFaces) ||
                (sector.Flags & SectorFlags.NotWalkableFloor) != 0)
            {
                dec_q0 = Clicks.ToWorld(-1);
                dec_q1 = Clicks.ToWorld(-1);
                dec_q2 = Clicks.ToWorld(-1);
                dec_q3 = Clicks.ToWorld(-1);

                return _noHeight;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if (sector.WallPortal == null || sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
            {

            }
            else
            {
                adjoiningRoom = sector.WallPortal.AdjoiningRoom;
                dec_currentRoom = adjoiningRoom;
                dec_boxExtendsInAnotherRoom = true;

                room = dec_currentRoom;

                posXsectors = room.Position.X;
                posZsectors = room.Position.Z;

                xInRoom = x - posXsectors;
                zInRoom = z - posZsectors;

                sector = room.Sectors[xInRoom, zInRoom];
            }

            Room oldRoom = adjoiningRoom;

            /*while (room.GetFloorRoomConnectionInfo(new VectorInt2(xInRoom, zInRoom)).TraversableType == Room.RoomConnectionType.FullPortal)
            {
                Room adjoiningRoom2 = sector.FloorPortal.AdjoiningRoom;

                if (!((room.Type == RoomType.Water) ^ (adjoiningRoom2.Type == RoomType.Water)))
                    break;

                dec_currentRoom = adjoiningRoom2;
                room = dec_currentRoom;

                posXsectors = room.Position.X;
                posZsectors = room.Position.Z;

                xInRoom = x - posXsectors;
                zInRoom = z - posZsectors;

                sector = room.Sectors[xInRoom, zInRoom];
            }*/

            var connInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(xInRoom, zInRoom));
            while (sector.FloorPortal != null && connInfo.TraversableType != Room.RoomConnectionType.NoPortal)
            {
                Room adjoiningRoom2 = sector.FloorPortal.AdjoiningRoom;

                if (sector.FloorPortal.Opacity == PortalOpacity.SolidFaces)
                {
                    if (!((room.Properties.Type == RoomType.Water) ^ (adjoiningRoom2.Properties.Type == RoomType.Water)))
                        break;
                }

                dec_currentRoom = adjoiningRoom2;
                room = dec_currentRoom;

                posXsectors = room.Position.X;
                posZsectors = room.Position.Z;

                xInRoom = x - posXsectors;
                zInRoom = z - posZsectors;

                sector = room.Sectors[xInRoom, zInRoom];
                connInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(xInRoom, zInRoom));
            }

            if ((sector.Flags & SectorFlags.NotWalkableFloor) != 0) return _noHeight;

            int floorXnZp = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZp : sector.Floor.XnZp,
                floorXpZp = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZp : sector.Floor.XpZp,
                floorXpZn = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZn : sector.Floor.XpZn,
                floorXnZn = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZn : sector.Floor.XnZn;

            int sumHeights = floorXnZp + floorXpZp + floorXpZn + floorXnZn;
            int meanFloorCornerHeight = sumHeights / 4;

            dec_q0 = floorXnZp;
            dec_q1 = floorXpZp;
            dec_q2 = floorXpZn;
            dec_q3 = floorXnZn;

            int slope1 = Math.Abs(dec_q0 - dec_q1) >= Clicks.ToWorld(3) ? 1 : 0;
            int slope2 = Math.Abs(dec_q1 - dec_q2) >= Clicks.ToWorld(3) ? 1 : 0;
            int slope3 = Math.Abs(dec_q2 - dec_q3) >= Clicks.ToWorld(3) ? 1 : 0;
            int slope4 = Math.Abs(dec_q3 - dec_q0) >= Clicks.ToWorld(3) ? 1 : 0;

            bool someFlag = false;

            if (floorXnZp == floorXpZn)
            {
                someFlag = false;
            }
            else
            {
                if (floorXpZp != floorXnZn)
                {
                    if (floorXnZp < floorXpZp && floorXnZp < floorXnZn ||
                        floorXpZn < floorXpZp && floorXpZn < floorXnZn ||
                        floorXnZp > floorXpZp && floorXnZp > floorXnZn ||
                        floorXpZn > floorXpZp && floorXpZn > floorXnZn)
                    {
                        someFlag = true;
                    }
                    else
                    {
                        someFlag = false;
                    }
                }
                else
                {
                    someFlag = true;
                }
            }

            int floorHeight = meanFloorCornerHeight + room.Position.Y;

            int ceiling = sector.HasGhostBlock
                ? sector.GhostBlock.Ceiling.Max + room.Position.Y
                : sector.Ceiling.Max + room.Position.Y;

            if (dec_water && room.Properties.Type == RoomType.Water && ceiling - meanFloorCornerHeight <= Clicks.ToWorld(1) && sector.CeilingPortal != null)
            {
                Room adjoiningRoom3 = sector.CeilingPortal.AdjoiningRoom;
                if (adjoiningRoom3.AlternateRoom != null && dec_flipped) adjoiningRoom3 = adjoiningRoom3.AlternateRoom;

                if (adjoiningRoom3.Properties.Type != RoomType.Water)
                {
                    dec_water = false;
                }
            }

            dec_currentRoom = oldRoom;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 + slope3 == 2 || slope2 + slope4 == 2)
            {
                if (dec_water && room.Properties.Type != RoomType.Water)
                    slope = true;
            }
            else
            {
                if (someFlag)
                {
                    if ((slope1 == 0 || slope2 == 0) && (slope3 == 0 || slope4 == 0))
                    {

                    }
                    else
                    {
                        if (dec_water && room.Properties.Type != RoomType.Water)
                            slope = true;
                    }
                }
                else
                {
                    if (slope1 + slope4 == 2 || slope2 + slope3 == 2)
                    {
                        if (dec_water && room.Properties.Type != RoomType.Water)
                            slope = true;
                    }
                }
            }

            if ((sector.Flags & SectorFlags.Box) == 0)
            {
                dec_monkey = (sector.Flags & SectorFlags.Monkey) != 0;
                return floorHeight;
            }
            else
            {
                if (!dec_graybox)
                {
                    dec_graybox = true;
                    dec_monkey = (sector.Flags & SectorFlags.Monkey) != 0;
                    return floorHeight;
                }
                else
                {
                    return _noHeight;
                }
            }
        }

        private bool Dec_CheckIfCanJumpX(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            int xMin = a.Xmin;
            if (a.Xmin <= b.Xmin)
                xMin = b.Xmin;

            int xMax = a.Xmax;
            if (a.Xmax >= b.Xmax)
                xMax = b.Xmax;

            int zMin = a.Zmin;
            int zMax = b.Zmax;

            int currentX = (xMin + xMax) >> 1;

            int floor = 0;

            // 1 sector jump
            if (zMax == zMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                    return true;

                return false;
            }

            // 2 sectors jump
            if (zMax == zMin - 2)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1))
                    {
                        floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                        if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                            return true;
                    }
                }

                return false;
            }

            // Swap cases

            zMin = b.Zmin;
            zMax = a.Zmax;

            // 1 sector jump
            if (zMax == zMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                    return true;

                return false;
            }

            // 2 sectors jump
            if (zMax == zMin - 2)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1))
                    {
                        floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                        if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                            return true;
                    }
                }

                return false;
            }

            return false;
        }

        private bool Dec_CheckIfCanJumpZ(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            int zMin = a.Zmin;
            if (a.Zmin <= b.Zmin)
                zMin = b.Zmin;

            int zMax = a.Zmax;
            if (a.Zmax >= b.Zmax)
                zMax = b.Zmax;

            int xMin = a.Xmin;
            int xMax = b.Xmax;

            int currentZ = (zMin + zMax) >> 1;

            int floor = 0;

            // 1 sector jump
            if (xMax == xMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                    return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                    return true;

                return false;
            }

            // 2 sectors jump
            if (xMax == xMin - 2)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                    return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ))
                    {
                        floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                        if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                            return true;
                    }
                }

                return false;
            }

            // Swap cases
            xMin = b.Xmin;
            xMax = a.Xmax;

            // 1 sector jump
            if (xMax == xMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                    return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                    return true;

                return false;
            }

            // 2 sectors jump
            if (xMax == xMin - 2)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                    return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ))
                    {
                        floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                        if (floor <= b.TrueFloor - Clicks.ToWorld(2) && floor != _noHeight)
                            return true;
                    }
                }

                return false;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapXmax(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin)
                startZ = a.Zmin;

            int endZ = a.Zmax;
            if (a.Zmax >= b.Zmax)
                endZ = b.Zmax;

            if (startZ >= endZ)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmax - 1, startZ)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmax, startZ)) break;

                    startZ++;

                    if (startZ >= endZ) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapXmin(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin)
                startZ = a.Zmin;

            int endZ = a.Zmax;
            if (a.Zmax >= b.Zmax)
                endZ = b.Zmax;

            if (startZ >= endZ)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmin, startZ)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmin - 1, startZ)) break;

                    startZ++;

                    if (startZ >= endZ) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int Dec_CalculateMeanFloor(int floorXnZp, int floorXpZp, int floorXpZn, int floorXnZn)
        {
            if (Sse2.IsSupported)
            {
                var floors = Vector128.Create(floorXnZp, floorXpZp, floorXpZn, floorXnZn);

                // Horizontal add: sum all 4 elements
                var sum1 = Sse2.Add(floors, Sse2.Shuffle(floors, 0b10_11_00_01)); // [0+1, 2+3, ...]
                var sum2 = Sse2.Add(sum1, Sse2.Shuffle(sum1, 0b01_00_11_10));     // [0+1+2+3, ...]

                return sum2.GetElement(0) / 4;
            }

            return (floorXnZp + floorXpZp + floorXpZn + floorXnZn) / 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Dec_OverlapZmax(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            int startX = b.Xmin;
            if (a.Xmin > b.Xmin)
                startX = a.Xmin;

            int endX = a.Xmax;
            if (a.Xmax >= b.Xmax)
                endX = b.Xmax;

            if (startX >= endX)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmax - 1)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmax)) break;

                    startX++;

                    if (startX >= endX) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapZmin(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            int startX = b.Xmin;
            if (a.Xmin > b.Xmin)
                startX = a.Xmin;

            int endX = a.Xmax;
            if (a.Xmax >= b.Xmax)
                endX = b.Xmax;

            if (startX >= endX)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmin)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmin - 1)) break;

                    startX++;

                    if (startX >= endX) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_BoxesOverlap(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            dec_jump = false;
            dec_monkey = false;

            dec_TombEngine_box_aux box1 = a;
            dec_TombEngine_box_aux box2 = b;

            if (b.TrueFloor > a.TrueFloor)
            {
                box1 = b;
                box2 = a;
            }

            // Cache values to avoid repeated memory access
            int b1Xmin = box1.Xmin, b1Xmax = box1.Xmax, b1Zmin = box1.Zmin, b1Zmax = box1.Zmax;
            int b2Xmin = box2.Xmin, b2Xmax = box2.Xmax, b2Zmin = box2.Zmin, b2Zmax = box2.Zmax;

            // AABB overlap test with SIMD
            bool xNoOverlap, zOverlap;

            if (Sse2.IsSupported)
            {
                // Load bounds into vectors
                // [b1Xmax, b1Zmax, b2Xmax, b2Zmax]
                var maxs = Vector128.Create(b1Xmax, b1Zmax, b2Xmax, b2Zmax);
                // [b2Xmin, b2Zmin, b1Xmin, b1Zmin]
                var mins = Vector128.Create(b2Xmin, b2Zmin, b1Xmin, b1Zmin);

                // Test: max > min for each pair
                // Result: [b1Xmax > b2Xmin, b1Zmax > b2Zmin, b2Xmax > b1Xmin, b2Zmax > b1Zmin]
                var cmp = Sse2.CompareGreaterThan(maxs, mins);
                int mask = Sse2.MoveMask(cmp.AsByte());

                // Extract results (each int = 4 bytes, so check groups of 4 bits)
                bool b1XmaxGtB2Xmin = (mask & 0x000F) == 0x000F;
                bool b1ZmaxGtB2Zmin = (mask & 0x00F0) == 0x00F0;
                bool b2XmaxGtB1Xmin = (mask & 0x0F00) == 0x0F00;
                bool b2ZmaxGtB1Zmin = (mask & 0xF000) == 0xF000;

                bool xOverlap = b1XmaxGtB2Xmin && b2XmaxGtB1Xmin;
                zOverlap = b1ZmaxGtB2Zmin && b2ZmaxGtB1Zmin;
                xNoOverlap = !xOverlap;
            }
            else
            {
                // Scalar fallback
                bool xOverlap = b1Xmax > b2Xmin && b1Xmin < b2Xmax;
                zOverlap = b1Zmax > b2Zmin && b1Zmin < b2Zmax;
                xNoOverlap = !xOverlap;
            }

            // Main logic (same structure as original)
            if (xNoOverlap)
            {
                if (zOverlap && Dec_CheckIfCanJumpZ(box1, box2))
                {
                    dec_jump = true;
                    return true;
                }

                // Early exit with cached values
                if (b1Xmax < b2Xmin || b1Xmin > b2Xmax || b1Zmax <= b2Zmin || b1Zmin >= b2Zmax)
                    return false;

                if (b1Xmax == b2Xmin && !Dec_OverlapXmax(box1, box2))
                    return false;

                if (b1Xmin == b2Xmax && !Dec_OverlapXmin(box1, box2))
                    return false;

                if (box1.Monkey && box2.Monkey)
                    dec_monkey = true;

                return true;
            }

            if (zOverlap)
            {
                if (box1.TrueFloor != box2.TrueFloor)
                    return false;

                if (box1.Monkey && box2.Monkey)
                    dec_monkey = true;

                return true;
            }

            if (Dec_CheckIfCanJumpX(box2, box1))
            {
                dec_jump = true;
                return true;
            }

            if (b1Zmax < b2Zmin || b1Zmin > b2Zmax)
                return false;

            if (b1Zmax == b2Zmin && !Dec_OverlapZmax(box1, box2))
                return false;

            if (b1Zmin != b2Zmax)
            {
                if (box1.Monkey && box2.Monkey)
                    dec_monkey = true;

                return true;
            }

            if (Dec_OverlapZmin(box1, box2))
            {
                if (box1.Monkey && box2.Monkey)
                    dec_monkey = true;

                return true;
            }

            return false;
        }
    }
}

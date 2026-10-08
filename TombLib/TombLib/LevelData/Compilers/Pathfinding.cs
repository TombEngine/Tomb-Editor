using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private void BuildPathFindingData()
        {
            ReportProgress(48, "Building pathfinding data");

            // Check for any rooms outside of map bounds
            if (_sortedRooms.Any(r => r != null && (r.Position.X < 0 || r.Position.Z < 0)))
                _progressReporter.ReportWarn("Some rooms are out of map bounds. This will cause pathfinding issues. " +
                    "Place all rooms within 2D map bounds.");

            // Fix monkey on portals
            foreach (var fixRoom in _tempRooms.Values)
            {
                for (int x = 0; x < fixRoom.NumXSectors; x++)
                {
                    for (int z = 0; z < fixRoom.NumZSectors; z++)
                    {
                        var sector = fixRoom.AuxSectors[x, z];
                        if (sector.FloorPortal != null)
                            sector.Monkey = FindMonkeyFloor(fixRoom.OriginalRoom, x, z);
                    }
                }
            }

            // Use decompiled code for generation of boxes and overlaps
            Dec_BuildBoxesAndOverlaps();

            // Convert ovelaps to TR format
            _overlaps = new ushort[dec_numOverlaps];
            if (dec_numOverlaps != 0)
                Array.Copy(dec_overlaps, _overlaps, dec_numOverlaps);

            // Convert boxes to TR format
            _boxes = new tr_box[dec_numBoxes];
            _zones = new tr_zone[dec_numBoxes];
            for (var i = 0; i < dec_numBoxes; i++)
            {
                _boxes[i] = new tr_box()
                {
                    Xmin = (byte)dec_boxes[i].Xmin,
                    Xmax = (byte)dec_boxes[i].Xmax,
                    Zmin = (byte)dec_boxes[i].Zmin,
                    Zmax = (byte)dec_boxes[i].Zmax,
                    TrueFloor = (short)-Clicks.ToWorld(dec_boxes[i].TrueFloor),
                    OverlapIndex = (ushort)((dec_boxes[i].OverlapIndex & _legacyOverlapIndexMask) | (dec_boxes[i].IsolatedBox ? 0x8000 : 0))
                };
                _zones[i] = new tr_zone()
                {
                    GroundZone1_Normal = 0x7ff,
                    GroundZone2_Normal = 0x7ff,
                    GroundZone3_Normal = 0x7ff,
                    GroundZone4_Normal = 0x7ff,
                    FlyZone_Normal = 0x7ff,
                    GroundZone1_Alternate = 0x7ff,
                    GroundZone2_Alternate = 0x7ff,
                    GroundZone3_Alternate = 0x7ff,
                    GroundZone4_Alternate = 0x7ff,
                    FlyZone_Alternate = 0x7ff
                };
            }

            // Create zones
            ushort groundZone1 = 1;
            ushort groundZone2 = 1;
            ushort groundZone3 = 1;
            ushort groundZone4 = 1;
            ushort flyZone = 1;
            for (var i = 0; i < _zones.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Normal == 0x7ff)
                {
                    _zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, false))
                    {
                        if (_zones[box].GroundZone1_Normal == 0x7ff) _zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Normal == 0x7ff)
                {
                    _zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, false))
                    {
                        if (_zones[box].GroundZone2_Normal == 0x7ff) _zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Normal == 0x7ff)
                {
                    _zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, false))
                    {
                        if (_zones[box].GroundZone3_Normal == 0x7ff) _zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Normal == 0x7ff)
                {
                    _zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, false))
                    {
                        if (_zones[box].GroundZone4_Normal == 0x7ff) _zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Normal == 0x7ff)
                {
                    _zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, false))
                    {
                        if (_zones[box].FlyZone_Normal == 0x7ff) _zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }
            }

            // Flipped rooms------------------------------------------
            ushort aGroundZone1 = 1;
            ushort aGroundZone2 = 1;
            ushort aGroundZone3 = 1;
            ushort aGroundZone4 = 1;
            ushort aFlyZone = 1;
            for (var i = 0; i < _zones.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, true))
                    {
                        if (_zones[box].GroundZone1_Alternate == 0x7ff) _zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, true))
                    {
                        if (_zones[box].GroundZone2_Alternate == 0x7ff) _zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, true))
                    {
                        if (_zones[box].GroundZone3_Alternate == 0x7ff) _zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, true))
                    {
                        if (_zones[box].GroundZone4_Alternate == 0x7ff) _zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Alternate == 0x7ff)
                {
                    _zones[i].FlyZone_Alternate = aFlyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, true))
                    {
                        if (_zones[box].FlyZone_Alternate == 0x7ff) _zones[box].FlyZone_Alternate = aFlyZone;
                    }

                    aFlyZone++;
                }
            }

            ReportProgress(52, "    Number of boxes/zones: " + _boxes.Length);
            ReportProgress(52, "    Number of overlaps: " + _overlaps.Length);

            if (_boxes.Length < _limits[Limit.BoxMinCount])
                _progressReporter.ReportWarn("Level has too few boxes. Add some room geometry or game may crash.");

            var boxLimit = _limits[Limit.BoxLimit];
            if (_boxes.Length >= boxLimit)
                _progressReporter.ReportWarn("Box limit is reached, maximum is " + boxLimit + ". Game may crash. Reduce level size or split it.");

            var overlapLimit = _limits[Limit.OverlapLimit] - 2;
            if (_overlaps.Length >= overlapLimit)
                _progressReporter.ReportWarn("Overlap limit is reached, maximum is " + overlapLimit + ". Game may crash. Reduce level size or split it.");
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType, bool flipped)
        {
            var boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes.
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag and same flipped flag
            var isWater = (_tempRooms[dec_boxes[box].Room].Flags & 0x01) != 0;

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                var last = false;

                for (int i = dec_boxes[next].OverlapIndex; i < _overlaps.Length && !last; i++)
                {
                    int overlapIndex = i;
                    last = (_overlaps[overlapIndex] & 0x8000) != 0;

                    var boxIndex = _overlaps[overlapIndex] & 0x7ff;

                    var add = false;

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 sectors.
                    if (zoneType == 1)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        var canJump = dec_boxes[boxIndex].Jump;

                        if (!dec_boxes[boxIndex].Slope && water == isWater && (canJump || step <= Clicks.ToWorld(1)) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 2)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        if (!dec_boxes[boxIndex].Slope && water == isWater && step <= Clicks.ToWorld(1) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step.
                    // In water they act like flying enemies. Guide seems to belong to this zone.
                    if (zoneType == 3)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        if (!dec_boxes[boxIndex].Slope && (water == isWater && step <= Clicks.ToWorld(1) || water) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 sectors and monkey.
                    if (zoneType == 4)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor);

                        // Check all possibilities
                        var canJump = dec_boxes[boxIndex].Jump;
                        var canMonkey = dec_boxes[boxIndex].Monkey;

                        if (!dec_boxes[boxIndex].Slope && water == isWater && (canJump || step <= (int)Level.SectorSizeUnit || canMonkey) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        if ((!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    if (stack.Contains(boxIndex) || !add)
                        continue;

                    if (!boxes.Contains(boxIndex))
                        stack.Push(boxIndex);

                    boxes.Add(boxIndex);
                }
            }

            return boxes;
        }
    }
}

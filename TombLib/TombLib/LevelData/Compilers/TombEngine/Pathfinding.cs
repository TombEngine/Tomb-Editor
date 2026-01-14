using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        public enum ZoneType
        {
            Skeleton,
            Basic,
            Crocodile,
            Human,
            Flyer
        }

        private void BuildPathFindingData()
        {
            ReportProgress(48, "Building pathfinding data");

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
            _overlaps = new List<TombEngineOverlap>();
            _overlaps.AddRange(dec_overlaps);

            // Convert boxes to TR format
            _boxes = new List<TombEngineBox>();
            _zones = new List<TombEngineZoneGroup>();

            for (var i = 0; i < dec_boxes.Count; i++)
            {
                var box = new TombEngineBox()
                {
                    Xmin = dec_boxes[i].Xmin,
                    Xmax = dec_boxes[i].Xmax,
                    Zmin = dec_boxes[i].Zmin,
                    Zmax = dec_boxes[i].Zmax,
                    TrueFloor = -dec_boxes[i].TrueFloor,
                    OverlapIndex = dec_boxes[i].OverlapIndex,
                    Flags = dec_boxes[i].IsolatedBox ? 0x8000 : 0
                };
                _boxes.Add(box);

                var zone = new TombEngineZoneGroup();
                _zones.Add(zone);
            }

            var zoneTypes = Enum.GetValues(typeof(ZoneType));

            // Create zones
            foreach (bool flipped in new[] { false, true })
            {
                int group = Convert.ToInt32(flipped);
                var zoneCount = Enumerable.Repeat(1, zoneTypes.Length).ToArray();

                for (var i = 0; i < _zones.Count; i++)
                {
                    foreach (var zoneType in (ZoneType[])zoneTypes)
                    {
                        if (_zones[i].Zones[group][(int)zoneType] == int.MaxValue)
                        {
                            _zones[i].Zones[group][(int)zoneType] = zoneCount[(int)zoneType];

                            foreach (var box in GetAllReachableBoxes(i, zoneType, flipped))
                            {
                                if (_zones[box].Zones[group][(int)zoneType] == int.MaxValue)
                                    _zones[box].Zones[group][(int)zoneType] = zoneCount[(int)zoneType];
                            }

                            zoneCount[(int)zoneType]++;
                        }
                    }
                }
            }

            ReportProgress(52, "    Number of boxes/zones: " + _boxes.Count);
            ReportProgress(52, "    Number of overlaps: " + _overlaps.Count);
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, ZoneType zoneType, bool flipped)
        {
            var boxes = new List<int>();

            // HACK: boxes with no overlaps have overlapIndex = -1
            if (_boxes[box].OverlapIndex < 0)
                return boxes;

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

                for (int i = _boxes[next].OverlapIndex; i < _overlaps.Count && !last; i++)
                {
                    int overlapIndex = i;
                    if (overlapIndex < 0)
                        return boxes;

                    last = (_overlaps[overlapIndex].Flags & 0x8000) != 0;

                    bool canJump = (_overlaps[overlapIndex].Flags & 0x0800) != 0;
                    bool canMonkey = (_overlaps[overlapIndex].Flags & 0x2000) != 0;

                    var boxIndex = _overlaps[overlapIndex].Box;

                    // Don't add a box if it doesn't belong to a same flip state.
                    bool sameFlip = (!flipped && dec_boxes[boxIndex].Flag0x04 || flipped && dec_boxes[boxIndex].Flag0x02);
                    if (!sameFlip)
                        continue;

                    bool water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                    int  step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                    // Don't add a box if it is underwater (for fly zone) or a slope (for all other zones).
                    if (zoneType != ZoneType.Flyer && dec_boxes[boxIndex].Slope)
                        continue;

                    // Water state check - CROC and HUMAN + monkey can cross water boundary
                    bool canCrossWater = (zoneType == ZoneType.Crocodile) ||
                                         (zoneType == ZoneType.Human && canMonkey);
                    if (water != isWater && !canCrossWater)
                        continue;

                    bool add = false;

                    switch (zoneType)
                    {
                        case ZoneType.Skeleton:
                            // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 sectors.
                            add = (step <= Clicks.ToWorld(1) || canJump);
                            break;

                        case ZoneType.Basic:
                            // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                            add = (step <= Clicks.ToWorld(1));
                            break;

                        case ZoneType.Crocodile:
                            // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step.
                            // In water they act like flying enemies. Guide seems to belong to this zone.
                            add = (step <= Clicks.ToWorld(1) || water);
                            break;

                        case ZoneType.Human:
                            // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 sectors and monkey.
                            add = (step <= (int)Level.SectorSizeUnit || canJump || canMonkey);
                            break;

                        case ZoneType.Flyer:
                            // Flying enemies. Always added, if not a water room (checked in the condition above).
                            add = true;
                            break;

                        default:
                            logger.Error("Unknown zone specified for box " + box);
                            break;
                    }

                    if (stack.Contains(boxIndex) || !add)
                        continue;

                    if (!boxes.Contains(boxIndex))
                        stack.Push(boxIndex);

                    boxes.Add(boxIndex);
                }
            }

            return boxes.Distinct();
        }
    }
}

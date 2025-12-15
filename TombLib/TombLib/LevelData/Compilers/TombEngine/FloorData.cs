using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.LevelData.SectorEnums;
using TombLib.NG;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private const ushort _fdTimerMask = 0x00FF;
        private const ushort _fdFunctionMask = 0x03FF;
        private const ushort _fdOneShotBit = 0x0100;
        private const ushort _fdEndBit = 0x8000;

        private void BuildFloorData()
        {
            ReportProgress(53, "Building floordata");

            for (var i = 0; i < _level.Rooms.Length; i++)
            {
                var room = _level.Rooms[i];
                if (room == null)
                    continue;

                var tempRoom = _tempRooms[room];

                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        Sector sector = room.Sectors[x, z];

                        // Build sector info
                        var compiledSector = GetSector(tempRoom, x, z);
                        compiledSector.TriggerIndex = -1;

                        if ((sector.Type == SectorType.Wall && sector.Floor.DiagonalSplit == DiagonalSplit.None) || sector.Type == SectorType.BorderWall)
                        {
                            // Sector is a complete wall

                            if (sector.WallPortal == null || sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
                            {
                                var ceilingPortalAssigned = false;

                                for (var xAround = Math.Max(0, x - 1); !ceilingPortalAssigned && xAround <= Math.Min(room.NumXSectors - 1, x + 1); ++xAround)
                                {
                                    for (var zAround = Math.Max(0, z - 1); !ceilingPortalAssigned && zAround <= Math.Min(room.NumZSectors - 1, z + 1); ++zAround)
                                    {
                                        if (xAround == x && zAround == z)
                                            continue;

                                        if (ceilingPortalAssigned)
                                            continue;

                                        var ceilingPortal = room.Sectors[xAround, zAround].CeilingPortal;
                                        if (ceilingPortal == null)
                                            continue;

                                        var adjoiningRoom = ceilingPortal.AdjoiningRoom;
                                        var pos = new VectorInt2(x, z);
                                        var adjoiningSector = adjoiningRoom.GetSectorTry(pos + room.SectorPos - adjoiningRoom.SectorPos);

                                        if (adjoiningSector != null)
                                        {
                                            compiledSector.CeilingCollision.Portals[0] = _roomRemapping[adjoiningRoom];
                                            compiledSector.CeilingCollision.Portals[1] = compiledSector.CeilingCollision.Portals[0];
                                            ceilingPortalAssigned = true;
                                        }
                                    }
                                }

                                if ((x == 0 || x == room.NumXSectors - 1) && (z == 0 || z == room.NumZSectors - 1))
                                {
                                    var x2 = x == 0 ? 1 : room.NumXSectors - 2;
                                    var z2 = z == 0 ? 1 : room.NumZSectors - 2;
                                    for (var b = 0; b < 2; ++b)
                                    {
                                        var neighborSector = b == 0 ? room.Sectors[x2, z] : room.Sectors[x, z2];

                                        if (neighborSector.WallPortal != null && neighborSector.WallPortal.Opacity != PortalOpacity.SolidFaces)
                                        {
                                            var adjoiningRoom = neighborSector.WallPortal.AdjoiningRoom;
                                            var pos = new VectorInt2(x, z);
                                            var adjoiningSector = adjoiningRoom.GetSectorTry(pos + room.SectorPos - adjoiningRoom.SectorPos);

                                            if (adjoiningSector != null && (adjoiningSector.Type != SectorType.BorderWall || adjoiningSector.WallPortal != null && adjoiningSector.WallPortal.Opacity != PortalOpacity.SolidFaces))
                                            {
                                                compiledSector.WallPortal = _roomRemapping[adjoiningRoom];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                compiledSector.WallPortal = _roomRemapping[sector.WallPortal.AdjoiningRoom];
                            }

                            compiledSector.FloorCollision.Planes[0].Z = -room.Position.Y;
                            compiledSector.FloorCollision.Planes[1].Z = -room.Position.Y;
                            compiledSector.CeilingCollision.Planes[0].Z = -room.Position.Y;
                            compiledSector.CeilingCollision.Planes[1].Z = -room.Position.Y;
                        }
                        else
                        {
                            // Sector is not a complete wall

                            Room.RoomConnectionType floorPortalType = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType;
                            Room.RoomConnectionType ceilingPortalType = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType;
                            var floorShape = new RoomSectorShape(sector, true, floorPortalType, sector.IsAnyWall);
                            var ceilingShape = new RoomSectorShape(sector, false, ceilingPortalType, sector.IsAnyWall);

                            // Floor collision
                            BuildFloorDataCollision(floorShape, floorPortalType, false, room, new VectorInt2(x, z));

                            // Ceiling collision
                            BuildFloorDataCollision(ceilingShape, ceilingPortalType, true, room, new VectorInt2(x, z));

                            // Triggers
                            var triggers = BuildTriggers(room, sector, new VectorInt2(x, z));
                            if (triggers.Count != 0)
                            {
                                compiledSector.TriggerIndex = checked((ushort)_floorData.Count);
                                _floorData.AddRange(triggers);
                            }
                        }

                        // Update the sector
                        SaveSector(tempRoom, x, z, compiledSector);
                    }
                }
            }

            ReportProgress(58, "    Floordata size: " + _floorData.Count * 2 + " bytes");
        }

        private List<ushort> BuildTriggers(Room room, Sector sector, VectorInt2 pos)
        {
            var result = new List<ushort>();

            // Collect all valid triggers
            var triggers = sector.Triggers.Where(t => NgParameterInfo.TriggerIsValid(_level.Settings, t)).ToList();

            // Filter out singular key/switch triggers, as they are technically invalid in engine
            if (triggers.Count == 1 && (triggers[0].TriggerType == TriggerType.Key ||
                                        triggers[0].TriggerType == TriggerType.Switch ||
                                        triggers[0].TriggerType == TriggerType.Pickup))
            {
                _progressReporter.ReportWarn("Key, switch or pickup trigger in room " + room + " at sector (" + pos.X + "," + pos.Y +
                                             ") has no additional actions and will be ignored.");
            }
            else if (triggers.Count > 0)
            {
                if (triggers.Count > 1) TriggerInstance.SortTriggerList(ref triggers);
                var setupTrigger = triggers[0];

                // Trigger type and setup are coming from the found setup trigger.
                // Other triggers are needed only for action.

                ushort trigger1 = 0;
                switch (setupTrigger.TriggerType)
                {
                    case TriggerType.Trigger:
                        trigger1 |= 0x00 << 8;
                        break;
                    case TriggerType.Pad:
                        trigger1 |= 0x01 << 8;
                        break;
                    case TriggerType.Switch:
                        trigger1 |= 0x02 << 8;
                        break;
                    case TriggerType.Key:
                        trigger1 |= 0x03 << 8;
                        break;
                    case TriggerType.Pickup:
                        trigger1 |= 0x04 << 8;
                        break;
                    case TriggerType.Heavy:
                        trigger1 |= 0x05 << 8;
                        break;
                    case TriggerType.Antipad:
                        trigger1 |= 0x06 << 8;
                        break;
                    case TriggerType.Combat:
                        trigger1 |= 0x07 << 8;
                        break;
                    case TriggerType.Dummy:
                        trigger1 |= 0x08 << 8;
                        break;
                    case TriggerType.Antitrigger:
                        trigger1 |= 0x09 << 8;
                        break;
                    case TriggerType.HeavySwitch:
                        trigger1 |= 0x0a << 8;
                        break;
                    case TriggerType.HeavyAntitrigger:
                        trigger1 |= 0x0b << 8;
                        break;
                    case TriggerType.Monkey:
                        trigger1 |= 0x0c << 8;
                        break;
                    case TriggerType.TightRope:
                        trigger1 |= 0x0d << 8;
                        break;
                    case TriggerType.Crawl:
                        trigger1 |= 0x0e << 8;
                        break;
                    case TriggerType.Climb:
                        trigger1 |= 0x0f << 8;
                        break;
                    case TriggerType.ConditionNg: // Not supported but still may be present in project
                    case TriggerType.Skeleton:
                        _progressReporter.ReportWarn("Sector (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " uses trigger type which is not supported in Tomb Engine.");
                        break;
                    default:
                        throw new Exception("Unknown trigger type found '" + setupTrigger + "'");
                }

                // Do some warnings in case user switches targets and some incompatible trigger targets are left behind

                if (setupTrigger.TargetType == TriggerTargetType.ActionNg ||
                    setupTrigger.TargetType == TriggerTargetType.ParameterNg ||
                    setupTrigger.TargetType == TriggerTargetType.TimerfieldNg ||
                    setupTrigger.TargetType == TriggerTargetType.FmvNg)
                    _progressReporter.ReportWarn("Sector (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " uses trigger target which is not supported in Tomb Engine.");

                var triggerSetup = GetTriggerParameter(setupTrigger.Timer, setupTrigger, _fdTimerMask);

                triggerSetup |= (ushort)(setupTrigger.OneShot ? 0x100 : 0);

                // Write bitmask
                triggerSetup |= (ushort)((setupTrigger.CodeBits & 0x1F) << 9);

                result.Add(trigger1);
                result.Add(triggerSetup);

                foreach (var trigger in triggers)
                {
                    ushort trigger2 = 0;

                    ushort func = (ushort)((ushort)trigger.TargetType << 10);

                    switch (trigger.TargetType)
                    {
                        case TriggerTargetType.Object:
                        case TriggerTargetType.Sink:
                        case TriggerTargetType.FlipMap:
                        case TriggerTargetType.FlipOn:
                        case TriggerTargetType.FlipOff:
                        case TriggerTargetType.Target:
                        case TriggerTargetType.FinishLevel:
                        case TriggerTargetType.PlayAudio:
                        case TriggerTargetType.FlipEffect:
                        case TriggerTargetType.Secret:
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, _fdFunctionMask) | func);
                            result.Add(trigger2);
                            break;

                        case TriggerTargetType.Camera:
                            // Trigger for camera
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, _fdFunctionMask) | func);
                            result.Add(trigger2);

                            // Additional short
                            trigger2 = GetTriggerParameter(trigger.Timer, trigger, _fdTimerMask);
                            trigger2 |= (ushort)(trigger.OneShot ? _fdOneShotBit : 0);
                            result.Add(trigger2);
                            break;

                        case TriggerTargetType.FlyByCamera:
                            // Trigger for fly by
                            if (!(trigger.Target is FlybyCameraInstance))
                                throw new Exception("A Flyby trigger must point to a flyby camera! ('" + trigger + "')");

                            var flyByCamera = (FlybyCameraInstance)trigger.Target;
                            trigger2 = (ushort)(flyByCamera.Sequence & _fdFunctionMask | func);
                            result.Add(trigger2);

                            trigger2 = (ushort)(trigger.OneShot ? _fdOneShotBit : 0);
                            result.Add(trigger2);
                            break;

                        case TriggerTargetType.VolumeEvent:
                        case TriggerTargetType.GlobalEvent:
                            if (!(trigger.Target is TriggerParameterString))
                            {
                                throw new Exception("An event trigger must reference an event set name! ('" + trigger + "')");
                            }

                            string setName = (trigger.Target as TriggerParameterString).Value;
                            {
                                var usedList = trigger.TargetType == TriggerTargetType.GlobalEvent ? _level.Settings.GlobalEventSets : _level.Settings.VolumeEventSets;

                                if (!usedList.Any(s => s.Name == setName))
                                {
                                    _progressReporter.ReportWarn("The trigger at (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " refers to the missing event set '" + setName + "'.");
                                    continue;
                                }

                                trigger2 = (ushort)((usedList.FindIndex(s => s.Name == setName)) & _fdFunctionMask | func);
                                result.Add(trigger2);

                                trigger2 = GetTriggerParameter(trigger.Timer, trigger, _fdTimerMask);
                                trigger2 |= (ushort)(trigger.OneShot ? _fdOneShotBit : 0);
                                result.Add(trigger2);
                            }

                            break;

                        default:
                            throw new Exception("Unknown trigger target found '" + trigger + "'");
                    }
                }

                result[result.Count - 1] |= _fdEndBit; // End of the action list
            }

            return result;
        }

        private ushort GetTriggerParameter(ITriggerParameter parameter, TriggerInstance triggerDiagnostic, ushort upperBound)
        {
            int index;
            if (parameter == null)
                index = 0;
            else if (parameter is Room)
                index = _roomRemapping[(Room)parameter];
            else if (parameter is ObjectInstance)
            {
                if (parameter is MoveableInstance)
                {
                    MoveableInstance @object = (MoveableInstance)parameter;
                    bool isAI = TrCatalog.IsMoveableAI(_level.Settings.GameVersion, @object.WadObjectId.TypeId);
                    var table = isAI ? _aiObjectsTable : _moveablesTable;
                    if (!table.TryGetValue(@object, out index))
                    {
                        _progressReporter.ReportWarn("Trigger '" + triggerDiagnostic + "') referring to illegal moveable '" + @object + "'.");
                        index = 0;
                    }
                }
                else if (parameter is CameraInstance)
                    index = _cameraTable[(CameraInstance)parameter];
                else if (parameter is SinkInstance)
                    index = _sinkTable[(SinkInstance)parameter];
                else if (parameter is FlybyCameraInstance)
                    index = _flybyTable[(FlybyCameraInstance)parameter];
                else if (parameter is StaticInstance)
                {
                    StaticInstance @object = (StaticInstance)parameter;
                    if (@object.ScriptId == null) // Create temporary script ID if necessary
                        index = unchecked((int)_scriptingIdsTable.UpdateWithNewId(@object, null));
                    else
                        index = unchecked((int)@object.ScriptId.Value);
                }
                else
                {
                    _progressReporter.ReportWarn("Triggering an object of type " + parameter.GetType().Name + " is not supported ('" + triggerDiagnostic + "'). Game version mismatch?");
                    return 0;
                }
            }
            else if (parameter is TriggerParameterUshort)
                index = ((TriggerParameterUshort)parameter).Key & upperBound;
            else
                throw new Exception("Trigger has unrecognized parameter! ('" + triggerDiagnostic + "')");

            if (index > upperBound)
            {
                _progressReporter.ReportWarn("Trigger parameter is too big ('" + triggerDiagnostic + "'). Game version mismatch?");
                return 0;
            }
            return (ushort)index;
        }

        private struct RoomSectorShape
        {
            public readonly bool SplitDirectionIsXEqualsZ;
            public readonly bool SplitPortalFirst;
            public readonly bool SplitPortalSecond;
            public readonly bool SplitWallFirst;
            public readonly bool SplitWallSecond;
            public readonly int HeightXnZn;
            public readonly int HeightXnZp;
            public readonly int HeightXpZn;
            public readonly int HeightXpZp;
            public readonly int DiagonalStep;

            public RoomSectorShape(Sector sector, bool floor, Room.RoomConnectionType portalType, bool wall)
            {
                var surface = floor ? sector.Floor : sector.Ceiling;

                HeightXnZn = surface.XnZn;
                HeightXpZn = surface.XpZn;
                HeightXnZp = surface.XnZp;
                HeightXpZp = surface.XpZp;
                SplitDirectionIsXEqualsZ = surface.SplitDirectionIsXEqualsZWithDiagonalSplit;

                if (sector.HasGhostBlock && sector.GhostBlock.Valid)
                {
                    HeightXnZn += floor ? sector.GhostBlock.Floor.XnZn : sector.GhostBlock.Ceiling.XnZn;
                    HeightXpZn += floor ? sector.GhostBlock.Floor.XpZn : sector.GhostBlock.Ceiling.XpZn;
                    HeightXnZp += floor ? sector.GhostBlock.Floor.XnZp : sector.GhostBlock.Ceiling.XnZp;
                    HeightXpZp += floor ? sector.GhostBlock.Floor.XpZp : sector.GhostBlock.Ceiling.XpZp;
                }

                switch (portalType)
                {
                    case Room.RoomConnectionType.NoPortal:
                        SplitPortalFirst = false;
                        SplitPortalSecond = false;
                        break;
                    case Room.RoomConnectionType.FullPortal:
                        SplitPortalFirst = true;
                        SplitPortalSecond = true;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXnZn:
                        SplitPortalFirst = true;
                        SplitPortalSecond = false;
                        SplitDirectionIsXEqualsZ = false;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXnZp:
                        SplitPortalFirst = true;
                        SplitPortalSecond = false;
                        SplitDirectionIsXEqualsZ = true;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXpZp:
                        SplitPortalFirst = false;
                        SplitPortalSecond = true;
                        SplitDirectionIsXEqualsZ = false;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXpZn:
                        SplitPortalFirst = false;
                        SplitPortalSecond = true;
                        SplitDirectionIsXEqualsZ = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (surface.DiagonalSplit)
                {
                    case DiagonalSplit.None:
                        DiagonalStep = 0;
                        SplitWallFirst = wall;
                        SplitWallSecond = wall;
                        break;
                    case DiagonalSplit.XnZn:
                        DiagonalStep = surface.XpZp - surface.XnZp;
                        SplitWallFirst = wall;
                        SplitWallSecond = false;
                        break;
                    case DiagonalSplit.XnZp:
                        DiagonalStep = surface.XpZn - surface.XpZp;

                        SplitWallFirst = wall;
                        SplitWallSecond = false;
                        break;
                    case DiagonalSplit.XpZn:
                        DiagonalStep = surface.XnZp - surface.XnZn;
                        HeightXnZn += DiagonalStep;
                        HeightXpZp += DiagonalStep;
                        DiagonalStep = -DiagonalStep;

                        SplitWallFirst = false;
                        SplitWallSecond = wall;
                        break;
                    case DiagonalSplit.XpZp:
                        DiagonalStep = surface.XnZn - surface.XpZn;
                        HeightXpZn += DiagonalStep;
                        HeightXnZp += DiagonalStep;
                        DiagonalStep = -DiagonalStep;

                        SplitWallFirst = false;
                        SplitWallSecond = wall;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public int Max => Math.Max(Math.Max(HeightXnZn, HeightXnZp), Math.Max(HeightXpZn, HeightXpZp));
            public int Min => Math.Min(Math.Min(HeightXnZn, HeightXnZp), Math.Min(HeightXpZn, HeightXpZp));
            public bool IsFlat => HeightXnZp == HeightXnZn && HeightXpZn == HeightXnZn && HeightXpZp == HeightXnZn &&
                DiagonalStep == 0 && SplitPortalSecond == SplitPortalFirst && SplitWallFirst == SplitWallSecond;
            public bool IsSplit => HeightXnZn - HeightXnZp != HeightXpZn - HeightXpZp || HeightXnZn - HeightXpZn != HeightXnZp - HeightXpZp ||
                DiagonalStep != 0 || SplitPortalSecond != SplitPortalFirst || SplitWallFirst != SplitWallSecond;
        }

        private void BuildFloorDataCollision(RoomSectorShape shape, Room.RoomConnectionType portalType, bool isCeiling, Room reportRoom, VectorInt2 reportPos)
        {
            TombEngineRoom newRoom = _tempRooms[reportRoom];
            TombEngineRoomSector newSector = newRoom.Sectors[newRoom.NumZSectors * reportPos.X + reportPos.Y];
            Sector sector = reportRoom.GetSector(reportPos);
            var newCollision = isCeiling ? newSector.CeilingCollision : newSector.FloorCollision;
            var portal = isCeiling ? sector.CeilingPortal : sector.FloorPortal;

            if (portalType == Room.RoomConnectionType.NoPortal)
                portal = null;

            if (shape.IsSplit)
            {
                if (shape.SplitDirectionIsXEqualsZ)
                {
                    newCollision.SplitAngle = (float)(Math.PI / 4);

                    if (shape.SplitWallFirst)
                    {
                        if (portal != null)
                            newCollision.Portals[0] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[0].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalFirst)
                            newCollision.Portals[0] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[0] = GetPlane(
                                new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZp, Level.HalfSectorSizeUnit),
                                new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZp, Level.HalfSectorSizeUnit),
                                new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZn, -Level.HalfSectorSizeUnit)
                            );
                    }

                    if (shape.SplitWallSecond)
                    {
                        if (portal != null)
                            newCollision.Portals[1] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[1].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalSecond)
                            newCollision.Portals[1] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[1] = GetPlane(
                            new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZn, -Level.HalfSectorSizeUnit),
                            new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZn - shape.DiagonalStep, -Level.HalfSectorSizeUnit),
                            new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZp - shape.DiagonalStep, Level.HalfSectorSizeUnit)
                        );
                    }
                }
                else
                {
                    newCollision.SplitAngle = (float)(3 * Math.PI / 4);

                    if (shape.SplitWallSecond)
                    {
                        if (portal != null)
                            newCollision.Portals[0] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[0].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalSecond)
                            newCollision.Portals[0] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[0] = GetPlane(
                            new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZp, Level.HalfSectorSizeUnit),
                            new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZn - shape.DiagonalStep, -Level.HalfSectorSizeUnit),
                            new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZp - shape.DiagonalStep, Level.HalfSectorSizeUnit)
                        );
                    }
                    if (shape.SplitWallFirst)
                    {
                        if (portal != null)
                            newCollision.Portals[1] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[1].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalFirst)
                            newCollision.Portals[1] = _roomRemapping[portal.AdjoiningRoom];

                        newCollision.Planes[1] = GetPlane(
                          new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZn, -Level.HalfSectorSizeUnit),
                          new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZp, Level.HalfSectorSizeUnit),
                          new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZn, -Level.HalfSectorSizeUnit)
                      );
                    }
                }
            }
            else
            {
                if (shape.SplitPortalFirst && shape.SplitPortalSecond)
                {
                    newCollision.Portals[0] = _roomRemapping[portal.AdjoiningRoom];
                    newCollision.Portals[1] = newCollision.Portals[0];
                }

                newCollision.Planes[0] = GetPlane(
                        new Vector3(-Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXnZp, Level.HalfSectorSizeUnit),
                        new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZp, Level.HalfSectorSizeUnit),
                        new Vector3(Level.HalfSectorSizeUnit, -reportRoom.Position.Y - shape.HeightXpZn, -Level.HalfSectorSizeUnit)
                    );
                newCollision.Planes[1] = newCollision.Planes[0];
            }
        }

        private Vector3 GetPlane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var plane = Plane.CreateFromVertices(p1, p2, p3);
            return new Vector3(-plane.Normal.X, -plane.Normal.Z, Vector3.Dot(plane.Normal, p1)) / plane.Normal.Y;
        }
    }
}

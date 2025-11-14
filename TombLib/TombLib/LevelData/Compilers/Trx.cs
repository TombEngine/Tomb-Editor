using System;
using System.Collections.Generic;
using System.IO;
using TombLib.IO;
using TombLib.LevelData.Compilers.Util;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.Compilers;

public partial class LevelCompilerClassicTR
{
    private const int _legacyRoomLimit = 255;
    private const int _noRoom = -1;

    private void WriteLevelTrx()
    {
        switch (_level.Settings.GameVersion)
        {
            case TRVersion.Game.TR1X:
                WriteLevelTr1();
                break;
            case TRVersion.Game.TR2X:
                WriteLevelTr2();
                break;
            default:
                throw new NotImplementedException("The selected game engine is not supported yet");
        }

        ReportProgress(98, "Writing TRX data");

        var injData = new TrxInjectionData();
        injData.SectorEdits.AddRange(GenerateTrxSectorEdits());

        using var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Append));
        TrxInjector.Serialize(injData, writer);
    }

    private IEnumerable<TrxSectorEdit> GenerateTrxSectorEdits()
    {
        foreach (var (teRoom, trRoom) in _tempRooms)
        {
            for (ushort x = 1; x < teRoom.NumXSectors - 1; x++)
            {
                for (ushort z = 1; z < teRoom.NumZSectors - 1; z++)
                {
                    if (GetSectorOverwrite(teRoom, trRoom, x, z) is TrxSectorEdit overwriteEdit)
                    {
                        yield return overwriteEdit;
                    }
                    if (GetClimbEntry(teRoom, x, z) is TrxSectorEdit climbEdit)
                    {
                        yield return climbEdit;
                    }
                    if (GetTriangulation(teRoom, x, z) is TrxSectorEdit triangulationEdit)
                    {
                        yield return triangulationEdit;
                    }
                }
            }
        }
    }

    private TrxSectorOverwrite GetSectorOverwrite(Room teRoom, tr_room trRoom, ushort x, ushort z)
    {
        var teSector = teRoom.Sectors[x, z];
        var roomBelow = GetPortalRoom(teSector.FloorPortal);
        var roomAbove = GetPortalRoom(teSector.CeilingPortal);
        
        if (roomBelow < _legacyRoomLimit && roomAbove < _legacyRoomLimit)
        {
            return null;
        }

        return new()
        {
            RoomIndex = (short)_roomRemapping[teRoom],
            X = x,
            Z = z,
            BaseSector = trRoom.Sectors[x * teRoom.NumZSectors + z],
            RoomAboveExt = (short)roomAbove,
            RoomBelowExt = (short)roomBelow,
        };
    }

    private int GetPortalRoom(PortalInstance portal)
    {
        return portal != null && portal.Opacity != PortalOpacity.SolidFaces
            ? _roomRemapping[portal.AdjoiningRoom]
            : _noRoom;
    }

    private TrxClimbEntry GetClimbEntry(Room teRoom, ushort x, ushort z)
    {
        if (_level.Settings.GameVersion != TRVersion.Game.TR1X)
        {
            return null;
        }

        var teSector = teRoom.Sectors[x, z];
        if ((teSector.Flags & SectorFlags.ClimbAny) == SectorFlags.None)
        {
            return null;
        }

        return new()
        {
            RoomIndex = (short)_roomRemapping[teRoom],
            X = x,
            Z = z,
            Flags = teSector.Flags,
        };
    }

    private TrxTriangulationEntry GetTriangulation(Room teRoom, ushort x, ushort z)
    {
        var teSector = teRoom.Sectors[x, z];
        if (teSector.IsAnyWall)
        {
            return null;
        }

        var pos = new VectorInt2(x, z);
        var floorPortalType = teRoom.GetFloorRoomConnectionInfo(pos, true).TraversableType;
        var ceilingPortalType = teRoom.GetCeilingRoomConnectionInfo(pos, true).TraversableType;
        var floorShape = new RoomSectorShape(teSector, true, floorPortalType, false);
        var ceilingShape = new RoomSectorShape(teSector, false, ceilingPortalType, false);

        if (!floorShape.IsSplit && !ceilingShape.IsSplit)
        {
            return null;
        }

        var result = new TrxTriangulationEntry
        {
            RoomIndex = (short)_roomRemapping[teRoom],
            X = x,
            Z = z,
        };
        
        var lastFunction = 0;
        if (floorShape.IsSplit)
        {
            result.Floor = new();
            BuildFloorDataCollision(floorShape, ceilingShape.Max, false, result.Floor, ref lastFunction,
                teRoom, pos, _level.Settings.GameVersion);
        }
        if (ceilingShape.IsSplit)
        {
            result.Ceiling = new();
            BuildFloorDataCollision(ceilingShape, floorShape.Min, true, result.Ceiling, ref lastFunction,
                teRoom, pos, _level.Settings.GameVersion);
        }

        return result;
    }
}

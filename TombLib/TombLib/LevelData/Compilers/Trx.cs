using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TombLib.IO;
using TombLib.LevelData.Compilers.Util;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.Compilers;

public partial class LevelCompilerClassicTR
{
    private const int _legacyRoomLimit = 255;
    private const int _noRoom = -1;
    private const int _maxSamples = 1000;

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
        injData.TexPages.AddRange(GenerateTrxTexPages());
        injData.SFX.AddRange(GenerateTrxSFXData());

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
        var teSector = teRoom.Sectors[x, z];
        var hasLadder = (teSector.Flags & SectorFlags.ClimbAny) != 0;
        var hasMonkey = (teSector.Flags & SectorFlags.Monkey) != 0;
        if (!hasLadder && !hasMonkey)
        {
            return null;
        }

        if (_level.Settings.GameVersion == TRVersion.Game.TR2X && !hasMonkey)
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
        if (teSector.IsFullWall)
        {
            return null;
        }

        var pos = new VectorInt2(x, z);
        var floorPortalType = teRoom.GetFloorRoomConnectionInfo(pos, true).TraversableType;
        var ceilingPortalType = teRoom.GetCeilingRoomConnectionInfo(pos, true).TraversableType;
        var floorShape = new RoomSectorShape(teSector, true, floorPortalType, teSector.IsAnyWall);
        var ceilingShape = new RoomSectorShape(teSector, false, ceilingPortalType, teSector.IsAnyWall);

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

    private IEnumerable<TrxTextureOverwrite> GenerateTrxTexPages()
    {
        var depth = _level.Settings.TrxTextureBitDepth;
        var version = _level.Settings.GameVersion;

        if (depth == TrxTextureBitDepth.Default)
            yield break;

        if (version == TRVersion.Game.TR1X && depth == TrxTextureBitDepth.Bit8)
            yield break;

        if (version == TRVersion.Game.TR2X && depth == TrxTextureBitDepth.Bit16)
            yield break;

        const int size = 256 * 256;
        const int bpp32 = 4;
        const int pageSize32 = size * bpp32;

        int numPages = _texture32Data.Length / pageSize32;

        byte[] data8 = null;
        ushort[] data16 = null;
        tr_color[] palette = null;

        if (depth == TrxTextureBitDepth.Bit8)
        {
            data8 = PackTextureMap32To8Bit(_texture32Data, new List<Color> { Color.FromArgb(2, 0, 0) }, out palette);
        }
        else if (depth == TrxTextureBitDepth.Bit16)
        {
            var packed = PackTextureMap32To16Bit(_texture32Data, _level.Settings);
            data16 = new ushort[size * numPages];
            Buffer.BlockCopy(packed, 0, data16, 0, data16.Length * sizeof(ushort));
        }

        for (ushort page = 0; page < numPages; page++)
        {
            var pixels = depth switch
            {
                TrxTextureBitDepth.Bit8 => Build8BitPage(page, size, data8, palette),
                TrxTextureBitDepth.Bit16 => Build16BitPage(page, size, data16),
                _ => Build32BitPage(page, size, pageSize32),
            };
            yield return new()
            {
                Page = page,
                Data = pixels,
            };
        }
    }

    private static uint[] Build8BitPage(int page, int size, byte[] data, tr_color[] palette)
    {
        var pixels = new uint[size];
        var baseIndex = page * size;

        const int scaleShift = 2; // Undo PackTextureMap32To8Bit component division
        for (var i = 0; i < size; i++)
        {
            var idx = data[baseIndex + i];
            if (idx == 0)
                continue;

            var c = palette[idx];
            pixels[i] = (0xFFu << 24) |
                ((uint)c.Blue << (16 + scaleShift)) |
                ((uint)c.Green << (8 + scaleShift)) |
                ((uint)c.Red << scaleShift);
        }

        return pixels;
    }

    private static uint[] Build16BitPage(int page, int size, ushort[] data)
    {
        var pixels = new uint[size];
        var baseIndex = page * size;

        for (var i = 0; i < size; i++)
        {
            var c = data[baseIndex + i];

            var a = (c & 0x8000) != 0 ? 0xFF : 0;
            var r = ((c >> 10) & 0x1F) * 255 / 31;
            var g = ((c >> 5) & 0x1F) * 255 / 31;
            var b = (c & 0x1F) * 255 / 31;

            pixels[i] = (uint)((a << 24) | (b << 16) | (g << 8) | r);
        }

        return pixels;
    }

    private uint[] Build32BitPage(int page, int size, int pageSize)
    {
        var pixels = new uint[size];
        Buffer.BlockCopy(_texture32Data, page * pageSize, pixels, 0, pageSize);

        for (var i = 0; i < size; i++)
        {
            // Swap Red and Blue channels
            var c = pixels[i];
            pixels[i] =
                (c & 0xFF00FF00) | // A + G
                ((c & 0x000000FF) << 16) |
                ((c & 0x00FF0000) >> 16);
        }

        return pixels;
    }

    private IEnumerable<TrxSFXData> GenerateTrxSFXData()
    {
        var samples = new Queue<Wad.WadSample>(_finalSamplesList);
        var sampleCount = 0;
        for (int i = 0; i < _finalSoundMap.Length; i++)
        {
            if (_finalSoundMap[i] == -1)
                continue;

            var soundInfo = _finalSoundInfosList[_finalSoundMap[i]];
            var details = GetTR12SoundDetails(soundInfo);
            var data = TrxSFXData.Create(i, details);
            data.Samples.AddRange(
                Enumerable.Range(0, soundInfo.Samples.Count)
                .Select(_ => samples.Dequeue().Data));
            sampleCount += data.Samples.Count;
            yield return data;
        }

        if (sampleCount > _maxSamples)
            _progressReporter.ReportWarn($"{sampleCount} samples included - limit is {_maxSamples}. This may lead to crashes.");
    }
}

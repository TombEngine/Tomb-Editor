using System;
using System.Collections.Generic;
using System.IO;
using TombLib.IO;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util;

public static class TrxInjector
{
    private const uint _magic = 'T' | 'R' << 8 | 'X' << 16 | 'J' << 24;
    private const uint _version = 4;
    private const uint _injectionType = 0; // Implies no link to a TRX config option

    public static void Serialize(TrxInjectionData data, BinaryWriterEx outWriter)
    {
        using var stream = new MemoryStream();
        using var injWriter = new BinaryWriterEx(stream);

        if (!WriteData(data, injWriter))
        {
            return;
        }

        var exportedData = stream.ToArray();
        var zippedData = ZLib.CompressData(exportedData);

        outWriter.Write(_magic);
        outWriter.Write(_version);
        outWriter.Write(_injectionType);

        outWriter.Write(exportedData.Length);
        outWriter.Write(zippedData.Length);
        outWriter.Write(zippedData);
    }

    private static bool WriteData(TrxInjectionData data, BinaryWriterEx writer)
    {
        var chunks = new List<TrxChunk>()
        {
            CreateChunk(TrxChunkType.DataEdits, data, WriteEdits),
        };

        chunks.RemoveAll(c => c.BlockCount == 0);
        if (chunks.Count == 0)
        {
            return false;
        }

        // Regular injections have applicability tests for OG levels. This is irrelevant
        // for embedded injections.
        writer.Write(0); // Number of tests
        writer.Write(0); // Total length of tests

        writer.Write(chunks.Count);
        chunks.ForEach(c => c.Serialize(writer));
        return true;
    }

    private static TrxChunk CreateChunk(TrxChunkType type,
        TrxInjectionData data, Func<TrxInjectionData, BinaryWriterEx, int> process)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriterEx(stream);
        int blockCount = process(data, writer);

        return new()
        {
            Type = type,
            BlockCount = blockCount,
            Data = stream.ToArray(),
        };
    }

    private static int WriteEdits(TrxInjectionData data, BinaryWriterEx writer)
    {
        int blockCount = 0;

        blockCount += WriteBlock(TrxBlockType.SectorEdits, data.SectorEdits.Count, writer,
            w => data.SectorEdits.ForEach(s => s.Serialize(w)));

        return blockCount;
    }

    private static int WriteBlock(TrxBlockType type, int elementCount,
        BinaryWriterEx writer, Action<BinaryWriterEx> subCallback)
    {
        if (elementCount == 0)
        {
            return 0;
        }

        using var stream = new MemoryStream();
        using var subWriter = new BinaryWriterEx(stream);
        subCallback(subWriter);
        subWriter.Flush();

        var data = stream.ToArray();
        writer.Write((int)type);
        writer.Write(elementCount);
        writer.Write(data.Length);
        writer.Write(data);

        return 1;
    }

    private class TrxChunk
    {
        public TrxChunkType Type { get; set; }
        public int BlockCount { get; set; }
        public byte[] Data { get; set; }

        public void Serialize(BinaryWriterEx writer)
        {
            writer.Write((int)Type);
            writer.Write(BlockCount);
            writer.Write(Data.Length);
            writer.Write(Data);
        }
    }

    // Only relevant values currently for TE
    private enum TrxChunkType
    {
        DataEdits = 6,
    }

    private enum TrxBlockType
    {
        SectorEdits = 17,
    }
}

public class TrxInjectionData
{
    public List<TrxSectorEdit> SectorEdits { get; set; } = new();
}

public abstract class TrxSectorEdit
{
    public abstract int Command { get; }
    public short RoomIndex { get; set; }
    public ushort X { get; set; }
    public ushort Z { get; set; }

    public void Serialize(BinaryWriterEx writer)
    {
        writer.Write(RoomIndex);
        writer.Write(X);
        writer.Write(Z);
        writer.Write(1); // "edit" count
        writer.Write(Command);
        SerializeImpl(writer);
    }

    protected abstract void SerializeImpl(BinaryWriterEx writer);
}

public class TrxSectorOverwrite : TrxSectorEdit
{
    public override int Command => 7;
    public tr_room_sector BaseSector { get; set; }
    public short RoomBelowExt { get; set; }
    public short RoomAboveExt { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        // TRX uses -1 for NO_ROOM and supports up to 1024 rooms, including
        // vertical portal support. The engine expects all values for the sector
        // here, including heights being in world units.
        writer.Write(BaseSector.FloorDataIndex);
        writer.Write(BaseSector.BoxIndex);
        writer.Write(RoomBelowExt);
        writer.Write((short)(BaseSector.Floor * Level.FullClickHeight));
        writer.Write(RoomAboveExt);
        writer.Write((short)(BaseSector.Ceiling * Level.FullClickHeight));
    }
}

public class TrxClimbEntry : TrxSectorEdit
{
    public override int Command => 11;
    public SectorFlags Flags { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        var direction = 0;
        direction |= Convert.ToInt32(Flags.HasFlag(SectorFlags.ClimbPositiveZ)) << 0;
        direction |= Convert.ToInt32(Flags.HasFlag(SectorFlags.ClimbPositiveX)) << 1;
        direction |= Convert.ToInt32(Flags.HasFlag(SectorFlags.ClimbNegativeZ)) << 2;
        direction |= Convert.ToInt32(Flags.HasFlag(SectorFlags.ClimbNegativeX)) << 3;
        writer.Write(direction);
    }
}

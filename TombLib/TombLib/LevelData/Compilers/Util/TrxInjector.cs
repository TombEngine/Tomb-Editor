using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TombLib.IO;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util;

public static class TrxInjector
{
    private const uint _magic = 'T' | 'R' << 8 | 'X' << 16 | 'J' << 24;
    private const uint _version = 9;
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

    public static byte[] Encode(string text)
        => Encoding.UTF8.GetBytes(text ?? string.Empty);

    private static bool WriteData(TrxInjectionData data, BinaryWriterEx writer)
    {
        var chunks = new List<TrxChunk>()
        {
            CreateChunk(TrxChunkType.CameraData, data, WriteCameraData),
            CreateChunk(TrxChunkType.DataEdits, data, WriteEdits),
            CreateChunk(TrxChunkType.SFX, data, WriteSFXData),
            CreateChunk(TrxChunkType.PathingData, data, WritePathingData),
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

    private static int WriteCameraData(TrxInjectionData data, BinaryWriterEx writer)
    {
        int blockCount = 0;

        blockCount += WriteBlock(TrxBlockType.FlybyCameras, data.FlybyCameras.Count, writer,
            w => data.FlybyCameras.ForEach(c =>
            {
                w.Write(c.X);
                w.Write(c.Y);
                w.Write(c.Z);
                w.Write(c.DirectionX);
                w.Write(c.DirectionY);
                w.Write(c.DirectionZ);
                w.Write(c.Sequence);
                w.Write(c.Index);
                w.Write(c.FOV);
                w.Write(c.Roll);
                w.Write(c.Timer);
                w.Write(c.Speed);
                w.Write(c.Flags);
                w.Write(c.Room);
            }));

        return blockCount;
    }

    private static int WriteEdits(TrxInjectionData data, BinaryWriterEx writer)
    {
        int blockCount = 0;

        blockCount += WriteBlock(TrxBlockType.SectorEdits, data.SectorEdits.Count, writer,
            w => data.SectorEdits.ForEach(s => s.Serialize(w)));
        blockCount += WriteBlock(TrxBlockType.TextureOverwrites, data.TexPages.Count, writer,
            w => data.TexPages.ForEach(t => t.Serialize(w)));
        blockCount += WriteBlock(TrxBlockType.ItemNameEdits, data.ItemNameEdits.Count, writer,
            w => data.ItemNameEdits.ForEach(t => t.Serialize(w)));
        blockCount += WriteBlock(TrxBlockType.PropertyEdits, data.PropertyEdits.Count, writer,
            w => data.PropertyEdits.ForEach(p => p.Serialize(w)));

        return blockCount;
    }

    private static int WriteSFXData(TrxInjectionData data, BinaryWriterEx writer)
    {
        return WriteBlock(TrxBlockType.SoundEffects, data.SFX.Count, writer,
            s => data.SFX.ForEach(f => f.Serialize(s)));
    }

    private static int WritePathingData(TrxInjectionData data, BinaryWriterEx writer)
    {
        return WriteBlock(TrxBlockType.OverlapIndices, data.OverlapIndices.Count, writer,
            w => data.OverlapIndices.ForEach(i => w.Write(i)));
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
        SFX = 5,
        DataEdits = 6,
        CameraData = 7,
        PathingData = 9,
    }

    private enum TrxBlockType
    {
        SoundEffects = 14,
        SectorEdits = 17,
        TextureOverwrites = 20,
        ItemNameEdits = 37,
        FlybyCameras = 38,
        PropertyEdits = 39,
        OverlapIndices = 44,
    }
}

public class TrxInjectionData
{
    public List<tr4_flyby_camera> FlybyCameras { get; set; } = new();
    public List<TrxSectorEdit> SectorEdits { get; set; } = new();
    public List<TrxTextureOverwrite> TexPages { get; set; } = new();
    public List<TrxSFXData> SFX { get; set; } = new();
    public List<TrxItemNameEdit> ItemNameEdits = new();
    public List<TrxPropertyEdit> PropertyEdits { get; set; } = new();
    public List<uint> OverlapIndices { get; set; } = new();
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
        direction |= Convert.ToInt32(Flags.HasFlag(SectorFlags.Monkey)) << 4;
        writer.Write(direction);
    }
}

public enum TrxMineCartType
{
    None,
    Left,
    Right,
    Stop,
}

public class TrxMineCartEntry : TrxSectorEdit
{
    public override int Command => 14;

    public TrxMineCartType Type { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write((int)Type);
    }
}

public class TrxTriangulationEntry : TrxSectorEdit
{
    public override int Command => 13;

    public List<ushort> Floor { get; set; }
    public List<ushort> Ceiling { get; set; }
    
    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        var type = 0;
        var data = new List<ushort>();
        if (Floor != null)
        {
            type |= 1 << 0;
            data.AddRange(Floor);
        }
        if (Ceiling != null)
        {
            type |= 1 << 1;
            data.AddRange(Ceiling);
        }

        writer.Write(type);
        foreach (var val in data)
        {
            writer.Write(val);
        }
    }
}

public class TRXRoomPropertyEntry : TrxSectorEdit
{
    public override int Command => 5;

    public short Flags { get; set; }
    public byte ReverbInfo { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(Flags);
        writer.Write(ReverbInfo);
    }
}

public class TrxTextureOverwrite
{
    public ushort Page { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public ushort Width { get; set; } = 256;
    public ushort Height { get; set; } = 256;
    public uint[] Data { get; set; }

    public void Serialize(BinaryWriterEx writer)
    {
        writer.Write(Page);
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Width);
        writer.Write(Height);
        for (int i = 0; i < Data.Length; i++)
        {
            writer.Write(Data[i]);
        }
    }
}

public class TrxSFXData
{
    public int ID { get; set; }
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public ushort Characteristics { get; set; }
    public byte Pitch { get; set; }
    public byte Range { get; set; } = 10;
    public List<byte[]> Samples { get; set; } = new();

    public void Serialize(BinaryWriterEx writer)
    {
        writer.Write((short)ID);
        writer.Write(Volume);
        writer.Write(Chance);
        writer.Write(Characteristics);
        writer.Write(Range * 1024);
        writer.Write(Pitch);
        foreach (var sample in Samples)
        {
            writer.Write(sample.Length);
            writer.Write(sample);
        }
    }

    public static TrxSFXData Create(int id, tr_sound_details details)
    {
        return new()
        {
            ID = id,
            Volume = details.Volume,
            Chance = details.Chance,
            Characteristics = details.Characteristics,
        };
    }

    public static TrxSFXData Create(int id, tr3_sound_details details)
    {
        return new()
        {
            ID = id,
            Volume = (ushort)(details.Volume << 7),
            Chance = details.Chance,
            Characteristics = details.Characteristics,
            Pitch = details.Pitch,
            Range = details.Range,
        };
    }
}

public class TrxItemNameEdit
{
    public short Index { get; set; }
    public string Name { get; set; }

    public void Serialize(BinaryWriterEx writer)
    {
        var data = TrxInjector.Encode(Name);
        writer.Write(Index);
        writer.Write(data.Length);
        writer.Write(data);
    }
}

public enum TrxPropertyTarget
{
    Object,
    Item,
}

public enum TrxPropertyType
{
    Int,
    Float,
    Double,
    Bool,
    XYZ,
}

public abstract class TrxPropertyEdit
{
    public abstract TrxPropertyTarget Type { get; }
    public List<TrxProperty> Properties { get; set; } = new();

    public void Serialize(BinaryWriterEx writer)
    {
        writer.Write((int)Type);
        SerializeImpl(writer);
        writer.Write(Properties.Count);
        Properties.ForEach(p => p.Serialize(writer));
    }

    protected abstract void SerializeImpl(BinaryWriterEx writer);
}

public class TrxObjectPropertyEdit : TrxPropertyEdit
{
    public override TrxPropertyTarget Type => TrxPropertyTarget.Object;
    public int ObjectId { get; set; }

    public TrxObjectPropertyEdit(int objectId)
    {
        ObjectId = objectId;
    }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(0); // object type = game
        writer.Write(ObjectId);
    }
}

public class TrxItemPropertyEdit : TrxPropertyEdit
{
    public override TrxPropertyTarget Type => TrxPropertyTarget.Item;
    public int ItemIndex { get; set; }

    public TrxItemPropertyEdit(int index)
    {
        ItemIndex = index;
    }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(ItemIndex);
    }
}

public abstract class TrxProperty
{
    public abstract TrxPropertyType Type { get; }
    public string Name { get; set; }

    public void Serialize(BinaryWriterEx writer)
    {
        var name = TrxInjector.Encode(Name);
        writer.Write(name.Length);
        writer.Write(name);
        writer.Write((int)Type);
        SerializeImpl(writer);
    }

    protected abstract void SerializeImpl(BinaryWriterEx writer);
}

public class TrxBoolProperty : TrxProperty
{
    public override TrxPropertyType Type => TrxPropertyType.Bool;
    public bool Value { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(Value ? 1 : 0);
    }
}

public class TrxFloatProperty : TrxProperty
{
    public override TrxPropertyType Type => TrxPropertyType.Float;
    public float Value { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(Value);
    }
}

public class TrxIntProperty : TrxProperty
{
    public override TrxPropertyType Type => TrxPropertyType.Int;
    public int Value { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(Value);
    }
}

public class TrxXYZProperty : TrxProperty
{
    public override TrxPropertyType Type => TrxPropertyType.XYZ;
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    protected override void SerializeImpl(BinaryWriterEx writer)
    {
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Z);
    }
}

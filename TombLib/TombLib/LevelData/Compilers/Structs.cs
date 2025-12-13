using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.IO;

namespace TombLib.LevelData.Compilers
{
    public enum AlternateKind
    {
        NotAlternated,
        BaseRoom,
        AlternateRoom
    }

    public class ShadeMatchSignature
    {
        public bool IsWater;
        public int AlternateGroup;
        public Vector3 Position;

        public override int GetHashCode()
        {
            if (!_hash.HasValue)
                _hash = (Position.X + ";" + Position.Y + ";" + Position.Z + ";" + AlternateGroup + ";" + (IsWater ? 1 : 0)).GetHashCode();
            return _hash.Value;
        }
        private int? _hash = null;

        public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_color
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public void write(BinaryWriter writer) {
            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_vertex
    {
        public short X;
        public short Y;
        public short Z;

        public tr_vertex(short X, short Y, short Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static bool operator ==(tr_vertex first, tr_vertex second)
        {
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        }

        public static bool operator !=(tr_vertex first, tr_vertex second)
        {
            return !(first == second);
        }

        public bool Equals(tr_vertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is tr_vertex))
                return false;
            return this == (tr_vertex)obj;
        }

        public override int GetHashCode()
        {
            return unchecked(X + Y * 695504311 + Z * 550048883);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_face4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public ushort[] Vertices;
        public ushort Texture;
        public ushort LightingEffect;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(Vertices[0]);
            writer.Write(Vertices[1]);
            writer.Write(Vertices[2]);
            writer.Write(Vertices[3]);
            writer.Write(Texture);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_face3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public ushort[] Vertices;
        public ushort Texture;
        public ushort LightingEffect;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(Vertices[0]);
            writer.Write(Vertices[1]);
            writer.Write(Vertices[2]);
            writer.Write(Texture);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_info
    {
        public int X;
        public int Z;
        public int YBottom;
        public int YTop;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_portal
    {
        public ushort AdjoiningRoom;
        public tr_vertex Normal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public tr_vertex[] Vertices;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_sector
    {
        public ushort FloorDataIndex;
        public ushort BoxIndex;
        public byte RoomBelow;
        public sbyte Floor;
        public byte RoomAbove;
        public sbyte Ceiling;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr4_room_light
    {
        public int X;
        public int Y;
        public int Z;
        public tr_color Color;
        public byte LightType;
        public ushort Intensity;
        public float In;
        public float Out;
        public float Length;
        public float CutOff;
        public float DirectionX;
        public float DirectionY;
        public float DirectionZ;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_vertex : IEquatable<tr_room_vertex>
    {
        public tr_vertex Position;
        public ushort Lighting1;
        public ushort Attributes;
        public ushort Lighting2;

        // For TR5 only
        public Vector3 Normal;
        public uint Color;

        // Used internally by Tomb Editor
        public bool IsOnPortal;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static unsafe bool operator ==(tr_room_vertex first, tr_room_vertex second)
        {
            return *(ulong*)&first == *(ulong*)&second && *(uint*)&first.Attributes == *(uint*)&second.Attributes;
        }

        public static bool operator !=(tr_room_vertex first, tr_room_vertex second)
        {
            return !(first == second);
        }

        public bool Equals(tr_room_vertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is tr_room_vertex))
                return false;
            return this == (tr_room_vertex)obj;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 37) ^ Position.X;
            hash = (hash * 37) ^ Position.Y;
            hash = (hash * 37) ^ Position.Z;
            hash = (hash * 37) ^ Lighting1;
            hash = (hash * 37) ^ Attributes;
            return hash;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_staticmesh
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Rotation;
        public ushort Intensity1;
        public ushort Intensity2;
        public ushort ObjectID;
    }

    public struct tr_room_sprite
    {
        public int Vertex;
        public int SpriteID;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write((ushort)Vertex);
            writer.Write((ushort)SpriteID);
        }
    }

    public class tr_room
    {
        public tr_room_info Info;
        public uint NumDataWords;
        public List<tr_room_vertex> Vertices;
        public List<tr_face4> Quads;
        public List<tr_face3> Triangles;
        public List<tr_room_sprite> Sprites;
        public List<tr_room_portal> Portals;
        public ushort NumZSectors;
        public ushort NumXSectors;
        public tr_room_sector[] Sectors;
        public uint AmbientIntensity;
        public short LightMode;
        public List<tr4_room_light> Lights;
        public List<tr_room_staticmesh> StaticMeshes;
        public short AlternateRoom;
        public short Flags;
        public byte WaterScheme;
        public byte ReverbInfo;
        public byte AlternateGroup;

        // Helper data
        public TrSectorAux[,] AuxSectors;
        public AlternateKind AlternateKind;
        public List<Room> ReachableRooms;
        public bool Visited;
        public bool Flipped;
        public Room FlippedRoom;
        public Room BaseRoom;
        public Room OriginalRoom;

        public void WriteTr1(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;

            writer.Write(0);

            writer.Write((ushort)Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Lighting1);
            }

            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);

            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // Sprites
            writer.Write((ushort)Sprites.Count);
            Sprites.ForEach(s => s.Write(writer));

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            writer.WriteBlockArray(Sectors);

            // Write room color
            writer.Write((ushort)(AmbientIntensity));

            // Write lights
            writer.WriteBlock((ushort)Lights.Count);
            if (Lights.Count != 0)
            {
                foreach (var light in Lights)
                {
                    writer.Write(light.X);
                    writer.Write(light.Y);
                    writer.Write(light.Z);
                    writer.Write(light.Intensity);
                    writer.Write((uint)(light.Out / 2));
                }
            }

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Count);
            if (StaticMeshes.Count != 0)
            {
                foreach (var sm in StaticMeshes)
                {
                    writer.Write(sm.X);
                    writer.Write(sm.Y);
                    writer.Write(sm.Z);
                    writer.Write(sm.Rotation);
                    writer.Write(sm.Intensity1);
                    writer.Write(sm.ObjectID);
                }
            }

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
        }

        public void WriteTr2(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;

            writer.Write(0);

            writer.Write((ushort)Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Lighting1);
                writer.Write(Vertices[k].Attributes);
                writer.Write(Vertices[k].Lighting2);
            }

            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);

            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // Sprites
            writer.Write((ushort)Sprites.Count);
            Sprites.ForEach(s => s.Write(writer));

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            writer.WriteBlockArray(Sectors);

            // Write room color
            writer.Write((ushort)(AmbientIntensity));
            writer.Write((ushort)(AmbientIntensity));

            // TODO: Light mode
            writer.Write(LightMode);

            // Write lights
            writer.WriteBlock((ushort)Lights.Count);
            if (Lights.Count != 0)
            {
                foreach (var light in Lights)
                {
                    writer.Write(light.X);
                    writer.Write(light.Y);
                    writer.Write(light.Z);
                    writer.Write(light.Intensity);
                    writer.Write(light.Intensity);
                    writer.Write((uint)light.Out / 2);
                    writer.Write((uint)light.Out / 2);
                }
            }

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Count);
            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
        }

        public void WriteTr3(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;

            writer.Write(0);

            writer.Write((ushort)Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Lighting1);
                writer.Write(Vertices[k].Attributes);
                writer.Write(Vertices[k].Lighting2);
            }

            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);

            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // Sprites, not used
            writer.Write((ushort)0);

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            writer.WriteBlockArray(Sectors);

            // Write room color
            writer.Write((ushort)(AmbientIntensity));

            // Light mode is broken in TR3
            writer.Write((ushort)0x00);

            // Write lights
            writer.WriteBlock((ushort)Lights.Count);
            if (Lights.Count != 0)
            {
                foreach (var light in Lights)
                {
                    writer.Write(light.X);
                    writer.Write(light.Y);
                    writer.Write(light.Z);
                    writer.Write(light.Color.Red);
                    writer.Write(light.Color.Green);
                    writer.Write(light.Color.Blue);

                    // Sun type is 1 and point light is 0 in TR3
                    writer.Write((byte)(light.LightType == 0 ? 1 : 0));

                    if (light.LightType == 0) // TR3 sun type
                    {
                        writer.Write((ushort)(light.DirectionX * Level.SectorSizeUnit));
                        writer.Write((ushort)(light.DirectionY * Level.SectorSizeUnit));
                        writer.Write((ushort)(light.DirectionZ * Level.SectorSizeUnit));
                        writer.Write((ushort)0x0000); // Padding
                    }
                    else
                    {
                        writer.Write((uint)light.Intensity);
                        writer.Write((uint)(light.Out / 2));
                    }
                }
            }

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Count);
            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write(WaterScheme);
            writer.Write(ReverbInfo);
            writer.Write((byte)0x00); // Alternate group was introduced in TR4
        }

        public void WriteTr4(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;

            writer.Write(0);

            writer.Write((ushort)Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Lighting1);
                writer.Write(Vertices[k].Attributes);
                writer.Write(Vertices[k].Lighting2);
            }

            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);

            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // Sprites, not used
            writer.Write((ushort)0);

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            writer.WriteBlockArray(Sectors);

            // Write room color
            writer.Write(AmbientIntensity);

            // Write lights
            writer.WriteBlock((ushort)Lights.Count);
            if (Lights.Count != 0)
                writer.WriteBlockArray(Lights);

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Count);
            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write(WaterScheme);
            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);
        }

        public void WriteTr5(BinaryWriterEx writer)
        {
            var lights = Lights.Where(l => l.LightType != 4).ToList();
            var bulbs  = Lights.Where(l => l.LightType == 4).ToList();

            var roomStartOffset = writer.BaseStream.Position;

            var xela = System.Text.Encoding.ASCII.GetBytes("XELA");
            writer.Write(xela);

            var startOfRoomPosition = writer.BaseStream.Position;
            var roomDataSize = 0;
            writer.Write((uint)roomDataSize);

            var separator = 0xcdcdcdcd;
            writer.Write(separator);

            var EndSDOffsetPosition = writer.BaseStream.Position;
            var EndSDOffset = (uint)0;
            writer.Write(EndSDOffset);

            var StartOfSDOffsetPosition = writer.BaseStream.Position;
            var StartOfSDOffset = (uint)0;
            writer.Write(StartOfSDOffset);

            writer.Write(separator);

            var EndPortalOffsetPosition = writer.BaseStream.Position;
            var EndPortalOffset = (uint)0;
            writer.Write(EndPortalOffset);

            // tr5_room_info
            writer.Write(Info.X);
            writer.Write(0);
            writer.Write(Info.Z);
            writer.Write(Info.YBottom);
            writer.Write(Info.YTop);

            writer.Write(NumZSectors);
            writer.Write(NumXSectors);

            writer.Write(AmbientIntensity);

            writer.Write((ushort)lights.Count);
            writer.Write((ushort)StaticMeshes.Count);

            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);
            writer.Write(WaterScheme);
            writer.Write((byte)0);

            writer.Write((uint)0x00007fff);
            writer.Write((uint)0x00007fff);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xffffffff);

            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write((ushort)0xffff);

            writer.Write((ushort)0);
            writer.Write((uint)0);
            writer.Write((uint)0);

            writer.Write(0xcdcdcdcd);
            writer.Write((uint)0);

            writer.Write((float)Info.X);
            writer.Write((float)0);
            writer.Write((float)Info.Z);

            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write((uint)0);
            writer.Write(0xcdcdcdcd);

            writer.Write((uint)Triangles.Count);
            writer.Write((uint)Quads.Count);

            var LightPointerOffsetPosition = writer.BaseStream.Position;
            writer.Write((uint)0);
            writer.Write((uint)0);

            writer.Write((uint)lights.Count);
            writer.Write((uint)bulbs.Count);

            writer.Write((float)Info.YTop);
            writer.Write((float)Info.YBottom);

            writer.Write((uint)1); // Dummy layer

            var LayerOffsetPosition = writer.BaseStream.Position;
            var LayerOffset = 0;
            writer.Write(LayerOffset);

            var VerticesOffsetPosition = writer.BaseStream.Position;
            var VerticesOffset = 0;
            writer.Write(VerticesOffset);

            var PolyOffsetPosition = writer.BaseStream.Position;
            var PolyOffset = 0;
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Write((uint)(Vertices.Count * 28));

            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);

            // Start of room data (after 216 bytes from XELA)
            var LightPointerOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);
            foreach (var light in lights)
            {
                writer.Write((float)light.X);
                writer.Write((float)light.Y);
                writer.Write((float)light.Z);
                writer.Write(light.Color.Red / 255.0f);
                writer.Write(light.Color.Green / 255.0f);
                writer.Write(light.Color.Blue / 255.0f);

                // TR5 keeps shadow intensity separately
                if (light.LightType == 3)
                    writer.Write((int)((light.Intensity / 8191.0f) * 255.0f));
                else
                    writer.Write(0xcdcdcdcd);

                writer.Write(light.In);
                writer.Write(light.Out);

                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.In) * 2.0f : 0));
                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.Out) * 2.0f : 0));
                writer.Write(light.CutOff);

                writer.Write(-light.DirectionX);
                writer.Write(-light.DirectionY);
                writer.Write(-light.DirectionZ);

                writer.Write(light.X);
                writer.Write(light.Y);
                writer.Write(light.Z);

                writer.Write((int)(-light.DirectionX * 16384.0f));
                writer.Write((int)(-light.DirectionY * 16384.0f));
                writer.Write((int)(-light.DirectionZ * 16384.0f));

                writer.Write(light.LightType);

                writer.Write((byte)0xcd);
                writer.Write((byte)0xcd);
                writer.Write((byte)0xcd);
            }

            var BulbPointerOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);
            foreach (var bulb in bulbs)
            {
                writer.Write((float)bulb.X);
                writer.Write((float)bulb.Y);
                writer.Write((float)bulb.Z);

                writer.Write(bulb.Out);
                writer.Write(bulb.Out * bulb.Out);
                writer.Write(bulb.Length * 65535.0f);

                writer.Write(bulb.Color.Red / 255.0f);
                writer.Write(bulb.Color.Green / 255.0f);
                writer.Write(bulb.Color.Blue / 255.0f);
            }

            StartOfSDOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);
            writer.WriteBlockArray(Sectors);
            EndSDOffset = (uint)(StartOfSDOffset + NumXSectors * NumZSectors * 8);

            writer.Write((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            writer.Write((ushort)0xcdcd);
            EndPortalOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);

            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            LayerOffset = (int)(writer.BaseStream.Position - roomStartOffset - 216);

            // Write just one layer
            writer.Write((uint)Vertices.Count);
            writer.Write((ushort)0);
            writer.Write((ushort)Quads.Count);
            writer.Write((ushort)Triangles.Count);
            writer.Write((ushort)0);

            writer.Write((ushort)0);
            writer.Write((ushort)0);

            writer.Write(Level.SectorSizeUnit);
            writer.Write((float)Info.YBottom);
            writer.Write(Level.SectorSizeUnit);
            writer.Write((NumXSectors - 1) * Level.SectorSizeUnit);
            writer.Write((float)Info.YTop);
            writer.Write((NumZSectors - 1) * Level.SectorSizeUnit);

            writer.Write((uint)0);
            var LayerVerticesOffset = writer.BaseStream.Position;
            writer.Write((uint)0);
            var LayerPolygonsOffset = writer.BaseStream.Position;
            writer.Write((uint)0);
            writer.Write((uint)0);

            PolyOffset = LayerOffset + 56;

            for (var k = 0; k < Quads.Count; k++)
            {
                Quads[k].Write(writer);
                writer.Write((ushort)0);
            }
            for (var k = 0; k < Triangles.Count; k++)
            {
                Triangles[k].Write(writer);
                writer.Write((ushort)0);
            }

            if (Triangles.Count % 2 != 0)
                writer.Write((ushort)0xcdcd);

            VerticesOffset = (int)(writer.BaseStream.Position - roomStartOffset - 216);

            foreach (var vertex in Vertices)
            {
                writer.Write((float)vertex.Position.X);
                writer.Write((float)vertex.Position.Y);
                writer.Write((float)vertex.Position.Z);
                writer.Write((float)vertex.Normal.X);
                writer.Write((float)vertex.Normal.Y);
                writer.Write((float)vertex.Normal.Z);
                writer.Write(vertex.Color);
            }

            var endOfRoomOffset = writer.BaseStream.Position;
            roomDataSize = (int)(endOfRoomOffset - roomStartOffset - 8);

            writer.Seek((int)startOfRoomPosition, SeekOrigin.Begin);
            writer.Write(roomDataSize);

            writer.Seek((int)EndSDOffsetPosition, SeekOrigin.Begin);
            writer.Write((int)EndSDOffset);
            writer.Write((int)StartOfSDOffset);

            writer.Seek((int)EndPortalOffsetPosition, SeekOrigin.Begin);
            writer.Write((int)EndPortalOffset);

            writer.Seek((int)LightPointerOffsetPosition, SeekOrigin.Begin);
            writer.Write(LightPointerOffset);
            writer.Write(BulbPointerOffset);

            writer.Seek((int)LayerOffsetPosition, SeekOrigin.Begin);
            writer.Write(LayerOffset);
            writer.Write(VerticesOffset);
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Seek((int)LayerVerticesOffset, SeekOrigin.Begin);
            writer.Write(VerticesOffset);
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Seek((int)endOfRoomOffset, SeekOrigin.Begin);
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_mesh
    {
        public tr_vertex Center;
        public int Radius;
        public short NumVertices;
        public tr_vertex[] Vertices;
        public short NumNormals;
        public tr_vertex[] Normals;
        public short[] Lights;
        public short NumTexturedQuads;
        public tr_face4[] TexturedQuads;
        public short NumTexturedTriangles;
        public tr_face3[] TexturedTriangles;
        public short NumColoredRectangles;
        public tr_face4[] ColoredRectangles;
        public short NumColoredTriangles;
        public tr_face3[] ColoredTriangles;
        public int MeshSize;
        public int MeshPointer;

        public long WriteTr4AndTr5(BinaryWriterEx writer)
        {
            long meshOffset1 = writer.BaseStream.Position;

            writer.WriteBlock(Center);
            writer.Write(Radius);

            writer.Write(NumVertices);
            writer.WriteBlockArray(Vertices);

            writer.Write(NumNormals);
            if (NumNormals > 0)
            {
                writer.WriteBlockArray(Normals);
            }
            else if (NumNormals < 0)
            {
                writer.WriteBlockArray(Lights);
            }

            writer.Write(NumTexturedQuads);
            if (NumTexturedQuads != 0)
                writer.WriteBlockArray(TexturedQuads);

            writer.Write(NumTexturedTriangles);
            if (NumTexturedTriangles != 0)
                writer.WriteBlockArray(TexturedTriangles);

            var meshOffset2 = writer.BaseStream.Position;
            var meshSize = meshOffset2 - meshOffset1;
            if (meshSize % 4 != 0)
            {
                const ushort tempFiller = 0;
                writer.Write(tempFiller);
                meshSize += 2;
            }

            return meshSize;
        }

        public long WriteTr3(BinaryWriterEx writer)
        {
            var meshOffset1 = writer.BaseStream.Position;

            writer.WriteBlock(Center);
            writer.Write(Radius);

            writer.Write(NumVertices);
            writer.WriteBlockArray(Vertices);

            writer.Write(NumNormals);
            if (NumNormals > 0)
            {
                writer.WriteBlockArray(Normals);
            }
            else if (NumNormals < 0)
            {
                writer.WriteBlockArray(Lights);
            }

            writer.Write(NumTexturedQuads);
            for (var k = 0; k < NumTexturedQuads; k++)
            {
                TexturedQuads[k].Write(writer);
            }

            writer.Write(NumTexturedTriangles);
            for (var k = 0; k < NumTexturedTriangles; k++)
            {
                TexturedTriangles[k].Write(writer);
            }

            writer.Write(NumColoredRectangles);
            writer.Write(NumColoredTriangles);

            var meshOffset2 = writer.BaseStream.Position;
            var meshSize = meshOffset2 - meshOffset1;
            if (meshSize % 4 != 0)
            {
                const ushort tempFiller = 0;
                writer.Write(tempFiller);
                meshSize += 2;
            }

            return meshSize;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_staticmesh
    {
        public uint ObjectID;
        public ushort Mesh;
        public tr_bounding_box VisibilityBox;
        public tr_bounding_box CollisionBox;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_bounding_box
    {
        public short X1;
        public short X2;
        public short Y1;
        public short Y2;
        public short Z1;
        public short Z2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_moveable
    {
        public uint ObjectID;
        public ushort NumMeshes;
        public ushort StartingMesh;
        public uint MeshTree;
        public uint FrameOffset;
        public ushort Animation;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_item
    {
        public ushort ObjectID;
        public short Room;
        public int X;
        public int Y;
        public int Z;
        public ushort Angle;
        public ushort Intensity1;
        public short Ocb;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_sprite_texture
    {
        public ushort Tile;
        public byte X;
        public byte Y;
        public ushort Width;
        public ushort Height;
        public short LeftSide;
        public short TopSide;
        public short RightSide;
        public short BottomSide;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_sprite_sequence
    {
        public int ObjectID;
        public short NegativeLength;
        public short Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_meshtree
    {
        public int Opcode;
        public int X;
        public int Y;
        public int Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_animation
    {
        public uint FrameOffset;
        public byte FrameRate;
        public byte FrameSize;
        public ushort StateID;
        public int Speed;
        public int Accel;
        public int SpeedLateral;
        public int AccelLateral;
        public ushort FrameStart;
        public ushort FrameEnd;
        public ushort NextAnimation;
        public ushort NextFrame;
        public ushort NumStateChanges;
        public ushort StateChangeOffset;
        public ushort NumAnimCommands;
        public ushort AnimCommand;

        public void Write(BinaryWriterEx writer, Level level)
        {
            writer.Write(FrameOffset);
            writer.Write(FrameRate);
            if (level.Settings.GameVersion.Native() == TRVersion.Game.TR1)
                writer.Write((byte)0);
            else
                writer.Write(FrameSize);
            writer.Write(StateID);
            writer.Write(Speed);
            writer.Write(Accel);
            if (level.Settings.GameVersion.Native() >= TRVersion.Game.TR4)
            {
                writer.Write(SpeedLateral);
                writer.Write(AccelLateral);
            }
            writer.Write(FrameStart);
            writer.Write(FrameEnd);
            writer.Write(NextAnimation);
            writer.Write(NextFrame);
            writer.Write(NumStateChanges);
            writer.Write(StateChangeOffset);
            writer.Write(NumAnimCommands);
            writer.Write(AnimCommand);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_state_change
    {
        public ushort StateID;
        public ushort NumAnimDispatches;
        public ushort AnimDispatch;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_anim_dispatch
    {
        public ushort Low;
        public ushort High;
        public ushort NextAnimation;
        public ushort NextFrame;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_box
    {
        public byte Zmin;
        public byte Zmax;
        public byte Xmin;
        public byte Xmax;
        public short TrueFloor;
        public ushort OverlapIndex;
    }

    public class TrSectorAux
    {
        public bool Wall;
        public bool SoftSlope;
        public bool HardSlope;
        public bool Monkey;
        public bool Box;
        public bool BorderWall;
        public bool Portal;
        public bool NotWalkableFloor;
        public Room WallPortal;
        public PortalInstance FloorPortal;
        public int LowestFloor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_zone
    {
        public ushort GroundZone1_Normal;
        public ushort GroundZone2_Normal;
        public ushort GroundZone3_Normal;
        public ushort GroundZone4_Normal;
        public ushort FlyZone_Normal;
        public ushort GroundZone1_Alternate;
        public ushort GroundZone2_Alternate;
        public ushort GroundZone3_Alternate;
        public ushort GroundZone4_Alternate;
        public ushort FlyZone_Alternate;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_sound_source
    {
        public int X;
        public int Y;
        public int Z;
        public ushort SoundID;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr3_sound_details
    {
        public ushort Sample;
        public byte Volume;
        public byte Range;
        public byte Chance;
        public byte Pitch;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_sound_details
    {
        public ushort Sample;
        public ushort Volume;
        public ushort Chance;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_camera
    {
        public int X;
        public int Y;
        public int Z;
        public short Room;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_cinematicFrame
    {
        public short TargetX;
        public short TargetY;
        public short TargetZ;
        public short PosX;
        public short PosY;
        public short PosZ;
        public ushort Fov;
        public ushort Roll;

        public static void Write(List<tr_cinematicFrame> frames, BinaryWriter writer)
        {
            writer.Write((ushort)frames.Count);

            foreach (var frame in frames)
            {
                writer.Write(frame.TargetX);
                writer.Write(frame.TargetY);
                writer.Write(frame.TargetZ);
                writer.Write(frame.PosX);
                writer.Write(frame.PosY);
                writer.Write(frame.PosZ);
                writer.Write(frame.Fov);
                writer.Write(frame.Roll);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_animatedTextures
    {
        public short NumTextureID;
        public short[] TextureID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_ai_item
    {
        public ushort ObjectID;
        public ushort Room;
        public int X;
        public int Y;
        public int Z;
        public short OCB;
        public ushort Flags;
        public ushort Angle;
        public ushort Unkown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr4_flyby_camera
    {
        public int X;
        public int Y;
        public int Z;
        public int DirectionX;
        public int DirectionY;
        public int DirectionZ;

        public byte Sequence;
        public byte Index;

        public ushort FOV;
        public short Roll;
        public ushort Timer;
        public ushort Speed;
        public ushort Flags;

        public int Room;

        public class ComparerFlyBy : IComparer<tr4_flyby_camera>
        {
            public int Compare(tr4_flyby_camera x, tr4_flyby_camera y)
            {
                if (x.Sequence != y.Sequence)
                    return x.Sequence > y.Sequence ? 1 : -1;

                if (x.Index == y.Index)
                    return 0;

                return x.Index > y.Index ? 1 : -1;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PluginRecord
    {
        public int PluginId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string Name;

        public uint Usages;
    }
}

using System;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.LuaProperties;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadStaticId : IWadObjectId, IEquatable<WadStaticId>, IComparable<WadStaticId>
    {
        public uint TypeId;

        public WadStaticId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadStaticId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadStaticId)other);
        public static bool operator <(WadStaticId first, WadStaticId second) => first.TypeId < second.TypeId;
        public static bool operator <=(WadStaticId first, WadStaticId second) => first.TypeId <= second.TypeId;
        public static bool operator >(WadStaticId first, WadStaticId second) => first.TypeId > second.TypeId;
        public static bool operator >=(WadStaticId first, WadStaticId second) => first.TypeId >= second.TypeId;
        public static bool operator ==(WadStaticId first, WadStaticId second) => first.TypeId == second.TypeId;
        public static bool operator !=(WadStaticId first, WadStaticId second) => !(first == second);
        public bool Equals(WadStaticId other) => this == other;
        public override bool Equals(object other) => other is WadStaticId && this == (WadStaticId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(TRVersion.Game gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetStaticName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(TRVersion.Game.TR4);
        public string ShortName(TRVersion.Game gameVersion) => TrCatalog.GetStaticName(gameVersion, TypeId);
    }

    public class WadStatic : IWadObject, ICloneable
    {
        public WadStaticId Id { get; private set; }
        public DataVersion Version { get; set; } = DataVersion.GetNext();

        public WadStatic(WadStaticId id)
        {
            Id = id;
            Lights = new List<WadLight>();
        }

        public WadMesh Mesh { get; set; } = WadMesh.Empty;
        public short Flags { get; set; } = 0;
        public BoundingBox VisibilityBox { get; set; } = new BoundingBox();
        public BoundingBox CollisionBox { get; set; } = new BoundingBox();
        public List<WadLight> Lights { get; set; } = new List<WadLight>();
        public short AmbientLight { get; set; } = 128;
        
        public bool Shatter { get; set; } = false;
        public int ShatterSoundID { get; set; } = -1;

        /// <summary>
        /// Level 1 (global) Lua property container. Stored in wad2 files.
        /// Holds per-object-type property overrides in boxed Lua format.
        /// </summary>
        public LuaPropertyContainer LuaProperties { get; set; } = new LuaPropertyContainer();

        public string ToString(TRVersion.Game gameVersion) => Id.ToString(gameVersion.Native());
        public override string ToString() => Id.ToString();

        public WadStatic Clone()
        {
            WadStatic clone = (WadStatic)MemberwiseClone();
            clone.Mesh = Mesh.Clone();
            clone.LuaProperties = LuaProperties.Clone();
            return clone;
        }
        object ICloneable.Clone() => Clone();

        IWadObjectId IWadObject.Id => Id;
    }
}

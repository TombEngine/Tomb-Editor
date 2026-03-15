using System;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public abstract class ItemInstance : PositionAndScriptBasedObjectInstance, IReplaceable, IRotateableY
    {
        public short Ocb { get; set; } = 0;

        private float _rotation;
        /// <summary> Rotation in radians in the interval [0, 360). The value is range reduced. </summary>
        public float RotationY
        {
            get { return _rotation; }
            set { _rotation = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        /// <summary> Rotation in radians in the interval [0, 2*Pi). The value is range reduced. </summary>
        public float RotationYRadians
        {
            get { return RotationY * (float)(Math.PI / 180); }
            set { RotationY = value * (float)(180 / Math.PI); }
        }

        public abstract ItemType ItemType { get; }

        public override string ToString()
        {
            return ItemType +
                   " (" + (Room?.ToString() ?? "NULL") + ")" +
                   ", Ocb = " + Ocb +
                   GetScriptIDOrName(false);
        }

        public string ShortName() => ItemType.ShortName() + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";

        public static ItemInstance FromItemType(ItemType item)
        {
            if (item.IsStatic)
                return new StaticInstance() { WadObjectId = item.StaticId };
            else
                return new MoveableInstance() { WadObjectId = item.MoveableId };
        }

        public string PrimaryAttribDesc => "Object ID";
        public string SecondaryAttribDesc => "OCB";

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            var otherInstance = other as ItemInstance;
            return (otherInstance?.ItemType.IsStatic == ItemType.IsStatic && 
                    otherInstance.ItemType == ItemType &&
                    (withProperties ? otherInstance?.Ocb == Ocb : true));
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var result = false;

            if (!ReplaceableEquals(other))
            {
                if (ItemType.IsStatic)
                {
                    var thisObj = (StaticInstance)this;
                    var thatObj = (StaticInstance)other;
                    if (thisObj.WadObjectId != thatObj.WadObjectId)
                    {
                        thisObj.WadObjectId = thatObj.WadObjectId;
                        result = true;
                    }
                    if (withProperties && thisObj.Ocb != thatObj.Ocb)
                    {
                        thisObj.Ocb = thatObj.Ocb;
                        result = true;
                    }
                }
                else
                {
                    var thisObj = (MoveableInstance)this;
                    var thatObj = (MoveableInstance)other;
                    if (thisObj.WadObjectId != thatObj.WadObjectId)
                    {
                        thisObj.WadObjectId = thatObj.WadObjectId;
                        result = true;
                    }
                    if (withProperties && thisObj.Ocb != thatObj.Ocb)
                    {
                        thisObj.Ocb = thatObj.Ocb;
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public struct ItemType
    {
        public bool IsStatic { get; }
        public WadMoveableId MoveableId { get; }
        public WadStaticId StaticId { get; }
        private readonly TRVersion.Game _gameVersion;

        public ItemType(WadStaticId staticId, TRVersion.Game gameVersion = TRVersion.Game.TR4)
        {
            IsStatic = true;
            MoveableId = new WadMoveableId();
            _gameVersion = gameVersion.Native();
            StaticId = staticId;
        }

        public ItemType(WadMoveableId moveableId, TRVersion.Game gameVersion = TRVersion.Game.TR4)
        {
            IsStatic = false;
            MoveableId = moveableId;
            _gameVersion = gameVersion.Native();
            StaticId = new WadStaticId();
        }

        public ItemType(WadStaticId staticId, LevelSettings levelSettings) // wad can be null
            : this(staticId, levelSettings?.GameVersion ?? TRVersion.Game.TR4)
        { }

        public ItemType(WadMoveableId moveableId, LevelSettings levelSettings) // wad can be null
            : this(moveableId, levelSettings?.GameVersion ?? TRVersion.Game.TR4)
        { }

        public static bool operator ==(ItemType first, ItemType second)
        {
            // Don't compare _gameVersion because nobody really cares about that, it's only there to make ToString work.
            return first.IsStatic == second.IsStatic && first.MoveableId == second.MoveableId && first.StaticId == second.StaticId;
        }

        public static bool operator !=(ItemType first, ItemType second) => !(first == second);

        public override bool Equals(object other) => other is ItemType && this == (ItemType)other;

        public override int GetHashCode()
        {
            int hashCode = IsStatic ? StaticId.GetHashCode() : MoveableId.GetHashCode();
            if (IsStatic)
                hashCode ^= unchecked((int)0xdcbc635b);
            return hashCode;
        }

        public override string ToString()
        {
            if (IsStatic)
                return StaticId.ToString(_gameVersion);
            else
                return MoveableId.ToString(_gameVersion);
        }

        public string ShortName()
        {
            if (IsStatic)
                return StaticId.ShortName(_gameVersion);
            else
                return MoveableId.ShortName(_gameVersion);
        }

        public IWadObject ToIWadObject(LevelSettings settings)
        {
            var wadObj = IsStatic
                ? (IWadObject)settings.WadTryGetStatic(StaticId)
                : (IWadObject)settings.WadTryGetMoveable(MoveableId);

            return wadObj;
        }
    }

}

using System.Numerics;
using TombLib.LuaProperties;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public class MoveableInstance : ItemInstance, IColorable, IRotateableYXRoll
    {
        // Don't use a reference here because the loaded Wad might not
        // contain each required object. Additionally the loaded Wads
        // can change. It would be unnecesary difficult to update all those references.
        public WadMoveableId WadObjectId { get; set; }

        public bool Invisible { get; set; } = false;
        public bool ClearBody { get; set; } = false;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.
        public Vector3 Color { get; set; } = new Vector3(1.0f);
        public override bool CopyToAlternateRooms => false;
        public override ItemType ItemType => new ItemType(WadObjectId, Room?.Level?.Settings);

        public float RotationX
        {
            get { return (Room?.Level?.IsTombEngine ?? false) ? _rotationX : 0.0f; }
            set { _rotationX = value; }
        }
        private float _rotationX = 0.0f;

        public float Roll
        {
            get { return (Room?.Level?.IsTombEngine ?? false) ? _roll : 0.0f; }
            set { _roll = value; }
        }
        private float _roll = 0.0f;

        /// <summary>
        /// Level 2 (per-instance) Lua property container. Stored in prj2 files.
        /// Holds per-entity property overrides in boxed Lua format.
        /// </summary>
        public LuaPropertyContainer LuaProperties { get; set; } = new LuaPropertyContainer();
    }
}

using System;
using System.Numerics;

namespace TombLib.LevelData
{
    public enum LightQuality : byte
    {
        Default, Low, Medium, High
    }
    public enum LightType : byte
    {
        Point, Shadow, Spot, Effect, Sun, FogBulb, Move, Glow
    }

    public class LightInstance : PositionBasedObjectInstance, IColorable, IReplaceable, IRotateableYX
    {
        public LightQuality Quality { get; set; } = LightQuality.Default;
        public LightType Type { get; set; }
        public Vector3 Color { get; set; } = new Vector3(1.0f, 1.0f, 1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)
        public float Intensity { get; set; } = 0.5f;
        public float InnerRange { get; set; } = 1.0f;
        public float OuterRange { get; set; } = 5.0f;
        public float InnerAngle { get; set; } = 20.0f;
        public float OuterAngle { get; set; } = 25.0f;
        public bool Enabled { get; set; } = true;
        public bool IsObstructedByRoomGeometry { get; set; } = true;
        public bool IsDynamicallyUsed { get; set; } = true;
        public bool IsStaticallyUsed { get; set; } = true;
        public bool IsUsedForImportedGeometry { get; set; } = true;
        public bool CastDynamicShadows { get; set; } = false;

        public bool CanCastDynamicShadows => CastDynamicShadows && (Type == LightType.Spot || Type == LightType.Point);

        private float _rotationX;
        private float _rotationY;
        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        public LightInstance(LightType type)
        {
            Type = type;
            switch (type)
            {
                case LightType.Shadow:
                    Intensity *= -1;
                    break;
                case LightType.Effect:
                    InnerRange = 0.99f;
                    OuterRange = 1.0f;
                    IsDynamicallyUsed = false;
                    break;
                case LightType.FogBulb:
                    IsObstructedByRoomGeometry = false;
                    IsStaticallyUsed = false;
                    IsUsedForImportedGeometry = false;
                    break;
                case LightType.Move:
                    InnerRange = 1.0f;
                    OuterRange = 5.0f;
                    IsDynamicallyUsed = false;
                    IsStaticallyUsed = false;
                    IsUsedForImportedGeometry = true;
                    break;
                case LightType.Glow:
                    InnerRange = 1.0f;
                    OuterRange = 5.0f;
                    IsDynamicallyUsed = false;
                    IsStaticallyUsed = false;
                    IsUsedForImportedGeometry = true;
                    break;
            }
        }

        public override string ToString()
        {
            return "Light " + Type +
                ", X = " +  WorldPosition.X +
                ", Y = " + -WorldPosition.Y +
                ", Z = " +  WorldPosition.Z;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);
        }

        public string PrimaryAttribDesc => "Colour";
        public string SecondaryAttribDesc => "Light type";

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            var otherInstance = other as LightInstance;
            return (otherInstance?.Color == Color && (withProperties ? otherInstance?.Type == Type : true));
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatColor = (LightInstance)other;
            if (!ReplaceableEquals(other) && Color != thatColor?.Color)
            {
                Color = thatColor.Color;
                return true;
            }
            else
                return false;
        }
    }
}

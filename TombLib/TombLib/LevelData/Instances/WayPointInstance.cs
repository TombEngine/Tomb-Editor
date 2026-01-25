using System;
using System.Linq;
using System.Numerics;

namespace TombLib.LevelData
{
    public enum WayPointType
    {
        Point,      // Single point, no radius
        Circle,     // Single point with radius
        Ellipse,    // Single point with two radii
        Square,     // Single point with radius (rendered as square)
        Rectangle,  // Single point with two radii (rendered as rectangle)
        Linear,     // Multi-point linear path
        Bezier      // Multi-point bezier path
    }

    public class WayPointInstance : PositionAndScriptBasedObjectInstance, IRotateableYXRoll, ISizeable
    {
        private string _name = "";
        private ushort _sequence;
        private ushort _number;
        private WayPointType _type = WayPointType.Point;

        // Public property to access the base name (without number suffix)
        public string BaseName
        {
            get { return _name; }
        }

        public ushort Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }

        public string Name
        {
            get 
            {
                // Singular types use name as-is
                if (IsSingularType())
                    return _name;
                else
                    return _name + "_" + _number;
            }
            set 
            { 
                // When setting name, extract the base name (strip trailing _number if present for multi-point types)
                if (!string.IsNullOrEmpty(value))
                {
                    int lastUnderscore = value.LastIndexOf('_');
                    if (lastUnderscore >= 0 && !IsSingularType())
                    {
                        string suffix = value.Substring(lastUnderscore + 1);
                        if (ushort.TryParse(suffix, out _))
                        {
                            _name = value.Substring(0, lastUnderscore);
                        }
                        else
                        {
                            _name = value;
                        }
                    }
                    else
                    {
                        _name = value;
                    }
                }
                else
                {
                    _name = "";
                }
                
                // Update LuaName to match Name
                LuaName = Name;
            }
        }

        public ushort Number
        {
            get { return _number; }
            set 
            { 
                _number = value;
                // Update LuaName when number changes
                LuaName = Name;
            }
        }

        public WayPointType Type 
        { 
            get { return _type; }
            set 
            { 
                _type = value;
                // Update LuaName when type changes (affects Name format)
                LuaName = Name;
            }
        }

        public float Radius1 { get; set; } = 1024.0f; // Default radius in units
        public float Radius2 { get; set; } = 1024.0f; // Default radius in units

        // ISizeable implementation for scale gizmo support on X and Z axes
        public Vector3 DefaultSize => new Vector3(Radius1 * 2, 0, Radius2 * 2);
        
        public Vector3 Size
        {
            get => new Vector3(Radius1 * 2, 0, Radius2 * 2);
            set
            {
                // Only allow scaling on X and Z axes for shapes
                if (RequiresRadius())
                {
                    Radius1 = Math.Max(0.01f, value.X / 2);
                    if (RequiresTwoRadii())
                        Radius2 = Math.Max(0.01f, value.Z / 2);
                    else
                        Radius2 = Radius1; // Keep them synchronized for single-radius shapes
                }
            }
        }

        private float _rotationX { get; set; }
        private float _rotationY { get; set; }
        private float _roll { get; set; }

        public bool IsSingularType()
        {
            return _type == WayPointType.Point ||
                   _type == WayPointType.Circle ||
                   _type == WayPointType.Ellipse ||
                   _type == WayPointType.Square ||
                   _type == WayPointType.Rectangle;
        }

        public bool RequiresRadius()
        {
            return _type == WayPointType.Circle ||
                   _type == WayPointType.Ellipse ||
                   _type == WayPointType.Square ||
                   _type == WayPointType.Rectangle;
        }

        public bool RequiresTwoRadii()
        {
            return _type == WayPointType.Ellipse ||
                   _type == WayPointType.Rectangle;
        }

        public WayPointInstance(ObjectInstance selectedObject = null)
        {
            if (selectedObject is WayPointInstance prevWayPoint)
            {
                var currNum = (ushort)(prevWayPoint.Number + 1);

                // Only push forward if it's a multi-point type
                if (!prevWayPoint.IsSingularType())
                {
                    // Push next waypoints with same name forward
                    var level = selectedObject.Room.Level;
                    var prevName = prevWayPoint._name;
                    foreach (var room in level.ExistingRooms)
                        foreach (var instance in room.Objects.OfType<WayPointInstance>())
                            if (instance._name == prevName && instance.Number >= currNum)
                                instance.Number++;
                }

                Number = prevWayPoint.IsSingularType() ? (ushort)0 : currNum;
                _name = prevWayPoint._name;
                Type = prevWayPoint.Type;
                Radius1 = prevWayPoint.Radius1;
                Radius2 = prevWayPoint.Radius2;

                // Additionally copy last waypoint parameters
                RotationX = prevWayPoint.RotationX;
                RotationY = prevWayPoint.RotationY;
                Roll = prevWayPoint.Roll;
                
                // Set LuaName to match Name
                LuaName = Name;
            }
        }

        public override ObjectInstance Clone()
        {
            var clone = (WayPointInstance)base.Clone();
            
            // For singular types, clear the name so user must provide a new one
            if (IsSingularType())
            {
                clone._name = "";
                clone.LuaName = "";
            }
            else
            {
                // For multi-point types, increment the number
                clone.Number = (ushort)(Number + 1);
            }
            
            return clone;
        }

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

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "WayPoint " +
                ", Name = " + Name +
                (IsSingularType() ? "" : ", Number = " + Number) +
                ", Type = " + Type +
                " (" + (Room?.ToString() ?? "NULL") + ")" +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => "WayPoint " + Name + (IsSingularType() ? "" : " (" + Number + ")") + " " + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";
    }
}

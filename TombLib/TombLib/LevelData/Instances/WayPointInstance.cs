using System;
using System.Linq;

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

    public class WayPointInstance : PositionAndScriptBasedObjectInstance, IRotateableYXRoll
    {
        private string _baseName = "WayPoint";
        private ushort _sequence;
        private ushort _number;
        private WayPointType _type = WayPointType.Point;

        public string BaseName
        {
            get { return _baseName; }
            set { _baseName = value ?? "WayPoint"; }
        }

        public string Name
        {
            get 
            {
                // Singular types don't have number suffix
                if (IsSingularType())
                    return _baseName;
                else
                    return _baseName + "_" + _number;
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
                            _baseName = value.Substring(0, lastUnderscore);
                        }
                        else
                        {
                            _baseName = value;
                        }
                    }
                    else
                    {
                        _baseName = value;
                    }
                }
                else
                {
                    _baseName = "WayPoint";
                }
            }
        }

        public ushort Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }

        public ushort Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public WayPointType Type 
        { 
            get { return _type; }
            set { _type = value; }
        }

        public float Radius1 { get; set; } = 1024.0f; // Default radius in units
        public float Radius2 { get; set; } = 1024.0f; // Default radius in units

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
                var currSeq = prevWayPoint.Sequence;
                var currNum = (ushort)(prevWayPoint.Number + 1);

                // Only push forward if it's a multi-point type
                if (!prevWayPoint.IsSingularType())
                {
                    // Push next waypoints in sequence forward
                    var level = selectedObject.Room.Level;
                    foreach (var room in level.ExistingRooms)
                        foreach (var instance in room.Objects.OfType<WayPointInstance>())
                            if (instance.Sequence == currSeq && instance.Number >= currNum)
                                instance.Number++;
                }

                Sequence = currSeq;
                Number = prevWayPoint.IsSingularType() ? (ushort)0 : currNum;
                _baseName = prevWayPoint._baseName;
                Type = prevWayPoint.Type;
                Radius1 = prevWayPoint.Radius1;
                Radius2 = prevWayPoint.Radius2;

                // Additionally copy last waypoint parameters
                RotationX = prevWayPoint.RotationX;
                RotationY = prevWayPoint.RotationY;
                Roll = prevWayPoint.Roll;
            }
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
                ", Sequence = " + Sequence +
                (IsSingularType() ? "" : ", Number = " + Number) +
                ", Type = " + Type +
                " (" + (Room?.ToString() ?? "NULL") + ")" +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => "WayPoint " + (IsSingularType() ? "" : "(" + Sequence + ":" + Number + ") ") + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);
            Sequence = args.ReassociateFlyBySequence(Sequence);
        }
    }
}

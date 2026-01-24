using System;
using System.Linq;

namespace TombLib.LevelData
{
    public enum PathType
    {
        Linear,
        Curved,
        Bezier
    }

    public class WayPointInstance : PositionAndScriptBasedObjectInstance, IRotateableYXRoll
    {
        private string _baseName = "WayPoint";
        private ushort _sequence;
        private ushort _number;

        public string Name
        {
            get { return _baseName + "_" + _number; }
            set 
            { 
                // When setting name, extract the base name (strip trailing _number if present)
                if (!string.IsNullOrEmpty(value))
                {
                    int lastUnderscore = value.LastIndexOf('_');
                    if (lastUnderscore >= 0)
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

        public PathType PathType { get; set; } = PathType.Linear;

        private float _rotationX { get; set; }
        private float _rotationY { get; set; }
        private float _roll { get; set; }

        public WayPointInstance(ObjectInstance selectedObject = null)
        {
            if (selectedObject is WayPointInstance prevWayPoint)
            {
                var currSeq = prevWayPoint.Sequence;
                var currNum = (ushort)(prevWayPoint.Number + 1);

                // Push next waypoints in sequence forward
                var level = selectedObject.Room.Level;
                foreach (var room in level.ExistingRooms)
                    foreach (var instance in room.Objects.OfType<WayPointInstance>())
                        if (instance.Sequence == currSeq && instance.Number >= currNum)
                            instance.Number++;

                Sequence = currSeq;
                Number = currNum;
                _baseName = prevWayPoint._baseName;
                PathType = prevWayPoint.PathType;

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
                ", Number = " + Number +
                ", PathType = " + PathType +
                " (" + (Room?.ToString() ?? "NULL") + ")" +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => "WayPoint (" + Sequence + ":" + Number + ")"  + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);
            Sequence = args.ReassociateFlyBySequence(Sequence);
        }
    }
}

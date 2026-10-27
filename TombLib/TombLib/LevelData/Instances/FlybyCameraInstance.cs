using System;
using System.Linq;

namespace TombLib.LevelData
{
    public enum DofMode
    {
        None = 0,
        Full = 1,
        Front = 2,
        Back = 3
    }

    public class FlybyCameraInstance : PositionAndScriptBasedObjectInstance, IRotateableYXRoll
    {
        public const float MaxFlybySpeed = 65535.0f / 655.0f;

        public ushort Sequence { get; set; }
        public ushort Number { get; set; }
        public short Timer { get; set; }
        public ushort Flags { get; set; }

        private float _speed { get; set; } = 1;
        private float _fov { get; set; } = 80;
        private float _roll { get; set; }
        private float _rotationX { get; set; }
        private float _rotationY { get; set; }
        private float _dofDistance { get; set; }
        private float _dofRange { get; set; }
        private float _dofStrength { get; set; }
        private DofMode _dofMode { get; set; }

        public int TimerToFrames => ((int)Timer >> 4);

        public FlybyCameraInstance(ObjectInstance selectedObject = null)
        {
            if (selectedObject != null && selectedObject is FlybyCameraInstance)
            {
                var prevCam = (FlybyCameraInstance)selectedObject;
                var currSeq = prevCam.Sequence;
                var currNum = (ushort)(prevCam.Number + 1);

                // Push next cameras in sequence forward
                var level = selectedObject.Room.Level;
                foreach (var room in level.ExistingRooms)
                    foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                        if (instance.Sequence == currSeq && instance.Number >= currNum)
                            instance.Number++;

                Sequence = currSeq;
                Number = currNum;
                Fov = prevCam.Fov;

                // Additionally copy last cam parameters

                RotationX = prevCam.RotationX;
                RotationY = prevCam.RotationY;
                Roll = prevCam.Roll;
                DofDistance = prevCam.DofDistance;
                DofRange = prevCam.DofRange;
                DofStrength = prevCam.DofStrength;
                DofMode = prevCam.DofMode;
            }
        }

        /// <summary> Speed in the range [0, MaxFlybySpeed]. </summary>
        public float Speed
        {
            get { return _speed; }
            set { _speed = float.IsFinite(value) ? Math.Clamp(value, 0.0f, MaxFlybySpeed) : 0.0f; }
        }

        /// <summary> Degrees in the range [0, 180]. </summary>
        public float Fov
        {
            get { return _fov; }
            set { _fov = Math.Max(0, Math.Min(180, value)); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        /// <summary> Degrees in the range [-90, 90]. </summary>
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

        /// <summary> World units in the range [0, +inf). </summary>
        public float DofDistance
        {
            get { return _dofDistance; }
            set { _dofDistance = float.IsFinite(value) ? Math.Max(0.0f, value) : 0.0f; }
        }

        /// <summary> World units in the range [0, +inf). </summary>
        public float DofRange
        {
            get { return _dofRange; }
            set { _dofRange = float.IsFinite(value) ? Math.Max(0.0f, value) : 0.0f; }
        }

        /// <summary> Raw renderer strength in the range [0, +inf). </summary>
        public float DofStrength
        {
            get { return _dofStrength; }
            set { _dofStrength = float.IsFinite(value) ? Math.Min(1.0f, Math.Max(0.0f, value)) : 0.0f; }
        }

        /// <summary> DOF mode. </summary>
        public DofMode DofMode
        {
            get { return _dofMode; }
            set { _dofMode = value; }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Flyby " +
                ", Sequence = " + Sequence +
                ", Number = " + Number +
                " (" + (Room?.ToString() ?? "NULL") + ")" +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public string ShortName() => "Flyby cam (" + Sequence + ":" + Number + ")"  + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);
            Sequence = args.ReassociateFlyBySequence(Sequence);
        }
    }
}

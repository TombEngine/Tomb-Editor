using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor
{
    /// <summary>
    /// Handles flyby camera sequence preview by interpolating position, target, FOV, roll, and speed using a Catmull-Rom spline.
    /// </summary>
    public class FlybyPreview
    {
        /// <summary> State of a single interpolated frame. </summary>
        public struct FrameState
        {
            public Vector3 Position;
            public float RotationY; // Yaw in radians    (FreeCamera convention)
            public float RotationX; // Pitch in radians  (FreeCamera convention: positive = look down)
            public float Roll;      // Roll in radians
            public float Fov;       // FOV in radians
        }

        private const float DegToRad = (float)(Math.PI / 180.0);

        /// <summary>
        /// Speed conversion matching the game engine.
        /// Editor Speed S is compiled as S * 655. The game advances CurrentSplinePosition (0–65536)
        /// by that amount per frame at 30fps. So segments/second = S * 655 * 30 / 65536.
        /// </summary>
        private const float SpeedScale = 655.0f * 30.0f / 65536.0f;

        /// <summary>
        /// Distance from camera to target point, matching the level compiler's convention.
        /// Target = Position + Direction * TargetDistance.
        /// </summary>
        private const float TargetDistance = 1024.0f;

        /// <summary>
        /// The game logic runs at 30 ticks per second.
        /// </summary>
        private const float GameTickRate = 30.0f;

        // SCF flag bits
        private const int FlagStopMovement = 1 << 8;
        private const int FlagCutToCam = 1 << 7;

        // Catmull-Rom knot arrays (padded via CatmullRomSpline.PadKnots)
        private readonly float[] _posX, _posY, _posZ;
        private readonly float[] _tgtX, _tgtY, _tgtZ;
        private readonly float[] _rollKnots, _fovKnots, _speedKnots;

        // Per-camera flags and timers
        private readonly List<ushort> _cameraFlags = new();
        private readonly List<short> _cameraTimers = new();
        private readonly int _numCameras;
        private readonly int _numSegments; // = _numCameras - 1

        private readonly float _speedMultiplier;

        // Current spline parameter: t ∈ [0, _numSegments]
        private float _currentT;

        // Freeze state (SCF_STOP_MOVEMENT)
        private float _freezeRemaining;
        private bool _frozenAtBoundary;
        private int _currentSegment;

        private readonly Stopwatch _stopwatch = new();
        private double _lastElapsed;

        public bool IsFinished { get; private set; }
        public int SequenceId { get; }
        public FrameState LastFrame { get; private set; }

        public FlybyPreview(Level level, int sequence, float speedMultiplier = 1.0f)
        {
            SequenceId = sequence;
            _speedMultiplier = speedMultiplier;

            var flybyCameras = level.ExistingRooms
                .SelectMany(r => r.Objects.OfType<FlybyCameraInstance>())
                .Where(c => c.Sequence == sequence)
                .OrderBy(c => c.Number)
                .ToList();

            if (flybyCameras.Count < 2)
            {
                IsFinished = true;
                _posX = _posY = _posZ = Array.Empty<float>();
                _tgtX = _tgtY = _tgtZ = Array.Empty<float>();
                _rollKnots = _fovKnots = _speedKnots = Array.Empty<float>();

                return;
            }

            _numCameras = flybyCameras.Count;
            _numSegments = _numCameras - 1;

            BuildKnotArrays(flybyCameras,
                out _posX, out _posY, out _posZ,
                out _tgtX, out _tgtY, out _tgtZ,
                out _rollKnots, out _fovKnots, out _speedKnots);

            for (int i = 0; i < _numCameras; i++)
            {
                _cameraFlags.Add(flybyCameras[i].Flags);
                _cameraTimers.Add(flybyCameras[i].Timer);
            }

            _currentT = 0;
            _currentSegment = 0;
            IsFinished = false;

            // Initialize LastFrame so rendering is consistent before the first Update().
            LastFrame = GetFrameAtT(0);
        }

        public void Start()
        {
            _stopwatch.Restart();
            _lastElapsed = 0;
        }

        public void Stop()
        {
            _stopwatch.Stop();
            IsFinished = true;
        }

        public FrameState Update()
        {
            if (IsFinished || _numCameras < 2)
                return default;

            float deltaTime = ComputeDeltaTime();

            if (TryHandleFreeze(deltaTime, out var frozenFrame))
                return frozenFrame;

            AdvancePosition(deltaTime);

            if (ProcessBoundaries(out var boundaryFrame))
                return boundaryFrame;

            if (_currentT >= _numSegments)
            {
                _currentT = _numSegments;
                IsFinished = true;
            }

            return EmitFrame();
        }

        #region Update helpers

        private float ComputeDeltaTime()
        {
            double currentElapsed = _stopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentElapsed - _lastElapsed);

            _lastElapsed = currentElapsed;

            return deltaTime;
        }

        /// <summary>
        /// Counts down the freeze timer. Returns <see langword="true"/> if still frozen.
        /// </summary>
        private bool TryHandleFreeze(float deltaTime, out FrameState frame)
        {
            frame = default;

            if (_freezeRemaining <= 0)
                return false;

            _freezeRemaining -= deltaTime;

            if (_freezeRemaining > 0)
            {
                frame = EmitFrame();
                return true;
            }

            _freezeRemaining = 0;
            _frozenAtBoundary = true;

            return false;
        }

        private void AdvancePosition(float deltaTime)
        {
            float currentSpeed = CatmullRomSpline.Evaluate(_currentT, _speedKnots);
            _currentT += currentSpeed * _speedMultiplier * deltaTime * SpeedScale;
        }

        private bool ProcessBoundaries(out FrameState frame)
        {
            frame = default;
            int maxSegment = _numSegments - 1;
            int loopGuard = _numCameras + 1;

            while (_currentSegment <= maxSegment && loopGuard-- > 0)
            {
                int nextCamera = _currentSegment + 1;
                float boundaryT = nextCamera;

                if (_currentT < boundaryT)
                    break;

                ushort flags = _cameraFlags[nextCamera];
                short timer = _cameraTimers[nextCamera];

                // SCF_STOP_MOVEMENT: freeze the camera.
                if ((flags & FlagStopMovement) != 0 && timer > 0 && !_frozenAtBoundary)
                {
                    int gameFrames = Math.Max(0, (timer >> 3) - 1);

                    if (gameFrames > 0)
                    {
                        _freezeRemaining = gameFrames / GameTickRate;
                        _currentT = boundaryT;
                        frame = EmitFrame();
                        return true;
                    }

                    _frozenAtBoundary = true;
                }

                _frozenAtBoundary = false;

                // SCF_CUT_TO_CAM: jump to another camera.
                if ((flags & FlagCutToCam) != 0)
                {
                    int targetCam = Math.Clamp(timer, 0, _numCameras - 1);

                    _currentT = targetCam;
                    _currentSegment = targetCam;
                    _frozenAtBoundary = false;

                    if (targetCam >= _numCameras - 1)
                    {
                        _currentT = _numSegments;
                        IsFinished = true;

                        break;
                    }

                    continue;
                }

                _currentSegment = nextCamera;
            }

            return false;
        }

        private FrameState EmitFrame()
        {
            var frame = GetFrameAtT(_currentT);
            LastFrame = frame;
            return frame;
        }

        #endregion Update helpers

        #region Frame interpolation

        private FrameState GetFrameAtT(float t)
        {
            t = Math.Clamp(t, 0, _numSegments);

            float px = CatmullRomSpline.Evaluate(t, _posX);
            float py = CatmullRomSpline.Evaluate(t, _posY);
            float pz = CatmullRomSpline.Evaluate(t, _posZ);
            float tx = CatmullRomSpline.Evaluate(t, _tgtX);
            float ty = CatmullRomSpline.Evaluate(t, _tgtY);
            float tz = CatmullRomSpline.Evaluate(t, _tgtZ);
            float roll = CatmullRomSpline.Evaluate(t, _rollKnots);
            float fov = CatmullRomSpline.Evaluate(t, _fovKnots);

            // Derive yaw and pitch from interpolated camera-to-target direction,
            // matching the game's LookAt() approach.
            float dx = tx - px;
            float dy = ty - py;
            float dz = tz - pz;
            float horizontalDist = (float)Math.Sqrt((dx * dx) + (dz * dz));

            float yaw = 0, pitch = 0;

            if (horizontalDist > 0.001f || Math.Abs(dy) > 0.001f)
            {
                yaw = (float)Math.Atan2(dx, dz);
                pitch = (float)Math.Atan2(-dy, horizontalDist);
            }

            return new FrameState
            {
                Position = new Vector3(px, py, pz),
                RotationY = yaw,
                RotationX = pitch,
                Roll = -roll * DegToRad,
                Fov = fov * DegToRad,
                Finished = IsFinished
            };
        }

        #endregion Frame interpolation

        #region Keyframe building

        /// <summary>
        /// Builds padded Catmull-Rom knot arrays from flyby cameras.
        /// </summary>
        private static void BuildKnotArrays(
            IList<FlybyCameraInstance> cameras,
            out float[] posX, out float[] posY, out float[] posZ,
            out float[] tgtX, out float[] tgtY, out float[] tgtZ,
            out float[] rollKnots, out float[] fovKnots, out float[] speedKnots)
        {
            int n = cameras.Count;

            var rawPosX = new float[n];
            var rawPosY = new float[n];
            var rawPosZ = new float[n];
            var rawTgtX = new float[n];
            var rawTgtY = new float[n];
            var rawTgtZ = new float[n];
            var rawRoll = new float[n];
            var rawFov = new float[n];
            var rawSpeed = new float[n];

            for (int i = 0; i < n; i++)
            {
                var cam = cameras[i];
                var worldPos = cam.Position + cam.Room.WorldPos;

                // Direction from rotation angles (same formula the level compiler uses)
                float yawRad = cam.RotationY * DegToRad;
                float pitchRad = cam.RotationX * DegToRad;
                float cosPitch = (float)Math.Cos(pitchRad);
                float dirX = cosPitch * (float)Math.Sin(yawRad);
                float dirY = (float)Math.Sin(pitchRad);
                float dirZ = cosPitch * (float)Math.Cos(yawRad);

                rawPosX[i] = worldPos.X;
                rawPosY[i] = worldPos.Y;
                rawPosZ[i] = worldPos.Z;
                rawTgtX[i] = worldPos.X + (TargetDistance * dirX);
                rawTgtY[i] = worldPos.Y + (TargetDistance * dirY);
                rawTgtZ[i] = worldPos.Z + (TargetDistance * dirZ);
                rawRoll[i] = cam.Roll;
                rawFov[i] = cam.Fov;
                rawSpeed[i] = cam.Speed;
            }

            // Unwrap roll so the spline takes the shortest angular path.
            UnwrapAngles(rawRoll);

            posX = CatmullRomSpline.PadKnots(rawPosX);
            posY = CatmullRomSpline.PadKnots(rawPosY);
            posZ = CatmullRomSpline.PadKnots(rawPosZ);
            tgtX = CatmullRomSpline.PadKnots(rawTgtX);
            tgtY = CatmullRomSpline.PadKnots(rawTgtY);
            tgtZ = CatmullRomSpline.PadKnots(rawTgtZ);
            rollKnots = CatmullRomSpline.PadKnots(rawRoll);
            fovKnots = CatmullRomSpline.PadKnots(rawFov);
            speedKnots = CatmullRomSpline.PadKnots(rawSpeed);
        }

        /// <summary>
        /// Adjusts consecutive angle values so that each delta is at most 180 degrees,
        /// preventing the spline from taking the long way around the 360-degree boundary.
        /// </summary>
        private static void UnwrapAngles(float[] angles)
        {
            for (int i = 1; i < angles.Length; i++)
            {
                float delta = angles[i] - angles[i - 1];
                angles[i] -= (float)Math.Round(delta / 360.0f) * 360.0f;
            }
        }

        #endregion Keyframe building
    }
}

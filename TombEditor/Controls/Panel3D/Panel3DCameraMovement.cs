#nullable enable

using Keys = TombLib.Input.EditorKey;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using TombEditor.Features.FlybyTimeline.Preview;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF.Controls.Rendering;
using TombLib.WPF.Input;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        private static bool IsKeyPressed(Keys key) => GetAsyncKeyState((int)key) < 0;

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (_movementTimer.Animating)
            {
                if (_movementTimer.Mode == AnimationMode.Snap)
                {
                    var lerpedRot = Vector2.Lerp(_lastCameraRot, _nextCameraRot, _movementTimer.MoveMultiplier);
                    Camera.Target = Vector3.Lerp(_lastCameraPos, _nextCameraPos, _movementTimer.MoveMultiplier);
                    Camera.RotationX = lerpedRot.X;
                    Camera.RotationY = lerpedRot.Y;
                    Camera.Distance = (float)MathC.Lerp(_lastCameraDist, _nextCameraDist, _movementTimer.MoveMultiplier);
                }
                Invalidate();
            }
            else
            {
                switch (_movementTimer.MoveKey)
                {
                    case Keys.Up:
                        Camera.Rotate(0, -_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Down:
                        Camera.Rotate(0, _editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Left:
                        Camera.Rotate(_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.Right:
                        Camera.Rotate(-_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.PageUp:
                        Camera.Zoom(-_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.PageDown:
                        Camera.Zoom(_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;
                }
            }
        }

        private void FlyModeTimer_Tick(object sender, EventArgs e)
        {
            if (_lastWindow != GetForegroundWindow() || IsKeyPressed(Keys.Escape))
            {
                ToggleFlyMode(false);
                _lastWindow = GetForegroundWindow();
                return;
            }

            Capture = true;

            Invalidate();
            var step = (float)_watch.Elapsed.TotalSeconds - (float)_flyModeTimer.Interval.TotalSeconds;

            step *= 500;
            step *= _editor.Configuration.Rendering3D_FlyModeMoveSpeed;

            /* Camera position handling */
            var newCameraPos = new Vector3();
            var cameraMoveSpeed = _editor.Configuration.Rendering3D_FlyModeMoveSpeed * 5 + step;

            if (ModifierKeys.HasFlag(Keys.Shift))
                cameraMoveSpeed *= 2;
            else if (ModifierKeys.HasFlag(Keys.Control))
                cameraMoveSpeed /= 2;

            if (IsKeyPressed(Keys.W))
                newCameraPos.Z -= cameraMoveSpeed;

            if (IsKeyPressed(Keys.A))
                newCameraPos.X += cameraMoveSpeed;

            if (IsKeyPressed(Keys.S))
                newCameraPos.Z += cameraMoveSpeed;

            if (IsKeyPressed(Keys.D))
                newCameraPos.X -= cameraMoveSpeed;

            if (IsKeyPressed(Keys.E))
                newCameraPos.Y += cameraMoveSpeed;

            if (IsKeyPressed(Keys.Q))
                newCameraPos.Y -= cameraMoveSpeed;

            Camera.MoveCameraPlane(newCameraPos);

            var room = _editor.GetRoomAtPosition(Camera.GetPosition());

            if (room != null)
                _editor.SelectedRoom = room;

            /* Camera rotation handling */
            var cursorPos = PointToClient(PointerState.ScreenPosition);

            float relativeDeltaX = (cursorPos.X - _lastMousePosition.X) / (float)Height;
            float relativeDeltaY = (cursorPos.Y - _lastMousePosition.Y) / (float)Height;

            if (cursorPos.X <= 0)
                PointerState.ScreenPosition = new Point(PointerState.ScreenPosition.X + Width - 2, PointerState.ScreenPosition.Y);
            else if (cursorPos.X >= Width - 1)
                PointerState.ScreenPosition = new Point(PointerState.ScreenPosition.X - Width + 2, PointerState.ScreenPosition.Y);

            if (cursorPos.Y <= 0)
                PointerState.ScreenPosition = new Point(PointerState.ScreenPosition.X, PointerState.ScreenPosition.Y + Height - 2);
            else if (cursorPos.Y >= Height - 1)
                PointerState.ScreenPosition = new Point(PointerState.ScreenPosition.X, PointerState.ScreenPosition.Y - Height + 2);

            if (cursorPos.X - _lastMousePosition.X >= (float)Width / 2 || cursorPos.X - _lastMousePosition.X <= -(float)Width / 2)
                relativeDeltaX = 0;

            if (cursorPos.Y - _lastMousePosition.Y >= (float)Height / 2 || cursorPos.Y - _lastMousePosition.Y <= -(float)Height / 2)
                relativeDeltaY = 0;

            Camera.Rotate(
                relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

            if (CanUseGizmo())
                _gizmo.MouseMoved(_viewProjection, GetRay(cursorPos.X, cursorPos.Y));

            _lastMousePosition = cursorPos;
        }

        public void ToggleFlyMode(bool state)
        {
            if (state == true)
            {
                _lastWindow = GetForegroundWindow();

                _oldCamera = Camera;
                Camera = new FreeCamera(_oldCamera.GetPosition(), _oldCamera.RotationX, _oldCamera.RotationY - (float)Math.PI,
                    _oldCamera.MinRotationX, _oldCamera.MaxRotationX, _oldCamera.FieldOfView);

                PointerState.Hide();

                _flyModeTimer.Start();
            }
            else
            {
                Capture = false;

                var p = Camera.GetPosition();
                var d = Camera.GetDirection();
                var t = Camera.GetTarget();

                t = p + d * Level.SectorSizeUnit;

                _oldCamera.RotationX = Camera.RotationX;
                _oldCamera.RotationY = Camera.RotationY - (float)Math.PI;

                Camera = _oldCamera;
                Camera.Distance = Level.SectorSizeUnit;
                Camera.Position = p;
                Camera.Target = t;

                PointerState.ScreenPosition = PointToScreen(new Point(Width / 2, Height / 2)); // Center cursor
                PointerState.Show();

                _flyModeTimer.Stop();
            }

            _editor.FlyMode = state;
        }

        public void ToggleCameraPreview(bool state, PositionBasedObjectInstance obj = null)
        {
            if (!state)
            {
                ExitCameraPreview();
                return;
            }

            // Don't start if fly mode is active.
            if (_editor.FlyMode)
                return;

            // Flyby sequence playback is handled by FlybyTimelineViewModel so the timeline and playhead stay synchronized.
            if (obj is FlybyCameraInstance)
                return;

            // Restart cleanly when switching between preview targets or modes.
            if (_editor.CameraPreviewMode != CameraPreviewType.None || _flybyPreview != null)
                ExitCameraPreview(false);

            // Stop any in-progress camera animation so it doesn't interfere with preview.
            _movementTimer.Stop(true);

            // Save the current camera before swapping to a free camera for preview.
            var savedCamera = Camera;
            Camera = new FreeCamera(savedCamera.GetPosition(),
                savedCamera.RotationX, savedCamera.RotationY - (float)Math.PI, -(float)Math.PI / 2, (float)Math.PI / 2,
                savedCamera.FieldOfView);

            if (obj is not CameraInstance cam)
            {
                // No specific object - enter static preview mode for external control (e.g. FormFlybyCamera).
                EnterStaticPreview(new FlybyPreview(savedCamera), "Camera preview active. Change parameters to update.");
                return;
            }

            // Static camera preview: position the camera at the instance's world position
            // and orient it towards the trigger-defined target or Lara.
            var preview = new FlybyPreview(savedCamera);
            Camera.Position = cam.WorldPosition;

            var targetPos = ResolveCameraTarget(cam);
            var toTarget = targetPos - Camera.Position;
            float distSq = toTarget.LengthSquared();

            // Fall back to +Z if target coincides with camera position.
            var direction = distSq > 0.001f ? toTarget / (float)Math.Sqrt(distSq) : Vector3.UnitZ;

            Camera.RotationY = (float)Math.Atan2(direction.X, direction.Z);
            Camera.RotationX = (float)Math.Asin(-direction.Y);

            EnterStaticPreview(preview, "Camera preview active. Press ESC or click to exit.");
        }

        public void UpdateFlybyFramePreview(FlybyCameraInstance flybyCamera)
        {
            if (_editor.CameraPreviewMode != CameraPreviewType.Static || _flybyPreview == null)
                return;

            _flybyPreview.SetStaticFrame(Camera, FlybyPreview.GetFrameForCamera(flybyCamera));
            Invalidate();
        }

        private void EnterStaticPreview(FlybyPreview preview, string message)
        {
            _flybyPreview = preview;

            _editor.CameraPreviewMode = CameraPreviewType.Static;
            _editor.SendMessage(message, PopupType.Info);

            Invalidate();
        }

        private void ExitCameraPreview(bool notifyUser = true)
        {
            if (_flybyPreview != null)
            {
                Camera = _flybyPreview.SavedCamera;
                _flybyPreview.Dispose();
                _flybyPreview = null;
            }

            _editor.CameraPreviewMode = CameraPreviewType.None;

            if (notifyUser)
                _editor.SendMessage("Camera preview ended.", PopupType.Info);

            Invalidate();
        }

        /// <summary>
        /// Resolves the look-at target for a static camera preview.
        /// Prioritizes the camera's room and its neighbors before searching remaining rooms.
        /// Falls back to Lara's position if no CAMERA_TARGET trigger is found.
        /// </summary>
        private Vector3 ResolveCameraTarget(CameraInstance cameraInstance)
        {
            var level = _editor.Level;
            var cameraRoom = cameraInstance.Room;

            // Build prioritized search order: camera's room and neighbors first, then remaining rooms
            var searchedRooms = new HashSet<Room>();
            var prioritizedRooms = new List<Room>();

            if (cameraRoom is not null)
            {
                foreach (var room in cameraRoom.AndAdjoiningRooms)
                {
                    if (searchedRooms.Add(room))
                        prioritizedRooms.Add(room);
                }
            }

            foreach (var room in level.ExistingRooms)
            {
                if (searchedRooms.Add(room))
                    prioritizedRooms.Add(room);
            }

            // Search for a Camera trigger referencing this instance and a co-located Target trigger
            foreach (var room in prioritizedRooms)
            {
                var target = FindCameraTargetInRoom(room, cameraInstance);

                if (target is not null)
                    return target.Value;
            }

            // No CAMERA_TARGET found - fall back to Lara's position
            var lara = level.ExistingRooms
                .SelectMany(r => r.Objects)
                .OfType<MoveableInstance>()
                .FirstOrDefault(m => m.WadObjectId == WadMoveableId.Lara);

            if (lara is not null)
                return lara.WorldPosition;

            // Last resort: look straight ahead from the camera position
            return cameraInstance.WorldPosition + (Vector3.UnitZ * Level.SectorSizeUnit);
        }

        private Vector3? FindCameraTargetInRoom(Room room, CameraInstance cameraInstance)
        {
            int xCount = room.Sectors.GetLength(0);
            int zCount = room.Sectors.GetLength(1);

            for (int x = 0; x < xCount; x++)
            {
                for (int z = 0; z < zCount; z++)
                {
                    var triggers = room.Sectors[x, z].Triggers;

                    bool hasCameraTrigger = triggers.Any(t =>
                        t.TargetType == TriggerTargetType.Camera && t.Target == cameraInstance);

                    if (!hasCameraTrigger)
                        continue;

                    // Found a Camera trigger - check for a Target trigger on the same sector
                    var targetInstance = triggers
                        .Where(t => t.TargetType == TriggerTargetType.Target && t.Target is PositionBasedObjectInstance)
                        .Select(t => t.Target as PositionBasedObjectInstance)
                        .FirstOrDefault();

                    if (targetInstance is not null)
                        return targetInstance.WorldPosition;
                }
            }

            return null;
        }
    }
}

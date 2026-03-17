using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Wad;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
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
            if (_lastWindow != GetForegroundWindow() || filter.IsKeyPressed(Keys.Escape))
            {
                ToggleFlyMode(false);
                _lastWindow = GetForegroundWindow();
                return;
            }

            Capture = true;

            Invalidate();
            var step = (float)_watch.Elapsed.TotalSeconds - _flyModeTimer.Interval / 1000.0f;

            step *= 500;
            step *= _editor.Configuration.Rendering3D_FlyModeMoveSpeed;

            /* Camera position handling */
            var newCameraPos = new Vector3();
            var cameraMoveSpeed = _editor.Configuration.Rendering3D_FlyModeMoveSpeed * 5 + step;

            if (ModifierKeys.HasFlag(Keys.Shift))
                cameraMoveSpeed *= 2;
            else if (ModifierKeys.HasFlag(Keys.Control))
                cameraMoveSpeed /= 2;

            if (filter.IsKeyPressed(Keys.W))
                newCameraPos.Z -= cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.A))
                newCameraPos.X += cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.S))
                newCameraPos.Z += cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.D))
                newCameraPos.X -= cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.E))
                newCameraPos.Y += cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.Q))
                newCameraPos.Y -= cameraMoveSpeed;

            Camera.MoveCameraPlane(newCameraPos);

            var room = GetCurrentRoom();

            if (room != null)
                _editor.SelectedRoom = room;

            /* Camera rotation handling */
            var cursorPos = PointToClient(Cursor.Position);

            float relativeDeltaX = (cursorPos.X - _lastMousePosition.X) / (float)Height;
            float relativeDeltaY = (cursorPos.Y - _lastMousePosition.Y) / (float)Height;

            if (cursorPos.X <= 0)
                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
            else if (cursorPos.X >= Width - 1)
                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

            if (cursorPos.Y <= 0)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
            else if (cursorPos.Y >= Height - 1)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

            if (cursorPos.X - _lastMousePosition.X >= (float)Width / 2 || cursorPos.X - _lastMousePosition.X <= -(float)Width / 2)
                relativeDeltaX = 0;

            if (cursorPos.Y - _lastMousePosition.Y >= (float)Height / 2 || cursorPos.Y - _lastMousePosition.Y <= -(float)Height / 2)
                relativeDeltaY = 0;

            Camera.Rotate(
                relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

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

                Cursor.Hide();

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

                Cursor.Position = PointToScreen(new Point(Width / 2, Height / 2)); // Center cursor
                Cursor.Show();

                _flyModeTimer.Stop();
            }

            _editor.FlyMode = state;
        }

        public void ToggleCameraPreview(bool state, int flybySequence = -1, float speedMultiplier = 1.0f, CameraInstance cameraInstance = null)
        {
            if (state)
            {
                // Don't start if fly mode is active or we're already in preview mode
                if (_editor.FlyMode || _editor.CameraPreviewMode)
                    return;

                // Stop any in-progress camera animation so it doesn't interfere with preview.
                _movementTimer.Stop(true);

                // Save the current camera state
                _flybyPreviewOldCamera = Camera;

                // Create a free camera for the preview
                Camera = new FreeCamera(
                    _flybyPreviewOldCamera.GetPosition(),
                    _flybyPreviewOldCamera.RotationX,
                    _flybyPreviewOldCamera.RotationY - (float)Math.PI,
                    -(float)Math.PI / 2, (float)Math.PI / 2,
                    _flybyPreviewOldCamera.FieldOfView);

                if (cameraInstance != null)
                {
                    // Static camera preview: position the camera at the instance's world position
                    // and orient it towards the trigger-defined target or Lara.
                    Camera.Position = cameraInstance.WorldPosition;

                    Vector3 targetPos = ResolveCameraTarget(cameraInstance);
                    Vector3 toTarget = targetPos - Camera.Position;
                    float distSq = toTarget.LengthSquared();

                    // Fall back to +Z if target coincides with camera position.
                    Vector3 direction = distSq > 0.001f
                        ? toTarget / (float)Math.Sqrt(distSq)
                        : Vector3.UnitZ;

                    // Compute yaw (RotationY) and pitch (RotationX) from direction vector.
                    // FreeCamera convention: yaw around Y, pitch around X, look direction is +Z.
                    Camera.RotationY = (float)Math.Atan2(direction.X, direction.Z);
                    Camera.RotationX = (float)Math.Asin(-direction.Y);

                    _editor.CameraPreviewMode = true;
                    _editor.CameraStaticPreviewMode = true;
                    _editor.SendMessage("Camera preview active. Press ESC or click to exit.", PopupType.Info);

                    Invalidate();
                }
                else if (flybySequence >= 0)
                {
                    // Flyby sequence preview
                    _flybyPreview = new FlybyPreview(_editor.Level, flybySequence, speedMultiplier);

                    if (_flybyPreview.IsFinished)
                    {
                        // Restore camera - not enough cameras to preview
                        Camera = _flybyPreviewOldCamera;
                        _flybyPreviewOldCamera = null;
                        _flybyPreview = null;
                        _editor.SendMessage("Flyby sequence needs at least 2 cameras to preview.", PopupType.Info);

                        return;
                    }

                    _flybyPreview.Start();
                    _flybyPreviewTimer.Start();
                    _editor.CameraPreviewMode = true;
                    _editor.CameraStaticPreviewMode = false;
                    _editor.SendMessage("Flyby preview playing... Press ESC or click to stop.", PopupType.Info);
                }
            }
            else
            {
                _flybyPreviewTimer.Stop();

                if (_flybyPreview != null)
                {
                    _flybyPreview.Stop();
                    _flybyPreview = null;
                }

                // Restore the original camera
                if (_flybyPreviewOldCamera != null)
                {
                    Camera = _flybyPreviewOldCamera;
                    _flybyPreviewOldCamera = null;
                }

                _editor.CameraPreviewMode = false;
                _editor.CameraStaticPreviewMode = false;
                _editor.SendMessage("Camera preview ended.", PopupType.Info);

                Invalidate();
            }
        }

        private void FlybyPreviewTimer_Tick(object sender, EventArgs e)
        {
            if (_flybyPreview == null || _flybyPreview.IsFinished)
            {
                ToggleCameraPreview(false);
                return;
            }

            // Check for ESC key to cancel
            if (filter.IsKeyPressed(Keys.Escape))
            {
                ToggleCameraPreview(false);
                return;
            }

            // Update the preview and get the current frame
            var frame = _flybyPreview.Update();

            // Apply the frame state to the camera
            Camera.Position = frame.Position;
            Camera.RotationY = frame.RotationY;
            Camera.RotationX = frame.RotationX;
            Camera.FieldOfView = frame.Fov;

            // Update camera target explicitly to prevent portal culling issues during flyby preview
            var rotation = Matrix4x4.CreateFromYawPitchRoll(frame.RotationY, frame.RotationX, 0);
            var look = MathC.HomogenousTransform(Vector3.UnitZ, rotation);
            Camera.Target = frame.Position + (Level.SectorSizeUnit * look);

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

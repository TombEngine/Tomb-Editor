#nullable enable

using System;
using System.Drawing;
using System.Numerics;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        public void ResetCamera(bool forceNewCamera = false, Room? destRoom = null)
        {
            if (_editor is null)
                return;

            var room = destRoom ?? _editor.SelectedRoom;

            // Point the camera to the room's center
            var target = new Vector3();

            if (room is not null)
                target = room.WorldPos + room.GetLocalCenter();

            // Calculate camera distance
            var roomDiagonal = new Vector2(room?.NumXSectors ?? 0, room?.NumZSectors ?? 0);

            var dist = (roomDiagonal.Length() * 0.8f + 2.1f) * Level.SectorSizeUnit;
            var rotX = 0.6f;
            var rotY = (float)Math.PI;

            bool flybyPreviewActive = _flybyPreview is not null && _flybyPreview.SavedCamera is not null;

            if (flybyPreviewActive || Camera is null || forceNewCamera || !_editor.Configuration.Rendering3D_AnimateCameraOnReset)
            {
                var newCamera = new ArcBallCamera(target, rotX, rotY, -(float)Math.PI / 2, (float)Math.PI / 2, dist, 100, 1000000, _editor.Configuration.Rendering3D_FieldOfView * (float)(Math.PI / 180));

                if (flybyPreviewActive)
                    _flybyPreview!.SavedCamera = newCamera;
                else
                    Camera = newCamera;

                Invalidate();
            }
            else
            {
                AnimateCamera(target, new Vector2(rotX, rotY), dist);
            }
        }

        public int TranslateCameraMouseMovement(Point value, bool horizontal = false)
        {
            if (Camera.RotationY < Math.PI * (1.0 / 4.0))
                return horizontal ? value.X : value.Y;
            else if (Camera.RotationY < Math.PI * (3.0 / 4.0))
                return horizontal ? value.Y : -value.X;
            else if (Camera.RotationY < Math.PI * (5.0 / 4.0))
                return horizontal ? -value.X : -value.Y;
            else if (Camera.RotationY < Math.PI * (7.0 / 4.0))
                return horizontal ? -value.Y : value.X;
            else
                return horizontal ? value.X : value.Y;
        }

        private void AnimateCamera(Vector3 oldPos, Vector3 newPos, Vector2 oldRot, Vector2 newRot, float oldDist, float newDist, float speed = 0.5f)
        {
            _nextCameraPos = newPos;
            _lastCameraPos = oldPos;
            _lastCameraRot = oldRot;
            _nextCameraRot = newRot;
            _lastCameraDist = oldDist;
            _nextCameraDist = newDist;

            _movementTimer.Animate(AnimationMode.Snap, speed);
        }

        private void AnimateCamera(Vector3 newPos, Vector2 newRot, float newDist, float speed = 0.5f)
            => AnimateCamera(Camera.Target, newPos, new Vector2(Camera.RotationX, Camera.RotationY), newRot, Camera.Distance, newDist, speed);

        private void AnimateCamera(Vector3 newPos, float speed = 0.5f)
            => AnimateCamera(newPos, new Vector2(Camera.RotationX, Camera.RotationY), Camera.Distance, speed);
    }
}

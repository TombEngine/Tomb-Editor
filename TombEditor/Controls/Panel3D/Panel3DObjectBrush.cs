using SharpDX.Toolkit.Graphics;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Features.Panel3D.ObjectBrush;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        // Brush stroke state.
        private bool _brushEngaged;
        private Vector3? _lastBrushWorldPosition;
        private float? _lastBrushDirectionAngle;

        // Brush hover adjustment state.
        private bool _brushParamEngaged = false;
        private bool _brushParamDeadzoneExceeded = false;
        private Vector3? _brushParamPinPoint;
        private Vector2 _brushParamPinHoverPos;

        // Brush cursor state (replaces Editor cursor fields).
        private Vector3? _brushCursorPosition;
        private Room _brushCursorRoom;

        #region Ray and Picking Helpers

        // Compute world XZ position from a ray and picking distance.
        private static Vector3 GetWorldPositionFromRay(Ray ray, float distance)
        {
            return new Vector3(ray.Position.X + ray.Direction.X * distance, 0, ray.Position.Z + ray.Direction.Z * distance);
        }

        // Do floor-only picking and return world position and room, or null if no floor hit.
        private (Vector3 WorldPos, Room Room)? PickBrushFloorPosition(Point location)
        {
            var ray = GetRay(location.X, location.Y);

            var picking = DoPicking(ray, skipObjects: true) as PickingResultSector;
            if (picking == null || !picking.BelongsToFloor)
                return null;

            return (GetWorldPositionFromRay(ray, picking.Distance), picking.Room);
        }

        #endregion

        #region Brush Cursor State

        // Update brush cursor position for rendering overlay.
        private void SetBrushCursor(Vector3 worldPos, Room room)
        {
            float localX = worldPos.X - room.WorldPos.X;
            float localZ = worldPos.Z - room.WorldPos.Z;
            float? floorH = Helpers.GetFloorHeightAtPoint(room, localX, localZ);
            float y = floorH.HasValue ? (floorH.Value + room.WorldPos.Y) : room.WorldPos.Y;

            _brushCursorPosition = new Vector3(worldPos.X, y, worldPos.Z);
            _brushCursorRoom = room;
        }

        #endregion

        #region Mouse Handlers

        private void HandleBrushMouseUp()
        {
            if (_brushEngaged)
				Actions.EndBrushStroke(_editor);

            _brushEngaged = false;
            _lastBrushWorldPosition = null;
            _lastBrushDirectionAngle = null;
			Actions.MouseDirectionAngle = null;
        }

        // Returns true if the scroll event was consumed by the brush handler.
        private bool HandleBrushWheelScroll(int delta, Point location)
        {
            if (_editor.Mode != EditorMode.ObjectPlacement)
                return false;

            bool settingsChanged = false;

            // Alt + mousewheel adjusts brush rotation.
            if (Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                float rotation = _editor.Configuration.ObjectBrush_Rotation + (delta > 0 ? Constants.AngleAdjustmentStep : -Constants.AngleAdjustmentStep);
                rotation = ((rotation % 360.0f) + 360.0f) % 360.0f;
                ObjectBrush.Actions.MouseDirectionAngle = _lastBrushDirectionAngle = _editor.Configuration.ObjectBrush_Rotation = 
                    (float)(Math.Round(rotation / ObjectBrush.Constants.AngleAdjustmentStep) * ObjectBrush.Constants.AngleAdjustmentStep);
                settingsChanged = true;
            }

            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                if (Control.ModifierKeys.HasFlag(Keys.Control))
                {
                    // Ctrl + shift + mousewheel adjusts brush density.

                    float density = _editor.Configuration.ObjectBrush_Density + (delta > 0 ? 0.1f : -0.1f);
                    _editor.Configuration.ObjectBrush_Density = 
                        Math.Min(Constants.MaxDensity, Math.Max(Constants.MinDensity, (float)Math.Round(density, 1)));
                    settingsChanged = true;
                }
                else
                {
                    // Shift + mousewheel adjusts brush radius.

                    float radius = _editor.Configuration.ObjectBrush_Radius + (delta > 0 ? Constants.RadiusAdjustmentStep : -Constants.RadiusAdjustmentStep);
                    _editor.Configuration.ObjectBrush_Radius =
                        Math.Min(Constants.MaxRadius * Level.SectorSizeUnit, Math.Max(Constants.MinRadius * Level.SectorSizeUnit, radius));
                    settingsChanged = true;
                }
            }

            if (settingsChanged)
            {
                _brushParamPinHoverPos = new Vector2(location.X, location.Y);
                _brushParamDeadzoneExceeded = false;
                _editor.ObjectBrushSettingsChange();
                Invalidate();
            }

            return settingsChanged;
        }

        private void HandleObjectPlacementMouseDown(Point location)
        {
            var floorHit = PickBrushFloorPosition(location);
            if (!floorHit.HasValue)
                return;

            if (_editor.Tool.Tool == EditorToolType.Fill)
            {
				// Fill executes immediately without brush engagement.
				Actions.ExecuteFill(_editor, _editor.SelectedRoom);
            }
            else
            {
                _brushEngaged = true;
                _lastBrushWorldPosition = Actions.BeginBrushStroke(_editor, _editor.SelectedRoom, floorHit.Value.WorldPos);
            }

            Invalidate();
        }

        // Returns true if the brush consumed the mouse move event (redraw needed).
        private bool HandleBrushMouseMove(Point location)
        {
            const float EraserQuantizationDistance = Level.SectorSizeUnit * 0.15f;

            if (_editor.Mode != EditorMode.ObjectPlacement || _editor.Tool.Tool == EditorToolType.Selection)
                return false;

            // When brush is not engaged, just update the cursor position for visual display.
            if (!_brushEngaged)
            {
                var hoverHit = PickBrushFloorPosition(location);
                if (hoverHit.HasValue)
                {
                    if (UpdateBrushParameters(location, hoverHit.Value.WorldPos))
                        return true;

                    SetBrushCursor(hoverHit.Value.WorldPos, hoverHit.Value.Room);
                    Invalidate();
                }

                return hoverHit.HasValue;
            }

            var floorHit = PickBrushFloorPosition(location);
            if (!floorHit.HasValue)
                return true;

            // Update visible cursor.
            SetBrushCursor(floorHit.Value.WorldPos, floorHit.Value.Room);

            // Eraser fires on fixed step; other tools quantize to avoid over-painting.
            float quantizationDistance = _editor.Tool.Tool == EditorToolType.Eraser ? EraserQuantizationDistance : _editor.Configuration.ObjectBrush_Radius;

            // For Line tool, constrain movement to the rotation direction.
            // Use bounding box extent along the rotation axis for seamless spacing.
            if (_editor.Tool.Tool == EditorToolType.Line)
            {
                if (!_lastBrushWorldPosition.HasValue)
                    return true;

                float rotRad = _editor.Configuration.ObjectBrush_Rotation * (float)(Math.PI / 180.0);
                var rotDir = new Vector3((float)Math.Sin(rotRad), 0, (float)Math.Cos(rotRad));

                var delta = floorHit.Value.WorldPos - _lastBrushWorldPosition.Value;
                delta.Y = 0;

                float proj = Vector3.Dot(delta, rotDir);
                float spacing = Helpers.ComputeLineSpacing(_editor);

                if (proj < spacing)
                    return true;

                // Snap to exact one-step advance from the last anchor for gapless tiling.
                var snappedPos = _lastBrushWorldPosition.Value + rotDir * spacing;
                bool painted = Actions.ContinueBrushStroke(_editor, _editor.SelectedRoom, snappedPos, null, spacing);

                if (painted)
                {
                    _lastBrushWorldPosition = snappedPos;
                    Invalidate();
                }
            }
            else
            {
                // Track mouse movement direction for FollowMouseDirection mode.
                UpdateMouseDirectionAngle(floorHit.Value.WorldPos);

                bool painted = Actions.ContinueBrushStroke(_editor, _editor.SelectedRoom,
                    floorHit.Value.WorldPos, _lastBrushWorldPosition, quantizationDistance);

                if (painted)
                {
                    _lastBrushWorldPosition = floorHit.Value.WorldPos;
                    Invalidate();
                }
            }

            return true;
        }

        #endregion

        #region Mouse Direction Tracking

        private void UpdateMouseDirectionAngle(Vector3 currentPos)
        {
            if (!_lastBrushWorldPosition.HasValue)
                return;

            float dx = currentPos.X - _lastBrushWorldPosition.Value.X;
            float dz = currentPos.Z - _lastBrushWorldPosition.Value.Z;

            if (dx * dx + dz * dz <= 0.01f)
                return;

            float angle = (float)(Math.Atan2(dx, dz) * (180.0f / Math.PI));
            angle = ((angle % 360.0f) + 360.0f) % 360.0f;

            if (_lastBrushDirectionAngle.HasValue)
            {
                float diff = ((angle - _lastBrushDirectionAngle.Value + 540.0f) % 360.0f) - 180.0f;
                _lastBrushDirectionAngle = ((_lastBrushDirectionAngle.Value + diff * 0.35f) % 360.0f + 360.0f) % 360.0f;
            }
            else
                _lastBrushDirectionAngle = angle;

			Actions.MouseDirectionAngle = _lastBrushDirectionAngle;
        }

        #endregion

        #region Parameter Handling

        // Adjusts ObjectBrush parameters based on modifier keys held during hover or brush stroke.
        // Alt: rotation tracks mouse movement direction.
        // Shift: radius = distance from pinned center to current cursor position.
        // Ctrl+Shift: density scales with distance from pin point.
        // Returns true if any parameter was modified.

        internal bool UpdateBrushParameters(Point location, Vector3 cursorWorldPos)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Shift) ||
                Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                if (!_brushParamEngaged)
                {
                    _brushParamPinPoint = cursorWorldPos;
                    _brushParamEngaged = true;
                    _brushParamPinHoverPos = new Vector2(location.X, location.Y);
                }
            }
            else
            {
                _brushParamEngaged = false;
                _brushParamDeadzoneExceeded = false;
                return false;
            }

            float screenDistance = Vector2.Distance(new Vector2(location.X, location.Y), _brushParamPinHoverPos);

            // Skip parameter update until the cursor has moved past the deadzone from the pin point.
            // Once exceeded, keep updating even if cursor moves back inside the deadzone.
            if (!_brushParamDeadzoneExceeded)
            {
                if (screenDistance < Constants.ParamDeadzone)
                    return true;

                _brushParamDeadzoneExceeded = true;
            }

            bool settingsChanged = false;

            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                if (Control.ModifierKeys.HasFlag(Keys.Control))
                {
                    // Ctrl+Shift: density scales with distance from pin point.
                    var density = (screenDistance / this.Size.Height) * Constants.MaxDensity;

                    _editor.Configuration.ObjectBrush_Density = Math.Min(Constants.MaxDensity, 
                        Math.Max(Constants.MinDensity, density));

                    settingsChanged = true;
                }
                else
                {
                    // Shift alone: radius = distance from pinned center to current cursor position.
                    float distance = Vector2.Distance(new Vector2(cursorWorldPos.X, cursorWorldPos.Z),
                        new Vector2(_brushParamPinPoint.Value.X, _brushParamPinPoint.Value.Z));

                    _editor.Configuration.ObjectBrush_Radius = Math.Min(Constants.MaxRadius * Level.SectorSizeUnit,
                        Math.Max(Constants.MinRadius * Level.SectorSizeUnit, distance));

                    settingsChanged = true;
                }
            }

            // Alt: rotation = smoothed angle from pin point to current cursor position.
            if (Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                _lastBrushWorldPosition = _brushParamPinPoint;
                UpdateMouseDirectionAngle(cursorWorldPos);

                if (_lastBrushDirectionAngle.HasValue)
                {
                    _editor.Configuration.ObjectBrush_Rotation = _lastBrushDirectionAngle.Value;
                    settingsChanged = true;
                }
            }

            if (settingsChanged)
                _editor.ObjectBrushSettingsChange();

            return settingsChanged;
        }

        #endregion

        #region Rendering Helpers

        private struct BrushOverlayState
        {
            public int Shape;
            public Vector4 Center;
            public Vector4 Color;
            public float Rotation;
        }

        // Compute brush overlay parameters. Returns null if brush is inactive.
        private BrushOverlayState ComputeBrushOverlay(bool reset = false)
        {
            const float MinBrushTransparency = 0.1f;
            const float MaxBrushTransparency = 0.4f;

            if (_editor.Mode != EditorMode.ObjectPlacement || _editor.Tool.Tool == EditorToolType.Selection || !_brushCursorPosition.HasValue || _brushCursorRoom == null)
                reset = true;

            int shape   = 0;
            var center  = Vector4.Zero;
            var rot     = -1.0f;
            var density = 0.25f;
            var color   = Vector4.Zero;

            if (!reset)
            {
                color = Vector4.One;

                if (_editor.Tool.Tool != EditorToolType.ObjectSelection &&
                    _editor.Tool.Tool != EditorToolType.ObjectDeselection &&
                    _editor.Tool.Tool != EditorToolType.Line &&
                    _editor.Tool.Tool != EditorToolType.Pencil)
                {
                    density = Math.Min(MaxBrushTransparency, Math.Max(MinBrushTransparency, _editor.Configuration.ObjectBrush_Density / Constants.MaxDensity));
                }

                var cursorPos = _brushCursorPosition.HasValue ? _brushCursorPosition.Value : Vector3.Zero;

                if (_editor.Tool.Tool == EditorToolType.Fill)
                {
                    const float FillBrushSize = 0.2f;

                    shape = (int)ObjectBrushShape.Circle + 1;
                    center = new Vector4(cursorPos.X, cursorPos.Y, cursorPos.Z, Level.SectorSizeUnit * FillBrushSize);

                    if (!_editor.Configuration.ObjectBrush_RandomizeRotation)
                        rot = _editor.Configuration.ObjectBrush_Rotation;
                }
                else
                {
                    shape = (int)_editor.Configuration.ObjectBrush_Shape + 1;
                    center = new Vector4(cursorPos.X, cursorPos.Y, cursorPos.Z, _editor.Configuration.ObjectBrush_Radius);

                    if (_editor.Tool.Tool != EditorToolType.ObjectSelection && _editor.Tool.Tool != EditorToolType.ObjectDeselection && _editor.Tool.Tool != EditorToolType.Eraser)
                    {
                        if (_editor.Tool.Tool != EditorToolType.Line && _editor.Configuration.ObjectBrush_FollowMouseDirection && _lastBrushDirectionAngle.HasValue)
                            rot = _lastBrushDirectionAngle.Value;
                        else if (_editor.Tool.Tool == EditorToolType.Line || !_editor.Configuration.ObjectBrush_RandomizeRotation || _editor.Configuration.ObjectBrush_FollowMouseDirection)
                            rot = _editor.Configuration.ObjectBrush_Rotation;
                    }
                }

                color.W = density;
            }

            return new BrushOverlayState { Shape = shape, Center = center, Color = color, Rotation = rot };
        }

        // Apply brush overlay parameters to a model effect shader.
        internal void ApplyBrushToModelEffect(Effect effect, bool reset = false)
        {
            var overlay = ComputeBrushOverlay(reset);

            effect.Parameters["BrushShape"].SetValue(overlay.Shape);
            effect.Parameters["BrushCenter"].SetValue(overlay.Center);
            effect.Parameters["BrushColor"].SetValue(overlay.Color);
            effect.Parameters["BrushRotation"].SetValue(overlay.Rotation);
            effect.Parameters["BrushLineWidth"].SetValue(_editor.Configuration.Rendering3D_LineWidth);
        }

        #endregion
    }
}

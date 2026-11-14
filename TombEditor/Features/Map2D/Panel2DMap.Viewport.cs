#nullable disable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace TombEditor.Features.Map2D
{
    // View positioning, zoom and keyboard-driven movement.
    public partial class Panel2DMap
    {
        public void ResetView()
        {
            ViewPosition = (new Vector2((float)ActualWidth, (float)ActualHeight) * 0.5f - new Vector2(16.0f)) / _viewScale;
        }

        private void MoveToFixedPoint(Point visualPoint, Vector2 worldPoint, bool limitPosition = false)
        {
            // Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint);
            if (limitPosition)
                LimitPosition();
            InvalidateVisual();
        }

        private void LimitPosition()
        {
            ViewPosition = Vector2.Clamp(ViewPosition, new Vector2(), new Vector2(_mapSize));
        }

        private int? FindClosestProbe(Vector2 clickPos)
        {
            if (_depthBar.DepthProbes.Count == 0)
                return null;

            int currentProbeIndex = 0;

            for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                if ((clickPos - _depthBar.DepthProbes[i].Position).Length() < (clickPos - _depthBar.DepthProbes[currentProbeIndex].Position).Length())
                    currentProbeIndex = i;

            if ((clickPos - _depthBar.DepthProbes[currentProbeIndex].Position).Length() < _probeRadius / 2 / ViewScale)
                return currentProbeIndex;
            else
                return null;
        }

        private void EngageMovement(Key key)
        {
            if (key is not (Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown))
                return;

            if (_moveKey != key)
            {
                _moveKey = key;
                _moveMultiplier = 0.0f;
            }

            if (!_movementTimer.IsEnabled)
                _movementTimer.Start();
        }

        private void StopMovement()
        {
            _movementTimer.Stop();
            _moveKey = Key.None;
            _moveMultiplier = 0.0f;
        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            if (_moveMultiplier < 1.0f)
                _moveMultiplier += _moveAcceleration;
            else
                _moveMultiplier = 1.0f;

            switch (_moveKey)
            {
                case Key.Down:
                    ViewPosition += new Vector2(0.0f, -_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _moveMultiplier);
                    LimitPosition();
                    InvalidateVisual();
                    break;
                case Key.Up:
                    ViewPosition += new Vector2(0.0f, _editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _moveMultiplier);
                    LimitPosition();
                    InvalidateVisual();
                    break;
                case Key.Left:
                    ViewPosition += new Vector2(-_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _moveMultiplier, 0.0f);
                    LimitPosition();
                    InvalidateVisual();
                    break;
                case Key.Right:
                    ViewPosition += new Vector2(_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _moveMultiplier, 0.0f);
                    LimitPosition();
                    InvalidateVisual();
                    break;
                case Key.PageDown:
                    ViewScale *= (float)Math.Exp(-_editor.Configuration.Map2D_NavigationSpeedKeyZoom * _moveMultiplier);
                    InvalidateVisual();
                    break;
                case Key.PageUp:
                    ViewScale *= (float)Math.Exp(_editor.Configuration.Map2D_NavigationSpeedKeyZoom * _moveMultiplier);
                    InvalidateVisual();
                    break;
            }
        }
    }
}

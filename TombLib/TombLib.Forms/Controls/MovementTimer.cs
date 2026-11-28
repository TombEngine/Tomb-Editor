#nullable enable

using System;
using System.Windows.Forms;
using System.Windows.Threading;
using TombLib.Utils;

namespace TombLib.Controls;

/// <summary>Identifies camera and geometry transition modes.</summary>
public enum AnimationMode
{
    Idle,
    Spin,
    Snap,
    GhostBlockUnfold
}

/// <summary>Schedules camera movement on the WPF dispatcher and releases pending ticks on disposal.</summary>
public class MovementTimer : IDisposable
{
    private readonly DispatcherTimer _moveTimer = new(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly float _moveAcceleration;
    private readonly EventHandler _moveEvent;
    private bool _disposed;

    private float _autoMoveStep = 0.0f;
    private float _autoMoveCurrent = 0.0f;

    /// <summary>Gets the current eased progress or directional acceleration.</summary>
    public float MoveMultiplier { get; private set; }

    /// <summary>Gets the current directional movement key.</summary>
    public Keys MoveKey { get; private set; } = Keys.None;

    /// <summary>Gets the active transition mode.</summary>
    public AnimationMode Mode { get; private set; } = AnimationMode.Idle;

    /// <summary>Gets whether an automatic transition is in progress.</summary>
    public bool Animating => Mode > AnimationMode.Spin;

    /// <summary>Creates a movement timer on the current UI dispatcher.</summary>
    public MovementTimer(EventHandler moveEvent, float moveAcceleration = 0.02f)
    {
        _moveTimer.Tick += UpdateTimer;

        _moveEvent = moveEvent;
        _moveAcceleration = moveAcceleration;
        _moveTimer.Tick += _moveEvent;
    }

    /// <summary>Stops movement and releases timer subscriptions.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop(true);
        _disposed = true;
        _moveTimer.Tick -= UpdateTimer;
        _moveTimer.Tick -= _moveEvent;
    }

    /// <summary>Starts directional movement if no transition is in progress.</summary>
    public bool Engage(Keys moveDirection)
    {
        if (_disposed || Mode != AnimationMode.Idle)
            return false;

        if (WinFormsUtils.DirectionalCameraKeys.Contains(moveDirection))
        {
            Mode = AnimationMode.Spin;
            if (!_moveTimer.IsEnabled)
                _moveTimer.IsEnabled = true;
            if (MoveKey != moveDirection)
            {
                MoveKey = moveDirection;
                MoveMultiplier = 0.0f;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>Starts an automatic transition with a duration measured in seconds.</summary>
    public void Animate(AnimationMode mode, float timeInSeconds)
    {
        if (_disposed)
            return;

        if (!float.IsFinite(timeInSeconds) || timeInSeconds <= 0.0f)
        {
            Stop(true);
            return;
        }

        Mode = mode;
        _autoMoveCurrent = 0.0f;
        _autoMoveStep = (float)_moveTimer.Interval.TotalSeconds / timeInSeconds;
        _moveTimer.IsEnabled = true;
    }

    /// <summary>Stops movement, optionally interrupting an automatic transition.</summary>
    public void Stop(bool force = false)
    {
        if (Mode > AnimationMode.Spin && !force)
            return;

        _moveTimer.IsEnabled = false;
        Mode = AnimationMode.Idle;
        MoveMultiplier = 0.0f;
        MoveKey = Keys.None;
    }

    private void UpdateTimer(object? sender, EventArgs e)
    {
        if (Mode == AnimationMode.Spin)
        {
            MoveMultiplier = Math.Clamp(MoveMultiplier + _moveAcceleration, 0.0f, 1.0f);
        }
        else if (Mode != AnimationMode.Idle)
        {
            _autoMoveCurrent += _autoMoveStep;

            // Allow timer to overflow for 2 subsequent frames, so we
            // get the final 1.0 value at all times.

            if (_autoMoveCurrent >= 1.0f + (_autoMoveStep * 2))
                Stop(true);
            else
            {
                var newMultiplier = MathC.SmoothStep(0.0, 1.0, _autoMoveCurrent);
                MoveMultiplier = (float)MathC.Clamp(newMultiplier, 0.0, 1.0);
            }
        }
        else
        {
            Stop(true);
        }
    }
}

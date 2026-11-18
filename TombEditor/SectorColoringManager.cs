#nullable enable

using System;
using System.Windows.Threading;
using TombLib.Rendering;

namespace TombEditor;

/// <summary>Animates transitions between sector color priorities on the editor dispatcher.</summary>
public class SectorColoringManager : IDisposable
{
    private const float TransitionSpeed = 0.335f;
    private readonly Editor _editor;
    private readonly DispatcherTimer _transitionAnimator = new() { Interval = TimeSpan.FromMilliseconds(50) };

    public class ChangeSectorColoringInfoEvent : IEditorEvent { }

    public SectorColoringInfo ColoringInfo;

    public SectorColoringManager(Editor editor)
    {
        _editor = editor;
        _transitionAnimator.Tick += UpdateTransitionAnimation;
        ColoringInfo = new SectorColoringInfo(_editor.Configuration.UI_ColorScheme);
    }

    /// <summary>Stops transition updates and releases the timer callback.</summary>
    public void Dispose()
    {
        _transitionAnimator.Stop();
        _transitionAnimator.Tick -= UpdateTransitionAnimation;
    }

    /// <summary>Animates a change to the dominant sector coloring type.</summary>
    public void SetPriority(SectorColoringType type)
    {
        if (ColoringInfo.SetPriority(type, false))
            _transitionAnimator.Start();
    }

    private void UpdateTransitionAnimation(object? sender, EventArgs e)
    {
        ColoringInfo.TransitionValue += TransitionSpeed;

        if (ColoringInfo.TransitionValue >= 1.0f)
        {
            ColoringInfo.TransitionValue = 1.0f;
            _transitionAnimator.Stop();
        }

        _editor.RaiseEvent(new ChangeSectorColoringInfoEvent());
    }
}

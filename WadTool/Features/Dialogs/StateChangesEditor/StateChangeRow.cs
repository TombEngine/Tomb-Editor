#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.Types;

namespace WadTool.Features.Dialogs.StateChangesEditor;

/// <summary>
/// Field identifiers for the legacy clamping rules
/// (<c>FormStateChangesEditor.dgvStateChanges_CellValidating</c>).
/// </summary>
public enum StateChangeRowField
{
    StateId,
    LowFrame,
    HighFrame,
    NextAnimation,
    NextLowFrame,
    NextHighFrame,
    BlendFrames
}

/// <summary>
/// WPF counterpart of the legacy <c>FormStateChangesEditor.WadStateChangeRow</c>:
/// one grid row per anim dispatch. Values are clamped by the owning view model on
/// assignment (legacy <c>CellValidating</c>) and every actual change is saved back
/// to the animation immediately (legacy <c>CellEndEdit</c> → <c>SaveChanges</c>).
/// </summary>
public sealed class StateChangeRow : ObservableObject
{
    private readonly StateChangesEditorWindowViewModel _owner;

    private string _stateName;
    private int _stateId;
    private int _lowFrame;
    private int _highFrame;
    private int _nextAnimation;
    private int _nextLowFrame;
    private int _nextHighFrame;
    private int _blendFrames;
    private BezierCurve2 _blendCurve;

    internal StateChangeRow(StateChangesEditorWindowViewModel owner, int stateId, int lowFrame, int highFrame,
        int nextAnimation, int nextLowFrame, int nextHighFrame, int blendFrames, BezierCurve2 blendCurve)
    {
        _owner = owner;
        _stateId = stateId;
        _lowFrame = lowFrame;
        _highFrame = highFrame;
        _nextAnimation = nextAnimation;
        _nextLowFrame = nextLowFrame;
        _nextHighFrame = nextHighFrame;
        _blendFrames = blendFrames;
        _blendCurve = blendCurve.Clone(); // The legacy row ctor cloned the incoming curve too
        _stateName = owner.GetStateName(stateId);
    }

    /// <summary>Read-only catalog name of <see cref="StateId"/> (first, grayed grid column).</summary>
    public string StateName
    {
        get => _stateName;
        private set => SetProperty(ref _stateName, value);
    }

    public int StateId
    {
        get => _stateId;
        set
        {
            bool changed = SetClamped(ref _stateId, value, StateChangeRowField.StateId, nameof(StateId));

            // Legacy refreshed the state name cell on every State ID commit.
            StateName = _owner.GetStateName(_stateId);

            if (changed)
                _owner.NotifyRowEdited();
        }
    }

    public int LowFrame
    {
        get => _lowFrame;
        set
        {
            if (SetClamped(ref _lowFrame, value, StateChangeRowField.LowFrame, nameof(LowFrame)))
                _owner.NotifyRowEdited();
        }
    }

    public int HighFrame
    {
        get => _highFrame;
        set
        {
            if (SetClamped(ref _highFrame, value, StateChangeRowField.HighFrame, nameof(HighFrame)))
                _owner.NotifyRowEdited();
        }
    }

    public int NextAnimation
    {
        get => _nextAnimation;
        set
        {
            bool changed = SetClamped(ref _nextAnimation, value, StateChangeRowField.NextAnimation, nameof(NextAnimation));
            OnPropertyChanged(nameof(NextAnimationToolTip));

            if (changed)
                _owner.NotifyRowEdited();
        }
    }

    public int NextLowFrame
    {
        get => _nextLowFrame;
        set
        {
            if (SetClamped(ref _nextLowFrame, value, StateChangeRowField.NextLowFrame, nameof(NextLowFrame)))
                _owner.NotifyRowEdited();
        }
    }

    public int NextHighFrame
    {
        get => _nextHighFrame;
        set
        {
            if (SetClamped(ref _nextHighFrame, value, StateChangeRowField.NextHighFrame, nameof(NextHighFrame)))
                _owner.NotifyRowEdited();
        }
    }

    public int BlendFrames
    {
        get => _blendFrames;
        set
        {
            if (SetClamped(ref _blendFrames, value, StateChangeRowField.BlendFrames, nameof(BlendFrames)))
                _owner.NotifyRowEdited();
        }
    }

    /// <summary>Saving after a curve edit is handled by the view model (legacy <c>ShowBlendCurveEditor</c>).</summary>
    public BezierCurve2 BlendCurve
    {
        get => _blendCurve;
        set => SetProperty(ref _blendCurve, value);
    }

    /// <summary>Tooltip of the "Next anim" cells (legacy <c>CellFormattingSafe</c>).</summary>
    public string NextAnimationToolTip => _owner.GetNextAnimationToolTip(_nextAnimation);

    /// <summary>
    /// Assigns the clamped value and always raises a change notification, so an
    /// out-of-range input is visually snapped back even when the stored value is unchanged.
    /// </summary>
    private bool SetClamped(ref int field, int proposedValue, StateChangeRowField kind, string propertyName)
    {
        int clampedValue = _owner.ClampRowValue(this, kind, proposedValue);
        bool changed = field != clampedValue;

        field = clampedValue;
        OnPropertyChanged(propertyName);

        return changed;
    }
}

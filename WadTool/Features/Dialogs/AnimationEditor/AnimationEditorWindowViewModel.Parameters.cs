#nullable enable

using System;
using TombLib.Types;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Animation parameters, bounding box fields and per-bone transform.</summary>
public partial class AnimationEditorWindowViewModel
{
    // ---- Editable animation properties ----

    partial void OnAnimNameChanged(string value)
    {
        if (!_allowUpdate || _editor.CurrentAnim is null) return;
        var newName = value.Trim();
        if (string.IsNullOrEmpty(newName) || newName == _editor.CurrentAnim.WadAnimation.Name) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.Name = newName;
        if (SelectedAnim is not null) SelectedAnim.Label = "(" + _editor.CurrentAnim.Index + ") " + newName;
    }

    partial void OnFrameRateChanged(double value)
    {
        if (!ValidAnim() || (byte)value == _editor.CurrentAnim.WadAnimation.FrameRate) return;
        PushUndoOnce();
        byte rate = (byte)TombLib.MathC.Clamp((float)value, 1, 255);
        _allowUpdate = false;
        EndFrame = Math.Clamp(Math.Round(_editor.CurrentAnim.WadAnimation.EndFrame / (_editor.CurrentAnim.WadAnimation.FrameRate / (float)rate)), 0.0, 32767.0);
        _allowUpdate = true;
        _editor.CurrentAnim.WadAnimation.FrameRate = rate;
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)EndFrame;
        AfterParameterChange();
    }

    partial void OnEndFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.EndFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextAnimationChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextAnimation) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextAnimation = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnStartVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartVelocity = v, _editor.CurrentAnim?.WadAnimation.StartVelocity, value);
    partial void OnEndVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndVelocity = v, _editor.CurrentAnim?.WadAnimation.EndVelocity, value);
    partial void OnStartHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.StartLateralVelocity, value);
    partial void OnEndHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.EndLateralVelocity, value);

    private void SetVelocity(Action<float> setter, float? current, double value)
    {
        if (!ValidAnim() || current is null || Math.Abs(current.Value - (float)value) < 1e-6f) return;
        PushUndoOnce();
        setter((float)value);
        AfterParameterChange();
    }

    partial void OnBlendFrameCountChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.BlendFrameCount) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.BlendFrameCount = (ushort)value;
        AfterParameterChange();
    }

    partial void OnStateIdTextChanged(string value)
    {
        if (_allowUpdate)
            UpdateStateChange(value);
    }

    partial void OnSelectedStateNameChanged(string? value)
    {
        if (value is null || !_allowUpdate)
            return;

        UpdateStateChange(value);
        SelectedStateName = null;
    }

    /// <summary>Moves the animation state to the previous or next known state name (wheel over the State list).</summary>
    public void StepState(int direction)
    {
        if (StateNames.Count == 0)
            return;

        var index = StateNames.IndexOf(StateIdText.Trim());
        var next = index < 0 ? (direction > 0 ? 0 : StateNames.Count - 1) : Math.Clamp(index + direction, 0, StateNames.Count - 1);

        if (next != index)
            UpdateStateChange(StateNames[next]);
    }

    /// <summary>Decodes a state ID or state name typed into the State field, applies it and puts the state name back into the field.</summary>
    private void UpdateStateChange(string input)
    {
        if (_editor.CurrentAnim is null)
            return;

        var decodeFailed = false;
        var oldValue = _editor.CurrentAnim.WadAnimation.StateId;

        // Try to identify if there was a string input.
        if (!ushort.TryParse(input, out var newValue))
        {
            var searchString = input.Trim();
            var possibleId = -1;

            for (int i = 0; i < 2; i++)
            {
                possibleId = TrCatalog.TryToGetStateID(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, searchString);

                if (possibleId >= 0)
                    break;

                // Try to do one more pass with spaces replaced by underlines.
                searchString = searchString.Replace(' ', '_');
            }

            if (possibleId >= 0)
            {
                newValue = (ushort)possibleId;
            }
            else
            {
                ShowPopup("State ID with given name wasn't found.", PopupType.Error);
                decodeFailed = true;
                newValue = oldValue;
            }
        }

        // Restore state name to the field, if possible.
        if (newValue != ushort.MaxValue)
        {
            _allowUpdate = false;
            StateIdText = GetStateDisplayText(newValue);
            _allowUpdate = true;
        }

        // Add legacy numerical ID as a tooltip.
        if (!decodeFailed)
            StateIdToolTip = "State ID = " + newValue;

        // Don't update if not changed or parse failed.
        if (oldValue == newValue || newValue == ushort.MaxValue)
            return;

        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.StateId = newValue;
    }

    // ---- Bounding box ----

    partial void OnBBoxMinXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinZChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxZChanged(double value) => SetBoundingBox();

    private void SetBoundingBox()
    {
        if (_timeline is null || !ValidAnim()) return;
        var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[_timeline.Value].BoundingBox;
        var newMin = new System.Numerics.Vector3((float)BBoxMinX, (float)BBoxMinY, (float)BBoxMinZ);
        var newMax = new System.Numerics.Vector3((float)BBoxMaxX, (float)BBoxMaxY, (float)BBoxMaxZ);
        if (newMin == bb.Minimum && newMax == bb.Maximum) return;

        PushUndoOnce();
        var deltaMin = newMin - bb.Minimum;
        var deltaMax = newMax - bb.Maximum;

        int start = _timeline.Value, end = _timeline.Value;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        for (int i = start; i <= end; i++)
        {
            var bb2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox;
            bb2.Minimum += deltaMin;
            bb2.Maximum += deltaMax;
            _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox = bb2;
        }
        AfterParameterChange();
    }

    // ---- Per-bone transform ----

    partial void OnTransformModeIndexChanged(int value)
    {
        _editor.TransformMode = (AnimTransformMode)value;
        if (_allowUpdate && _editor.MadeChanges) UpdateTransform();
    }

    partial void OnRotationXChanged(double value) => UpdateTransform();
    partial void OnRotationYChanged(double value) => UpdateTransform();
    partial void OnRotationZChanged(double value) => UpdateTransform();
    partial void OnTranslationXChanged(double value) => UpdateTransform();
    partial void OnTranslationYChanged(double value) => UpdateTransform();
    partial void OnTranslationZChanged(double value) => UpdateTransform();

    private void UpdateTransform()
    {
        if (_panel is null || !_allowUpdate || _editor.CurrentKeyFrame is null)
            return;

        // Block modifications during blend preview.
        if (_blendState.IsActive && _editor.Tool.Configuration.AnimationEditor_ChainPlayback)
            return;

        // The editor pushes the undo entry itself on the first change.
        int meshIndex = _panel.SelectedMesh is null ? 0 : _panel.Model.Meshes.IndexOf(_panel.SelectedMesh);
        _editor.UpdateTransform(meshIndex,
            new System.Numerics.Vector3(TombLib.MathC.DegToRad((float)RotationX), TombLib.MathC.DegToRad((float)RotationY), TombLib.MathC.DegToRad((float)RotationZ)),
            new System.Numerics.Vector3((float)TranslationX, (float)TranslationY, (float)TranslationZ));
        _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();
    }

    // ---- Bone / mesh selection ----

    partial void OnSelectedBoneItemChanged(MeshBoneItem? value)
    {
        if (!_allowUpdate || _panel is null || value is null) return;
        _panel.SelectedMesh = value.Index < 0 ? null : _panel.Model.Meshes[value.Index];
        UpdateTransformUI();
        _panel.Invalidate();
    }

    // ---- Root motion (TEN) ----

    partial void OnRootPosXChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosYChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosZChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotXChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotYChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotZChanged(bool value) => ApplyRootMotion();

    private void ApplyRootMotion()
    {
        if (!_allowUpdate || _editor.CurrentAnim is null) return;
        PushUndoOnce();
        var rootMotion = _editor.CurrentAnim.WadAnimation.RootMotion;
        rootMotion.TranslationX = RootPosX;
        rootMotion.TranslationY = RootPosY;
        rootMotion.TranslationZ = RootPosZ;
        rootMotion.RotationX = RootRotX;
        rootMotion.RotationY = RootRotY;
        rootMotion.RotationZ = RootRotZ;
        _editor.CurrentAnim.WadAnimation.RootMotion = rootMotion;
    }

    // ---- Blend curve preset ----

    partial void OnBlendPresetIndexChanged(int value)
    {
        if (_bezier is null || value < 0) return;
        _bezier.Value.Set(value switch
        {
            0 => BezierCurve2.Linear,
            1 => BezierCurve2.EaseIn,
            2 => BezierCurve2.EaseOut,
            _ => BezierCurve2.EaseInOut
        });
        _bezier.UpdateUI();
    }

    /// <summary>Called by the window when the blend-curve editor value changes (legacy resets the preset combo).</summary>
    public void OnBlendCurveEdited()
    {
        if (!_allowUpdate)
            return;

        _allowUpdate = false;
        BlendPresetIndex = -1;
        _allowUpdate = true;
        SetSaved(false);
    }
}

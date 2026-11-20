#nullable enable

using CommunityToolkit.Mvvm.Input;
using LevelFileDialog = TombLib.WPF.Services.LevelFileDialog;
using System;
using System.IO;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Interpolate, reverse, mirror, import and export tools.</summary>
public partial class AnimationEditorWindowViewModel
{
    // ---- View / config toggles (slice 3e) ----

    partial void OnScrollGridChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_ScrollGrid = value;
    partial void OnRestoreGridHeightChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange = value;
    partial void OnSmoothAnimationChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_SmoothAnimation = value;
    partial void OnShowGizmoChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowGizmo = value; _panel?.Invalidate(); }
    partial void OnShowGridChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowGrid = value; _panel?.Invalidate(); }
    partial void OnShowCollisionBoxChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowCollisionBox = value; _panel?.Invalidate(); }
    partial void OnShowSkinChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowSkin = value; _panel?.Invalidate(); }

    // ---- Interpolate / reverse / mirror (slice 3e) ----

    [RelayCommand]
    private void Interpolate()
    {
        if (_timeline is null || !ValidAndSelected()) return;

        int numFrames;
        var selection = TombLib.MathC.Clamp(_timeline.Selection.Y - _timeline.Selection.X - 1, 0, int.MaxValue).ToString();
        string? input = InputBox.Show(Owner, "Interpolation", "Enter number of interpolated frames:", selection);
        if (input is null) return;
        if (!int.TryParse(input, out numFrames)) numFrames = 3;

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        int start = _timeline.Selection.X;
        int end = (_timeline.SelectionSize == 1 && _timeline.Selection.Y == _timeline.Maximum) ? _timeline.Minimum : _timeline.Selection.Y;
        InterpolateFrames(start, end, numFrames);
    }

    private void InterpolateFrames(int frameIndex1, int frameIndex2, int numFrames, bool updateGui = true)
    {
        if (_timeline is null || _panel is null) return;
        var frame1 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1];
        var frame2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex2];

        if (frameIndex2 - frameIndex1 > 1)
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(frameIndex1 + 1, frameIndex2 - frameIndex1 - 1);

        for (int i = 0; i < numFrames; i++)
        {
            var keyFrame = new KeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
            {
                keyFrame.Rotations.Add(System.Numerics.Vector3.Zero);
                keyFrame.Quaternions.Add(System.Numerics.Quaternion.Identity);
                keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                keyFrame.TranslationsMatrices.Add(System.Numerics.Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
            }
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
        }

        float k = 1.0f / (numFrames + 1);
        for (int i = 0; i < numFrames; i++)
        {
            var keyframe = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];
            keyframe.Translations[0] = System.Numerics.Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
            keyframe.TranslationsMatrices[0] = System.Numerics.Matrix4x4.CreateTranslation(keyframe.Translations[0]);
            for (int j = 0; j < keyframe.Quaternions.Count; j++)
            {
                keyframe.Quaternions[j] = System.Numerics.Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                keyframe.Rotations[j] = TombLib.MathC.QuaternionToEuler(keyframe.Quaternions[j]);
            }
            var bbMin = System.Numerics.Vector3.Lerp(frame1.BoundingBox.Minimum, frame2.BoundingBox.Minimum, k * (i + 1));
            var bbMax = System.Numerics.Vector3.Lerp(frame1.BoundingBox.Maximum, frame2.BoundingBox.Maximum, k * (i + 1));
            keyframe.BoundingBox = new TombLib.BoundingBox(bbMin, bbMax);
        }

        FixEndFrame(numFrames);

        if (!updateGui)
            return;

        OnKeyframesListChanged();
        _timeline.Highlight(frameIndex1, frameIndex1 + numFrames + 1);
        _timeline.ResetSelection();
        _timeline.Value = frameIndex1 + numFrames + 1;
        ShowPopup("Successfully inserted " + numFrames + " interpolated frames between frames " + frameIndex1 + " and " + frameIndex2, PopupType.Info);
    }

    [RelayCommand]
    private void Reverse()
    {
        if (_timeline is null || _panel is null || !_editor.ValidAnimationAndFrames) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.Reverse(start, end - start);
        foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
            if (ac.FrameBased && ac.Parameter1 >= start && ac.Parameter1 <= end)
                ac.Parameter1 = (short)((_editor.CurrentAnim.WadAnimation.FrameRate * end) - (ac.Parameter1 - (_editor.CurrentAnim.WadAnimation.FrameRate * start)));

        _timeline.Highlight(start, end);
        SelectFrame();
        UpdateTransformUI();
    }

    [RelayCommand]
    private void Mirror()
    {
        if (_timeline is null || _panel is null || !_editor.ValidAnimationAndFrames) return;

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        float accumulatedRotation = 0;
        foreach (var kf in _editor.CurrentAnim.DirectXAnimation.KeyFrames)
            accumulatedRotation += Math.Abs((Math.Round(TombLib.MathC.RadToDeg(kf.Rotations[0]).Y / 90.0f) * 90.0f) % 180.0f) != 0 ? 1 : 0;
        accumulatedRotation /= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        bool flipZ = accumulatedRotation > 0.5f;

        var bonePairs = _panel.Model.GetBonePairs(flipZ);
        if (bonePairs is null || bonePairs.Count == 0) { ShowPopup("No valid bones were found for mirroring.", PopupType.Error); return; }
        if (flipZ) ShowPopup("Broken identity rotation detected. Impossible to mirror animation correctly.", PopupType.Warning);

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        for (int i = start; i <= end; i++)
        {
            var frame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i];
            frame.Translations[0] = new System.Numerics.Vector3(-frame.Translations[0].X, frame.Translations[0].Y, frame.Translations[0].Z);
            var newMin = new System.Numerics.Vector3(-frame.BoundingBox.Minimum.X, frame.BoundingBox.Minimum.Y, frame.BoundingBox.Minimum.Z);
            var newMax = new System.Numerics.Vector3(-frame.BoundingBox.Maximum.X, frame.BoundingBox.Maximum.Y, frame.BoundingBox.Maximum.Z);
            frame.BoundingBox = new TombLib.BoundingBox(newMin, newMax);

            foreach (var pair in bonePairs)
            {
                if (pair[1] != -1)
                {
                    var r1 = TombLib.MathC.RadToDeg(frame.Rotations[pair[0]]);
                    var r2 = TombLib.MathC.RadToDeg(frame.Rotations[pair[1]]);
                    var nr1 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r2.X, (360.0f - r2.Y) % 360.0f, (360.0f - r2.Z) % 360.0f));
                    var nr2 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r1.X, (360.0f - r1.Y) % 360.0f, (360.0f - r1.Z) % 360.0f));
                    frame.Rotations[pair[0]] = nr1;
                    frame.Rotations[pair[1]] = nr2;
                    frame.Quaternions[pair[0]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(nr1.Y, nr1.X, nr1.Z);
                    frame.Quaternions[pair[1]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(nr2.Y, nr2.X, nr2.Z);
                }
                else
                {
                    var r1 = TombLib.MathC.RadToDeg(frame.Rotations[pair[0]]);
                    r1 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r1.X, (360.0f - r1.Y) % 360.0f, (360.0f - r1.Z) % 360.0f));
                    frame.Rotations[pair[0]] = r1;
                    frame.Quaternions[pair[0]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(r1.Y, r1.X, r1.Z);
                }
            }
        }

        // HACK (legacy): bounce undo so bounding box and translation update correctly.
        _editor.Tool.UndoManager.Undo();
        _editor.Tool.UndoManager.Redo();

        if (!flipZ && _editor.Moveable.Id.TypeId != 0 && _editor.Moveable.Meshes.Count > 3)
            ShowPopup("Animation mirroring may fail on certain non-Lara models.\nPlease check animation integrity.", PopupType.Info);

        _timeline.Highlight(start, end);
        SelectFrame();
        UpdateTransformUI();
    }

    // ---- Import / export (slice 3e) ----

    [RelayCommand]
    private void ImportAnimation()
    {
        if (_panel is null || _editor.CurrentAnim is null) return;
        string? path = LevelFileDialog.BrowseFile(Owner, "Select a file with animations", BaseGeometryImporter.AnimationFileExtensions, false);
        if (path is null) return;

        WadAnimation animation;
        bool containsMetadata = false;
        try
        {
            if (Path.GetExtension(path) == ".anim") { containsMetadata = true; animation = WadActions.ImportAnimationFromXml(_editor.Tool, path); }
            else if (Path.GetExtension(path) == ".trw") { containsMetadata = true; animation = WadActions.ImportAnimationFromTrw(path, _editor.CurrentAnim.Index); }
            else animation = WadActions.ImportAnimationFromModel(_editor.Tool, Owner, _editor.Moveable.Bones.Count, path);
        }
        catch (Exception ex) { ShowPopup("Error while loading animation.\nException: " + ex.Message, PopupType.Error); return; }

        if (animation is null) return;
        if (animation.Name == new WadAnimation().Name) animation.Name = _editor.CurrentAnim.WadAnimation.Name;
        if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
        { ShowPopup("You can only import an animation with the same number of bones as current moveable.", PopupType.Error); return; }

        _editor.CheckAnimationIntegrity(animation);
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        if (containsMetadata)
            _editor.CurrentAnim.WadAnimation = animation;
        else
        {
            _editor.CurrentAnim.WadAnimation.KeyFrames.Clear();
            _editor.CurrentAnim.WadAnimation.KeyFrames.AddRange(animation.KeyFrames);
            _editor.CurrentAnim.WadAnimation.EndFrame = animation.EndFrame;
        }

        _editor.CurrentAnim.DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);
        if (SelectedAnim is not null) SelectedAnim.Label = "(" + _editor.CurrentAnim.Index + ") " + _editor.CurrentAnim.WadAnimation.Name;
        _timeline.Value = 0;
        SelectAnimation(_editor.CurrentAnim);
        _panel?.Invalidate();
    }

    [RelayCommand]
    private void ExportAnimation()
    {
        if (_editor.CurrentAnim is null) return;
        string? path = LevelFileDialog.BrowseFile(Owner, "Specify file to save animation",
            new System.Collections.Generic.List<FileFormat> { new FileFormat("TombEditor XML", "anim") }, true);
        if (path is null) return;

        var animationToSave = _editor.GetSavedAnimation(_editor.CurrentAnim);
        if (!WadActions.ExportAnimationToXml(_editor.Moveable, animationToSave, path))
            ShowPopup("Can't export current animation to XML file", PopupType.Error);
    }
}

#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using TombLib.Graphics;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;
using WadTool.Features.Dialogs.AnimCommandsEditor;
using WadTool.Features.Dialogs.StateChangesEditor;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Keyframe and animation editing operations.</summary>
public partial class AnimationEditorWindowViewModel
{
    // ---- Keyframe & animation operations (slice 3c) ----

    private void OnKeyframesListChanged()
    {
        if (_timeline is null) return;
        if (_editor.ValidAnimationAndFrames)
        {
            _frameCount = _timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
            _timeline.Minimum = 0;
            _timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
            bool previousAllowUpdate = _allowUpdate;
            _allowUpdate = false;
            EndFrame = _editor.CurrentAnim.WadAnimation.EndFrame;
            _allowUpdate = previousAllowUpdate;
            UpdateStatusLabel();
        }
        else
            StatusText = string.Empty;
    }

    private void FixEndFrame(int delta)
    {
        var v = Math.Max(0, _editor.CurrentAnim.WadAnimation.EndFrame + delta);
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)v;
    }

    private bool ValidAndSelected(bool prompt = true)
    {
        if (_timeline is null) return false;
        if (_timeline.SelectionIsEmpty)
        { if (prompt) ShowPopup("No frames selected. Please select at least 1 frame.", PopupType.Error); }
        else if (_editor.CurrentAnim is null)
        { if (prompt) ShowPopup("No animation selected. Select animation to work with.", PopupType.Error); }
        else if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        { if (prompt) ShowPopup("Current animation contains no frames.", PopupType.Error); }
        else
            return true;
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo() => _editor.Tool.UndoManager.Undo();
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo() => _editor.Tool.UndoManager.Redo();
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _editor.SaveChanges();
        SetSaved(true);
    }

    private void SetSaved(bool saved)
    {
        if (_isSaved == saved)
            return;

        _isSaved = saved;
        SaveCommand.NotifyCanExecuteChanged();
        Title = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion) + (saved ? "" : "*");
    }

    [RelayCommand]
    private void AddAnimation()
    {
        var wadAnimation = new WadAnimation { FrameRate = 1, Name = "New Animation " + _editor.Animations.Count };
        var keyFrame = new WadKeyFrame();
        foreach (var bone in _editor.Moveable.Bones)
            keyFrame.Angles.Add(new WadKeyFrameRotation());
        wadAnimation.KeyFrames.Add(keyFrame);

        var dxAnimation = Animation.FromWad2(_editor.Moveable.Bones, wadAnimation);
        var node = new AnimationNode(wadAnimation, dxAnimation, _editor.Animations.Count);

        _editor.Animations.Add(node);
        var item = new AnimListItem(node, "(" + node.Index + ") " + node.WadAnimation.Name);
        Animations.Add(item);
        SelectedAnim = item;
        SetSaved(false);
        NameFocusRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void AddFrame()
    {
        if (_timeline is null) return;
        AddNewFrame(_timeline.Value + 1, true);
    }

    private void AddNewFrame(int index, bool undo)
    {
        if (_editor.CurrentAnim is null || _timeline is null) return;
        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        KeyFrame keyFrame;
        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        {
            index = 0;
            keyFrame = new KeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
            {
                keyFrame.Rotations.Add(System.Numerics.Vector3.Zero);
                keyFrame.Quaternions.Add(System.Numerics.Quaternion.Identity);
                keyFrame.Translations.Add(bone.Translation);
                keyFrame.TranslationsMatrices.Add(System.Numerics.Matrix4x4.CreateTranslation(bone.Translation));
            }
        }
        else
            keyFrame = _editor.CurrentKeyFrame.Clone();

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
        OnKeyframesListChanged();
        FixEndFrame(1);
    }

    [RelayCommand]
    private void DeleteFrame() => DeleteFrames(true, true, true);

    private void DeleteFrames(bool prompt, bool undo, bool updateGUI)
    {
        if (_timeline is null || _panel is null || !ValidAndSelected(prompt)) return;

        if (prompt &&
            UserDialogs.Show(Owner, "Do you really want to delete frame" +
                (_timeline.SelectionSize == 1 ? " " + _timeline.Value : "s " + _timeline.Selection.X + "-" + _timeline.Selection.Y) + "?",
                "Confirm", DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.No)
            return;

        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int selectionStart = _timeline.Selection.X;
        int selectionEnd = _timeline.Selection.Y;
        int cursorPos = (_timeline.Value < _timeline.Selection.X || _timeline.Value > _timeline.Selection.Y) ? _timeline.Value : -1;

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(_timeline.Selection.X, _timeline.SelectionSize);

        if (!updateGUI) return;

        FixEndFrame(-_timeline.SelectionSize);
        _timeline.ResetSelection();
        OnKeyframesListChanged();

        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count != 0)
            _timeline.Value = cursorPos != -1
                ? (cursorPos < selectionStart ? cursorPos : selectionStart + (cursorPos - selectionEnd - 1))
                : selectionStart;
        else
        {
            _timeline.Value = 0;
            StatusText = string.Empty;
        }
        _panel.Invalidate();
    }

    [RelayCommand]
    private void CutFrame()
    {
        if (!ValidAndSelected()) return;
        CopyFramesInternal(false);
        DeleteFrames(false, true, true);
    }

    [RelayCommand]
    private void CopyFrame() => CopyFramesInternal(true);

    private void CopyFramesInternal(bool updateGUI)
    {
        if (_timeline is null || !ValidAndSelected()) return;
        _editor.ClipboardKeyFrames.Clear();
        for (int i = _timeline.Selection.X; i <= _timeline.Selection.Y; i++)
            _editor.ClipboardKeyFrames.Add(_editor.CurrentAnim.DirectXAnimation.KeyFrames[i].Clone());
        if (updateGUI)
            _timeline.Highlight(_timeline.Selection.X, _timeline.Selection.Y);
    }

    [RelayCommand]
    private void PasteFrame()
    {
        if (_timeline is null || _panel is null || _editor.CurrentAnim is null) return;
        if (_editor.ClipboardKeyFrames is null || _editor.ClipboardKeyFrames.Count <= 0)
        {
            ShowPopup("Nothing to paste!", PopupType.Warning);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int startIndex = _timeline.SelectionIsEmpty ? _timeline.Value : _timeline.Selection.X;
        int endIndex = _timeline.SelectionIsEmpty ? _timeline.Value : _timeline.Selection.Y;
        int cursorPos = (_timeline.SelectionIsEmpty || _timeline.Value < _timeline.Selection.X || _timeline.Value > _timeline.Selection.Y) ? _timeline.Value : -1;

        if (!_timeline.SelectionIsEmpty)
            DeleteFrames(false, false, false);

        var pastedFrames = new System.Collections.Generic.List<KeyFrame>();
        _editor.ClipboardKeyFrames.ForEach(frame => pastedFrames.Add(frame.Clone()));
        _editor.CurrentAnim.DirectXAnimation.KeyFrames.InsertRange(startIndex, pastedFrames);
        OnKeyframesListChanged();
        FixEndFrame(pastedFrames.Count - _timeline.SelectionSize);

        int insertEnd = startIndex + _editor.ClipboardKeyFrames.Count - 1;
        if (cursorPos != -1 && cursorPos <= _timeline.Maximum)
            _timeline.Value = cursorPos < startIndex ? cursorPos : insertEnd + (cursorPos - endIndex) + (_timeline.SelectionIsEmpty ? 1 : 0);
        else
            _timeline.Value = insertEnd;

        _timeline.ResetSelection();
        _timeline.Highlight(startIndex, insertEnd);
        _panel.Invalidate();
    }

    [RelayCommand]
    private void EditAnimCommands() => EditAnimCommands(null);

    /// <summary>Opens the anim commands editor, optionally selecting the given command.</summary>
    public void EditAnimCommands(WadAnimCommand? command)
    {
        if (_editor.CurrentAnim is null) return;
        var vm = new AnimCommandsEditorWindowViewModel(_editor, _editor.CurrentAnim, command);
        var dialog = new AnimCommandsEditorWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();
        _editor.Tool.AnimationEditorAnimationChanged(_editor.CurrentAnim, false);
    }

    [RelayCommand]
    private void EditStateChanges() => EditStateChanges(null);

    private void EditStateChanges(WadStateChange? newStateChange)
    {
        if (_editor.CurrentAnim is null) return;
        var vm = new StateChangesEditorWindowViewModel(_editor, _editor.CurrentAnim, newStateChange);
        var dialog = new StateChangesEditorWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void FixAnimation() => FixAnimations(AnimationFixScope.Current);

    // ---- Animation operations (slice 3d) ----

    [RelayCommand]
    private void DeleteAnimation()
    {
        if (_editor.CurrentAnim is null) return;
        if (UserDialogs.Show(Owner, "Do you really want to delete '" + _editor.CurrentAnim.WadAnimation.Name + "'?",
                "Confirm", DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.No)
            return;
        DeleteAnimationCore(_editor.CurrentAnim);
    }

    private void DeleteAnimationCore(AnimationNode animToDelete, bool updateUi = true)
    {
        if (animToDelete is null || !_editor.Animations.Contains(animToDelete)) return;

        int currentIndex = _editor.Animations.IndexOf(animToDelete);
        for (int i = 0; i < _editor.Animations.Count; i++)
        {
            if (i == currentIndex) continue;
            var animation = _editor.Animations[i];
            if (animation.Index > currentIndex) animation.Index--;
            if (animation.WadAnimation.NextAnimation > currentIndex) animation.WadAnimation.NextAnimation--;
            foreach (var stateChange in animation.WadAnimation.StateChanges)
                foreach (var dispatch in stateChange.Dispatches)
                    if (dispatch.NextAnimation > currentIndex) dispatch.NextAnimation--;
        }

        _editor.Animations.Remove(animToDelete);

        if (!updateUi)
            return;

        RebuildAnimationsList();
        _panel?.Invalidate();
        SetSaved(false);
        _timeline?.Invalidate();
    }

    [RelayCommand]
    private void CutAnimation()
    {
        if (_editor.CurrentAnim is null) { ShowPopup("No animation to cut!", PopupType.Warning); return; }
        _editor.ClipboardNode = _editor.CurrentAnim;
        DeleteAnimation();
    }

    [RelayCommand]
    private void CopyAnimation()
    {
        if (_editor.CurrentAnim is null) { ShowPopup("No animation to copy!", PopupType.Warning); return; }
        _editor.ClipboardNode = _editor.CurrentAnim.Clone();
    }

    [RelayCommand]
    private void PasteAnimation()
    {
        if (_editor.ClipboardNode is null || _editor.CurrentAnim is null) { ShowPopup("No animation to paste!", PopupType.Warning); return; }
        int animationIndex = _editor.Animations.Count;
        var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
        _editor.Animations.Add(pastedAnim);
        pastedAnim.WadAnimation.Name += " - Copy";
        RebuildAnimationsList();
        SelectAnimByIndex(animationIndex);
        SetSaved(false);
    }

    [RelayCommand]
    private void ReplaceAnimation()
    {
        if (_editor.ClipboardNode is null || _editor.CurrentAnim is null) { ShowPopup("No animation to replace!", PopupType.Warning); return; }
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        int animationIndex = _editor.CurrentAnim.Index;
        var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
        _editor.Animations[animationIndex] = pastedAnim;
        pastedAnim.WadAnimation.Name += " - Copy";
        RebuildAnimationsList();
        SelectAnimByIndex(animationIndex);
        SetSaved(false);
    }

    [RelayCommand]
    private void SplitAnimation()
    {
        if (_timeline is null || _editor.CurrentAnim is null) { ShowPopup("No animation to split!", PopupType.Warning); return; }

        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 3)
        { ShowPopup("You must have at least 3 frames for splitting the animation", PopupType.Error); return; }

        int numFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (_timeline.Value == 0 || _timeline.Value == numFrames - 1)
        { ShowPopup("You can't set the first or the last frame for splitting the animation", PopupType.Error); return; }

        var newWadAnimation = _editor.CurrentAnim.WadAnimation.Clone();
        var newDirectXAnimation = _editor.CurrentAnim.DirectXAnimation.Clone();
        int numFrames2 = numFrames - _timeline.Value;

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(_timeline.Value + 1, numFrames2 - 1);
        newDirectXAnimation.KeyFrames.RemoveRange(0, _timeline.Value);

        newWadAnimation.Name += " - splitted";
        _editor.Animations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation, _editor.Animations.Count));

        RebuildAnimationsList();
        SelectAnimByIndex(_editor.Animations.Count - 1);
        SetSaved(false);
    }
}

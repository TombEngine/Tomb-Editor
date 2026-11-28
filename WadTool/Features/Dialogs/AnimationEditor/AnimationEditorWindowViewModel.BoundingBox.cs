#nullable enable

using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Animation bounding box calculation.</summary>
public partial class AnimationEditorWindowViewModel
{
    // ---- Bounding box (slice 3d) ----

    private System.Collections.Generic.List<int> GetSelectedMeshList()
        => Bones.Where(b => b.Checked).Select(b => b.Index).ToList();

    [RelayCommand]
    private void SelectAllMeshes() { foreach (var b in Bones) b.Checked = true; }
    [RelayCommand]
    private void SelectNoMeshes() { foreach (var b in Bones) b.Checked = false; }

    [RelayCommand]
    private void CalcBoundingBox() => CalculateAnimationBoundingBox(false);
    [RelayCommand]
    private void ClearBoundingBox() => CalculateAnimationBoundingBox(true);

    private void CalculateAnimationBoundingBox(bool clear)
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_timeline.SelectionIsEmpty) { start = _timeline.Selection.X; end = _timeline.Selection.Y + 1; }

        for (int i = start; i < end; i++)
            CalculateKeyframeBoundingBox(i, clear);

        _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();
    }

    private void CalculateKeyframeBoundingBox(int index, bool clear, bool undo = false)
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames) return;
        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        var meshList = GetSelectedMeshList();
        var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[index];
        _panel.Model.BuildAnimationPose(keyFrame);

        if (clear || meshList.Count == 0)
            keyFrame.BoundingBox = new TombLib.BoundingBox();
        else
            keyFrame.CalculateBoundingBox(_panel.Model, _panel.Skin, meshList);

        if (index == _timeline.Value)
        {
            UpdateTransformUI();
            _panel.Invalidate();
        }

        SetSaved(false);
    }

    [RelayCommand]
    private void GrowBoundingBox() => InflateAnimationBoundingBox(new System.Numerics.Vector3((float)GrowX, (float)GrowY, (float)GrowZ));
    [RelayCommand]
    private void ShrinkBoundingBox() => InflateAnimationBoundingBox(new System.Numerics.Vector3((float)-GrowX, (float)-GrowY, (float)-GrowZ));

    private void InflateAnimationBoundingBox(System.Numerics.Vector3 value)
    {
        if (_timeline is null || !_editor.ValidAnimationAndFrames || value.Length() == 0f) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_timeline.SelectionIsEmpty) { start = _timeline.Selection.X; end = _timeline.Selection.Y + 1; }

        for (int i = start; i < end; i++)
        {
            var kf = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i];
            kf.BoundingBox = kf.BoundingBox.Inflate(value);
        }

        if (_panel is not null)
        {
            UpdateTransformUI();
            _panel.Invalidate();
        }
    }
}

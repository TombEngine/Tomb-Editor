#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Graphics;
using TombLib.Services;
using TombLib.Wad.Catalog;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.AnimEditor;

// Animation list: filtering, multi-selection and bulk deletion.
public partial class AnimationEditorWindowViewModel
{
    private const int MaxStateId = 256;

    private List<AnimListItem> _selectedAnimItems = new();

    [ObservableProperty] private string _filterText = string.Empty;

    /// <summary>Called by the window when the (multi-)selection of the animation list changes.</summary>
    public void SetSelectedAnimations(IEnumerable<AnimListItem> items) => _selectedAnimItems = items.ToList();

    /// <summary>Rebuilds the animation list according to the filter text and tries to keep the current animation selected.</summary>
    private void RebuildAnimationsList()
    {
        var hadCurrent = _editor.CurrentAnim is not null;
        var currentIndex = _editor.CurrentAnim?.Index ?? -1;
        var filter = AnimationListFilter.Parse(FilterText, FindStateByName);

        _rebuildingList = true;

        try
        {
            Animations.Clear();

            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                var node = _editor.Animations[i];

                if (filter.IsMatch(i, node.WadAnimation))
                    Animations.Add(new AnimListItem(node, "(" + node.Index + ") " + node.WadAnimation.Name));
            }
        }
        finally
        {
            _rebuildingList = false;
        }

        if (_editor.Animations.Count == 0)
            SelectAnimation(null);
        else if (Animations.Count > 0)
            SelectedAnim = (hadCurrent ? Animations.FirstOrDefault(item => item.Node.Index == currentIndex) : null) ?? Animations[0];

        UpdateStatusLabel();
    }

    /// <summary>Allows the next parameter edit to push a fresh undo step.</summary>
    public void ResetUndoCoalescing() => _editor.MadeChanges = false;

    /// <summary>Refreshes the transform fields from the current keyframe, e.g. while a gizmo is being dragged.</summary>
    public void RefreshTransformUI() => UpdateTransformUI();

    private static string GetAnimationLabel(AnimationNode node) => "(" + node.Index + ") " + node.WadAnimation.Name;

    /// <summary>Handles an animation replaced by undo/redo: swaps the list item for the new node and optionally selects it.</summary>
    private void ReplaceAnimationItem(AnimationNode animation, bool focus)
    {
        var item = Animations.FirstOrDefault(entry => entry.Node.Index == animation.Index);

        if (item is null)
            return;

        var oldNode = item.Node;
        var cursorPosition = -1;

        if (_timeline is not null &&
            animation.DirectXAnimation.KeyFrames.Count == oldNode.DirectXAnimation.KeyFrames.Count &&
            animation.WadAnimation.FrameRate == oldNode.WadAnimation.FrameRate)
            cursorPosition = _timeline.Value;

        var newItem = new AnimListItem(animation, GetAnimationLabel(animation));
        var wasSelected = ReferenceEquals(item, SelectedAnim);
        var index = Animations.IndexOf(item);

        _rebuildingList = true;

        try
        {
            Animations[index] = newItem;

            if (wasSelected)
                SelectedAnim = newItem;
        }
        finally
        {
            _rebuildingList = false;
        }

        if (focus)
        {
            if (ReferenceEquals(SelectedAnim, newItem))
                SelectAnimation(animation);
            else
                SelectedAnim = newItem;

            if (_timeline is not null)
                _timeline.Value = cursorPosition != -1 ? cursorPosition : _timeline.Minimum;

            UpdateTransformUI();
        }

        SetSaved(false);
    }

    private int FindStateByName(string name)
    {
        for (uint i = 0; i < MaxStateId; i++)
        {
            var stateName = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, i);

            if (!stateName.Contains("Unknown") && stateName.Contains(name, StringComparison.OrdinalIgnoreCase))
                return (int)i;
        }

        return -1;
    }

    [RelayCommand]
    private void ApplyFilter() => RebuildAnimationsList();

    [RelayCommand]
    private void ResetFilter()
    {
        FilterText = string.Empty;
        RebuildAnimationsList();
    }

    [RelayCommand]
    private void DeleteSelectedAnimations()
    {
        if (_selectedAnimItems.Count == 0)
            return;

        if (UserDialogs.Show(Owner, "Do you really want to delete all selected animations?", "Confirm", DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.No)
            return;

        var nodes = _selectedAnimItems.Select(item => item.Node).ToList();

        for (int i = 0; i < nodes.Count; i++)
            DeleteAnimationCore(nodes[i], i == nodes.Count - 1);
    }
}

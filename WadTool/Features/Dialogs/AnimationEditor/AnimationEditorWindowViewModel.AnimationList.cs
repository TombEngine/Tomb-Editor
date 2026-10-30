#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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

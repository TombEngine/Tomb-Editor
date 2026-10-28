#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.Graphics;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Represents an animation in the editor's selection list.</summary>
public sealed partial class AnimListItem : ObservableObject
{
    [ObservableProperty] private string _label;

    /// <summary>Gets the animation represented by this list entry.</summary>
    public AnimationNode Node { get; }

    public AnimListItem(AnimationNode node, string label)
    {
        Node = node;
        _label = label;
    }
}

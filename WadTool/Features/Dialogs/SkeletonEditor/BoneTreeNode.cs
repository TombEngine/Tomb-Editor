#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WadTool.Features.Dialogs.SkeletonEditor;

/// <summary>
/// One node of the skeleton <see cref="System.Windows.Controls.TreeView"/>, wrapping a
/// <see cref="WadMeshBoneNode"/> (legacy <c>DarkTreeNode.Tag</c>). The hierarchy is rebuilt from the
/// bone opcodes (push / pop / read) on every structural change, like the legacy <c>LoadSkeleton</c>.
/// </summary>
public sealed partial class BoneTreeNode : ObservableObject
{
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isExpanded = true;

    public WadMeshBoneNode BoneNode { get; }

    public ObservableCollection<BoneTreeNode> Children { get; } = new();

    public BoneTreeNode(WadMeshBoneNode boneNode, string text)
    {
        BoneNode = boneNode;
        _text = text;
    }
}

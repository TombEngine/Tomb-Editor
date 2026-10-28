#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Represents a mesh included in animation bounding-box calculations.</summary>
public sealed partial class MeshBoneItem : ObservableObject
{
    [ObservableProperty] private bool _checked = true;

    /// <summary>Gets the mesh index.</summary>
    public int Index { get; }

    /// <summary>Gets the displayed bone name.</summary>
    public string Name { get; }

    public MeshBoneItem(int index, string name)
    {
        Index = index;
        Name = name;
    }
}

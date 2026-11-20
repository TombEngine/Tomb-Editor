#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.WPF;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable list of skeleton meshes used by the current animation.</summary>
public partial class SkeletonPanel : UserControl
{
    public SkeletonPanel()
    {
        InitializeComponent();
    }

    private void OnBoneListDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if ((e.OriginalSource as DependencyObject)?.FindAncestor<DataGridRow>()?.Item is MeshBoneItem bone)
            bone.Checked = !bone.Checked;
    }
}

#nullable enable

using System.Windows.Controls;
using System.Windows.Input;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable bounding box editor of the Animation Editor.</summary>
public partial class BoundingBoxPanel : UserControl
{
    public BoundingBoxPanel()
    {
        InitializeComponent();
    }

    private void OnParameterLostFocus(object sender, KeyboardFocusChangedEventArgs e) => PanelHelper.ResetUndoCoalescing(this);
}

#nullable enable

using System.Windows.Controls;
using System.Windows.Input;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable bounding box editor of the Animation Editor.</summary>
public partial class BoundingBoxPanel : UserControl
{
    public BoundingBoxPanel()
    {
        InitializeComponent();
    }

    private void OnParameterLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        => (DataContext as AnimationEditorWindowViewModel)?.ResetUndoCoalescing();
}
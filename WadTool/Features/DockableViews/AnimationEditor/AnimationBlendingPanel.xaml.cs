#nullable enable

using System.Windows.Controls;
using System.Windows.Input;
using WadTool.Controls;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable blend frame count and curve editor.</summary>
public partial class AnimationBlendingPanel : UserControl
{
    public AnimationBlendingPanel()
    {
        InitializeComponent();

        AddHandler(Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnParameterLostFocus), true);
    }

    /// <summary>The curve editor hosted by the panel.</summary>
    public BezierCurveEditor Curve => bezierEditor;

    private void OnParameterLostFocus(object sender, KeyboardFocusChangedEventArgs e) => PanelHelper.ResetUndoCoalescing(this);
}

#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WadTool.Features.Dialogs.AnimEditor;

namespace WadTool.Features.DockableViews.AnimationEditor;

/// <summary>Dockable editor of the name, state and timing of the current animation.</summary>
public partial class CurrentAnimationPanel : UserControl
{
    public CurrentAnimationPanel()
    {
        InitializeComponent();

        AddHandler(Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnParameterLostFocus), true);
        stateIdBox.AddHandler(KeyUpEvent, new KeyEventHandler(OnStateIdKeyUp), true);
        stateIdBox.AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnStateIdLostFocus), true);

        // The text was set before the box template existed; dock panels apply it only when first shown.
        stateIdBox.Loaded += (_, _) =>
        {
            if (stateIdBox.InputTextBox is not null)
                stateIdBox.InputTextBox.Text = stateIdBox.Text;
        };
    }

    private AnimationEditorWindowViewModel? ViewModel => DataContext as AnimationEditorWindowViewModel;

    /// <summary>Moves the keyboard focus to the animation name field.</summary>
    public void FocusName() => animationNameBox.Focus();

    private void OnParameterLostFocus(object sender, KeyboardFocusChangedEventArgs e) => PanelHelper.ResetUndoCoalescing(this);

    private void OnParameterKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            PanelHelper.ResetUndoCoalescing(this);
    }

    /// <summary>Enter accepts a suggestion and commits the typed state in one go, like the previous State field.</summary>
    private void OnStateIdKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && stateIdBox.InputTextBox is not null)
            stateIdBox.Text = stateIdBox.InputTextBox.Text;
    }

    /// <summary>WinForms commits the typed state when the field is left (Validated), not only on Enter.</summary>
    private void OnStateIdLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!stateIdBox.IsKeyboardFocusWithin && stateIdBox.InputTextBox is not null && stateIdBox.Text != stateIdBox.InputTextBox.Text)
            stateIdBox.Text = stateIdBox.InputTextBox.Text;
    }

    // Like the WinForms combo box, the wheel over a closed State list steps through the known states.
    private void StateCombo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (ViewModel is null || sender is not ComboBox { IsDropDownOpen: false })
            return;

        ViewModel.StepState(e.Delta > 0 ? -1 : 1);
        e.Handled = true;
    }

    private void StateSearchButton_Click(object sender, RoutedEventArgs e)
    {
        stateCombo.OpenAndFocusSearch();
    }
}

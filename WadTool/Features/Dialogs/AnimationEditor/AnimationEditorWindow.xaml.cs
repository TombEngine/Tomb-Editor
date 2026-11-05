#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Wad;
using TombLib.WPF;
using TombLib.WPF.CustomControls;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Native WPF AnimationEditor view with a Direct3D viewport and MVVM editing tools.</summary>
public partial class AnimationEditorWindow : Window
{
    private readonly PanelRenderingAnimationEditor _panel;
    private readonly AnimationTrackBar _timeline;
    private readonly BezierCurveEditor _bezier;
    private AnimationEditorWindowViewModel? _vm;

    public AnimationEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panel = new PanelRenderingAnimationEditor();
        _timeline = timeline;
        _bezier = bezierEditor;
        panelRenderingHost.Child = _panel;

        _timeline.ValueChanged += OnTimelineValueChanged;
        _timeline.SelectionChanged += OnTimelineSelectionChanged;
        _timeline.AnimCommandDoubleClick += OnTimelineAnimCommandDoubleClick;
        animationList.SelectionChanged += OnAnimationListSelectionChanged;
        _bezier.ValueChanged += OnBezierValueChanged;
        _panel.PointerEntered += OnPanelPointerEntered;
        _panel.PointerDragged += OnPanelPointerDragged;

        stateIdBox.AddHandler(KeyUpEvent, new KeyEventHandler(OnStateIdKeyUp), true);
        stateIdBox.AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnStateIdLostFocus), true);

        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) =>
        {
            // The text was set before the box template existed.
            if (stateIdBox.InputTextBox is not null)
                stateIdBox.InputTextBox.Text = stateIdBox.Text;

            if (_vm is not null)
                TombLib.WPF.WindowConfiguration.ConfigureWindow(this, _vm.Configuration, key: "FormAnimationEditor");
        };
        Closing += OnClosingHandler;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.NameFocusRequested -= OnNameFocusRequested;

        _vm = e.NewValue as AnimationEditorWindowViewModel;

        if (_vm is null)
            return;

        _vm.NameFocusRequested += OnNameFocusRequested;
        _vm.AttachControls(_panel, _timeline, _bezier);
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

    private void OnNameFocusRequested(object? sender, EventArgs e) => animationNameBox.Focus();

    /// <summary>
    /// Keyboard shortcuts of the previous form, mapped to the existing commands. They are ignored while a text
    /// input has focus.
    /// </summary>
    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled || _vm is null)
            return;

        bool control = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control;
        bool none = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None;
        bool textInput = System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase or System.Windows.Controls.ComboBox;

        // Text inputs keep plain keys and Ctrl+A/X/C/V; the other Ctrl shortcuts stay global like in the previous form.
        if (textInput && !(control && e.Key is System.Windows.Input.Key.S or System.Windows.Input.Key.Z or System.Windows.Input.Key.Y or System.Windows.Input.Key.I))
            return;

        if (HandleSelectionKey(e, control, none))
            return;

        System.Windows.Input.ICommand? command = e.Key switch
        {
            System.Windows.Input.Key.Z when control => _vm.UndoCommand,
            System.Windows.Input.Key.Y when control => _vm.RedoCommand,
            System.Windows.Input.Key.S when control => _vm.SaveCommand,
            System.Windows.Input.Key.I when control => _vm.InterpolateCommand,
            System.Windows.Input.Key.X when control => _vm.CutFrameCommand,
            System.Windows.Input.Key.C when control => _vm.CopyFrameCommand,
            System.Windows.Input.Key.V when control => _vm.PasteFrameCommand,
            System.Windows.Input.Key.Space when none => _vm.PlayCommand,
            System.Windows.Input.Key.Up when none => _vm.GotoStartCommand,
            System.Windows.Input.Key.Down when none => _vm.GotoEndCommand,
            System.Windows.Input.Key.Delete when none => _vm.DeleteFrameCommand,
            _ => null
        };

        if (command is null || !command.CanExecute(null))
            return;

        command.Execute(null);
        e.Handled = true;
    }
    private bool HandleSelectionKey(System.Windows.Input.KeyEventArgs e, bool control, bool none)
    {
        if (_vm is null)
            return false;

        switch (e.Key)
        {
            case System.Windows.Input.Key.Left when none:
                _vm.StepFrameLooping(false);
                break;

            case System.Windows.Input.Key.Right when none:
                _vm.StepFrameLooping(true);
                break;

            case System.Windows.Input.Key.I when none:
                _vm.MarkSelection(true);
                break;

            case System.Windows.Input.Key.O when none:
                _vm.MarkSelection(false);
                break;

            case System.Windows.Input.Key.A when control:
                _vm.SelectAllFrames();
                break;

            case System.Windows.Input.Key.Escape when none:
                _vm.ClearSelections();
                break;

            default:
                return false;
        }

        e.Handled = true;
        return true;
    }
    private void OnTimelineValueChanged(object? sender, EventArgs e) => _vm?.OnTimelineValueChanged();

    private void OnTimelineSelectionChanged(object? sender, EventArgs e) => _vm?.OnTimelineSelectionChanged();

    private void OnTimelineAnimCommandDoubleClick(object? sender, WadAnimCommand command) => _vm?.EditAnimCommands(command);

    private void OnAnimationListSelectionChanged(object sender, SelectionChangedEventArgs e)
        => _vm?.SetSelectedAnimations(animationList.SelectedItems.Cast<AnimListItem>());

    private void OnAnimationListClick(object sender, MouseButtonEventArgs e) => _vm?.ResetChainedPlaybackStart();

    private void OnBezierValueChanged(object? sender, EventArgs e) => _vm?.OnBlendCurveEdited();

    private void OnPanelPointerEntered(object? sender, EventArgs e) => _vm?.ResetUndoCoalescing();

    private void OnPanelPointerDragged(object? sender, EventArgs e) => _vm?.RefreshTransformUI();

    private void OnParameterLostFocus(object sender, KeyboardFocusChangedEventArgs e) => _vm?.ResetUndoCoalescing();

    private void OnBoneListDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if ((e.OriginalSource as DependencyObject)?.FindAncestor<DataGridRow>()?.Item is MeshBoneItem bone)
            bone.Checked = !bone.Checked;
    }

    private void OnParameterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            _vm?.ResetUndoCoalescing();
    }

    private void OnClosingHandler(object? sender, CancelEventArgs e)
    {
        if (_vm is not null && !_vm.HandleClosing())
            e.Cancel = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panel.Dispose();

        _timeline.ValueChanged -= OnTimelineValueChanged;
        _timeline.SelectionChanged -= OnTimelineSelectionChanged;
        _timeline.AnimCommandDoubleClick -= OnTimelineAnimCommandDoubleClick;
        animationList.SelectionChanged -= OnAnimationListSelectionChanged;
        _bezier.ValueChanged -= OnBezierValueChanged;
        _panel.PointerEntered -= OnPanelPointerEntered;
        _panel.PointerDragged -= OnPanelPointerDragged;
        _vm?.Detach();
    }
}

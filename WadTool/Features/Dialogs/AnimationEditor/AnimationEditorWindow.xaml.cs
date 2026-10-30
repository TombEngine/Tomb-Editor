#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        stateIdBox.LostKeyboardFocus += (_, _) => _vm?.CommitStateId();
        stateIdBox.KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) _vm?.CommitStateId(); };

        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) =>
        {
            if (_vm is not null)
                TombLib.WPF.WindowConfiguration.ConfigureWindow(this, _vm.Configuration, key: "FormAnimationEditor");
        };
        Closing += OnClosingHandler;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = e.NewValue as AnimationEditorWindowViewModel;
        _vm?.AttachControls(_panel, _timeline, _bezier);
    }

    /// <summary>
    /// Keyboard shortcuts of the previous form, mapped to the existing commands. They are ignored while a text
    /// input has focus.
    /// </summary>
    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled || _vm is null || System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase or System.Windows.Controls.ComboBox)
            return;

        bool control = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control;
        bool none = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None;

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

    private void OnBezierValueChanged(object? sender, EventArgs e) => _vm?.OnBlendCurveEdited();

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
        _vm?.Detach();
    }
}

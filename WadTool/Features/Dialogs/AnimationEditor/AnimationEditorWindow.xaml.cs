#nullable enable

using DarkUI.WPF.AvalonDock;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
    private readonly DockLayoutHelper _dock;
    private AnimationEditorWindowViewModel? _vm;

    public AnimationEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panel = new PanelRenderingAnimationEditor();
        _timeline = timelinePanel.Track;
        _bezier = blendingPanel.Curve;
        _dock = new DockLayoutHelper(dockManager);
        _dock.CaptureDefault();

        _bezier.ValueChanged += OnBezierValueChanged;
        _panel.PointerEntered += OnPanelPointerEntered;
        _panel.PointerDragged += OnPanelPointerDragged;

        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) =>
        {
            panelRenderingHost.Child ??= _panel;

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
        RestoreDockLayout();
    }

    private void OnNameFocusRequested(object? sender, EventArgs e) => currentAnimationPanel.FocusName();

    /// <summary>
    /// Keyboard shortcuts of the previous form, mapped to the existing commands. They are ignored while a text
    /// input has focus.
    /// </summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled || _vm is null)
            return;

        bool control = Keyboard.Modifiers == ModifierKeys.Control;
        bool none = Keyboard.Modifiers == ModifierKeys.None;
        bool textInput = Keyboard.FocusedElement is TextBoxBase or ComboBox;

        // Text inputs keep plain keys and Ctrl+A/X/C/V; the other Ctrl shortcuts stay global like in the previous form.
        if (textInput && !(control && e.Key is Key.S or Key.Z or Key.Y or Key.I))
            return;

        if (HandleSelectionKey(e, control, none))
            return;

        ICommand? command = e.Key switch
        {
            Key.Z when control => _vm.UndoCommand,
            Key.Y when control => _vm.RedoCommand,
            Key.S when control => _vm.SaveCommand,
            Key.I when control => _vm.InterpolateCommand,
            Key.X when control => _vm.CutFrameCommand,
            Key.C when control => _vm.CopyFrameCommand,
            Key.V when control => _vm.PasteFrameCommand,
            Key.Space when none => _vm.PlayCommand,
            Key.Up when none => _vm.GotoStartCommand,
            Key.Down when none => _vm.GotoEndCommand,
            Key.Delete when none => _vm.DeleteFrameCommand,
            _ => null
        };

        if (command is null || !command.CanExecute(null))
            return;

        command.Execute(null);
        e.Handled = true;
    }

    private bool HandleSelectionKey(KeyEventArgs e, bool control, bool none)
    {
        if (_vm is null)
            return false;

        switch (e.Key)
        {
            case Key.Left when none:
                _vm.StepFrameLooping(false);
                break;

            case Key.Right when none:
                _vm.StepFrameLooping(true);
                break;

            case Key.I when none:
                _vm.MarkSelection(true);
                break;

            case Key.O when none:
                _vm.MarkSelection(false);
                break;

            case Key.A when control:
                _vm.SelectAllFrames();
                break;

            case Key.Escape when none:
                _vm.ClearSelections();
                break;

            default:
                return false;
        }

        e.Handled = true;
        return true;
    }

    private void OnBezierValueChanged(object? sender, EventArgs e) => _vm?.OnBlendCurveEdited();

    private void OnPanelPointerEntered(object? sender, EventArgs e) => _vm?.ResetUndoCoalescing();

    private void OnPanelPointerDragged(object? sender, EventArgs e) => _vm?.RefreshTransformUI();

    private void OnClosingHandler(object? sender, CancelEventArgs e)
    {
        if (_vm is not null && !_vm.HandleClosing())
        {
            e.Cancel = true;
            return;
        }

        if (_vm is not null)
            _vm.Configuration.AnimationEditor_DockLayout = _dock.Serialize();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panel.Dispose();

        _bezier.ValueChanged -= OnBezierValueChanged;
        _panel.PointerEntered -= OnPanelPointerEntered;
        _panel.PointerDragged -= OnPanelPointerDragged;
        _vm?.Detach();
    }
}

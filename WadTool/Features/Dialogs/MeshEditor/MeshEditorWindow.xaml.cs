#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TombLib.Forms;
using TombLib.WPF;
using TombLib.WPF.Input;
using WadTool.Controls;

using PopUpInfo = TombLib.WPF.PopUpInfo;

namespace WadTool.Features.Dialogs.MeshEditor;

/// <summary>Native WPF MeshEditor view with a Direct3D viewport and MVVM editing tools.</summary>
public partial class MeshEditorWindow : Window
{
    private readonly PanelRenderingMesh _panelMesh;
    private readonly PanelTextureMap _panelTextureMap;
    private readonly PopUpInfo _popup = new();
    private MeshEditorWindowViewModel? _vm;

    public MeshEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panelMesh = new PanelRenderingMesh();
        _panelTextureMap = textureMap;
        meshHost.Child = _panelMesh;
        _panelTextureMap.SelectedTextureChanged += OnTextureSelectionChanged;

        meshTree.SelectedItemChanged += OnTreeSelectedItemChanged;
        meshTree.MouseDoubleClick += OnTreeMouseDoubleClick;

        DataContextChanged += OnDataContextChanged;
        PreviewKeyDown += OnPreviewKeyDown;
        Closing += OnClosingHandler;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.EnsureSelectionVisible -= OnEnsureSelectionVisible;
        }

        _vm = e.NewValue as MeshEditorWindowViewModel;
        if (_vm is null)
            return;

        _vm.MessageRaised += OnMessageRaised;
        _vm.EnsureSelectionVisible += OnEnsureSelectionVisible;
        _vm.AttachPanels(_panelMesh, _panelTextureMap);
    }

    private void OnClosingHandler(object? sender, CancelEventArgs e)
    {
        if (_vm is not null && !_vm.HandleClosing())
            e.Cancel = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panelMesh.Dispose();
        _popup.Dispose();

        _panelTextureMap.SelectedTextureChanged -= OnTextureSelectionChanged;
        meshTree.SelectedItemChanged -= OnTreeSelectedItemChanged;
        meshTree.MouseDoubleClick -= OnTreeMouseDoubleClick;

        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.EnsureSelectionVisible -= OnEnsureSelectionVisible;
            _vm.Detach();
        }
    }

    private void OnTreeSelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        => _vm?.SelectTreeNode(e.NewValue as MeshEditorTreeNode);

    private void OnTreeMouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (meshTree.SelectedItem is MeshEditorTreeNode { IsMesh: true } && _vm?.RenameCommand.CanExecute(null) == true)
            _vm.RenameCommand.Execute(null);
    }

    private void OnTextureSelectionChanged(object? sender, EventArgs e) => _vm?.NotifyTextureSelectionChanged();

    private void OnMessageRaised(string message, PopupType type) => PopUpInfo.Show(_popup, null, _panelMesh, message, type);

    private void OnEnsureSelectionVisible()
    {
        // Best-effort: bring the selected tree item into view once the container exists.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (meshTree.ItemContainerGenerator.ContainerFromItem(meshTree.SelectedItem) is FrameworkElement fe)
                fe.BringIntoView();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Handled || _vm is null || !_vm.ShowEditingTools ||
            KeyboardInput.IsEditingText(e.OriginalSource as DependencyObject) || KeyboardInput.IsEditingText(Keyboard.FocusedElement as DependencyObject))
        {
            return;
        }

        bool control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (e.Key == Key.Escape)
        {
            _vm.ClearSelection();
            e.Handled = true;
            return;
        }

        var command = e.Key switch
        {
            Key.Z when control && _vm.UndoEnabled => _vm.UndoCommand,
            Key.Y when control && _vm.RedoEnabled => _vm.RedoCommand,
            Key.F when control && _vm.IsFaceMode => _vm.FindTextureCommand,
            Key.OemMinus or Key.OemPlus or Key.OemTilde or Key.OemBackslash when _panelTextureMap.VisibleTexture is { IsAvailable: true } =>
                shift ? _vm.MirrorTextureCommand : _vm.RotateTextureCommand,
            _ => null
        };
        if (command?.CanExecute(null) != true)
            return;

        command.Execute(null);
        e.Handled = true;
    }
}

#nullable enable

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.WPF;
using WadTool.Controls;

using PopUpInfo = TombLib.WPF.PopUpInfo;

namespace WadTool.Features.Dialogs.SkeletonEditor;

/// <summary>Native WPF SkeletonEditor view with a Direct3D viewport and MVVM editing tools.</summary>
public partial class SkeletonEditorWindow : Window
{
    private readonly PanelRenderingSkeleton _panel;
    private readonly PopUpInfo _popup = new();
    private SkeletonEditorWindowViewModel? _vm;
    private System.Drawing.Point _startPoint;

    public SkeletonEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panel = new PanelRenderingSkeleton();
        _panel.MouseDown += OnPanelMouseDown;
        _panel.MouseUp += OnPanelMouseUp;
        _panel.MouseDoubleClick += OnPanelMouseDoubleClick;
        renderingHost.Child = _panel;

        treeSkeleton.SelectedItemChanged += OnTreeSelectedItemChanged;
        DataContextChanged += OnDataContextChanged;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.MessageRaised -= OnMessageRaised;

        _vm = e.NewValue as SkeletonEditorWindowViewModel;
        if (_vm is null)
            return;

        _vm.MessageRaised += OnMessageRaised;
        _vm.AttachPanel(_panel);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panel.Dispose();
        _popup.Dispose();

        _panel.MouseDown -= OnPanelMouseDown;
        _panel.MouseUp -= OnPanelMouseUp;
        _panel.MouseDoubleClick -= OnPanelMouseDoubleClick;

        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.Detach();
        }
    }

    private void OnTreeSelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        => _vm?.SelectTreeNode(e.NewValue as BoneTreeNode);

    private void OnPanelMouseDown(object? sender, MouseEventArgs e) => _startPoint = e.Location;

    private void OnPanelMouseUp(object? sender, MouseEventArgs e)
    {
        // Mirror the legacy panelRendering_MouseUp: sync the tree to the picked bone and, on a
        // right-click that wasn't a drag, open the bone context menu.
        _vm?.SyncTreeSelectionFromPanel();

        if (e.Button == MouseButtons.Right &&
            Math.Abs(e.X - _startPoint.X) < 2 && Math.Abs(e.Y - _startPoint.Y) < 2 &&
            treeSkeleton.ContextMenu is not null)
        {
            treeSkeleton.ContextMenu.PlacementTarget = treeSkeleton;
            treeSkeleton.ContextMenu.IsOpen = true;
        }

        _startPoint = System.Drawing.Point.Empty;
    }

    private void OnPanelMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _vm?.EditMeshCommand.CanExecute(null) == true)
            _vm.EditMeshCommand.Execute(null);
    }

    private void OnMessageRaised(string message, PopupType type)
        => PopUpInfo.Show(_popup, null, _panel, message, type);
}

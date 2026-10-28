#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TombEditor.Features.ContextMenus;

namespace TombEditor.Features.Panel3D.ToolPalette;

public partial class ToolboxView : UserControl
{
    private readonly DispatcherTimer _contextMenuTimer;
    private readonly ToolboxViewModel? _viewModel;
    private ContextMenu? _gridPaintMenu;

    public ToolboxView()
    {
        InitializeComponent();

        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            _viewModel = new ToolboxViewModel();
            DataContext = _viewModel;
        }

        _contextMenuTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _contextMenuTimer.Tick += OnContextMenuTimerTick;
    }

    /// <summary>Stops pending interaction and releases editor subscriptions.</summary>
    public void Cleanup()
    {
        _contextMenuTimer.Stop();
        _viewModel?.Cleanup();

        if (_gridPaintMenu is not null)
            _gridPaintMenu.IsOpen = false;
    }

    private void OnGridPaintMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.RightButton == MouseButtonState.Pressed)
            ShowGridPaintContextMenu();
        else
            _contextMenuTimer.Start();
    }

    private void OnGridPaintMouseUp(object sender, MouseButtonEventArgs e) => _contextMenuTimer.Stop();

    private void OnContextMenuTimerTick(object? sender, EventArgs e)
    {
        _contextMenuTimer.Stop();
        ShowGridPaintContextMenu();
    }

    private void ShowGridPaintContextMenu()
    {
        _gridPaintMenu = GridPaintWpfContextMenu.Create(Editor.Instance);
        _gridPaintMenu.PlacementTarget = this;
        _gridPaintMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
        _gridPaintMenu.IsOpen = true;
    }
}

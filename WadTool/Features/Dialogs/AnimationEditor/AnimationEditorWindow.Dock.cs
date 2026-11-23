#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Dockable panel handling of the Animation Editor window.</summary>
public partial class AnimationEditorWindow
{
    private const string BlendingPanelId = "animationBlending";
    private const string RootMotionPanelId = "rootMotion";
    private const string TransformPanelId = "transform";

    private void RestoreDockLayout()
    {
        if (_vm is null)
            return;

        _dock.Load(_vm.Configuration.AnimationEditor_DockLayout);
        _vm.PropertyChanged += OnViewModelPropertyChanged;

        // Layouts saved while the timeline was not floatable still carry that flag.
        if (_dock.Find("timeline") is { } timeline)
            timeline.CanFloat = true;

        ApplyMinimumPanelSizes();

        ApplyProjectPanels();
    }

    // Small enough for the side columns plus the timeline to fit a short window.
    private void ApplyMinimumPanelSizes()
    {
        _dock.SetMinimumSize("animationList", 0.0, 100.0);
        _dock.SetMinimumSize("currentAnimation", 0.0, 190.0);
        _dock.SetMinimumSize(BlendingPanelId, 0.0, 140.0);
        _dock.SetMinimumSize("skeleton", 0.0, 90.0);
        _dock.SetMinimumSize(TransformPanelId, 0.0, 120.0);
        _dock.SetMinimumSize(RootMotionPanelId, 0.0, 50.0);
        _dock.SetMinimumSize("boundingBox", 0.0, 190.0);
        _dock.SetMinimumSize("timeline", 300.0, 84.0);

        // Panels with a fixed set of controls keep their height; the animation and bone lists take the extra space.
        _dock.SetFixedHeight("currentAnimation", 230.0);
        _dock.SetFixedHeight(BlendingPanelId, 230.0);
        _dock.SetFixedHeight(TransformPanelId, 175.0);
        _dock.SetFixedHeight(RootMotionPanelId, 60.0);
        _dock.SetFixedHeight("boundingBox", 250.0);
    }

    private void ApplyProjectPanels()
    {
        if (_vm is null)
            return;

        // TombEngine only panels follow the project type, whatever the saved layout says.
        _dock.SetShown(BlendingPanelId, _vm.IsTombEngine);
        _dock.SetShown(RootMotionPanelId, _vm.IsTombEngine);
        blendingMenuItem.Visibility = _vm.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
        rootMotionMenuItem.Visibility = blendingMenuItem.Visibility;

        UpdateTransformTitle();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AnimationEditorWindowViewModel.TransformHeader))
            UpdateTransformTitle();
    }

    private void UpdateTransformTitle()
    {
        if (_dock.Find(TransformPanelId) is { } anchorable && _vm is not null && !string.IsNullOrEmpty(_vm.TransformHeader))
            anchorable.Title = _vm.TransformHeader;
    }

    private void OnViewMenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menu)
            return;

        foreach (var item in menu.Items)
        {
            if (item is MenuItem { Tag: string contentId } panelItem)
                panelItem.IsChecked = _dock.IsShown(contentId);
        }
    }

    private void OnTogglePanel(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string contentId } item)
            _dock.SetShown(contentId, item.IsChecked);
    }

    private void OnResetLayout(object sender, RoutedEventArgs e)
    {
        _dock.Reset();
        ApplyProjectPanels();
    }
}

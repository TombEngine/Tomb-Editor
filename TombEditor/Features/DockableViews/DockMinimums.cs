#nullable enable

using AvalonDock;
using AvalonDock.Layout;
using System;
using System.Linq;
using System.Windows;

namespace TombEditor.Features.DockableViews;

/// <summary>Keeps splitters from shrinking a docked pane below the size its content asks for.</summary>
public static class DockMinimums
{
    private const double CaptionHeight = 25.0;
    private const double TabStripHeight = 22.0;

    /// <summary>
    /// Sets each pane's minimum size from its content. AvalonDock does not write minimum sizes into saved layouts,
    /// so this runs after every layout load and every time a pane is shown. Panes whose views are not measured
    /// yet drop their old minimum instead of keeping a stale one.
    /// </summary>
    public static void Apply(DockingManager dockManager)
    {
        foreach (var pane in dockManager.Layout.Descendents().OfType<LayoutAnchorablePane>())
        {
            var views = pane.Children.OfType<LayoutAnchorable>()
                .Select(anchorable => anchorable.Content)
                .OfType<FrameworkElement>()
                .Where(view => view.DesiredSize.Width > 0.0 || view.DesiredSize.Height > 0.0)
                .ToList();

            if (views.Count == 0)
            {
                pane.DockMinWidth = 0.0;
                pane.DockMinHeight = 0.0;
                continue;
            }

            var tabStrip = pane.ChildrenCount > 1 ? TabStripHeight : 0.0;
            pane.DockMinWidth = Math.Ceiling(views.Max(view => view.DesiredSize.Width));
            pane.DockMinHeight = Math.Ceiling(views.Max(view => view.DesiredSize.Height) + CaptionHeight + tabStrip);
        }
    }
}
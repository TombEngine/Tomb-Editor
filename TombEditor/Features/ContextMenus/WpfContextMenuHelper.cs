#nullable enable
using System;
using System.Windows;
using System.Windows.Controls;
using TombLib.Icons;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;

namespace TombEditor.Features.ContextMenus;

/// <summary>
/// Shared helpers for the WPF replacements of the legacy DarkContextMenu
/// classes under <c>TombEditor.Controls.ContextMenus</c>.
/// Each WPF context menu builds its items in code (it captures the
/// runtime target passed in at right-click time) and pops itself at the
/// supplied physical screen point. The two functions here factor out
/// the common item / open code so each menu file stays focused on its
/// item list.
/// </summary>
internal static class WpfContextMenuHelper
{
    /// <summary>
    /// Build a <see cref="MenuItem"/> with the supplied header, optional
    /// icon path (e.g. <c>"General/clipboard"</c>), click handler,
    /// enabled and checked flags.
    /// </summary>
    public static MenuItem Item(string header, string? iconPath, Action onClick,
                                bool enabled = true, bool isChecked = false)
    {
        var item = new MenuItem
        {
            Header      = header,
            IsEnabled   = enabled,
            IsChecked   = isChecked,
            IsCheckable = isChecked,
        };
        ApplyImplicitStyle(item);
        if (iconPath != null)
        {
            var image = new Image { Source = IconSources.Load(iconPath) };
            ApplyIconStyle(image);
            item.Icon = image;
        }
        item.Click += (_, _) => onClick();
        return item;
    }

    /// <summary>
    /// Build a styled separator that matches the active DarkUI theme.
    /// </summary>
    public static Separator Sep()
    {
        var sep = new Separator();
        var style = Application.Current?.TryFindResource(System.Windows.Controls.MenuItem.SeparatorStyleKey) as Style;
        if (style != null)
            sep.Style = style;
        return sep;
    }

    /// <summary>
    /// Pop <paramref name="menu"/> at the supplied physical screen point.
    /// Converts pixels to DIPs via the active WPF window's
    /// <see cref="PresentationSource"/> so the popup lands under the
    /// cursor on high-DPI displays. Also wires the menu into the main
    /// window's resource tree so the DarkUI implicit styles resolve.
    /// </summary>
    public static void Open(ContextMenu menu, System.Drawing.Point screenPoint)
    {
        ApplyImplicitStyle(menu);
        menu.PlacementTarget = Application.Current?.MainWindow;
        var dip = PhysicalToDip(screenPoint);
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
        menu.HorizontalOffset = dip.X;
        menu.VerticalOffset   = dip.Y;
        menu.IsOpen = true;
    }

    private static void ApplyImplicitStyle(FrameworkElement element)
    {
        if (element.Style != null)
            return;
        var style = Application.Current?.TryFindResource(element.GetType()) as Style;
        if (style != null)
            element.Style = style;
    }

    private static void ApplyIconStyle(Image image)
    {
        var style = Application.Current?.TryFindResource("SmallImageIcon") as Style;
        if (style != null)
            image.Style = style;
    }

    private static Point PhysicalToDip(System.Drawing.Point screenPoint)
    {
        var window = Application.Current?.MainWindow;
        if (window != null)
        {
            var src = PresentationSource.FromVisual(window);
            if (src?.CompositionTarget != null)
                return src.CompositionTarget.TransformFromDevice.Transform(
                    new Point(screenPoint.X, screenPoint.Y));
        }
        return new Point(screenPoint.X, screenPoint.Y);
    }
}

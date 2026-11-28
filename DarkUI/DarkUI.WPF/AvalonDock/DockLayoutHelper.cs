#nullable enable

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace DarkUI.WPF.AvalonDock;

/// <summary>Saves, restores and toggles the panels of a <see cref="DockingManager"/> whose anchorables are declared in XAML.</summary>
public sealed class DockLayoutHelper
{
    private readonly DockingManager _dockManager;
    private readonly Dictionary<string, (double Width, double Height)> _minimumSizes = new();
    private readonly Dictionary<string, double> _fixedHeights = new();
    private readonly Dictionary<string, LayoutAnchorable> _anchorableCache = new();
    private ILayoutRoot? _cachedLayout;
    private bool _minimumSizeHooked;
    private string _defaultLayout = string.Empty;

    public DockLayoutHelper(DockingManager dockManager)
    {
        _dockManager = dockManager;
    }

    /// <summary>Remembers the current layout as the one <see cref="Reset"/> returns to.</summary>
    public void CaptureDefault() => _defaultLayout = Serialize();

    /// <summary>Serializes the current layout to XML.</summary>
    public string Serialize()
    {
        var serializer = new XmlLayoutSerializer(_dockManager);
        using var writer = new StringWriter();
        serializer.Serialize(writer);
        return writer.ToString();
    }

    /// <summary>Restores a layout, keeping the XAML-declared views. Returns false when the XML is empty or unusable.</summary>
    public bool Load(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        var views = _dockManager.Layout.Descendents().OfType<LayoutContent>()
            .Where(content => !string.IsNullOrEmpty(content.ContentId) && content.Content is not null)
            .GroupBy(content => content.ContentId)
            .ToDictionary(group => group.Key, group => group.First().Content);

        var serializer = new XmlLayoutSerializer(_dockManager);
        serializer.LayoutSerializationCallback += (_, e) =>
        {
            if (e.Model.ContentId is not null && views.TryGetValue(e.Model.ContentId, out var view))
                e.Content = view;
            else
                e.Cancel = true;
        };

        try
        {
            using var reader = new StringReader(xml);
            serializer.Deserialize(reader);
            return true;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.Xml.XmlException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>Returns to the layout captured by <see cref="CaptureDefault"/>.</summary>
    public void Reset() => Load(_defaultLayout);

    /// <summary>
    /// Pins the minimum size of the pane hosting a panel, also after the user docks it elsewhere. A freshly created
    /// pane does not inherit the minimum declared in XAML, and minimums saved in an older layout can be too large for
    /// the panels to fit a short window (the bottom panel then overflows and cannot be resized).
    /// </summary>
    public void SetMinimumSize(string contentId, double minWidth, double minHeight)
    {
        _minimumSizes[contentId] = (minWidth, minHeight);
        HookLayoutUpdates();
        ApplyMinimumSizes();
    }

    /// <summary>
    /// Gives the pane hosting a panel a fixed pixel height instead of a proportional one, so growing the window
    /// only stretches the proportional panes. The user can still drag the splitters.
    /// </summary>
    public void SetFixedHeight(string contentId, double height)
    {
        _fixedHeights[contentId] = height;
        HookLayoutUpdates();
        ApplyMinimumSizes();
    }

    private void HookLayoutUpdates()
    {
        if (_minimumSizeHooked)
            return;

        _minimumSizeHooked = true;
        _dockManager.LayoutUpdated += (_, _) => ApplyMinimumSizes();
    }

    private void ApplyMinimumSizes()
    {
        // Layout updates fire on every layout pass (for example each playback tick), so avoid walking the layout tree.
        var layout = _dockManager.Layout;

        if (!ReferenceEquals(layout, _cachedLayout))
        {
            _cachedLayout = layout;
            _anchorableCache.Clear();
        }

        foreach (var (contentId, (minWidth, minHeight)) in _minimumSizes)
        {
            if (GetPane(contentId) is not { } pane)
                continue;

            if (pane.DockMinWidth != minWidth)
                pane.DockMinWidth = minWidth;

            if (pane.DockMinHeight != minHeight)
                pane.DockMinHeight = minHeight;
        }

        foreach (var (contentId, height) in _fixedHeights)
        {
            if (GetPane(contentId) is { } pane && pane.DockHeight.IsStar)
                pane.DockHeight = new GridLength(height, GridUnitType.Pixel);
        }
    }

    private LayoutAnchorablePane? GetPane(string contentId)
    {
        if (!_anchorableCache.TryGetValue(contentId, out var anchorable))
        {
            anchorable = Find(contentId);

            if (anchorable is not null)
                _anchorableCache[contentId] = anchorable;
        }

        return anchorable?.Parent as LayoutAnchorablePane;
    }

    /// <summary>Finds an anchorable by its content id.</summary>
    public LayoutAnchorable? Find(string contentId)
        => _dockManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(anchorable => anchorable.ContentId == contentId);

    /// <summary>True when the panel exists and is not hidden.</summary>
    public bool IsShown(string contentId) => Find(contentId) is { IsHidden: false };

    /// <summary>Shows or hides a panel, re-docking it when its previous container no longer exists.</summary>
    public void SetShown(string contentId, bool shown)
    {
        var anchorable = Find(contentId);

        if (anchorable is null)
            return;

        if (!shown)
        {
            if (!anchorable.IsHidden)
                anchorable.Hide();

            return;
        }

        if (!anchorable.IsHidden)
            return;

        anchorable.Show();

        // AvalonDock ignores Show() when the previous container was replaced by a layout load.
        if (anchorable.IsHidden)
        {
            _dockManager.Layout.Hidden.Remove(anchorable);
            anchorable.AddToLayout(_dockManager, AnchorableShowStrategy.Left | AnchorableShowStrategy.Most);
        }
    }
}

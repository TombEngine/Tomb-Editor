#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TombLib.LevelData;
using TombLib.Wad;

namespace TombLib.WPF.CustomControls;

/// <summary>A node of <see cref="WadTreeView"/> (category or wad object).</summary>
public sealed partial class WadTreeNode : ObservableObject
{
    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isExpanded;

    public object? Tag { get; set; }
    public WadTreeNode? Parent { get; set; }
    public ObservableCollection<WadTreeNode> Children { get; } = new();
}

/// <summary>
/// WPF rewrite of the shared <c>WadTreeView</c>: shows a wad's moveables / statics / sprite
/// sequences in a multi-select tree, plus game version and metadata editors, with an inline
/// search row replacing the legacy PopUpSearch pop-up. The public API (Wad, SelectedWadObjectIds,
/// Select, UpdateContent, events) mirrors the legacy control; multi-selection is managed in
/// code-behind because the native WPF TreeView is single-select.
/// </summary>
public partial class WadTreeView : UserControl
{
    private Wad2? _wad;
    private bool _changing;
    private bool _updatingVersionCombo;
    private readonly List<WadTreeNode> _selection = new();
    private WadTreeNode? _selectionAnchor;

    public ObservableCollection<WadTreeNode> RootNodes { get; } = new();

    public event EventHandler? ClickOnEmpty;
    public event EventHandler? SelectedWadObjectIdsChanged;
    public event EventHandler? MetadataChanged;
    public event EventHandler? DoubleClick;

    public bool ReadOnly { get; set; }
    public bool MultiSelect { get; set; } = true;

    public Wad2? Wad
    {
        get { return _wad; }
        set { _wad = value; UpdateContent(); }
    }

    public bool ItemSelected => _selection.Count > 0 && _selection[0].Children.Count == 0;

    public IEnumerable<IWadObjectId> SelectedWadObjectIds => _selection.Select(node => node.Tag).OfType<IWadObjectId>();

    public WadTreeView()
    {
        InitializeComponent();

        tree.ItemsSource = RootNodes;

        foreach (var gameVersion in TRVersion.NativeVersions)
            comboGameVersion.Items.Add(gameVersion);

        UpdateContent();
    }

    public void UpdateContent()
    {
        bool wadLoaded = _wad != null;
        contentPanel.Visibility = wadLoaded ? Visibility.Visible : Visibility.Collapsed;
        emptyPanel.Visibility = wadLoaded ? Visibility.Collapsed : Visibility.Visible;

        tbNotes.IsReadOnly = ReadOnly;

        // Update game version control
        _updatingVersionCombo = true;
        try
        {
            comboGameVersion.SelectedItem = wadLoaded ? (object)_wad!.GameVersion : null;
        }
        finally
        {
            _updatingVersionCombo = false;
        }

        // Update metadata
        UpdateMetadata();

        // Update tree
        KeepSelection(() =>
        {
            var nodes = RootNodes.ToList();
            RootNodes.Clear();
            if (_wad == null)
                return;

            {
                var mainNode = AddOrReuseChild(nodes, "Moveables");
                UpdateList(mainNode, _wad.Moveables.Values.Select(o => o.Id), o => o.ToString(_wad.GameVersion));
            }
            {
                var mainNode = AddOrReuseChild(nodes, "Statics");
                UpdateList(mainNode, _wad.Statics.Values.Select(o => o.Id), o => o.ToString(_wad.GameVersion));
            }
            {
                var mainNode = AddOrReuseChild(nodes, "Sprite sequences");
                UpdateList(mainNode, _wad.SpriteSequences.Values.Select(o => o.Id), o => o.ToString(_wad.GameVersion));
            }

            foreach (WadTreeNode node in nodes)
                RootNodes.Add(node);
        });
    }

    public void UpdateMetadata()
    {
        tbDate.Text = _wad?.Timestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
        tbNotes.Text = _wad?.UserNotes;
    }

    private static WadTreeNode AddOrReuseChild(IList<WadTreeNode> nodes, string text)
    {
        foreach (WadTreeNode childNode in nodes)
            if (childNode.Text.Equals(text, StringComparison.InvariantCulture))
                return childNode;
        {
            var childNode = new WadTreeNode { Text = text, IsExpanded = false };
            nodes.Add(childNode);
            return childNode;
        }
    }

    // This function tries to recycle tree nodes to preserve their extended attributes.
    private static void UpdateList<T>(WadTreeNode oldNode, IEnumerable<T> listOfThings, Func<T, string> formatObject) where T : notnull
    {
        IDictionary<object, WadTreeNode> oldChildNodeLookup = oldNode.Children.ToDictionary(node => node.Tag!);
        var newChildNodes = new List<WadTreeNode>();

        foreach (T thing in listOfThings)
        {
            if (!oldChildNodeLookup.TryGetValue(thing, out WadTreeNode? childNode))
                childNode = new WadTreeNode { Tag = thing, IsExpanded = false };
            childNode.Text = formatObject(thing);
            childNode.Parent = oldNode;
            newChildNodes.Add(childNode);
        }

        oldNode.Children.Clear();
        foreach (WadTreeNode childNode in newChildNodes)
            oldNode.Children.Add(childNode);
    }

    private void KeepSelection(Action update)
    {
        var selectedTags = new HashSet<object>(_selection.Select(node => node.Tag).Where(tag => tag != null)!);
        foreach (WadTreeNode node in _selection)
            node.IsSelected = false;
        _selection.Clear();

        update();

        try
        {
            _changing = true;

            // Restore selection
            foreach (WadTreeNode node in CollectAllNodes(RootNodes))
                if (node.Tag != null && selectedTags.Contains(node.Tag))
                {
                    node.IsSelected = true;
                    _selection.Add(node);
                }
        }
        finally
        {
            _changing = false;
        }

        SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);
    }

    public static IEnumerable<WadTreeNode> CollectAllNodes(IEnumerable<WadTreeNode> @this)
    {
        foreach (WadTreeNode node in @this)
        {
            yield return node;
            if (node.Children.Count != 0)
                foreach (WadTreeNode child in CollectAllNodes(node.Children))
                    yield return child;
        }
    }

    public void Select(List<IWadObjectId> idList)
    {
        List<WadTreeNode> selectedNodesList = CollectAllNodes(RootNodes)
            .Where(node => node.Tag is IWadObjectId id && idList.Any(entry => entry.ToString() == id.ToString()))
            .ToList();

        if (selectedNodesList.Count == 0)
            return;

        SetSelection(selectedNodesList, selectedNodesList[0]);

        foreach (WadTreeNode node in selectedNodesList)
            ExpandAncestors(node);

        BringNodeIntoView(selectedNodesList[0]);
    }

    public void Select(IWadObjectId id) => Select(new List<IWadObjectId>() { id });

    public void SelectFirst()
    {
        WadTreeNode? first = CollectAllNodes(RootNodes).FirstOrDefault(node => node.Tag is IWadObjectId);
        if (first?.Tag is IWadObjectId id)
            Select(id);
    }

    private void SetSelection(IReadOnlyList<WadTreeNode> nodes, WadTreeNode? anchor = null)
    {
        if (anchor != null)
            _selectionAnchor = anchor;

        if (nodes.Count == _selection.Count && !nodes.Except(_selection).Any())
            return;

        foreach (WadTreeNode node in _selection)
            node.IsSelected = false;
        _selection.Clear();

        foreach (WadTreeNode node in nodes)
        {
            node.IsSelected = true;
            _selection.Add(node);
        }

        if (!_changing)
            SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void ExpandAncestors(WadTreeNode node)
    {
        for (WadTreeNode? current = node.Parent; current != null; current = current.Parent)
            current.IsExpanded = true;
    }

    private void BringNodeIntoView(WadTreeNode node)
    {
        Dispatcher.BeginInvoke(new Action(() => GetContainer(node)?.BringIntoView()), DispatcherPriority.Background);
    }

    private TreeViewItem? GetContainer(WadTreeNode node)
    {
        var path = new Stack<WadTreeNode>();
        for (WadTreeNode? current = node; current != null; current = current.Parent)
            path.Push(current);

        ItemsControl parent = tree;
        TreeViewItem? container = null;

        while (path.Count > 0)
        {
            WadTreeNode current = path.Pop();
            parent.UpdateLayout();
            container = parent.ItemContainerGenerator.ContainerFromItem(current) as TreeViewItem;
            if (container == null)
                return null;
            parent = container;
        }

        return container;
    }

    private void NodeContent_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not WadTreeNode node)
            return;

        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            // Like the legacy control (ExpandOnDoubleClick = false): double click never toggles
            // expansion, it just notifies the consumer. Handling the event here keeps the native
            // TreeViewItem from expanding.
            SetSelection(new[] { node }, node);
            DoubleClick?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            ModifierKeys modifiers = Keyboard.Modifiers;

            if (MultiSelect && modifiers.HasFlag(ModifierKeys.Control))
            {
                var newSelection = new List<WadTreeNode>(_selection);
                if (!newSelection.Remove(node))
                    newSelection.Add(node);
                SetSelection(newSelection, node);
                e.Handled = true; // Keep the native single-selection from collapsing the set.
            }
            else if (MultiSelect && modifiers.HasFlag(ModifierKeys.Shift) && _selectionAnchor != null)
            {
                var allNodes = CollectAllNodes(RootNodes).ToList();
                int from = allNodes.IndexOf(_selectionAnchor);
                int to = allNodes.IndexOf(node);
                if (from >= 0 && to >= 0)
                {
                    if (from > to)
                        (from, to) = (to, from);
                    SetSelection(allNodes.GetRange(from, to - from + 1));
                }
                e.Handled = true;
            }
            else
            {
                // Plain click: select here and let the native selection/focus proceed so
                // keyboard navigation keeps working.
                SetSelection(new[] { node }, node);
            }
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            // Right click selects the node under the cursor (like DarkTreeView) so an attached
            // context menu targets it; an already-selected node keeps the multi-selection.
            if (!_selection.Contains(node))
                SetSelection(new[] { node }, node);
        }
    }

    private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_changing)
            return;

        // Covers keyboard navigation; mouse clicks already went through NodeContent_MouseDown.
        if (e.NewValue is WadTreeNode node &&
            !Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
            !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            SetSelection(new[] { node }, node);
    }

    private void comboGameVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingVersionCombo || _wad == null || comboGameVersion.SelectedItem == null)
            return;

        _wad.GameVersion = (TRVersion.Game)comboGameVersion.SelectedItem;
        UpdateContent();
    }

    private void tbNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_wad != null)
            _wad.UserNotes = tbNotes.Text;
        MetadataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void emptyPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_wad == null)
            ClickOnEmpty?.Invoke(this, EventArgs.Empty);
    }

    private void butSearch_Click(object sender, RoutedEventArgs e)
    {
        if (searchPanel.Visibility == Visibility.Visible)
        {
            searchPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            searchPanel.Visibility = Visibility.Visible;
            tbSearch.Focus();
            tbSearch.SelectAll();
        }
    }

    private void butSearchNext_Click(object sender, RoutedEventArgs e) => FindNext();

    private void tbSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            FindNext();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            searchPanel.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
    }

    private void FindNext()
    {
        string query = tbSearch.Text;
        if (string.IsNullOrWhiteSpace(query))
            return;

        var allNodes = CollectAllNodes(RootNodes).ToList();
        if (allNodes.Count == 0)
            return;

        // Cycle through matches, starting after the last selected node.
        int start = _selection.Count > 0 ? allNodes.IndexOf(_selection[_selection.Count - 1]) : -1;
        for (int i = 1; i <= allNodes.Count; i++)
        {
            WadTreeNode node = allNodes[(start + i) % allNodes.Count];
            if (node.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                SetSelection(new[] { node }, node);
                ExpandAncestors(node);
                BringNodeIntoView(node);
                return;
            }
        }
    }
}

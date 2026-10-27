using DarkUI.Collections;
using DarkUI.Config;
using DarkUI.Extensions;
using DarkUI.Forms;
using DarkUI.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTreeView : DarkScrollView
    {
        #region Event Region

        public event EventHandler SelectedNodesChanged;
        public event EventHandler AfterNodeExpand;
        public event EventHandler AfterNodeCollapse;
        public event EventHandler NodesMoved;

        #endregion

        #region Field Region

        private bool _disposed;

        private const int ExpandAreaSize = 16;
        private const int IconSize = 16;

        private int _itemHeight = 20;
        private int _indent = 20;

        private ObservableList<DarkTreeNode> _nodes;

        private DarkTreeNode _anchoredNodeStart;
        private DarkTreeNode _anchoredNodeEnd;

        private Bitmap _nodeClosed;
        private Bitmap _nodeClosedHover;
        private Bitmap _nodeClosedHoverSelected;
        private Bitmap _nodeOpen;
        private Bitmap _nodeOpenHover;
        private Bitmap _nodeOpenHoverSelected;

        private DarkTreeNode _provisionalNode;
        private DarkTreeNode _dropNode;
        private DropPosition _dropPosition;
        private int _dropIndicatorY;
        private bool _provisionalDragging;
        private bool _mouseInClientArea;
        private List<DarkTreeNode> _dragNodes;
        private Point _dragPos;

        private readonly Color _borderColor = Colors.LightBorder;

        private enum DropPosition { None, Before, After, Into }

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableList<DarkTreeNode> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != null)
                {
                    _nodes.ItemsAdded -= Nodes_ItemsAdded;
                    _nodes.ItemsRemoved -= Nodes_ItemsRemoved;

                    foreach (var node in _nodes)
                        UnhookNodeEvents(node);
                }

                _nodes = value;

                _nodes.ItemsAdded += Nodes_ItemsAdded;
                _nodes.ItemsRemoved += Nodes_ItemsRemoved;

                foreach (var node in _nodes)
                    HookNodeEvents(node);

                UpdateNodes();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<DarkTreeNode> SelectedNodes { get; }

        [Category("Appearance")]
        [Description("Determines the height of tree nodes.")]
        [DefaultValue(20)]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                _itemHeight = value;
                MaxDragChange = _itemHeight;
                UpdateNodes();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of horizontal space given by parent node.")]
        [DefaultValue(20)]
        public int Indent
        {
            get { return _indent; }
            set
            {
                _indent = value;
                UpdateNodes();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color OverrideOddColor { get; set; } = Color.Transparent;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color OverrideEvenColor { get; set; } = Color.Transparent;

        [Category("Behavior")]
        [Description("Determines whether parent node should be expanded on mouse double-click.")]
        [DefaultValue(true)]
        public bool ExpandOnDoubleClick { get; set; }

        [Category("Behavior")]
        [Description("Determines whether multiple tree nodes can be selected at once.")]
        [DefaultValue(false)]
        public bool MultiSelect { get; set; }

        [Category("Behavior")]
        [Description("Determines whether nodes can be moved within this tree view.")]
        [DefaultValue(false)]
        public bool AllowMoveNodes { get; set; }

        [Category("Appearance")]
        [Description("Determines whether icons are rendered with the tree nodes.")]
        [DefaultValue(false)]
        public bool ShowIcons { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int VisibleNodeCount { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IComparer<DarkTreeNode> TreeViewNodeSorter { get; set; }

        // Optional predicate to restrict which nodes can act as drop targets.
        // When null, any node may receive drops.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<DarkTreeNode, bool> CanDropIntoNode { get; set; }

        #endregion

        #region Constructor Region

        public DarkTreeView()
        {
            Nodes = new ObservableList<DarkTreeNode>();
            SelectedNodes = new ObservableCollection<DarkTreeNode>();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

            MaxDragChange = _itemHeight;

            LoadIcons();
        }

        #endregion

        #region Dispose Region

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DisposeIcons();

                SelectedNodesChanged = null;
                AfterNodeExpand = null;
                AfterNodeCollapse = null;
                NodesMoved = null;

                _nodes?.Dispose();

                SelectedNodes.CollectionChanged -= SelectedNodes_CollectionChanged;

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Event Handler Region

        private void Nodes_ItemsAdded(object sender, ObservableListModified<DarkTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                node.ParentNode = null;
                node.ParentTree = this;
                node.IsRoot = true;

                HookNodeEvents(node);
            }

            if (TreeViewNodeSorter != null)
                Nodes.Sort(TreeViewNodeSorter);

            UpdateNodes();
        }

        private void Nodes_ItemsRemoved(object sender, ObservableListModified<DarkTreeNode> e)
        {
            foreach (var node in e.Items)
                UnhookNodeEvents(node);

            UpdateNodes();
        }

        private void ChildNodes_ItemsAdded(object sender, ObservableListModified<DarkTreeNode> e)
        {
            foreach (var node in e.Items)
                HookNodeEvents(node);

            UpdateNodes();
        }

        private void ChildNodes_ItemsRemoved(object sender, ObservableListModified<DarkTreeNode> e)
        {
            foreach (var node in e.Items)
            {
                if (SelectedNodes.Contains(node))
                    SelectedNodes.Remove(node);

                UnhookNodeEvents(node);
            }

            UpdateNodes();
        }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedNodesChanged?.Invoke(this, null);
        }

        private void Nodes_TextChanged(object sender, EventArgs e)
        {
            UpdateNodes();
        }

        private void Nodes_NodeExpanded(object sender, EventArgs e)
        {
            UpdateNodes();

            AfterNodeExpand?.Invoke(this, null);
        }

        private void Nodes_NodeCollapsed(object sender, EventArgs e)
        {
            UpdateNodes();

            AfterNodeCollapse?.Invoke(this, null);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_provisionalDragging)
            {
                if (OffsetMousePosition != _dragPos)
                {
                    StartDrag();
                    HandleDrag();
                    return;
                }
            }

            CheckHover();

            if (IsDragging)
                HandleDrag();

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            CheckHover();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                foreach (var node in Nodes)
                    CheckNodeClick(node, OffsetMousePosition, e.Button);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsDragging)
            {
                HandleDrop();
            }

            if (_provisionalDragging)
            {

                if (_provisionalNode != null)
                {
                    var pos = _dragPos;
                    if (OffsetMousePosition == pos)
                        SelectNode(_provisionalNode);
                }

                _provisionalDragging = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
                return;

            if (e.Button == MouseButtons.Left)
            {
                foreach (var node in Nodes)
                    CheckNodeDoubleClick(node, OffsetMousePosition);
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (var node in Nodes)
                NodeMouseLeave(node);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsDragging)
                return;

            if (Nodes.Count == 0)
                return;

            if (e.KeyCode != Keys.Down && e.KeyCode != Keys.Up && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                return;

            if (_anchoredNodeEnd == null)
            {
                if (Nodes.Count > 0)
                    SelectNode(Nodes[0]);
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                    if (MultiSelect && ModifierKeys == Keys.Shift)
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Up:
                                if (_anchoredNodeEnd.PrevVisibleNode != null)
                                {
                                    SelectAnchoredRange(_anchoredNodeEnd.PrevVisibleNode);
                                    EnsureVisible();
                                }
                                break;
                            case Keys.Down:
                                if (_anchoredNodeEnd.NextVisibleNode != null)
                                {
                                    SelectAnchoredRange(_anchoredNodeEnd.NextVisibleNode);
                                    EnsureVisible();
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Up:
                                if (_anchoredNodeEnd.PrevVisibleNode != null)
                                {
                                    SelectNode(_anchoredNodeEnd.PrevVisibleNode);
                                    EnsureVisible();
                                }
                                break;
                            case Keys.Down:
                                if (_anchoredNodeEnd.NextVisibleNode != null)
                                {
                                    SelectNode(_anchoredNodeEnd.NextVisibleNode);
                                    EnsureVisible();
                                }
                                break;
                        }
                    }
                    break;
                case Keys.Left:
                case Keys.Right:
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            if (_anchoredNodeEnd.Expanded && _anchoredNodeEnd.Nodes.Count > 0)
                            {
                                _anchoredNodeEnd.Expanded = false;
                            }
                            else
                            {
                                if (_anchoredNodeEnd.ParentNode != null)
                                {
                                    SelectNode(_anchoredNodeEnd.ParentNode);
                                    EnsureVisible();
                                }
                            }
                            break;
                        case Keys.Right:
                            if (!_anchoredNodeEnd.Expanded)
                            {
                                _anchoredNodeEnd.Expanded = true;
                            }
                            else
                            {
                                if (_anchoredNodeEnd.Nodes.Count > 0)
                                {
                                    SelectNode(_anchoredNodeEnd.Nodes[0]);
                                    EnsureVisible();
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Method Region

        private void HookNodeEvents(DarkTreeNode node)
        {
            node.Nodes.ItemsAdded += ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved += ChildNodes_ItemsRemoved;

            node.TextChanged += Nodes_TextChanged;
            node.NodeExpanded += Nodes_NodeExpanded;
            node.NodeCollapsed += Nodes_NodeCollapsed;

            if (node is DarkTreeNodeEx)
            {
                (node as DarkTreeNodeEx).SubTextChanged += Nodes_TextChanged;
                (node as DarkTreeNodeEx).ExtraIconChanged += Nodes_TextChanged;
            }
               

            foreach (var childNode in node.Nodes)
                HookNodeEvents(childNode);
        }

        private void UnhookNodeEvents(DarkTreeNode node)
        {
            node.Nodes.ItemsAdded -= ChildNodes_ItemsAdded;
            node.Nodes.ItemsRemoved -= ChildNodes_ItemsRemoved;

            node.TextChanged -= Nodes_TextChanged;
            node.NodeExpanded -= Nodes_NodeExpanded;
            node.NodeCollapsed -= Nodes_NodeCollapsed;

            foreach (var childNode in node.Nodes)
                UnhookNodeEvents(childNode);
        }

        private void ClearNodes()
        {
            SelectedNodes.CollectionChanged -= SelectedNodes_CollectionChanged;
            SelectedNodes.Clear();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;
        }

        private void UpdateNodes()
        {
            if (IsDragging)
                return;

            if (Nodes.Count == 0)
                return;

            var yOffset = 0;
            var isOdd = false;
            var index = 0;
            DarkTreeNode prevNode = null;

            ContentSize = new Size(0, 0);

            for (var i = 0; i <= Nodes.Count - 1; i++)
            {
                var node = Nodes[i];
                UpdateNode(node, ref prevNode, 0, ref yOffset, ref isOdd, ref index);
            }

            ContentSize = new Size(ContentSize.Width, yOffset);

            VisibleNodeCount = index;

            Invalidate();
        }

        private void UpdateNode(DarkTreeNode node, ref DarkTreeNode prevNode, int indent, ref int yOffset,
                                ref bool isOdd, ref int index)
        {
            UpdateNodeBounds(node, yOffset, indent);

            yOffset += ItemHeight;

            node.Odd = isOdd;
            isOdd = !isOdd;

            node.VisibleIndex = index;
            index++;

            node.PrevVisibleNode = prevNode;

            if (prevNode != null)
                prevNode.NextVisibleNode = node;

            prevNode = node;

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    UpdateNode(childNode, ref prevNode, indent + Indent, ref yOffset, ref isOdd, ref index);
            }
        }

        private void UpdateNodeBounds(DarkTreeNode node, int yOffset, int indent)
        {
            var expandTop = yOffset + ItemHeight / 2 - ExpandAreaSize / 2;
            node.ExpandArea = new Rectangle(indent + 3, expandTop, ExpandAreaSize, ExpandAreaSize);

            var iconTop = yOffset + ItemHeight / 2 - IconSize / 2;

            if (ShowIcons)
                node.IconArea = new Rectangle(node.ExpandArea.Right + 2, iconTop, IconSize, IconSize);
            else if (node.Nodes.Count > 0)
                node.IconArea = new Rectangle(node.ExpandArea.Right, iconTop, 0, 0);
            else
                node.IconArea = new Rectangle(node.ExpandArea.Left, iconTop, 0, 0);

            using (var g = CreateGraphics())
            {
                if (node is DarkTreeNodeEx)
                {
                    var nodeEx = node as DarkTreeNodeEx;
                    var subTextFont = new Font(Font.FontFamily, 8.25f);

                    var textSize = (int)g.MeasureString(nodeEx.Text, Font).Width;
                    var subTextSize = (int)g.MeasureString(nodeEx.SubText, subTextFont).Width;

                    nodeEx.TextArea = new Rectangle(nodeEx.IconArea.Right + 2, yOffset - (Font.Height / 2) + 2, textSize + 1, ItemHeight);
                    nodeEx.SubTextArea = new Rectangle(nodeEx.IconArea.Right + 4, yOffset + (Font.Height / 2) - 1, subTextSize + 1, ItemHeight);

                    if (ShowIcons && nodeEx.ExtraIcon != null)
                        nodeEx.ExtraIconArea = new Rectangle(nodeEx.TextArea.Right + 4, nodeEx.TextArea.Y + Font.Height + 1, nodeEx.ExtraIcon.Width, ItemHeight);

                    nodeEx.TextArea = new Rectangle(nodeEx.TextArea.X, nodeEx.TextArea.Y,
                        Math.Max(Math.Max(nodeEx.TextArea.Right, nodeEx.SubTextArea.Right), nodeEx.ExtraIconArea.Right) + 1 - indent, ItemHeight);

                    nodeEx.FullArea = new Rectangle(indent, yOffset, nodeEx.TextArea.Right - indent, ItemHeight);
                }
                else
                {
                    var textSize = (int)g.MeasureString(node.Text, Font).Width;
                    node.TextArea = new Rectangle(node.IconArea.Right + 2, yOffset, textSize + 1, ItemHeight);
                    node.FullArea = new Rectangle(indent, yOffset, node.TextArea.Right - indent, ItemHeight);
                }
            }

            if (ContentSize.Width < node.TextArea.Right + 2)
                ContentSize = new Size(node.TextArea.Right + 2, ContentSize.Height);
        }

        private void LoadIcons()
        {
            DisposeIcons();

            _nodeClosed = TreeViewIcons.node_closed_empty.SetColor(Colors.LightText);
            _nodeClosedHover = TreeViewIcons.node_closed_empty.SetColor(Colors.BlueHighlight);
            _nodeClosedHoverSelected = TreeViewIcons.node_closed_full.SetColor(Colors.LightText);
            _nodeOpen = TreeViewIcons.node_open.SetColor(Colors.LightText);
            _nodeOpenHover = TreeViewIcons.node_open.SetColor(Colors.BlueHighlight);
            _nodeOpenHoverSelected = TreeViewIcons.node_open_empty.SetColor(Colors.LightText);
        }

        private void DisposeIcons()
        {
            _nodeClosed?.Dispose();

            _nodeClosedHover?.Dispose();

            _nodeClosedHoverSelected?.Dispose();

            _nodeOpen?.Dispose();

            _nodeOpenHover?.Dispose();

            _nodeOpenHoverSelected?.Dispose();
        }

        private void CheckHover()
        {
            _mouseInClientArea = ClientRectangle.Contains(PointToClient(MousePosition));

            if (!_mouseInClientArea)
            {
                if (IsDragging && _dropNode != null)
                {
                    _dropNode = null;
                    Invalidate();
                }

                return;
            }

            foreach (var node in Nodes)
                CheckNodeHover(node, OffsetMousePosition);
        }

        private void NodeMouseLeave(DarkTreeNode node)
        {
            node.ExpandAreaHot = false;

            foreach (var childNode in node.Nodes)
                NodeMouseLeave(childNode);

            Invalidate();
        }

        private void CheckNodeHover(DarkTreeNode node, Point location)
        {
            if (!IsDragging)
            {
                var hot = node.ExpandArea.Contains(location);
                if (node.ExpandAreaHot != hot)
                {
                    node.ExpandAreaHot = hot;
                    Invalidate();
                }
            }

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    CheckNodeHover(childNode, location);
            }
        }

        public void ExpandAllNodes()
        {
            foreach (var node in Nodes)
                ExpandNodes(node);
        }

        private void ExpandNodes(DarkTreeNode parent)
        {
            parent.EnsureVisible();
            foreach (var node in parent.Nodes)
                ExpandNodes(node);
        }

        private void CheckNodeClick(DarkTreeNode node, Point location, MouseButtons button)
        {
            var rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (node.ExpandArea.Contains(location))
                {
                    if (button == MouseButtons.Left)
                        node.Expanded = !node.Expanded;
                }
                else
                {
                    switch (button)
                    {
                        case MouseButtons.Left:
                            if (MultiSelect && ModifierKeys == Keys.Shift)
                            {
                                SelectAnchoredRange(node);
                            }
                            else if (MultiSelect && ModifierKeys == Keys.Control)
                            {
                                ToggleNode(node);
                            }
                            else
                            {
                                if (!SelectedNodes.Contains(node))
                                    SelectNode(node);

                                _dragPos = OffsetMousePosition;
                                _provisionalDragging = true;
                                _provisionalNode = node;
                            }

                            return;
                        case MouseButtons.Right:
                            if (MultiSelect && ModifierKeys == Keys.Shift)
                                return;

                            if (MultiSelect && ModifierKeys == Keys.Control)
                                return;

                            if (!SelectedNodes.Contains(node))
                                SelectNode(node);

                            return;
                    }
                }
            }

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    CheckNodeClick(childNode, location, button);
            }
        }

        private void CheckNodeDoubleClick(DarkTreeNode node, Point location)
        {
            var rect = GetNodeFullRowArea(node);
            if (rect.Contains(location))
            {
                if (!node.ExpandArea.Contains(location))
                    node.Expanded = !node.Expanded;

                return;
            }

            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    CheckNodeDoubleClick(childNode, location);
            }
        }

        public void SelectNode(DarkTreeNode node)
        {
            ClearNodes();
            SelectedNodes.Add(node);

            _anchoredNodeStart = node;
            _anchoredNodeEnd = node;

            Invalidate();
        }

        public void SelectNodes(DarkTreeNode startNode, DarkTreeNode endNode)
        {
            var nodes = new List<DarkTreeNode>();

            if (startNode == endNode)
                nodes.Add(startNode);

            if (startNode.VisibleIndex < endNode.VisibleIndex)
            {
                var node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.NextVisibleNode;
                    nodes.Add(node);
                }
            }
            else if (startNode.VisibleIndex > endNode.VisibleIndex)
            {
                var node = startNode;
                nodes.Add(node);
                while (node != endNode && node != null)
                {
                    node = node.PrevVisibleNode;
                    nodes.Add(node);
                }
            }

            SelectNodes(nodes, false);
        }

        public void SelectNodes(List<DarkTreeNode> nodes, bool updateAnchors = true)
        {
            ClearNodes();

            foreach (var node in nodes)
                SelectedNodes.Add(node);

            if (updateAnchors && SelectedNodes.Count > 0)
            {
                _anchoredNodeStart = SelectedNodes[SelectedNodes.Count - 1];
                _anchoredNodeEnd = SelectedNodes[SelectedNodes.Count - 1];
            }

            Invalidate();
        }

        public List<DarkTreeNode> GetAllNodes() => GetAllNodes(new List<DarkTreeNode>(), Nodes);
        private List<DarkTreeNode> GetAllNodes(List<DarkTreeNode> list, ObservableList<DarkTreeNode> source)
        {
            foreach (var node in source)
            {
                list.Add(node);
                GetAllNodes(list, node.Nodes);
            }
            return list;
        }

        private void SelectAnchoredRange(DarkTreeNode node)
        {
            _anchoredNodeEnd = node;
            SelectNodes(_anchoredNodeStart, _anchoredNodeEnd);
        }

        public void ToggleNode(DarkTreeNode node)
        {
            if (SelectedNodes.Contains(node))
            {
                SelectedNodes.Remove(node);

                // If we just removed both the anchor start AND end then reset them
                if (_anchoredNodeStart == node && _anchoredNodeEnd == node)
                {
                    if (SelectedNodes.Count > 0)
                    {
                        _anchoredNodeStart = SelectedNodes[0];
                        _anchoredNodeEnd = SelectedNodes[0];
                    }
                    else
                    {
                        _anchoredNodeStart = null;
                        _anchoredNodeEnd = null;
                    }
                }

                // If we just removed the anchor start then update it accordingly
                if (_anchoredNodeStart == node)
                {
                    Debug.Assert(_anchoredNodeEnd != null, nameof(_anchoredNodeEnd) + " != null");
                    if (_anchoredNodeEnd.VisibleIndex < node.VisibleIndex)
                        _anchoredNodeStart = node.PrevVisibleNode;
                    else if (_anchoredNodeEnd.VisibleIndex > node.VisibleIndex)
                        _anchoredNodeStart = node.NextVisibleNode;
                    else
                        _anchoredNodeStart = _anchoredNodeEnd;
                }

                // If we just removed the anchor end then update it accordingly
                if (_anchoredNodeEnd == node)
                {
                    Debug.Assert(_anchoredNodeStart != null, nameof(_anchoredNodeStart) + " != null");
                    if (_anchoredNodeStart.VisibleIndex < node.VisibleIndex)
                        _anchoredNodeEnd = node.PrevVisibleNode;
                    else if (_anchoredNodeStart.VisibleIndex > node.VisibleIndex)
                        _anchoredNodeEnd = node.NextVisibleNode;
                    else
                        _anchoredNodeEnd = _anchoredNodeStart;
                }
            }
            else
            {
                SelectedNodes.Add(node);

                _anchoredNodeStart = node;
                _anchoredNodeEnd = node;
            }

            Invalidate();
        }

        public Rectangle GetNodeFullRowArea(DarkTreeNode node)
        {
            if (node.ParentNode != null && !node.ParentNode.Expanded)
                return new Rectangle(-1, -1, -1, -1);

            var width = Math.Max(ContentSize.Width, Viewport.Width);
            var rect = new Rectangle(0, node.FullArea.Top, width, ItemHeight);
            return rect;
        }

        public void EnsureVisible()
        {
            if (SelectedNodes.Count == 0)
                return;

            foreach (var node in SelectedNodes)
                node.EnsureVisible();

            int itemTop;
            if (!MultiSelect)
                itemTop = SelectedNodes[0].FullArea.Top;
            else
                itemTop = SelectedNodes.Last().FullArea.Top;

            var itemBottom = itemTop + ItemHeight;

            if (itemTop < Viewport.Top)
                VScrollTo(itemTop);

            if (itemBottom > Viewport.Bottom)
                VScrollTo(itemBottom - Viewport.Height);
        }

        public void Sort()
        {
            if (TreeViewNodeSorter == null)
                return;

            Nodes.Sort(TreeViewNodeSorter);

            foreach (var node in Nodes)
                SortChildNodes(node);
        }

        private void SortChildNodes(DarkTreeNode node)
        {
            node.Nodes.Sort(TreeViewNodeSorter);

            foreach (var childNode in node.Nodes)
                SortChildNodes(childNode);
        }

        public DarkTreeNode FindNode(string path)
        {
            foreach (var node in Nodes)
            {
                var compNode = FindNode(node, path);
                if (compNode != null)
                    return compNode;
            }

            return null;
        }

        private static DarkTreeNode FindNode(DarkTreeNode parentNode, string path, bool recursive = true)
        {
            if (parentNode.FullPath == path)
                return parentNode;

            foreach (var node in parentNode.Nodes)
            {
                if (node.FullPath == path)
                    return node;

                if (!recursive)
                    continue;

                var compNode = FindNode(node, path);
                if (compNode != null)
                    return compNode;
            }

            return null;
        }

        public void MoveSelectedNodeUp()
        {
            if (SelectedNodes[0].VisibleIndex == 0)
                return; // Node can't be moved higher because it's already at 0

            DarkTreeNode[] cachedNodes = Nodes.ToArray();
            Nodes.Clear();

            for (int i = 0; i < cachedNodes.Length; i++)
            {
                if (i + 1 == SelectedNodes[0].VisibleIndex)
                {
                    Nodes.Add(cachedNodes[i + 1]);
                    Nodes.Add(cachedNodes[i]);
                    i++;
                }
                else
                    Nodes.Add(cachedNodes[i]);
            }

            ScrollTo(SelectedNodes[0].FullArea.Location);
        }

        public void MoveSelectedNodeDown()
        {
            if (SelectedNodes[0].VisibleIndex == Nodes.Count - 1)
                return; // Node can't be moved lower because it's already at the bottom

            DarkTreeNode[] cachedNodes = Nodes.ToArray();
            Nodes.Clear();

            for (int i = 0; i < cachedNodes.Length; i++)
            {
                if (i == SelectedNodes[0].VisibleIndex)
                {
                    Nodes.Add(cachedNodes[i + 1]);
                    Nodes.Add(cachedNodes[i]);
                    i++;
                }
                else
                    Nodes.Add(cachedNodes[i]);
            }

            ScrollTo(SelectedNodes[0].FullArea.Location);
        }

        #endregion

        #region Drag & Drop Region

        protected override void StartDrag()
        {
            if (!AllowMoveNodes)
            {
                _provisionalDragging = false;
                return;
            }

            // Create initial list of nodes to drag
            _dragNodes = new List<DarkTreeNode>();
            foreach (var node in SelectedNodes)
                _dragNodes.Add(node);

            // Clear out any nodes with a parent that is being dragged
            foreach (var node in _dragNodes.ToList())
            {
                if (node.ParentNode == null)
                    continue;

                if (_dragNodes.Contains(node.ParentNode))
                    _dragNodes.Remove(node);
            }

            _provisionalDragging = false;

            Cursor = Cursors.SizeAll;

            base.StartDrag();
        }

        private void HandleDrag()
        {
            if (!AllowMoveNodes)
                return;

            _mouseInClientArea = ClientRectangle.Contains(PointToClient(MousePosition));

            if (!_mouseInClientArea)
            {
                if (Cursor != Cursors.No)
                    Cursor = Cursors.No;

                ClearDropTarget();
                return;
            }

            // Find the node the mouse is hovering over and compute drop position.
            DarkTreeNode hitNode = null;
            bool hitDragNode = false;
            var position = DropPosition.None;
            int indicatorY = 0;

            var allVisible = GetAllNodes();
            foreach (var node in allVisible)
            {
                var rect = GetNodeFullRowArea(node);
                if (!rect.Contains(OffsetMousePosition))
                    continue;

                if (_dragNodes.Contains(node))
                {
                    hitDragNode = true;
                    break;
                }

                bool canDropInto = CanDropIntoNode == null || CanDropIntoNode(node);
                int relativeY = OffsetMousePosition.Y - rect.Top;
                int zoneSize = rect.Height / 4;

                if (relativeY < zoneSize)
                {
                    position = DropPosition.Before;
                    indicatorY = rect.Top;
                }
                else if (relativeY > rect.Height - zoneSize)
                {
                    position = DropPosition.After;
                    indicatorY = rect.Bottom;
                }
                else if (canDropInto)
                {
                    position = DropPosition.Into;
                    indicatorY = 0;
                }
                else
                {
                    // Not a valid drop-into target, treat center as before/after
                    // based on which half the mouse sits in.
                    if (relativeY < rect.Height / 2)
                    {
                        position = DropPosition.Before;
                        indicatorY = rect.Top;
                    }
                    else
                    {
                        position = DropPosition.After;
                        indicatorY = rect.Bottom;
                    }
                }

                hitNode = node;
                break;
            }

            // No valid drop target hit — determine fallback based on mouse position.
            if (hitNode == null && !hitDragNode && _mouseInClientArea)
            {
                var firstNonDrag = allVisible.FirstOrDefault(n => !_dragNodes.Contains(n));
                if (firstNonDrag != null)
                {
                    var firstRect = GetNodeFullRowArea(firstNonDrag);
                    if (OffsetMousePosition.Y <= firstRect.Top)
                    {
                        // Mouse is above the first node — insert before it.
                        hitNode = firstNonDrag;
                        position = DropPosition.Before;
                        indicatorY = firstRect.Top;
                    }
                    else
                    {
                        // Mouse is below all nodes — append to root.
                        position = DropPosition.After;
                        indicatorY = ContentSize.Height;
                    }
                }
            }

            // Validate drop target.
            if (hitNode != null && !CanMoveNodes(_dragNodes, hitNode, position))
            {
                ClearDropTarget();
                Cursor = Cursors.No;
                return;
            }

            if (_dropNode != hitNode || _dropPosition != position || _dropIndicatorY != indicatorY)
            {
                _dropNode = hitNode;
                _dropPosition = position;
                _dropIndicatorY = indicatorY;
                Invalidate();
            }

            Cursor = Cursors.SizeAll;
        }

        private void HandleDrop()
        {
            if (!AllowMoveNodes)
                return;

            var dropNode = _dropNode;
            var position = _dropPosition;

            // Root-level drop: mouse in control area but not over any specific node.
            if (dropNode == null && _mouseInClientArea)
            {
                MoveNodesToCollection(_dragNodes, Nodes, -1);
                StopDrag();
                UpdateNodes();
                return;
            }

            if (dropNode == null || position == DropPosition.None)
            {
                StopDrag();
                return;
            }

            switch (position)
            {
                case DropPosition.Into:
                    MoveNodesToCollection(_dragNodes, dropNode.Nodes, -1);
                    dropNode.Expanded = true;
                    break;

                case DropPosition.Before:
                case DropPosition.After:
                    var targetCollection = dropNode.ParentNode != null ? dropNode.ParentNode.Nodes : Nodes;
                    int targetIndex = targetCollection.IndexOf(dropNode);

                    if (position == DropPosition.After)
                        targetIndex++;

                    MoveNodesToCollection(_dragNodes, targetCollection, targetIndex);
                    break;
            }

            StopDrag();
            UpdateNodes();
        }

        private void MoveNodesToCollection(List<DarkTreeNode> nodes, ObservableList<DarkTreeNode> target, int insertIndex)
        {
            var cachedSelectedNodes = SelectedNodes.ToList();

            foreach (var node in nodes)
            {
                var sourceCollection = node.ParentNode != null ? node.ParentNode.Nodes : Nodes;

                // Adjust insert index when removing from the same collection before the target position.
                if (sourceCollection == target && insertIndex >= 0)
                {
                    int currentIndex = sourceCollection.IndexOf(node);
                    if (currentIndex >= 0 && currentIndex < insertIndex)
                        insertIndex--;
                }

                sourceCollection.Remove(node);

                if (insertIndex >= 0 && insertIndex <= target.Count)
                    target.Insert(insertIndex, node);
                else
                    target.Add(node);

                if (insertIndex >= 0)
                    insertIndex++;
            }

            if (TreeViewNodeSorter != null)
                target.Sort(TreeViewNodeSorter);

            foreach (var node in cachedSelectedNodes)
            {
                if (!SelectedNodes.Contains(node))
                    SelectedNodes.Add(node);
            }

            NodesMoved?.Invoke(this, EventArgs.Empty);
        }

        private void ClearDropTarget()
        {
            if (_dropNode != null || _dropPosition != DropPosition.None)
            {
                _dropNode = null;
                _dropPosition = DropPosition.None;
                _dropIndicatorY = 0;
                Invalidate();
            }
        }

        protected override void StopDrag()
        {
            _dragNodes = null;
            _dropNode = null;
            _dropPosition = DropPosition.None;
            _dropIndicatorY = 0;

            Cursor = Cursors.Default;

            Invalidate();

            base.StopDrag();
        }

        private bool CanMoveNodes(IEnumerable<DarkTreeNode> dragNodes, DarkTreeNode dropNode, DropPosition position)
        {
            if (dropNode == null)
                return false;

            foreach (var node in dragNodes)
            {
                // Cannot drop node onto itself.
                if (node == dropNode)
                    return false;

                // For Before/After: check if the node is already at that exact position.
                if (position == DropPosition.Before || position == DropPosition.After)
                {
                    var targetCollection = dropNode.ParentNode != null ? dropNode.ParentNode.Nodes : Nodes;
                    var sourceCollection = node.ParentNode != null ? node.ParentNode.Nodes : Nodes;

                    if (sourceCollection == targetCollection)
                    {
                        int srcIdx = sourceCollection.IndexOf(node);
                        int dstIdx = targetCollection.IndexOf(dropNode);
                        if (position == DropPosition.Before && srcIdx == dstIdx - 1)
                            return false;
                        if (position == DropPosition.After && srcIdx == dstIdx + 1)
                            return false;
                    }
                }

                // For Into: cannot drop into current parent.
                if (position == DropPosition.Into && node.ParentNode == dropNode)
                    return false;

                // Cannot drop into a descendant of the dragged node.
                var parentNode = dropNode.ParentNode;
                while (parentNode != null)
                {
                    if (node == parentNode)
                        return false;
                    parentNode = parentNode.ParentNode;
                }
            }

            return true;
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var b = new Pen(new SolidBrush(_borderColor)))
            {
                e.Graphics.DrawRectangle(b, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        protected override void PaintContent(Graphics g)
        {
            foreach (var node in Nodes)
                DrawNode(node, g);

            // Draw drop indicator line during drag operation.
            if (IsDragging && _dropPosition != DropPosition.Into && _dropPosition != DropPosition.None)
            {
                int lineY = Math.Max(2, _dropIndicatorY);
                int width = Math.Max(ContentSize.Width, Viewport.Width);

                using (var pen = new Pen(Colors.BlueHighlight, 2.0f))
                {
                    g.DrawLine(pen, 0, lineY, width, lineY);
                }
            }
        }

        private void DrawNode(DarkTreeNode node, Graphics g)
        {
            var rect = GetNodeFullRowArea(node);

            // 1. Draw background
            var bgColor = node.BackColor;

            Color oddColor = Colors.HeaderBackground;
            Color evenColor = Colors.GreyBackground;

            if (OverrideOddColor != Color.Transparent)
                oddColor = OverrideOddColor;

            if (OverrideEvenColor != Color.Transparent)
                evenColor = OverrideEvenColor;

            if (bgColor == Color.Transparent)
                bgColor = node.Odd ? oddColor : evenColor;

            if (SelectedNodes.Count > 0 && SelectedNodes.Contains(node))
                bgColor = Focused ? Colors.BlueSelection : Colors.GreySelection;

            if (IsDragging && _dropNode == node && _dropPosition == DropPosition.Into)
                bgColor = Focused ? Colors.BlueSelection : Colors.GreySelection;

            using (var b = new SolidBrush(bgColor))
            {
                g.FillRectangle(b, rect);
            }

            // 2. Draw plus/minus icon
            if (node.Nodes.Count > 0)
            {
                var pos = new Point(node.ExpandArea.Location.X - 1, node.ExpandArea.Location.Y - 1);

                var icon = _nodeOpen;

                if (node.Expanded && !node.ExpandAreaHot)
                    icon = _nodeOpen;
                else if (node.Expanded && node.ExpandAreaHot && !SelectedNodes.Contains(node))
                    icon = _nodeOpenHover;
                else if (node.Expanded && node.ExpandAreaHot && SelectedNodes.Contains(node))
                    icon = _nodeOpenHoverSelected;
                else if (!node.Expanded && !node.ExpandAreaHot)
                    icon = _nodeClosed;
                else if (!node.Expanded && node.ExpandAreaHot && !SelectedNodes.Contains(node))
                    icon = _nodeClosedHover;
                else if (!node.Expanded && node.ExpandAreaHot && SelectedNodes.Contains(node))
                    icon = _nodeClosedHoverSelected;

                g.DrawImage(icon, pos);
            }

            // 3. Draw icon
            if (ShowIcons && node.Icon != null)
            {
                if (node.Expanded && node.ExpandedIcon != null)
                    g.DrawImage(node.ExpandedIcon, node.IconArea.Location);
                else
                    g.DrawImage(node.Icon, node.IconArea.Location);
            }

            // 4. Draw text
            using (var b = new SolidBrush(Enabled ? Colors.LightText : Colors.DisabledText))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(node.Text, Font, b, node.TextArea, stringFormat);

                if (node is DarkTreeNodeEx)
                {
                    var nodeEx = node as DarkTreeNodeEx;
                    var subTextFont = new Font(Font.FontFamily, 8.25f);

                    g.DrawString(nodeEx.SubText, subTextFont, b, nodeEx.SubTextArea, stringFormat);

                    if (ShowIcons && nodeEx.ExtraIcon != null)
                        g.DrawImage(nodeEx.ExtraIcon, nodeEx.ExtraIconArea.Location);
                }
            }

            // 5. Draw child nodes
            if (node.Expanded)
            {
                foreach (var childNode in node.Nodes)
                    DrawNode(childNode, g);
            }
        }

        #endregion
    }
}

using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Controls.VisualScripting
{
    public enum ConnectionMode
    {
        Previous,
        Next,
        Else
    }

    public partial class NodeEditor : UserControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 ViewPosition { get; set; } = new Vector2(60.0f, 60.0f);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TriggerNode> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                SelectedNodes.Clear();
                UpdateVisibleNodes(true);
            }
        }
        private List<TriggerNode> _nodes = new List<TriggerNode>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TriggerNode> SelectedNodes { get; private set; } = new List<TriggerNode>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TriggerNode SelectedNode => SelectedNodes.LastOrDefault();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TriggerNode HotNode { get; set; } = null;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionMode HotNodeMode { get; set; } = ConnectionMode.Previous;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Resizing { get; private set; } = false;

        // Loaded lists of action and condition functions.

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<NodeFunction> NodeFunctions { get; private set; } = new List<NodeFunction>();

        // Precache lists of objects to avoid polling every time user changes function
        // in a node list. By default it is set to nothing, but must be replaced externally
        // by a form/control where node editor is placed.

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedLuaFunctions { get { return _cachedLuaFunctions; } }
        private List<string> _cachedLuaFunctions = new List<string>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedVolumeEventSets { get { return _cachedVolumeEventSets; } }
        private List<string> _cachedVolumeEventSets = new List<string>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedGlobalEventSets { get { return _cachedGlobalEventSets; } }
        private List<string> _cachedGlobalEventSets = new List<string>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<MoveableInstance> CachedMoveables { get { return _cachedMoveables; } }
        private List<MoveableInstance> _cachedMoveables = new List<MoveableInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<StaticInstance> CachedStatics { get { return _cachedStatics; } }
        private List<StaticInstance> _cachedStatics = new List<StaticInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<CameraInstance> CachedCameras { get { return _cachedCameras; } }
        private List<CameraInstance> _cachedCameras = new List<CameraInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<SinkInstance> CachedSinks { get { return _cachedSinks; } }
        private List<SinkInstance> _cachedSinks = new List<SinkInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<FlybyCameraInstance> CachedFlybys { get { return _cachedFlybys; } }
        private List<FlybyCameraInstance> _cachedFlybys = new List<FlybyCameraInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<VolumeInstance> CachedVolumes { get { return _cachedVolumes; } }
        private List<VolumeInstance> _cachedVolumes = new List<VolumeInstance>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Room> CachedRooms { get { return _cachedRooms; } }
        private List<Room> _cachedRooms = new List<Room>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<WadSoundInfo> CachedSoundInfos { get { return _cachedSoundInfos; } }
        private List<WadSoundInfo> _cachedSoundInfos = new List<WadSoundInfo>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedSoundTracks { get { return _cachedSoundTracks; } }
        private List<string> _cachedSoundTracks = new List<string>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedVideos { get { return _cachedVideos; } }
        private List<string> _cachedVideos = new List<string>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedWadSlots { get { return _cachedWadSlots; } }
        private List<string> _cachedWadSlots = new List<string>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> CachedSpriteSlots { get { return _cachedSpriteSlots; } }
        private List<string> _cachedSpriteSlots = new List<string>();

        public Color SelectionColor { get; set; } = Colors.BlueSelection;
        public float GridStep { get; set; } = 8.0f;
        public int GridSize { get; set; } = 256;
        public float GridOffset => GridSize / 64.0f;
        public int DefaultNodeWidth { get; set; } = TriggerNode.DefaultSize;
        public bool LinksAsRopes { get; set; } = false;
        public bool ShowGrips { get; set; } = false;

        private const float _mouseWheelScrollFactor = 0.04f;

        private const float _hotNodeTransparency = 0.6f;
        private const float _connectedNodeTransparency = 0.8f;
        private const int _selectionThickness = 2;
        private const int _gripSize = 8;

        private Vector3 _defaultConditionNodeTint = new Vector3(0.8f, 1.0f, 0.8f);
        private Vector3 _defaultActionNodeTint = new Vector3(0.8f, 0.8f, 1.0f);

        private static readonly Pen _gridPen = new Pen((Colors.DarkBackground.ToFloat3Color() * 1.15f).ToWinFormsColor(), 1);
        private static readonly Pen _selectionPen = new Pen(Colors.BlueSelection, _selectionThickness);
        private static readonly Brush _selectionBrush = new SolidBrush(Colors.BlueSelection.ToFloat3Color().ToWinFormsColor(0.5f));

        private Rectangle2 _selectionArea;

        private bool _selectionInProgress;
        private Point _lastMousePosition;
        private Point _initialMousePosition;
        private Vector2? _viewMoveMouseWorldCoord;

        private float _animProgress = 1.0f;
        private PointF[] _animSnapCoords = null;
        private readonly Timer _updateTimer;
        private bool _queueMove = false;

        private FormFunctionList _functionList = null;

        public event EventHandler ViewPositionChanged;
        private void OnViewPositionChanged()
            => ViewPositionChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler SelectionChanged;
        private void OnSelectionChanged()
            => SelectionChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler LocatedItemFound;
        public void OnLocatedItemFound(IHasLuaName item)
            => LocatedItemFound?.Invoke(item, EventArgs.Empty);

        public event EventHandler SoundtrackPlayed;
        public void OnSoundtrackPlayed(string name)
            => SoundtrackPlayed?.Invoke(name, EventArgs.Empty);

        public event EventHandler SoundEffectPlayed;
        public void OnSoundEffectPlayed(string sound)
            => SoundEffectPlayed?.Invoke(sound, EventArgs.Empty);

        public NodeEditor()
        {
            SetStyle(ControlStyles.UserPaint | 
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.Selectable, true);

            AllowDrop = true;
            DoubleBuffered = true;

            _updateTimer = new Timer { Interval = 10 };
            _updateTimer.Tick += UpdateTimer_Tick;

            DragDrop += NodeEditor_DragDrop;
            DragEnter += NodeEditor_DragEnter;
        }

        protected override void Dispose(bool disposing)
        {
            _updateTimer.Stop();
            _updateTimer.Tick -= UpdateTimer_Tick;

            DragDrop -= NodeEditor_DragDrop;
            DragEnter -= NodeEditor_DragEnter;

            foreach (Control control in Controls)
                control.Dispose();

            Controls.Clear();

            base.Dispose(disposing);
        }

        public void Initialize(Level level, List<string> scriptFunctions = null)
        {
            ViewPosition = new Vector2(GridSize / 2.0f,
                                       GridSize / 2.0f);
            PopulateCachedNodeLists(level, scriptFunctions);
            UpdateVisibleNodes(true);
            _updateTimer.Start();
        }

        public void PopulateCachedNodeLists(Level level, List<string> scriptFunctions = null)
        {
            var allObjects = level.GetAllObjects();

            _cachedMoveables        = allObjects.OfType<MoveableInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName) && 
                                        !TrCatalog.IsMoveableAI(TRVersion.Game.TombEngine, o.WadObjectId.TypeId)).ToList();
            _cachedStatics          = allObjects.OfType<StaticInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cachedCameras          = allObjects.OfType<CameraInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cachedSinks            = allObjects.OfType<SinkInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cachedFlybys           = allObjects.OfType<FlybyCameraInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cachedVolumes          = allObjects.OfType<VolumeInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cachedWadSlots         = level.Settings.WadGetAllMoveables().Select(m => TrCatalog.GetMoveableName(level.Settings.GameVersion, m.Key.TypeId)).ToList();
            _cachedSpriteSlots      = level.Settings.WadGetAllSpriteSequences().Select(m => TrCatalog.GetSpriteSequenceName(level.Settings.GameVersion, m.Key.TypeId)).ToList();
            _cachedSoundTracks      = level.Settings.GetListOfSoundtracks();
            _cachedVideos           = level.Settings.GetListOfVideos();
            _cachedSoundInfos       = level.Settings.GlobalSoundMap;
            _cachedRooms            = level.ExistingRooms;
            _cachedVolumeEventSets  = level.Settings.VolumeEventSets.Select(s => s.Name).ToList();
            _cachedGlobalEventSets  = level.Settings.GlobalEventSets.Select(s => s.Name).ToList();

            if (scriptFunctions != null)
                _cachedLuaFunctions = scriptFunctions;
            else
                _cachedLuaFunctions = ScriptingUtils.GetAllFunctionNames(level.Settings.MakeAbsolute(level.Settings.TenLuaScriptFile));
        }

        public TriggerNode MakeNode(bool condition)
        {
            TriggerNode result;

            if (condition)
                result = new TriggerNodeCondition()
                {
                    Name = LuaSyntax.If.Capitalize() + " " + (LinearizedNodes().OfType<TriggerNodeCondition>().Count() + 1),
                    Color = Colors.GreyBackground.ToFloat3Color() * _defaultConditionNodeTint
                };
            else
                result = new TriggerNodeAction()
                {
                    Name = "Action " + (LinearizedNodes().OfType<TriggerNodeAction>().Count() + 1),
                    Color = Colors.GreyBackground.ToFloat3Color() * _defaultActionNodeTint
                };

            result.Size = DefaultNodeWidth;

            return result;
        }

        public void AddNode(bool linkToPrevious, bool linkToElse, bool condition)
        {
            var node = MakeNode(condition);

            Nodes.Add(node);
            if (linkToPrevious)
                LinkToSelectedNode(node, linkToElse);

            UpdateVisibleNodes();
            node.ScreenPosition = GetBestPosition(node);

            SelectNode(node, false, true);
            ShowNode(SelectedNode);

            LayoutVisibleNodes();
        }

        public void AddConditionNode(bool linkToPrevious, bool linkToElse)
        {
            AddNode(linkToPrevious, linkToElse, true);
        }

        public void AddActionNode(bool linkToPrevious, bool linkToElse)
        {
            AddNode(linkToPrevious, linkToElse, false);
        }

        public void ClearNodes()
        {
            Nodes.Clear();
            UpdateVisibleNodes();
            ClearSelection();
        }

        public void LinkToSelectedNode(TriggerNode node, bool elseIfPossible)
        {
            if (SelectedNode == null || node == null || node.Previous != null)
                return;

            if (SelectedNode is TriggerNodeCondition && 
                ((elseIfPossible || (SelectedNode as TriggerNodeCondition)?.Next != null) && 
                 (SelectedNode as TriggerNodeCondition)?.Else == null))
            {
                (SelectedNode as TriggerNodeCondition).Else = node;
            }
            else if (SelectedNode.Next == null)
            {
                SelectedNode.Next = node;
            }
            else
                return;

            node.Previous = SelectedNode;

            if (Nodes.Contains(node))
                Nodes.Remove(node);
        }

        public void LinkSelectedNodes()
        {
            if (SelectedNodes.Count < 2)
                return;

            var orderedNodes = SelectedNodes.OrderByDescending(n => n.ScreenPosition.Y)
                                            .ThenBy(n => n.ScreenPosition.X).ToList();

            for (int i = 0; i <= orderedNodes.Count - 2; i++)
            {
                var node = orderedNodes[i];
                var nextNode = orderedNodes[i + 1];

                if (node.Next != null || nextNode.Previous != null)
                    continue;

                node.Next = nextNode;
                nextNode.Previous = node;

                if (Nodes.Contains(nextNode))
                    Nodes.Remove(nextNode);
            }

            Invalidate();
        }

        public void MoveSelectedNodes(TriggerNode rootNode, Vector2 delta)
        {
            if (SelectedNodes.Count <= 1)
                return;

            Lock(true);
            {
                foreach (var node in SelectedNodes)
                {
                    if (node == rootNode)
                        continue;

                    node.ScreenPosition += delta;
                    var visibleNode = Controls.OfType<VisibleNodeBase>().FirstOrDefault(n => n.Node == node);

                    if (visibleNode != null)
                        visibleNode.RefreshPosition();
                }
            }
            Lock(false);
        }

        public void SelectNode(TriggerNode node, bool toggle, bool reset)
        {
            if (!LinearizedNodes().Contains(node))
                return;

            Resizing = false;

            if (reset)
                SelectedNodes.Clear();

            if (!SelectedNodes.Contains(node))
                SelectedNodes.Add(node);
            else if (toggle)
                SelectedNodes.Remove(node);

            Invalidate();
            OnSelectionChanged();
        }

        public void SelectAllNodes()
        {
            SelectedNodes.Clear();
            SelectedNodes.AddRange(LinearizedNodes());
            Invalidate();
            OnSelectionChanged();
        }

        public void SelectNodesInArea()
        {
            if (_selectionArea == Rectangle2.Zero ||
                _selectionArea.Width == 0 || _selectionArea.Height == 0)
                return;

            var selectionRect = ToVisualCoord(_selectionArea);
            bool selectionChanged = false;

            foreach (var control in Controls.OfType<VisibleNodeBase>())
            {
                if (!control.Visible)
                    continue;

                var controlRect = new RectangleF(control.Location, control.Size);
                if (selectionRect.IntersectsWith(controlRect))
                {
                    if (!SelectedNodes.Contains(control.Node))
                    {
                        selectionChanged = true;
                        SelectedNodes.Add(control.Node);
                    }
                }
                else if (SelectedNodes.Contains(control.Node))
                {
                    selectionChanged = true;
                    SelectedNodes.Remove(control.Node);
                }
            }

            if (selectionChanged)
                OnSelectionChanged();
        }

        public void ClearSelection()
        {
            SelectedNodes.Clear();
            OnSelectionChanged();
            Invalidate();
        }

        public void LockNode(TriggerNode node, bool locked)
        {
            if (node == null)
                return;

            var control = Controls.OfType<VisibleNodeBase>().FirstOrDefault(c => c.Node == node);

            if (control == null)
                return;

            node.Locked = control.Locked = locked;
        }

        public void ShowNode(TriggerNode node)
        {
            if (node == null)
                return;

            var control = Controls.OfType<VisibleNodeBase>().FirstOrDefault(c => c.Node == node);

            if (control == null)
                return;

            var pos = ToVisualCoord(control.Node.ScreenPosition);
            pos.X = pos.X + control.Width / 2;

            var rect = control.ClientRectangle;
            rect.Offset(ToVisualCoord(control.Node.ScreenPosition));

            if (ClientRectangle.Contains(rect))
                return;

            var finalCoord = FromVisualCoord(pos);
            ViewPosition = finalCoord;
            LayoutVisibleNodes();

            Invalidate();
            OnViewPositionChanged();
        }

        public void FindNodeByName(string name)
        {
            var match = LinearizedNodes().FirstOrDefault(n => n.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1);

            if (match != null)
            {
                SelectNode(match, false, true);
                ShowNode(SelectedNode);
            }
        }

        public void DeleteNodes()
        {
            if (SelectedNodes.Count == 0)
                return;

            foreach (var node in SelectedNodes)
            {
                DisconnectPreviousNode(node);
                DisconnectNextNode(node);
                DisconnectElseNode(node);
                Nodes.Remove(node);
            }

            UpdateVisibleNodes();
            ClearSelection();
        }

        public Vector2 FromVisualCoord(PointF pos)
        {
            return new Vector2((pos.X - Width * 0.5f) / GridStep + ViewPosition.X, 
                               (Height * 0.5f - pos.Y) / GridStep + ViewPosition.Y);
        }

        public Rectangle2 FromVisualCoord(RectangleF area)
        {
            var visibleAreaStart = FromVisualCoord(new PointF(area.Left, area.Top));
            var visibleAreaEnd   = FromVisualCoord(new PointF(area.Right, area.Bottom));
            return new Rectangle2(Vector2.Min(visibleAreaStart, visibleAreaEnd), Vector2.Max(visibleAreaStart, visibleAreaEnd));
        }

        public Point ToVisualCoord(Vector2 pos)
        {
            return new Point((int)Math.Floor((pos.X - ViewPosition.X) * GridStep + Width * 0.5f), 
                             (int)Math.Floor(Height * 0.5f - (pos.Y - ViewPosition.Y) * GridStep));
        }

        public RectangleF ToVisualCoord(Rectangle2 area)
        {
            var start = ToVisualCoord(area.Start);
            var end   = ToVisualCoord(area.End);
            return RectangleF.FromLTRB(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint, bool limitPosition = false)
        {
            // Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint);

            if (limitPosition)
                ViewPosition = Vector2.Clamp(ViewPosition, new Vector2(), new Vector2(GridSize));
            
            LayoutVisibleNodes();

            Invalidate();
            OnViewPositionChanged();
        }

        public void UpdateVisibleNodes(bool fullRedraw = false)
        {
            if (fullRedraw)
                Lock(true);

            var visibleNodes = Controls.OfType<VisibleNodeBase>().ToList();
            var linearizedNodes = LinearizedNodes();

            // Remove orphaned controls
            for (int i = visibleNodes.Count - 1; i >= 0; i--)
            {
                var control = visibleNodes[i];
                if (!linearizedNodes.Contains(control.Node))
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
            }

            var newControls = new List<VisibleNodeBase>();

            // Recreate new controls
            foreach (var node in Nodes)
                AddNodeControl(node, newControls);

            // Add all controls at once, to avoid flickering
            foreach (var control in newControls)
            {
                if (fullRedraw)
                    control.Location = new Point(int.MaxValue, int.MaxValue);

                Controls.Add(control);
                control.SpawnFunctionList(NodeFunctions);
            }

            if (fullRedraw)
                Lock(false);

            LayoutVisibleNodes();
            Invalidate();
        }

        public void RefreshArgumentUI()
        {
            foreach (var vnode in Controls.OfType<VisibleNodeBase>())
                vnode.SpawnUIElements();
        }

        public bool AnimateSnap(ConnectionMode mode, VisibleNodeBase node)
        {
            if (_animProgress != -1.0f)
                return false;

            _animSnapCoords = node.GetNodeScreenPosition(mode, true);
            _animProgress = 0.0f;
            return true;
        }

        public void AnimateUnsnap()
        {
            _animSnapCoords = new PointF[2] { _lastMousePosition, _lastMousePosition };
            _animProgress = -1.0f;
        }

        public void LayoutVisibleNodes()
        {
            Lock(true);
            foreach (var control in Controls.OfType<VisibleNodeBase>())
                control.RefreshPosition();
            Lock(false);
        }

        public Vector2 GetBestPosition(TriggerNode node)
        {
            var selectedVisibleNode = Controls.OfType<VisibleNodeBase>().FirstOrDefault(n => n.Node == node);
            return GetBestPosition(selectedVisibleNode);
        }

        public Vector2 GetBestPosition(VisibleNodeBase newNode)
        {
            var selectedVisibleNode = Controls.OfType<VisibleNodeBase>().FirstOrDefault(n => n.Node == SelectedNode);

            var pos = Vector2.Zero;

            if (selectedVisibleNode == null)
            {
                var x1 = ViewPosition.X;
                var x2 = FromVisualCoord(ToVisualCoord(ViewPosition) + newNode.Size).X;
                pos = ViewPosition - new Vector2((x2 - x1) / 2.0f, 0.0f);
            }
            else
            {
                var location = ToVisualCoord(selectedVisibleNode.Node.ScreenPosition);
                var x = location.X + selectedVisibleNode.Width / 2;
                x -= newNode.Node.Size / 2;
                var y = location.Y + selectedVisibleNode.Height;
                pos = new Vector2(x, y);
            }    

            bool colliding = true;

            while (colliding)
            {
                pos.Y += (int)(GridStep * 4);

                colliding = false;

                foreach (var control in Controls.OfType<VisibleNodeBase>())
                {
                    var rect = control.ClientRectangle;
                    rect.Offset(ToVisualCoord(control.Node.ScreenPosition));
                    rect.Inflate((int)GridStep / 2, (int)GridStep * 2);

                    var rect2 = newNode.ClientRectangle;
                    rect2.Offset(new Point((int)pos.X, (int)pos.Y));
                    rect2.Inflate((int)GridStep / 2, (int)GridStep * 2);

                    if (rect.IntersectsWith(rect2))
                        colliding = true;
                }
            }

            return FromVisualCoord(new PointF((int)pos.X, (int)pos.Y));
        }

        private void AddNodeControl(TriggerNode node, List<VisibleNodeBase> collectedControls)
        {
            if (!Controls.OfType<VisibleNodeBase>().Any(c => c.Node == node))
            {
                VisibleNodeBase control = null;

                if (node is TriggerNodeAction)
                    control = new VisibleNodeAction(node);
                else if (node is TriggerNodeCondition)
                    control = new VisibleNodeCondition(node);

                collectedControls.Add(control);
                control.BackColor = node.Color.ToWinFormsColor();
                control.Visible = false;
                control.SnapToBorders = false;
                control.DragAnyPoint = true;
                control.GripSize = ShowGrips ? _gripSize : 0;
                control.Size = new Size(node.Size, control.Size.Height);

                if (node.ScreenPosition.Y == float.MaxValue)
                    node.ScreenPosition = GetBestPosition(control);

                control.Location = ToVisualCoord(node.ScreenPosition);
            }

            if (node.Next != null)
                AddNodeControl(node.Next, collectedControls);

            if (node is TriggerNodeCondition)
                if ((node as TriggerNodeCondition).Else != null)
                    AddNodeControl((node as TriggerNodeCondition).Else, collectedControls);
        }

        public List<TriggerNode> LinearizedNodes()
        {
            return TriggerNode.LinearizeNodes(Nodes);
        }

        private void DisconnectPreviousNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Previous != node || n.Previous.Next != n)
                    continue;
                
                n.Previous = n.Previous.Next = null;

                if (!Nodes.Contains(n))
                    Nodes.Add(n);

                return;
            }
        }

        private void DisconnectElseNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Previous != node)
                    continue;

                if (!(n.Previous is TriggerNodeCondition))
                    continue;

                var prevNode = (n.Previous as TriggerNodeCondition);
                if (prevNode.Else != n)
                    continue;

                prevNode.Else = null;
                n.Previous = null;

                if (!Nodes.Contains(n))
                    Nodes.Add(n);

                return;
            }
        }

        private void DisconnectNextNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Next == node)
                    n.Next = n.Next.Previous = null;

                if (n is TriggerNodeCondition && (n as TriggerNodeCondition).Else == node)
                    (n as TriggerNodeCondition).Else = (n as TriggerNodeCondition).Else.Previous = null;
            }

            if (!Nodes.Contains(node))
                Nodes.Add(node);
        }

        public void ResetHotNode()
        {
            if (HotNode == null)
                return;

            if (!Nodes.Contains(HotNode) && HotNode.Previous == null)
                Nodes.Add(HotNode);
            HotNode = null;
            Invalidate();
        }

        private Rectangle GetNodeRect(TriggerNode node)
        {
            foreach (var control in Controls)
            {
                if (!(control is VisibleNodeBase))
                    continue;

                var visibleNode = control as VisibleNodeBase;

                if (!visibleNode.Visible)
                    continue;

                if (visibleNode.Node != node)
                    continue;

                var rect = visibleNode.ClientRectangle;
                rect.Offset(visibleNode.Location);
                return rect;
            }

            return new Rectangle();
        }

        public bool TestRecursivity(TriggerNode incomingNode, TriggerNode targetNode, bool backwards)
        {
            bool result = false;

            if (backwards)
            {
                if (targetNode.Previous != null)
                    result = TestRecursivity(incomingNode, targetNode.Previous, backwards);
            }
            else
            {
                if (targetNode.Next != null)
                    result = TestRecursivity(incomingNode, targetNode.Next, backwards);

                if (!result && targetNode is TriggerNodeCondition)
                {
                    var elseNode = (targetNode as TriggerNodeCondition).Else;
                    if (elseNode != null)
                        result = TestRecursivity(incomingNode, elseNode, backwards);
                }
            }

            if (!result)
                return incomingNode == targetNode;
            else
                return result;
        }

        private void OpenNodeContext(Point location)
        {
            if (_functionList != null)
                _functionList.FormClosed -= MakeNodeByContext;

            _functionList = new FormFunctionList(Cursor.Position, this, NodeFunctions);
            _functionList.FormClosed += MakeNodeByContext;
            _functionList.Show();
        }

        private void MakeNodeByContext(object sender, FormClosedEventArgs e)
        {
            var func = (sender as FormFunctionList).SelectedFunction;
            if (func == null)
                return;

            var node = MakeNode(func.Conditional);
            node.Function = func.Signature;
            node.ScreenPosition = FromVisualCoord(PointToClient((sender as Form).Location));

            Nodes.Add(node);

            LinkToSelectedNode(node, Control.ModifierKeys == Keys.Alt);
            UpdateVisibleNodes();
            SelectNode(node, false, true);
        }

        private void DrawShadow(PaintEventArgs e, VisibleNodeBase node)
        {
            if (!node.Visible)
                return;

            var rect = node.ClientRectangle;
            rect.Offset(node.Location);
            rect.Inflate(16, 16);
            e.Graphics.DrawImage(Properties.Resources.misc_Shadow, rect);
        }

        private void DrawHeader(PaintEventArgs e, VisibleNodeBase node)
        {
            if (!node.Visible)
                return;

            var size = TextRenderer.MeasureText(node.Node.Name, Font);

            var rect = node.ClientRectangle;
            rect.Height = size.Height;
            rect.Offset(node.Location);
            rect.Offset(0, -(int)(size.Height * 1.2f));

            using (var b = new SolidBrush(Colors.LightText.ToFloat3Color().ToWinFormsColor(0.5f)))
                e.Graphics.DrawString(node.Node.Name, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            var condNode = node as VisibleNodeCondition;
            if (condNode == null)
                return;

            using (var b = new SolidBrush(Colors.LightText.ToFloat3Color().ToWinFormsColor(0.3f)))
            {
                size = TextRenderer.MeasureText(LuaSyntax.Then, Font);
                var nextPoint = condNode.GetNodeScreenPosition(ConnectionMode.Next);
                rect = new Rectangle((int)nextPoint[0].X, condNode.Location.Y + condNode.Height + (int)(size.Height * 0.4f),
                                     (int)(nextPoint[1].X - nextPoint[0].X), size.Height);

                e.Graphics.DrawImage(Properties.Resources.misc_Shadow, 
                    new Rectangle((int)((nextPoint[0].X + nextPoint[1].X) / 2) - size.Width / 2, rect.Y, size.Width, size.Height));

                e.Graphics.DrawString(LuaSyntax.Then, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                size = TextRenderer.MeasureText(LuaSyntax.Else, Font);
                nextPoint = condNode.GetNodeScreenPosition(ConnectionMode.Else);
                rect = new Rectangle((int)nextPoint[0].X, condNode.Location.Y + condNode.Height + (int)(size.Height * 0.4f),
                                     (int)(nextPoint[1].X - nextPoint[0].X), size.Height);

                e.Graphics.DrawImage(Properties.Resources.misc_Shadow,
                    new Rectangle((int)((nextPoint[0].X + nextPoint[1].X) / 2) - size.Width / 2, rect.Y, size.Width, size.Height));

                e.Graphics.DrawString(LuaSyntax.Else, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void DrawVisibleNodeLink(PaintEventArgs e, List<VisibleNodeBase> nodes, VisibleNodeBase node)
        {
            for (int i = 0; i < 2; i++)
            {
                TriggerNode second;

                if (i == 0)
                    second = node.Node.Next;
                else if (i == 1 && node.Node is TriggerNodeCondition && (node.Node as TriggerNodeCondition).Else != null)
                    second = (node.Node as TriggerNodeCondition).Else;
                else
                    continue;

                if (second == null)
                    continue;

                var nextMode = i == 0 ? ConnectionMode.Next : ConnectionMode.Else;
                var nextVisibleNode = nodes.FirstOrDefault(n => n.Node == second);

                if (nextVisibleNode == null)
                    continue;

                // Reverse R/G color bias for else node
                var color = (nextMode == ConnectionMode.Else) ? FlipColorBias(node.Node.Color) : node.Node.Color;

                var p1 = node.GetNodeScreenPosition(nextMode);
                var p2 = nextVisibleNode.GetNodeScreenPosition(ConnectionMode.Previous);
                DrawLink(e, color, _connectedNodeTransparency, p1, p2);
            }
        }

        private void DrawLink(PaintEventArgs e, Vector3 color, float alpha, PointF[] p1, PointF[] p2)
        {
            if (LinksAsRopes)
            {
                var width = (float)MathC.Clamp((p2[1].X - p2[0].X), 1, 2);
                var start = new Point((int)(p1[0].X + (p1[1].X - p1[0].X) / 2.0f), (int)p1[0].Y);
                var end   = new Point((int)(p2[0].X + (p2[1].X - p2[0].X) / 2.0f), (int)p2[0].Y);
                var midY  = (p1[0].Y + p2[0].Y) / 2.0f;

                var normColor = color == Vector3.Zero ? color : Vector3.Normalize(color);
                var luma = color.GetLuma();
                if (luma < 0.33f)
                    normColor = MathC.SmoothStep(color, normColor, luma * 3.0f);

                using (var p = new Pen(normColor.ToWinFormsColor(), width))
                    e.Graphics.DrawBezier(p, start, new PointF(start.X, midY), new PointF(end.X, midY), end);
            }
            else
            {
                using (var b = new SolidBrush(color.ToWinFormsColor(alpha)))
                    e.Graphics.FillPolygon(b, new PointF[4] { p1[0], p1[1], p2[1], p2[0] });
            }
        }

        private void DrawHotNode(PaintEventArgs e, List<VisibleNodeBase> nodeList)
        {
            if (HotNode == null)
                return;

            var visibleNode = nodeList.FirstOrDefault(n => n.Node == HotNode);
            if (visibleNode == null)
                return;

            var start = visibleNode.GetNodeScreenPosition(HotNodeMode, true);

            var p = new PointF[2];

            if (_animProgress >= 0.0f)
            {
                p[0].X = (float)MathC.SmoothStep(_lastMousePosition.X, _animSnapCoords[0].X, _animProgress);
                p[0].Y = (float)MathC.SmoothStep(_lastMousePosition.Y, _animSnapCoords[0].Y, _animProgress);
                p[1].X = (float)MathC.SmoothStep(_lastMousePosition.X, _animSnapCoords[1].X, _animProgress);
                p[1].Y = (float)MathC.SmoothStep(_lastMousePosition.Y, _animSnapCoords[1].Y, _animProgress);
            }
            else
                p = new PointF[2] { _lastMousePosition, _lastMousePosition };

            var alpha = (float)MathC.Lerp(_hotNodeTransparency, _connectedNodeTransparency, _animProgress >= 0.0f ? _animProgress : 0.0f);

            // Reverse R/G bias for else node
            var color = (HotNodeMode == ConnectionMode.Else) ? FlipColorBias(HotNode.Color) : HotNode.Color;

            DrawLink(e, color, alpha, start, p);
        }

        private Vector3 FlipColorBias(Vector3 source)
        {
            var hue = source.ToWinFormsColor().GetHue();
            var flipYX = new Vector3(source.Y, source.X, source.Z);
            var flipZX = new Vector3(source.X, source.Z, source.Y);

            if (Math.Abs(flipYX.ToWinFormsColor().GetHue() - hue) > 90.0f)
                return flipYX;
            else
                return flipZX;
        }

        private void DrawSelection(PaintEventArgs e)
        {
            if (SelectedNodes.Count == 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.None;

            using (var b = new SolidBrush(SelectionColor))
            {
                foreach (var node in SelectedNodes)
                {
                    var rect = GetNodeRect(node);

                    if (rect.Width == 0 && rect.Height == 0)
                        continue;

                    rect.Inflate(_selectionThickness, _selectionThickness);
                    e.Graphics.FillRectangle(b, rect);
                }
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            _lastMousePosition = PointToClient(Cursor.Position);

            if (HotNode != null)
            {
                if (Control.MouseButtons == MouseButtons.None)
                    if (_lastMousePosition.X < 0 || _lastMousePosition.X > Width ||
                        _lastMousePosition.Y < 0 || _lastMousePosition.Y > Height)
                        ResetHotNode();

                Invalidate();
            }

            if (_animProgress >= 0.0f && _animProgress < 1.0f)
            {
                _animProgress += 0.1f;
                if (_animProgress > 1.0f)
                    _animProgress = 1.0f;
            }

            if (_queueMove)
            {
                MoveToFixedPoint(_lastMousePosition, _viewMoveMouseWorldCoord.Value, true);
                Invalidate();
                _queueMove = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw background
            using (var b = new SolidBrush(Colors.DarkBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                // Draw grid lines
                var GridLines0 = FromVisualCoord(new PointF());
                var GridLines1 = FromVisualCoord(new PointF() + Size);
                var GridLinesStart = Vector2.Min(GridLines0, GridLines1);
                var GridLinesEnd = Vector2.Max(GridLines0, GridLines1);
                GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(GridSize));
                GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(GridSize));
                var GridLinesStartInt = new Point((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
                var GridLinesEndInt = new Point((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

                for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                    e.Graphics.DrawLine(_gridPen,
                        ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, GridSize)));

                for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                    e.Graphics.DrawLine(_gridPen,
                        ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(GridSize, y)));

                var nodeList = Controls.OfType<VisibleNodeBase>().ToList();

                // Update colors
                foreach (var n in nodeList)
                    if (n.BackColor != n.Node.Color.ToWinFormsColor())
                        n.BackColor = n.Node.Color.ToWinFormsColor();

                // Draw node links antialiased
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw connected nodes
                foreach (var n in nodeList)
                    DrawVisibleNodeLink(e, nodeList, n);

                // Draw hot node
                DrawHotNode(e, nodeList);

                // Switch back to non-antialiased state
                e.Graphics.SmoothingMode = SmoothingMode.Default;

                // Draw selection area
                if (_selectionArea != Rectangle2.Zero)
                {
                    e.Graphics.FillRectangle(_selectionBrush, ToVisualCoord(_selectionArea));
                    e.Graphics.DrawRectangle(_selectionPen, ToVisualCoord(_selectionArea));
                }

                // Draw shadows
                foreach (var n in nodeList)
                    DrawShadow(e, n);

                // Draw selected node highlights
                DrawSelection(e);

                // Draw labels (after everything else)
                foreach (var n in nodeList)
                    DrawHeader(e, n);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Absorb event
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutVisibleNodes();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(typeof(DarkFloatingToolboxContainer)))
                e.Effect = DragDropEffects.Move;
            else
            {
                var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;
                if (obj == null)
                    return;

                e.Effect = DragDropEffects.Copy;
                AnimateUnsnap();

                switch (HotNodeMode)
                {
                    case ConnectionMode.Previous:
                        DisconnectNextNode(obj);
                        break;

                    case ConnectionMode.Next:
                        DisconnectPreviousNode(obj);
                        break;

                    case ConnectionMode.Else:
                        DisconnectElseNode(obj);
                        break;

                    default:
                        break;
                }

                HotNode = obj;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;
            if (obj == null || HotNode != obj)
                return;

            ResetHotNode();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _animSnapCoords = new PointF[2] { _lastMousePosition, _lastMousePosition };

            if (e.Button == MouseButtons.Right && _viewMoveMouseWorldCoord != null)
            {
                _queueMove = true;
                Resizing = true;
                return;
            }
            else if (e.Button == MouseButtons.Left && _selectionInProgress)
            {
                _selectionArea.End = FromVisualCoord(e.Location);
                SelectNodesInArea();
                Invalidate();
            }

            // HACK: Totally ugly hack, but without it, there is no guarantee that any nested
            // control won't be active and keyboard events won't fire.
            if (FindForm() == Form.ActiveForm)
                FindForm().ActiveControl = null;

            // Make sure resizing attrib is unset at all times when not explicitly set.
            Resizing = false;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _selectionArea = Rectangle2.Zero;
            _selectionInProgress = false;
            Invalidate();

            if (e.Button != MouseButtons.Right)
                return;

            var delta = Vector2.Distance(new VectorInt2(_lastMousePosition.X, _lastMousePosition.Y), 
                                         new VectorInt2(_initialMousePosition.X, _initialMousePosition.Y));
            if (delta < 4.0f)
                OpenNodeContext(e.Location);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Control.MouseButtons == MouseButtons.None)
            {
                ResetHotNode();
                _selectionArea = Rectangle2.Zero;
                _selectionInProgress = false;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                _lastMousePosition = _initialMousePosition = e.Location;
                Capture = true; // Capture mouse for zoom and panning
                _viewMoveMouseWorldCoord = FromVisualCoord(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                var clickPos = FromVisualCoord(e.Location);
                _selectionArea = new Rectangle2(clickPos, clickPos);
                _selectionInProgress = true;
                ClearSelection();
            }
            else
                _selectionInProgress = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Controls.OfType<VisibleNodeBase>().Any(c =>
            {
                var b = c.ClientRectangle;
                b.Offset(c.Location);
                return b.Contains(e.Location);
            }))
                return;

            Capture = true; // Capture mouse for zoom and panning

            var delta = e.Delta * _mouseWheelScrollFactor;

            if (Control.ModifierKeys == Keys.Shift)
                ViewPosition += new Vector2(delta, 0.0f);
            else
                ViewPosition += new Vector2(0.0f, delta);

            ViewPosition = Vector2.Clamp(ViewPosition, new Vector2(), new Vector2(GridSize));
            LayoutVisibleNodes();

            Invalidate();
            OnViewPositionChanged();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
                OpenNodeContext(e.Location);
        }

        private void NodeEditor_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as IHasLuaName) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void NodeEditor_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as IHasLuaName) == null)
                return;

            var item = e.Data.GetData(e.Data.GetFormats()[0]) as PositionAndScriptBasedObjectInstance;

            if (string.IsNullOrEmpty(item.LuaName))
                return;

            var neededArgument = ArgumentType.Boolean; // Just use boolean because nothing will return it as enum

            if (item is MoveableInstance)
                neededArgument = ArgumentType.Moveables;
            else if (item is StaticInstance)
                neededArgument = ArgumentType.Statics;
            else if (item is CameraInstance)
                neededArgument = ArgumentType.Cameras;
            else if (item is FlybyCameraInstance)
                neededArgument = ArgumentType.FlybyCameras;
            else if (item is SinkInstance)
                neededArgument = ArgumentType.Sinks;
            else if (item is VolumeInstance)
                neededArgument = ArgumentType.Volumes;

            if (neededArgument == ArgumentType.Boolean)
                return;

            if (!NodeFunctions.Any(f => f.Arguments.Any(a => a.Type == neededArgument)))
                return;

            Lock(true);
            {
                var node = MakeNode(false);
                node.ScreenPosition = FromVisualCoord(PointToClient(new Point(e.X, e.Y)));

                Nodes.Add(node);
                UpdateVisibleNodes();
                SelectNode(node, false, true);

                var newVisibleNode = Controls.OfType<VisibleNodeBase>().First(c => c.Node == node);
                newVisibleNode.SelectFirstFunction(neededArgument, item.LuaName);
                newVisibleNode.BringToFront();
            }
            Lock(false);
            Invalidate();
        }

        private void Lock(bool locked)
        {
            Resizing = locked;

            if (locked)
                SuspendLayout();
            else
                ResumeLayout();
        }
    }
}
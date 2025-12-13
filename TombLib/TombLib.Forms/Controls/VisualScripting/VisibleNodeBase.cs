using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Extensions;
using DarkUI.Icons;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public partial class VisibleNodeBase : DarkFloatingToolbox
    {
        protected const int _gripWidth = 200;
        protected const int _gripHeight = 6;
        protected const int _elementSpacing = 8;
        protected const int _elementHeight = 24;

        protected const int _visibilityMargin = 32;

        protected List<Rectangle> _grips = new List<Rectangle>();
        protected int _currentGrip = -1;
        protected int _lastSnappedGrip = -1;

        private bool _mouseDown = false;
        private int _lastSelectedIndex = -1;

        private List<ArgumentEditor> _argControls = new List<ArgumentEditor>();

        public NodeEditor Editor => Parent as NodeEditor;
        public TriggerNode Node { get; protected set; }

        public bool Locked
        {
            get { return Node.Locked; }

            set
            {
                Node.Locked = value;
                RefreshLock();
            }
        }

        // This constructor overload is needed so that VS designer don't get crazy.
        public VisibleNodeBase() : this(new TriggerNodeAction()) { }

        public VisibleNodeBase(TriggerNode node)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponent();
            SnapToBorders = false;
            Node = node;
            Size = new Size(Editor?.DefaultNodeWidth ?? TriggerNode.DefaultSize, Size.Height);
            GripSize = 0;

            cbFunction.Control.DisableMouseScrolling = true;

            SpawnGrips();
        }

        protected override void Dispose(bool disposing)
        {
            DisposeUI();

            if (disposing)
            {
                MouseDown -= Ctrl_RightClick;
                cbFunction.MouseDown -= Ctrl_RightClick;
            }

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        protected virtual void SpawnGrips()
        {
            _grips.Clear();
            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, 0, _gripWidth, _gripHeight));
            Invalidate();
        }

        public void DisposeUI()
        {
            toolTip.RemoveAll();

            foreach (var sub in WinFormsUtils.AllSubControls(this))
                sub.MouseDown -= Ctrl_RightClick;

            // Remove old controls in reverse order
            for (int i = _argControls.Count - 1; i >= 0; i--)
            {
                var control = _argControls[i];

                control.ValueChanged -= Ctrl_ValueChanged;
                control.LocatedItemFound -= Ctrl_LocatedItemFound;
                control.SoundEffectPlayed -= Ctrl_SoundEffectPlayed;
                control.SoundtrackPlayed -= Ctrl_SoundtrackPlayed;

                if (Controls.Contains(control))
                    Controls.Remove(control);

                _argControls.RemoveAt(i);
                control.Dispose();
            }
        }

        public void ResetArguments()
        {
            Node.Arguments.Clear();

            var funcSetup = cbFunction.SelectedItem as NodeFunction;
            for (int i = 0; i < funcSetup.Arguments.Count; i++)
                Node.Arguments.Add(new TriggerNodeArgument() { Name = funcSetup.Arguments[i].Name, Value = funcSetup.Arguments[i].DefaultValue });
        }

        public void SpawnFunctionList(List<NodeFunction> functions)
        {
            if (functions == null || functions.Count == 0)
            {
                cbFunction.Visible = false;
                return;
            }

            cbFunction.Visible = true;
            cbFunction.Items.Clear();
            foreach (var f in functions)
                if (f.Conditional == (Node is TriggerNodeCondition))
                    cbFunction.Items.Add(f);

            if (cbFunction.Items.Count > 0)
            {
                int existingIndex = cbFunction.Items.OfType<NodeFunction>().IndexOf(f => f.Signature == Node.Function);

                if (existingIndex != -1)
                    cbFunction.SelectedIndex = existingIndex;
                else
                    cbFunction.SelectedIndex = 0;
            }
        }

        public void SelectFirstFunction(ArgumentType type, string luaName)
        {
            var index = cbFunction.Items.OfType<NodeFunction>().IndexOf(f => f.Arguments.Any(t => t.Type == type));
            if (index == -1)
                return;

            cbFunction.SelectedIndex = index;

            var control = _argControls.FirstOrDefault(a => a.ArgumentType == type);
            if (control != null)
                control.Text = TextExtensions.Quote(luaName);
        }

        public void SpawnUIElements()
        {
            var func = cbFunction.SelectedItem as NodeFunction;

            if (func == null)
                return;

            Visible = false;
            SuspendLayout();
            DisposeUI();

            var scale = 1.0f;
            using (var gfx = FindForm().CreateGraphics())
                scale = (float)gfx.DpiX / 96.0f;

            Size = new Size((int)(Node.Size * scale), Size.Height);

            int scaledSpacing = (int)(_elementSpacing * scale);
            int scaledHeight  = (int)(_elementHeight * scale);

            int refWidth = (Width - scaledSpacing * 2) - GripSize; 
            int newY = scaledSpacing;
            int newX = scaledSpacing + GripSize;

            cbFunction.Size = new Size(refWidth, scaledHeight);
            cbFunction.Location = new Point(newX, newY);

            var elementsOnLines = new List<int>();
            int elements = 1;
            for (int i = 0; i <= func.Arguments.Count; i++)
            {
                elements++;
                bool lastEntry = i == func.Arguments.Count;

                if (lastEntry || func.Arguments[i].NewLine)
                {
                    elementsOnLines.Add(elements - 1);
                    elements = 1;
                }
            }

            int line = 0;
            for (int i = 0; i < func.Arguments.Count; i++)
            {
                var ctrl = new ArgumentEditor()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Name = "argEditor" + i.ToString(),
                };

                ctrl.SuspendLayout();

                _argControls.Add(ctrl);
                Controls.Add(ctrl);

                if (func.Arguments[i].NewLine)
                    line++;

                float normScale = func.Arguments[i].Width / 100.0f;
                float workLineWidth = refWidth - (elementsOnLines[line] - 1) * scaledSpacing;

                ctrl.SetToolTip(toolTip, TextExtensions.SingleLineToMultiLine(func.Arguments[i].Description));
                ctrl.SetArgumentType(func.Arguments[i], Editor);

                if (func.Arguments[i].NewLine)
                {
                    newX  = scaledSpacing + GripSize;
                    newY += scaledHeight + scaledSpacing;
                }
                else
                {
                    if (_argControls.Count == 1)
                        newX = cbFunction.Left + cbFunction.Width + scaledSpacing;
                    else
                        newX = _argControls[i - 1].Left + _argControls[i - 1].Width + scaledSpacing;
                }

                var controlSize = new Size((int)Math.Round(workLineWidth * normScale), cbFunction.Height);

                // HACK: Trim second-line controls which may stick of bounds due to accumulated rounding error
                if (line > 0 && controlSize.Width + newX > Size.Width - scaledSpacing)
                    controlSize.Width = controlSize.Width - ((controlSize.Width + newX) - (Size.Width - scaledSpacing));

                ctrl.Size = controlSize;
                ctrl.Location = new Point(newX, newY);
                ctrl.ValueChanged += Ctrl_ValueChanged;
                ctrl.LocatedItemFound += Ctrl_LocatedItemFound;
                ctrl.SoundEffectPlayed += Ctrl_SoundEffectPlayed;
                ctrl.SoundtrackPlayed += Ctrl_SoundtrackPlayed;

                ctrl.ResumeLayout();
            }

            var newHeight = scaledSpacing +
                             elementsOnLines.Count * (scaledHeight + scaledSpacing);

            // HACK: Fix up first line position
            var firstLineControls = _argControls.Where(c => c.Location.Y == scaledSpacing).ToList();
            if (firstLineControls.Count > 0)
            {
                int delta = (firstLineControls.Last().Location.X + firstLineControls.Last().Width) -
                             firstLineControls.First().Location.X + scaledSpacing;

                cbFunction.Width -= delta;
                firstLineControls.ForEach(c => c.Location = new Point(c.Location.X - delta, c.Location.Y));
            }

            Size = new Size(Size.Width, newHeight);
            cbFunction.Visible = true;

            for (int i = 0; i < Node.Arguments.Count; i++)
                RefreshArgument(i);

            foreach (var sub in WinFormsUtils.AllSubControls(this))
                sub.MouseDown += Ctrl_RightClick;

            RefreshLock();
            ResumeLayout();
            Visible = true;
            Invalidate();
            Editor?.Invalidate();
        }

        private void Ctrl_LocatedItemFound(object sender, EventArgs e)
        {
            if (sender is IHasLuaName)
                Editor.OnLocatedItemFound(sender as IHasLuaName);
        }

        private void Ctrl_SoundtrackPlayed(object sender, EventArgs e)
        {
            if (sender is string)
                Editor.OnSoundtrackPlayed(sender as string);
        }

        private void Ctrl_SoundEffectPlayed(object sender, EventArgs e)
        {
            if (sender is string)
                Editor.OnSoundEffectPlayed(sender as string);
        }

        private void Ctrl_ValueChanged(object sender, EventArgs e)
        {
            var ctrl = sender as ArgumentEditor;
            int index = _argControls.IndexOf(ctrl);

            if (index != -1 && Node.Arguments.Count > index)
                Node.Arguments[index] = new TriggerNodeArgument() { Name = Node.Arguments[index].Name, Value = ctrl.Text };
        }

        private void Ctrl_RightClick(object sender, MouseEventArgs e)
        {
            if (!cbFunction.Visible)
                return;

            if (e.Button != MouseButtons.Right)
                return;

            var funcList = new FormFunctionList(Cursor.Position, cbFunction, cbFunction.Items.OfType<NodeFunction>().ToList());
            funcList.Show(this.FindForm());
        }

        public void RefreshArgument(int index)
        {
            if (Node.Arguments.Count > index && _argControls.Count > index)
                _argControls[index].Text = Node.Arguments[index].Value;
        }

        public void RefreshPosition()
        {
            if (IsInView())
            {
                if (!Visible) Visible = true;

                var newX = MathC.Clamp(Node.ScreenPosition.X, 0, Editor.GridSize);
                var newY = MathC.Clamp(Node.ScreenPosition.Y, 0, Editor.GridSize);
                if (newX != Node.ScreenPosition.X || newY != Node.ScreenPosition.Y)
                    Node.ScreenPosition = new Vector2(newX, newY);

                Location = Editor.ToVisualCoord(new Vector2(newX, newY));
            }
            else
                Visible = false;
        }

        public void StorePosition()
        {
            Node.ScreenPosition = Editor.FromVisualCoord(Location);
        }

        public PointF[] GetNodeScreenPosition(ConnectionMode mode, bool force = false)
        {
            var result = new PointF[] { new PointF(-1, -1), new PointF(-1, -1) };

            if (Node == null)
                return result;

            if (_grips.Count < (int)mode + 1)
                return result;

            var grip = _grips[(int)mode];

            var location = Editor.ToVisualCoord(Node.ScreenPosition);
            var x = location.X + grip.Left;
            var y = location.Y + grip.Top + grip.Height / 2;

            return new PointF[]
            {
                new PointF(x, y),
                new PointF(x + grip.Width, y)
            };
        }

        private bool ValidConnection(ConnectionMode mode, TriggerNode node)
        {
            if (node == null || node == Node)
                return false;

            if (mode == Editor.HotNodeMode)
                return false;

            // HACK: Later, maybe implement connection mode class (next/previous)
            if ((mode == ConnectionMode.Else && Editor.HotNodeMode == ConnectionMode.Next) ||
                (Editor.HotNodeMode == ConnectionMode.Else && mode == ConnectionMode.Next))
                return false;

            if (Editor.TestRecursivity(node, Node, mode == ConnectionMode.Next))
                return false;

            switch (mode)
            {
                case ConnectionMode.Next:
                    if (Node.Next != null)
                        return false;
                    break;

                case ConnectionMode.Previous:
                    if (Node.Previous != null)
                        return false;
                    break;

                case ConnectionMode.Else:
                    if ((Node as TriggerNodeCondition)?.Else != null)
                        return false;
                    break;
            }

            return true;
        }

        private void RefreshLock()
        {
            foreach (Control control in Controls)
                control.Enabled = !Node.Locked;

            Invalidate();
        }
        
        private int GetGrip(Point location)
        {
            int result = -1;

            foreach (var grip in _grips)
            {
                var inflatedGrip = grip;
                inflatedGrip.Inflate(4, 4); // Forgive users for near-misses
                if (inflatedGrip.Contains(location))
                    result = _grips.IndexOf(grip);
            }

            return result;
        }

        public bool IsInView()
        {
            if (Editor == null)
                return true;

            var newLocation = Editor.ToVisualCoord(Node.ScreenPosition);
            var newRect = ClientRectangle;
            newRect.Offset(newLocation);
            newRect.Inflate(_visibilityMargin, _visibilityMargin);

            return newRect.IntersectsWith(Editor.ClientRectangle);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (Node.Locked)
                return;

            base.OnDragOver(e);

            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;

            if (obj == Node)
                return;

            if (obj != null)
                e.Effect = DragDropEffects.Copy;

            var grip = GetGrip(PointToClient(new Point(e.X, e.Y)));
            if (grip == -1)
                return;

            var mode = (ConnectionMode)grip;

            if (ValidConnection(mode, obj) && Editor.AnimateSnap(mode, this))
                _lastSnappedGrip = grip;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            if (Node.Locked)
                return;

            base.OnDragDrop(e);

            if (_lastSnappedGrip == -1)
            {
                Editor.ResetHotNode();
                return;
            }

            var mode = (ConnectionMode)_lastSnappedGrip;
            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;

            if (!ValidConnection(mode, obj))
            {
                Editor.ResetHotNode();
                return;
            }

            switch (mode)
            {
                case ConnectionMode.Previous:

                    if (obj is TriggerNodeCondition && Editor.HotNodeMode == ConnectionMode.Else)
                        (obj as TriggerNodeCondition).Else = Node;
                    else
                        obj.Next = Node;

                    Node.Previous = obj;

                    if (Editor.Nodes.Contains(Node))
                        Editor.Nodes.Remove(Node);

                    break;

                case ConnectionMode.Next:

                    obj.Previous = Node;
                    Node.Next = obj;

                    if (Editor.Nodes.Contains(obj))
                        Editor.Nodes.Remove(obj);

                    break;

                case ConnectionMode.Else:

                    if (this is VisibleNodeCondition)
                    {
                        var condNode = Node as TriggerNodeCondition;

                        obj.Previous = Node;
                        condNode.Else = obj;

                        if (Editor.Nodes.Contains(obj))
                            Editor.Nodes.Remove(obj);
                    }
                    break;
            }

            Editor.HotNode = null;
            Editor.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (Node.Locked)
                return;

            base.OnMouseLeave(e);

            _currentGrip = _lastSnappedGrip = -1;
            _mouseDown = false;

            Invalidate();

            if (Editor == null)
                return;

            // HACK: If mouse has gone outside of node bounds, reset active control
            // and focus on node editor itself. It is needed because of winforms bug
            // which prevents parent control with deeply nested child controls to
            // accept keyboard commands.

            if (RectangleToScreen(ClientRectangle).Contains(Control.MousePosition))
                return;

            if (Form.ActiveForm == Editor.FindForm())
            {
                Editor.FindForm().ActiveControl = null;
                Editor.Focus();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Node.Locked)
                return;

            base.OnMouseMove(e);

            _currentGrip = GetGrip(e.Location);
            Invalidate();

            if (_mouseDown || e.Button != MouseButtons.Left)
                return;

            _mouseDown = true;

            Capture = true;
            BringToFront();

            var grip = GetGrip(e.Location);
            if (grip != -1)
            {
                Editor.HotNodeMode = (ConnectionMode)grip;
                Editor.FindForm().ActiveControl = null;
                DoDragDrop(Node, DragDropEffects.Copy);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Node.Locked)
                return;

            if (e.Button == MouseButtons.Left)
            {
                bool ctrlPressed  = Control.ModifierKeys.HasFlag(Keys.Control);
                bool shiftPressed = Control.ModifierKeys.HasFlag(Keys.Shift);

                Editor.SelectNode(Node, !shiftPressed, !ctrlPressed && !shiftPressed);
                BringToFront();
            }

            if (GetGrip(e.Location) == -1)
                base.OnMouseDown(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            // HACK: bypassing base OnLocationChanged dramatically increases winforms redraw times.
            // It doesn't provoke any rendering artifacts, so use it whenever parent is resizing.

            if (Editor?.Resizing ?? true)
                return;

            var oldPosition = Node.ScreenPosition;
            StorePosition();
            Editor.MoveSelectedNodes(Node, Node.ScreenPosition - oldPosition);

            Editor.Refresh(); // Use instead of Invalidate() to avoid WinXP-like trails
            base.OnLocationChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SpawnGrips();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!IsInView())
                return;
            
            base.OnPaint(e);

            if (Locked)
            {
                using (var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Colors.GreyHighlight.MultiplyAlpha(0.3f), BackColor))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            foreach (var grip in _grips)
            {
                if (_grips.IndexOf(grip) == _currentGrip)
                    using (var brush = new SolidBrush((BackColor.ToFloat3Color() + new Vector3(0.2f, 0.2f, 0.2f)).ToWinFormsColor()))
                        e.Graphics.FillRectangle(brush, grip);

                using (var brush = new TextureBrush(MenuIcons.grip_fill, WrapMode.Tile))
                    e.Graphics.FillRectangle(brush, grip);
            }

            if (!cbFunction.Visible)
                using (var b = new SolidBrush(Colors.LightText.ToFloat3Color().ToWinFormsColor(0.7f)))
                    e.Graphics.DrawString("No node functions. Update node script file.", Font, b, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Prevent controls flickering
                return cp;
            }
        }

        private void cbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent multiple firings (thanks Winforms)
            if (_lastSelectedIndex == cbFunction.SelectedIndex)
                return;

            var funcSetup = cbFunction.SelectedItem as NodeFunction;
            Node.Function = funcSetup.Signature;

            if ((_lastSelectedIndex != -1 && cbFunction.SelectedIndex != -1) || (funcSetup.Arguments.Count != Node.Arguments.Count))
                ResetArguments();

            SpawnUIElements();

            toolTip.SetToolTip(sender as Control, TextExtensions.SingleLineToMultiLine((cbFunction.SelectedItem as NodeFunction)?.Description ?? string.Empty));

            _lastSelectedIndex = cbFunction.SelectedIndex;
        }

        private void cbFunction_MouseDown(object sender, MouseEventArgs e)
        {
            Ctrl_RightClick(sender, e);
        }
    }
}
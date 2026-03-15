using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using TombLib.LevelData;
using TombLib.Wad;
using System.Drawing;
using DarkUI.Config;
using TombLib.Forms;
using System.Globalization;

namespace TombLib.Controls
{
    public class WadTreeView : UserControl
    {
        private Wad2 _wad;
        private DarkTreeView tree;
        private DarkComboBox suggestedGameVersionComboBox;
        private DarkLabel darkLabel1;
        private DarkButton butSearch;
        private DarkTextBox tbDate;
        private DarkLabel darkLabel3;
        private DarkTextBox tbNotes;
        private DarkLabel darkLabel2;
        private DarkPanel panelMetadata;
        private DarkPanel panelTree;
        private DarkPanel panelVersion;

        public bool ItemSelected => tree.SelectedNodes.Count > 0 && tree.SelectedNodes[0].Nodes.Count == 0;

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool ReadOnly { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Wad2 Wad
        {
            get { return _wad; }
            set { _wad = value; UpdateContent(); }
        }

        private bool _changing;

        public WadTreeView()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            tree.SelectedNodesChanged += (s, e) => { if (!_changing) SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty); };
            tbNotes.TextChanged +=(s, e) => { MetadataChanged?.Invoke(this, EventArgs.Empty); };

            // Populate game version
            foreach (var gameVersion in TRVersion.NativeVersions)
                suggestedGameVersionComboBox.Items.Add(gameVersion);
        }

        public IEnumerable<IWadObjectId> SelectedWadObjectIds => tree.SelectedNodes.Select(node => node.Tag).OfType<IWadObjectId>();

        public event EventHandler ClickOnEmpty;
        public event EventHandler SelectedWadObjectIdsChanged;
        public event EventHandler MetadataChanged;

        public void UpdateContent()
        {
            bool wadLoaded = Wad != null;
            panelVersion.Visible = wadLoaded;
            panelMetadata.Visible = wadLoaded;
            panelTree.Visible = wadLoaded;

            tbNotes.ReadOnly = ReadOnly;

            // Update game version control
            if (wadLoaded)
            {
                if (!Wad.GameVersion.Equals(suggestedGameVersionComboBox.SelectedItem))
                    suggestedGameVersionComboBox.SelectedItem = Wad.GameVersion;
            }
            else
                suggestedGameVersionComboBox.SelectedItem = null;

            // Update metadata
            UpdateMetadata();

            // Update tree
            KeepSelection(() =>
            {
                var nodes = tree.Nodes.ToList();
                tree.Nodes.Clear();
                if (Wad == null)
                    return;

                {
                    var mainNode = AddOrReuseChild(nodes, "Moveables");
                    UpdateList(mainNode, _wad.Moveables.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }
                {
                    var mainNode = AddOrReuseChild(nodes, "Statics");
                    UpdateList(mainNode, _wad.Statics.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }
                {
                    var mainNode = AddOrReuseChild(nodes, "Sprite sequences");
                    UpdateList(mainNode, _wad.SpriteSequences.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }

                tree.Nodes.AddRange(nodes);
            });
        }

        public void UpdateMetadata()
        {
            tbDate.Text = Wad?.Timestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
            tbNotes.Text = Wad?.UserNotes;
        }

        private static DarkTreeNode AddOrReuseChild(IList<DarkTreeNode> nodes, string text)
        {
            foreach (DarkTreeNode childNode in nodes)
                if (childNode.Text.Equals(text, StringComparison.InvariantCulture))
                    return childNode;
            {
                DarkTreeNode childNode = new DarkTreeNode(text) { Expanded = false };
                nodes.Add(childNode);
                return childNode;
            }
        }

        // This function tries to recycle tree nodes to preserve their extended attribute.
        private void UpdateList<T>(DarkTreeNode oldNode, IEnumerable<T> listOfThings, Func<T, string> formatObject)
        {
            IDictionary<object, DarkTreeNode> oldChildNodeLookup = oldNode.Nodes.ToDictionary(node => node.Tag);
            var newChildNodes = new List<DarkTreeNode>();

            foreach (T thing in listOfThings)
            {
                DarkTreeNode childNode;
                if (!oldChildNodeLookup.TryGetValue(thing, out childNode))
                    childNode = new DarkTreeNode { Tag = thing, Expanded = false };
                childNode.Text = formatObject(thing);
                newChildNodes.Add(childNode);
            }

            oldNode.Nodes.Clear();
            oldNode.Nodes.AddRange(newChildNodes);
        }

        private void KeepSelection(Action update)
        {
            var selectedNodes = new HashSet<object>(tree.SelectedNodes.Select(node => node.Tag).Where(tag => tag != null));
            tree.SelectedNodes.Clear();

            update();

            try
            {
                // Update nodes
                _changing = true;

                // Restore selection
                var newSelectedNodes = new List<DarkTreeNode>();
                foreach (DarkTreeNode node in CollectAllNodes(tree.Nodes))
                    if (node.Tag != null)
                        if (selectedNodes.Contains(node.Tag))
                            newSelectedNodes.Add(node);
                tree.SelectNodes(newSelectedNodes);
            }
            finally
            {
                _changing = false;
            }

            SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);

            // Workaround update problems
            Invalidate();
        }

        public static IEnumerable<DarkTreeNode> CollectAllNodes(IEnumerable<DarkTreeNode> @this)
        {
            foreach (DarkTreeNode node in @this)
            {
                yield return node;
                if (node.Nodes.Count != 0)
                    foreach (DarkTreeNode child in CollectAllNodes(node.Nodes))
                        yield return child;
            }
        }

        public void Select(List<IWadObjectId> IdList)
        {
            var list = CollectAllNodes(tree.Nodes).ToList();
            List<DarkTreeNode> selectedNodesList = list.Where(node => node.Tag is IWadObjectId && IdList.Any(entry => entry.ToString() == ((IWadObjectId)(node.Tag)).ToString())).ToList();

            if (selectedNodesList.Count > 0)
            {
                tree.SelectedNodes.Clear();

                foreach(var node in selectedNodesList)
                {
                    var currentNode = node;
                    tree.SelectedNodes.Add(currentNode);

                    // Expand
                    while (currentNode != null)
                    {
                        currentNode.Expanded = true;
                        currentNode = currentNode.ParentNode;
                    }
                }

                tree.EnsureVisible();
            }
        }
        public void Select(IWadObjectId Id) => Select(new List<IWadObjectId>() { Id });
        public void SelectFirst() => Select((IWadObjectId)CollectAllNodes(tree.Nodes).FirstOrDefault(node => node.Tag is IWadObjectId).Tag);

        private void suggestedGameVersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selection = suggestedGameVersionComboBox.SelectedItem;
            if (selection == null)
                return;
            Wad.GameVersion = (TRVersion.Game)selection;
            UpdateContent();
        }

        private void tree_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void tree_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void Tree_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void Tree_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void Tree_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void InitializeComponent()
        {
            this.tree = new DarkUI.Controls.DarkTreeView();
            this.suggestedGameVersionComboBox = new DarkUI.Controls.DarkComboBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbNotes = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbDate = new DarkUI.Controls.DarkTextBox();
            this.panelMetadata = new DarkUI.Controls.DarkPanel();
            this.panelTree = new DarkUI.Controls.DarkPanel();
            this.panelVersion = new DarkUI.Controls.DarkPanel();
            this.panelMetadata.SuspendLayout();
            this.panelTree.SuspendLayout();
            this.panelVersion.SuspendLayout();
            this.SuspendLayout();
            // 
            // tree
            // 
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.ExpandOnDoubleClick = false;
            this.tree.Location = new System.Drawing.Point(0, 0);
            this.tree.MaxDragChange = 20;
            this.tree.MultiSelect = true;
            this.tree.Name = "tree";
            this.tree.Size = new System.Drawing.Size(288, 561);
            this.tree.TabIndex = 1;
            this.tree.Click += new System.EventHandler(this.tree_Click);
            this.tree.DoubleClick += new System.EventHandler(this.tree_DoubleClick);
            this.tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Tree_KeyDown);
            this.tree.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Tree_KeyPress);
            this.tree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Tree_KeyUp);
            this.tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tree_MouseDown);
            // 
            // suggestedGameVersionComboBox
            // 
            this.suggestedGameVersionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.suggestedGameVersionComboBox.FormattingEnabled = true;
            this.suggestedGameVersionComboBox.Location = new System.Drawing.Point(81, 3);
            this.suggestedGameVersionComboBox.Name = "suggestedGameVersionComboBox";
            this.suggestedGameVersionComboBox.Size = new System.Drawing.Size(178, 21);
            this.suggestedGameVersionComboBox.TabIndex = 0;
            this.suggestedGameVersionComboBox.SelectedIndexChanged += new System.EventHandler(this.suggestedGameVersionComboBox_SelectedIndexChanged);
            this.suggestedGameVersionComboBox.Resize += new System.EventHandler(this.suggestedGameVersionComboBox_Resize);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(0, 6);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(75, 13);
            this.darkLabel1.TabIndex = 3;
            this.darkLabel1.Text = "Game version:";
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombLib.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(265, 3);
            this.butSearch.Name = "butSearch";
            this.butSearch.Size = new System.Drawing.Size(21, 21);
            this.butSearch.TabIndex = 5;
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.BackColor = System.Drawing.Color.Transparent;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 7);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(34, 13);
            this.darkLabel2.TabIndex = 0;
            this.darkLabel2.Text = "Date:";
            // 
            // tbNotes
            // 
            this.tbNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNotes.Location = new System.Drawing.Point(40, 33);
            this.tbNotes.Name = "tbNotes";
            this.tbNotes.Size = new System.Drawing.Size(248, 22);
            this.tbNotes.TabIndex = 1;
            this.tbNotes.TextChanged += new System.EventHandler(this.tbNotes_TextChanged);
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.BackColor = System.Drawing.Color.Transparent;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 35);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(40, 13);
            this.darkLabel3.TabIndex = 2;
            this.darkLabel3.Text = "Notes:";
            // 
            // tbDate
            // 
            this.tbDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDate.Location = new System.Drawing.Point(40, 5);
            this.tbDate.Name = "tbDate";
            this.tbDate.ReadOnly = true;
            this.tbDate.Size = new System.Drawing.Size(248, 22);
            this.tbDate.TabIndex = 3;
            // 
            // panelMetadata
            // 
            this.panelMetadata.Controls.Add(this.tbDate);
            this.panelMetadata.Controls.Add(this.darkLabel3);
            this.panelMetadata.Controls.Add(this.darkLabel2);
            this.panelMetadata.Controls.Add(this.tbNotes);
            this.panelMetadata.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelMetadata.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelMetadata.Location = new System.Drawing.Point(0, 591);
            this.panelMetadata.Name = "panelMetadata";
            this.panelMetadata.Size = new System.Drawing.Size(288, 56);
            this.panelMetadata.TabIndex = 11;
            // 
            // panelTree
            // 
            this.panelTree.Controls.Add(this.tree);
            this.panelTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTree.Location = new System.Drawing.Point(0, 30);
            this.panelTree.Name = "panelTree";
            this.panelTree.Size = new System.Drawing.Size(288, 561);
            this.panelTree.TabIndex = 12;
            // 
            // panelVersion
            // 
            this.panelVersion.Controls.Add(this.suggestedGameVersionComboBox);
            this.panelVersion.Controls.Add(this.darkLabel1);
            this.panelVersion.Controls.Add(this.butSearch);
            this.panelVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelVersion.Location = new System.Drawing.Point(0, 0);
            this.panelVersion.Margin = new System.Windows.Forms.Padding(0);
            this.panelVersion.Name = "panelVersion";
            this.panelVersion.Size = new System.Drawing.Size(288, 30);
            this.panelVersion.TabIndex = 13;
            // 
            // WadTreeView
            // 
            this.Controls.Add(this.panelTree);
            this.Controls.Add(this.panelVersion);
            this.Controls.Add(this.panelMetadata);
            this.Name = "WadTreeView";
            this.Size = new System.Drawing.Size(288, 647);
            this.Click += new System.EventHandler(this.WadTreeView_Click);
            this.panelMetadata.ResumeLayout(false);
            this.panelMetadata.PerformLayout();
            this.panelTree.ResumeLayout(false);
            this.panelVersion.ResumeLayout(false);
            this.panelVersion.PerformLayout();
            this.ResumeLayout(false);

        }

        [DefaultValue(true)]
        public bool MultiSelect
        {
            get { return tree.MultiSelect; }
            set { tree.MultiSelect = value; }
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.IntersectClip(new RectangleF(new PointF(), ClientSize));

            // Draw notify message if no wad is loaded
            if (_wad == null)
            {
                // Draw background
                using (var b = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(b, ClientRectangle);

                string notifyMessage = "Click here to load new wad file.";

                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(notifyMessage, Font, b, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void WadTreeView_Click(object sender, EventArgs e)
        {
            if (_wad == null && ClickOnEmpty != null) ClickOnEmpty(this, e);
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(tree) { ShowAboveControl = true };
                searchPopUp.Show(this);
        }

        private void suggestedGameVersionComboBox_Resize(object sender, EventArgs e)
        {
            butSearch.Size = new Size(suggestedGameVersionComboBox.Size.Height, suggestedGameVersionComboBox.Size.Height);
            butSearch.Location = new Point(ClientSize.Width - butSearch.Size.Width - Padding.Right - Margin.Right, suggestedGameVersionComboBox.Location.Y);
            suggestedGameVersionComboBox.Size = new Size(ClientSize.Width - suggestedGameVersionComboBox.Location.X - butSearch.Size.Width - Padding.Right - Margin.Right - 5, suggestedGameVersionComboBox.Size.Height);
        }

        private void tbNotes_TextChanged(object sender, EventArgs e)
        {
            Wad.UserNotes = tbNotes.Text;
        }
    }
}

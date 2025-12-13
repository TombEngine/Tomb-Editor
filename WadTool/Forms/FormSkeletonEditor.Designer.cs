namespace WadTool
{
    partial class FormSkeletonEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSkeletonEditor));
            treeSkeleton = new DarkUI.Controls.DarkTreeView();
            cbDrawGizmo = new DarkUI.Controls.DarkCheckBox();
            cbDrawGrid = new DarkUI.Controls.DarkCheckBox();
            butSaveChanges = new DarkUI.Controls.DarkButton();
            butRenameBone = new DarkUI.Controls.DarkButton();
            butDeleteBone = new DarkUI.Controls.DarkButton();
            butLoadModel = new DarkUI.Controls.DarkButton();
            darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            butAddFromWad2 = new DarkUI.Controls.DarkButton();
            butAddFromFile = new DarkUI.Controls.DarkButton();
            butReplaceFromWad2 = new DarkUI.Controls.DarkButton();
            butReplaceFromFile = new DarkUI.Controls.DarkButton();
            cmBone = new DarkUI.Controls.DarkContextMenu();
            popToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            addChildBoneFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            addChildBoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceFromWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sectionCurrentBone = new DarkUI.Controls.DarkSectionPanel();
            comboLightType = new DarkUI.Controls.DarkComboBox();
            butSetToAll = new DarkUI.Controls.DarkButton();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel29 = new DarkUI.Controls.DarkLabel();
            darkLabel27 = new DarkUI.Controls.DarkLabel();
            nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel26 = new DarkUI.Controls.DarkLabel();
            darkLabel21 = new DarkUI.Controls.DarkLabel();
            panelAll = new DarkUI.Controls.DarkPanel();
            panelMain = new DarkUI.Controls.DarkPanel();
            panelRendering = new Controls.PanelRenderingSkeleton();
            panelRight = new DarkUI.Controls.DarkPanel();
            section3D = new DarkUI.Controls.DarkSectionPanel();
            darkPanel1 = new DarkUI.Controls.DarkPanel();
            butEditMesh = new DarkUI.Controls.DarkButton();
            butExportSelectedMesh = new DarkUI.Controls.DarkButton();
            panelSkinned = new DarkUI.Controls.DarkPanel();
            darkPanel2 = new DarkUI.Controls.DarkPanel();
            lblSkin = new DarkUI.Controls.DarkLabel();
            butClearSkin = new DarkUI.Controls.DarkButton();
            butSetSkin = new DarkUI.Controls.DarkButton();
            butCancel = new DarkUI.Controls.DarkButton();
            toolTip = new System.Windows.Forms.ToolTip(components);
            cmBone.SuspendLayout();
            sectionCurrentBone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransX).BeginInit();
            panelAll.SuspendLayout();
            panelMain.SuspendLayout();
            panelRight.SuspendLayout();
            section3D.SuspendLayout();
            darkPanel1.SuspendLayout();
            panelSkinned.SuspendLayout();
            darkPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // treeSkeleton
            // 
            treeSkeleton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            treeSkeleton.ExpandOnDoubleClick = false;
            treeSkeleton.Location = new System.Drawing.Point(2, 3);
            treeSkeleton.MaxDragChange = 20;
            treeSkeleton.Name = "treeSkeleton";
            treeSkeleton.Size = new System.Drawing.Size(310, 327);
            treeSkeleton.TabIndex = 0;
            treeSkeleton.Click += treeSkeleton_Click;
            treeSkeleton.MouseDown += treeSkeleton_MouseDown;
            // 
            // cbDrawGizmo
            // 
            cbDrawGizmo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbDrawGizmo.AutoSize = true;
            cbDrawGizmo.Checked = true;
            cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            cbDrawGizmo.Location = new System.Drawing.Point(86, 570);
            cbDrawGizmo.Name = "cbDrawGizmo";
            cbDrawGizmo.Size = new System.Drawing.Size(87, 17);
            cbDrawGizmo.TabIndex = 82;
            cbDrawGizmo.Text = "Draw gizmo";
            toolTip.SetToolTip(cbDrawGizmo, "Draw gizmo");
            cbDrawGizmo.CheckedChanged += cbDrawGizmo_CheckedChanged;
            // 
            // cbDrawGrid
            // 
            cbDrawGrid.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbDrawGrid.AutoSize = true;
            cbDrawGrid.Checked = true;
            cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            cbDrawGrid.Location = new System.Drawing.Point(7, 570);
            cbDrawGrid.Name = "cbDrawGrid";
            cbDrawGrid.Size = new System.Drawing.Size(77, 17);
            cbDrawGrid.TabIndex = 81;
            cbDrawGrid.Text = "Draw grid";
            toolTip.SetToolTip(cbDrawGrid, "Draw coordinate grid");
            cbDrawGrid.CheckedChanged += cbDrawGrid_CheckedChanged;
            // 
            // butSaveChanges
            // 
            butSaveChanges.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butSaveChanges.Checked = false;
            butSaveChanges.Location = new System.Drawing.Point(609, 566);
            butSaveChanges.Name = "butSaveChanges";
            butSaveChanges.Size = new System.Drawing.Size(81, 23);
            butSaveChanges.TabIndex = 80;
            butSaveChanges.Text = "OK";
            butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butSaveChanges.Click += butSaveChanges_Click;
            // 
            // butRenameBone
            // 
            butRenameBone.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butRenameBone.Checked = false;
            butRenameBone.Location = new System.Drawing.Point(3, 335);
            butRenameBone.Name = "butRenameBone";
            butRenameBone.Size = new System.Drawing.Size(100, 23);
            butRenameBone.TabIndex = 84;
            butRenameBone.Text = "Rename";
            butRenameBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butRenameBone, "Rename currently selected bone");
            butRenameBone.Click += butRenameBone_Click;
            // 
            // butDeleteBone
            // 
            butDeleteBone.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butDeleteBone.Checked = false;
            butDeleteBone.Location = new System.Drawing.Point(214, 335);
            butDeleteBone.Name = "butDeleteBone";
            butDeleteBone.Size = new System.Drawing.Size(99, 23);
            butDeleteBone.TabIndex = 83;
            butDeleteBone.Text = "Delete";
            butDeleteBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butDeleteBone, "Delete currently selected bone");
            butDeleteBone.Click += butDeleteBone_Click;
            // 
            // butLoadModel
            // 
            butLoadModel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butLoadModel.Checked = false;
            butLoadModel.Location = new System.Drawing.Point(214, 393);
            butLoadModel.Name = "butLoadModel";
            butLoadModel.Size = new System.Drawing.Size(99, 23);
            butLoadModel.TabIndex = 80;
            butLoadModel.Text = "Replace model";
            butLoadModel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butLoadModel, "Replace all meshes in skeleton from single 3D model");
            butLoadModel.Click += butLoadModel_Click;
            // 
            // darkStatusStrip1
            // 
            darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkStatusStrip1.Location = new System.Drawing.Point(0, 595);
            darkStatusStrip1.Name = "darkStatusStrip1";
            darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            darkStatusStrip1.Size = new System.Drawing.Size(784, 24);
            darkStatusStrip1.TabIndex = 85;
            darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // butAddFromWad2
            // 
            butAddFromWad2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butAddFromWad2.Checked = false;
            butAddFromWad2.Location = new System.Drawing.Point(109, 364);
            butAddFromWad2.Name = "butAddFromWad2";
            butAddFromWad2.Size = new System.Drawing.Size(99, 23);
            butAddFromWad2.TabIndex = 87;
            butAddFromWad2.Text = "Add existing";
            butAddFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butAddFromWad2, "Make new bone by using existing mesh from current wad");
            butAddFromWad2.Click += butSelectMesh_Click;
            // 
            // butAddFromFile
            // 
            butAddFromFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butAddFromFile.Checked = false;
            butAddFromFile.Location = new System.Drawing.Point(3, 364);
            butAddFromFile.Name = "butAddFromFile";
            butAddFromFile.Size = new System.Drawing.Size(100, 23);
            butAddFromFile.TabIndex = 86;
            butAddFromFile.Text = "Add new";
            butAddFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butAddFromFile, "Make new bone by importing 3D model");
            butAddFromFile.Click += butAddFromFile_Click;
            // 
            // butReplaceFromWad2
            // 
            butReplaceFromWad2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butReplaceFromWad2.Checked = false;
            butReplaceFromWad2.Location = new System.Drawing.Point(214, 364);
            butReplaceFromWad2.Name = "butReplaceFromWad2";
            butReplaceFromWad2.Size = new System.Drawing.Size(99, 23);
            butReplaceFromWad2.TabIndex = 90;
            butReplaceFromWad2.Text = "Replace existing";
            butReplaceFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butReplaceFromWad2, "Replace mesh for currently selected bone with existing one in wad");
            butReplaceFromWad2.Click += butReplaceFromWad2_Click;
            // 
            // butReplaceFromFile
            // 
            butReplaceFromFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butReplaceFromFile.Checked = false;
            butReplaceFromFile.Location = new System.Drawing.Point(3, 393);
            butReplaceFromFile.Name = "butReplaceFromFile";
            butReplaceFromFile.Size = new System.Drawing.Size(100, 23);
            butReplaceFromFile.TabIndex = 89;
            butReplaceFromFile.Text = "Import mesh";
            butReplaceFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butReplaceFromFile, "Import new mesh for currently selected bone from 3D model");
            butReplaceFromFile.Click += butReplaceFromFile_Click;
            // 
            // cmBone
            // 
            cmBone.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmBone.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmBone.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { popToolStripMenuItem, pushToolStripMenuItem, toolStripMenuItem1, moveUpToolStripMenuItem, moveDownToolStripMenuItem, toolStripMenuItem2, addChildBoneFromFileToolStripMenuItem, replaceToolStripMenuItem, toolStripMenuItem4, addChildBoneToolStripMenuItem, replaceFromWad2ToolStripMenuItem, toolStripMenuItem3, deleteToolStripMenuItem, renameToolStripMenuItem });
            cmBone.Name = "cmBone";
            cmBone.Size = new System.Drawing.Size(218, 252);
            // 
            // popToolStripMenuItem
            // 
            popToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            popToolStripMenuItem.CheckOnClick = true;
            popToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            popToolStripMenuItem.Name = "popToolStripMenuItem";
            popToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            popToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            popToolStripMenuItem.Text = "Pop";
            popToolStripMenuItem.Click += PopToolStripMenuItem_Click;
            // 
            // pushToolStripMenuItem
            // 
            pushToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            pushToolStripMenuItem.CheckOnClick = true;
            pushToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            pushToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+P";
            pushToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            pushToolStripMenuItem.Text = "Push";
            pushToolStripMenuItem.Click += PushToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(214, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            moveUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            moveUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            moveUpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up;
            moveUpToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            moveUpToolStripMenuItem.Text = "Move up";
            moveUpToolStripMenuItem.Click += MoveUpToolStripMenuItem_Click;
            // 
            // moveDownToolStripMenuItem
            // 
            moveDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            moveDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            moveDownToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down;
            moveDownToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            moveDownToolStripMenuItem.Text = "Move down";
            moveDownToolStripMenuItem.Click += MoveDownToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(214, 6);
            // 
            // addChildBoneFromFileToolStripMenuItem
            // 
            addChildBoneFromFileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            addChildBoneFromFileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            addChildBoneFromFileToolStripMenuItem.Image = Properties.Resources.general_plus_math_16;
            addChildBoneFromFileToolStripMenuItem.Name = "addChildBoneFromFileToolStripMenuItem";
            addChildBoneFromFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            addChildBoneFromFileToolStripMenuItem.Text = "Add child bone from file";
            addChildBoneFromFileToolStripMenuItem.Click += AddChildBoneFromFileToolStripMenuItem_Click;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            replaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            replaceToolStripMenuItem.Image = Properties.Resources.actions_refresh_16;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            replaceToolStripMenuItem.Text = "Replace from file";
            replaceToolStripMenuItem.Click += ReplaceToolStripMenuItem_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(214, 6);
            // 
            // addChildBoneToolStripMenuItem
            // 
            addChildBoneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            addChildBoneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            addChildBoneToolStripMenuItem.Image = Properties.Resources.general_search_16;
            addChildBoneToolStripMenuItem.Name = "addChildBoneToolStripMenuItem";
            addChildBoneToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            addChildBoneToolStripMenuItem.Text = "Add child bone from Wad2";
            addChildBoneToolStripMenuItem.Click += AddChildBoneToolStripMenuItem_Click;
            // 
            // replaceFromWad2ToolStripMenuItem
            // 
            replaceFromWad2ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            replaceFromWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            replaceFromWad2ToolStripMenuItem.Image = Properties.Resources.general_search_16;
            replaceFromWad2ToolStripMenuItem.Name = "replaceFromWad2ToolStripMenuItem";
            replaceFromWad2ToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            replaceFromWad2ToolStripMenuItem.Text = "Replace from Wad2";
            replaceFromWad2ToolStripMenuItem.Click += ReplaceFromWad2ToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(214, 6);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            deleteToolStripMenuItem.Image = Properties.Resources.trash_16;
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += DeleteToolStripMenuItem_Click;
            // 
            // renameToolStripMenuItem
            // 
            renameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            renameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            renameToolStripMenuItem.Image = Properties.Resources.edit_16;
            renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            renameToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            renameToolStripMenuItem.Text = "Rename";
            renameToolStripMenuItem.Click += RenameToolStripMenuItem_Click;
            // 
            // sectionCurrentBone
            // 
            sectionCurrentBone.Controls.Add(comboLightType);
            sectionCurrentBone.Controls.Add(butSetToAll);
            sectionCurrentBone.Controls.Add(darkLabel1);
            sectionCurrentBone.Controls.Add(nudTransZ);
            sectionCurrentBone.Controls.Add(nudTransY);
            sectionCurrentBone.Controls.Add(darkLabel29);
            sectionCurrentBone.Controls.Add(darkLabel27);
            sectionCurrentBone.Controls.Add(nudTransX);
            sectionCurrentBone.Controls.Add(darkLabel26);
            sectionCurrentBone.Controls.Add(darkLabel21);
            sectionCurrentBone.Dock = System.Windows.Forms.DockStyle.Bottom;
            sectionCurrentBone.Location = new System.Drawing.Point(0, 472);
            sectionCurrentBone.Name = "sectionCurrentBone";
            sectionCurrentBone.SectionHeader = "Current mesh";
            sectionCurrentBone.Size = new System.Drawing.Size(316, 82);
            sectionCurrentBone.TabIndex = 107;
            // 
            // comboLightType
            // 
            comboLightType.FormattingEnabled = true;
            comboLightType.Items.AddRange(new object[] { "Dynamic Lighting", "Static Lighting" });
            comboLightType.Location = new System.Drawing.Point(82, 56);
            comboLightType.Name = "comboLightType";
            comboLightType.Size = new System.Drawing.Size(147, 23);
            comboLightType.TabIndex = 106;
            comboLightType.SelectedIndexChanged += comboLightType_SelectedIndexChanged;
            // 
            // butSetToAll
            // 
            butSetToAll.Checked = false;
            butSetToAll.Location = new System.Drawing.Point(235, 56);
            butSetToAll.Name = "butSetToAll";
            butSetToAll.Size = new System.Drawing.Size(78, 23);
            butSetToAll.TabIndex = 108;
            butSetToAll.Text = "Set to all";
            butSetToAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butSetToAll.Click += butSetToAll_Click;
            // 
            // darkLabel1
            // 
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(4, 59);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(76, 13);
            darkLabel1.TabIndex = 107;
            darkLabel1.Text = "Light mode:";
            // 
            // nudTransZ
            // 
            nudTransZ.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudTransZ.DecimalPlaces = 4;
            nudTransZ.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransZ.Location = new System.Drawing.Point(248, 28);
            nudTransZ.LoopValues = false;
            nudTransZ.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransZ.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransZ.Name = "nudTransZ";
            nudTransZ.Size = new System.Drawing.Size(65, 22);
            nudTransZ.TabIndex = 104;
            nudTransZ.ValueChanged += nudTransZ_ValueChanged;
            // 
            // nudTransY
            // 
            nudTransY.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudTransY.DecimalPlaces = 4;
            nudTransY.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransY.Location = new System.Drawing.Point(165, 28);
            nudTransY.LoopValues = false;
            nudTransY.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransY.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransY.Name = "nudTransY";
            nudTransY.Size = new System.Drawing.Size(64, 22);
            nudTransY.TabIndex = 103;
            nudTransY.ValueChanged += nudTransY_ValueChanged;
            // 
            // darkLabel29
            // 
            darkLabel29.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel29.Location = new System.Drawing.Point(4, 30);
            darkLabel29.Name = "darkLabel29";
            darkLabel29.Size = new System.Drawing.Size(60, 13);
            darkLabel29.TabIndex = 105;
            darkLabel29.Text = "Pivot:";
            // 
            // darkLabel27
            // 
            darkLabel27.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel27.AutoSize = true;
            darkLabel27.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel27.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel27.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel27.Location = new System.Drawing.Point(66, 30);
            darkLabel27.Name = "darkLabel27";
            darkLabel27.Size = new System.Drawing.Size(14, 13);
            darkLabel27.TabIndex = 99;
            darkLabel27.Text = "X";
            // 
            // nudTransX
            // 
            nudTransX.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudTransX.DecimalPlaces = 4;
            nudTransX.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransX.Location = new System.Drawing.Point(82, 28);
            nudTransX.LoopValues = false;
            nudTransX.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransX.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransX.Name = "nudTransX";
            nudTransX.Size = new System.Drawing.Size(64, 22);
            nudTransX.TabIndex = 102;
            nudTransX.ValueChanged += nudTransX_ValueChanged;
            // 
            // darkLabel26
            // 
            darkLabel26.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel26.AutoSize = true;
            darkLabel26.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel26.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel26.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel26.Location = new System.Drawing.Point(150, 30);
            darkLabel26.Name = "darkLabel26";
            darkLabel26.Size = new System.Drawing.Size(14, 13);
            darkLabel26.TabIndex = 100;
            darkLabel26.Text = "Y";
            // 
            // darkLabel21
            // 
            darkLabel21.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel21.AutoSize = true;
            darkLabel21.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel21.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel21.Location = new System.Drawing.Point(233, 30);
            darkLabel21.Name = "darkLabel21";
            darkLabel21.Size = new System.Drawing.Size(14, 13);
            darkLabel21.TabIndex = 101;
            darkLabel21.Text = "Z";
            // 
            // panelAll
            // 
            panelAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panelAll.Controls.Add(panelMain);
            panelAll.Controls.Add(panelRight);
            panelAll.Location = new System.Drawing.Point(7, 6);
            panelAll.Name = "panelAll";
            panelAll.Size = new System.Drawing.Size(770, 554);
            panelAll.TabIndex = 108;
            // 
            // panelMain
            // 
            panelMain.Controls.Add(panelRendering);
            panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMain.Location = new System.Drawing.Point(0, 0);
            panelMain.Name = "panelMain";
            panelMain.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            panelMain.Size = new System.Drawing.Size(454, 554);
            panelMain.TabIndex = 0;
            // 
            // panelRendering
            // 
            panelRendering.AllowRendering = true;
            panelRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRendering.Location = new System.Drawing.Point(0, 0);
            panelRendering.Name = "panelRendering";
            panelRendering.Size = new System.Drawing.Size(452, 554);
            panelRendering.TabIndex = 0;
            panelRendering.MouseDoubleClick += panelRendering_MouseDoubleClick;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(section3D);
            panelRight.Controls.Add(sectionCurrentBone);
            panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            panelRight.Location = new System.Drawing.Point(454, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(316, 554);
            panelRight.TabIndex = 1;
            // 
            // section3D
            // 
            section3D.Controls.Add(darkPanel1);
            section3D.Controls.Add(panelSkinned);
            section3D.Dock = System.Windows.Forms.DockStyle.Fill;
            section3D.Location = new System.Drawing.Point(0, 0);
            section3D.Name = "section3D";
            section3D.SectionHeader = "Mesh tree";
            section3D.Size = new System.Drawing.Size(316, 472);
            section3D.TabIndex = 108;
            // 
            // darkPanel1
            // 
            darkPanel1.Controls.Add(treeSkeleton);
            darkPanel1.Controls.Add(butAddFromWad2);
            darkPanel1.Controls.Add(butReplaceFromFile);
            darkPanel1.Controls.Add(butRenameBone);
            darkPanel1.Controls.Add(butReplaceFromWad2);
            darkPanel1.Controls.Add(butAddFromFile);
            darkPanel1.Controls.Add(butDeleteBone);
            darkPanel1.Controls.Add(butEditMesh);
            darkPanel1.Controls.Add(butLoadModel);
            darkPanel1.Controls.Add(butExportSelectedMesh);
            darkPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            darkPanel1.Location = new System.Drawing.Point(1, 25);
            darkPanel1.Name = "darkPanel1";
            darkPanel1.Size = new System.Drawing.Size(314, 417);
            darkPanel1.TabIndex = 113;
            // 
            // butEditMesh
            // 
            butEditMesh.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butEditMesh.Checked = false;
            butEditMesh.Location = new System.Drawing.Point(109, 335);
            butEditMesh.Name = "butEditMesh";
            butEditMesh.Size = new System.Drawing.Size(99, 23);
            butEditMesh.TabIndex = 92;
            butEditMesh.Text = "Edit mesh";
            butEditMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butEditMesh, "Edit mesh for currently selected bone");
            butEditMesh.Click += butEditMesh_Click;
            // 
            // butExportSelectedMesh
            // 
            butExportSelectedMesh.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butExportSelectedMesh.Checked = false;
            butExportSelectedMesh.Location = new System.Drawing.Point(109, 393);
            butExportSelectedMesh.Name = "butExportSelectedMesh";
            butExportSelectedMesh.Size = new System.Drawing.Size(99, 23);
            butExportSelectedMesh.TabIndex = 91;
            butExportSelectedMesh.Text = "Export mesh";
            butExportSelectedMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butExportSelectedMesh, "Export currently selected bone's mesh to 3D model");
            butExportSelectedMesh.Click += butExportSelectedMesh_Click;
            // 
            // panelSkinned
            // 
            panelSkinned.Controls.Add(darkPanel2);
            panelSkinned.Controls.Add(butClearSkin);
            panelSkinned.Controls.Add(butSetSkin);
            panelSkinned.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelSkinned.Location = new System.Drawing.Point(1, 442);
            panelSkinned.Name = "panelSkinned";
            panelSkinned.Size = new System.Drawing.Size(314, 29);
            panelSkinned.TabIndex = 93;
            // 
            // darkPanel2
            // 
            darkPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            darkPanel2.Controls.Add(lblSkin);
            darkPanel2.Location = new System.Drawing.Point(3, 3);
            darkPanel2.Name = "darkPanel2";
            darkPanel2.Size = new System.Drawing.Size(205, 23);
            darkPanel2.TabIndex = 109;
            // 
            // lblSkin
            // 
            lblSkin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSkin.ForeColor = System.Drawing.Color.Gray;
            lblSkin.Location = new System.Drawing.Point(3, 5);
            lblSkin.Name = "lblSkin";
            lblSkin.Size = new System.Drawing.Size(199, 13);
            lblSkin.TabIndex = 106;
            lblSkin.Text = "Skinned mesh:";
            lblSkin.Click += butSetSkin_Click;
            // 
            // butClearSkin
            // 
            butClearSkin.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butClearSkin.Checked = false;
            butClearSkin.Location = new System.Drawing.Point(266, 3);
            butClearSkin.Name = "butClearSkin";
            butClearSkin.Size = new System.Drawing.Size(47, 23);
            butClearSkin.TabIndex = 108;
            butClearSkin.Text = "Clear";
            butClearSkin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butClearSkin, "Remove skinned mesh");
            butClearSkin.Click += butClearSkin_Click;
            // 
            // butSetSkin
            // 
            butSetSkin.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butSetSkin.Checked = false;
            butSetSkin.Location = new System.Drawing.Point(214, 3);
            butSetSkin.Name = "butSetSkin";
            butSetSkin.Size = new System.Drawing.Size(47, 23);
            butSetSkin.TabIndex = 107;
            butSetSkin.Text = "Add";
            butSetSkin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butSetSkin, "Add skinned mesh");
            butSetSkin.Click += butSetSkin_Click;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.Location = new System.Drawing.Point(696, 566);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(81, 23);
            butCancel.TabIndex = 112;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // FormSkeletonEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 619);
            Controls.Add(butCancel);
            Controls.Add(panelAll);
            Controls.Add(cbDrawGrid);
            Controls.Add(cbDrawGizmo);
            Controls.Add(darkStatusStrip1);
            Controls.Add(butSaveChanges);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(800, 500);
            Name = "FormSkeletonEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Skeleton editor";
            KeyDown += formSkeletonEditor_KeyDown;
            cmBone.ResumeLayout(false);
            sectionCurrentBone.ResumeLayout(false);
            sectionCurrentBone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransX).EndInit();
            panelAll.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            section3D.ResumeLayout(false);
            darkPanel1.ResumeLayout(false);
            panelSkinned.ResumeLayout(false);
            darkPanel2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeSkeleton;
        private DarkUI.Controls.DarkCheckBox cbDrawGizmo;
        private DarkUI.Controls.DarkCheckBox cbDrawGrid;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkButton butRenameBone;
        private DarkUI.Controls.DarkButton butDeleteBone;
        private DarkUI.Controls.DarkButton butLoadModel;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkButton butAddFromWad2;
        private DarkUI.Controls.DarkButton butAddFromFile;
        private DarkUI.Controls.DarkButton butReplaceFromWad2;
        private DarkUI.Controls.DarkButton butReplaceFromFile;
        private DarkUI.Controls.DarkContextMenu cmBone;
        private System.Windows.Forms.ToolStripMenuItem popToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pushToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceFromWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChildBoneFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChildBoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private DarkUI.Controls.DarkSectionPanel sectionCurrentBone;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkLabel darkLabel27;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkLabel darkLabel29;
        private DarkUI.Controls.DarkLabel darkLabel26;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkPanel panelAll;
        private DarkUI.Controls.DarkPanel panelRight;
        private DarkUI.Controls.DarkSectionPanel section3D;
        private DarkUI.Controls.DarkPanel panelMain;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butExportSelectedMesh;
        private Controls.PanelRenderingSkeleton panelRendering;
        private DarkUI.Controls.DarkButton butSetToAll;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboLightType;
        private DarkUI.Controls.DarkButton butEditMesh;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkPanel panelSkinned;
        private DarkUI.Controls.DarkButton butSetSkin;
        private DarkUI.Controls.DarkLabel lblSkin;
        private DarkUI.Controls.DarkButton butClearSkin;
        private DarkUI.Controls.DarkPanel darkPanel1;
        private DarkUI.Controls.DarkPanel darkPanel2;
    }
}
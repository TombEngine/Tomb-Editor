using TombLib.Controls;
using TombLib.Wad;

namespace WadTool
{
    partial class FormAnimationEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAnimationEditor));
            topMenu = new DarkUI.Controls.DarkMenuStrip();
            fileeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            animationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            curToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            batchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            resampleAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resampleAnimationToKeyframesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mirrorAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reverseAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            fixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            currentAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectedAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            allAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            findReplaceAnimcommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            frameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertFrameAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertnFramesAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteEveryNthFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            interpolateFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            calculateCollisionBoxForCurrentFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteCollisionBoxForCurrentFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renderingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawGizmoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawCollisionBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            drawSkinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem(); // da develop
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            smoothAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scrollGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            restoreGridHeightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new DarkUI.Controls.DarkStatusStrip();
            statusFrame = new System.Windows.Forms.ToolStripStatusLabel();
            darkLabel22 = new DarkUI.Controls.DarkLabel();
            darkLabel23 = new DarkUI.Controls.DarkLabel();
            darkLabel24 = new DarkUI.Controls.DarkLabel();
            darkLabel25 = new DarkUI.Controls.DarkLabel();
            tbStateId = new DarkAutocompleteTextBox();
            darkLabel7 = new DarkUI.Controls.DarkLabel();
            darkLabel6 = new DarkUI.Controls.DarkLabel();
            darkLabel5 = new DarkUI.Controls.DarkLabel();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            tbName = new DarkUI.Controls.DarkTextBox();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            tbSearchAnimation = new DarkUI.Controls.DarkTextBox();
            topBar = new DarkUI.Controls.DarkToolStrip();
            butTbSaveAllChanges = new System.Windows.Forms.ToolStripButton();
            butTbUndo = new System.Windows.Forms.ToolStripButton();
            butTbRedo = new System.Windows.Forms.ToolStripButton();
            butTbResetCamera = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            labelAnims = new System.Windows.Forms.ToolStripLabel();
            butTbAddAnimation = new System.Windows.Forms.ToolStripButton();
            butTbImport = new System.Windows.Forms.ToolStripButton();
            butTbDeleteAnimation = new System.Windows.Forms.ToolStripButton();
            butTbCutAnimation = new System.Windows.Forms.ToolStripButton();
            butTbCopyAnimation = new System.Windows.Forms.ToolStripButton();
            butTbPasteAnimation = new System.Windows.Forms.ToolStripButton();
            butTbReplaceAnimation = new System.Windows.Forms.ToolStripButton();
            butTbSplitAnimation = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            labelFrames = new System.Windows.Forms.ToolStripLabel();
            butTbAddFrame = new System.Windows.Forms.ToolStripButton();
            butTbDeleteFrame = new System.Windows.Forms.ToolStripButton();
            butTbCutFrame = new System.Windows.Forms.ToolStripButton();
            butTbCopyFrame = new System.Windows.Forms.ToolStripButton();
            butTbPasteFrame = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            labelRoom = new System.Windows.Forms.ToolStripLabel();
            comboRoomList = new DarkUI.Controls.ToolStripDarkComboBox();
            lstAnimations = new DarkUI.Controls.DarkListView();
            darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            butShowAll = new DarkUI.Controls.DarkButton();
            butDeleteAnimation = new DarkUI.Controls.DarkButton();
            darkButton1 = new DarkUI.Controls.DarkButton();
            butAddNewAnimation = new DarkUI.Controls.DarkButton();
            panelRendering = new Controls.PanelRenderingAnimationEditor();
            darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            panelRootMotion = new DarkUI.Controls.DarkPanel();
            cbRootPosZ = new DarkUI.Controls.DarkCheckBox();
            darkLabel11 = new DarkUI.Controls.DarkLabel();
            cbRootRotation = new DarkUI.Controls.DarkCheckBox();
            cbRootPosX = new DarkUI.Controls.DarkCheckBox();
            cbRootPosY = new DarkUI.Controls.DarkCheckBox();
            dgvBoundingMeshList = new DarkUI.Controls.DarkDataGridView();
            dgvBoundingMeshListCheckboxes = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            dgvBoundingMeshListMeshes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            darkLabel33 = new DarkUI.Controls.DarkLabel();
            darkLabel30 = new DarkUI.Controls.DarkLabel();
            darkLabel34 = new DarkUI.Controls.DarkLabel();
            darkLabel35 = new DarkUI.Controls.DarkLabel();
            darkLabel31 = new DarkUI.Controls.DarkLabel();
            darkLabel32 = new DarkUI.Controls.DarkLabel();
            darkLabel17 = new DarkUI.Controls.DarkLabel();
            darkLabel20 = new DarkUI.Controls.DarkLabel();
            darkLabel14 = new DarkUI.Controls.DarkLabel();
            darkLabel15 = new DarkUI.Controls.DarkLabel();
            nudBBoxMaxY = new DarkUI.Controls.DarkNumericUpDown();
            nudBBoxMaxZ = new DarkUI.Controls.DarkNumericUpDown();
            nudBBoxMaxX = new DarkUI.Controls.DarkNumericUpDown();
            nudBBoxMinY = new DarkUI.Controls.DarkNumericUpDown();
            nudBBoxMinZ = new DarkUI.Controls.DarkNumericUpDown();
            butShrinkBBox = new DarkUI.Controls.DarkButton();
            nudBBoxMinX = new DarkUI.Controls.DarkNumericUpDown();
            butResetBBoxAnim = new DarkUI.Controls.DarkButton();
            butCalcBBoxAnim = new DarkUI.Controls.DarkButton();
            nudGrowY = new DarkUI.Controls.DarkNumericUpDown();
            nudGrowX = new DarkUI.Controls.DarkNumericUpDown();
            butGrowBBox = new DarkUI.Controls.DarkButton();
            nudGrowZ = new DarkUI.Controls.DarkNumericUpDown();
            panelTimeline = new System.Windows.Forms.Panel();
            timeline = new AnimationTrackBar();
            panelTransport = new System.Windows.Forms.Panel();
            darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            butTransportStart = new System.Windows.Forms.ToolStripButton();
            butTransportFrameBack = new System.Windows.Forms.ToolStripButton();
            butTransportPlay = new System.Windows.Forms.ToolStripButton();
            butTransportFrameForward = new System.Windows.Forms.ToolStripButton();
            butTransportEnd = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            butTransportChained = new System.Windows.Forms.ToolStripButton();
            butTransportSound = new System.Windows.Forms.ToolStripButton();
            butTransportCondition = new System.Windows.Forms.ToolStripButton();
            darkSectionPanel4 = new DarkUI.Controls.DarkSectionPanel();
            nudEndHorVel = new DarkUI.Controls.DarkNumericUpDown();
            nudStartHorVel = new DarkUI.Controls.DarkNumericUpDown();
            nudEndVertVel = new DarkUI.Controls.DarkNumericUpDown();
            nudStartVertVel = new DarkUI.Controls.DarkNumericUpDown();
            nudEndFrame = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            nudNextFrame = new DarkUI.Controls.DarkNumericUpDown();
            nudNextAnim = new DarkUI.Controls.DarkNumericUpDown();
            nudFramerate = new DarkUI.Controls.DarkNumericUpDown();
            cmbStateID = new DarkSearchableComboBox();
            darkButton3 = new DarkUI.Controls.DarkButton();
            butEditStateChanges = new DarkUI.Controls.DarkButton();
            panelMain = new System.Windows.Forms.Panel();
            panelView = new DarkUI.Controls.DarkSectionPanel();
            panelRight = new System.Windows.Forms.Panel();
            panelTransform = new DarkUI.Controls.DarkSectionPanel();
            darkLabel8 = new DarkUI.Controls.DarkLabel();
            picTransformPreview = new System.Windows.Forms.PictureBox();
            cmbTransformMode = new DarkUI.Controls.DarkComboBox();
            darkLabel29 = new DarkUI.Controls.DarkLabel();
            darkLabel28 = new DarkUI.Controls.DarkLabel();
            darkLabel21 = new DarkUI.Controls.DarkLabel();
            darkLabel26 = new DarkUI.Controls.DarkLabel();
            nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel27 = new DarkUI.Controls.DarkLabel();
            nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            darkLabel18 = new DarkUI.Controls.DarkLabel();
            nudRotX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel19 = new DarkUI.Controls.DarkLabel();
            nudRotY = new DarkUI.Controls.DarkNumericUpDown();
            nudRotZ = new DarkUI.Controls.DarkNumericUpDown();
            darkSectionPanel5 = new DarkUI.Controls.DarkSectionPanel();
            darkLabel10 = new DarkUI.Controls.DarkLabel();
            darkLabel9 = new DarkUI.Controls.DarkLabel();
            panelLeft = new System.Windows.Forms.Panel();
            sectionBlending = new DarkUI.Controls.DarkSectionPanel();
            bezierCurveEditor = new Controls.BezierCurveEditor();
            darkLabel36 = new DarkUI.Controls.DarkLabel();
            cbBlendPreset = new DarkUI.Controls.DarkComboBox();
            darkLabel13 = new DarkUI.Controls.DarkLabel();
            nudBlendFrameCount = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel12 = new DarkUI.Controls.DarkLabel();
            bcAnimation = new Controls.BezierCurveEditor();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            darkContextMenu1 = new DarkUI.Controls.DarkContextMenu();
            cmTimelineContextMenu = new DarkUI.Controls.DarkContextMenu();
            cmMarkInMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmMarkOutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmSelectAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cnClearSelectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            cmCreateAnimCommandMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmCreateStateChangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            topMenu.SuspendLayout();
            statusStrip.SuspendLayout();
            topBar.SuspendLayout();
            darkSectionPanel1.SuspendLayout();
            darkSectionPanel2.SuspendLayout();
            panelRootMotion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBoundingMeshList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowZ).BeginInit();
            panelTimeline.SuspendLayout();
            panelTransport.SuspendLayout();
            darkToolStrip1.SuspendLayout();
            darkSectionPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudEndHorVel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStartHorVel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudEndVertVel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStartVertVel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudEndFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNextFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNextAnim).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFramerate).BeginInit();
            panelMain.SuspendLayout();
            panelView.SuspendLayout();
            panelRight.SuspendLayout();
            panelTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picTransformPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotZ).BeginInit();
            darkSectionPanel5.SuspendLayout();
            panelLeft.SuspendLayout();
            sectionBlending.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudBlendFrameCount).BeginInit();
            cmTimelineContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // topMenu
            // 
            topMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            topMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            topMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileeToolStripMenuItem, animationToolStripMenuItem, frameToolStripMenuItem, renderingToolStripMenuItem });
            topMenu.Location = new System.Drawing.Point(0, 0);
            topMenu.Name = "topMenu";
            topMenu.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            topMenu.Size = new System.Drawing.Size(1039, 24);
            topMenu.TabIndex = 0;
            topMenu.Text = "darkMenuStrip1";
            // 
            // fileeToolStripMenuItem
            // 
            fileeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            fileeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, saveChangesToolStripMenuItem, toolStripMenuItem3, closeToolStripMenuItem });
            fileeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            fileeToolStripMenuItem.Name = "fileeToolStripMenuItem";
            fileeToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            fileeToolStripMenuItem.Text = "Edit";

            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            undoToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
            undoToolStripMenuItem.Image = Properties.Resources.general_undo_16;
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            undoToolStripMenuItem.Text = "Undo";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            redoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
            redoToolStripMenuItem.Image = Properties.Resources.general_redo_16;
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            redoToolStripMenuItem.Text = "Redo";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // saveChangesToolStripMenuItem
            // 
            saveChangesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            saveChangesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            saveChangesToolStripMenuItem.Image = Properties.Resources.general_Save_16;
            saveChangesToolStripMenuItem.Name = "saveChangesToolStripMenuItem";
            saveChangesToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            saveChangesToolStripMenuItem.Text = "Save changes";
            saveChangesToolStripMenuItem.Click += saveChangesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(142, 6);
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            closeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // animationToolStripMenuItem
            // 
            animationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addNewToolStripMenuItem, deleteToolStripMenuItem, splitAnimationToolStripMenuItem, toolStripMenuItem5, curToolStripMenuItem, copyToolStripMenuItem1, pasteToolStripMenuItem1, toolStripSeparator3, importToolStripMenuItem, exportToolStripMenuItem, batchToolStripMenuItem, toolStripMenuItem7, resampleAnimationToolStripMenuItem, resampleAnimationToKeyframesToolStripMenuItem, mirrorAnimationToolStripMenuItem, reverseAnimationToolStripMenuItem, toolStripMenuItem2, fixToolStripMenuItem, findReplaceAnimcommandsToolStripMenuItem });
            animationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            animationToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            animationToolStripMenuItem.Text = "Animations";
            // 
            // addNewToolStripMenuItem
            // 
            addNewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            addNewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            addNewToolStripMenuItem.Image = Properties.Resources.general_plus_math_16;
            addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            addNewToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            addNewToolStripMenuItem.Text = "New animation";
            addNewToolStripMenuItem.Click += addNewAnimationToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            deleteToolStripMenuItem.Image = Properties.Resources.trash_16;
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            deleteToolStripMenuItem.Text = "Delete animation";
            deleteToolStripMenuItem.Click += deleteAnimationToolStripMenuItem_Click;
            // 
            // splitAnimationToolStripMenuItem
            // 
            splitAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            splitAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            splitAnimationToolStripMenuItem.Image = Properties.Resources.split_16;
            splitAnimationToolStripMenuItem.Name = "splitAnimationToolStripMenuItem";
            splitAnimationToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            splitAnimationToolStripMenuItem.Text = "Split animation";
            splitAnimationToolStripMenuItem.Click += splitAnimationToolStripMenuItem_Click;
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new System.Drawing.Size(247, 6);
            // 
            // curToolStripMenuItem
            // 
            curToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            curToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            curToolStripMenuItem.Image = Properties.Resources.actions_cut_16;
            curToolStripMenuItem.Name = "curToolStripMenuItem";
            curToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            curToolStripMenuItem.Text = "Cut";
            curToolStripMenuItem.Click += cutAnimationToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem1
            // 
            copyToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            copyToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            copyToolStripMenuItem1.Image = Properties.Resources.copy_16;
            copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            copyToolStripMenuItem1.Size = new System.Drawing.Size(250, 22);
            copyToolStripMenuItem1.Text = "Copy";
            copyToolStripMenuItem1.Click += copyAnimationToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem1
            // 
            pasteToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            pasteToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            pasteToolStripMenuItem1.Image = Properties.Resources.general_paste_16;
            pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            pasteToolStripMenuItem1.Size = new System.Drawing.Size(250, 22);
            pasteToolStripMenuItem1.Text = "Paste";
            pasteToolStripMenuItem1.Click += pasteAnimationToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(247, 6);
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            importToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            importToolStripMenuItem.Image = Properties.Resources.general_Import_16;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            importToolStripMenuItem.Text = "Import...";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            exportToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            exportToolStripMenuItem.Image = Properties.Resources.general_Export_16;
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            exportToolStripMenuItem.Text = "Export...";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click;
            // 
            // batchToolStripMenuItem
            // 
            batchToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            batchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { importToolStripMenuItem1, exportSelectedToolStripMenuItem, exportAllToolStripMenuItem });
            batchToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            batchToolStripMenuItem.Name = "batchToolStripMenuItem";
            batchToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            batchToolStripMenuItem.Text = "Batch";
            // 
            // importToolStripMenuItem1
            // 
            importToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            importToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            importToolStripMenuItem1.Name = "importToolStripMenuItem1";
            importToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            importToolStripMenuItem1.Text = "Import...";
            importToolStripMenuItem1.Click += importToolStripMenuItem1_Click;
            // 
            // exportSelectedToolStripMenuItem
            // 
            exportSelectedToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            exportSelectedToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
            exportSelectedToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            exportSelectedToolStripMenuItem.Text = "Export selected...";
            exportSelectedToolStripMenuItem.Click += exportSelectedToolStripMenuItem_Click;
            // 
            // exportAllToolStripMenuItem
            // 
            exportAllToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            exportAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            exportAllToolStripMenuItem.Text = "Export all...";
            exportAllToolStripMenuItem.Click += exportAllToolStripMenuItem_Click_1;
            // 
            // toolStripMenuItem7
            // 
            toolStripMenuItem7.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new System.Drawing.Size(247, 6);
            // 
            // resampleAnimationToolStripMenuItem
            // 
            resampleAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            resampleAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            resampleAnimationToolStripMenuItem.Image = Properties.Resources.actions_interpolate_16;
            resampleAnimationToolStripMenuItem.Name = "resampleAnimationToolStripMenuItem";
            resampleAnimationToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            resampleAnimationToolStripMenuItem.Text = "Resample animation";
            resampleAnimationToolStripMenuItem.Click += resampleAnimationToolStripMenuItem_Click;
            // 
            // resampleAnimationToKeyframesToolStripMenuItem
            // 
            resampleAnimationToKeyframesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            resampleAnimationToKeyframesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            resampleAnimationToKeyframesToolStripMenuItem.Name = "resampleAnimationToKeyframesToolStripMenuItem";
            resampleAnimationToKeyframesToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            resampleAnimationToKeyframesToolStripMenuItem.Text = "Resample animation to framerate";
            resampleAnimationToKeyframesToolStripMenuItem.Click += resampleAnimationToKeyframesToolStripMenuItem_Click;
            // 
            // mirrorAnimationToolStripMenuItem
            // 
            mirrorAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mirrorAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mirrorAnimationToolStripMenuItem.Image = Properties.Resources.general_Mirror;
            mirrorAnimationToolStripMenuItem.Name = "mirrorAnimationToolStripMenuItem";
            mirrorAnimationToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            mirrorAnimationToolStripMenuItem.Text = "Mirror animation";
            mirrorAnimationToolStripMenuItem.Click += mirrorAnimationToolStripMenuItem_Click;
            // 
            // reverseAnimationToolStripMenuItem
            // 
            reverseAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            reverseAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            reverseAnimationToolStripMenuItem.Name = "reverseAnimationToolStripMenuItem";
            reverseAnimationToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            reverseAnimationToolStripMenuItem.Text = "Reverse animation";
            reverseAnimationToolStripMenuItem.Click += reverseAnimationToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(247, 6);
            // 
            // fixToolStripMenuItem
            // 
            fixToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            fixToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { currentAnimationToolStripMenuItem, selectedAnimationsToolStripMenuItem, allAnimationsToolStripMenuItem });
            fixToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            fixToolStripMenuItem.Name = "fixToolStripMenuItem";
            fixToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            fixToolStripMenuItem.Text = "Fix";
            // 
            // currentAnimationToolStripMenuItem
            // 
            currentAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            currentAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            currentAnimationToolStripMenuItem.Name = "currentAnimationToolStripMenuItem";
            currentAnimationToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            currentAnimationToolStripMenuItem.Text = "Current animation...";
            currentAnimationToolStripMenuItem.Click += currentAnimationToolStripMenuItem_Click;
            // 
            // selectedAnimationsToolStripMenuItem
            // 
            selectedAnimationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            selectedAnimationsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            selectedAnimationsToolStripMenuItem.Name = "selectedAnimationsToolStripMenuItem";
            selectedAnimationsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            selectedAnimationsToolStripMenuItem.Text = "Selected animations...";
            selectedAnimationsToolStripMenuItem.Click += selectedAnimationsToolStripMenuItem_Click;
            // 
            // allAnimationsToolStripMenuItem
            // 
            allAnimationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            allAnimationsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            allAnimationsToolStripMenuItem.Name = "allAnimationsToolStripMenuItem";
            allAnimationsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            allAnimationsToolStripMenuItem.Text = "All animations...";
            allAnimationsToolStripMenuItem.Click += allAnimationsToolStripMenuItem_Click;
            // 
            // findReplaceAnimcommandsToolStripMenuItem
            // 
            findReplaceAnimcommandsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            findReplaceAnimcommandsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            findReplaceAnimcommandsToolStripMenuItem.Image = Properties.Resources.general_Find_and_replace_16;
            findReplaceAnimcommandsToolStripMenuItem.Name = "findReplaceAnimcommandsToolStripMenuItem";
            findReplaceAnimcommandsToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            findReplaceAnimcommandsToolStripMenuItem.Text = "Find && replace animcommands...";
            findReplaceAnimcommandsToolStripMenuItem.Click += findReplaceAnimcommandsToolStripMenuItem_Click;
            // 
            // frameToolStripMenuItem
            // 
            frameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            frameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { insertFrameAfterCurrentOneToolStripMenuItem, insertnFramesAfterCurrentOneToolStripMenuItem, deleteFrameToolStripMenuItem, deleteEveryNthFrameToolStripMenuItem, toolStripMenuItem4, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, toolStripMenuItem1, interpolateFramesToolStripMenuItem, toolStripMenuItem6, calculateCollisionBoxForCurrentFrameToolStripMenuItem, deleteCollisionBoxForCurrentFrameToolStripMenuItem });
            frameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            frameToolStripMenuItem.Name = "frameToolStripMenuItem";
            frameToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            frameToolStripMenuItem.Text = "Frames";
            // 
            // insertFrameAfterCurrentOneToolStripMenuItem
            // 
            insertFrameAfterCurrentOneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            insertFrameAfterCurrentOneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            insertFrameAfterCurrentOneToolStripMenuItem.Image = Properties.Resources.general_plus_math_16;
            insertFrameAfterCurrentOneToolStripMenuItem.Name = "insertFrameAfterCurrentOneToolStripMenuItem";
            insertFrameAfterCurrentOneToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            insertFrameAfterCurrentOneToolStripMenuItem.Text = "Insert frame after current one";
            insertFrameAfterCurrentOneToolStripMenuItem.Click += insertFrameAfterCurrentOneToolStripMenuItem_Click;
            // 
            // insertnFramesAfterCurrentOneToolStripMenuItem
            // 
            insertnFramesAfterCurrentOneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            insertnFramesAfterCurrentOneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            insertnFramesAfterCurrentOneToolStripMenuItem.Name = "insertnFramesAfterCurrentOneToolStripMenuItem";
            insertnFramesAfterCurrentOneToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            insertnFramesAfterCurrentOneToolStripMenuItem.Text = "Insert (n) frames after current one";
            insertnFramesAfterCurrentOneToolStripMenuItem.Click += insertFramesAfterCurrentToolStripMenuItem_Click;
            // 
            // deleteFrameToolStripMenuItem
            // 
            deleteFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            deleteFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            deleteFrameToolStripMenuItem.Image = Properties.Resources.trash_16;
            deleteFrameToolStripMenuItem.Name = "deleteFrameToolStripMenuItem";
            deleteFrameToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            deleteFrameToolStripMenuItem.Text = "Delete frames";
            deleteFrameToolStripMenuItem.Click += deleteFramesToolStripMenuItem_Click;
            // 
            // deleteEveryNthFrameToolStripMenuItem
            // 
            deleteEveryNthFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            deleteEveryNthFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            deleteEveryNthFrameToolStripMenuItem.Name = "deleteEveryNthFrameToolStripMenuItem";
            deleteEveryNthFrameToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            deleteEveryNthFrameToolStripMenuItem.Text = "Delete every (n)th frame";
            deleteEveryNthFrameToolStripMenuItem.Click += DeleteEveryNthFrameToolStripMenuItem_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(248, 6);
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cutToolStripMenuItem.Image = Properties.Resources.actions_cut_16;
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            cutToolStripMenuItem.Text = "Cut";
            cutToolStripMenuItem.Click += cutFramesToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            copyToolStripMenuItem.Image = Properties.Resources.copy_16;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyFramesToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            pasteToolStripMenuItem.Image = Properties.Resources.general_paste_16;
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += pasteFramesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(248, 6);
            // 
            // interpolateFramesToolStripMenuItem
            // 
            interpolateFramesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            interpolateFramesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            interpolateFramesToolStripMenuItem.Image = Properties.Resources.actions_interpolate_16;
            interpolateFramesToolStripMenuItem.Name = "interpolateFramesToolStripMenuItem";
            interpolateFramesToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            interpolateFramesToolStripMenuItem.Text = "Interpolate frames";
            interpolateFramesToolStripMenuItem.Click += interpolateFramesToolStripMenuItem_Click;
            // 
            // toolStripMenuItem6
            // 
            toolStripMenuItem6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripMenuItem6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new System.Drawing.Size(248, 6);
            // 
            // calculateCollisionBoxForCurrentFrameToolStripMenuItem
            // 
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.Image = Properties.Resources.general_box_16;
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.Name = "calculateCollisionBoxForCurrentFrameToolStripMenuItem";
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.Text = "Calculate bounding box";
            calculateCollisionBoxForCurrentFrameToolStripMenuItem.Click += calculateBoundingBoxForCurrentFrameToolStripMenuItem_Click;
            // 
            // deleteCollisionBoxForCurrentFrameToolStripMenuItem
            // 
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.Name = "deleteCollisionBoxForCurrentFrameToolStripMenuItem";
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.Text = "Delete bounding box";
            deleteCollisionBoxForCurrentFrameToolStripMenuItem.Click += deleteCollisionBoxForCurrentFrameToolStripMenuItem_Click;
            // 
            // renderingToolStripMenuItem
            // 
            renderingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            renderingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { drawGizmoToolStripMenuItem, drawGridToolStripMenuItem, drawCollisionBoxToolStripMenuItem, toolStripSeparator9, smoothAnimationsToolStripMenuItem, scrollGridToolStripMenuItem, restoreGridHeightToolStripMenuItem });
            renderingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            renderingToolStripMenuItem.Name = "renderingToolStripMenuItem";
            renderingToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            renderingToolStripMenuItem.Text = "Rendering";
            // 
            // drawGizmoToolStripMenuItem
            // 
            drawGizmoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            drawGizmoToolStripMenuItem.Checked = true;
            drawGizmoToolStripMenuItem.CheckOnClick = true;
            drawGizmoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            drawGizmoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            drawGizmoToolStripMenuItem.Name = "drawGizmoToolStripMenuItem";
            drawGizmoToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            drawGizmoToolStripMenuItem.Text = "Draw gizmo";
            drawGizmoToolStripMenuItem.Click += drawGizmoToolStripMenuItem_Click;
            // 
            // drawGridToolStripMenuItem
            // 
            drawGridToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            drawGridToolStripMenuItem.Checked = true;
            drawGridToolStripMenuItem.CheckOnClick = true;
            drawGridToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            drawGridToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            drawGridToolStripMenuItem.Name = "drawGridToolStripMenuItem";
            drawGridToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            drawGridToolStripMenuItem.Text = "Draw grid";
            drawGridToolStripMenuItem.Click += drawGridToolStripMenuItem_Click;
            // 
            // drawCollisionBoxToolStripMenuItem
            // 
            drawCollisionBoxToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            drawCollisionBoxToolStripMenuItem.Checked = true;
            drawCollisionBoxToolStripMenuItem.CheckOnClick = true;
            drawCollisionBoxToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            drawCollisionBoxToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            drawCollisionBoxToolStripMenuItem.Name = "drawCollisionBoxToolStripMenuItem";
            drawCollisionBoxToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            drawCollisionBoxToolStripMenuItem.Text = "Draw collision box";
            drawCollisionBoxToolStripMenuItem.Click += drawCollisionBoxToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator9.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(171, 6);
            // 
            // smoothAnimationsToolStripMenuItem
            // 
            smoothAnimationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            smoothAnimationsToolStripMenuItem.Checked = true;
            smoothAnimationsToolStripMenuItem.CheckOnClick = true;
            smoothAnimationsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            smoothAnimationsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            smoothAnimationsToolStripMenuItem.Name = "smoothAnimationsToolStripMenuItem";
            smoothAnimationsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            smoothAnimationsToolStripMenuItem.Text = "Smooth animation";
            smoothAnimationsToolStripMenuItem.Click += smoothAnimationsToolStripMenuItem_Click;
            // 
            // scrollGridToolStripMenuItem
            // 
            scrollGridToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            scrollGridToolStripMenuItem.Checked = true;
            scrollGridToolStripMenuItem.CheckOnClick = true;
            scrollGridToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            scrollGridToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            scrollGridToolStripMenuItem.Name = "scrollGridToolStripMenuItem";
            scrollGridToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            scrollGridToolStripMenuItem.Text = "Scroll grid";
            scrollGridToolStripMenuItem.Click += scrollGridToolStripMenuItem_Click;
            // 
            // restoreGridHeightToolStripMenuItem
            // 
            restoreGridHeightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            restoreGridHeightToolStripMenuItem.CheckOnClick = true;
            restoreGridHeightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            restoreGridHeightToolStripMenuItem.Name = "restoreGridHeightToolStripMenuItem";
            restoreGridHeightToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            restoreGridHeightToolStripMenuItem.Text = "Restore grid height";
            restoreGridHeightToolStripMenuItem.Click += restoreGridHeightToolStripMenuItem_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            statusStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusFrame });
            statusStrip.Location = new System.Drawing.Point(0, 770);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            statusStrip.Size = new System.Drawing.Size(1039, 25);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "darkStatusStrip1";
            // 
            // statusFrame
            // 
            statusFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            statusFrame.Margin = new System.Windows.Forms.Padding(0, 1, 0, 2);
            statusFrame.Name = "statusFrame";
            statusFrame.Size = new System.Drawing.Size(43, 14);
            statusFrame.Text = "Frame:";
            statusFrame.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // darkLabel22
            // 
            darkLabel22.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel22.Location = new System.Drawing.Point(207, 125);
            darkLabel22.Name = "darkLabel22";
            darkLabel22.Size = new System.Drawing.Size(60, 13);
            darkLabel22.TabIndex = 120;
            darkLabel22.Text = "End Z vel";
            // 
            // darkLabel23
            // 
            darkLabel23.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel23.Location = new System.Drawing.Point(2, 125);
            darkLabel23.Name = "darkLabel23";
            darkLabel23.Size = new System.Drawing.Size(64, 13);
            darkLabel23.TabIndex = 114;
            darkLabel23.Text = "Start X vel";
            // 
            // darkLabel24
            // 
            darkLabel24.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel24.Location = new System.Drawing.Point(139, 125);
            darkLabel24.Name = "darkLabel24";
            darkLabel24.Size = new System.Drawing.Size(65, 13);
            darkLabel24.TabIndex = 118;
            darkLabel24.Text = "Start Z vel";
            // 
            // darkLabel25
            // 
            darkLabel25.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel25.Location = new System.Drawing.Point(69, 125);
            darkLabel25.Name = "darkLabel25";
            darkLabel25.Size = new System.Drawing.Size(61, 13);
            darkLabel25.TabIndex = 116;
            darkLabel25.Text = "End X vel";
            // 
            // tbStateId
            // 
            tbStateId.Location = new System.Drawing.Point(44, 56);
            tbStateId.Name = "tbStateId";
            tbStateId.Size = new System.Drawing.Size(193, 22);
            tbStateId.TabIndex = 7;
            tbStateId.KeyDown += tbStateId_KeyDown;
            tbStateId.Validated += tbStateId_Validated;
            // 
            // darkLabel7
            // 
            darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel7.Location = new System.Drawing.Point(2, 58);
            darkLabel7.Name = "darkLabel7";
            darkLabel7.Size = new System.Drawing.Size(38, 13);
            darkLabel7.TabIndex = 102;
            darkLabel7.Text = "State:";
            // 
            // darkLabel6
            // 
            darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel6.Location = new System.Drawing.Point(207, 82);
            darkLabel6.Name = "darkLabel6";
            darkLabel6.Size = new System.Drawing.Size(65, 13);
            darkLabel6.TabIndex = 100;
            darkLabel6.Text = "Next frame";
            // 
            // darkLabel5
            // 
            darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel5.Location = new System.Drawing.Point(139, 82);
            darkLabel5.Name = "darkLabel5";
            darkLabel5.Size = new System.Drawing.Size(64, 13);
            darkLabel5.TabIndex = 98;
            darkLabel5.Text = "Next anim";
            // 
            // darkLabel4
            // 
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel4.Location = new System.Drawing.Point(1, 82);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(64, 13);
            darkLabel4.TabIndex = 96;
            darkLabel4.Text = "Framerate";
            // 
            // tbName
            // 
            tbName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(44, 28);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(231, 22);
            tbName.TabIndex = 6;
            tbName.TextChanged += tbName_TextChanged;
            tbName.KeyDown += tbName_KeyDown;
            tbName.Validated += animParameter_Validated;
            // 
            // darkLabel3
            // 
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(2, 30);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(41, 20);
            darkLabel3.TabIndex = 94;
            darkLabel3.Text = "Name:";
            // 
            // tbSearchAnimation
            // 
            tbSearchAnimation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbSearchAnimation.Location = new System.Drawing.Point(4, 28);
            tbSearchAnimation.Name = "tbSearchAnimation";
            tbSearchAnimation.Size = new System.Drawing.Size(221, 22);
            tbSearchAnimation.TabIndex = 0;
            toolTip1.SetToolTip(tbSearchAnimation, "Numerical input - filter by state ID.\r\nString input - filter by animation name.\r\n\r\nTokens:\r\ns:[name or ID] - state name or ID\r\na:[name or ID] - anim name or ID");
            tbSearchAnimation.KeyDown += tbSearchByStateID_KeyDown;
            // 
            // topBar
            // 
            topBar.AutoSize = false;
            topBar.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            topBar.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            topBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            topBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { butTbSaveAllChanges, butTbUndo, butTbRedo, butTbResetCamera, toolStripSeparator1, labelAnims, butTbAddAnimation, butTbImport, butTbDeleteAnimation, butTbCutAnimation, butTbCopyAnimation, butTbPasteAnimation, butTbReplaceAnimation, butTbSplitAnimation, toolStripSeparator2, labelFrames, butTbAddFrame, butTbDeleteFrame, butTbCutFrame, butTbCopyFrame, butTbPasteFrame, toolStripSeparator4, labelRoom, comboRoomList });
            topBar.Location = new System.Drawing.Point(0, 24);
            topBar.Name = "topBar";
            topBar.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            topBar.Size = new System.Drawing.Size(1039, 28);
            topBar.TabIndex = 6;
            topBar.Text = "darkToolStrip1";
            // 
            // butTbSaveAllChanges
            // 
            butTbSaveAllChanges.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbSaveAllChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbSaveAllChanges.Enabled = false;
            butTbSaveAllChanges.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbSaveAllChanges.Image = Properties.Resources.general_Save_16;
            butTbSaveAllChanges.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbSaveAllChanges.Name = "butTbSaveAllChanges";
            butTbSaveAllChanges.Size = new System.Drawing.Size(23, 25);
            butTbSaveAllChanges.Click += butTbSaveChanges_Click;
            // 
            // butTbUndo
            // 
            butTbUndo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbUndo.Enabled = false;
            butTbUndo.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbUndo.Image = Properties.Resources.general_undo_16;
            butTbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbUndo.Name = "butTbUndo";
            butTbUndo.Size = new System.Drawing.Size(23, 25);
            butTbUndo.ToolTipText = "Undo";
            butTbUndo.Click += butTbUndo_Click;
            // 
            // butTbRedo
            // 
            butTbRedo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbRedo.Enabled = false;
            butTbRedo.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbRedo.Image = Properties.Resources.general_redo_16;
            butTbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbRedo.Name = "butTbRedo";
            butTbRedo.Size = new System.Drawing.Size(23, 25);
            butTbRedo.ToolTipText = "Redo";
            butTbRedo.Click += butTbRedo_Click;
            // 
            // butTbResetCamera
            // 
            butTbResetCamera.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbResetCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbResetCamera.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbResetCamera.Image = Properties.Resources.general_target_16;
            butTbResetCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbResetCamera.Name = "butTbResetCamera";
            butTbResetCamera.Size = new System.Drawing.Size(23, 25);
            butTbResetCamera.ToolTipText = "Reset camera";
            butTbResetCamera.Click += butTbResetCamera_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // labelAnims
            // 
            labelAnims.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            labelAnims.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelAnims.Name = "labelAnims";
            labelAnims.Size = new System.Drawing.Size(44, 25);
            labelAnims.Text = "Anims:";
            // 
            // butTbAddAnimation
            // 
            butTbAddAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbAddAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbAddAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbAddAnimation.Image = Properties.Resources.general_plus_math_16;
            butTbAddAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbAddAnimation.Name = "butTbAddAnimation";
            butTbAddAnimation.Size = new System.Drawing.Size(23, 25);
            butTbAddAnimation.Text = "toolStripButton2";
            butTbAddAnimation.ToolTipText = "Add animation";
            butTbAddAnimation.Click += butTbAddAnimation_Click;
            // 
            // butTbImport
            // 
            butTbImport.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbImport.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbImport.Image = Properties.Resources.general_Import_16;
            butTbImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbImport.Name = "butTbImport";
            butTbImport.Size = new System.Drawing.Size(23, 25);
            butTbImport.ToolTipText = "Import...";
            butTbImport.Click += butTbImport_Click;
            // 
            // butTbDeleteAnimation
            // 
            butTbDeleteAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbDeleteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbDeleteAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbDeleteAnimation.Image = Properties.Resources.trash_16;
            butTbDeleteAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbDeleteAnimation.Name = "butTbDeleteAnimation";
            butTbDeleteAnimation.Size = new System.Drawing.Size(23, 25);
            butTbDeleteAnimation.Text = "toolStripButton3";
            butTbDeleteAnimation.ToolTipText = "Delete animation";
            butTbDeleteAnimation.Click += butTbDeleteAnimation_Click;
            // 
            // butTbCutAnimation
            // 
            butTbCutAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbCutAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbCutAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbCutAnimation.Image = Properties.Resources.actions_cut_16;
            butTbCutAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbCutAnimation.Name = "butTbCutAnimation";
            butTbCutAnimation.Size = new System.Drawing.Size(23, 25);
            butTbCutAnimation.Text = "toolStripButton8";
            butTbCutAnimation.ToolTipText = "Cut animation";
            butTbCutAnimation.Click += butTbCutAnimation_Click;
            // 
            // butTbCopyAnimation
            // 
            butTbCopyAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbCopyAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbCopyAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbCopyAnimation.Image = Properties.Resources.copy_16;
            butTbCopyAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbCopyAnimation.Name = "butTbCopyAnimation";
            butTbCopyAnimation.Size = new System.Drawing.Size(23, 25);
            butTbCopyAnimation.Text = "toolStripButton4";
            butTbCopyAnimation.ToolTipText = "Copy animation";
            butTbCopyAnimation.Click += butTbCopyAnimation_Click;
            // 
            // butTbPasteAnimation
            // 
            butTbPasteAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbPasteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbPasteAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbPasteAnimation.Image = Properties.Resources.general_paste_16;
            butTbPasteAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbPasteAnimation.Name = "butTbPasteAnimation";
            butTbPasteAnimation.Size = new System.Drawing.Size(23, 25);
            butTbPasteAnimation.Text = "toolStripButton5";
            butTbPasteAnimation.ToolTipText = "Paste animation";
            butTbPasteAnimation.Click += butTbPasteAnimation_Click;
            // 
            // butTbReplaceAnimation
            // 
            butTbReplaceAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbReplaceAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbReplaceAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbReplaceAnimation.Image = Properties.Resources.general_paste_new_16;
            butTbReplaceAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbReplaceAnimation.Name = "butTbReplaceAnimation";
            butTbReplaceAnimation.Size = new System.Drawing.Size(23, 25);
            butTbReplaceAnimation.Text = "toolStripButton5";
            butTbReplaceAnimation.ToolTipText = "Replace animation";
            butTbReplaceAnimation.Click += butTbReplaceAnimation_Click;
            // 
            // butTbSplitAnimation
            // 
            butTbSplitAnimation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbSplitAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbSplitAnimation.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbSplitAnimation.Image = Properties.Resources.split_16;
            butTbSplitAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbSplitAnimation.Name = "butTbSplitAnimation";
            butTbSplitAnimation.Size = new System.Drawing.Size(23, 25);
            butTbSplitAnimation.Text = "toolStripButton5";
            butTbSplitAnimation.ToolTipText = "Split animation";
            butTbSplitAnimation.Click += butTbSplitAnimation_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // labelFrames
            // 
            labelFrames.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            labelFrames.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelFrames.Name = "labelFrames";
            labelFrames.Size = new System.Drawing.Size(48, 25);
            labelFrames.Text = "Frames:";
            // 
            // butTbAddFrame
            // 
            butTbAddFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbAddFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbAddFrame.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbAddFrame.Image = Properties.Resources.general_plus_math_16;
            butTbAddFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbAddFrame.Name = "butTbAddFrame";
            butTbAddFrame.Size = new System.Drawing.Size(23, 25);
            butTbAddFrame.Text = "toolStripButton2";
            butTbAddFrame.ToolTipText = "Add frame";
            butTbAddFrame.Click += butTbAddFrame_Click;
            // 
            // butTbDeleteFrame
            // 
            butTbDeleteFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbDeleteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbDeleteFrame.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbDeleteFrame.Image = Properties.Resources.trash_16;
            butTbDeleteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbDeleteFrame.Name = "butTbDeleteFrame";
            butTbDeleteFrame.Size = new System.Drawing.Size(23, 25);
            butTbDeleteFrame.Text = "toolStripButton3";
            butTbDeleteFrame.ToolTipText = "Delete frames";
            butTbDeleteFrame.Click += butTbDeleteFrame_Click;
            // 
            // butTbCutFrame
            // 
            butTbCutFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbCutFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbCutFrame.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbCutFrame.Image = Properties.Resources.actions_cut_16;
            butTbCutFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbCutFrame.Name = "butTbCutFrame";
            butTbCutFrame.Size = new System.Drawing.Size(23, 25);
            butTbCutFrame.Text = "toolStripButton8";
            butTbCutFrame.ToolTipText = "Cut frames";
            butTbCutFrame.Click += butTbCutFrame_Click;
            // 
            // butTbCopyFrame
            // 
            butTbCopyFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbCopyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbCopyFrame.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbCopyFrame.Image = Properties.Resources.copy_16;
            butTbCopyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbCopyFrame.Name = "butTbCopyFrame";
            butTbCopyFrame.Size = new System.Drawing.Size(23, 25);
            butTbCopyFrame.Text = "toolStripButton4";
            butTbCopyFrame.ToolTipText = "Copy frames";
            butTbCopyFrame.Click += butTbCopyFrame_Click;
            // 
            // butTbPasteFrame
            // 
            butTbPasteFrame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbPasteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbPasteFrame.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbPasteFrame.Image = Properties.Resources.general_paste_16;
            butTbPasteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbPasteFrame.Name = "butTbPasteFrame";
            butTbPasteFrame.Size = new System.Drawing.Size(23, 25);
            butTbPasteFrame.Text = "toolStripButton5";
            butTbPasteFrame.ToolTipText = "Paste frames";
            butTbPasteFrame.Click += butTbPasteFrame_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            toolStripSeparator4.Visible = false;
            // 
            // labelRoom
            // 
            labelRoom.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            labelRoom.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelRoom.Name = "labelRoom";
            labelRoom.Size = new System.Drawing.Size(42, 25);
            labelRoom.Text = "Room:";
            labelRoom.Visible = false;
            // 
            // comboRoomList
            // 
            comboRoomList.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            comboRoomList.Enabled = false;
            comboRoomList.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            comboRoomList.Name = "comboRoomList";
            comboRoomList.SelectedIndex = -1;
            comboRoomList.Size = new System.Drawing.Size(121, 25);
            comboRoomList.Visible = false;
            comboRoomList.SelectedIndexChanged += comboRoomList_SelectedIndexChanged;
            // 
            // lstAnimations
            // 
            lstAnimations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstAnimations.Location = new System.Drawing.Point(4, 53);
            lstAnimations.MouseWheelScrollSpeedV = 0.2F;
            lstAnimations.MultiSelect = true;
            lstAnimations.Name = "lstAnimations";
            lstAnimations.Size = new System.Drawing.Size(271, 187);
            lstAnimations.TabIndex = 3;
            lstAnimations.SelectedIndicesChanged += lstAnimations_SelectedIndicesChanged;
            lstAnimations.Click += lstAnimations_Click;
            // 
            // darkSectionPanel1
            // 
            darkSectionPanel1.Controls.Add(lstAnimations);
            darkSectionPanel1.Controls.Add(butShowAll);
            darkSectionPanel1.Controls.Add(tbSearchAnimation);
            darkSectionPanel1.Controls.Add(butDeleteAnimation);
            darkSectionPanel1.Controls.Add(darkButton1);
            darkSectionPanel1.Controls.Add(butAddNewAnimation);
            darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            darkSectionPanel1.Location = new System.Drawing.Point(0, 0);
            darkSectionPanel1.MaximumSize = new System.Drawing.Size(280, 10000);
            darkSectionPanel1.MinimumSize = new System.Drawing.Size(280, 120);
            darkSectionPanel1.Name = "darkSectionPanel1";
            darkSectionPanel1.SectionHeader = "Animation List";
            darkSectionPanel1.Size = new System.Drawing.Size(280, 272);
            darkSectionPanel1.TabIndex = 9;
            // 
            // butShowAll
            // 
            butShowAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butShowAll.Checked = false;
            butShowAll.Image = Properties.Resources.actions_delete_16;
            butShowAll.Location = new System.Drawing.Point(253, 28);
            butShowAll.Name = "butShowAll";
            butShowAll.Size = new System.Drawing.Size(22, 22);
            butShowAll.TabIndex = 2;
            toolTip1.SetToolTip(butShowAll, "Reset filtering");
            butShowAll.Click += butShowAll_Click;
            // 
            // butDeleteAnimation
            // 
            butDeleteAnimation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butDeleteAnimation.Checked = false;
            butDeleteAnimation.Image = (System.Drawing.Image)resources.GetObject("butDeleteAnimation.Image");
            butDeleteAnimation.Location = new System.Drawing.Point(252, 244);
            butDeleteAnimation.Name = "butDeleteAnimation";
            butDeleteAnimation.Size = new System.Drawing.Size(23, 24);
            butDeleteAnimation.TabIndex = 5;
            toolTip1.SetToolTip(butDeleteAnimation, "Delete selected animations");
            butDeleteAnimation.Click += butDeleteAnimation_Click;
            // 
            // darkButton1
            // 
            darkButton1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkButton1.Checked = false;
            darkButton1.Image = Properties.Resources.general_filter_16;
            darkButton1.Location = new System.Drawing.Point(228, 28);
            darkButton1.Name = "darkButton1";
            darkButton1.Size = new System.Drawing.Size(22, 22);
            darkButton1.TabIndex = 1;
            toolTip1.SetToolTip(darkButton1, "Filter list.\r\nNumerical input - filter by state ID\r\nString input - filter by name");
            darkButton1.Click += butSearchByStateID_Click;
            // 
            // butAddNewAnimation
            // 
            butAddNewAnimation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butAddNewAnimation.Checked = false;
            butAddNewAnimation.Image = (System.Drawing.Image)resources.GetObject("butAddNewAnimation.Image");
            butAddNewAnimation.Location = new System.Drawing.Point(225, 244);
            butAddNewAnimation.Name = "butAddNewAnimation";
            butAddNewAnimation.Size = new System.Drawing.Size(23, 24);
            butAddNewAnimation.TabIndex = 4;
            toolTip1.SetToolTip(butAddNewAnimation, "Add new animation");
            butAddNewAnimation.Click += butAddNewAnimation_Click;
            // 
            // panelRendering
            // 
            panelRendering.AllowRendering = true;
            panelRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRendering.Location = new System.Drawing.Point(1, 1);
            panelRendering.Name = "panelRendering";
            panelRendering.Size = new System.Drawing.Size(469, 670);
            panelRendering.TabIndex = 9;
            panelRendering.MouseDoubleClick += panelRendering_MouseDoubleClick;
            panelRendering.MouseEnter += panelRendering_MouseEnter;
            panelRendering.MouseMove += panelRendering_MouseMove;
            // 
            // darkSectionPanel2
            // 
            darkSectionPanel2.Controls.Add(dgvBoundingMeshList);
            darkSectionPanel2.Controls.Add(panelRootMotion);
            darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            darkSectionPanel2.Location = new System.Drawing.Point(0, 0);
            darkSectionPanel2.Name = "darkSectionPanel2";
            darkSectionPanel2.SectionHeader = "Skeleton";
            darkSectionPanel2.Size = new System.Drawing.Size(280, 386);
            darkSectionPanel2.TabIndex = 6;
            // 
            // panelRootMotion
            // 
            panelRootMotion.Controls.Add(cbRootPosZ);
            panelRootMotion.Controls.Add(darkLabel11);
            panelRootMotion.Controls.Add(cbRootRotation);
            panelRootMotion.Controls.Add(cbRootPosX);
            panelRootMotion.Controls.Add(cbRootPosY);
            panelRootMotion.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelRootMotion.Location = new System.Drawing.Point(1, 353);
            panelRootMotion.Name = "panelRootMotion";
            panelRootMotion.Size = new System.Drawing.Size(278, 32);
            panelRootMotion.TabIndex = 26;
            // 
            // cbRootPosZ
            // 
            cbRootPosZ.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbRootPosZ.AutoSize = true;
            cbRootPosZ.Location = new System.Drawing.Point(168, 9);
            cbRootPosZ.Name = "cbRootPosZ";
            cbRootPosZ.Size = new System.Drawing.Size(32, 17);
            cbRootPosZ.TabIndex = 101;
            cbRootPosZ.Text = "Z";
            // 
            // darkLabel11
            // 
            darkLabel11.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel11.Location = new System.Drawing.Point(4, 10);
            darkLabel11.Name = "darkLabel11";
            darkLabel11.Size = new System.Drawing.Size(76, 13);
            darkLabel11.TabIndex = 106;
            darkLabel11.Text = "Root motion:";
            // 
            // cbRootRotation
            // 
            cbRootRotation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbRootRotation.AutoSize = true;
            cbRootRotation.Location = new System.Drawing.Point(206, 9);
            cbRootRotation.Name = "cbRootRotation";
            cbRootRotation.Size = new System.Drawing.Size(71, 17);
            cbRootRotation.TabIndex = 102;
            cbRootRotation.Text = "Rotation";
            // 
            // cbRootPosX
            // 
            cbRootPosX.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbRootPosX.AutoSize = true;
            cbRootPosX.Location = new System.Drawing.Point(91, 9);
            cbRootPosX.Name = "cbRootPosX";
            cbRootPosX.Size = new System.Drawing.Size(32, 17);
            cbRootPosX.TabIndex = 99;
            cbRootPosX.Text = "X";
            cbRootPosX.CheckedChanged += cbRootPosX_CheckedChanged;
            // 
            // cbRootPosY
            // 
            cbRootPosY.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbRootPosY.AutoSize = true;
            cbRootPosY.Location = new System.Drawing.Point(131, 9);
            cbRootPosY.Name = "cbRootPosY";
            cbRootPosY.Size = new System.Drawing.Size(31, 17);
            cbRootPosY.TabIndex = 100;
            cbRootPosY.Text = "Y";
            // 
            // dgvBoundingMeshList
            // 
            dgvBoundingMeshList.AllowUserToAddRows = false;
            dgvBoundingMeshList.AllowUserToDeleteRows = false;
            dgvBoundingMeshList.AllowUserToDragDropRows = false;
            dgvBoundingMeshList.AllowUserToPasteCells = false;
            dgvBoundingMeshList.AllowUserToResizeColumns = false;
            dgvBoundingMeshList.ColumnHeadersHeight = 17;
            dgvBoundingMeshList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dgvBoundingMeshListCheckboxes, dgvBoundingMeshListMeshes });
            dgvBoundingMeshList.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvBoundingMeshList.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dgvBoundingMeshList.Location = new System.Drawing.Point(1, 25);
            dgvBoundingMeshList.MultiSelect = false;
            dgvBoundingMeshList.Name = "dgvBoundingMeshList";
            dgvBoundingMeshList.RowHeadersWidth = 41;
            dgvBoundingMeshList.Size = new System.Drawing.Size(278, 328);
            dgvBoundingMeshList.TabIndex = 25;
            dgvBoundingMeshList.CellMouseDoubleClick += dgvBoundingMeshList_CellMouseDoubleClick;
            dgvBoundingMeshList.SelectionChanged += dgvBoundingMeshList_SelectionChanged;
            // 
            // dgvBoundingMeshListCheckboxes
            // 
            dgvBoundingMeshListCheckboxes.HeaderText = "Use";
            dgvBoundingMeshListCheckboxes.Name = "dgvBoundingMeshListCheckboxes";
            dgvBoundingMeshListCheckboxes.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            dgvBoundingMeshListCheckboxes.Width = 40;
            // 
            // dgvBoundingMeshListMeshes
            // 
            dgvBoundingMeshListMeshes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dgvBoundingMeshListMeshes.HeaderText = "Mesh";
            dgvBoundingMeshListMeshes.Name = "dgvBoundingMeshListMeshes";
            dgvBoundingMeshListMeshes.ReadOnly = true;
            dgvBoundingMeshListMeshes.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // darkLabel33
            // 
            darkLabel33.AutoSize = true;
            darkLabel33.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel33.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel33.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel33.Location = new System.Drawing.Point(30, 30);
            darkLabel33.Name = "darkLabel33";
            darkLabel33.Size = new System.Drawing.Size(14, 13);
            darkLabel33.TabIndex = 47;
            darkLabel33.Text = "X";
            // 
            // darkLabel30
            // 
            darkLabel30.AutoSize = true;
            darkLabel30.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel30.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel30.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel30.Location = new System.Drawing.Point(30, 58);
            darkLabel30.Name = "darkLabel30";
            darkLabel30.Size = new System.Drawing.Size(14, 13);
            darkLabel30.TabIndex = 44;
            darkLabel30.Text = "X";
            // 
            // darkLabel34
            // 
            darkLabel34.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel34.Location = new System.Drawing.Point(1, 58);
            darkLabel34.Name = "darkLabel34";
            darkLabel34.Size = new System.Drawing.Size(32, 13);
            darkLabel34.TabIndex = 100;
            darkLabel34.Text = "Max:";
            // 
            // darkLabel35
            // 
            darkLabel35.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel35.Location = new System.Drawing.Point(1, 30);
            darkLabel35.Name = "darkLabel35";
            darkLabel35.Size = new System.Drawing.Size(32, 13);
            darkLabel35.TabIndex = 99;
            darkLabel35.Text = "Min:";
            // 
            // darkLabel31
            // 
            darkLabel31.AutoSize = true;
            darkLabel31.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel31.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel31.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel31.Location = new System.Drawing.Point(195, 30);
            darkLabel31.Name = "darkLabel31";
            darkLabel31.Size = new System.Drawing.Size(14, 13);
            darkLabel31.TabIndex = 49;
            darkLabel31.Text = "Z";
            // 
            // darkLabel32
            // 
            darkLabel32.AutoSize = true;
            darkLabel32.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel32.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel32.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel32.Location = new System.Drawing.Point(112, 30);
            darkLabel32.Name = "darkLabel32";
            darkLabel32.Size = new System.Drawing.Size(14, 13);
            darkLabel32.TabIndex = 48;
            darkLabel32.Text = "Y";
            // 
            // darkLabel17
            // 
            darkLabel17.AutoSize = true;
            darkLabel17.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel17.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel17.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel17.Location = new System.Drawing.Point(195, 58);
            darkLabel17.Name = "darkLabel17";
            darkLabel17.Size = new System.Drawing.Size(14, 13);
            darkLabel17.TabIndex = 46;
            darkLabel17.Text = "Z";
            // 
            // darkLabel20
            // 
            darkLabel20.AutoSize = true;
            darkLabel20.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel20.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel20.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel20.Location = new System.Drawing.Point(112, 58);
            darkLabel20.Name = "darkLabel20";
            darkLabel20.Size = new System.Drawing.Size(14, 13);
            darkLabel20.TabIndex = 45;
            darkLabel20.Text = "Y";
            // 
            // darkLabel14
            // 
            darkLabel14.AutoSize = true;
            darkLabel14.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel14.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel14.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel14.Location = new System.Drawing.Point(196, 120);
            darkLabel14.Name = "darkLabel14";
            darkLabel14.Size = new System.Drawing.Size(14, 13);
            darkLabel14.TabIndex = 43;
            darkLabel14.Text = "Z";
            // 
            // darkLabel15
            // 
            darkLabel15.AutoSize = true;
            darkLabel15.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel15.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel15.Location = new System.Drawing.Point(113, 121);
            darkLabel15.Name = "darkLabel15";
            darkLabel15.Size = new System.Drawing.Size(14, 13);
            darkLabel15.TabIndex = 42;
            darkLabel15.Text = "Y";
            // 
            // nudBBoxMaxY
            // 
            nudBBoxMaxY.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMaxY.Location = new System.Drawing.Point(128, 56);
            nudBBoxMaxY.LoopValues = false;
            nudBBoxMaxY.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMaxY.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMaxY.Name = "nudBBoxMaxY";
            nudBBoxMaxY.Size = new System.Drawing.Size(64, 22);
            nudBBoxMaxY.TabIndex = 39;
            nudBBoxMaxY.ValueChanged += nudBBoxMaxY_ValueChanged;
            nudBBoxMaxY.Validated += animParameter_Validated;
            // 
            // nudBBoxMaxZ
            // 
            nudBBoxMaxZ.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMaxZ.Location = new System.Drawing.Point(211, 56);
            nudBBoxMaxZ.LoopValues = false;
            nudBBoxMaxZ.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMaxZ.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMaxZ.Name = "nudBBoxMaxZ";
            nudBBoxMaxZ.Size = new System.Drawing.Size(64, 22);
            nudBBoxMaxZ.TabIndex = 40;
            nudBBoxMaxZ.ValueChanged += nudBBoxMaxZ_ValueChanged;
            nudBBoxMaxZ.Validated += animParameter_Validated;
            // 
            // nudBBoxMaxX
            // 
            nudBBoxMaxX.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMaxX.Location = new System.Drawing.Point(46, 56);
            nudBBoxMaxX.LoopValues = false;
            nudBBoxMaxX.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMaxX.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMaxX.Name = "nudBBoxMaxX";
            nudBBoxMaxX.Size = new System.Drawing.Size(64, 22);
            nudBBoxMaxX.TabIndex = 38;
            nudBBoxMaxX.ValueChanged += nudBBoxMaxX_ValueChanged;
            nudBBoxMaxX.Validated += animParameter_Validated;
            // 
            // nudBBoxMinY
            // 
            nudBBoxMinY.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMinY.Location = new System.Drawing.Point(128, 28);
            nudBBoxMinY.LoopValues = false;
            nudBBoxMinY.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMinY.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMinY.Name = "nudBBoxMinY";
            nudBBoxMinY.Size = new System.Drawing.Size(64, 22);
            nudBBoxMinY.TabIndex = 36;
            nudBBoxMinY.ValueChanged += nudBBoxMinY_ValueChanged;
            nudBBoxMinY.Validated += animParameter_Validated;
            // 
            // nudBBoxMinZ
            // 
            nudBBoxMinZ.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMinZ.Location = new System.Drawing.Point(211, 28);
            nudBBoxMinZ.LoopValues = false;
            nudBBoxMinZ.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMinZ.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMinZ.Name = "nudBBoxMinZ";
            nudBBoxMinZ.Size = new System.Drawing.Size(65, 22);
            nudBBoxMinZ.TabIndex = 37;
            nudBBoxMinZ.ValueChanged += nudBBoxMinZ_ValueChanged;
            nudBBoxMinZ.Validated += animParameter_Validated;
            // 
            // butShrinkBBox
            // 
            butShrinkBBox.Checked = false;
            butShrinkBBox.Location = new System.Drawing.Point(143, 146);
            butShrinkBBox.Name = "butShrinkBBox";
            butShrinkBBox.Size = new System.Drawing.Size(133, 23);
            butShrinkBBox.TabIndex = 34;
            butShrinkBBox.Text = "Shrink";
            toolTip1.SetToolTip(butShrinkBBox, "Deflate bounding box");
            butShrinkBBox.Click += butShrinkBBox_Click;
            // 
            // nudBBoxMinX
            // 
            nudBBoxMinX.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudBBoxMinX.Location = new System.Drawing.Point(46, 28);
            nudBBoxMinX.LoopValues = false;
            nudBBoxMinX.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudBBoxMinX.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudBBoxMinX.Name = "nudBBoxMinX";
            nudBBoxMinX.Size = new System.Drawing.Size(64, 22);
            nudBBoxMinX.TabIndex = 35;
            nudBBoxMinX.ValueChanged += nudBBoxMinX_ValueChanged;
            nudBBoxMinX.Validated += animParameter_Validated;
            // 
            // butResetBBoxAnim
            // 
            butResetBBoxAnim.Checked = false;
            butResetBBoxAnim.Location = new System.Drawing.Point(143, 84);
            butResetBBoxAnim.Name = "butResetBBoxAnim";
            butResetBBoxAnim.Size = new System.Drawing.Size(132, 23);
            butResetBBoxAnim.TabIndex = 29;
            butResetBBoxAnim.Text = "Delete";
            toolTip1.SetToolTip(butResetBBoxAnim, "Delete collision box");
            butResetBBoxAnim.Click += butResetBBoxAnim_Click;
            // 
            // butCalcBBoxAnim
            // 
            butCalcBBoxAnim.Checked = false;
            butCalcBBoxAnim.Location = new System.Drawing.Point(5, 84);
            butCalcBBoxAnim.Name = "butCalcBBoxAnim";
            butCalcBBoxAnim.Size = new System.Drawing.Size(132, 23);
            butCalcBBoxAnim.TabIndex = 28;
            butCalcBBoxAnim.Text = "Calculate";
            toolTip1.SetToolTip(butCalcBBoxAnim, "Calculate collision box");
            butCalcBBoxAnim.Click += butCalcBBoxAnim_Click;
            // 
            // nudGrowY
            // 
            nudGrowY.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudGrowY.Location = new System.Drawing.Point(129, 118);
            nudGrowY.LoopValues = false;
            nudGrowY.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            nudGrowY.Name = "nudGrowY";
            nudGrowY.Size = new System.Drawing.Size(64, 22);
            nudGrowY.TabIndex = 31;
            // 
            // nudGrowX
            // 
            nudGrowX.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudGrowX.Location = new System.Drawing.Point(47, 118);
            nudGrowX.LoopValues = false;
            nudGrowX.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            nudGrowX.Name = "nudGrowX";
            nudGrowX.Size = new System.Drawing.Size(64, 22);
            nudGrowX.TabIndex = 30;
            // 
            // butGrowBBox
            // 
            butGrowBBox.Checked = false;
            butGrowBBox.Location = new System.Drawing.Point(5, 146);
            butGrowBBox.Name = "butGrowBBox";
            butGrowBBox.Size = new System.Drawing.Size(132, 23);
            butGrowBBox.TabIndex = 33;
            butGrowBBox.Text = "Grow";
            toolTip1.SetToolTip(butGrowBBox, "Inflate bounding box");
            butGrowBBox.Click += butGrowBBox_Click;
            // 
            // nudGrowZ
            // 
            nudGrowZ.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 0 });
            nudGrowZ.Location = new System.Drawing.Point(212, 118);
            nudGrowZ.LoopValues = false;
            nudGrowZ.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            nudGrowZ.Name = "nudGrowZ";
            nudGrowZ.Size = new System.Drawing.Size(64, 22);
            nudGrowZ.TabIndex = 32;
            // 
            // panelTimeline
            // 
            panelTimeline.Controls.Add(timeline);
            panelTimeline.Controls.Add(panelTransport);
            panelTimeline.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelTimeline.Location = new System.Drawing.Point(4, 676);
            panelTimeline.Name = "panelTimeline";
            panelTimeline.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            panelTimeline.Size = new System.Drawing.Size(1031, 38);
            panelTimeline.TabIndex = 8;
            // 
            // timeline
            // 
            timeline.Dock = System.Windows.Forms.DockStyle.Fill;
            timeline.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            timeline.Location = new System.Drawing.Point(0, 2);
            timeline.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            timeline.Maximum = 0;
            timeline.Minimum = 0;
            timeline.Name = "timeline";
            timeline.SelectionEnd = 0;
            timeline.SelectionStart = 0;
            timeline.Size = new System.Drawing.Size(784, 36);
            timeline.TabIndex = 3;
            timeline.TabStop = false;
            timeline.Value = 0;
            timeline.ValueChanged += timeline_ValueChanged;
            timeline.SelectionChanged += timeline_SelectionChanged;
            timeline.MouseDown += timeline_MouseDown;
            // 
            // panelTransport
            // 
            panelTransport.Controls.Add(darkToolStrip1);
            panelTransport.Dock = System.Windows.Forms.DockStyle.Right;
            panelTransport.Location = new System.Drawing.Point(784, 2);
            panelTransport.Name = "panelTransport";
            panelTransport.Size = new System.Drawing.Size(247, 36);
            panelTransport.TabIndex = 2;
            // 
            // darkToolStrip1
            // 
            darkToolStrip1.AutoSize = false;
            darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkToolStrip1.CanOverflow = false;
            darkToolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripSeparator6, butTransportStart, butTransportFrameBack, butTransportPlay, butTransportFrameForward, butTransportEnd, toolStripSeparator7, butTransportChained, butTransportSound, butTransportCondition });
            darkToolStrip1.Location = new System.Drawing.Point(0, 0);
            darkToolStrip1.Name = "darkToolStrip1";
            darkToolStrip1.Padding = new System.Windows.Forms.Padding(3, 0, 1, 0);
            darkToolStrip1.Size = new System.Drawing.Size(247, 36);
            darkToolStrip1.TabIndex = 0;
            darkToolStrip1.Text = "darkToolStrip1";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator6.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 36);
            // 
            // butTransportStart
            // 
            butTransportStart.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportStart.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportStart.Image = Properties.Resources.transport_start_24;
            butTransportStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportStart.Name = "butTransportStart";
            butTransportStart.Size = new System.Drawing.Size(28, 33);
            butTransportStart.ToolTipText = "Go to start";
            butTransportStart.Click += butTransportStart_Click;
            // 
            // butTransportFrameBack
            // 
            butTransportFrameBack.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportFrameBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportFrameBack.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportFrameBack.Image = Properties.Resources.transport_frame_back_24;
            butTransportFrameBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportFrameBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportFrameBack.Name = "butTransportFrameBack";
            butTransportFrameBack.Size = new System.Drawing.Size(28, 33);
            butTransportFrameBack.ToolTipText = "Back 1 frame";
            butTransportFrameBack.Click += butTransportFrameBack_Click;
            // 
            // butTransportPlay
            // 
            butTransportPlay.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportPlay.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportPlay.Image = Properties.Resources.transport_play_24;
            butTransportPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportPlay.Name = "butTransportPlay";
            butTransportPlay.Size = new System.Drawing.Size(28, 33);
            butTransportPlay.ToolTipText = "Playback";
            butTransportPlay.Click += butTransportPlay_Click;
            // 
            // butTransportFrameForward
            // 
            butTransportFrameForward.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportFrameForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportFrameForward.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportFrameForward.Image = Properties.Resources.transport_frame_forward_24;
            butTransportFrameForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportFrameForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportFrameForward.Name = "butTransportFrameForward";
            butTransportFrameForward.Size = new System.Drawing.Size(28, 33);
            butTransportFrameForward.ToolTipText = "Forward 1 frame";
            butTransportFrameForward.Click += butTransportFrameForward_Click;
            // 
            // butTransportEnd
            // 
            butTransportEnd.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportEnd.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportEnd.Image = Properties.Resources.transport_end_24;
            butTransportEnd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportEnd.Name = "butTransportEnd";
            butTransportEnd.Size = new System.Drawing.Size(28, 33);
            butTransportEnd.ToolTipText = "Go to end";
            butTransportEnd.Click += butTransportEnd_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator7.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(6, 36);
            // 
            // butTransportChained
            // 
            butTransportChained.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportChained.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportChained.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportChained.Image = Properties.Resources.transport_chain_disabled_24;
            butTransportChained.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportChained.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportChained.Name = "butTransportChained";
            butTransportChained.Size = new System.Drawing.Size(28, 33);
            butTransportChained.ToolTipText = "Chain playback";
            butTransportChained.Click += transportChained_Click;
            // 
            // butTransportSound
            // 
            butTransportSound.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportSound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportSound.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportSound.Image = Properties.Resources.transport_mute_24;
            butTransportSound.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportSound.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportSound.Name = "butTransportSound";
            butTransportSound.Size = new System.Drawing.Size(28, 33);
            butTransportSound.ToolTipText = "Toggle sound preview";
            butTransportSound.Click += butTransportSound_Click;
            // 
            // butTransportCondition
            // 
            butTransportCondition.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTransportCondition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTransportCondition.DoubleClickEnabled = true;
            butTransportCondition.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTransportCondition.Image = Properties.Resources.transport_on_nothing_24;
            butTransportCondition.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            butTransportCondition.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTransportCondition.Name = "butTransportCondition";
            butTransportCondition.Size = new System.Drawing.Size(28, 33);
            butTransportCondition.Click += butTransportCondition_Click;
            // 
            // darkSectionPanel4
            // 
            darkSectionPanel4.Controls.Add(nudEndHorVel);
            darkSectionPanel4.Controls.Add(nudStartHorVel);
            darkSectionPanel4.Controls.Add(nudEndVertVel);
            darkSectionPanel4.Controls.Add(nudStartVertVel);
            darkSectionPanel4.Controls.Add(nudEndFrame);
            darkSectionPanel4.Controls.Add(darkLabel2);
            darkSectionPanel4.Controls.Add(nudNextFrame);
            darkSectionPanel4.Controls.Add(nudNextAnim);
            darkSectionPanel4.Controls.Add(tbStateId);
            darkSectionPanel4.Controls.Add(nudFramerate);
            darkSectionPanel4.Controls.Add(darkLabel25);
            darkSectionPanel4.Controls.Add(cmbStateID);
            darkSectionPanel4.Controls.Add(darkButton3);
            darkSectionPanel4.Controls.Add(darkLabel24);
            darkSectionPanel4.Controls.Add(darkLabel23);
            darkSectionPanel4.Controls.Add(darkLabel22);
            darkSectionPanel4.Controls.Add(butEditStateChanges);
            darkSectionPanel4.Controls.Add(tbName);
            darkSectionPanel4.Controls.Add(darkLabel3);
            darkSectionPanel4.Controls.Add(darkLabel4);
            darkSectionPanel4.Controls.Add(darkLabel5);
            darkSectionPanel4.Controls.Add(darkLabel6);
            darkSectionPanel4.Controls.Add(darkLabel7);
            darkSectionPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            darkSectionPanel4.Location = new System.Drawing.Point(0, 272);
            darkSectionPanel4.MaximumSize = new System.Drawing.Size(280, 238);
            darkSectionPanel4.Name = "darkSectionPanel4";
            darkSectionPanel4.SectionHeader = "Current Animation";
            darkSectionPanel4.Size = new System.Drawing.Size(280, 197);
            darkSectionPanel4.TabIndex = 127;
            // 
            // nudEndHorVel
            // 
            nudEndHorVel.DecimalPlaces = 4;
            nudEndHorVel.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudEndHorVel.Location = new System.Drawing.Point(211, 141);
            nudEndHorVel.LoopValues = true;
            nudEndHorVel.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudEndHorVel.Minimum = new decimal(new int[] { 32767, 0, 0, int.MinValue });
            nudEndHorVel.Name = "nudEndHorVel";
            nudEndHorVel.Size = new System.Drawing.Size(64, 22);
            nudEndHorVel.TabIndex = 130;
            nudEndHorVel.ValueChanged += nudEndHorVel_ValueChanged;
            nudEndHorVel.Validated += animParameter_Validated;
            // 
            // nudStartHorVel
            // 
            nudStartHorVel.DecimalPlaces = 4;
            nudStartHorVel.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudStartHorVel.Location = new System.Drawing.Point(142, 141);
            nudStartHorVel.LoopValues = true;
            nudStartHorVel.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudStartHorVel.Minimum = new decimal(new int[] { 32767, 0, 0, int.MinValue });
            nudStartHorVel.Name = "nudStartHorVel";
            nudStartHorVel.Size = new System.Drawing.Size(64, 22);
            nudStartHorVel.TabIndex = 129;
            nudStartHorVel.ValueChanged += nudStartHorVel_ValueChanged;
            nudStartHorVel.Validated += animParameter_Validated;
            // 
            // nudEndVertVel
            // 
            nudEndVertVel.DecimalPlaces = 4;
            nudEndVertVel.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudEndVertVel.Location = new System.Drawing.Point(73, 141);
            nudEndVertVel.LoopValues = true;
            nudEndVertVel.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudEndVertVel.Minimum = new decimal(new int[] { 32767, 0, 0, int.MinValue });
            nudEndVertVel.Name = "nudEndVertVel";
            nudEndVertVel.Size = new System.Drawing.Size(64, 22);
            nudEndVertVel.TabIndex = 128;
            nudEndVertVel.ValueChanged += nudEndVertVel_ValueChanged;
            nudEndVertVel.Validated += animParameter_Validated;
            // 
            // nudStartVertVel
            // 
            nudStartVertVel.DecimalPlaces = 4;
            nudStartVertVel.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudStartVertVel.Location = new System.Drawing.Point(4, 141);
            nudStartVertVel.LoopValues = true;
            nudStartVertVel.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudStartVertVel.Minimum = new decimal(new int[] { 32767, 0, 0, int.MinValue });
            nudStartVertVel.Name = "nudStartVertVel";
            nudStartVertVel.Size = new System.Drawing.Size(64, 22);
            nudStartVertVel.TabIndex = 127;
            nudStartVertVel.ValueChanged += nudStartVertVel_ValueChanged;
            nudStartVertVel.Validated += animParameter_Validated;
            // 
            // nudEndFrame
            // 
            nudEndFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudEndFrame.Location = new System.Drawing.Point(73, 98);
            nudEndFrame.LoopValues = false;
            nudEndFrame.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudEndFrame.Name = "nudEndFrame";
            nudEndFrame.Size = new System.Drawing.Size(64, 22);
            nudEndFrame.TabIndex = 125;
            nudEndFrame.ValueChanged += nudEndFrame_ValueChanged;
            // 
            // darkLabel2
            // 
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(70, 82);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(64, 13);
            darkLabel2.TabIndex = 126;
            darkLabel2.Text = "End frame";
            // 
            // nudNextFrame
            // 
            nudNextFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudNextFrame.Location = new System.Drawing.Point(211, 98);
            nudNextFrame.LoopValues = false;
            nudNextFrame.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudNextFrame.Name = "nudNextFrame";
            nudNextFrame.Size = new System.Drawing.Size(64, 22);
            nudNextFrame.TabIndex = 11;
            nudNextFrame.ValueChanged += nudNextFrame_ValueChanged;
            nudNextFrame.Validated += animParameter_Validated;
            // 
            // nudNextAnim
            // 
            nudNextAnim.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudNextAnim.Location = new System.Drawing.Point(142, 98);
            nudNextAnim.LoopValues = false;
            nudNextAnim.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudNextAnim.Name = "nudNextAnim";
            nudNextAnim.Size = new System.Drawing.Size(64, 22);
            nudNextAnim.TabIndex = 10;
            nudNextAnim.ValueChanged += nudNextAnim_ValueChanged;
            nudNextAnim.Validated += animParameter_Validated;
            // 
            // nudFramerate
            // 
            nudFramerate.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudFramerate.Location = new System.Drawing.Point(4, 98);
            nudFramerate.LoopValues = false;
            nudFramerate.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudFramerate.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFramerate.Name = "nudFramerate";
            nudFramerate.Size = new System.Drawing.Size(64, 22);
            nudFramerate.TabIndex = 9;
            nudFramerate.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudFramerate.ValueChanged += nudFramerate_ValueChanged;
            nudFramerate.Validated += animParameter_Validated;
            // 
            // cmbStateID
            // 
            cmbStateID.Location = new System.Drawing.Point(44, 56);
            cmbStateID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmbStateID.Name = "cmbStateID";
            cmbStateID.Size = new System.Drawing.Size(231, 23);
            cmbStateID.TabIndex = 124;
            cmbStateID.TabStop = false;
            cmbStateID.SelectedIndexChanged += cmbStateID_SelectedIndexChanged;
            // 
            // darkButton3
            // 
            darkButton3.Checked = false;
            darkButton3.Location = new System.Drawing.Point(142, 169);
            darkButton3.Name = "darkButton3";
            darkButton3.Size = new System.Drawing.Size(133, 23);
            darkButton3.TabIndex = 17;
            darkButton3.Text = "Anim commands...";
            darkButton3.Click += butEditAnimCommands_Click;
            // 
            // butEditStateChanges
            // 
            butEditStateChanges.Checked = false;
            butEditStateChanges.Location = new System.Drawing.Point(4, 169);
            butEditStateChanges.Name = "butEditStateChanges";
            butEditStateChanges.Size = new System.Drawing.Size(133, 23);
            butEditStateChanges.TabIndex = 16;
            butEditStateChanges.Text = "State changes...";
            butEditStateChanges.Click += butEditStateChanges_Click;
            // 
            // panelMain
            // 
            panelMain.Controls.Add(panelView);
            panelMain.Controls.Add(panelRight);
            panelMain.Controls.Add(panelLeft);
            panelMain.Controls.Add(panelTimeline);
            panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMain.Location = new System.Drawing.Point(0, 52);
            panelMain.Name = "panelMain";
            panelMain.Padding = new System.Windows.Forms.Padding(4);
            panelMain.Size = new System.Drawing.Size(1039, 718);
            panelMain.TabIndex = 129;
            // 
            // panelView
            // 
            panelView.Controls.Add(panelRendering);
            panelView.Dock = System.Windows.Forms.DockStyle.Fill;
            panelView.Location = new System.Drawing.Point(284, 4);
            panelView.Name = "panelView";
            panelView.SectionHeader = null;
            panelView.Size = new System.Drawing.Size(471, 672);
            panelView.TabIndex = 13;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(darkSectionPanel2);
            panelRight.Controls.Add(panelTransform);
            panelRight.Controls.Add(darkSectionPanel5);
            panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            panelRight.Location = new System.Drawing.Point(755, 4);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(280, 672);
            panelRight.TabIndex = 12;
            // 
            // panelTransform
            // 
            panelTransform.Controls.Add(darkLabel8);
            panelTransform.Controls.Add(picTransformPreview);
            panelTransform.Controls.Add(cmbTransformMode);
            panelTransform.Controls.Add(darkLabel29);
            panelTransform.Controls.Add(darkLabel28);
            panelTransform.Controls.Add(darkLabel21);
            panelTransform.Controls.Add(darkLabel26);
            panelTransform.Controls.Add(nudTransX);
            panelTransform.Controls.Add(darkLabel27);
            panelTransform.Controls.Add(nudTransY);
            panelTransform.Controls.Add(nudTransZ);
            panelTransform.Controls.Add(darkLabel1);
            panelTransform.Controls.Add(darkLabel18);
            panelTransform.Controls.Add(nudRotX);
            panelTransform.Controls.Add(darkLabel19);
            panelTransform.Controls.Add(nudRotY);
            panelTransform.Controls.Add(nudRotZ);
            panelTransform.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelTransform.Location = new System.Drawing.Point(0, 386);
            panelTransform.Name = "panelTransform";
            panelTransform.SectionHeader = "Transform";
            panelTransform.Size = new System.Drawing.Size(280, 112);
            panelTransform.TabIndex = 130;
            // 
            // darkLabel8
            // 
            darkLabel8.AutoSize = true;
            darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel8.Location = new System.Drawing.Point(2, 87);
            darkLabel8.Name = "darkLabel8";
            darkLabel8.Size = new System.Drawing.Size(40, 13);
            darkLabel8.TabIndex = 101;
            darkLabel8.Text = "Mode:";
            // 
            // picTransformPreview
            // 
            picTransformPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picTransformPreview.Location = new System.Drawing.Point(198, 84);
            picTransformPreview.Name = "picTransformPreview";
            picTransformPreview.Size = new System.Drawing.Size(77, 23);
            picTransformPreview.TabIndex = 100;
            picTransformPreview.TabStop = false;
            toolTip1.SetToolTip(picTransformPreview, "Transform graph preview");
            // 
            // cmbTransformMode
            // 
            cmbTransformMode.FormattingEnabled = true;
            cmbTransformMode.Items.AddRange(new object[] { "None", "Smooth", "Smooth reverse", "Linear", "Linear reverse", "Symmetric smooth", "Symmetric linear" });
            cmbTransformMode.Location = new System.Drawing.Point(46, 84);
            cmbTransformMode.Name = "cmbTransformMode";
            cmbTransformMode.Size = new System.Drawing.Size(146, 23);
            cmbTransformMode.TabIndex = 24;
            toolTip1.SetToolTip(cmbTransformMode, "Transform interpolation mode");
            cmbTransformMode.SelectedIndexChanged += cmbTransformMode_SelectedIndexChanged;
            // 
            // darkLabel29
            // 
            darkLabel29.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel29.Location = new System.Drawing.Point(1, 58);
            darkLabel29.Name = "darkLabel29";
            darkLabel29.Size = new System.Drawing.Size(28, 13);
            darkLabel29.TabIndex = 98;
            darkLabel29.Text = "Pos:";
            // 
            // darkLabel28
            // 
            darkLabel28.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel28.Location = new System.Drawing.Point(1, 30);
            darkLabel28.Name = "darkLabel28";
            darkLabel28.Size = new System.Drawing.Size(29, 13);
            darkLabel28.TabIndex = 97;
            darkLabel28.Text = "Rot:";
            // 
            // darkLabel21
            // 
            darkLabel21.AutoSize = true;
            darkLabel21.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel21.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel21.Location = new System.Drawing.Point(195, 58);
            darkLabel21.Name = "darkLabel21";
            darkLabel21.Size = new System.Drawing.Size(14, 13);
            darkLabel21.TabIndex = 20;
            darkLabel21.Text = "Z";
            // 
            // darkLabel26
            // 
            darkLabel26.AutoSize = true;
            darkLabel26.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel26.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel26.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel26.Location = new System.Drawing.Point(112, 58);
            darkLabel26.Name = "darkLabel26";
            darkLabel26.Size = new System.Drawing.Size(14, 13);
            darkLabel26.TabIndex = 19;
            darkLabel26.Text = "Y";
            // 
            // nudTransX
            // 
            nudTransX.DecimalPlaces = 4;
            nudTransX.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransX.Location = new System.Drawing.Point(46, 56);
            nudTransX.LoopValues = false;
            nudTransX.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransX.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransX.Name = "nudTransX";
            nudTransX.Size = new System.Drawing.Size(64, 22);
            nudTransX.TabIndex = 21;
            nudTransX.ValueChanged += nudTransX_ValueChanged;
            // 
            // darkLabel27
            // 
            darkLabel27.AutoSize = true;
            darkLabel27.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel27.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel27.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel27.Location = new System.Drawing.Point(30, 58);
            darkLabel27.Name = "darkLabel27";
            darkLabel27.Size = new System.Drawing.Size(14, 13);
            darkLabel27.TabIndex = 18;
            darkLabel27.Text = "X";
            // 
            // nudTransY
            // 
            nudTransY.DecimalPlaces = 4;
            nudTransY.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransY.Location = new System.Drawing.Point(128, 56);
            nudTransY.LoopValues = false;
            nudTransY.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransY.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransY.Name = "nudTransY";
            nudTransY.Size = new System.Drawing.Size(64, 22);
            nudTransY.TabIndex = 22;
            nudTransY.ValueChanged += nudTransY_ValueChanged;
            // 
            // nudTransZ
            // 
            nudTransZ.DecimalPlaces = 4;
            nudTransZ.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransZ.Location = new System.Drawing.Point(211, 56);
            nudTransZ.LoopValues = false;
            nudTransZ.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            nudTransZ.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            nudTransZ.Name = "nudTransZ";
            nudTransZ.Size = new System.Drawing.Size(65, 22);
            nudTransZ.TabIndex = 23;
            nudTransZ.ValueChanged += nudTransZ_ValueChanged;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel1.Location = new System.Drawing.Point(195, 30);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(14, 13);
            darkLabel1.TabIndex = 14;
            darkLabel1.Text = "R";
            // 
            // darkLabel18
            // 
            darkLabel18.AutoSize = true;
            darkLabel18.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel18.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel18.Location = new System.Drawing.Point(112, 30);
            darkLabel18.Name = "darkLabel18";
            darkLabel18.Size = new System.Drawing.Size(14, 13);
            darkLabel18.TabIndex = 13;
            darkLabel18.Text = "P";
            // 
            // nudRotX
            // 
            nudRotX.DecimalPlaces = 4;
            nudRotX.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotX.Location = new System.Drawing.Point(46, 28);
            nudRotX.LoopValues = true;
            nudRotX.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotX.Name = "nudRotX";
            nudRotX.Size = new System.Drawing.Size(64, 22);
            nudRotX.TabIndex = 18;
            nudRotX.ValueChanged += nudRotX_ValueChanged;
            // 
            // darkLabel19
            // 
            darkLabel19.AutoSize = true;
            darkLabel19.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel19.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel19.Location = new System.Drawing.Point(30, 30);
            darkLabel19.Name = "darkLabel19";
            darkLabel19.Size = new System.Drawing.Size(14, 13);
            darkLabel19.TabIndex = 12;
            darkLabel19.Text = "Y";
            // 
            // nudRotY
            // 
            nudRotY.DecimalPlaces = 4;
            nudRotY.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotY.Location = new System.Drawing.Point(128, 28);
            nudRotY.LoopValues = true;
            nudRotY.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotY.Name = "nudRotY";
            nudRotY.Size = new System.Drawing.Size(64, 22);
            nudRotY.TabIndex = 19;
            nudRotY.ValueChanged += nudRotY_ValueChanged;
            // 
            // nudRotZ
            // 
            nudRotZ.DecimalPlaces = 4;
            nudRotZ.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotZ.Location = new System.Drawing.Point(211, 28);
            nudRotZ.LoopValues = true;
            nudRotZ.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotZ.Name = "nudRotZ";
            nudRotZ.Size = new System.Drawing.Size(64, 22);
            nudRotZ.TabIndex = 20;
            nudRotZ.ValueChanged += nudRotZ_ValueChanged;
            // 
            // darkSectionPanel5
            // 
            darkSectionPanel5.Controls.Add(darkLabel10);
            darkSectionPanel5.Controls.Add(nudGrowX);
            darkSectionPanel5.Controls.Add(darkLabel9);
            darkSectionPanel5.Controls.Add(nudGrowZ);
            darkSectionPanel5.Controls.Add(darkLabel33);
            darkSectionPanel5.Controls.Add(butShrinkBBox);
            darkSectionPanel5.Controls.Add(darkLabel30);
            darkSectionPanel5.Controls.Add(darkLabel34);
            darkSectionPanel5.Controls.Add(darkLabel15);
            darkSectionPanel5.Controls.Add(darkLabel35);
            darkSectionPanel5.Controls.Add(darkLabel14);
            darkSectionPanel5.Controls.Add(darkLabel31);
            darkSectionPanel5.Controls.Add(butGrowBBox);
            darkSectionPanel5.Controls.Add(butResetBBoxAnim);
            darkSectionPanel5.Controls.Add(nudGrowY);
            darkSectionPanel5.Controls.Add(darkLabel32);
            darkSectionPanel5.Controls.Add(butCalcBBoxAnim);
            darkSectionPanel5.Controls.Add(darkLabel17);
            darkSectionPanel5.Controls.Add(darkLabel20);
            darkSectionPanel5.Controls.Add(nudBBoxMinX);
            darkSectionPanel5.Controls.Add(nudBBoxMinZ);
            darkSectionPanel5.Controls.Add(nudBBoxMinY);
            darkSectionPanel5.Controls.Add(nudBBoxMaxX);
            darkSectionPanel5.Controls.Add(nudBBoxMaxZ);
            darkSectionPanel5.Controls.Add(nudBBoxMaxY);
            darkSectionPanel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            darkSectionPanel5.Location = new System.Drawing.Point(0, 498);
            darkSectionPanel5.Name = "darkSectionPanel5";
            darkSectionPanel5.SectionHeader = "Bounding Box";
            darkSectionPanel5.Size = new System.Drawing.Size(280, 174);
            darkSectionPanel5.TabIndex = 131;
            // 
            // darkLabel10
            // 
            darkLabel10.AutoSize = true;
            darkLabel10.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel10.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel10.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel10.Location = new System.Drawing.Point(31, 121);
            darkLabel10.Name = "darkLabel10";
            darkLabel10.Size = new System.Drawing.Size(14, 13);
            darkLabel10.TabIndex = 102;
            darkLabel10.Text = "X";
            // 
            // darkLabel9
            // 
            darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel9.Location = new System.Drawing.Point(2, 121);
            darkLabel9.Name = "darkLabel9";
            darkLabel9.Size = new System.Drawing.Size(43, 13);
            darkLabel9.TabIndex = 101;
            darkLabel9.Text = "Scale";
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(darkSectionPanel1);
            panelLeft.Controls.Add(darkSectionPanel4);
            panelLeft.Controls.Add(sectionBlending);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Location = new System.Drawing.Point(4, 4);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(280, 672);
            panelLeft.TabIndex = 11;
            // 
            // sectionBlending
            // 
            sectionBlending.Controls.Add(bezierCurveEditor);
            sectionBlending.Controls.Add(darkLabel36);
            sectionBlending.Controls.Add(cbBlendPreset);
            sectionBlending.Controls.Add(darkLabel13);
            sectionBlending.Controls.Add(nudBlendFrameCount);
            sectionBlending.Controls.Add(darkLabel12);
            sectionBlending.Controls.Add(bcAnimation);
            sectionBlending.Dock = System.Windows.Forms.DockStyle.Bottom;
            sectionBlending.Location = new System.Drawing.Point(0, 469);
            sectionBlending.Name = "sectionBlending";
            sectionBlending.SectionHeader = "Animation Blending";
            sectionBlending.Size = new System.Drawing.Size(280, 203);
            sectionBlending.TabIndex = 128;
            // 
            // bezierCurveEditor
            // 
            bezierCurveEditor.Location = new System.Drawing.Point(6, 57);
            bezierCurveEditor.Name = "bezierCurveEditor";
            bezierCurveEditor.Size = new System.Drawing.Size(269, 113);
            bezierCurveEditor.TabIndex = 110;
            toolTip1.SetToolTip(bezierCurveEditor, "Specify blending curve by dragging handles");
            bezierCurveEditor.ValueChanged += bezierCurveEditor_ValueChanged;
            // 
            // darkLabel36
            // 
            darkLabel36.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel36.AutoSize = true;
            darkLabel36.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel36.Location = new System.Drawing.Point(3, 179);
            darkLabel36.Name = "darkLabel36";
            darkLabel36.Size = new System.Drawing.Size(41, 13);
            darkLabel36.TabIndex = 109;
            darkLabel36.Text = "Preset:";
            // 
            // cbBlendPreset
            // 
            cbBlendPreset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbBlendPreset.FormattingEnabled = true;
            cbBlendPreset.Items.AddRange(new object[] { "Linear", "Ease In", "Ease Out", "Ease In and Out" });
            cbBlendPreset.Location = new System.Drawing.Point(49, 175);
            cbBlendPreset.Name = "cbBlendPreset";
            cbBlendPreset.Size = new System.Drawing.Size(225, 23);
            cbBlendPreset.TabIndex = 108;
            toolTip1.SetToolTip(cbBlendPreset, "Predefined curve preset");
            cbBlendPreset.SelectedIndexChanged += cbBlendPreset_SelectedIndexChanged;
            // 
            // darkLabel13
            // 
            darkLabel13.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel13.Location = new System.Drawing.Point(233, 32);
            darkLabel13.Name = "darkLabel13";
            darkLabel13.Size = new System.Drawing.Size(41, 13);
            darkLabel13.TabIndex = 107;
            darkLabel13.Text = "frames";
            // 
            // nudBlendFrameCount
            // 
            nudBlendFrameCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudBlendFrameCount.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudBlendFrameCount.Location = new System.Drawing.Point(170, 29);
            nudBlendFrameCount.LoopValues = false;
            nudBlendFrameCount.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudBlendFrameCount.Name = "nudBlendFrameCount";
            nudBlendFrameCount.Size = new System.Drawing.Size(61, 22);
            nudBlendFrameCount.TabIndex = 97;
            toolTip1.SetToolTip(nudBlendFrameCount, "Blending duration to the next animation in frames");
            nudBlendFrameCount.ValueChanged += nudBlendFrameCount_ValueChanged;
            // 
            // darkLabel12
            // 
            darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel12.Location = new System.Drawing.Point(6, 32);
            darkLabel12.Name = "darkLabel12";
            darkLabel12.Size = new System.Drawing.Size(162, 13);
            darkLabel12.TabIndex = 98;
            darkLabel12.Text = "Next anim blending duration:";
            // 
            // bcAnimation
            // 
            bcAnimation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            bcAnimation.Location = new System.Drawing.Point(6, 57);
            bcAnimation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            bcAnimation.Name = "bcAnimation";
            bcAnimation.Size = new System.Drawing.Size(266, 85);
            bcAnimation.TabIndex = 0;
            // 
            // darkContextMenu1
            // 
            darkContextMenu1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkContextMenu1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkContextMenu1.Name = "darkContextMenu1";
            darkContextMenu1.Size = new System.Drawing.Size(61, 4);
            // 
            // cmTimelineContextMenu
            // 
            cmTimelineContextMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmTimelineContextMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmTimelineContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmMarkInMenuItem, cmMarkOutMenuItem, cmSelectAllMenuItem, cnClearSelectionMenuItem, toolStripSeparator8, cmCreateAnimCommandMenuItem, cmCreateStateChangeMenuItem });
            cmTimelineContextMenu.Name = "cmTimelineContextMenu";
            cmTimelineContextMenu.Size = new System.Drawing.Size(206, 143);
            // 
            // cmMarkInMenuItem
            // 
            cmMarkInMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmMarkInMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmMarkInMenuItem.Name = "cmMarkInMenuItem";
            cmMarkInMenuItem.Size = new System.Drawing.Size(205, 22);
            cmMarkInMenuItem.Text = "Mark in";
            cmMarkInMenuItem.Click += cmMarkInMenuItem_Click;
            // 
            // cmMarkOutMenuItem
            // 
            cmMarkOutMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmMarkOutMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmMarkOutMenuItem.Name = "cmMarkOutMenuItem";
            cmMarkOutMenuItem.Size = new System.Drawing.Size(205, 22);
            cmMarkOutMenuItem.Text = "Mark out";
            cmMarkOutMenuItem.Click += cmMarkOutMenuItem_Click;
            // 
            // cmSelectAllMenuItem
            // 
            cmSelectAllMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmSelectAllMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmSelectAllMenuItem.Name = "cmSelectAllMenuItem";
            cmSelectAllMenuItem.Size = new System.Drawing.Size(205, 22);
            cmSelectAllMenuItem.Text = "Select all";
            cmSelectAllMenuItem.Click += cmSelectAllMenuItem_Click;
            // 
            // cnClearSelectionMenuItem
            // 
            cnClearSelectionMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cnClearSelectionMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cnClearSelectionMenuItem.Name = "cnClearSelectionMenuItem";
            cnClearSelectionMenuItem.Size = new System.Drawing.Size(205, 22);
            cnClearSelectionMenuItem.Text = "Clear selection";
            cnClearSelectionMenuItem.Click += cnClearSelectionMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(202, 6);
            // 
            // cmCreateAnimCommandMenuItem
            // 
            cmCreateAnimCommandMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmCreateAnimCommandMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmCreateAnimCommandMenuItem.Name = "cmCreateAnimCommandMenuItem";
            cmCreateAnimCommandMenuItem.Size = new System.Drawing.Size(205, 22);
            cmCreateAnimCommandMenuItem.Text = "Create anim command...";
            cmCreateAnimCommandMenuItem.Click += cmCreateAnimCommandMenuItem_Click;
            // 
            // cmCreateStateChangeMenuItem
            // 
            cmCreateStateChangeMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            cmCreateStateChangeMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            cmCreateStateChangeMenuItem.Name = "cmCreateStateChangeMenuItem";
            cmCreateStateChangeMenuItem.Size = new System.Drawing.Size(205, 22);
            cmCreateStateChangeMenuItem.Text = "Create state change...";
            cmCreateStateChangeMenuItem.Click += cmCreateStateChangeMenuItem_Click;
            // 
            // toolStripButton6
            // 
            toolStripButton6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripButton6.Image = (System.Drawing.Image)resources.GetObject("toolStripButton6.Image");
            toolStripButton6.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new System.Drawing.Size(28, 35);
            // 
            // FormAnimationEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1039, 795);
            Controls.Add(panelMain);
            Controls.Add(topBar);
            Controls.Add(statusStrip);
            Controls.Add(topMenu);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = topMenu;
            MinimumSize = new System.Drawing.Size(890, 660);
            Name = "FormAnimationEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Animation editor";
            FormClosing += formAnimationEditor_FormClosing;
            Shown += FormAnimationEditor_Shown;
            topMenu.ResumeLayout(false);
            topMenu.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            topBar.ResumeLayout(false);
            topBar.PerformLayout();
            darkSectionPanel1.ResumeLayout(false);
            darkSectionPanel1.PerformLayout();
            darkSectionPanel2.ResumeLayout(false);
            panelRootMotion.ResumeLayout(false);
            panelRootMotion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBoundingMeshList).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMaxX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBBoxMinX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGrowZ).EndInit();
            panelTimeline.ResumeLayout(false);
            panelTransport.ResumeLayout(false);
            darkToolStrip1.ResumeLayout(false);
            darkToolStrip1.PerformLayout();
            darkSectionPanel4.ResumeLayout(false);
            darkSectionPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudEndHorVel).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStartHorVel).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudEndVertVel).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStartVertVel).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudEndFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNextFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNextAnim).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFramerate).EndInit();
            panelMain.ResumeLayout(false);
            panelView.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelTransform.ResumeLayout(false);
            panelTransform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picTransformPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotZ).EndInit();
            darkSectionPanel5.ResumeLayout(false);
            darkSectionPanel5.PerformLayout();
            panelLeft.ResumeLayout(false);
            sectionBlending.ResumeLayout(false);
            sectionBlending.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudBlendFrameCount).EndInit();
            cmTimelineContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkMenuStrip topMenu;
        private System.Windows.Forms.ToolStripMenuItem fileeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem frameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertFrameAfterCurrentOneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem interpolateFramesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGizmoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawCollisionBoxToolStripMenuItem;
        private DarkUI.Controls.DarkStatusStrip statusStrip;
        private DarkUI.Controls.DarkButton butDeleteAnimation;
        private System.Windows.Forms.ToolStripStatusLabel statusFrame;
        private DarkUI.Controls.DarkButton butAddNewAnimation;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private TombLib.Controls.DarkAutocompleteTextBox tbStateId;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem calculateCollisionBoxForCurrentFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private DarkUI.Controls.DarkToolStrip topBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel labelAnims;
        private System.Windows.Forms.ToolStripButton butTbAddAnimation;
        private System.Windows.Forms.ToolStripButton butTbDeleteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCopyAnimation;
        private System.Windows.Forms.ToolStripButton butTbPasteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCutFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel labelFrames;
        private System.Windows.Forms.ToolStripButton butTbAddFrame;
        private System.Windows.Forms.ToolStripButton butTbDeleteFrame;
        private System.Windows.Forms.ToolStripButton butTbCopyFrame;
        private System.Windows.Forms.ToolStripButton butTbPasteFrame;
        private System.Windows.Forms.ToolStripMenuItem insertnFramesAfterCurrentOneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem curToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTbCutAnimation;
        private DarkUI.Controls.DarkLabel darkLabel22;
        private DarkUI.Controls.DarkLabel darkLabel23;
        private DarkUI.Controls.DarkLabel darkLabel24;
        private DarkUI.Controls.DarkLabel darkLabel25;
        private System.Windows.Forms.ToolStripButton butTbReplaceAnimation;
        private System.Windows.Forms.ToolStripButton butTbSplitAnimation;
        private System.Windows.Forms.ToolStripMenuItem splitAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel labelRoom;
        private DarkUI.Controls.DarkTextBox tbSearchAnimation;
        private DarkUI.Controls.DarkButton darkButton1;
        private DarkUI.Controls.DarkButton butShowAll;
        private System.Windows.Forms.ToolStripMenuItem deleteCollisionBoxForCurrentFrameToolStripMenuItem;
        private DarkUI.Controls.DarkListView lstAnimations;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel4;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelTimeline;
        private System.Windows.Forms.Panel panelTransport;
        private Controls.PanelRenderingAnimationEditor panelRendering;
        private AnimationTrackBar timeline;
        private DarkUI.Controls.DarkButton darkButton3;
        private DarkUI.Controls.DarkButton butEditStateChanges;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
        private System.Windows.Forms.ToolStripButton butTransportSound;
        private System.Windows.Forms.ToolStripButton butTransportStart;
        private System.Windows.Forms.ToolStripButton butTransportFrameBack;
        private System.Windows.Forms.ToolStripButton butTransportPlay;
        private System.Windows.Forms.ToolStripButton butTransportFrameForward;
        private System.Windows.Forms.ToolStripButton butTransportEnd;
        private DarkUI.Controls.ToolStripDarkComboBox comboRoomList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripButton butTbSaveAllChanges;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton butTransportCondition;
        private System.Windows.Forms.ToolStripButton butTbUndo;
        private System.Windows.Forms.ToolStripButton butTbRedo;
        private System.Windows.Forms.ToolStripMenuItem resampleAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resampleAnimationToKeyframesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTransportChained;
        private TombLib.Controls.DarkSearchableComboBox cmbStateID;
        private DarkUI.Controls.DarkNumericUpDown nudNextFrame;
        private DarkUI.Controls.DarkNumericUpDown nudNextAnim;
        private DarkUI.Controls.DarkNumericUpDown nudFramerate;
        private System.Windows.Forms.ToolStripMenuItem deleteEveryNthFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findReplaceAnimcommandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTbImport;
        private System.Windows.Forms.ToolStripMenuItem smoothAnimationsToolStripMenuItem;
        private DarkUI.Controls.DarkContextMenu darkContextMenu1;
        private DarkUI.Controls.DarkContextMenu cmTimelineContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cmMarkInMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmMarkOutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cnClearSelectionMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem cmCreateAnimCommandMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmCreateStateChangeMenuItem;
        private DarkUI.Controls.DarkSectionPanel panelTransform;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkNumericUpDown nudRotX;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkNumericUpDown nudRotY;
        private DarkUI.Controls.DarkNumericUpDown nudRotZ;
        private DarkUI.Controls.DarkLabel darkLabel28;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkLabel darkLabel26;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkLabel darkLabel27;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private System.Windows.Forms.PictureBox picTransformPreview;
        private DarkUI.Controls.DarkComboBox cmbTransformMode;
        private DarkUI.Controls.DarkLabel darkLabel29;
        private System.Windows.Forms.ToolStripMenuItem cmSelectAllMenuItem;
        private System.Windows.Forms.ToolStripButton butTbResetCamera;
        private System.Windows.Forms.ToolStripMenuItem scrollGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.Panel panelLeft;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMaxY;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMaxZ;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMaxX;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMinY;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMinZ;
        private DarkUI.Controls.DarkButton butShrinkBBox;
        private DarkUI.Controls.DarkNumericUpDown nudBBoxMinX;
        private DarkUI.Controls.DarkButton butResetBBoxAnim;
        private DarkUI.Controls.DarkButton butCalcBBoxAnim;
        private DarkUI.Controls.DarkNumericUpDown nudGrowY;
        private DarkUI.Controls.DarkDataGridView dgvBoundingMeshList;
        private DarkUI.Controls.DarkNumericUpDown nudGrowX;
        private DarkUI.Controls.DarkButton butGrowBBox;
        private DarkUI.Controls.DarkNumericUpDown nudGrowZ;
        private System.Windows.Forms.Panel panelRight;
        private DarkUI.Controls.DarkSectionPanel panelView;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn dgvBoundingMeshListCheckboxes;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvBoundingMeshListMeshes;
        private System.Windows.Forms.ToolStripMenuItem reverseAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mirrorAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreGridHeightToolStripMenuItem;
        private DarkUI.Controls.DarkNumericUpDown nudEndFrame;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudEndHorVel;
        private DarkUI.Controls.DarkNumericUpDown nudStartHorVel;
        private DarkUI.Controls.DarkNumericUpDown nudEndVertVel;
        private DarkUI.Controls.DarkNumericUpDown nudStartVertVel;
        private System.Windows.Forms.ToolStripMenuItem batchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedAnimationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allAnimationsToolStripMenuItem;
        private DarkUI.Controls.DarkSectionPanel sectionBlending;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkCheckBox cbRootPosZ;
        private DarkUI.Controls.DarkNumericUpDown nudBlendFrameCount;
        private DarkUI.Controls.DarkCheckBox cbRootPosX;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkCheckBox cbRootPosY;
        private DarkUI.Controls.DarkCheckBox cbRootRotation;
        private Controls.BezierCurveEditor bcAnimation;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkLabel darkLabel33;
        private DarkUI.Controls.DarkLabel darkLabel30;
        private DarkUI.Controls.DarkLabel darkLabel34;
        private DarkUI.Controls.DarkLabel darkLabel35;
        private DarkUI.Controls.DarkLabel darkLabel31;
        private DarkUI.Controls.DarkLabel darkLabel32;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private DarkUI.Controls.DarkLabel darkLabel20;
        private DarkUI.Controls.DarkLabel darkLabel36;
        private DarkUI.Controls.DarkComboBox cbBlendPreset;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel5;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private Controls.BezierCurveEditor bezierCurveEditor;
        private DarkUI.Controls.DarkPanel panelRootMotion;
        private System.Windows.Forms.ToolStripMenuItem drawSkinToolStripMenuItem;
    }
}
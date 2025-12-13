namespace WadTool
{
    partial class FormMeshEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMeshEditor));
            lstMeshes = new DarkUI.Controls.DarkTreeView();
            panelMesh = new Controls.PanelRenderingMesh();
            btCancel = new DarkUI.Controls.DarkButton();
            btOk = new DarkUI.Controls.DarkButton();
            panelEditingTools = new DarkUI.Controls.DarkSectionPanel();
            tabsModes = new TombLib.Controls.DarkTabbedContainer();
            tabFaceAttributes = new System.Windows.Forms.TabPage();
            cbTexture = new DarkUI.Controls.DarkCheckBox();
            cbBlendMode = new DarkUI.Controls.DarkComboBox();
            butApplyToAllFaces = new DarkUI.Controls.DarkButton();
            cbBlend = new DarkUI.Controls.DarkCheckBox();
            nudShineStrength = new DarkUI.Controls.DarkNumericUpDown();
            cbSheen = new DarkUI.Controls.DarkCheckBox();
            butDoubleSide = new DarkUI.Controls.DarkButton();
            tabVertexRemap = new System.Windows.Forms.TabPage();
            butAutoFit = new DarkUI.Controls.DarkButton();
            nudVertexNum = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            butFindVertex = new DarkUI.Controls.DarkButton();
            butRemapVertex = new DarkUI.Controls.DarkButton();
            tabVertexShadesAndNormals = new System.Windows.Forms.TabPage();
            butApplyShadesToAllVertices = new DarkUI.Controls.DarkButton();
            panelColor = new DarkUI.Controls.DarkPanel();
            butRecalcNormalsAvg = new DarkUI.Controls.DarkButton();
            darkLabel7 = new DarkUI.Controls.DarkLabel();
            darkLabel10 = new DarkUI.Controls.DarkLabel();
            butRecalcNormals = new DarkUI.Controls.DarkButton();
            tabVertexEffects = new System.Windows.Forms.TabPage();
            butPreview = new DarkUI.Controls.DarkButton();
            butConvertFromShades = new DarkUI.Controls.DarkButton();
            nudMove = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel5 = new DarkUI.Controls.DarkLabel();
            nudGlow = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            butApplyToAllVertices = new DarkUI.Controls.DarkButton();
            tabVertexWeights = new System.Windows.Forms.TabPage();
            butApplyWeightsToAllVertices = new DarkUI.Controls.DarkButton();
            nudWeightIndex4 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightIndex3 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightIndex2 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightIndex1 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightValue4 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightValue3 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightValue2 = new DarkUI.Controls.DarkNumericUpDown();
            nudWeightValue1 = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel12 = new DarkUI.Controls.DarkLabel();
            darkLabel11 = new DarkUI.Controls.DarkLabel();
            tabSphere = new System.Windows.Forms.TabPage();
            darkLabel6 = new DarkUI.Controls.DarkLabel();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            butResetSphere = new DarkUI.Controls.DarkButton();
            nudSphereRadius = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel9 = new DarkUI.Controls.DarkLabel();
            nudSphereZ = new DarkUI.Controls.DarkNumericUpDown();
            nudSphereY = new DarkUI.Controls.DarkNumericUpDown();
            nudSphereX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel8 = new DarkUI.Controls.DarkLabel();
            cbExtra = new DarkUI.Controls.DarkCheckBox();
            cbEditingMode = new DarkUI.Controls.DarkComboBox();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            panelSearchTree = new DarkUI.Controls.DarkPanel();
            butSearchMeshes = new DarkUI.Controls.DarkButton();
            tbSearchMeshes = new DarkUI.Controls.DarkTextBox();
            panelMain = new DarkUI.Controls.DarkPanel();
            panelCenter = new DarkUI.Controls.DarkPanel();
            topBar = new DarkUI.Controls.DarkToolStrip();
            butTbUndo = new System.Windows.Forms.ToolStripButton();
            butTbRedo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            butTbImport = new System.Windows.Forms.ToolStripButton();
            butTbExport = new System.Windows.Forms.ToolStripButton();
            butTbRename = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            butTbResetCamera = new System.Windows.Forms.ToolStripButton();
            butTbAxis = new System.Windows.Forms.ToolStripButton();
            butTbWireframe = new System.Windows.Forms.ToolStripButton();
            butTbAlpha = new System.Windows.Forms.ToolStripButton();
            butTbBilinear = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            butTbFindSelectedTexture = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            butTbRotateTexture = new System.Windows.Forms.ToolStripButton();
            butTbMirrorTexture = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            butHide = new System.Windows.Forms.ToolStripButton();
            panelEditing = new DarkUI.Controls.DarkPanel();
            panelTexturing = new DarkUI.Controls.DarkSectionPanel();
            panelTextureMap = new Controls.PanelTextureMap();
            panelTextureTools = new DarkUI.Controls.DarkPanel();
            butMaterialEditor = new DarkUI.Controls.DarkButton();
            butAnimationRanges = new DarkUI.Controls.DarkButton();
            panelTexturingTools = new DarkUI.Controls.DarkPanel();
            comboCurrentTexture = new TombLib.Controls.DarkSearchableComboBox();
            darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            butAddTexture = new System.Windows.Forms.ToolStripDropDownButton();
            addEmbeddedTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addExternalTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            butReplaceTexture = new System.Windows.Forms.ToolStripDropDownButton();
            replaceWithEmbeddedTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceWithExternalTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            butExportTexture = new System.Windows.Forms.ToolStripButton();
            butDeleteTexture = new System.Windows.Forms.ToolStripButton();
            butAllTextures = new DarkUI.Controls.DarkButton();
            panelTree = new DarkUI.Controls.DarkPanel();
            toolTip = new System.Windows.Forms.ToolTip(components);
            statusLabel = new DarkUI.Controls.DarkLabel();
            panelEditingTools.SuspendLayout();
            tabsModes.SuspendLayout();
            tabFaceAttributes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudShineStrength).BeginInit();
            tabVertexRemap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVertexNum).BeginInit();
            tabVertexShadesAndNormals.SuspendLayout();
            tabVertexEffects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMove).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGlow).BeginInit();
            tabVertexWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue1).BeginInit();
            tabSphere.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSphereRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereX).BeginInit();
            darkSectionPanel2.SuspendLayout();
            panelSearchTree.SuspendLayout();
            panelMain.SuspendLayout();
            panelCenter.SuspendLayout();
            topBar.SuspendLayout();
            panelEditing.SuspendLayout();
            panelTexturing.SuspendLayout();
            panelTextureTools.SuspendLayout();
            panelTexturingTools.SuspendLayout();
            darkToolStrip1.SuspendLayout();
            panelTree.SuspendLayout();
            SuspendLayout();
            // 
            // lstMeshes
            // 
            lstMeshes.Dock = System.Windows.Forms.DockStyle.Fill;
            lstMeshes.ExpandOnDoubleClick = false;
            lstMeshes.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lstMeshes.Location = new System.Drawing.Point(1, 55);
            lstMeshes.MaxDragChange = 20;
            lstMeshes.Name = "lstMeshes";
            lstMeshes.ShowIcons = true;
            lstMeshes.Size = new System.Drawing.Size(257, 509);
            lstMeshes.TabIndex = 1;
            lstMeshes.Text = "darkTreeView1";
            lstMeshes.SelectedNodesChanged += lstMeshes_SelectedNodesChanged;
            lstMeshes.MouseDoubleClick += lstMeshes_MouseDoubleClick;
            // 
            // panelMesh
            // 
            panelMesh.AllowRendering = true;
            panelMesh.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMesh.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            panelMesh.Location = new System.Drawing.Point(0, 29);
            panelMesh.Name = "panelMesh";
            panelMesh.Size = new System.Drawing.Size(396, 535);
            panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btCancel.Checked = false;
            btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btCancel.Location = new System.Drawing.Point(915, 575);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(81, 23);
            btCancel.TabIndex = 52;
            btCancel.Text = "Cancel";
            btCancel.Click += btCancel_Click;
            // 
            // btOk
            // 
            btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btOk.Checked = false;
            btOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btOk.Location = new System.Drawing.Point(828, 575);
            btOk.Name = "btOk";
            btOk.Size = new System.Drawing.Size(81, 23);
            btOk.TabIndex = 53;
            btOk.Text = "OK";
            btOk.Click += btOk_Click;
            // 
            // panelEditingTools
            // 
            panelEditingTools.Controls.Add(tabsModes);
            panelEditingTools.Controls.Add(cbExtra);
            panelEditingTools.Controls.Add(cbEditingMode);
            panelEditingTools.Controls.Add(darkLabel2);
            panelEditingTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelEditingTools.Location = new System.Drawing.Point(4, 404);
            panelEditingTools.Name = "panelEditingTools";
            panelEditingTools.SectionHeader = "Editing tools";
            panelEditingTools.Size = new System.Drawing.Size(327, 161);
            panelEditingTools.TabIndex = 54;
            // 
            // tabsModes
            // 
            tabsModes.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            tabsModes.Anchor = System.Windows.Forms.AnchorStyles.Top;
            tabsModes.Controls.Add(tabFaceAttributes);
            tabsModes.Controls.Add(tabVertexRemap);
            tabsModes.Controls.Add(tabVertexShadesAndNormals);
            tabsModes.Controls.Add(tabVertexEffects);
            tabsModes.Controls.Add(tabVertexWeights);
            tabsModes.Controls.Add(tabSphere);
            tabsModes.Location = new System.Drawing.Point(1, 58);
            tabsModes.Multiline = true;
            tabsModes.Name = "tabsModes";
            tabsModes.SelectedIndex = 0;
            tabsModes.Size = new System.Drawing.Size(324, 102);
            tabsModes.TabIndex = 7;
            // 
            // tabFaceAttributes
            // 
            tabFaceAttributes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabFaceAttributes.Controls.Add(cbTexture);
            tabFaceAttributes.Controls.Add(cbBlendMode);
            tabFaceAttributes.Controls.Add(butApplyToAllFaces);
            tabFaceAttributes.Controls.Add(cbBlend);
            tabFaceAttributes.Controls.Add(nudShineStrength);
            tabFaceAttributes.Controls.Add(cbSheen);
            tabFaceAttributes.Controls.Add(butDoubleSide);
            tabFaceAttributes.Location = new System.Drawing.Point(4, 4);
            tabFaceAttributes.Name = "tabFaceAttributes";
            tabFaceAttributes.Padding = new System.Windows.Forms.Padding(3);
            tabFaceAttributes.Size = new System.Drawing.Size(316, 58);
            tabFaceAttributes.TabIndex = 1;
            tabFaceAttributes.Text = "Face attributes";
            // 
            // cbTexture
            // 
            cbTexture.AutoSize = true;
            cbTexture.Checked = true;
            cbTexture.CheckState = System.Windows.Forms.CheckState.Checked;
            cbTexture.Location = new System.Drawing.Point(7, 34);
            cbTexture.Name = "cbTexture";
            cbTexture.Size = new System.Drawing.Size(63, 17);
            cbTexture.TabIndex = 15;
            cbTexture.Text = "Texture";
            toolTip.SetToolTip(cbTexture, "Apply textures to faces");
            // 
            // cbBlendMode
            // 
            cbBlendMode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbBlendMode.FormattingEnabled = true;
            cbBlendMode.Location = new System.Drawing.Point(81, 3);
            cbBlendMode.Name = "cbBlendMode";
            cbBlendMode.Size = new System.Drawing.Size(203, 23);
            cbBlendMode.TabIndex = 11;
            // 
            // butApplyToAllFaces
            // 
            butApplyToAllFaces.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butApplyToAllFaces.Checked = false;
            butApplyToAllFaces.Location = new System.Drawing.Point(215, 32);
            butApplyToAllFaces.Name = "butApplyToAllFaces";
            butApplyToAllFaces.Size = new System.Drawing.Size(99, 23);
            butApplyToAllFaces.TabIndex = 9;
            butApplyToAllFaces.Text = "Apply to all";
            toolTip.SetToolTip(butApplyToAllFaces, "Apply specified face attributes to all faces");
            butApplyToAllFaces.Click += butApplyToAllFaces_Click;
            // 
            // cbBlend
            // 
            cbBlend.AutoSize = true;
            cbBlend.Checked = true;
            cbBlend.CheckState = System.Windows.Forms.CheckState.Checked;
            cbBlend.Location = new System.Drawing.Point(7, 5);
            cbBlend.Name = "cbBlend";
            cbBlend.Size = new System.Drawing.Size(75, 17);
            cbBlend.TabIndex = 14;
            cbBlend.Text = "Blending:";
            toolTip.SetToolTip(cbBlend, "Apply blend mode and double-sided attribute to faces");
            // 
            // nudShineStrength
            // 
            nudShineStrength.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            nudShineStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudShineStrength.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudShineStrength.Location = new System.Drawing.Point(146, 32);
            nudShineStrength.LoopValues = false;
            nudShineStrength.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudShineStrength.Name = "nudShineStrength";
            nudShineStrength.Size = new System.Drawing.Size(65, 23);
            nudShineStrength.TabIndex = 7;
            toolTip.SetToolTip(nudShineStrength, "Shininess value on the range from 0 to 63");
            // 
            // cbSheen
            // 
            cbSheen.AutoSize = true;
            cbSheen.Checked = true;
            cbSheen.CheckState = System.Windows.Forms.CheckState.Checked;
            cbSheen.Location = new System.Drawing.Point(81, 34);
            cbSheen.Name = "cbSheen";
            cbSheen.Size = new System.Drawing.Size(61, 17);
            cbSheen.TabIndex = 13;
            cbSheen.Text = "Sheen:";
            toolTip.SetToolTip(cbSheen, "Apply sheen attribute to faces");
            // 
            // butDoubleSide
            // 
            butDoubleSide.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butDoubleSide.Checked = false;
            butDoubleSide.Image = Properties.Resources.texture_DoubleSided_1_16;
            butDoubleSide.Location = new System.Drawing.Point(290, 3);
            butDoubleSide.Name = "butDoubleSide";
            butDoubleSide.Size = new System.Drawing.Size(24, 23);
            butDoubleSide.TabIndex = 12;
            butDoubleSide.Tag = "";
            toolTip.SetToolTip(butDoubleSide, "Toggle double-sided attribute");
            butDoubleSide.Click += butDoubleSide_Click;
            // 
            // tabVertexRemap
            // 
            tabVertexRemap.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabVertexRemap.Controls.Add(butAutoFit);
            tabVertexRemap.Controls.Add(nudVertexNum);
            tabVertexRemap.Controls.Add(darkLabel1);
            tabVertexRemap.Controls.Add(butFindVertex);
            tabVertexRemap.Controls.Add(butRemapVertex);
            tabVertexRemap.Location = new System.Drawing.Point(4, 4);
            tabVertexRemap.Name = "tabVertexRemap";
            tabVertexRemap.Size = new System.Drawing.Size(316, 58);
            tabVertexRemap.TabIndex = 0;
            tabVertexRemap.Text = "Vertex remap";
            // 
            // butAutoFit
            // 
            butAutoFit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butAutoFit.Checked = false;
            butAutoFit.Location = new System.Drawing.Point(2, 32);
            butAutoFit.Name = "butAutoFit";
            butAutoFit.Size = new System.Drawing.Size(312, 23);
            butAutoFit.TabIndex = 6;
            butAutoFit.Text = "Automatic fit";
            toolTip.SetToolTip(butAutoFit, "Try to automatically remap all vertices sitting on mesh holes.\r\nFor legacy engines, you still have to manually remap hair hole.");
            butAutoFit.Click += butAutoFit_Click;
            // 
            // nudVertexNum
            // 
            nudVertexNum.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            nudVertexNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudVertexNum.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVertexNum.Location = new System.Drawing.Point(91, 3);
            nudVertexNum.LoopValues = false;
            nudVertexNum.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudVertexNum.Name = "nudVertexNum";
            nudVertexNum.Size = new System.Drawing.Size(62, 23);
            nudVertexNum.TabIndex = 2;
            toolTip.SetToolTip(nudVertexNum, "Vertex number to operate with");
            nudVertexNum.KeyDown += nudVertexNum_KeyDown;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(3, 6);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(84, 13);
            darkLabel1.TabIndex = 1;
            darkLabel1.Text = "Vertex number:";
            // 
            // butFindVertex
            // 
            butFindVertex.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butFindVertex.Checked = false;
            butFindVertex.Image = Properties.Resources.general_search_16;
            butFindVertex.Location = new System.Drawing.Point(159, 3);
            butFindVertex.Name = "butFindVertex";
            butFindVertex.Size = new System.Drawing.Size(77, 23);
            butFindVertex.TabIndex = 5;
            butFindVertex.Text = "Jump to";
            butFindVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butFindVertex, "Jump to specified vertex number");
            butFindVertex.Click += butFindVertex_Click;
            // 
            // butRemapVertex
            // 
            butRemapVertex.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butRemapVertex.Checked = false;
            butRemapVertex.Image = Properties.Resources.replace_16;
            butRemapVertex.Location = new System.Drawing.Point(240, 3);
            butRemapVertex.Name = "butRemapVertex";
            butRemapVertex.Size = new System.Drawing.Size(74, 23);
            butRemapVertex.TabIndex = 3;
            butRemapVertex.Text = "Remap";
            butRemapVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butRemapVertex, "Remap selected vertex to specified vertex number");
            butRemapVertex.Click += butRemapVertex_Click;
            // 
            // tabVertexShadesAndNormals
            // 
            tabVertexShadesAndNormals.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabVertexShadesAndNormals.Controls.Add(butApplyShadesToAllVertices);
            tabVertexShadesAndNormals.Controls.Add(panelColor);
            tabVertexShadesAndNormals.Controls.Add(butRecalcNormalsAvg);
            tabVertexShadesAndNormals.Controls.Add(darkLabel7);
            tabVertexShadesAndNormals.Controls.Add(darkLabel10);
            tabVertexShadesAndNormals.Controls.Add(butRecalcNormals);
            tabVertexShadesAndNormals.Location = new System.Drawing.Point(4, 4);
            tabVertexShadesAndNormals.Name = "tabVertexShadesAndNormals";
            tabVertexShadesAndNormals.Padding = new System.Windows.Forms.Padding(3);
            tabVertexShadesAndNormals.Size = new System.Drawing.Size(316, 58);
            tabVertexShadesAndNormals.TabIndex = 3;
            tabVertexShadesAndNormals.Text = "Normals & shades";
            // 
            // butApplyShadesToAllVertices
            // 
            butApplyShadesToAllVertices.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butApplyShadesToAllVertices.Checked = false;
            butApplyShadesToAllVertices.Location = new System.Drawing.Point(220, 32);
            butApplyShadesToAllVertices.Name = "butApplyShadesToAllVertices";
            butApplyShadesToAllVertices.Size = new System.Drawing.Size(94, 23);
            butApplyShadesToAllVertices.TabIndex = 18;
            butApplyShadesToAllVertices.Text = "Apply to all";
            toolTip.SetToolTip(butApplyShadesToAllVertices, "Apply specified vertex shade to all vertices");
            butApplyShadesToAllVertices.Click += butApplyShadesToAllVertices_Click;
            // 
            // panelColor
            // 
            panelColor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            panelColor.Location = new System.Drawing.Point(120, 32);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(94, 23);
            panelColor.TabIndex = 16;
            panelColor.Tag = "";
            toolTip.SetToolTip(panelColor, "Vertex shade (color) to apply");
            panelColor.MouseDown += panelColor_MouseDown;
            // 
            // butRecalcNormalsAvg
            // 
            butRecalcNormalsAvg.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butRecalcNormalsAvg.Checked = false;
            butRecalcNormalsAvg.Location = new System.Drawing.Point(220, 3);
            butRecalcNormalsAvg.Name = "butRecalcNormalsAvg";
            butRecalcNormalsAvg.Size = new System.Drawing.Size(94, 23);
            butRecalcNormalsAvg.TabIndex = 23;
            butRecalcNormalsAvg.Text = "Averaged";
            toolTip.SetToolTip(butRecalcNormalsAvg, "Averaged normals recalculation is more rough and straighforward way,\r\nbut still may be useful in some cases.");
            butRecalcNormalsAvg.Click += butRecalcNormalsAvg_Click;
            // 
            // darkLabel7
            // 
            darkLabel7.AutoSize = true;
            darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel7.Location = new System.Drawing.Point(3, 35);
            darkLabel7.Name = "darkLabel7";
            darkLabel7.Size = new System.Drawing.Size(110, 13);
            darkLabel7.TabIndex = 21;
            darkLabel7.Text = "Vertex shade (color):";
            // 
            // darkLabel10
            // 
            darkLabel10.AutoSize = true;
            darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel10.Location = new System.Drawing.Point(3, 8);
            darkLabel10.Name = "darkLabel10";
            darkLabel10.Size = new System.Drawing.Size(112, 13);
            darkLabel10.TabIndex = 22;
            darkLabel10.Text = "Recalculate normals:";
            // 
            // butRecalcNormals
            // 
            butRecalcNormals.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butRecalcNormals.Checked = false;
            butRecalcNormals.Location = new System.Drawing.Point(120, 3);
            butRecalcNormals.Name = "butRecalcNormals";
            butRecalcNormals.Size = new System.Drawing.Size(94, 23);
            butRecalcNormals.TabIndex = 20;
            butRecalcNormals.Text = "Weighted";
            toolTip.SetToolTip(butRecalcNormals, "Weighted normals recalculation gives better results\r\nbecause it takes angles between different faces in consideration.");
            butRecalcNormals.Click += butRecalcNormals_Click;
            // 
            // tabVertexEffects
            // 
            tabVertexEffects.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabVertexEffects.Controls.Add(butPreview);
            tabVertexEffects.Controls.Add(butConvertFromShades);
            tabVertexEffects.Controls.Add(nudMove);
            tabVertexEffects.Controls.Add(darkLabel5);
            tabVertexEffects.Controls.Add(nudGlow);
            tabVertexEffects.Controls.Add(darkLabel4);
            tabVertexEffects.Controls.Add(butApplyToAllVertices);
            tabVertexEffects.Location = new System.Drawing.Point(4, 4);
            tabVertexEffects.Name = "tabVertexEffects";
            tabVertexEffects.Padding = new System.Windows.Forms.Padding(3);
            tabVertexEffects.Size = new System.Drawing.Size(316, 58);
            tabVertexEffects.TabIndex = 2;
            tabVertexEffects.Text = "Vertex effects";
            // 
            // butPreview
            // 
            butPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butPreview.Checked = false;
            butPreview.Image = Properties.Resources.play_16;
            butPreview.Location = new System.Drawing.Point(222, 3);
            butPreview.Name = "butPreview";
            butPreview.Size = new System.Drawing.Size(92, 23);
            butPreview.TabIndex = 21;
            butPreview.Tag = "";
            butPreview.Text = "Preview";
            butPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butPreview, "Preview all effects");
            butPreview.Click += butPreview_Click;
            // 
            // butConvertFromShades
            // 
            butConvertFromShades.Checked = false;
            butConvertFromShades.Location = new System.Drawing.Point(4, 32);
            butConvertFromShades.Name = "butConvertFromShades";
            butConvertFromShades.Size = new System.Drawing.Size(212, 23);
            butConvertFromShades.TabIndex = 20;
            butConvertFromShades.Text = "Convert from shades";
            toolTip.SetToolTip(butConvertFromShades, "Convert vertex effects from legacy workflow involving vertex shades.");
            butConvertFromShades.Click += butConvertFromShades_Click;
            // 
            // nudMove
            // 
            nudMove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudMove.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudMove.Location = new System.Drawing.Point(151, 3);
            nudMove.LoopValues = false;
            nudMove.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudMove.Name = "nudMove";
            nudMove.Size = new System.Drawing.Size(65, 23);
            nudMove.TabIndex = 14;
            toolTip.SetToolTip(nudMove, "For legacy engines, any value above 0 results in constant movement only for merged statics.\r\nIn such case, movement strength is derived from room effect strength.\r\n");
            // 
            // darkLabel5
            // 
            darkLabel5.AutoSize = true;
            darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel5.Location = new System.Drawing.Point(109, 6);
            darkLabel5.Name = "darkLabel5";
            darkLabel5.Size = new System.Drawing.Size(38, 13);
            darkLabel5.TabIndex = 13;
            darkLabel5.Text = "Move:";
            // 
            // nudGlow
            // 
            nudGlow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudGlow.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudGlow.Location = new System.Drawing.Point(40, 3);
            nudGlow.LoopValues = false;
            nudGlow.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudGlow.Name = "nudGlow";
            nudGlow.Size = new System.Drawing.Size(65, 23);
            nudGlow.TabIndex = 12;
            toolTip.SetToolTip(nudGlow, "For legacy engines, any value above 0 results in constant glow only for merged statics.\r\nIn such case, glow strength is derived from room effect strength.");
            // 
            // darkLabel4
            // 
            darkLabel4.AutoSize = true;
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel4.Location = new System.Drawing.Point(1, 6);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(37, 13);
            darkLabel4.TabIndex = 11;
            darkLabel4.Text = "Glow:";
            // 
            // butApplyToAllVertices
            // 
            butApplyToAllVertices.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butApplyToAllVertices.Checked = false;
            butApplyToAllVertices.Location = new System.Drawing.Point(222, 32);
            butApplyToAllVertices.Name = "butApplyToAllVertices";
            butApplyToAllVertices.Size = new System.Drawing.Size(92, 23);
            butApplyToAllVertices.TabIndex = 10;
            butApplyToAllVertices.Text = "Apply to all";
            toolTip.SetToolTip(butApplyToAllVertices, "Apply specified vertex attributes to all faces");
            butApplyToAllVertices.Click += butApplyToAllVertices_Click;
            // 
            // tabVertexWeights
            // 
            tabVertexWeights.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabVertexWeights.Controls.Add(butApplyWeightsToAllVertices);
            tabVertexWeights.Controls.Add(nudWeightIndex4);
            tabVertexWeights.Controls.Add(nudWeightIndex3);
            tabVertexWeights.Controls.Add(nudWeightIndex2);
            tabVertexWeights.Controls.Add(nudWeightIndex1);
            tabVertexWeights.Controls.Add(nudWeightValue4);
            tabVertexWeights.Controls.Add(nudWeightValue3);
            tabVertexWeights.Controls.Add(nudWeightValue2);
            tabVertexWeights.Controls.Add(nudWeightValue1);
            tabVertexWeights.Controls.Add(darkLabel12);
            tabVertexWeights.Controls.Add(darkLabel11);
            tabVertexWeights.Location = new System.Drawing.Point(4, 4);
            tabVertexWeights.Name = "tabVertexWeights";
            tabVertexWeights.Padding = new System.Windows.Forms.Padding(3);
            tabVertexWeights.Size = new System.Drawing.Size(316, 58);
            tabVertexWeights.TabIndex = 5;
            tabVertexWeights.Text = "Vertex weights";
            // 
            // butApplyWeightsToAllVertices
            // 
            butApplyWeightsToAllVertices.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butApplyWeightsToAllVertices.Checked = false;
            butApplyWeightsToAllVertices.Location = new System.Drawing.Point(284, 3);
            butApplyWeightsToAllVertices.Name = "butApplyWeightsToAllVertices";
            butApplyWeightsToAllVertices.Size = new System.Drawing.Size(30, 52);
            butApplyWeightsToAllVertices.TabIndex = 34;
            butApplyWeightsToAllVertices.Text = "Fill";
            toolTip.SetToolTip(butApplyWeightsToAllVertices, "Apply current weights to all vertices");
            butApplyWeightsToAllVertices.Click += butApplyWeightsToAllVertices_Click;
            // 
            // nudWeightIndex4
            // 
            nudWeightIndex4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightIndex4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightIndex4.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightIndex4.Location = new System.Drawing.Point(223, 3);
            nudWeightIndex4.LoopValues = false;
            nudWeightIndex4.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudWeightIndex4.Name = "nudWeightIndex4";
            nudWeightIndex4.Size = new System.Drawing.Size(56, 23);
            nudWeightIndex4.TabIndex = 33;
            toolTip.SetToolTip(nudWeightIndex4, "Bone index");
            // 
            // nudWeightIndex3
            // 
            nudWeightIndex3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightIndex3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightIndex3.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightIndex3.Location = new System.Drawing.Point(162, 3);
            nudWeightIndex3.LoopValues = false;
            nudWeightIndex3.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudWeightIndex3.Name = "nudWeightIndex3";
            nudWeightIndex3.Size = new System.Drawing.Size(56, 23);
            nudWeightIndex3.TabIndex = 32;
            toolTip.SetToolTip(nudWeightIndex3, "Bone index");
            // 
            // nudWeightIndex2
            // 
            nudWeightIndex2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightIndex2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightIndex2.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightIndex2.Location = new System.Drawing.Point(101, 3);
            nudWeightIndex2.LoopValues = false;
            nudWeightIndex2.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudWeightIndex2.Name = "nudWeightIndex2";
            nudWeightIndex2.Size = new System.Drawing.Size(56, 23);
            nudWeightIndex2.TabIndex = 31;
            toolTip.SetToolTip(nudWeightIndex2, "Bone index");
            // 
            // nudWeightIndex1
            // 
            nudWeightIndex1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightIndex1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightIndex1.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightIndex1.Location = new System.Drawing.Point(40, 3);
            nudWeightIndex1.LoopValues = false;
            nudWeightIndex1.Maximum = new decimal(new int[] { 63, 0, 0, 0 });
            nudWeightIndex1.Name = "nudWeightIndex1";
            nudWeightIndex1.Size = new System.Drawing.Size(56, 23);
            nudWeightIndex1.TabIndex = 30;
            toolTip.SetToolTip(nudWeightIndex1, "Bone index");
            // 
            // nudWeightValue4
            // 
            nudWeightValue4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightValue4.DecimalPlaces = 4;
            nudWeightValue4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightValue4.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            nudWeightValue4.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 196608 });
            nudWeightValue4.Location = new System.Drawing.Point(223, 32);
            nudWeightValue4.LoopValues = false;
            nudWeightValue4.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightValue4.Name = "nudWeightValue4";
            nudWeightValue4.Size = new System.Drawing.Size(56, 23);
            nudWeightValue4.TabIndex = 29;
            toolTip.SetToolTip(nudWeightValue4, "Bone weight");
            // 
            // nudWeightValue3
            // 
            nudWeightValue3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightValue3.DecimalPlaces = 4;
            nudWeightValue3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightValue3.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            nudWeightValue3.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 196608 });
            nudWeightValue3.Location = new System.Drawing.Point(162, 32);
            nudWeightValue3.LoopValues = false;
            nudWeightValue3.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightValue3.Name = "nudWeightValue3";
            nudWeightValue3.Size = new System.Drawing.Size(56, 23);
            nudWeightValue3.TabIndex = 28;
            toolTip.SetToolTip(nudWeightValue3, "Bone weight");
            // 
            // nudWeightValue2
            // 
            nudWeightValue2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightValue2.DecimalPlaces = 4;
            nudWeightValue2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightValue2.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            nudWeightValue2.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 196608 });
            nudWeightValue2.Location = new System.Drawing.Point(101, 32);
            nudWeightValue2.LoopValues = false;
            nudWeightValue2.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightValue2.Name = "nudWeightValue2";
            nudWeightValue2.Size = new System.Drawing.Size(56, 23);
            nudWeightValue2.TabIndex = 27;
            toolTip.SetToolTip(nudWeightValue2, "Bone weight");
            // 
            // nudWeightValue1
            // 
            nudWeightValue1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudWeightValue1.DecimalPlaces = 4;
            nudWeightValue1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudWeightValue1.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            nudWeightValue1.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 196608 });
            nudWeightValue1.Location = new System.Drawing.Point(40, 32);
            nudWeightValue1.LoopValues = false;
            nudWeightValue1.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWeightValue1.Name = "nudWeightValue1";
            nudWeightValue1.Size = new System.Drawing.Size(56, 23);
            nudWeightValue1.TabIndex = 26;
            toolTip.SetToolTip(nudWeightValue1, "Bone weight");
            // 
            // darkLabel12
            // 
            darkLabel12.AutoSize = true;
            darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel12.Location = new System.Drawing.Point(1, 35);
            darkLabel12.Name = "darkLabel12";
            darkLabel12.Size = new System.Drawing.Size(38, 13);
            darkLabel12.TabIndex = 16;
            darkLabel12.Text = "Value:";
            // 
            // darkLabel11
            // 
            darkLabel11.AutoSize = true;
            darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel11.Location = new System.Drawing.Point(1, 6);
            darkLabel11.Name = "darkLabel11";
            darkLabel11.Size = new System.Drawing.Size(36, 13);
            darkLabel11.TabIndex = 15;
            darkLabel11.Text = "Bone:";
            // 
            // tabSphere
            // 
            tabSphere.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            tabSphere.Controls.Add(darkLabel6);
            tabSphere.Controls.Add(darkLabel3);
            tabSphere.Controls.Add(butResetSphere);
            tabSphere.Controls.Add(nudSphereRadius);
            tabSphere.Controls.Add(darkLabel9);
            tabSphere.Controls.Add(nudSphereZ);
            tabSphere.Controls.Add(nudSphereY);
            tabSphere.Controls.Add(nudSphereX);
            tabSphere.Controls.Add(darkLabel8);
            tabSphere.Location = new System.Drawing.Point(4, 4);
            tabSphere.Name = "tabSphere";
            tabSphere.Padding = new System.Windows.Forms.Padding(3);
            tabSphere.Size = new System.Drawing.Size(316, 58);
            tabSphere.TabIndex = 4;
            tabSphere.Text = "Sphere";
            // 
            // darkLabel6
            // 
            darkLabel6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel6.AutoSize = true;
            darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel6.Location = new System.Drawing.Point(212, 6);
            darkLabel6.Name = "darkLabel6";
            darkLabel6.Size = new System.Drawing.Size(16, 13);
            darkLabel6.TabIndex = 23;
            darkLabel6.Text = "Z:";
            // 
            // darkLabel3
            // 
            darkLabel3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            darkLabel3.AutoSize = true;
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(107, 6);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(15, 13);
            darkLabel3.TabIndex = 22;
            darkLabel3.Text = "Y:";
            // 
            // butResetSphere
            // 
            butResetSphere.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butResetSphere.Checked = false;
            butResetSphere.Location = new System.Drawing.Point(117, 32);
            butResetSphere.Name = "butResetSphere";
            butResetSphere.Size = new System.Drawing.Size(197, 23);
            butResetSphere.TabIndex = 9;
            butResetSphere.Text = "Recalculate";
            toolTip.SetToolTip(butResetSphere, "Calculates average sphere which encloses the mesh");
            butResetSphere.Click += butResetSphere_Click;
            // 
            // nudSphereRadius
            // 
            nudSphereRadius.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudSphereRadius.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudSphereRadius.Location = new System.Drawing.Point(50, 32);
            nudSphereRadius.LoopValues = false;
            nudSphereRadius.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudSphereRadius.Name = "nudSphereRadius";
            nudSphereRadius.Size = new System.Drawing.Size(61, 23);
            nudSphereRadius.TabIndex = 15;
            nudSphereRadius.ValueChanged += nudSphereData_ValueChanged;
            nudSphereRadius.Validated += nudSphereData_ValueChanged;
            // 
            // darkLabel9
            // 
            darkLabel9.AutoSize = true;
            darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel9.Location = new System.Drawing.Point(3, 35);
            darkLabel9.Name = "darkLabel9";
            darkLabel9.Size = new System.Drawing.Size(45, 13);
            darkLabel9.TabIndex = 14;
            darkLabel9.Text = "Radius:";
            // 
            // nudSphereZ
            // 
            nudSphereZ.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudSphereZ.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudSphereZ.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudSphereZ.Location = new System.Drawing.Point(229, 3);
            nudSphereZ.LoopValues = false;
            nudSphereZ.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudSphereZ.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudSphereZ.Name = "nudSphereZ";
            nudSphereZ.Size = new System.Drawing.Size(85, 23);
            nudSphereZ.TabIndex = 13;
            nudSphereZ.ValueChanged += nudSphereData_ValueChanged;
            nudSphereZ.Validated += nudSphereData_ValueChanged;
            // 
            // nudSphereY
            // 
            nudSphereY.Anchor = System.Windows.Forms.AnchorStyles.Top;
            nudSphereY.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudSphereY.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudSphereY.Location = new System.Drawing.Point(124, 3);
            nudSphereY.LoopValues = false;
            nudSphereY.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudSphereY.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudSphereY.Name = "nudSphereY";
            nudSphereY.Size = new System.Drawing.Size(85, 23);
            nudSphereY.TabIndex = 11;
            nudSphereY.ValueChanged += nudSphereData_ValueChanged;
            nudSphereY.Validated += nudSphereData_ValueChanged;
            // 
            // nudSphereX
            // 
            nudSphereX.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            nudSphereX.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudSphereX.Location = new System.Drawing.Point(20, 3);
            nudSphereX.LoopValues = false;
            nudSphereX.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudSphereX.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudSphereX.Name = "nudSphereX";
            nudSphereX.Size = new System.Drawing.Size(85, 23);
            nudSphereX.TabIndex = 21;
            nudSphereX.ValueChanged += nudSphereData_ValueChanged;
            nudSphereX.Validated += nudSphereData_ValueChanged;
            // 
            // darkLabel8
            // 
            darkLabel8.AutoSize = true;
            darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel8.Location = new System.Drawing.Point(4, 6);
            darkLabel8.Name = "darkLabel8";
            darkLabel8.Size = new System.Drawing.Size(16, 13);
            darkLabel8.TabIndex = 8;
            darkLabel8.Text = "X:";
            // 
            // cbExtra
            // 
            cbExtra.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cbExtra.Location = new System.Drawing.Point(208, 32);
            cbExtra.Name = "cbExtra";
            cbExtra.Size = new System.Drawing.Size(112, 17);
            cbExtra.TabIndex = 4;
            cbExtra.Text = "All numbers";
            toolTip.SetToolTip(cbExtra, "Show additional screen information");
            cbExtra.CheckedChanged += cbVertexNumbers_CheckedChanged;
            // 
            // cbEditingMode
            // 
            cbEditingMode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbEditingMode.FormattingEnabled = true;
            cbEditingMode.Location = new System.Drawing.Point(45, 29);
            cbEditingMode.Name = "cbEditingMode";
            cbEditingMode.Size = new System.Drawing.Size(156, 23);
            cbEditingMode.TabIndex = 9;
            toolTip.SetToolTip(cbEditingMode, "Mesh editor operation mode");
            cbEditingMode.SelectedIndexChanged += cbEditingMode_SelectedIndexChanged;
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(4, 32);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(40, 13);
            darkLabel2.TabIndex = 8;
            darkLabel2.Text = "Mode:";
            // 
            // darkSectionPanel2
            // 
            darkSectionPanel2.Controls.Add(lstMeshes);
            darkSectionPanel2.Controls.Add(panelSearchTree);
            darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            darkSectionPanel2.Location = new System.Drawing.Point(0, 0);
            darkSectionPanel2.Name = "darkSectionPanel2";
            darkSectionPanel2.SectionHeader = "Mesh list";
            darkSectionPanel2.Size = new System.Drawing.Size(259, 565);
            darkSectionPanel2.TabIndex = 55;
            // 
            // panelSearchTree
            // 
            panelSearchTree.Controls.Add(butSearchMeshes);
            panelSearchTree.Controls.Add(tbSearchMeshes);
            panelSearchTree.Dock = System.Windows.Forms.DockStyle.Top;
            panelSearchTree.Location = new System.Drawing.Point(1, 25);
            panelSearchTree.Name = "panelSearchTree";
            panelSearchTree.Size = new System.Drawing.Size(257, 30);
            panelSearchTree.TabIndex = 2;
            // 
            // butSearchMeshes
            // 
            butSearchMeshes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSearchMeshes.Checked = false;
            butSearchMeshes.Image = Properties.Resources.general_search_16;
            butSearchMeshes.Location = new System.Drawing.Point(233, 3);
            butSearchMeshes.Name = "butSearchMeshes";
            butSearchMeshes.Selectable = false;
            butSearchMeshes.Size = new System.Drawing.Size(24, 23);
            butSearchMeshes.TabIndex = 10;
            toolTip.SetToolTip(butSearchMeshes, "Search for meshes by name");
            butSearchMeshes.Click += butSearchMeshes_Click;
            // 
            // tbSearchMeshes
            // 
            tbSearchMeshes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tbSearchMeshes.Location = new System.Drawing.Point(0, 3);
            tbSearchMeshes.Name = "tbSearchMeshes";
            tbSearchMeshes.Size = new System.Drawing.Size(233, 23);
            tbSearchMeshes.TabIndex = 11;
            tbSearchMeshes.KeyDown += tbSearchMeshes_KeyDown;
            // 
            // panelMain
            // 
            panelMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panelMain.Controls.Add(panelCenter);
            panelMain.Controls.Add(panelEditing);
            panelMain.Controls.Add(panelTree);
            panelMain.Location = new System.Drawing.Point(6, 4);
            panelMain.Name = "panelMain";
            panelMain.Size = new System.Drawing.Size(990, 565);
            panelMain.TabIndex = 56;
            // 
            // panelCenter
            // 
            panelCenter.Controls.Add(panelMesh);
            panelCenter.Controls.Add(topBar);
            panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCenter.Location = new System.Drawing.Point(263, 0);
            panelCenter.Name = "panelCenter";
            panelCenter.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            panelCenter.Size = new System.Drawing.Size(396, 565);
            panelCenter.TabIndex = 57;
            // 
            // topBar
            // 
            topBar.AutoSize = false;
            topBar.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            topBar.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            topBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            topBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { butTbUndo, butTbRedo, toolStripSeparator3, butTbImport, butTbExport, butTbRename, toolStripSeparator1, butTbResetCamera, butTbAxis, butTbWireframe, butTbAlpha, butTbBilinear, toolStripSeparator2, butTbFindSelectedTexture, toolStripSeparator4, butTbRotateTexture, butTbMirrorTexture, toolStripSeparator5, butHide });
            topBar.Location = new System.Drawing.Point(0, 1);
            topBar.Name = "topBar";
            topBar.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            topBar.Size = new System.Drawing.Size(396, 28);
            topBar.TabIndex = 59;
            topBar.Text = "darkToolStrip1";
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
            butTbUndo.ToolTipText = "Undo (Ctrl+Z)";
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
            butTbRedo.ToolTipText = "Redo (Ctrl+Y)";
            butTbRedo.Click += butTbRedo_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
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
            butTbImport.ToolTipText = "Import mesh";
            butTbImport.Click += butTbImport_Click;
            // 
            // butTbExport
            // 
            butTbExport.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbExport.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbExport.Image = Properties.Resources.general_Export_16;
            butTbExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbExport.Name = "butTbExport";
            butTbExport.Size = new System.Drawing.Size(23, 25);
            butTbExport.ToolTipText = "Export mesh";
            butTbExport.Click += butTbExport_Click;
            // 
            // butTbRename
            // 
            butTbRename.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbRename.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbRename.Image = Properties.Resources.edit_16;
            butTbRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbRename.Name = "butTbRename";
            butTbRename.Size = new System.Drawing.Size(23, 25);
            butTbRename.ToolTipText = "Rename mesh";
            butTbRename.Click += butTbRename_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
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
            // butTbAxis
            // 
            butTbAxis.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbAxis.CheckOnClick = true;
            butTbAxis.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbAxis.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbAxis.Image = Properties.Resources.general_axis_16;
            butTbAxis.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbAxis.Name = "butTbAxis";
            butTbAxis.Size = new System.Drawing.Size(23, 25);
            butTbAxis.ToolTipText = "Draw grid and axis";
            butTbAxis.CheckedChanged += butTbAxis_Click;
            // 
            // butTbWireframe
            // 
            butTbWireframe.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbWireframe.CheckOnClick = true;
            butTbWireframe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbWireframe.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbWireframe.Image = Properties.Resources.wireframe_16;
            butTbWireframe.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbWireframe.Name = "butTbWireframe";
            butTbWireframe.Size = new System.Drawing.Size(23, 25);
            butTbWireframe.ToolTipText = "Draw in wireframe mode";
            butTbWireframe.CheckedChanged += butTbWireframe_Click;
            // 
            // butTbAlpha
            // 
            butTbAlpha.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbAlpha.CheckOnClick = true;
            butTbAlpha.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbAlpha.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbAlpha.Image = Properties.Resources.actions_AlphaTest_16;
            butTbAlpha.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbAlpha.Name = "butTbAlpha";
            butTbAlpha.Size = new System.Drawing.Size(23, 25);
            butTbAlpha.ToolTipText = "Toggle transparency";
            butTbAlpha.CheckedChanged += butTbAlpha_Click;
            // 
            // butTbBilinear
            // 
            butTbBilinear.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbBilinear.CheckOnClick = true;
            butTbBilinear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbBilinear.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbBilinear.Image = Properties.Resources.general_blur_16;
            butTbBilinear.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbBilinear.Name = "butTbBilinear";
            butTbBilinear.Size = new System.Drawing.Size(23, 25);
            butTbBilinear.ToolTipText = "Toggle bilinear filter";
            butTbBilinear.CheckedChanged += butTbBilinear_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // butTbFindSelectedTexture
            // 
            butTbFindSelectedTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbFindSelectedTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbFindSelectedTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbFindSelectedTexture.Image = Properties.Resources.general_search_texture_16;
            butTbFindSelectedTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbFindSelectedTexture.Name = "butTbFindSelectedTexture";
            butTbFindSelectedTexture.Size = new System.Drawing.Size(23, 25);
            butTbFindSelectedTexture.ToolTipText = "Search for mesh by selected texture area (Ctrl+F)";
            butTbFindSelectedTexture.Click += butTbFindSelectedTexture_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            // 
            // butTbRotateTexture
            // 
            butTbRotateTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbRotateTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbRotateTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbRotateTexture.Image = Properties.Resources.general_Rotate;
            butTbRotateTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbRotateTexture.Name = "butTbRotateTexture";
            butTbRotateTexture.Size = new System.Drawing.Size(23, 25);
            butTbRotateTexture.ToolTipText = "Rotate current texture (~)";
            butTbRotateTexture.Click += butTbRotateTexture_Click;
            // 
            // butTbMirrorTexture
            // 
            butTbMirrorTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butTbMirrorTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butTbMirrorTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butTbMirrorTexture.Image = Properties.Resources.general_Mirror;
            butTbMirrorTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butTbMirrorTexture.Name = "butTbMirrorTexture";
            butTbMirrorTexture.Size = new System.Drawing.Size(23, 25);
            butTbMirrorTexture.ToolTipText = "Mirror current texture (Shift+~)";
            butTbMirrorTexture.Click += butTbMirrorTexture_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            toolStripSeparator5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            toolStripSeparator5.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            // 
            // butHide
            // 
            butHide.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butHide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butHide.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butHide.Image = (System.Drawing.Image)resources.GetObject("butHide.Image");
            butHide.ImageTransparentColor = System.Drawing.Color.Magenta;
            butHide.Name = "butHide";
            butHide.Size = new System.Drawing.Size(23, 25);
            butHide.Text = "Hide mesh if skinned mesh is used for the parent object";
            butHide.Click += butHide_Click;
            // 
            // panelEditing
            // 
            panelEditing.Controls.Add(panelTexturing);
            panelEditing.Controls.Add(panelEditingTools);
            panelEditing.Dock = System.Windows.Forms.DockStyle.Right;
            panelEditing.Location = new System.Drawing.Point(659, 0);
            panelEditing.Name = "panelEditing";
            panelEditing.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            panelEditing.Size = new System.Drawing.Size(331, 565);
            panelEditing.TabIndex = 56;
            // 
            // panelTexturing
            // 
            panelTexturing.Controls.Add(panelTextureMap);
            panelTexturing.Controls.Add(panelTextureTools);
            panelTexturing.Controls.Add(panelTexturingTools);
            panelTexturing.Dock = System.Windows.Forms.DockStyle.Fill;
            panelTexturing.Location = new System.Drawing.Point(4, 0);
            panelTexturing.Name = "panelTexturing";
            panelTexturing.SectionHeader = "Texturing";
            panelTexturing.Size = new System.Drawing.Size(327, 404);
            panelTexturing.TabIndex = 0;
            // 
            // panelTextureMap
            // 
            panelTextureMap.Dock = System.Windows.Forms.DockStyle.Fill;
            panelTextureMap.Location = new System.Drawing.Point(1, 55);
            panelTextureMap.Name = "panelTextureMap";
            panelTextureMap.Size = new System.Drawing.Size(325, 323);
            panelTextureMap.TabIndex = 0;
            // 
            // panelTextureTools
            // 
            panelTextureTools.Controls.Add(butMaterialEditor);
            panelTextureTools.Controls.Add(butAnimationRanges);
            panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelTextureTools.Location = new System.Drawing.Point(1, 378);
            panelTextureTools.Name = "panelTextureTools";
            panelTextureTools.Size = new System.Drawing.Size(325, 25);
            panelTextureTools.TabIndex = 12;
            // 
            // butMaterialEditor
            // 
            butMaterialEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butMaterialEditor.Checked = false;
            butMaterialEditor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butMaterialEditor.Location = new System.Drawing.Point(0, 2);
            butMaterialEditor.Name = "butMaterialEditor";
            butMaterialEditor.Size = new System.Drawing.Size(160, 23);
            butMaterialEditor.TabIndex = 12;
            butMaterialEditor.Tag = "EditMaterials";
            butMaterialEditor.Text = "Materials";
            toolTip.SetToolTip(butMaterialEditor, "Edit materials");
            butMaterialEditor.Click += butMaterialEditor_Click;
            // 
            // butAnimationRanges
            // 
            butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            butAnimationRanges.Checked = false;
            butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butAnimationRanges.Location = new System.Drawing.Point(165, 2);
            butAnimationRanges.Name = "butAnimationRanges";
            butAnimationRanges.Size = new System.Drawing.Size(160, 23);
            butAnimationRanges.TabIndex = 11;
            butAnimationRanges.Tag = "EditAnimationRanges";
            butAnimationRanges.Text = "Animated textures";
            toolTip.SetToolTip(butAnimationRanges, "Edit animated texture sets");
            butAnimationRanges.Click += butAnimationRanges_Click;
            // 
            // panelTexturingTools
            // 
            panelTexturingTools.Controls.Add(comboCurrentTexture);
            panelTexturingTools.Controls.Add(darkToolStrip1);
            panelTexturingTools.Controls.Add(butAllTextures);
            panelTexturingTools.Dock = System.Windows.Forms.DockStyle.Top;
            panelTexturingTools.Location = new System.Drawing.Point(1, 25);
            panelTexturingTools.Name = "panelTexturingTools";
            panelTexturingTools.Size = new System.Drawing.Size(325, 30);
            panelTexturingTools.TabIndex = 1;
            // 
            // comboCurrentTexture
            // 
            comboCurrentTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboCurrentTexture.Location = new System.Drawing.Point(30, 3);
            comboCurrentTexture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboCurrentTexture.Name = "comboCurrentTexture";
            comboCurrentTexture.Size = new System.Drawing.Size(183, 23);
            comboCurrentTexture.TabIndex = 4;
            comboCurrentTexture.SelectedValueChanged += comboCurrentTexture_SelectedValueChanged;
            // 
            // darkToolStrip1
            // 
            darkToolStrip1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkToolStrip1.AutoSize = false;
            darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkToolStrip1.CanOverflow = false;
            darkToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { butAddTexture, butReplaceTexture, butExportTexture, butDeleteTexture });
            darkToolStrip1.Location = new System.Drawing.Point(213, 1);
            darkToolStrip1.Name = "darkToolStrip1";
            darkToolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            darkToolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            darkToolStrip1.Size = new System.Drawing.Size(111, 28);
            darkToolStrip1.TabIndex = 9;
            darkToolStrip1.Text = "darkToolStrip1";
            // 
            // butAddTexture
            // 
            butAddTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butAddTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butAddTexture.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addEmbeddedTextureToolStripMenuItem, addExternalTextureToolStripMenuItem });
            butAddTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butAddTexture.Image = Properties.Resources.general_plus_math_16;
            butAddTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butAddTexture.Name = "butAddTexture";
            butAddTexture.Size = new System.Drawing.Size(29, 25);
            // 
            // addEmbeddedTextureToolStripMenuItem
            // 
            addEmbeddedTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            addEmbeddedTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            addEmbeddedTextureToolStripMenuItem.Name = "addEmbeddedTextureToolStripMenuItem";
            addEmbeddedTextureToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            addEmbeddedTextureToolStripMenuItem.Text = "Add embedded texture";
            addEmbeddedTextureToolStripMenuItem.Click += addEmbeddedTextureToolStripMenuItem_Click;
            // 
            // addExternalTextureToolStripMenuItem
            // 
            addExternalTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            addExternalTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            addExternalTextureToolStripMenuItem.Name = "addExternalTextureToolStripMenuItem";
            addExternalTextureToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            addExternalTextureToolStripMenuItem.Text = "Add external texture";
            addExternalTextureToolStripMenuItem.Click += addExternalTextureToolStripMenuItem_Click;
            // 
            // butReplaceTexture
            // 
            butReplaceTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butReplaceTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butReplaceTexture.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { replaceWithEmbeddedTextureToolStripMenuItem, replaceWithExternalTextureToolStripMenuItem });
            butReplaceTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butReplaceTexture.Image = Properties.Resources.actions_refresh_16;
            butReplaceTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butReplaceTexture.Name = "butReplaceTexture";
            butReplaceTexture.Size = new System.Drawing.Size(29, 25);
            // 
            // replaceWithEmbeddedTextureToolStripMenuItem
            // 
            replaceWithEmbeddedTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            replaceWithEmbeddedTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            replaceWithEmbeddedTextureToolStripMenuItem.Name = "replaceWithEmbeddedTextureToolStripMenuItem";
            replaceWithEmbeddedTextureToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            replaceWithEmbeddedTextureToolStripMenuItem.Text = "Replace with embedded texture";
            replaceWithEmbeddedTextureToolStripMenuItem.Click += replaceWithEmbeddedTextureToolStripMenuItem_Click;
            // 
            // replaceWithExternalTextureToolStripMenuItem
            // 
            replaceWithExternalTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            replaceWithExternalTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            replaceWithExternalTextureToolStripMenuItem.Name = "replaceWithExternalTextureToolStripMenuItem";
            replaceWithExternalTextureToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            replaceWithExternalTextureToolStripMenuItem.Text = "Replace with external texture";
            replaceWithExternalTextureToolStripMenuItem.Click += replaceWithExternalTextureToolStripMenuItem_Click;
            // 
            // butExportTexture
            // 
            butExportTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butExportTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butExportTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butExportTexture.Image = Properties.Resources.general_Export_16;
            butExportTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butExportTexture.Name = "butExportTexture";
            butExportTexture.Size = new System.Drawing.Size(23, 25);
            butExportTexture.Text = "Export current texture to file";
            butExportTexture.Click += butExportTexture_Click;
            // 
            // butDeleteTexture
            // 
            butDeleteTexture.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            butDeleteTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            butDeleteTexture.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            butDeleteTexture.Image = Properties.Resources.trash_16;
            butDeleteTexture.ImageTransparentColor = System.Drawing.Color.Magenta;
            butDeleteTexture.Name = "butDeleteTexture";
            butDeleteTexture.Size = new System.Drawing.Size(23, 25);
            butDeleteTexture.Text = "Delete texture";
            butDeleteTexture.Click += butDeleteTexture_Click;
            // 
            // butAllTextures
            // 
            butAllTextures.Checked = false;
            butAllTextures.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butAllTextures.Image = Properties.Resources.actions_DrawAllRooms_16;
            butAllTextures.Location = new System.Drawing.Point(1, 3);
            butAllTextures.Name = "butAllTextures";
            butAllTextures.Size = new System.Drawing.Size(24, 23);
            butAllTextures.TabIndex = 8;
            butAllTextures.Tag = "";
            toolTip.SetToolTip(butAllTextures, "List all textures from wad");
            butAllTextures.Click += butAllTextures_Click;
            // 
            // panelTree
            // 
            panelTree.Controls.Add(darkSectionPanel2);
            panelTree.Dock = System.Windows.Forms.DockStyle.Left;
            panelTree.Location = new System.Drawing.Point(0, 0);
            panelTree.Name = "panelTree";
            panelTree.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            panelTree.Size = new System.Drawing.Size(263, 565);
            panelTree.TabIndex = 0;
            // 
            // statusLabel
            // 
            statusLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            statusLabel.AutoSize = true;
            statusLabel.ForeColor = System.Drawing.Color.DarkGray;
            statusLabel.Location = new System.Drawing.Point(3, 580);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(285, 13);
            statusLabel.TabIndex = 57;
            statusLabel.Text = "This label will contain useful info about current mesh.";
            // 
            // FormMeshEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1001, 605);
            Controls.Add(panelMain);
            Controls.Add(statusLabel);
            Controls.Add(btCancel);
            Controls.Add(btOk);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1000, 640);
            Name = "FormMeshEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Mesh editor";
            panelEditingTools.ResumeLayout(false);
            panelEditingTools.PerformLayout();
            tabsModes.ResumeLayout(false);
            tabFaceAttributes.ResumeLayout(false);
            tabFaceAttributes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudShineStrength).EndInit();
            tabVertexRemap.ResumeLayout(false);
            tabVertexRemap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudVertexNum).EndInit();
            tabVertexShadesAndNormals.ResumeLayout(false);
            tabVertexShadesAndNormals.PerformLayout();
            tabVertexEffects.ResumeLayout(false);
            tabVertexEffects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMove).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGlow).EndInit();
            tabVertexWeights.ResumeLayout(false);
            tabVertexWeights.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex4).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex3).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightIndex1).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue4).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue3).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWeightValue1).EndInit();
            tabSphere.ResumeLayout(false);
            tabSphere.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSphereRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSphereX).EndInit();
            darkSectionPanel2.ResumeLayout(false);
            panelSearchTree.ResumeLayout(false);
            panelSearchTree.PerformLayout();
            panelMain.ResumeLayout(false);
            panelCenter.ResumeLayout(false);
            topBar.ResumeLayout(false);
            topBar.PerformLayout();
            panelEditing.ResumeLayout(false);
            panelTexturing.ResumeLayout(false);
            panelTextureTools.ResumeLayout(false);
            panelTexturingTools.ResumeLayout(false);
            darkToolStrip1.ResumeLayout(false);
            darkToolStrip1.PerformLayout();
            panelTree.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.PanelRenderingMesh panelMesh;
        private DarkUI.Controls.DarkTreeView lstMeshes;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkSectionPanel panelEditingTools;
        private DarkUI.Controls.DarkButton butRemapVertex;
        private DarkUI.Controls.DarkNumericUpDown nudVertexNum;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkCheckBox cbExtra;
        private DarkUI.Controls.DarkButton butFindVertex;
        private DarkUI.Controls.DarkPanel panelMain;
        private DarkUI.Controls.DarkPanel panelTree;
        private TombLib.Controls.DarkTabbedContainer tabsModes;
        private System.Windows.Forms.TabPage tabVertexRemap;
        private System.Windows.Forms.TabPage tabFaceAttributes;
        private DarkUI.Controls.DarkNumericUpDown nudShineStrength;
        private DarkUI.Controls.DarkButton butApplyToAllFaces;
        private System.Windows.Forms.TabPage tabVertexEffects;
        private DarkUI.Controls.DarkButton butApplyToAllVertices;
        private DarkUI.Controls.DarkNumericUpDown nudGlow;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkNumericUpDown nudMove;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkComboBox cbBlendMode;
        private DarkUI.Controls.DarkPanel panelColor;
        private System.Windows.Forms.TabPage tabVertexShadesAndNormals;
        private DarkUI.Controls.DarkButton butApplyShadesToAllVertices;
        private DarkUI.Controls.DarkButton butRecalcNormals;
        private DarkUI.Controls.DarkButton butConvertFromShades;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkComboBox cbEditingMode;
        private DarkUI.Controls.DarkButton butDoubleSide;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butAutoFit;
        private System.Windows.Forms.TabPage tabSphere;
        private DarkUI.Controls.DarkNumericUpDown nudSphereZ;
        private DarkUI.Controls.DarkNumericUpDown nudSphereY;
        private DarkUI.Controls.DarkNumericUpDown nudSphereX;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkButton butResetSphere;
        private DarkUI.Controls.DarkButton butPreview;
        private DarkUI.Controls.DarkPanel panelEditing;
        private DarkUI.Controls.DarkSectionPanel panelTexturing;
        private Controls.PanelTextureMap panelTextureMap;
        private DarkUI.Controls.DarkPanel panelTexturingTools;
        private System.Windows.Forms.ToolStripDropDownButton butAddTexture;
        private System.Windows.Forms.ToolStripButton butDeleteTexture;
        private TombLib.Controls.DarkSearchableComboBox comboCurrentTexture;
        private DarkUI.Controls.DarkCheckBox cbTexture;
        private DarkUI.Controls.DarkCheckBox cbBlend;
        private DarkUI.Controls.DarkCheckBox cbSheen;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkButton butRecalcNormalsAvg;
        private System.Windows.Forms.ToolStripButton butExportTexture;
        private DarkUI.Controls.DarkNumericUpDown nudSphereRadius;
        private DarkUI.Controls.DarkButton butAllTextures;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel statusLabel;
        private DarkUI.Controls.DarkPanel panelCenter;
        private DarkUI.Controls.DarkToolStrip topBar;
        private System.Windows.Forms.ToolStripButton butTbUndo;
        private System.Windows.Forms.ToolStripButton butTbRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton butTbResetCamera;
        private System.Windows.Forms.ToolStripButton butTbWireframe;
        private System.Windows.Forms.ToolStripButton butTbAlpha;
        private System.Windows.Forms.ToolStripButton butTbBilinear;
        private DarkUI.Controls.DarkButton butSearchMeshes;
        private DarkUI.Controls.DarkTextBox tbSearchMeshes;
        private DarkUI.Controls.DarkPanel panelSearchTree;
        private System.Windows.Forms.ToolStripButton butTbAxis;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton butTbMirrorTexture;
        private System.Windows.Forms.ToolStripButton butTbRotateTexture;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton butTbImport;
        private System.Windows.Forms.ToolStripButton butTbExport;
        private System.Windows.Forms.ToolStripButton butTbFindSelectedTexture;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton butTbRename;
        private System.Windows.Forms.ToolStripDropDownButton butReplaceTexture;
        private System.Windows.Forms.ToolStripButton butHide;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.TabPage tabVertexWeights;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkNumericUpDown nudWeightIndex4;
        private DarkUI.Controls.DarkNumericUpDown nudWeightIndex3;
        private DarkUI.Controls.DarkNumericUpDown nudWeightIndex2;
        private DarkUI.Controls.DarkNumericUpDown nudWeightIndex1;
        private DarkUI.Controls.DarkNumericUpDown nudWeightValue4;
        private DarkUI.Controls.DarkNumericUpDown nudWeightValue3;
        private DarkUI.Controls.DarkNumericUpDown nudWeightValue2;
        private DarkUI.Controls.DarkNumericUpDown nudWeightValue1;
		private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkPanel panelTextureTools;
        private DarkUI.Controls.DarkButton butApplyWeightsToAllVertices;
		private DarkUI.Controls.DarkToolStrip darkToolStrip1;
		private System.Windows.Forms.ToolStripMenuItem addEmbeddedTextureToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addExternalTextureToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem replaceWithEmbeddedTextureToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem replaceWithExternalTextureToolStripMenuItem;
        private DarkUI.Controls.DarkButton butMaterialEditor;
    }
}
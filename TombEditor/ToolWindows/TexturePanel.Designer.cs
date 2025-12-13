using TombEditor.Controls;

namespace TombEditor.ToolWindows
{
    partial class TexturePanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panelTextureTools = new System.Windows.Forms.Panel();
            butBumpMaps = new DarkUI.Controls.DarkButton();
            butMirror = new DarkUI.Controls.DarkButton();
            butDoubleSide = new DarkUI.Controls.DarkButton();
            butTextureSounds = new DarkUI.Controls.DarkButton();
            cmbTileSize = new DarkUI.Controls.DarkComboBox();
            butAnimationRanges = new DarkUI.Controls.DarkButton();
            butRotate = new DarkUI.Controls.DarkButton();
            cmbBlending = new DarkUI.Controls.DarkComboBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            butBrowseTexture = new DarkUI.Controls.DarkButton();
            butDeleteTexture = new DarkUI.Controls.DarkButton();
            butMaterialEditor = new DarkUI.Controls.DarkButton();
            textureSelectionPanel = new System.Windows.Forms.Panel();
            butAddTexture = new DarkUI.Controls.DarkButton();
            comboCurrentTexture = new TombLib.Controls.DarkSearchableComboBox();
            panelTextureMap = new PanelTextureMapMain();
            panelMaterials = new DarkUI.Controls.DarkPanel();
            panelTextures = new DarkUI.Controls.DarkPanel();
            panelTextureTools.SuspendLayout();
            textureSelectionPanel.SuspendLayout();
            panelMaterials.SuspendLayout();
            panelTextures.SuspendLayout();
            SuspendLayout();
            // 
            // panelTextureTools
            // 
            panelTextureTools.Controls.Add(butBumpMaps);
            panelTextureTools.Controls.Add(butMirror);
            panelTextureTools.Controls.Add(butDoubleSide);
            panelTextureTools.Controls.Add(butTextureSounds);
            panelTextureTools.Controls.Add(cmbTileSize);
            panelTextureTools.Controls.Add(butAnimationRanges);
            panelTextureTools.Controls.Add(butRotate);
            panelTextureTools.Controls.Add(cmbBlending);
            panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelTextureTools.Location = new System.Drawing.Point(0, 613);
            panelTextureTools.Name = "panelTextureTools";
            panelTextureTools.Size = new System.Drawing.Size(332, 57);
            panelTextureTools.TabIndex = 10;
            // 
            // butBumpMaps
            // 
            butBumpMaps.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butBumpMaps.Checked = false;
            butBumpMaps.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butBumpMaps.Location = new System.Drawing.Point(216, 30);
            butBumpMaps.Name = "butBumpMaps";
            butBumpMaps.Size = new System.Drawing.Size(90, 23);
            butBumpMaps.TabIndex = 12;
            butBumpMaps.Text = "Bumpmaps";
            toolTip.SetToolTip(butBumpMaps, "Edit bumpmaps...");
            butBumpMaps.Click += butBumpMaps_Click;
            // 
            // butMirror
            // 
            butMirror.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butMirror.Checked = false;
            butMirror.Image = Properties.Resources.texture_Mirror;
            butMirror.Location = new System.Drawing.Point(196, 3);
            butMirror.Name = "butMirror";
            butMirror.Size = new System.Drawing.Size(23, 23);
            butMirror.TabIndex = 8;
            butMirror.Tag = "MirrorTexture";
            // 
            // butDoubleSide
            // 
            butDoubleSide.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butDoubleSide.Checked = false;
            butDoubleSide.Image = Properties.Resources.texture_DoubleSided_1_16;
            butDoubleSide.Location = new System.Drawing.Point(26, 3);
            butDoubleSide.Name = "butDoubleSide";
            butDoubleSide.Size = new System.Drawing.Size(24, 23);
            butDoubleSide.TabIndex = 5;
            butDoubleSide.Tag = "SetTextureDoubleSided";
            // 
            // butTextureSounds
            // 
            butTextureSounds.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butTextureSounds.Checked = false;
            butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butTextureSounds.Location = new System.Drawing.Point(121, 30);
            butTextureSounds.Name = "butTextureSounds";
            butTextureSounds.Size = new System.Drawing.Size(90, 23);
            butTextureSounds.TabIndex = 11;
            butTextureSounds.Text = "Sounds";
            toolTip.SetToolTip(butTextureSounds, "Edit texture sounds...");
            butTextureSounds.Click += butTextureSounds_Click;
            // 
            // cmbTileSize
            // 
            cmbTileSize.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cmbTileSize.FormattingEnabled = true;
            cmbTileSize.Location = new System.Drawing.Point(224, 3);
            cmbTileSize.Name = "cmbTileSize";
            cmbTileSize.Size = new System.Drawing.Size(82, 23);
            cmbTileSize.TabIndex = 9;
            toolTip.SetToolTip(cmbTileSize, "Selection tile size");
            cmbTileSize.SelectionChangeCommitted += cmbTileSize_SelectionChangeCommitted;
            // 
            // butAnimationRanges
            // 
            butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butAnimationRanges.Checked = false;
            butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butAnimationRanges.Location = new System.Drawing.Point(26, 30);
            butAnimationRanges.Name = "butAnimationRanges";
            butAnimationRanges.Size = new System.Drawing.Size(90, 23);
            butAnimationRanges.TabIndex = 10;
            butAnimationRanges.Tag = "EditAnimationRanges";
            butAnimationRanges.Text = "Animations";
            // 
            // butRotate
            // 
            butRotate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butRotate.Checked = false;
            butRotate.Image = Properties.Resources.texture_Rotate;
            butRotate.Location = new System.Drawing.Point(169, 3);
            butRotate.Name = "butRotate";
            butRotate.Size = new System.Drawing.Size(23, 23);
            butRotate.TabIndex = 7;
            butRotate.Tag = "RotateTexture";
            // 
            // cmbBlending
            // 
            cmbBlending.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cmbBlending.FormattingEnabled = true;
            cmbBlending.Location = new System.Drawing.Point(54, 3);
            cmbBlending.Name = "cmbBlending";
            cmbBlending.Size = new System.Drawing.Size(110, 23);
            cmbBlending.TabIndex = 6;
            toolTip.SetToolTip(cmbBlending, "Blending mode");
            cmbBlending.SelectedIndexChanged += cmbBlending_SelectedIndexChanged;
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            // 
            // butBrowseTexture
            // 
            butBrowseTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butBrowseTexture.Checked = false;
            butBrowseTexture.Enabled = false;
            butBrowseTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butBrowseTexture.Image = Properties.Resources.actions_refresh_16;
            butBrowseTexture.Location = new System.Drawing.Point(276, 3);
            butBrowseTexture.Name = "butBrowseTexture";
            butBrowseTexture.Size = new System.Drawing.Size(24, 23);
            butBrowseTexture.TabIndex = 2;
            toolTip.SetToolTip(butBrowseTexture, "Replace texture...");
            butBrowseTexture.Click += butBrowseTexture_Click;
            // 
            // butDeleteTexture
            // 
            butDeleteTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butDeleteTexture.Checked = false;
            butDeleteTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butDeleteTexture.Image = Properties.Resources.general_trash_16;
            butDeleteTexture.Location = new System.Drawing.Point(305, 3);
            butDeleteTexture.Name = "butDeleteTexture";
            butDeleteTexture.Size = new System.Drawing.Size(24, 23);
            butDeleteTexture.TabIndex = 3;
            toolTip.SetToolTip(butDeleteTexture, "Delete texture");
            butDeleteTexture.Click += butDeleteTexture_Click;
            // 
            // butMaterialEditor
            // 
            butMaterialEditor.Anchor = System.Windows.Forms.AnchorStyles.Top;
            butMaterialEditor.Checked = false;
            butMaterialEditor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butMaterialEditor.Location = new System.Drawing.Point(26, 0);
            butMaterialEditor.Name = "butMaterialEditor";
            butMaterialEditor.Size = new System.Drawing.Size(280, 23);
            butMaterialEditor.TabIndex = 4;
            butMaterialEditor.Text = "Materials";
            toolTip.SetToolTip(butMaterialEditor, "Edit materials...");
            butMaterialEditor.Click += butMaterialEditor_Click;
            // 
            // textureSelectionPanel
            // 
            textureSelectionPanel.Controls.Add(butBrowseTexture);
            textureSelectionPanel.Controls.Add(butAddTexture);
            textureSelectionPanel.Controls.Add(comboCurrentTexture);
            textureSelectionPanel.Controls.Add(butDeleteTexture);
            textureSelectionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            textureSelectionPanel.Location = new System.Drawing.Point(0, 25);
            textureSelectionPanel.Name = "textureSelectionPanel";
            textureSelectionPanel.Size = new System.Drawing.Size(332, 28);
            textureSelectionPanel.TabIndex = 11;
            // 
            // butAddTexture
            // 
            butAddTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butAddTexture.Checked = false;
            butAddTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            butAddTexture.Image = Properties.Resources.general_plus_math_16;
            butAddTexture.Location = new System.Drawing.Point(247, 3);
            butAddTexture.Name = "butAddTexture";
            butAddTexture.Size = new System.Drawing.Size(24, 23);
            butAddTexture.TabIndex = 1;
            butAddTexture.Tag = "AddTexture";
            // 
            // comboCurrentTexture
            // 
            comboCurrentTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboCurrentTexture.Location = new System.Drawing.Point(3, 3);
            comboCurrentTexture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboCurrentTexture.Name = "comboCurrentTexture";
            comboCurrentTexture.Size = new System.Drawing.Size(239, 23);
            comboCurrentTexture.TabIndex = 0;
            comboCurrentTexture.SelectedValueChanged += comboCurrentTexture_SelectedValueChanged;
            // 
            // panelTextureMap
            // 
            panelTextureMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panelTextureMap.Location = new System.Drawing.Point(3, 3);
            panelTextureMap.Name = "panelTextureMap";
            panelTextureMap.Padding = new System.Windows.Forms.Padding(1, 0, 1, 1);
            panelTextureMap.Size = new System.Drawing.Size(326, 555);
            panelTextureMap.TabIndex = 4;
            panelTextureMap.VisibleTexture = null;
            // 
            // panelMaterials
            // 
            panelMaterials.Controls.Add(butMaterialEditor);
            panelMaterials.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelMaterials.Location = new System.Drawing.Point(0, 670);
            panelMaterials.Name = "panelMaterials";
            panelMaterials.Size = new System.Drawing.Size(332, 27);
            panelMaterials.TabIndex = 13;
            // 
            // panelTextures
            // 
            panelTextures.Controls.Add(panelTextureMap);
            panelTextures.Dock = System.Windows.Forms.DockStyle.Fill;
            panelTextures.Location = new System.Drawing.Point(0, 53);
            panelTextures.Name = "panelTextures";
            panelTextures.Size = new System.Drawing.Size(332, 560);
            panelTextures.TabIndex = 14;
            // 
            // TexturePanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panelTextures);
            Controls.Add(panelTextureTools);
            Controls.Add(panelMaterials);
            Controls.Add(textureSelectionPanel);
            DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            DockText = "Texturing";
            MinimumSize = new System.Drawing.Size(286, 100);
            Name = "TexturePanel";
            SerializationKey = "TexturePanel";
            Size = new System.Drawing.Size(332, 697);
            panelTextureTools.ResumeLayout(false);
            textureSelectionPanel.ResumeLayout(false);
            panelMaterials.ResumeLayout(false);
            panelTextures.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel panelTextureTools;
        private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkButton butTextureSounds;
        private DarkUI.Controls.DarkComboBox cmbBlending;
        private DarkUI.Controls.DarkButton butRotate;
        private DarkUI.Controls.DarkButton butMirror;
        private DarkUI.Controls.DarkButton butDoubleSide;
        private DarkUI.Controls.DarkComboBox cmbTileSize;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel textureSelectionPanel;
        private TombLib.Controls.DarkSearchableComboBox comboCurrentTexture;
        private DarkUI.Controls.DarkButton butAddTexture;
        private DarkUI.Controls.DarkButton butDeleteTexture;
        private DarkUI.Controls.DarkButton butBrowseTexture;
        private DarkUI.Controls.DarkButton butBumpMaps;
        private PanelTextureMapMain panelTextureMap;
		private DarkUI.Controls.DarkButton butMaterialEditor;
        private DarkUI.Controls.DarkPanel panelMaterials;
        private DarkUI.Controls.DarkPanel panelTextures;
    }
}

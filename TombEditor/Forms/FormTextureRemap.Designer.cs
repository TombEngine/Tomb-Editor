namespace TombEditor.Forms
{
    partial class FormTextureRemap
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboSourceTexture = new DarkUI.Controls.DarkComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.sourcePanel = new DarkUI.Controls.DarkSectionPanel();
            this.sourceTextureMap = new TombEditor.Forms.FormTextureRemap.PanelTextureMapForRemap();
            this.destinationPanel = new DarkUI.Controls.DarkSectionPanel();
            this.destinationTextureMap = new TombEditor.Forms.FormTextureRemap.PanelTextureMapForRemap();
            this.comboDestinationTexture = new DarkUI.Controls.DarkComboBox();
            this.cbRestrictToSelectedRooms = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.cbUntextureCompletely = new DarkUI.Controls.DarkCheckBox();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.cbRemapAnimTextures = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.scalingFactor = new DarkUI.Controls.DarkNumericUpDown();
            this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.sourcePanel.SuspendLayout();
            this.destinationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactor)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboSourceTexture
            // 
            this.comboSourceTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSourceTexture.FormattingEnabled = true;
            this.comboSourceTexture.Location = new System.Drawing.Point(4, 28);
            this.comboSourceTexture.Name = "comboSourceTexture";
            this.comboSourceTexture.Size = new System.Drawing.Size(327, 23);
            this.comboSourceTexture.TabIndex = 0;
            this.comboSourceTexture.SelectedValueChanged += new System.EventHandler(this.comboSourceTexture_SelectedValueChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.sourcePanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.destinationPanel, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 118);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(688, 525);
            this.tableLayoutPanel1.TabIndex = 23;
            // 
            // sourcePanel
            // 
            this.sourcePanel.Controls.Add(this.sourceTextureMap);
            this.sourcePanel.Controls.Add(this.comboSourceTexture);
            this.sourcePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcePanel.Location = new System.Drawing.Point(6, 6);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.SectionHeader = "Source";
            this.sourcePanel.Size = new System.Drawing.Size(335, 513);
            this.sourcePanel.TabIndex = 1;
            // 
            // sourceTextureMap
            // 
            this.sourceTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceTextureMap.FormParent = null;
            this.sourceTextureMap.IsDestination = false;
            this.sourceTextureMap.Location = new System.Drawing.Point(3, 55);
            this.sourceTextureMap.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.sourceTextureMap.Name = "sourceTextureMap";
            this.sourceTextureMap.Scaling = 1F;
            this.sourceTextureMap.Size = new System.Drawing.Size(328, 454);
            this.sourceTextureMap.TabIndex = 2;
            // 
            // destinationPanel
            // 
            this.destinationPanel.Controls.Add(this.destinationTextureMap);
            this.destinationPanel.Controls.Add(this.comboDestinationTexture);
            this.destinationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.destinationPanel.Location = new System.Drawing.Point(347, 6);
            this.destinationPanel.Name = "destinationPanel";
            this.destinationPanel.SectionHeader = "Destination";
            this.destinationPanel.Size = new System.Drawing.Size(335, 513);
            this.destinationPanel.TabIndex = 2;
            // 
            // destinationTextureMap
            // 
            this.destinationTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationTextureMap.FormParent = null;
            this.destinationTextureMap.IsDestination = true;
            this.destinationTextureMap.Location = new System.Drawing.Point(4, 55);
            this.destinationTextureMap.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.destinationTextureMap.Name = "destinationTextureMap";
            this.destinationTextureMap.Scaling = 1F;
            this.destinationTextureMap.Size = new System.Drawing.Size(327, 454);
            this.destinationTextureMap.TabIndex = 1;
            // 
            // comboDestinationTexture
            // 
            this.comboDestinationTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboDestinationTexture.FormattingEnabled = true;
            this.comboDestinationTexture.Location = new System.Drawing.Point(4, 28);
            this.comboDestinationTexture.Name = "comboDestinationTexture";
            this.comboDestinationTexture.Size = new System.Drawing.Size(327, 23);
            this.comboDestinationTexture.TabIndex = 0;
            this.comboDestinationTexture.SelectedValueChanged += new System.EventHandler(this.comboDestinationTexture_SelectedValueChanged);
            // 
            // cbRestrictToSelectedRooms
            // 
            this.cbRestrictToSelectedRooms.Checked = true;
            this.cbRestrictToSelectedRooms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRestrictToSelectedRooms.Location = new System.Drawing.Point(9, 69);
            this.cbRestrictToSelectedRooms.Name = "cbRestrictToSelectedRooms";
            this.cbRestrictToSelectedRooms.Size = new System.Drawing.Size(277, 17);
            this.cbRestrictToSelectedRooms.TabIndex = 1;
            this.cbRestrictToSelectedRooms.Text = "Restrict to selected rooms";
            // 
            // darkLabel3
            // 
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(6, 9);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(673, 31);
            this.darkLabel3.TabIndex = 25;
            this.darkLabel3.Text = "Automated tool to remap texture areas throughout the level.\r\nSelect source area a" +
    "nd then move corresponding destination area to remap source to.";
            // 
            // cbUntextureCompletely
            // 
            this.cbUntextureCompletely.Location = new System.Drawing.Point(9, 46);
            this.cbUntextureCompletely.Name = "cbUntextureCompletely";
            this.cbUntextureCompletely.Size = new System.Drawing.Size(156, 17);
            this.cbUntextureCompletely.TabIndex = 0;
            this.cbUntextureCompletely.Text = "Untexture completely";
            this.cbUntextureCompletely.CheckedChanged += new System.EventHandler(this.cbUntextureCompletely_CheckedChanged);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(519, 642);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 4;
            this.butOk.Text = "Remap";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(605, 642);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 5;
            this.butCancel.Text = "Close";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // cbRemapAnimTextures
            // 
            this.cbRemapAnimTextures.Checked = true;
            this.cbRemapAnimTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRemapAnimTextures.Location = new System.Drawing.Point(9, 92);
            this.cbRemapAnimTextures.Name = "cbRemapAnimTextures";
            this.cbRemapAnimTextures.Size = new System.Drawing.Size(277, 17);
            this.cbRemapAnimTextures.TabIndex = 2;
            this.cbRemapAnimTextures.Text = "Remap animated textures too (no undo possible)";
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(559, 93);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(74, 20);
            this.darkLabel1.TabIndex = 26;
            this.darkLabel1.Text = "Scale factor:";
            // 
            // scalingFactor
            // 
            this.scalingFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingFactor.DecimalPlaces = 2;
            this.scalingFactor.IncrementAlternate = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.scalingFactor.Location = new System.Drawing.Point(634, 91);
            this.scalingFactor.LoopValues = false;
            this.scalingFactor.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.scalingFactor.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.scalingFactor.Name = "scalingFactor";
            this.scalingFactor.Size = new System.Drawing.Size(51, 22);
            this.scalingFactor.TabIndex = 27;
            this.scalingFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.scalingFactor.ValueChanged += new System.EventHandler(this.scalingFactor_ValueChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 671);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 5, 0, 3);
            this.statusStrip.Size = new System.Drawing.Size(694, 26);
            this.statusStrip.TabIndex = 28;
            this.statusStrip.Text = "darkStatusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            // 
            // FormTextureRemap
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(694, 697);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.scalingFactor);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.cbUntextureCompletely);
            this.Controls.Add(this.cbRemapAnimTextures);
            this.Controls.Add(this.cbRestrictToSelectedRooms);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "FormTextureRemap";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Texture remapping assistant";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.sourcePanel.ResumeLayout(false);
            this.destinationPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactor)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkComboBox comboSourceTexture;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkUI.Controls.DarkComboBox comboDestinationTexture;
        private PanelTextureMapForRemap destinationTextureMap;
        private DarkUI.Controls.DarkCheckBox cbRestrictToSelectedRooms;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkCheckBox cbUntextureCompletely;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkCheckBox cbRemapAnimTextures;
        private DarkUI.Controls.DarkSectionPanel sourcePanel;
        private DarkUI.Controls.DarkSectionPanel destinationPanel;
        private PanelTextureMapForRemap sourceTextureMap;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown scalingFactor;
        private DarkUI.Controls.DarkStatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
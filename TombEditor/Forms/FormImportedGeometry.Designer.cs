namespace TombEditor.Forms
{
    partial class FormImportedGeometry
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
            components = new System.ComponentModel.Container();
            butCancel = new DarkUI.Controls.DarkButton();
            butOk = new DarkUI.Controls.DarkButton();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            butAssign = new DarkUI.Controls.DarkButton();
            importedGeometryLabel = new DarkUI.Controls.DarkLabel();
            comboLightingModel = new DarkUI.Controls.DarkComboBox();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            butEditMaterials = new DarkUI.Controls.DarkButton();
            cbAlphaTest = new DarkUI.Controls.DarkCheckBox();
            panelColor = new DarkUI.Controls.DarkPanel();
            cbHide = new DarkUI.Controls.DarkCheckBox();
            cbSharpEdges = new DarkUI.Controls.DarkCheckBox();
            importedGeometryManager = new Controls.ImportedGeometryManager();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            darkGroupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(727, 419);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 5;
            butCancel.Text = "Cancel";
            butCancel.Click += butCancel_Click;
            // 
            // butOk
            // 
            butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOk.Checked = false;
            butOk.Location = new System.Drawing.Point(641, 419);
            butOk.Name = "butOk";
            butOk.Size = new System.Drawing.Size(80, 23);
            butOk.TabIndex = 4;
            butOk.Text = "OK";
            butOk.Click += butOk_Click;
            // 
            // darkLabel1
            // 
            darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(4, 9);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(806, 30);
            darkLabel1.TabIndex = 2;
            darkLabel1.Text = "Select a model from the following list or add a new one by pressing plus button.\r\nDouble-click on entry or press 'Assign' to assign the loaded file to this imported geometry object.";
            // 
            // darkLabel2
            // 
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(6, 10);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(95, 17);
            darkLabel2.TabIndex = 2;
            darkLabel2.Text = "Chosen entry:";
            darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // butAssign
            // 
            butAssign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butAssign.Checked = false;
            butAssign.Location = new System.Drawing.Point(697, 8);
            butAssign.Name = "butAssign";
            butAssign.Size = new System.Drawing.Size(98, 23);
            butAssign.TabIndex = 1;
            butAssign.Text = "Assign";
            butAssign.Click += butAssign_Click;
            // 
            // importedGeometryLabel
            // 
            importedGeometryLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            importedGeometryLabel.Location = new System.Drawing.Point(102, 10);
            importedGeometryLabel.Name = "importedGeometryLabel";
            importedGeometryLabel.Size = new System.Drawing.Size(534, 17);
            importedGeometryLabel.TabIndex = 2;
            importedGeometryLabel.Text = "None";
            importedGeometryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboLightingModel
            // 
            comboLightingModel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboLightingModel.FormattingEnabled = true;
            comboLightingModel.Items.AddRange(new object[] { "No lights", "Vertex colors", "Calculate from room lights", "Specified tint only" });
            comboLightingModel.Location = new System.Drawing.Point(102, 36);
            comboLightingModel.Name = "comboLightingModel";
            comboLightingModel.Size = new System.Drawing.Size(147, 23);
            comboLightingModel.TabIndex = 3;
            // 
            // darkLabel4
            // 
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel4.Location = new System.Drawing.Point(6, 37);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(113, 17);
            darkLabel4.TabIndex = 6;
            darkLabel4.Text = "Lighting model:";
            darkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // darkGroupBox1
            // 
            darkGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox1.Controls.Add(butEditMaterials);
            darkGroupBox1.Controls.Add(cbAlphaTest);
            darkGroupBox1.Controls.Add(panelColor);
            darkGroupBox1.Controls.Add(cbHide);
            darkGroupBox1.Controls.Add(cbSharpEdges);
            darkGroupBox1.Controls.Add(butAssign);
            darkGroupBox1.Controls.Add(comboLightingModel);
            darkGroupBox1.Controls.Add(darkLabel4);
            darkGroupBox1.Controls.Add(darkLabel2);
            darkGroupBox1.Controls.Add(importedGeometryLabel);
            darkGroupBox1.Location = new System.Drawing.Point(6, 346);
            darkGroupBox1.Name = "darkGroupBox1";
            darkGroupBox1.Size = new System.Drawing.Size(801, 67);
            darkGroupBox1.TabIndex = 7;
            darkGroupBox1.TabStop = false;
            // 
            // butEditMaterials
            // 
            butEditMaterials.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butEditMaterials.Checked = false;
            butEditMaterials.Location = new System.Drawing.Point(697, 37);
            butEditMaterials.Name = "butEditMaterials";
            butEditMaterials.Size = new System.Drawing.Size(98, 23);
            butEditMaterials.TabIndex = 15;
            butEditMaterials.Text = "Edit materials...";
            butEditMaterials.Click += butEditMaterials_Click;
            // 
            // cbAlphaTest
            // 
            cbAlphaTest.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cbAlphaTest.AutoSize = true;
            cbAlphaTest.Location = new System.Drawing.Point(456, 40);
            cbAlphaTest.Name = "cbAlphaTest";
            cbAlphaTest.Size = new System.Drawing.Size(128, 17);
            cbAlphaTest.TabIndex = 14;
            cbAlphaTest.Text = "Faster alpha testing";
            toolTip1.SetToolTip(cbAlphaTest, "Use simple alpha test instead of blending.\r\nUseful for drawing foliage and grated textures.");
            // 
            // panelColor
            // 
            panelColor.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            panelColor.Location = new System.Drawing.Point(255, 36);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(59, 23);
            panelColor.TabIndex = 13;
            panelColor.Tag = "EditAmbientLight";
            panelColor.Click += panelColor_Click;
            // 
            // cbHide
            // 
            cbHide.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cbHide.AutoSize = true;
            cbHide.Location = new System.Drawing.Point(590, 40);
            cbHide.Name = "cbHide";
            cbHide.Size = new System.Drawing.Size(97, 17);
            cbHide.TabIndex = 8;
            cbHide.Text = "Hide in editor";
            toolTip1.SetToolTip(cbHide, "Hide mesh in editor only.\r\nMesh will be still visible in game.");
            // 
            // cbSharpEdges
            // 
            cbSharpEdges.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cbSharpEdges.AutoSize = true;
            cbSharpEdges.Location = new System.Drawing.Point(330, 40);
            cbSharpEdges.Name = "cbSharpEdges";
            cbSharpEdges.Size = new System.Drawing.Size(120, 17);
            cbSharpEdges.TabIndex = 7;
            cbSharpEdges.Text = "Force sharp edges";
            toolTip1.SetToolTip(cbSharpEdges, "Make all edges of a mesh sharp");
            // 
            // importedGeometryManager
            // 
            importedGeometryManager.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            importedGeometryManager.LevelSettings = null;
            importedGeometryManager.Location = new System.Drawing.Point(6, 40);
            importedGeometryManager.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            importedGeometryManager.Name = "importedGeometryManager";
            importedGeometryManager.SelectedImportedGeometry = null;
            importedGeometryManager.Size = new System.Drawing.Size(801, 305);
            importedGeometryManager.TabIndex = 0;
            importedGeometryManager.MouseDoubleClick += importedGeometryManager_MouseDoubleClick;
            // 
            // FormImportedGeometry
            // 
            AcceptButton = butOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(814, 449);
            Controls.Add(darkGroupBox1);
            Controls.Add(butCancel);
            Controls.Add(butOk);
            Controls.Add(darkLabel1);
            Controls.Add(importedGeometryManager);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(800, 488);
            Name = "FormImportedGeometry";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Imported Geometry Settings";
            darkGroupBox1.ResumeLayout(false);
            darkGroupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Controls.ImportedGeometryManager importedGeometryManager;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butAssign;
        private DarkUI.Controls.DarkLabel importedGeometryLabel;
        private DarkUI.Controls.DarkComboBox comboLightingModel;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbSharpEdges;
        private DarkUI.Controls.DarkCheckBox cbHide;
        private DarkUI.Controls.DarkPanel panelColor;
        private DarkUI.Controls.DarkCheckBox cbAlphaTest;
        private System.Windows.Forms.ToolTip toolTip1;
		private DarkUI.Controls.DarkButton butEditMaterials;
	}
}
namespace TombEditor.Features.DockableViews.ImportedGeometryBrowser
{
    partial class ImportedGeometryBrowser
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
            this.components = new System.ComponentModel.Container();
            this.panelItem = new TombEditor.Controls.PanelRenderingImportedGeometry();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.butAddItem = new DarkUI.Controls.DarkButton();
            this.comboItems = new TombLib.Controls.DarkSearchableComboBox();
            this.panelRightBottom = new System.Windows.Forms.Panel();
            this.butFindItem = new DarkUI.Controls.DarkButton();
            this.butItemDown = new DarkUI.Controls.DarkButton();
            this.butItemUp = new DarkUI.Controls.DarkButton();
            this.panelRight = new System.Windows.Forms.Panel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelHeader.SuspendLayout();
            this.panelRightBottom.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelItem
            // 
            this.panelItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelItem.AnimatePreview = true;
            this.panelItem.AutoSize = true;
            this.panelItem.DrawTransparency = false;
            this.panelItem.Location = new System.Drawing.Point(3, 2);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(279, 165);
            this.panelItem.TabIndex = 62;
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.butAddItem);
            this.panelHeader.Controls.Add(this.comboItems);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 25);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.panelHeader.Size = new System.Drawing.Size(284, 27);
            this.panelHeader.TabIndex = 72;
            // 
            // butAddItem
            // 
            this.butAddItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddItem.Checked = false;
            this.butAddItem.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddItem.Location = new System.Drawing.Point(258, 2);
            this.butAddItem.Name = "butAddItem";
            this.butAddItem.Size = new System.Drawing.Size(24, 23);
            this.butAddItem.TabIndex = 4;
            this.butAddItem.Tag = "AddImportedGeometry";
            // 
            // comboItems
            // 
            this.comboItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboItems.Location = new System.Drawing.Point(3, 2);
            this.comboItems.Name = "comboItems";
            this.comboItems.Size = new System.Drawing.Size(249, 23);
            this.comboItems.TabIndex = 1;
            this.comboItems.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.comboItems_Format);
            this.comboItems.SelectedIndexChanged += new System.EventHandler(this.comboItems_SelectedIndexChanged);
            // 
            // panelRightBottom
            // 
            this.panelRightBottom.Controls.Add(this.butFindItem);
            this.panelRightBottom.Controls.Add(this.butItemDown);
            this.panelRightBottom.Controls.Add(this.butItemUp);
            this.panelRightBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelRightBottom.Location = new System.Drawing.Point(0, 167);
            this.panelRightBottom.Name = "panelRightBottom";
            this.panelRightBottom.Size = new System.Drawing.Size(284, 33);
            this.panelRightBottom.TabIndex = 1;
            // 
            // butFindItem
            // 
            this.butFindItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butFindItem.Checked = false;
            this.butFindItem.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butFindItem.Image = global::TombEditor.Properties.Resources.general_target_16;
            this.butFindItem.Location = new System.Drawing.Point(133, 6);
            this.butFindItem.Name = "butFindItem";
            this.butFindItem.Size = new System.Drawing.Size(92, 23);
            this.butFindItem.TabIndex = 8;
            this.butFindItem.Tag = "";
            this.butFindItem.Text = "Locate item";
            this.butFindItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butFindItem, "Locate item");
            this.butFindItem.Click += new System.EventHandler(this.butFindItem_Click);
            // 
            // butItemDown
            // 
            this.butItemDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butItemDown.Checked = false;
            this.butItemDown.Image = global::TombEditor.Properties.Resources.general_ArrowDown_16;
            this.butItemDown.Location = new System.Drawing.Point(258, 6);
            this.butItemDown.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.butItemDown.Name = "butItemDown";
            this.butItemDown.Size = new System.Drawing.Size(24, 23);
            this.butItemDown.TabIndex = 6;
            this.toolTip.SetToolTip(this.butItemDown, "Next item");
            this.butItemDown.Click += new System.EventHandler(this.butItemDown_Click);
            // 
            // butItemUp
            // 
            this.butItemUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butItemUp.Checked = false;
            this.butItemUp.Image = global::TombEditor.Properties.Resources.general_ArrowUp_16;
            this.butItemUp.Location = new System.Drawing.Point(231, 6);
            this.butItemUp.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.butItemUp.Name = "butItemUp";
            this.butItemUp.Size = new System.Drawing.Size(24, 23);
            this.butItemUp.TabIndex = 5;
            this.toolTip.SetToolTip(this.butItemUp, "Previous item");
            this.butItemUp.Click += new System.EventHandler(this.butItemUp_Click);
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.panelRightBottom);
            this.panelRight.Controls.Add(this.panelItem);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 52);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(284, 200);
            this.panelRight.TabIndex = 73;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // ImportedGeometryBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelHeader);
            this.DockText = "Imported Geometry";
            this.MinimumSize = new System.Drawing.Size(237, 168);
            this.Name = "ImportedGeometryBrowser";
            this.SerializationKey = "ImportedGeometryBrowser";
            this.Size = new System.Drawing.Size(284, 252);
            this.panelHeader.ResumeLayout(false);
            this.panelRightBottom.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelRenderingImportedGeometry panelItem;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelRightBottom;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butItemDown;
        private DarkUI.Controls.DarkButton butItemUp;
        private DarkUI.Controls.DarkButton butAddItem;
        private DarkUI.Controls.DarkButton butFindItem;
        private TombLib.Controls.DarkSearchableComboBox comboItems;
    }
}

namespace TombEditor.Forms
{
    partial class FormPreviewWad
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelItem = new TombEditor.Forms.FormPreviewWad.PanelRenderingItemPreview();
            this.wadTree = new TombLib.Controls.WadTreeView();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panelItem);
            this.panel1.Controls.Add(this.wadTree);
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(675, 392);
            this.panel1.TabIndex = 0;
            // 
            // panelItem
            // 
            this.panelItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelItem.AnimatePreview = true;
            this.panelItem.AutoSize = true;
            this.panelItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelItem.DrawTransparency = false;
            this.panelItem.Editor = null;
            this.panelItem.Location = new System.Drawing.Point(293, 7);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(373, 376);
            this.panelItem.TabIndex = 65;
            // 
            // wadTree
            // 
            this.wadTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.wadTree.Location = new System.Drawing.Point(10, 7);
            this.wadTree.Name = "wadTree";
            this.wadTree.Size = new System.Drawing.Size(277, 376);
            this.wadTree.TabIndex = 64;
            this.wadTree.SelectedWadObjectIdsChanged += new System.EventHandler(this.wadTree_SelectedWadObjectIdsChanged);
            // 
            // FormPreviewWad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.ClientSize = new System.Drawing.Size(683, 400);
            this.Controls.Add(this.panel1);
            this.FlatBorder = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPreviewWad";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormReferencedWadPreview";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private TombLib.Controls.WadTreeView wadTree;
        private PanelRenderingItemPreview panelItem;
    }
}
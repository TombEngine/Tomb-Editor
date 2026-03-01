namespace WadTool
{
    partial class FormLuaProperties
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
            if (disposing)
            {
                _elementHost?.Dispose();
                components?.Dispose();
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
            this.panelButtons = new System.Windows.Forms.Panel();
            this.butReset = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lstObjects = new DarkUI.Controls.DarkListView();
            this.splitter = new System.Windows.Forms.Splitter();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelButtons.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.butReset);
            this.panelButtons.Controls.Add(this.butCancel);
            this.panelButtons.Controls.Add(this.butOK);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 448);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(6);
            this.panelButtons.Size = new System.Drawing.Size(624, 36);
            this.panelButtons.TabIndex = 0;
            this.panelButtons.Layout += new System.Windows.Forms.LayoutEventHandler(this.panelButtons_Layout);
            // 
            // butReset
            // 
            this.butReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butReset.Location = new System.Drawing.Point(6, 7);
            this.butReset.Name = "butReset";
            this.butReset.Padding = new System.Windows.Forms.Padding(0);
            this.butReset.Size = new System.Drawing.Size(80, 23);
            this.butReset.TabIndex = 2;
            this.butReset.Text = "Reset All";
            this.butReset.Click += new System.EventHandler(this.butReset_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(538, 7);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(0);
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(452, 7);
            this.butOK.Name = "butOK";
            this.butOK.Padding = new System.Windows.Forms.Padding(0);
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.lstObjects);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(3);
            this.panelLeft.Size = new System.Drawing.Size(200, 448);
            this.panelLeft.TabIndex = 1;
            // 
            // lstObjects
            // 
            this.lstObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstObjects.Location = new System.Drawing.Point(0, 0);
            this.lstObjects.MultiSelect = false;
            this.lstObjects.Name = "lstObjects";
            this.lstObjects.Size = new System.Drawing.Size(200, 448);
            this.lstObjects.TabIndex = 0;
            this.lstObjects.SelectedIndicesChanged += new System.EventHandler(this.lstObjects_SelectedIndicesChanged);
            // 
            // splitter
            // 
            this.splitter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.splitter.Location = new System.Drawing.Point(200, 0);
            this.splitter.MinExtra = 180;
            this.splitter.MinSize = 120;
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(3, 448);
            this.splitter.TabIndex = 2;
            this.splitter.TabStop = false;
            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(203, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Padding = new System.Windows.Forms.Padding(3);
            this.panelContent.Size = new System.Drawing.Size(421, 448);
            this.panelContent.TabIndex = 3;
            // 
            // FormLuaProperties
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(624, 484);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "FormLuaProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Item Properties";
            this.panelButtons.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelButtons;
        private DarkUI.Controls.DarkButton butReset;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private System.Windows.Forms.Panel panelLeft;
        private DarkUI.Controls.DarkListView lstObjects;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.Panel panelContent;
    }
}

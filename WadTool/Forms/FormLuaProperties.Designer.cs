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
			panelButtons = new System.Windows.Forms.Panel();
			butReset = new DarkUI.Controls.DarkButton();
			butCancel = new DarkUI.Controls.DarkButton();
			butOK = new DarkUI.Controls.DarkButton();
			panelLeft = new System.Windows.Forms.Panel();
			lstObjects = new DarkUI.Controls.DarkListView();
			splitter = new System.Windows.Forms.Splitter();
			panelContent = new System.Windows.Forms.Panel();
			panelButtons.SuspendLayout();
			panelLeft.SuspendLayout();
			SuspendLayout();
			// 
			// panelButtons
			// 
			panelButtons.Controls.Add(butReset);
			panelButtons.Controls.Add(butCancel);
			panelButtons.Controls.Add(butOK);
			panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			panelButtons.Location = new System.Drawing.Point(5, 449);
			panelButtons.Name = "panelButtons";
			panelButtons.Padding = new System.Windows.Forms.Padding(6);
			panelButtons.Size = new System.Drawing.Size(614, 30);
			panelButtons.TabIndex = 0;
			// 
			// butReset
			// 
			butReset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			butReset.Checked = false;
			butReset.Location = new System.Drawing.Point(3, 7);
			butReset.Name = "butReset";
			butReset.Size = new System.Drawing.Size(80, 23);
			butReset.TabIndex = 2;
			butReset.Text = "Reset All";
			butReset.Click += butReset_Click;
			// 
			// butCancel
			// 
			butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(534, 7);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 1;
			butCancel.Text = "Cancel";
			butCancel.Click += butCancel_Click;
			// 
			// butOK
			// 
			butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butOK.Checked = false;
			butOK.Location = new System.Drawing.Point(448, 7);
			butOK.Name = "butOK";
			butOK.Size = new System.Drawing.Size(80, 23);
			butOK.TabIndex = 0;
			butOK.Text = "OK";
			butOK.Click += butOK_Click;
			// 
			// panelLeft
			// 
			panelLeft.Controls.Add(lstObjects);
			panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
			panelLeft.Location = new System.Drawing.Point(5, 5);
			panelLeft.Name = "panelLeft";
			panelLeft.Padding = new System.Windows.Forms.Padding(3);
			panelLeft.Size = new System.Drawing.Size(200, 444);
			panelLeft.TabIndex = 1;
			// 
			// lstObjects
			// 
			lstObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			lstObjects.Location = new System.Drawing.Point(3, 3);
			lstObjects.Margin = new System.Windows.Forms.Padding(0);
			lstObjects.Name = "lstObjects";
			lstObjects.Size = new System.Drawing.Size(194, 438);
			lstObjects.TabIndex = 0;
			lstObjects.SelectedIndicesChanged += lstObjects_SelectedIndicesChanged;
			// 
			// splitter
			// 
			splitter.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			splitter.Location = new System.Drawing.Point(205, 5);
			splitter.MinExtra = 180;
			splitter.MinSize = 120;
			splitter.Name = "splitter";
			splitter.Size = new System.Drawing.Size(3, 444);
			splitter.TabIndex = 2;
			splitter.TabStop = false;
			// 
			// panelContent
			// 
			panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			panelContent.Location = new System.Drawing.Point(208, 5);
			panelContent.Margin = new System.Windows.Forms.Padding(0);
			panelContent.Name = "panelContent";
			panelContent.Size = new System.Drawing.Size(411, 444);
			panelContent.TabIndex = 3;
			// 
			// FormLuaProperties
			// 
			AcceptButton = butOK;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(624, 484);
			Controls.Add(panelContent);
			Controls.Add(splitter);
			Controls.Add(panelLeft);
			Controls.Add(panelButtons);
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(500, 350);
			Name = "FormLuaProperties";
			Padding = new System.Windows.Forms.Padding(5);
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Item Properties";
			panelButtons.ResumeLayout(false);
			panelLeft.ResumeLayout(false);
			ResumeLayout(false);
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

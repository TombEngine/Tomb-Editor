namespace TombIDE.ScriptingStudio.Forms
{
	partial class FormFileCreation
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.button_Create = new DarkUI.Controls.DarkButton();
			this.comboBox_FileFormat = new DarkUI.Controls.DarkComboBox();
			this.label_FileName = new DarkUI.Controls.DarkLabel();
			this.label_Format = new DarkUI.Controls.DarkLabel();
			this.label_Where = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.textBox_NewFileName = new DarkUI.Controls.DarkTextBox();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Cancel
			// 
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(382, 10);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.button_Cancel.TabIndex = 2;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_Create
			// 
			this.button_Create.Checked = false;
			this.button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Create.Location = new System.Drawing.Point(302, 10);
			this.button_Create.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Create.Name = "button_Create";
			this.button_Create.Size = new System.Drawing.Size(75, 23);
			this.button_Create.TabIndex = 1;
			this.button_Create.Text = "Create";
			this.button_Create.Click += new System.EventHandler(this.button_Create_Click);
			// 
			// comboBox_FileFormat
			// 
			this.comboBox_FileFormat.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.comboBox_FileFormat.FormattingEnabled = true;
			this.comboBox_FileFormat.Items.AddRange(new object[] {
            ".TXT (Text file)",
            ".JSON5 (TR1X / TR2X File)",
            ".LUA (Lua file)"});
			this.comboBox_FileFormat.Location = new System.Drawing.Point(71, 370);
			this.comboBox_FileFormat.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
			this.comboBox_FileFormat.Name = "comboBox_FileFormat";
			this.comboBox_FileFormat.Size = new System.Drawing.Size(386, 23);
			this.comboBox_FileFormat.TabIndex = 2;
			// 
			// label_FileName
			// 
			this.label_FileName.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label_FileName.AutoSize = true;
			this.label_FileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_FileName.Location = new System.Drawing.Point(4, 342);
			this.label_FileName.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
			this.label_FileName.Name = "label_FileName";
			this.label_FileName.Size = new System.Drawing.Size(60, 13);
			this.label_FileName.TabIndex = 0;
			this.label_FileName.Text = "File Name:";
			// 
			// label_Format
			// 
			this.label_Format.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label_Format.AutoSize = true;
			this.label_Format.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Format.Location = new System.Drawing.Point(4, 373);
			this.label_Format.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.label_Format.Name = "label_Format";
			this.label_Format.Size = new System.Drawing.Size(67, 13);
			this.label_Format.TabIndex = 3;
			this.label_Format.Text = "File Format:";
			// 
			// label_Where
			// 
			this.label_Where.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label_Where.AutoSize = true;
			this.label_Where.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Where.Location = new System.Drawing.Point(8, 8);
			this.label_Where.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label_Where.Name = "label_Where";
			this.label_Where.Size = new System.Drawing.Size(94, 13);
			this.label_Where.TabIndex = 5;
			this.label_Where.Text = "Where to Create:";
			// 
			// panel_01
			// 
			this.panel_01.Controls.Add(this.label_Where);
			this.panel_01.Controls.Add(this.treeView);
			this.panel_01.Controls.Add(this.label_Format);
			this.panel_01.Controls.Add(this.comboBox_FileFormat);
			this.panel_01.Controls.Add(this.textBox_NewFileName);
			this.panel_01.Controls.Add(this.label_FileName);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(464, 399);
			this.panel_01.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.ItemHeight = 30;
			this.treeView.Location = new System.Drawing.Point(-1, 27);
			this.treeView.MaxDragChange = 30;
			this.treeView.Name = "treeView";
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(464, 300);
			this.treeView.TabIndex = 4;
			// 
			// textBox_NewFileName
			// 
			this.textBox_NewFileName.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.textBox_NewFileName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_NewFileName.Location = new System.Drawing.Point(71, 335);
			this.textBox_NewFileName.Margin = new System.Windows.Forms.Padding(0, 3, 6, 6);
			this.textBox_NewFileName.Name = "textBox_NewFileName";
			this.textBox_NewFileName.Size = new System.Drawing.Size(386, 29);
			this.textBox_NewFileName.TabIndex = 1;
			// 
			// panel_02
			// 
			this.panel_02.Controls.Add(this.button_Cancel);
			this.panel_02.Controls.Add(this.button_Create);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 399);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(464, 42);
			this.panel_02.TabIndex = 2;
			// 
			// FormFileCreation
			// 
			this.AcceptButton = this.button_Create;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(464, 441);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormFileCreation";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.panel_01.ResumeLayout(false);
			this.panel_01.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkComboBox comboBox_FileFormat;
		private DarkUI.Controls.DarkLabel label_FileName;
		private DarkUI.Controls.DarkLabel label_Format;
		private DarkUI.Controls.DarkLabel label_Where;
		private DarkUI.Controls.DarkTextBox textBox_NewFileName;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
	}
}
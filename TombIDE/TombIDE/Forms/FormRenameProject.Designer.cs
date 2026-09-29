namespace TombIDE
{
	partial class FormRenameProject
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
            this.button_Apply = new DarkUI.Controls.DarkButton();
            this.button_Cancel = new DarkUI.Controls.DarkButton();
            this.checkBox_RenameDirectory = new DarkUI.Controls.DarkCheckBox();
            this.checkBox_RenameTrproj = new DarkUI.Controls.DarkCheckBox();
            this.panel_02 = new System.Windows.Forms.Panel();
            this.label = new DarkUI.Controls.DarkLabel();
            this.textBox_NewName = new DarkUI.Controls.DarkTextBox();
            this.panel_01 = new System.Windows.Forms.Panel();
            this.panel_02.SuspendLayout();
            this.panel_01.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Apply
            // 
            this.button_Apply.Checked = false;
            this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_Apply.Location = new System.Drawing.Point(300, 10);
            this.button_Apply.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
            this.button_Apply.Name = "button_Apply";
            this.button_Apply.Size = new System.Drawing.Size(75, 23);
            this.button_Apply.TabIndex = 2;
            this.button_Apply.Text = "Apply";
            this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Checked = false;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(381, 10);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "Cancel";
            // 
            // checkBox_RenameDirectory
            // 
            this.checkBox_RenameDirectory.Location = new System.Drawing.Point(6, 11);
            this.checkBox_RenameDirectory.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
            this.checkBox_RenameDirectory.Name = "checkBox_RenameDirectory";
            this.checkBox_RenameDirectory.Size = new System.Drawing.Size(185, 23);
            this.checkBox_RenameDirectory.TabIndex = 0;
            this.checkBox_RenameDirectory.Text = "Rename project directory as well";
            // 
            // checkBox_RenameTrproj
            // 
            this.checkBox_RenameTrproj.Checked = true;
            this.checkBox_RenameTrproj.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_RenameTrproj.Location = new System.Drawing.Point(6, 34);
            this.checkBox_RenameTrproj.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.checkBox_RenameTrproj.Name = "checkBox_RenameTrproj";
            this.checkBox_RenameTrproj.Size = new System.Drawing.Size(185, 23);
            this.checkBox_RenameTrproj.TabIndex = 1;
            this.checkBox_RenameTrproj.Text = "Rename .trproj file as well";
            // 
            // panel_02
            // 
            this.panel_02.Controls.Add(this.checkBox_RenameDirectory);
            this.panel_02.Controls.Add(this.checkBox_RenameTrproj);
            this.panel_02.Controls.Add(this.button_Cancel);
            this.panel_02.Controls.Add(this.button_Apply);
            this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_02.Location = new System.Drawing.Point(0, 55);
            this.panel_02.Name = "panel_02";
            this.panel_02.Size = new System.Drawing.Size(464, 62);
            this.panel_02.TabIndex = 2;
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label.Location = new System.Drawing.Point(3, 3);
            this.label.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(39, 13);
            this.label.TabIndex = 0;
            this.label.Text = "Name:";
            // 
            // textBox_NewName
            // 
            this.textBox_NewName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox_NewName.Location = new System.Drawing.Point(6, 19);
            this.textBox_NewName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
            this.textBox_NewName.Name = "textBox_NewName";
            this.textBox_NewName.Size = new System.Drawing.Size(450, 29);
            this.textBox_NewName.TabIndex = 1;
            // 
            // panel_01
            // 
            this.panel_01.Controls.Add(this.textBox_NewName);
            this.panel_01.Controls.Add(this.label);
            this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_01.Location = new System.Drawing.Point(0, 0);
            this.panel_01.Margin = new System.Windows.Forms.Padding(0);
            this.panel_01.Name = "panel_01";
            this.panel_01.Size = new System.Drawing.Size(464, 55);
            this.panel_01.TabIndex = 0;
            // 
            // FormRenameProject
            // 
            this.AcceptButton = this.button_Apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(464, 117);
            this.Controls.Add(this.panel_01);
            this.Controls.Add(this.panel_02);
            this.FlatBorder = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRenameProject";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rename Project";
            this.panel_02.ResumeLayout(false);
            this.panel_01.ResumeLayout(false);
            this.panel_01.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkCheckBox checkBox_RenameDirectory;
		private DarkUI.Controls.DarkCheckBox checkBox_RenameTrproj;
		private System.Windows.Forms.Panel panel_02;
        private DarkUI.Controls.DarkLabel label;
        private DarkUI.Controls.DarkTextBox textBox_NewName;
        private System.Windows.Forms.Panel panel_01;
    }
}
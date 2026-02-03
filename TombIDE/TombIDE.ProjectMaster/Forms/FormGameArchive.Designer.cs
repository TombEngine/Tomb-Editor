namespace TombIDE.ProjectMaster.Forms
{
	partial class FormGameArchive
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
			tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			panel_TextBox = new System.Windows.Forms.Panel();
			richTextBox = new System.Windows.Forms.RichTextBox();
			label_Enter = new DarkUI.Controls.DarkLabel();
			button_Generate = new DarkUI.Controls.DarkButton();
			label_Info = new DarkUI.Controls.DarkLabel();
			tableLayoutPanel.SuspendLayout();
			panel_TextBox.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanel.Controls.Add(panel_TextBox, 0, 1);
			tableLayoutPanel.Controls.Add(label_Enter, 0, 0);
			tableLayoutPanel.Controls.Add(button_Generate, 1, 2);
			tableLayoutPanel.Controls.Add(label_Info, 0, 2);
			tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 3;
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			tableLayoutPanel.Size = new System.Drawing.Size(784, 561);
			tableLayoutPanel.TabIndex = 0;
			// 
			// panel_TextBox
			// 
			panel_TextBox.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			panel_TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			tableLayoutPanel.SetColumnSpan(panel_TextBox, 2);
			panel_TextBox.Controls.Add(richTextBox);
			panel_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			panel_TextBox.Location = new System.Drawing.Point(0, 40);
			panel_TextBox.Margin = new System.Windows.Forms.Padding(0);
			panel_TextBox.Name = "panel_TextBox";
			panel_TextBox.Padding = new System.Windows.Forms.Padding(3);
			panel_TextBox.Size = new System.Drawing.Size(784, 451);
			panel_TextBox.TabIndex = 1;
			// 
			// richTextBox
			// 
			richTextBox.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			richTextBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			richTextBox.ForeColor = System.Drawing.Color.Gainsboro;
			richTextBox.Location = new System.Drawing.Point(3, 3);
			richTextBox.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
			richTextBox.Name = "richTextBox";
			richTextBox.Size = new System.Drawing.Size(776, 443);
			richTextBox.TabIndex = 0;
			richTextBox.Text = "";
			// 
			// label_Enter
			// 
			label_Enter.AutoSize = true;
			label_Enter.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Enter.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Enter.Location = new System.Drawing.Point(20, 0);
			label_Enter.Margin = new System.Windows.Forms.Padding(20, 0, 20, 5);
			label_Enter.Name = "label_Enter";
			label_Enter.Size = new System.Drawing.Size(604, 35);
			label_Enter.TabIndex = 2;
			label_Enter.Text = "README.txt text: (Optional)";
			label_Enter.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// button_Generate
			// 
			button_Generate.Checked = false;
			button_Generate.Dock = System.Windows.Forms.DockStyle.Right;
			button_Generate.Location = new System.Drawing.Point(644, 511);
			button_Generate.Margin = new System.Windows.Forms.Padding(0, 20, 20, 20);
			button_Generate.Name = "button_Generate";
			button_Generate.Size = new System.Drawing.Size(120, 30);
			button_Generate.TabIndex = 5;
			button_Generate.Text = "Generate Archive...";
			button_Generate.Click += button_Generate_Click;
			// 
			// label_Info
			// 
			label_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Info.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Info.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Info.Location = new System.Drawing.Point(20, 491);
			label_Info.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
			label_Info.Name = "label_Info";
			label_Info.Size = new System.Drawing.Size(624, 70);
			label_Info.TabIndex = 3;
			label_Info.Text = "IMPORTANT: Please manually verify the files in the archive after it's been created.";
			label_Info.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormGameArchive
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(784, 561);
			Controls.Add(tableLayoutPanel);
			MinimumSize = new System.Drawing.Size(740, 480);
			Name = "FormGameArchive";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Create an archive";
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			panel_TextBox.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private DarkUI.Controls.DarkLabel label_Info;
		private System.Windows.Forms.Panel panel_TextBox;
		private System.Windows.Forms.RichTextBox richTextBox;
		private DarkUI.Controls.DarkLabel label_Enter;
		private DarkUI.Controls.DarkButton button_Generate;
	}
}
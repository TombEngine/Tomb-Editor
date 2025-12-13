using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormMoveable
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
            butOK = new DarkButton();
            butCancel = new DarkButton();
            cbInvisible = new DarkCheckBox();
            cbBit2 = new DarkCheckBox();
            cbClearBody = new DarkCheckBox();
            cbBit1 = new DarkCheckBox();
            cbBit3 = new DarkCheckBox();
            cbBit4 = new DarkCheckBox();
            cbBit5 = new DarkCheckBox();
            tbOCB = new DarkTextBox();
            label1 = new DarkLabel();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            lblColor = new DarkLabel();
            panelColor = new System.Windows.Forms.Panel();
            butResetTint = new DarkButton();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // butOK
            // 
            butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOK.Checked = false;
            butOK.Location = new System.Drawing.Point(39, 191);
            butOK.Name = "butOK";
            butOK.Size = new System.Drawing.Size(80, 23);
            butOK.TabIndex = 8;
            butOK.Text = "OK";
            butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOK.Click += butOK_Click;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(127, 191);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 9;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // cbInvisible
            // 
            cbInvisible.AutoSize = true;
            cbInvisible.Location = new System.Drawing.Point(76, 9);
            cbInvisible.Name = "cbInvisible";
            cbInvisible.Size = new System.Drawing.Size(68, 17);
            cbInvisible.TabIndex = 6;
            cbInvisible.Text = "Invisible";
            // 
            // cbBit2
            // 
            cbBit2.AutoSize = true;
            cbBit2.Location = new System.Drawing.Point(8, 32);
            cbBit2.Name = "cbBit2";
            cbBit2.Size = new System.Drawing.Size(48, 17);
            cbBit2.TabIndex = 2;
            cbBit2.Text = "Bit 2";
            // 
            // cbClearBody
            // 
            cbClearBody.AutoSize = true;
            cbClearBody.Location = new System.Drawing.Point(76, 32);
            cbClearBody.Name = "cbClearBody";
            cbClearBody.Size = new System.Drawing.Size(81, 17);
            cbClearBody.TabIndex = 7;
            cbClearBody.Text = "Clear body";
            // 
            // cbBit1
            // 
            cbBit1.AutoSize = true;
            cbBit1.Location = new System.Drawing.Point(8, 9);
            cbBit1.Name = "cbBit1";
            cbBit1.Size = new System.Drawing.Size(48, 17);
            cbBit1.TabIndex = 1;
            cbBit1.Text = "Bit 1";
            // 
            // cbBit3
            // 
            cbBit3.AutoSize = true;
            cbBit3.Location = new System.Drawing.Point(8, 55);
            cbBit3.Name = "cbBit3";
            cbBit3.Size = new System.Drawing.Size(48, 17);
            cbBit3.TabIndex = 3;
            cbBit3.Text = "Bit 3";
            // 
            // cbBit4
            // 
            cbBit4.AutoSize = true;
            cbBit4.Location = new System.Drawing.Point(8, 79);
            cbBit4.Name = "cbBit4";
            cbBit4.Size = new System.Drawing.Size(48, 17);
            cbBit4.TabIndex = 4;
            cbBit4.Text = "Bit 4";
            // 
            // cbBit5
            // 
            cbBit5.AutoSize = true;
            cbBit5.Location = new System.Drawing.Point(8, 102);
            cbBit5.Name = "cbBit5";
            cbBit5.Size = new System.Drawing.Size(48, 17);
            cbBit5.TabIndex = 5;
            cbBit5.Text = "Bit 5";
            // 
            // tbOCB
            // 
            tbOCB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbOCB.Location = new System.Drawing.Point(39, 135);
            tbOCB.Name = "tbOCB";
            tbOCB.Size = new System.Drawing.Size(168, 22);
            tbOCB.TabIndex = 0;
            tbOCB.KeyPress += tbOCB_KeyPress;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label1.Location = new System.Drawing.Point(5, 137);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(32, 13);
            label1.TabIndex = 14;
            label1.Text = "OCB:";
            // 
            // lblColor
            // 
            lblColor.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblColor.AutoSize = true;
            lblColor.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblColor.Location = new System.Drawing.Point(8, 167);
            lblColor.Name = "lblColor";
            lblColor.Size = new System.Drawing.Size(30, 13);
            lblColor.TabIndex = 15;
            lblColor.Text = "Tint:";
            // 
            // panelColor
            // 
            panelColor.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panelColor.BackColor = System.Drawing.Color.White;
            panelColor.Location = new System.Drawing.Point(39, 163);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(141, 22);
            panelColor.TabIndex = 16;
            panelColor.Click += panelColor_Click;
            // 
            // butResetTint
            // 
            butResetTint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butResetTint.Checked = false;
            butResetTint.Image = Properties.Resources.general_undo_16;
            butResetTint.Location = new System.Drawing.Point(185, 163);
            butResetTint.Name = "butResetTint";
            butResetTint.Size = new System.Drawing.Size(22, 22);
            butResetTint.TabIndex = 17;
            butResetTint.Tag = "";
            toolTip1.SetToolTip(butResetTint, "Reset tint to default");
            butResetTint.Click += butResetTint_Click;
            // 
            // FormMoveable
            // 
            AcceptButton = butOK;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(213, 221);
            Controls.Add(butResetTint);
            Controls.Add(panelColor);
            Controls.Add(lblColor);
            Controls.Add(cbBit5);
            Controls.Add(tbOCB);
            Controls.Add(cbInvisible);
            Controls.Add(label1);
            Controls.Add(cbBit4);
            Controls.Add(cbBit2);
            Controls.Add(butCancel);
            Controls.Add(cbBit3);
            Controls.Add(butOK);
            Controls.Add(cbBit1);
            Controls.Add(cbClearBody);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormMoveable";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Object";
            Load += FormObject_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkCheckBox cbInvisible;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbClearBody;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit5;
        private DarkTextBox tbOCB;
        private DarkLabel label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DarkLabel lblColor;
        private System.Windows.Forms.Panel panelColor;
        private DarkButton butResetTint;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
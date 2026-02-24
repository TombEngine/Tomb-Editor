using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormWayPoint
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
            butCancel = new DarkButton();
            butOK = new DarkButton();
            lblName = new DarkLabel();
            lblSequence = new DarkLabel();
            lblNumber = new DarkLabel();
            lblType = new DarkLabel();
            lblDimension1 = new DarkLabel();
            lblDimension2 = new DarkLabel();
            lblRotationX = new DarkLabel();
            lblRotationY = new DarkLabel();
            lblRoll = new DarkLabel();
            txtName = new DarkTextBox();
            numSequence = new DarkNumericUpDown();
            numNumber = new DarkNumericUpDown();
            cmbType = new DarkComboBox();
            numDimension1 = new DarkNumericUpDown();
            numDimension2 = new DarkNumericUpDown();
            numRotationX = new DarkNumericUpDown();
            numRotationY = new DarkNumericUpDown();
            numRoll = new DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numSequence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNumber).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDimension1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDimension2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRotationX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRotationY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRoll).BeginInit();
            SuspendLayout();
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(239, 248);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 19;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOK
            // 
            butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOK.Checked = false;
            butOK.Location = new System.Drawing.Point(153, 248);
            butOK.Name = "butOK";
            butOK.Size = new System.Drawing.Size(80, 23);
            butOK.TabIndex = 18;
            butOK.Text = "OK";
            butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOK.Click += butOK_Click;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblName.Location = new System.Drawing.Point(12, 15);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(39, 13);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            // 
            // lblSequence
            // 
            lblSequence.AutoSize = true;
            lblSequence.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblSequence.Location = new System.Drawing.Point(12, 41);
            lblSequence.Name = "lblSequence";
            lblSequence.Size = new System.Drawing.Size(61, 13);
            lblSequence.TabIndex = 2;
            lblSequence.Text = "Sequence:";
            // 
            // lblNumber
            // 
            lblNumber.AutoSize = true;
            lblNumber.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblNumber.Location = new System.Drawing.Point(12, 67);
            lblNumber.Name = "lblNumber";
            lblNumber.Size = new System.Drawing.Size(51, 13);
            lblNumber.TabIndex = 4;
            lblNumber.Text = "Number:";
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblType.Location = new System.Drawing.Point(12, 93);
            lblType.Name = "lblType";
            lblType.Size = new System.Drawing.Size(32, 13);
            lblType.TabIndex = 6;
            lblType.Text = "Type:";
            // 
            // lblDimension1
            // 
            lblDimension1.AutoSize = true;
            lblDimension1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblDimension1.Location = new System.Drawing.Point(12, 119);
            lblDimension1.Name = "lblDimension1";
            lblDimension1.Size = new System.Drawing.Size(74, 13);
            lblDimension1.TabIndex = 8;
            lblDimension1.Text = "Dimension 1:";
            // 
            // lblDimension2
            // 
            lblDimension2.AutoSize = true;
            lblDimension2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblDimension2.Location = new System.Drawing.Point(12, 145);
            lblDimension2.Name = "lblDimension2";
            lblDimension2.Size = new System.Drawing.Size(74, 13);
            lblDimension2.TabIndex = 10;
            lblDimension2.Text = "Dimension 2:";
            // 
            // lblRotationX
            // 
            lblRotationX.AutoSize = true;
            lblRotationX.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblRotationX.Location = new System.Drawing.Point(12, 171);
            lblRotationX.Name = "lblRotationX";
            lblRotationX.Size = new System.Drawing.Size(64, 13);
            lblRotationX.TabIndex = 12;
            lblRotationX.Text = "Rotation X:";
            // 
            // lblRotationY
            // 
            lblRotationY.AutoSize = true;
            lblRotationY.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblRotationY.Location = new System.Drawing.Point(12, 197);
            lblRotationY.Name = "lblRotationY";
            lblRotationY.Size = new System.Drawing.Size(63, 13);
            lblRotationY.TabIndex = 14;
            lblRotationY.Text = "Rotation Y:";
            // 
            // lblRoll
            // 
            lblRoll.AutoSize = true;
            lblRoll.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblRoll.Location = new System.Drawing.Point(12, 223);
            lblRoll.Name = "lblRoll";
            lblRoll.Size = new System.Drawing.Size(64, 13);
            lblRoll.TabIndex = 16;
            lblRoll.Text = "Rotation Z:";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(92, 12);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(227, 22);
            txtName.TabIndex = 1;
            // 
            // numSequence
            // 
            numSequence.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numSequence.Location = new System.Drawing.Point(92, 38);
            numSequence.LoopValues = false;
            numSequence.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSequence.Name = "numSequence";
            numSequence.Size = new System.Drawing.Size(227, 22);
            numSequence.TabIndex = 3;
            // 
            // numNumber
            // 
            numNumber.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numNumber.Location = new System.Drawing.Point(92, 64);
            numNumber.LoopValues = false;
            numNumber.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numNumber.Name = "numNumber";
            numNumber.Size = new System.Drawing.Size(227, 22);
            numNumber.TabIndex = 5;
            // 
            // cmbType
            // 
            cmbType.FormattingEnabled = true;
            cmbType.Items.AddRange(new object[] { "Point", "Circle", "Ellipse", "Square", "Rectangle", "Linear", "Bezier" });
            cmbType.Location = new System.Drawing.Point(92, 90);
            cmbType.Name = "cmbType";
            cmbType.Size = new System.Drawing.Size(227, 23);
            cmbType.TabIndex = 7;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            // 
            // numDimension1
            // 
            numDimension1.DecimalPlaces = 2;
            numDimension1.IncrementAlternate = new decimal(new int[] { 100, 0, 0, 0 });
            numDimension1.Location = new System.Drawing.Point(92, 116);
            numDimension1.LoopValues = false;
            numDimension1.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numDimension1.Name = "numDimension1";
            numDimension1.Size = new System.Drawing.Size(227, 22);
            numDimension1.TabIndex = 9;
            numDimension1.Value = new decimal(new int[] { 1024, 0, 0, 0 });
            // 
            // numDimension2
            // 
            numDimension2.DecimalPlaces = 2;
            numDimension2.IncrementAlternate = new decimal(new int[] { 100, 0, 0, 0 });
            numDimension2.Location = new System.Drawing.Point(92, 142);
            numDimension2.LoopValues = false;
            numDimension2.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numDimension2.Name = "numDimension2";
            numDimension2.Size = new System.Drawing.Size(227, 22);
            numDimension2.TabIndex = 11;
            numDimension2.Value = new decimal(new int[] { 1024, 0, 0, 0 });
            // 
            // numRotationX
            // 
            numRotationX.DecimalPlaces = 2;
            numRotationX.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRotationX.Location = new System.Drawing.Point(92, 168);
            numRotationX.LoopValues = false;
            numRotationX.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            numRotationX.Minimum = new decimal(new int[] { 90, 0, 0, int.MinValue });
            numRotationX.Name = "numRotationX";
            numRotationX.Size = new System.Drawing.Size(227, 22);
            numRotationX.TabIndex = 13;
            // 
            // numRotationY
            // 
            numRotationY.DecimalPlaces = 2;
            numRotationY.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRotationY.Location = new System.Drawing.Point(92, 194);
            numRotationY.LoopValues = false;
            numRotationY.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numRotationY.Name = "numRotationY";
            numRotationY.Size = new System.Drawing.Size(227, 22);
            numRotationY.TabIndex = 15;
            // 
            // numRoll
            // 
            numRoll.DecimalPlaces = 2;
            numRoll.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRoll.Location = new System.Drawing.Point(92, 220);
            numRoll.LoopValues = false;
            numRoll.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numRoll.Name = "numRoll";
            numRoll.Size = new System.Drawing.Size(227, 22);
            numRoll.TabIndex = 17;
            // 
            // FormWayPoint
            // 
            AcceptButton = butOK;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(331, 281);
            Controls.Add(numRoll);
            Controls.Add(numRotationY);
            Controls.Add(numRotationX);
            Controls.Add(numDimension2);
            Controls.Add(numDimension1);
            Controls.Add(cmbType);
            Controls.Add(numNumber);
            Controls.Add(numSequence);
            Controls.Add(txtName);
            Controls.Add(lblRoll);
            Controls.Add(lblRotationY);
            Controls.Add(lblRotationX);
            Controls.Add(lblDimension2);
            Controls.Add(lblDimension1);
            Controls.Add(lblType);
            Controls.Add(lblNumber);
            Controls.Add(lblSequence);
            Controls.Add(lblName);
            Controls.Add(butCancel);
            Controls.Add(butOK);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormWayPoint";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "WayPoint";
            Load += FormWayPoint_Load;
            ((System.ComponentModel.ISupportInitialize)numSequence).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNumber).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDimension1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDimension2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRotationX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRotationY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoll).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkLabel lblName;
        private DarkLabel lblSequence;
        private DarkLabel lblNumber;
        private DarkLabel lblType;
        private DarkLabel lblDimension1;
        private DarkLabel lblDimension2;
        private DarkLabel lblRotationX;
        private DarkLabel lblRotationY;
        private DarkLabel lblRoll;
        private DarkTextBox txtName;
        private DarkNumericUpDown numSequence;
        private DarkNumericUpDown numNumber;
        private DarkComboBox cmbType;
        private DarkNumericUpDown numDimension1;
        private DarkNumericUpDown numDimension2;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkNumericUpDown numRoll;
    }
}

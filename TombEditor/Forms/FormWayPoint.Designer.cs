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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.lblName = new DarkUI.Controls.DarkLabel();
            this.lblSequence = new DarkUI.Controls.DarkLabel();
            this.lblNumber = new DarkUI.Controls.DarkLabel();
            this.lblPathType = new DarkUI.Controls.DarkLabel();
            this.lblRotationX = new DarkUI.Controls.DarkLabel();
            this.lblRotationY = new DarkUI.Controls.DarkLabel();
            this.lblRoll = new DarkUI.Controls.DarkLabel();
            this.txtName = new DarkUI.Controls.DarkTextBox();
            this.numSequence = new DarkUI.Controls.DarkNumericUpDown();
            this.numNumber = new DarkUI.Controls.DarkNumericUpDown();
            this.cmbPathType = new DarkUI.Controls.DarkComboBox();
            this.numRotationX = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationY = new DarkUI.Controls.DarkNumericUpDown();
            this.numRoll = new DarkUI.Controls.DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numSequence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoll)).BeginInit();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(239, 220);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 8;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(153, 220);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 7;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblName.Location = new System.Drawing.Point(12, 15);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(39, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name:";
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblSequence.Location = new System.Drawing.Point(12, 41);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(60, 13);
            this.lblSequence.TabIndex = 2;
            this.lblSequence.Text = "Sequence:";
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblNumber.Location = new System.Drawing.Point(12, 67);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(51, 13);
            this.lblNumber.TabIndex = 4;
            this.lblNumber.Text = "Number:";
            // 
            // lblPathType
            // 
            this.lblPathType.AutoSize = true;
            this.lblPathType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPathType.Location = new System.Drawing.Point(12, 93);
            this.lblPathType.Name = "lblPathType";
            this.lblPathType.Size = new System.Drawing.Size(59, 13);
            this.lblPathType.TabIndex = 6;
            this.lblPathType.Text = "Path Type:";
            // 
            // lblRotationX
            // 
            this.lblRotationX.AutoSize = true;
            this.lblRotationX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationX.Location = new System.Drawing.Point(12, 119);
            this.lblRotationX.Name = "lblRotationX";
            this.lblRotationX.Size = new System.Drawing.Size(64, 13);
            this.lblRotationX.TabIndex = 8;
            this.lblRotationX.Text = "Rotation X:";
            // 
            // lblRotationY
            // 
            this.lblRotationY.AutoSize = true;
            this.lblRotationY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationY.Location = new System.Drawing.Point(12, 145);
            this.lblRotationY.Name = "lblRotationY";
            this.lblRotationY.Size = new System.Drawing.Size(63, 13);
            this.lblRotationY.TabIndex = 10;
            this.lblRotationY.Text = "Rotation Y:";
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRoll.Location = new System.Drawing.Point(12, 171);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(30, 13);
            this.lblRoll.TabIndex = 12;
            this.lblRoll.Text = "Roll:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(82, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(237, 20);
            this.txtName.TabIndex = 1;
            // 
            // numSequence
            // 
            this.numSequence.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numSequence.Location = new System.Drawing.Point(82, 38);
            this.numSequence.LoopValues = false;
            this.numSequence.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numSequence.Name = "numSequence";
            this.numSequence.Size = new System.Drawing.Size(237, 22);
            this.numSequence.TabIndex = 3;
            // 
            // numNumber
            // 
            this.numNumber.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numNumber.Location = new System.Drawing.Point(82, 64);
            this.numNumber.LoopValues = false;
            this.numNumber.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numNumber.Name = "numNumber";
            this.numNumber.Size = new System.Drawing.Size(237, 22);
            this.numNumber.TabIndex = 5;
            // 
            // cmbPathType
            // 
            this.cmbPathType.FormattingEnabled = true;
            this.cmbPathType.Items.AddRange(new object[] {
            "Linear",
            "Curved",
            "Bezier"});
            this.cmbPathType.Location = new System.Drawing.Point(82, 90);
            this.cmbPathType.Name = "cmbPathType";
            this.cmbPathType.Size = new System.Drawing.Size(237, 23);
            this.cmbPathType.TabIndex = 7;
            // 
            // numRotationX
            // 
            this.numRotationX.DecimalPlaces = 2;
            this.numRotationX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationX.Location = new System.Drawing.Point(82, 116);
            this.numRotationX.LoopValues = false;
            this.numRotationX.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numRotationX.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numRotationX.Name = "numRotationX";
            this.numRotationX.Size = new System.Drawing.Size(237, 22);
            this.numRotationX.TabIndex = 9;
            // 
            // numRotationY
            // 
            this.numRotationY.DecimalPlaces = 2;
            this.numRotationY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationY.Location = new System.Drawing.Point(82, 142);
            this.numRotationY.LoopValues = false;
            this.numRotationY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRotationY.Name = "numRotationY";
            this.numRotationY.Size = new System.Drawing.Size(237, 22);
            this.numRotationY.TabIndex = 11;
            // 
            // numRoll
            // 
            this.numRoll.DecimalPlaces = 2;
            this.numRoll.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRoll.Location = new System.Drawing.Point(82, 168);
            this.numRoll.LoopValues = false;
            this.numRoll.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRoll.Name = "numRoll";
            this.numRoll.Size = new System.Drawing.Size(237, 22);
            this.numRoll.TabIndex = 13;
            // 
            // FormWayPoint
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(331, 255);
            this.Controls.Add(this.numRoll);
            this.Controls.Add(this.numRotationY);
            this.Controls.Add(this.numRotationX);
            this.Controls.Add(this.cmbPathType);
            this.Controls.Add(this.numNumber);
            this.Controls.Add(this.numSequence);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblRoll);
            this.Controls.Add(this.lblRotationY);
            this.Controls.Add(this.lblRotationX);
            this.Controls.Add(this.lblPathType);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.lblSequence);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWayPoint";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WayPoint";
            this.Load += new System.EventHandler(this.FormWayPoint_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSequence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoll)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkLabel lblName;
        private DarkLabel lblSequence;
        private DarkLabel lblNumber;
        private DarkLabel lblPathType;
        private DarkLabel lblRotationX;
        private DarkLabel lblRotationY;
        private DarkLabel lblRoll;
        private DarkTextBox txtName;
        private DarkNumericUpDown numSequence;
        private DarkNumericUpDown numNumber;
        private DarkComboBox cmbPathType;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkNumericUpDown numRoll;
    }
}

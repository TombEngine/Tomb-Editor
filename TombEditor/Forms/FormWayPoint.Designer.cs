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
            this.lblShape = new DarkUI.Controls.DarkLabel();
            this.lblRadius1 = new DarkUI.Controls.DarkLabel();
            this.lblRadius2 = new DarkUI.Controls.DarkLabel();
            this.lblRotationX = new DarkUI.Controls.DarkLabel();
            this.lblRotationY = new DarkUI.Controls.DarkLabel();
            this.lblRoll = new DarkUI.Controls.DarkLabel();
            this.txtName = new DarkUI.Controls.DarkTextBox();
            this.numSequence = new DarkUI.Controls.DarkNumericUpDown();
            this.numNumber = new DarkUI.Controls.DarkNumericUpDown();
            this.cmbPathType = new DarkUI.Controls.DarkComboBox();
            this.cmbShape = new DarkUI.Controls.DarkComboBox();
            this.numRadius1 = new DarkUI.Controls.DarkNumericUpDown();
            this.numRadius2 = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationX = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationY = new DarkUI.Controls.DarkNumericUpDown();
            this.numRoll = new DarkUI.Controls.DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numSequence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius2)).BeginInit();
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
            this.butCancel.Location = new System.Drawing.Point(239, 298);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 12;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(153, 298);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 11;
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
            // lblShape
            // 
            this.lblShape.AutoSize = true;
            this.lblShape.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblShape.Location = new System.Drawing.Point(12, 119);
            this.lblShape.Name = "lblShape";
            this.lblShape.Size = new System.Drawing.Size(42, 13);
            this.lblShape.TabIndex = 8;
            this.lblShape.Text = "Shape:";
            // 
            // lblRadius1
            // 
            this.lblRadius1.AutoSize = true;
            this.lblRadius1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRadius1.Location = new System.Drawing.Point(12, 145);
            this.lblRadius1.Name = "lblRadius1";
            this.lblRadius1.Size = new System.Drawing.Size(52, 13);
            this.lblRadius1.TabIndex = 10;
            this.lblRadius1.Text = "Radius 1:";
            // 
            // lblRadius2
            // 
            this.lblRadius2.AutoSize = true;
            this.lblRadius2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRadius2.Location = new System.Drawing.Point(12, 171);
            this.lblRadius2.Name = "lblRadius2";
            this.lblRadius2.Size = new System.Drawing.Size(52, 13);
            this.lblRadius2.TabIndex = 12;
            this.lblRadius2.Text = "Radius 2:";
            // 
            // lblRotationX
            // 
            this.lblRotationX.AutoSize = true;
            this.lblRotationX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationX.Location = new System.Drawing.Point(12, 197);
            this.lblRotationX.Name = "lblRotationX";
            this.lblRotationX.Size = new System.Drawing.Size(64, 13);
            this.lblRotationX.TabIndex = 14;
            this.lblRotationX.Text = "Rotation X:";
            // 
            // lblRotationY
            // 
            this.lblRotationY.AutoSize = true;
            this.lblRotationY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationY.Location = new System.Drawing.Point(12, 223);
            this.lblRotationY.Name = "lblRotationY";
            this.lblRotationY.Size = new System.Drawing.Size(63, 13);
            this.lblRotationY.TabIndex = 16;
            this.lblRotationY.Text = "Rotation Y:";
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRoll.Location = new System.Drawing.Point(12, 249);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(30, 13);
            this.lblRoll.TabIndex = 18;
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
            // cmbShape
            // 
            this.cmbShape.FormattingEnabled = true;
            this.cmbShape.Items.AddRange(new object[] {
            "Circle",
            "Ellipse"});
            this.cmbShape.Location = new System.Drawing.Point(82, 116);
            this.cmbShape.Name = "cmbShape";
            this.cmbShape.Size = new System.Drawing.Size(237, 23);
            this.cmbShape.TabIndex = 9;
            this.cmbShape.SelectedIndexChanged += new System.EventHandler(this.cmbShape_SelectedIndexChanged);
            // 
            // numRadius1
            // 
            this.numRadius1.DecimalPlaces = 2;
            this.numRadius1.IncrementAlternate = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numRadius1.Location = new System.Drawing.Point(82, 142);
            this.numRadius1.LoopValues = false;
            this.numRadius1.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numRadius1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numRadius1.Name = "numRadius1";
            this.numRadius1.Size = new System.Drawing.Size(237, 22);
            this.numRadius1.TabIndex = 11;
            this.numRadius1.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // numRadius2
            // 
            this.numRadius2.DecimalPlaces = 2;
            this.numRadius2.IncrementAlternate = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numRadius2.Location = new System.Drawing.Point(82, 168);
            this.numRadius2.LoopValues = false;
            this.numRadius2.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numRadius2.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numRadius2.Name = "numRadius2";
            this.numRadius2.Size = new System.Drawing.Size(237, 22);
            this.numRadius2.TabIndex = 13;
            this.numRadius2.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // numRotationX
            // 
            this.numRotationX.DecimalPlaces = 2;
            this.numRotationX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationX.Location = new System.Drawing.Point(82, 194);
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
            this.numRotationX.TabIndex = 15;
            // 
            // numRotationY
            // 
            this.numRotationY.DecimalPlaces = 2;
            this.numRotationY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationY.Location = new System.Drawing.Point(82, 220);
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
            this.Controls.Add(this.numRadius2);
            this.Controls.Add(this.numRadius1);
            this.Controls.Add(this.cmbShape);
            this.Controls.Add(this.cmbPathType);
            this.Controls.Add(this.numNumber);
            this.Controls.Add(this.numSequence);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblRoll);
            this.Controls.Add(this.lblRotationY);
            this.Controls.Add(this.lblRotationX);
            this.Controls.Add(this.lblRadius2);
            this.Controls.Add(this.lblRadius1);
            this.Controls.Add(this.lblShape);
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
            ((System.ComponentModel.ISupportInitialize)(this.numRadius1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius2)).EndInit();
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
        private DarkLabel lblShape;
        private DarkLabel lblRadius1;
        private DarkLabel lblRadius2;
        private DarkLabel lblRotationX;
        private DarkLabel lblRotationY;
        private DarkLabel lblRoll;
        private DarkTextBox txtName;
        private DarkNumericUpDown numSequence;
        private DarkNumericUpDown numNumber;
        private DarkComboBox cmbPathType;
        private DarkComboBox cmbShape;
        private DarkNumericUpDown numRadius1;
        private DarkNumericUpDown numRadius2;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkNumericUpDown numRoll;
    }
}

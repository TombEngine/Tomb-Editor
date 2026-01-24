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
            this.lblNumber = new DarkUI.Controls.DarkLabel();
            this.lblType = new DarkUI.Controls.DarkLabel();
            this.lblDimension1 = new DarkUI.Controls.DarkLabel();
            this.lblDimension2 = new DarkUI.Controls.DarkLabel();
            this.lblRotationX = new DarkUI.Controls.DarkLabel();
            this.lblRotationY = new DarkUI.Controls.DarkLabel();
            this.lblRoll = new DarkUI.Controls.DarkLabel();
            this.txtName = new DarkUI.Controls.DarkTextBox();
            this.numNumber = new DarkUI.Controls.DarkNumericUpDown();
            this.cmbType = new DarkUI.Controls.DarkComboBox();
            this.numDimension1 = new DarkUI.Controls.DarkNumericUpDown();
            this.numDimension2 = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationX = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationY = new DarkUI.Controls.DarkNumericUpDown();
            this.numRoll = new DarkUI.Controls.DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDimension1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDimension2)).BeginInit();
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
            this.butCancel.Location = new System.Drawing.Point(239, 222);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 17;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(153, 222);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 16;
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
            this.lblName.Size = new System.Drawing.Point(39, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name:";
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblNumber.Location = new System.Drawing.Point(12, 41);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(51, 13);
            this.lblNumber.TabIndex = 2;
            this.lblNumber.Text = "Number:";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblType.Location = new System.Drawing.Point(12, 67);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(34, 13);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "Type:";
            // 
            // lblDimension1
            // 
            this.lblDimension1.AutoSize = true;
            this.lblDimension1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDimension1.Location = new System.Drawing.Point(12, 93);
            this.lblDimension1.Name = "lblDimension1";
            this.lblDimension1.Size = new System.Drawing.Size(71, 13);
            this.lblDimension1.TabIndex = 6;
            this.lblDimension1.Text = "Dimension 1:";
            // 
            // lblDimension2
            // 
            this.lblDimension2.AutoSize = true;
            this.lblDimension2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDimension2.Location = new System.Drawing.Point(12, 119);
            this.lblDimension2.Name = "lblDimension2";
            this.lblDimension2.Size = new System.Drawing.Size(71, 13);
            this.lblDimension2.TabIndex = 8;
            this.lblDimension2.Text = "Dimension 2:";
            // 
            // lblRotationX
            // 
            this.lblRotationX.AutoSize = true;
            this.lblRotationX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationX.Location = new System.Drawing.Point(12, 145);
            this.lblRotationX.Name = "lblRotationX";
            this.lblRotationX.Size = new System.Drawing.Size(64, 13);
            this.lblRotationX.TabIndex = 10;
            this.lblRotationX.Text = "Rotation X:";
            // 
            // lblRotationY
            // 
            this.lblRotationY.AutoSize = true;
            this.lblRotationY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRotationY.Location = new System.Drawing.Point(12, 171);
            this.lblRotationY.Name = "lblRotationY";
            this.lblRotationY.Size = new System.Drawing.Size(63, 13);
            this.lblRotationY.TabIndex = 12;
            this.lblRotationY.Text = "Rotation Y:";
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRoll.Location = new System.Drawing.Point(12, 197);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(42, 13);
            this.lblRoll.TabIndex = 14;
            this.lblRoll.Text = "Z Axis:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(92, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(227, 20);
            this.txtName.TabIndex = 1;
            // 
            // numNumber
            // 
            this.numNumber.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numNumber.Location = new System.Drawing.Point(92, 38);
            this.numNumber.LoopValues = false;
            this.numNumber.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numNumber.Name = "numNumber";
            this.numNumber.Size = new System.Drawing.Size(227, 22);
            this.numNumber.TabIndex = 3;
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Point",
            "Circle",
            "Ellipse",
            "Square",
            "Rectangle",
            "Linear",
            "Bezier"});
            this.cmbType.Location = new System.Drawing.Point(92, 64);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(227, 23);
            this.cmbType.TabIndex = 5;
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // numDimension1
            // 
            this.numDimension1.DecimalPlaces = 2;
            this.numDimension1.IncrementAlternate = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numDimension1.Location = new System.Drawing.Point(92, 90);
            this.numDimension1.LoopValues = false;
            this.numDimension1.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numDimension1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numDimension1.Name = "numDimension1";
            this.numDimension1.Size = new System.Drawing.Size(227, 22);
            this.numDimension1.TabIndex = 7;
            this.numDimension1.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // numDimension2
            // 
            this.numDimension2.DecimalPlaces = 2;
            this.numDimension2.IncrementAlternate = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numDimension2.Location = new System.Drawing.Point(92, 116);
            this.numDimension2.LoopValues = false;
            this.numDimension2.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numDimension2.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numDimension2.Name = "numDimension2";
            this.numDimension2.Size = new System.Drawing.Size(227, 22);
            this.numDimension2.TabIndex = 9;
            this.numDimension2.Value = new decimal(new int[] {
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
            this.numRotationX.Location = new System.Drawing.Point(92, 142);
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
            this.numRotationX.Size = new System.Drawing.Size(227, 22);
            this.numRotationX.TabIndex = 11;
            // 
            // numRotationY
            // 
            this.numRotationY.DecimalPlaces = 2;
            this.numRotationY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationY.Location = new System.Drawing.Point(92, 168);
            this.numRotationY.LoopValues = false;
            this.numRotationY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRotationY.Name = "numRotationY";
            this.numRotationY.Size = new System.Drawing.Size(227, 22);
            this.numRotationY.TabIndex = 13;
            // 
            // numRoll
            // 
            this.numRoll.DecimalPlaces = 2;
            this.numRoll.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRoll.Location = new System.Drawing.Point(92, 194);
            this.numRoll.LoopValues = false;
            this.numRoll.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRoll.Name = "numRoll";
            this.numRoll.Size = new System.Drawing.Size(227, 22);
            this.numRoll.TabIndex = 15;
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
            this.Controls.Add(this.numDimension2);
            this.Controls.Add(this.numDimension1);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.numNumber);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblRoll);
            this.Controls.Add(this.lblRotationY);
            this.Controls.Add(this.lblRotationX);
            this.Controls.Add(this.lblDimension2);
            this.Controls.Add(this.lblDimension1);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblNumber);
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
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDimension1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDimension2)).EndInit();
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
        private DarkLabel lblNumber;
        private DarkLabel lblType;
        private DarkLabel lblDimension1;
        private DarkLabel lblDimension2;
        private DarkLabel lblRotationX;
        private DarkLabel lblRotationY;
        private DarkLabel lblRoll;
        private DarkTextBox txtName;
        private DarkNumericUpDown numNumber;
        private DarkComboBox cmbType;
        private DarkNumericUpDown numDimension1;
        private DarkNumericUpDown numDimension2;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkNumericUpDown numRoll;
    }
}

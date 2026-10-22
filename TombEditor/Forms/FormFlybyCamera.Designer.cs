using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormFlybyCamera
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
            this.cbBit0 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit1 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit2 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit3 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit4 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit5 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit6 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit13 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit12 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit11 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit10 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit9 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit8 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit7 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit15 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit14 = new DarkUI.Controls.DarkCheckBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.label2 = new DarkUI.Controls.DarkLabel();
            this.label3 = new DarkUI.Controls.DarkLabel();
            this.label4 = new DarkUI.Controls.DarkLabel();
            this.label5 = new DarkUI.Controls.DarkLabel();
            this.label6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.labelDofMode = new DarkUI.Controls.DarkLabel();
            this.labelDofDistance = new DarkUI.Controls.DarkLabel();
            this.labelDofRange = new DarkUI.Controls.DarkLabel();
            this.labelDofStrength = new DarkUI.Controls.DarkLabel();
            this.numSequence = new DarkUI.Controls.DarkNumericUpDown();
            this.numNumber = new DarkUI.Controls.DarkNumericUpDown();
            this.numTimer = new DarkUI.Controls.DarkNumericUpDown();
            this.numSpeed = new DarkUI.Controls.DarkNumericUpDown();
            this.numFOV = new DarkUI.Controls.DarkNumericUpDown();
            this.numRoll = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationX = new DarkUI.Controls.DarkNumericUpDown();
            this.numRotationY = new DarkUI.Controls.DarkNumericUpDown();
            this.comboDofMode = new DarkUI.Controls.DarkComboBox();
            this.numDofDistance = new DarkUI.Controls.DarkNumericUpDown();
            this.numDofRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numDofStrength = new DarkUI.Controls.DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numSequence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFOV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofStrength)).BeginInit();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(456, 389);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(370, 389);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // cbBit0
            // 
            this.cbBit0.Location = new System.Drawing.Point(208, 12);
            this.cbBit0.Name = "cbBit0";
            this.cbBit0.Size = new System.Drawing.Size(300, 32);
            this.cbBit0.TabIndex = 8;
            this.cbBit0.Text = "Make a cut to flyby from Lara camera position";
            // 
            // cbBit1
            // 
            this.cbBit1.Location = new System.Drawing.Point(208, 35);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(300, 32);
            this.cbBit1.TabIndex = 9;
            this.cbBit1.Text = "Track entity position";
            // 
            // cbBit2
            // 
            this.cbBit2.Location = new System.Drawing.Point(208, 58);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(300, 32);
            this.cbBit2.TabIndex = 10;
            this.cbBit2.Text = "Infinite loop";
            // 
            // cbBit3
            // 
            this.cbBit3.Location = new System.Drawing.Point(208, 81);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(300, 32);
            this.cbBit3.TabIndex = 11;
            this.cbBit3.Text = "Create tracking camera";
            // 
            // cbBit4
            // 
            this.cbBit4.Location = new System.Drawing.Point(208, 104);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(300, 32);
            this.cbBit4.TabIndex = 12;
            this.cbBit4.Text = "Focus on Lara\'s last head position";
            // 
            // cbBit5
            // 
            this.cbBit5.Location = new System.Drawing.Point(208, 127);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(300, 32);
            this.cbBit5.TabIndex = 13;
            this.cbBit5.Text = "Focus on Lara\'s head";
            // 
            // cbBit6
            // 
            this.cbBit6.Location = new System.Drawing.Point(208, 150);
            this.cbBit6.Name = "cbBit6";
            this.cbBit6.Size = new System.Drawing.Size(300, 32);
            this.cbBit6.TabIndex = 14;
            this.cbBit6.Text = "Snap back to Lara at the end of sequence";
            // 
            // cbBit13
            // 
            this.cbBit13.Location = new System.Drawing.Point(208, 311);
            this.cbBit13.Name = "cbBit13";
            this.cbBit13.Size = new System.Drawing.Size(300, 32);
            this.cbBit13.TabIndex = 21;
            this.cbBit13.Text = "Unused";
            // 
            // cbBit12
            // 
            this.cbBit12.Location = new System.Drawing.Point(208, 288);
            this.cbBit12.Name = "cbBit12";
            this.cbBit12.Size = new System.Drawing.Size(300, 32);
            this.cbBit12.TabIndex = 20;
            this.cbBit12.Text = "Unused";
            // 
            // cbBit11
            // 
            this.cbBit11.Location = new System.Drawing.Point(208, 265);
            this.cbBit11.Name = "cbBit11";
            this.cbBit11.Size = new System.Drawing.Size(300, 32);
            this.cbBit11.TabIndex = 19;
            this.cbBit11.Text = "Let Lara move in cinematic mode";
            // 
            // cbBit10
            // 
            this.cbBit10.Location = new System.Drawing.Point(208, 242);
            this.cbBit10.Name = "cbBit10";
            this.cbBit10.Size = new System.Drawing.Size(300, 32);
            this.cbBit10.TabIndex = 18;
            this.cbBit10.Text = "Cinematic mode";
            // 
            // cbBit9
            // 
            this.cbBit9.Location = new System.Drawing.Point(208, 219);
            this.cbBit9.Name = "cbBit9";
            this.cbBit9.Size = new System.Drawing.Size(300, 32);
            this.cbBit9.TabIndex = 17;
            this.cbBit9.Text = "Disable exit from sequence with \"Look At\" key";
            // 
            // cbBit8
            // 
            this.cbBit8.Location = new System.Drawing.Point(208, 196);
            this.cbBit8.Name = "cbBit8";
            this.cbBit8.Size = new System.Drawing.Size(300, 32);
            this.cbBit8.TabIndex = 16;
            this.cbBit8.Text = "Freeze camera (Timer field)";
            // 
            // cbBit7
            // 
            this.cbBit7.Location = new System.Drawing.Point(208, 173);
            this.cbBit7.Name = "cbBit7";
            this.cbBit7.Size = new System.Drawing.Size(317, 32);
            this.cbBit7.TabIndex = 15;
            this.cbBit7.Text = "Jump to another camera in sequence (Timer field)";
            // 
            // cbBit15
            // 
            this.cbBit15.Location = new System.Drawing.Point(208, 357);
            this.cbBit15.Name = "cbBit15";
            this.cbBit15.Size = new System.Drawing.Size(300, 32);
            this.cbBit15.TabIndex = 23;
            this.cbBit15.Text = "Unused";
            // 
            // cbBit14
            // 
            this.cbBit14.Location = new System.Drawing.Point(208, 334);
            this.cbBit14.Name = "cbBit14";
            this.cbBit14.Size = new System.Drawing.Size(300, 32);
            this.cbBit14.TabIndex = 22;
            this.cbBit14.Text = "Activate heavy trigger";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Sequence:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Number:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label3.Location = new System.Drawing.Point(12, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "FOV:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label4.Location = new System.Drawing.Point(12, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Roll:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label5.Location = new System.Drawing.Point(12, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Timer:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label6.Location = new System.Drawing.Point(12, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Speed:";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(12, 178);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(64, 13);
            this.darkLabel1.TabIndex = 22;
            this.darkLabel1.Text = "Rotation X:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(12, 204);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(63, 13);
            this.darkLabel2.TabIndex = 24;
            this.darkLabel2.Text = "Rotation Y:";
            // 
            // labelDofMode
            // 
            this.labelDofMode.AutoSize = true;
            this.labelDofMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelDofMode.Location = new System.Drawing.Point(12, 230);
            this.labelDofMode.Name = "labelDofMode";
            this.labelDofMode.Size = new System.Drawing.Size(62, 13);
            this.labelDofMode.TabIndex = 30;
            this.labelDofMode.Text = "DOF mode:";
            // 
            // labelDofDistance
            // 
            this.labelDofDistance.AutoSize = true;
            this.labelDofDistance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelDofDistance.Location = new System.Drawing.Point(12, 256);
            this.labelDofDistance.Name = "labelDofDistance";
            this.labelDofDistance.Size = new System.Drawing.Size(52, 13);
            this.labelDofDistance.TabIndex = 31;
            this.labelDofDistance.Text = "Distance:";
            // 
            // labelDofRange
            // 
            this.labelDofRange.AutoSize = true;
            this.labelDofRange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelDofRange.Location = new System.Drawing.Point(12, 282);
            this.labelDofRange.Name = "labelDofRange";
            this.labelDofRange.Size = new System.Drawing.Size(42, 13);
            this.labelDofRange.TabIndex = 32;
            this.labelDofRange.Text = "Range:";
            // 
            // labelDofStrength
            // 
            this.labelDofStrength.AutoSize = true;
            this.labelDofStrength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelDofStrength.Location = new System.Drawing.Point(12, 308);
            this.labelDofStrength.Name = "labelDofStrength";
            this.labelDofStrength.Size = new System.Drawing.Size(50, 13);
            this.labelDofStrength.TabIndex = 33;
            this.labelDofStrength.Text = "Strength:";
            // 
            // numSequence
            // 
            this.numSequence.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numSequence.Location = new System.Drawing.Point(82, 20);
            this.numSequence.LoopValues = false;
            this.numSequence.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numSequence.Name = "numSequence";
            this.numSequence.Size = new System.Drawing.Size(71, 22);
            this.numSequence.TabIndex = 0;
            // 
            // numNumber
            // 
            this.numNumber.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numNumber.Location = new System.Drawing.Point(82, 46);
            this.numNumber.LoopValues = false;
            this.numNumber.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numNumber.Name = "numNumber";
            this.numNumber.Size = new System.Drawing.Size(71, 22);
            this.numNumber.TabIndex = 1;
            // 
            // numTimer
            // 
            this.numTimer.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numTimer.Location = new System.Drawing.Point(82, 72);
            this.numTimer.LoopValues = false;
            this.numTimer.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numTimer.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.numTimer.Name = "numTimer";
            this.numTimer.Size = new System.Drawing.Size(71, 22);
            this.numTimer.TabIndex = 2;
            // 
            // numSpeed
            // 
            this.numSpeed.DecimalPlaces = 2;
            this.numSpeed.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numSpeed.Location = new System.Drawing.Point(82, 98);
            this.numSpeed.LoopValues = false;
            this.numSpeed.Name = "numSpeed";
            this.numSpeed.Size = new System.Drawing.Size(71, 22);
            this.numSpeed.TabIndex = 3;
            // 
            // numFOV
            // 
            this.numFOV.DecimalPlaces = 2;
            this.numFOV.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numFOV.Location = new System.Drawing.Point(82, 124);
            this.numFOV.LoopValues = false;
            this.numFOV.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numFOV.Name = "numFOV";
            this.numFOV.Size = new System.Drawing.Size(71, 22);
            this.numFOV.TabIndex = 4;
            // 
            // numRoll
            // 
            this.numRoll.DecimalPlaces = 2;
            this.numRoll.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRoll.Location = new System.Drawing.Point(82, 150);
            this.numRoll.LoopValues = false;
            this.numRoll.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRoll.Name = "numRoll";
            this.numRoll.LoopValues = true;
            this.numRoll.Size = new System.Drawing.Size(71, 22);
            this.numRoll.TabIndex = 5;
            // 
            // numRotationX
            // 
            this.numRotationX.DecimalPlaces = 2;
            this.numRotationX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationX.Location = new System.Drawing.Point(82, 176);
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
            this.numRotationX.LoopValues = true;
            this.numRotationX.Size = new System.Drawing.Size(71, 22);
            this.numRotationX.TabIndex = 6;
            // 
            // numRotationY
            // 
            this.numRotationY.DecimalPlaces = 2;
            this.numRotationY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numRotationY.Location = new System.Drawing.Point(82, 202);
            this.numRotationY.LoopValues = false;
            this.numRotationY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numRotationY.Name = "numRotationY";
            this.numRotationY.LoopValues = true;
            this.numRotationY.Size = new System.Drawing.Size(71, 22);
            this.numRotationY.TabIndex = 7;
            // 
            // comboDofMode
            // 
            this.comboDofMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDofMode.FormattingEnabled = true;
            this.comboDofMode.Items.AddRange(new object[] {
            "None",
            "Full",
            "Front",
            "Back"});
            this.comboDofMode.Location = new System.Drawing.Point(82, 228);
            this.comboDofMode.Name = "comboDofMode";
            this.comboDofMode.Size = new System.Drawing.Size(71, 23);
            this.comboDofMode.TabIndex = 8;
            // 
            // numDofDistance
            // 
            this.numDofDistance.DecimalPlaces = 2;
            this.numDofDistance.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numDofDistance.IncrementAlternate = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numDofDistance.Location = new System.Drawing.Point(82, 254);
            this.numDofDistance.LoopValues = false;
            this.numDofDistance.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numDofDistance.Name = "numDofDistance";
            this.numDofDistance.Size = new System.Drawing.Size(71, 22);
            this.numDofDistance.TabIndex = 9;
            // 
            // numDofRange
            // 
            this.numDofRange.DecimalPlaces = 2;
            this.numDofRange.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numDofRange.IncrementAlternate = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numDofRange.Location = new System.Drawing.Point(82, 280);
            this.numDofRange.LoopValues = false;
            this.numDofRange.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numDofRange.Name = "numDofRange";
            this.numDofRange.Size = new System.Drawing.Size(71, 22);
            this.numDofRange.TabIndex = 10;
            // 
            // numDofStrength
            // 
            this.numDofStrength.DecimalPlaces = 2;
            this.numDofStrength.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            this.numDofStrength.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numDofStrength.Location = new System.Drawing.Point(82, 306);
            this.numDofStrength.LoopValues = false;
            this.numDofStrength.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDofStrength.Name = "numDofStrength";
            this.numDofStrength.Size = new System.Drawing.Size(71, 22);
            this.numDofStrength.TabIndex = 11;
            // 
            // FormFlybyCamera
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(541, 418);
            this.Controls.Add(this.numDofStrength);
            this.Controls.Add(this.numDofRange);
            this.Controls.Add(this.numDofDistance);
            this.Controls.Add(this.comboDofMode);
            this.Controls.Add(this.numRotationY);
            this.Controls.Add(this.numRotationX);
            this.Controls.Add(this.numRoll);
            this.Controls.Add(this.numFOV);
            this.Controls.Add(this.numSpeed);
            this.Controls.Add(this.numTimer);
            this.Controls.Add(this.numNumber);
            this.Controls.Add(this.numSequence);
            this.Controls.Add(this.labelDofStrength);
            this.Controls.Add(this.labelDofRange);
            this.Controls.Add(this.labelDofDistance);
            this.Controls.Add(this.labelDofMode);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbBit15);
            this.Controls.Add(this.cbBit14);
            this.Controls.Add(this.cbBit13);
            this.Controls.Add(this.cbBit12);
            this.Controls.Add(this.cbBit11);
            this.Controls.Add(this.cbBit10);
            this.Controls.Add(this.cbBit9);
            this.Controls.Add(this.cbBit8);
            this.Controls.Add(this.cbBit7);
            this.Controls.Add(this.cbBit6);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbBit0);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFlybyCamera";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Flyby camera";
            this.Load += new System.EventHandler(this.FormFlybyCamera_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSequence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFOV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRotationY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDofStrength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkCheckBox cbBit0;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit5;
        private DarkCheckBox cbBit6;
        private DarkCheckBox cbBit13;
        private DarkCheckBox cbBit12;
        private DarkCheckBox cbBit11;
        private DarkCheckBox cbBit10;
        private DarkCheckBox cbBit9;
        private DarkCheckBox cbBit8;
        private DarkCheckBox cbBit7;
        private DarkCheckBox cbBit15;
        private DarkCheckBox cbBit14;
        private DarkLabel label1;
        private DarkLabel label2;
        private DarkLabel label3;
        private DarkLabel label4;
        private DarkLabel label5;
        private DarkLabel label6;
        private DarkLabel darkLabel1;
        private DarkLabel darkLabel2;
        private DarkLabel labelDofMode;
        private DarkLabel labelDofDistance;
        private DarkLabel labelDofRange;
        private DarkLabel labelDofStrength;
        private DarkNumericUpDown numSequence;
        private DarkNumericUpDown numNumber;
        private DarkNumericUpDown numTimer;
        private DarkNumericUpDown numSpeed;
        private DarkNumericUpDown numFOV;
        private DarkNumericUpDown numRoll;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkComboBox comboDofMode;
        private DarkNumericUpDown numDofDistance;
        private DarkNumericUpDown numDofRange;
        private DarkNumericUpDown numDofStrength;
    }
}

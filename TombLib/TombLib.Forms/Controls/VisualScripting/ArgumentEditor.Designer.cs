namespace TombLib.Controls.VisualScripting
{
    partial class ArgumentEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBool = new DarkUI.Controls.DarkPanel();
            cbBool = new DarkUI.Controls.DarkCheckBox();
            groupNumerical = new DarkUI.Controls.DarkPanel();
            nudNumerical = new DarkUI.Controls.DarkNumericUpDown();
            groupVector2 = new DarkUI.Controls.DarkPanel();
            tableVector2 = new System.Windows.Forms.TableLayoutPanel();
            nudVector2Y = new DarkUI.Controls.DarkNumericUpDown();
            nudVector2X = new DarkUI.Controls.DarkNumericUpDown();
            groupVector3 = new DarkUI.Controls.DarkPanel();
            tableVector3 = new System.Windows.Forms.TableLayoutPanel();
            nudVector3Z = new DarkUI.Controls.DarkNumericUpDown();
            nudVector3Y = new DarkUI.Controls.DarkNumericUpDown();
            nudVector3X = new DarkUI.Controls.DarkNumericUpDown();
            groupString = new DarkUI.Controls.DarkPanel();
            tbString = new DarkUI.Controls.DarkTextBox();
            panelMultiline = new DarkUI.Controls.DarkPanel();
            butMultiline = new DarkUI.Controls.DarkButton();
            groupColor = new DarkUI.Controls.DarkPanel();
            panelColor = new DarkUI.Controls.DarkPanel();
            groupTime = new DarkUI.Controls.DarkPanel();
            tableTime = new System.Windows.Forms.TableLayoutPanel();
            nudTimeCents = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeSeconds = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeMinutes = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeHours = new DarkUI.Controls.DarkNumericUpDown();
            groupList = new DarkUI.Controls.DarkPanel();
            cbList = new DarkSearchableComboBox();
            panelAction = new DarkUI.Controls.DarkPanel();
            butAction = new DarkUI.Controls.DarkButton();
            groupBool.SuspendLayout();
            groupNumerical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudNumerical).BeginInit();
            groupVector2.SuspendLayout();
            tableVector2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVector2Y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector2X).BeginInit();
            groupVector3.SuspendLayout();
            tableVector3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVector3Z).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3Y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3X).BeginInit();
            groupString.SuspendLayout();
            panelMultiline.SuspendLayout();
            groupColor.SuspendLayout();
            groupTime.SuspendLayout();
            tableTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTimeCents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeSeconds).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeHours).BeginInit();
            groupList.SuspendLayout();
            panelAction.SuspendLayout();
            SuspendLayout();
            // 
            // groupBool
            // 
            groupBool.Controls.Add(cbBool);
            groupBool.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupBool.Location = new System.Drawing.Point(0, 204);
            groupBool.Margin = new System.Windows.Forms.Padding(0);
            groupBool.Name = "groupBool";
            groupBool.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            groupBool.Size = new System.Drawing.Size(511, 29);
            groupBool.TabIndex = 1;
            groupBool.Visible = false;
            // 
            // cbBool
            // 
            cbBool.AutoSize = true;
            cbBool.Dock = System.Windows.Forms.DockStyle.Fill;
            cbBool.Location = new System.Drawing.Point(0, 2);
            cbBool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbBool.Name = "cbBool";
            cbBool.Size = new System.Drawing.Size(511, 27);
            cbBool.TabIndex = 1;
            cbBool.Text = "None";
            cbBool.CheckedChanged += rb_CheckedChanged;
            // 
            // groupNumerical
            // 
            groupNumerical.Controls.Add(nudNumerical);
            groupNumerical.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupNumerical.Location = new System.Drawing.Point(0, 175);
            groupNumerical.Margin = new System.Windows.Forms.Padding(1);
            groupNumerical.Name = "groupNumerical";
            groupNumerical.Size = new System.Drawing.Size(511, 29);
            groupNumerical.TabIndex = 2;
            groupNumerical.Visible = false;
            // 
            // nudNumerical
            // 
            nudNumerical.DecimalPlaces = 2;
            nudNumerical.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudNumerical.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudNumerical.Location = new System.Drawing.Point(0, 6);
            nudNumerical.LoopValues = false;
            nudNumerical.Margin = new System.Windows.Forms.Padding(0);
            nudNumerical.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudNumerical.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudNumerical.Name = "nudNumerical";
            nudNumerical.Size = new System.Drawing.Size(511, 23);
            nudNumerical.TabIndex = 1;
            nudNumerical.ValueChanged += nudNumerical_ValueChanged;
            // 
            // groupVector2
            // 
            groupVector2.Controls.Add(tableVector2);
            groupVector2.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupVector2.Location = new System.Drawing.Point(0, 146);
            groupVector2.Margin = new System.Windows.Forms.Padding(0);
            groupVector2.Name = "groupVector2";
            groupVector2.Size = new System.Drawing.Size(511, 29);
            groupVector2.TabIndex = 3;
            groupVector2.Visible = false;
            // 
            // tableVector2
            // 
            tableVector2.ColumnCount = 2;
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.Controls.Add(nudVector2Y, 1, 0);
            tableVector2.Controls.Add(nudVector2X, 0, 0);
            tableVector2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableVector2.Location = new System.Drawing.Point(0, 0);
            tableVector2.Margin = new System.Windows.Forms.Padding(0);
            tableVector2.Name = "tableVector2";
            tableVector2.RowCount = 1;
            tableVector2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableVector2.Size = new System.Drawing.Size(511, 29);
            tableVector2.TabIndex = 2;
            // 
            // nudVector2Y
            // 
            nudVector2Y.AllowDrop = true;
            nudVector2Y.DecimalPlaces = 2;
            nudVector2Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudVector2Y.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector2Y.Location = new System.Drawing.Point(257, 6);
            nudVector2Y.LoopValues = false;
            nudVector2Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudVector2Y.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector2Y.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector2Y.Name = "nudVector2Y";
            nudVector2Y.Size = new System.Drawing.Size(252, 23);
            nudVector2Y.TabIndex = 1;
            nudVector2Y.ValueChanged += nudVector2_ValueChanged;
            // 
            // nudVector2X
            // 
            nudVector2X.AllowDrop = true;
            nudVector2X.DecimalPlaces = 2;
            nudVector2X.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudVector2X.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector2X.Location = new System.Drawing.Point(0, 6);
            nudVector2X.LoopValues = false;
            nudVector2X.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudVector2X.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector2X.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector2X.Name = "nudVector2X";
            nudVector2X.Size = new System.Drawing.Size(253, 23);
            nudVector2X.TabIndex = 0;
            nudVector2X.ValueChanged += nudVector2_ValueChanged;
            // 
            // groupVector3
            // 
            groupVector3.Controls.Add(tableVector3);
            groupVector3.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupVector3.Location = new System.Drawing.Point(0, 117);
            groupVector3.Margin = new System.Windows.Forms.Padding(0);
            groupVector3.Name = "groupVector3";
            groupVector3.Size = new System.Drawing.Size(511, 29);
            groupVector3.TabIndex = 4;
            groupVector3.Visible = false;
            // 
            // tableVector3
            // 
            tableVector3.ColumnCount = 3;
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.Controls.Add(nudVector3Z, 2, 0);
            tableVector3.Controls.Add(nudVector3Y, 1, 0);
            tableVector3.Controls.Add(nudVector3X, 0, 0);
            tableVector3.Dock = System.Windows.Forms.DockStyle.Fill;
            tableVector3.Location = new System.Drawing.Point(0, 0);
            tableVector3.Margin = new System.Windows.Forms.Padding(0);
            tableVector3.Name = "tableVector3";
            tableVector3.RowCount = 1;
            tableVector3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableVector3.Size = new System.Drawing.Size(511, 29);
            tableVector3.TabIndex = 1;
            // 
            // nudVector3Z
            // 
            nudVector3Z.AllowDrop = true;
            nudVector3Z.DecimalPlaces = 2;
            nudVector3Z.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudVector3Z.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3Z.Location = new System.Drawing.Point(342, 6);
            nudVector3Z.LoopValues = false;
            nudVector3Z.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            nudVector3Z.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3Z.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3Z.Name = "nudVector3Z";
            nudVector3Z.Size = new System.Drawing.Size(169, 23);
            nudVector3Z.TabIndex = 2;
            nudVector3Z.ValueChanged += nudVector3_ValueChanged;
            nudVector3Z.DragDrop += vector3Control_DragDrop;
            nudVector3Z.DragEnter += draggableControl_DragEnter;
            // 
            // nudVector3Y
            // 
            nudVector3Y.AllowDrop = true;
            nudVector3Y.DecimalPlaces = 2;
            nudVector3Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudVector3Y.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3Y.Location = new System.Drawing.Point(172, 6);
            nudVector3Y.LoopValues = false;
            nudVector3Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudVector3Y.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3Y.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3Y.Name = "nudVector3Y";
            nudVector3Y.Size = new System.Drawing.Size(166, 23);
            nudVector3Y.TabIndex = 1;
            nudVector3Y.ValueChanged += nudVector3_ValueChanged;
            nudVector3Y.DragDrop += vector3Control_DragDrop;
            nudVector3Y.DragEnter += draggableControl_DragEnter;
            // 
            // nudVector3X
            // 
            nudVector3X.AllowDrop = true;
            nudVector3X.DecimalPlaces = 2;
            nudVector3X.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudVector3X.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3X.Location = new System.Drawing.Point(0, 6);
            nudVector3X.LoopValues = false;
            nudVector3X.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudVector3X.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3X.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3X.Name = "nudVector3X";
            nudVector3X.Size = new System.Drawing.Size(168, 23);
            nudVector3X.TabIndex = 0;
            nudVector3X.ValueChanged += nudVector3_ValueChanged;
            nudVector3X.DragDrop += vector3Control_DragDrop;
            nudVector3X.DragEnter += draggableControl_DragEnter;
            // 
            // groupString
            // 
            groupString.Controls.Add(tbString);
            groupString.Controls.Add(panelMultiline);
            groupString.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupString.Location = new System.Drawing.Point(0, 88);
            groupString.Margin = new System.Windows.Forms.Padding(0);
            groupString.Name = "groupString";
            groupString.Size = new System.Drawing.Size(511, 29);
            groupString.TabIndex = 5;
            groupString.Visible = false;
            // 
            // tbString
            // 
            tbString.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbString.Location = new System.Drawing.Point(0, 6);
            tbString.Margin = new System.Windows.Forms.Padding(1);
            tbString.Name = "tbString";
            tbString.Size = new System.Drawing.Size(483, 23);
            tbString.TabIndex = 3;
            tbString.TextChanged += tbString_TextChanged;
            // 
            // panelMultiline
            // 
            panelMultiline.Controls.Add(butMultiline);
            panelMultiline.Dock = System.Windows.Forms.DockStyle.Right;
            panelMultiline.Location = new System.Drawing.Point(483, 0);
            panelMultiline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelMultiline.Name = "panelMultiline";
            panelMultiline.Size = new System.Drawing.Size(28, 29);
            panelMultiline.TabIndex = 4;
            // 
            // butMultiline
            // 
            butMultiline.Checked = false;
            butMultiline.Dock = System.Windows.Forms.DockStyle.Bottom;
            butMultiline.Image = Properties.Resources.general_Multiline_text_16;
            butMultiline.Location = new System.Drawing.Point(0, 5);
            butMultiline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            butMultiline.Name = "butMultiline";
            butMultiline.Size = new System.Drawing.Size(28, 24);
            butMultiline.TabIndex = 2;
            butMultiline.Click += butMultiline_Click;
            // 
            // groupColor
            // 
            groupColor.Controls.Add(panelColor);
            groupColor.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupColor.Location = new System.Drawing.Point(0, 59);
            groupColor.Margin = new System.Windows.Forms.Padding(0);
            groupColor.Name = "groupColor";
            groupColor.Size = new System.Drawing.Size(511, 29);
            groupColor.TabIndex = 6;
            groupColor.Visible = false;
            // 
            // panelColor
            // 
            panelColor.AllowDrop = true;
            panelColor.Dock = System.Windows.Forms.DockStyle.Fill;
            panelColor.Location = new System.Drawing.Point(0, 0);
            panelColor.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(511, 29);
            panelColor.TabIndex = 1;
            panelColor.BackColorChanged += panelColor_BackColorChanged;
            panelColor.DragDrop += panelColor_DragDrop;
            panelColor.DragEnter += draggableControl_DragEnter;
            panelColor.MouseClick += panelColor_MouseClick;
            // 
            // groupTime
            // 
            groupTime.Controls.Add(tableTime);
            groupTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupTime.Location = new System.Drawing.Point(0, 30);
            groupTime.Margin = new System.Windows.Forms.Padding(0);
            groupTime.Name = "groupTime";
            groupTime.Size = new System.Drawing.Size(511, 29);
            groupTime.TabIndex = 7;
            groupTime.Visible = false;
            // 
            // tableTime
            // 
            tableTime.ColumnCount = 4;
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.Controls.Add(nudTimeCents, 3, 0);
            tableTime.Controls.Add(nudTimeSeconds, 2, 0);
            tableTime.Controls.Add(nudTimeMinutes, 1, 0);
            tableTime.Controls.Add(nudTimeHours, 0, 0);
            tableTime.Dock = System.Windows.Forms.DockStyle.Fill;
            tableTime.Location = new System.Drawing.Point(0, 0);
            tableTime.Margin = new System.Windows.Forms.Padding(0);
            tableTime.Name = "tableTime";
            tableTime.RowCount = 1;
            tableTime.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTime.Size = new System.Drawing.Size(511, 29);
            tableTime.TabIndex = 2;
            // 
            // nudTimeCents
            // 
            nudTimeCents.AllowDrop = true;
            nudTimeCents.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudTimeCents.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeCents.Location = new System.Drawing.Point(383, 6);
            nudTimeCents.LoopValues = false;
            nudTimeCents.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            nudTimeCents.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            nudTimeCents.Name = "nudTimeCents";
            nudTimeCents.Size = new System.Drawing.Size(128, 23);
            nudTimeCents.TabIndex = 3;
            nudTimeCents.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeSeconds
            // 
            nudTimeSeconds.AllowDrop = true;
            nudTimeSeconds.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudTimeSeconds.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeSeconds.Location = new System.Drawing.Point(256, 6);
            nudTimeSeconds.LoopValues = false;
            nudTimeSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudTimeSeconds.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            nudTimeSeconds.Name = "nudTimeSeconds";
            nudTimeSeconds.Size = new System.Drawing.Size(123, 23);
            nudTimeSeconds.TabIndex = 2;
            nudTimeSeconds.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeMinutes
            // 
            nudTimeMinutes.AllowDrop = true;
            nudTimeMinutes.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudTimeMinutes.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeMinutes.Location = new System.Drawing.Point(129, 6);
            nudTimeMinutes.LoopValues = false;
            nudTimeMinutes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudTimeMinutes.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            nudTimeMinutes.Name = "nudTimeMinutes";
            nudTimeMinutes.Size = new System.Drawing.Size(123, 23);
            nudTimeMinutes.TabIndex = 1;
            nudTimeMinutes.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeHours
            // 
            nudTimeHours.AllowDrop = true;
            nudTimeHours.Dock = System.Windows.Forms.DockStyle.Bottom;
            nudTimeHours.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeHours.Location = new System.Drawing.Point(0, 6);
            nudTimeHours.LoopValues = false;
            nudTimeHours.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudTimeHours.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudTimeHours.Name = "nudTimeHours";
            nudTimeHours.Size = new System.Drawing.Size(125, 23);
            nudTimeHours.TabIndex = 0;
            nudTimeHours.ValueChanged += nudTime_ValueChanged;
            // 
            // groupList
            // 
            groupList.Controls.Add(cbList);
            groupList.Controls.Add(panelAction);
            groupList.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupList.Location = new System.Drawing.Point(0, 1);
            groupList.Margin = new System.Windows.Forms.Padding(0);
            groupList.Name = "groupList";
            groupList.Size = new System.Drawing.Size(511, 29);
            groupList.TabIndex = 8;
            groupList.Visible = false;
            // 
            // cbList
            // 
            cbList.AllowDrop = true;
            cbList.Dock = System.Windows.Forms.DockStyle.Bottom;
            cbList.Location = new System.Drawing.Point(0, 5);
            cbList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            cbList.Name = "cbList";
            cbList.SearchThreshold = 10;
            cbList.Size = new System.Drawing.Size(483, 24);
            cbList.TabIndex = 2;
            cbList.SelectedIndexChanged += cbList_SelectedIndexChanged;
            cbList.DragDrop += cbList_DragDrop;
            cbList.DragEnter += draggableControl_DragEnter;
            // 
            // panelAction
            // 
            panelAction.Controls.Add(butAction);
            panelAction.Dock = System.Windows.Forms.DockStyle.Right;
            panelAction.Location = new System.Drawing.Point(483, 0);
            panelAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelAction.Name = "panelAction";
            panelAction.Size = new System.Drawing.Size(28, 29);
            panelAction.TabIndex = 3;
            // 
            // butAction
            // 
            butAction.Checked = false;
            butAction.Dock = System.Windows.Forms.DockStyle.Bottom;
            butAction.Image = Properties.Resources.general_target_16;
            butAction.Location = new System.Drawing.Point(0, 5);
            butAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            butAction.Name = "butAction";
            butAction.Size = new System.Drawing.Size(28, 24);
            butAction.TabIndex = 0;
            butAction.Click += butAction_Click;
            // 
            // ArgumentEditor
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupList);
            Controls.Add(groupTime);
            Controls.Add(groupColor);
            Controls.Add(groupString);
            Controls.Add(groupVector3);
            Controls.Add(groupVector2);
            Controls.Add(groupNumerical);
            Controls.Add(groupBool);
            Margin = new System.Windows.Forms.Padding(1);
            Name = "ArgumentEditor";
            Size = new System.Drawing.Size(511, 233);
            groupBool.ResumeLayout(false);
            groupBool.PerformLayout();
            groupNumerical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudNumerical).EndInit();
            groupVector2.ResumeLayout(false);
            tableVector2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudVector2Y).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector2X).EndInit();
            groupVector3.ResumeLayout(false);
            tableVector3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudVector3Z).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3Y).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3X).EndInit();
            groupString.ResumeLayout(false);
            groupString.PerformLayout();
            panelMultiline.ResumeLayout(false);
            groupColor.ResumeLayout(false);
            groupTime.ResumeLayout(false);
            tableTime.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudTimeCents).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeSeconds).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeHours).EndInit();
            groupList.ResumeLayout(false);
            panelAction.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private DarkUI.Controls.DarkPanel groupBool;
        private DarkUI.Controls.DarkPanel groupNumerical;
        private DarkUI.Controls.DarkPanel groupVector2;
        private DarkUI.Controls.DarkPanel groupVector3;
        private DarkUI.Controls.DarkPanel groupString;
        private DarkUI.Controls.DarkPanel groupColor;
        private DarkUI.Controls.DarkPanel groupTime;
        private DarkUI.Controls.DarkPanel groupList;
        private DarkUI.Controls.DarkCheckBox cbBool;
        private DarkUI.Controls.DarkNumericUpDown nudNumerical;
        private System.Windows.Forms.TableLayoutPanel tableVector2;
        private DarkUI.Controls.DarkNumericUpDown nudVector2Y;
        private DarkUI.Controls.DarkNumericUpDown nudVector2X;
        private System.Windows.Forms.TableLayoutPanel tableVector3;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Z;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Y;
        private DarkUI.Controls.DarkNumericUpDown nudVector3X;
        private DarkUI.Controls.DarkTextBox tbString;
        private DarkUI.Controls.DarkPanel panelMultiline;
        private DarkUI.Controls.DarkButton butMultiline;
        private DarkUI.Controls.DarkPanel panelColor;
        private System.Windows.Forms.TableLayoutPanel tableTime;
        private DarkUI.Controls.DarkNumericUpDown nudTimeCents;
        private DarkUI.Controls.DarkNumericUpDown nudTimeSeconds;
        private DarkUI.Controls.DarkNumericUpDown nudTimeMinutes;
        private DarkUI.Controls.DarkNumericUpDown nudTimeHours;
        private DarkSearchableComboBox cbList;
        private DarkUI.Controls.DarkPanel panelAction;
        private DarkUI.Controls.DarkButton butAction;
    }
}

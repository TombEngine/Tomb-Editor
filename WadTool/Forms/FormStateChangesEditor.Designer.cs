namespace WadTool
{
    partial class FormStateChangesEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dgvStateChanges = new DarkUI.Controls.DarkDataGridView();
            columnStateName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnStateId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnLowFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnHighFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnNextAnimation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnNextFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnNextFrameHigh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnBlendFrameCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnBlendCurve = new System.Windows.Forms.DataGridViewButtonColumn();
            btCancel = new DarkUI.Controls.DarkButton();
            btOk = new DarkUI.Controls.DarkButton();
            butPlayStateChange = new DarkUI.Controls.DarkButton();
            dgvControls = new TombLib.Controls.DarkDataGridViewControls();
            lblStateChangeAnnouncement = new DarkUI.Controls.DarkLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            butApply = new DarkUI.Controls.DarkButton();
            stateChangeGroup = new DarkUI.Controls.DarkGroupBox();
            ((System.ComponentModel.ISupportInitialize)dgvStateChanges).BeginInit();
            stateChangeGroup.SuspendLayout();
            SuspendLayout();
            //
            // dgvStateChanges
            //
            dgvStateChanges.AllowUserToAddRows = false;
            dgvStateChanges.AllowUserToDragDropRows = false;
            dgvStateChanges.AllowUserToPasteCells = false;
            dgvStateChanges.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvStateChanges.AutoGenerateColumns = false;
            dgvStateChanges.ColumnHeadersHeight = 17;
            dgvStateChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { columnStateName, columnStateId, columnLowFrame, columnHighFrame, columnNextAnimation, columnNextFrame, columnNextFrameHigh, columnBlendFrameCount, columnBlendCurve });
            dgvStateChanges.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dgvStateChanges.Location = new System.Drawing.Point(7, 22);
            dgvStateChanges.Name = "dgvStateChanges";
            dgvStateChanges.RowHeadersWidth = 40;
            dgvStateChanges.RowTemplate.Height = 16;
            dgvStateChanges.ShowCellErrors = false;
            dgvStateChanges.Size = new System.Drawing.Size(789, 292);
            dgvStateChanges.TabIndex = 48;
            dgvStateChanges.CellFormattingSafe += dgvStateChanges_CellFormattingSafe;
            dgvStateChanges.CellContentClick += dgvStateChanges_CellContentClick;
            dgvStateChanges.CellEndEdit += dgvStateChanges_CellEndEdit;
            dgvStateChanges.CellMouseDoubleClick += dgvStateChanges_CellMouseDoubleClick;
            dgvStateChanges.CellValidating += dgvStateChanges_CellValidating;
            dgvStateChanges.CellPainting += dgvStateChanges_CellPainting;
            dgvStateChanges.SelectionChanged += dgvStateChanges_SelectionChanged;
            dgvStateChanges.UserDeletedRow += dgvStateChanges_UserDeletedRow;
            //
            // columnStateName
            //
            columnStateName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnStateName.DataPropertyName = "StateName";
            columnStateName.HeaderText = "State name";
            columnStateName.Name = "columnStateName";
            columnStateName.ReadOnly = true;
            //
            // columnStateId
            //
            columnStateId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnStateId.DataPropertyName = "StateId";
            columnStateId.FillWeight = 50F;
            columnStateId.HeaderText = "State ID";
            columnStateId.Name = "columnStateId";
            //
            // columnLowFrame
            //
            columnLowFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnLowFrame.DataPropertyName = "LowFrame";
            columnLowFrame.FillWeight = 50F;
            columnLowFrame.HeaderText = "Low frame";
            columnLowFrame.Name = "columnLowFrame";
            //
            // columnHighFrame
            //
            columnHighFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnHighFrame.DataPropertyName = "HighFrame";
            columnHighFrame.FillWeight = 50F;
            columnHighFrame.HeaderText = "High frame";
            columnHighFrame.Name = "columnHighFrame";
            //
            // columnNextAnimation
            //
            columnNextAnimation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnNextAnimation.DataPropertyName = "NextAnimation";
            columnNextAnimation.FillWeight = 50F;
            columnNextAnimation.HeaderText = "Next anim";
            columnNextAnimation.Name = "columnNextAnimation";
            //
            // columnNextFrame
            //
            columnNextFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnNextFrame.DataPropertyName = "NextFrame";
            columnNextFrame.FillWeight = 50F;
            columnNextFrame.HeaderText = "Next frame";
            columnNextFrame.Name = "columnNextFrame";
            //
            // columnNextFrameHigh
            //
            columnNextFrameHigh.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnNextFrameHigh.DataPropertyName = "BlendEndFrame";
            columnNextFrameHigh.FillWeight = 50F;
            columnNextFrameHigh.HeaderText = "Next frame high";
            columnNextFrameHigh.Name = "columnNextFrameHigh";
            //
            // columnBlendFrameCount
            //
            columnBlendFrameCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnBlendFrameCount.DataPropertyName = "BlendFrameCount";
            columnBlendFrameCount.FillWeight = 50F;
            columnBlendFrameCount.HeaderText = "Frame count";
            columnBlendFrameCount.Name = "columnBlendFrameCount";
            //
            // columnBlendCurve
            //
            columnBlendCurve.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnBlendCurve.FillWeight = 50F;
            columnBlendCurve.HeaderText = "Blend curve";
            columnBlendCurve.Name = "columnBlendCurve";
            columnBlendCurve.Text = "";
            columnBlendCurve.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // btCancel
            //
            btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btCancel.Checked = false;
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.Location = new System.Drawing.Point(721, 336);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(81, 23);
            btCancel.TabIndex = 50;
            btCancel.Text = "Cancel";
            btCancel.Click += btCancel_Click;
            //
            // btOk
            //
            btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btOk.Checked = false;
            btOk.Location = new System.Drawing.Point(634, 336);
            btOk.Name = "btOk";
            btOk.Size = new System.Drawing.Size(81, 23);
            btOk.TabIndex = 51;
            btOk.Text = "OK";
            btOk.Click += btOk_Click;
            //
            // butPlayStateChange
            //
            butPlayStateChange.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butPlayStateChange.Checked = false;
            butPlayStateChange.Image = Properties.Resources.actions_play_16;
            butPlayStateChange.Location = new System.Drawing.Point(802, 290);
            butPlayStateChange.Name = "butPlayStateChange";
            butPlayStateChange.Size = new System.Drawing.Size(28, 24);
            butPlayStateChange.TabIndex = 50;
            toolTip1.SetToolTip(butPlayStateChange, "Play state change in chain mode");
            butPlayStateChange.Click += butPlayStateChange_Click;
            //
            // dgvControls
            //
            dgvControls.AlwaysInsertAtZero = false;
            dgvControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            dgvControls.Enabled = false;
            dgvControls.Location = new System.Drawing.Point(802, 22);
            dgvControls.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            dgvControls.MinimumSize = new System.Drawing.Size(0, 32);
            dgvControls.Name = "dgvControls";
            dgvControls.Size = new System.Drawing.Size(28, 258);
            dgvControls.TabIndex = 49;
            //
            // lblStateChangeAnnouncement
            //
            lblStateChangeAnnouncement.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStateChangeAnnouncement.ForeColor = System.Drawing.Color.Gray;
            lblStateChangeAnnouncement.Location = new System.Drawing.Point(8, 341);
            lblStateChangeAnnouncement.Name = "lblStateChangeAnnouncement";
            lblStateChangeAnnouncement.Size = new System.Drawing.Size(534, 13);
            lblStateChangeAnnouncement.TabIndex = 53;
            lblStateChangeAnnouncement.Text = "Pending state change...";
            lblStateChangeAnnouncement.Visible = false;
            //
            // butApply
            //
            butApply.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butApply.Checked = false;
            butApply.Location = new System.Drawing.Point(548, 336);
            butApply.Name = "butApply";
            butApply.Size = new System.Drawing.Size(80, 23);
            butApply.TabIndex = 102;
            butApply.Text = "Apply";
            butApply.Click += butApply_Click;
            //
            // stateChangeGroup
            //
            stateChangeGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            stateChangeGroup.Controls.Add(dgvControls);
            stateChangeGroup.Controls.Add(butPlayStateChange);
            stateChangeGroup.Controls.Add(dgvStateChanges);
            stateChangeGroup.Location = new System.Drawing.Point(5, 9);
            stateChangeGroup.Name = "stateChangeGroup";
            stateChangeGroup.Size = new System.Drawing.Size(837, 321);
            stateChangeGroup.TabIndex = 104;
            stateChangeGroup.TabStop = false;
            stateChangeGroup.Text = "State change editor";
            //
            // FormStateChangesEditor
            //
            AcceptButton = btOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(847, 364);
            Controls.Add(stateChangeGroup);
            Controls.Add(butApply);
            Controls.Add(lblStateChangeAnnouncement);
            Controls.Add(btCancel);
            Controls.Add(btOk);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(596, 242);
            Name = "FormStateChangesEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "State changes";
            ((System.ComponentModel.ISupportInitialize)dgvStateChanges).EndInit();
            stateChangeGroup.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TombLib.Controls.DarkDataGridViewControls dgvControls;
        private DarkUI.Controls.DarkDataGridView dgvStateChanges;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton butPlayStateChange;
        private DarkUI.Controls.DarkLabel lblStateChangeAnnouncement;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLowFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHighFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextAnimation;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrameHigh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnBlendFrameCount;
        private System.Windows.Forms.DataGridViewButtonColumn columnBlendCurve;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkGroupBox stateChangeGroup;
    }
}

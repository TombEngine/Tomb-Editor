namespace TombEditor.Forms
{
    partial class FormSoundSource
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
            butOK = new DarkUI.Controls.DarkButton();
            butCancel = new DarkUI.Controls.DarkButton();
            optionPlaySoundFromWadGroupBox = new DarkUI.Controls.DarkGroupBox();
            cbSoundEnabled = new DarkUI.Controls.DarkCheckBox();
            butSearch = new DarkUI.Controls.DarkButton();
            tbSearch = new DarkUI.Controls.DarkTextBox();
            comboPlayMode = new DarkUI.Controls.DarkComboBox();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            butPlaySound = new DarkUI.Controls.DarkButton();
            lstSounds = new DarkUI.Controls.DarkListView();
            optionPlaySoundFromWadGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // butOK
            // 
            butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOK.Checked = false;
            butOK.Location = new System.Drawing.Point(306, 535);
            butOK.Name = "butOK";
            butOK.Size = new System.Drawing.Size(80, 23);
            butOK.TabIndex = 0;
            butOK.Text = "OK";
            butOK.Click += butOK_Click;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(392, 535);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 1;
            butCancel.Text = "Cancel";
            butCancel.Click += butCancel_Click;
            // 
            // optionPlaySoundFromWadGroupBox
            // 
            optionPlaySoundFromWadGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            optionPlaySoundFromWadGroupBox.Controls.Add(butSearch);
            optionPlaySoundFromWadGroupBox.Controls.Add(tbSearch);
            optionPlaySoundFromWadGroupBox.Controls.Add(comboPlayMode);
            optionPlaySoundFromWadGroupBox.Controls.Add(darkLabel2);
            optionPlaySoundFromWadGroupBox.Controls.Add(butPlaySound);
            optionPlaySoundFromWadGroupBox.Controls.Add(lstSounds);
            optionPlaySoundFromWadGroupBox.Location = new System.Drawing.Point(12, 12);
            optionPlaySoundFromWadGroupBox.Name = "optionPlaySoundFromWadGroupBox";
            optionPlaySoundFromWadGroupBox.Size = new System.Drawing.Size(460, 517);
            optionPlaySoundFromWadGroupBox.TabIndex = 66;
            optionPlaySoundFromWadGroupBox.TabStop = false;
            optionPlaySoundFromWadGroupBox.Text = "Sound to play";
            // 
            // cbSoundEnabled
            // 
            cbSoundEnabled.AutoSize = true;
            cbSoundEnabled.Location = new System.Drawing.Point(12, 541);
            cbSoundEnabled.Name = "cbSoundEnabled";
            cbSoundEnabled.Size = new System.Drawing.Size(68, 17);
            cbSoundEnabled.TabIndex = 110;
            cbSoundEnabled.Text = "Enabled";
            // 
            // butSearch
            // 
            butSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSearch.Checked = false;
            butSearch.Image = Properties.Resources.general_search_16;
            butSearch.Location = new System.Drawing.Point(428, 21);
            butSearch.Name = "butSearch";
            butSearch.Selectable = false;
            butSearch.Size = new System.Drawing.Size(24, 23);
            butSearch.TabIndex = 109;
            butSearch.Click += butSearch_Click;
            // 
            // tbSearch
            // 
            tbSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tbSearch.Location = new System.Drawing.Point(8, 21);
            tbSearch.Name = "tbSearch";
            tbSearch.Size = new System.Drawing.Size(421, 23);
            tbSearch.TabIndex = 0;
            tbSearch.KeyDown += tbSearch_KeyDown;
            // 
            // comboPlayMode
            // 
            comboPlayMode.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboPlayMode.FormattingEnabled = true;
            comboPlayMode.Items.AddRange(new object[] { "Always", "Only when flipmaps are off", "Only when flipmaps are on", "Auto-decide based on room type" });
            comboPlayMode.Location = new System.Drawing.Point(77, 486);
            comboPlayMode.Name = "comboPlayMode";
            comboPlayMode.Size = new System.Drawing.Size(277, 23);
            comboPlayMode.TabIndex = 2;
            // 
            // darkLabel2
            // 
            darkLabel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(9, 489);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(62, 13);
            darkLabel2.TabIndex = 63;
            darkLabel2.Text = "Play mode:";
            // 
            // butPlaySound
            // 
            butPlaySound.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butPlaySound.Checked = false;
            butPlaySound.Image = Properties.Resources.actions_play_16;
            butPlaySound.Location = new System.Drawing.Point(360, 486);
            butPlaySound.Name = "butPlaySound";
            butPlaySound.Size = new System.Drawing.Size(92, 23);
            butPlaySound.TabIndex = 3;
            butPlaySound.Text = "Play sound";
            butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butPlaySound.Click += butPlay_Click;
            // 
            // lstSounds
            // 
            lstSounds.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstSounds.Location = new System.Drawing.Point(8, 50);
            lstSounds.Name = "lstSounds";
            lstSounds.Size = new System.Drawing.Size(444, 430);
            lstSounds.TabIndex = 1;
            lstSounds.Text = "darkListView1";
            lstSounds.Click += lstSounds_Click;
            lstSounds.DoubleClick += lstSounds_DoubleClick;
            // 
            // FormSoundSource
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(484, 570);
            Controls.Add(cbSoundEnabled);
            Controls.Add(butOK);
            Controls.Add(butCancel);
            Controls.Add(optionPlaySoundFromWadGroupBox);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(500, 520);
            Name = "FormSoundSource";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Sound source";
            optionPlaySoundFromWadGroupBox.ResumeLayout(false);
            optionPlaySoundFromWadGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkGroupBox optionPlaySoundFromWadGroupBox;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkListView lstSounds;
        private DarkUI.Controls.DarkComboBox comboPlayMode;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butSearch;
        private DarkUI.Controls.DarkTextBox tbSearch;
        private DarkUI.Controls.DarkCheckBox cbSoundEnabled;
    }
}
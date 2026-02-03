namespace TombEditor.ToolWindows
{
    partial class RoomOptions
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            cbNoPathfinding = new DarkUI.Controls.DarkCheckBox();
            cbHorizon = new DarkUI.Controls.DarkCheckBox();
            darkLabel19 = new DarkUI.Controls.DarkLabel();
            darkLabel18 = new DarkUI.Controls.DarkLabel();
            darkLabel16 = new DarkUI.Controls.DarkLabel();
            cbFlagOutside = new DarkUI.Controls.DarkCheckBox();
            cbFlagCold = new DarkUI.Controls.DarkCheckBox();
            cbFlagDamage = new DarkUI.Controls.DarkCheckBox();
            darkLabel15 = new DarkUI.Controls.DarkLabel();
            panelRoomAmbientLight = new DarkUI.Controls.DarkPanel();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            cbNoLensflare = new DarkUI.Controls.DarkCheckBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            numLightEffectStrength = new DarkUI.Controls.DarkNumericUpDown();
            comboPortalShade = new DarkUI.Controls.DarkComboBox();
            comboLightEffect = new DarkUI.Controls.DarkComboBox();
            tbRoomTags = new TombLib.Controls.DarkAutocompleteTextBox();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            butSelectPreviousRoom = new DarkUI.Controls.DarkButton();
            butNewRoom = new DarkUI.Controls.DarkButton();
            butDeleteRoom = new DarkUI.Controls.DarkButton();
            butDublicateRoom = new DarkUI.Controls.DarkButton();
            butLocked = new DarkUI.Controls.DarkButton();
            comboFlipMap = new DarkUI.Controls.DarkComboBox();
            butRoomUp = new DarkUI.Controls.DarkButton();
            comboReverberation = new DarkUI.Controls.DarkComboBox();
            comboRoomType = new DarkUI.Controls.DarkComboBox();
            comboRoom = new TombLib.Controls.DarkSearchableComboBox();
            butRoomDown = new DarkUI.Controls.DarkButton();
            butEditRoomName = new DarkUI.Controls.DarkButton();
            butCropRoom = new DarkUI.Controls.DarkButton();
            butSplitRoom = new DarkUI.Controls.DarkButton();
            butHidden = new DarkUI.Controls.DarkButton();
            cbFlagNoCaustics = new DarkUI.Controls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)numLightEffectStrength).BeginInit();
            SuspendLayout();
            // 
            // cbNoPathfinding
            // 
            cbNoPathfinding.AutoCheck = false;
            cbNoPathfinding.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbNoPathfinding.Location = new System.Drawing.Point(129, 130);
            cbNoPathfinding.Name = "cbNoPathfinding";
            cbNoPathfinding.Size = new System.Drawing.Size(96, 17);
            cbNoPathfinding.TabIndex = 8;
            cbNoPathfinding.Tag = "SetRoomNoPathfinding";
            cbNoPathfinding.Text = "No pathfinding";
            // 
            // cbHorizon
            // 
            cbHorizon.AutoCheck = false;
            cbHorizon.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbHorizon.Location = new System.Drawing.Point(3, 130);
            cbHorizon.Name = "cbHorizon";
            cbHorizon.Size = new System.Drawing.Size(58, 17);
            cbHorizon.TabIndex = 6;
            cbHorizon.Tag = "SetRoomSkybox";
            cbHorizon.Text = "Skybox";
            // 
            // darkLabel19
            // 
            darkLabel19.AutoSize = true;
            darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel19.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel19.Location = new System.Drawing.Point(87, 84);
            darkLabel19.Name = "darkLabel19";
            darkLabel19.Size = new System.Drawing.Size(48, 13);
            darkLabel19.TabIndex = 103;
            darkLabel19.Text = "Flipmap";
            // 
            // darkLabel18
            // 
            darkLabel18.AutoSize = true;
            darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel18.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel18.Location = new System.Drawing.Point(148, 84);
            darkLabel18.Name = "darkLabel18";
            darkLabel18.Size = new System.Drawing.Size(42, 13);
            darkLabel18.TabIndex = 100;
            darkLabel18.Text = "Reverb";
            // 
            // darkLabel16
            // 
            darkLabel16.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel16.AutoSize = true;
            darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel16.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel16.Location = new System.Drawing.Point(140, 194);
            darkLabel16.Name = "darkLabel16";
            darkLabel16.Size = new System.Drawing.Size(36, 13);
            darkLabel16.TabIndex = 96;
            darkLabel16.Text = "Effect";
            // 
            // cbFlagOutside
            // 
            cbFlagOutside.AutoCheck = false;
            cbFlagOutside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbFlagOutside.Location = new System.Drawing.Point(3, 150);
            cbFlagOutside.Name = "cbFlagOutside";
            cbFlagOutside.Size = new System.Drawing.Size(50, 17);
            cbFlagOutside.TabIndex = 9;
            cbFlagOutside.Tag = "SetRoomOutside";
            cbFlagOutside.Text = "Wind";
            // 
            // cbFlagCold
            // 
            cbFlagCold.AutoCheck = false;
            cbFlagCold.AutoSize = true;
            cbFlagCold.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbFlagCold.Location = new System.Drawing.Point(64, 150);
            cbFlagCold.Name = "cbFlagCold";
            cbFlagCold.Size = new System.Drawing.Size(50, 17);
            cbFlagCold.TabIndex = 10;
            cbFlagCold.Tag = "SetRoomCold";
            cbFlagCold.Text = "Cold";
            // 
            // cbFlagDamage
            // 
            cbFlagDamage.AutoCheck = false;
            cbFlagDamage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbFlagDamage.Location = new System.Drawing.Point(64, 130);
            cbFlagDamage.Name = "cbFlagDamage";
            cbFlagDamage.Size = new System.Drawing.Size(64, 17);
            cbFlagDamage.TabIndex = 7;
            cbFlagDamage.Tag = "SetRoomDamage";
            cbFlagDamage.Text = "Damage";
            // 
            // darkLabel15
            // 
            darkLabel15.AutoSize = true;
            darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel15.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel15.Location = new System.Drawing.Point(0, 84);
            darkLabel15.Name = "darkLabel15";
            darkLabel15.Size = new System.Drawing.Size(62, 13);
            darkLabel15.TabIndex = 91;
            darkLabel15.Text = "Room type";
            // 
            // panelRoomAmbientLight
            // 
            panelRoomAmbientLight.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            panelRoomAmbientLight.Location = new System.Drawing.Point(3, 210);
            panelRoomAmbientLight.Name = "panelRoomAmbientLight";
            panelRoomAmbientLight.Size = new System.Drawing.Size(59, 23);
            panelRoomAmbientLight.TabIndex = 12;
            panelRoomAmbientLight.Tag = "EditAmbientLight";
            // 
            // darkLabel3
            // 
            darkLabel3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel3.AutoSize = true;
            darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(0, 194);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(50, 13);
            darkLabel3.TabIndex = 88;
            darkLabel3.Text = "Ambient";
            // 
            // cbNoLensflare
            // 
            cbNoLensflare.AutoCheck = false;
            cbNoLensflare.AutoSize = true;
            cbNoLensflare.Location = new System.Drawing.Point(129, 150);
            cbNoLensflare.Name = "cbNoLensflare";
            cbNoLensflare.Size = new System.Drawing.Size(88, 17);
            cbNoLensflare.TabIndex = 11;
            cbNoLensflare.Tag = "SetRoomNoLensflare";
            cbNoLensflare.Text = "No lensflare";
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            // 
            // numLightEffectStrength
            // 
            numLightEffectStrength.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            numLightEffectStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numLightEffectStrength.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numLightEffectStrength.Location = new System.Drawing.Point(246, 210);
            numLightEffectStrength.LoopValues = false;
            numLightEffectStrength.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            numLightEffectStrength.Name = "numLightEffectStrength";
            numLightEffectStrength.Size = new System.Drawing.Size(36, 23);
            numLightEffectStrength.TabIndex = 15;
            numLightEffectStrength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            toolTip.SetToolTip(numLightEffectStrength, "Light / transform effect strength");
            numLightEffectStrength.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numLightEffectStrength.ValueChanged += numLightEffectStrength_ValueChanged;
            // 
            // comboPortalShade
            // 
            comboPortalShade.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            comboPortalShade.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboPortalShade.Items.AddRange(new object[] { "Default", "Smooth", "Sharp" });
            comboPortalShade.Location = new System.Drawing.Point(68, 210);
            comboPortalShade.Name = "comboPortalShade";
            comboPortalShade.Size = new System.Drawing.Size(69, 23);
            comboPortalShade.TabIndex = 13;
            toolTip.SetToolTip(comboPortalShade, "Smoothing on room edges");
            comboPortalShade.SelectedIndexChanged += comboPortalShade_SelectedIndexChanged;
            // 
            // comboLightEffect
            // 
            comboLightEffect.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboLightEffect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboLightEffect.Items.AddRange(new object[] { "None", "Default", "Reflection", "Glow", "Move", "Glow & Move", "Mist" });
            comboLightEffect.Location = new System.Drawing.Point(143, 210);
            comboLightEffect.Name = "comboLightEffect";
            comboLightEffect.Size = new System.Drawing.Size(97, 23);
            comboLightEffect.TabIndex = 14;
            toolTip.SetToolTip(comboLightEffect, "Light / transform effect on room vertices");
            comboLightEffect.SelectedIndexChanged += comboLightEffect_SelectedIndexChanged;
            // 
            // tbRoomTags
            // 
            tbRoomTags.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbRoomTags.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            tbRoomTags.Location = new System.Drawing.Point(39, 57);
            tbRoomTags.Name = "tbRoomTags";
            tbRoomTags.Size = new System.Drawing.Size(183, 22);
            tbRoomTags.TabIndex = 2;
            tbRoomTags.Tag = "SetRoomTags";
            toolTip.SetToolTip(tbRoomTags, "Set room tags, separated by spaces");
            tbRoomTags.TextChanged += TbTags_TextChanged;
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(1, 59);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(33, 13);
            darkLabel2.TabIndex = 110;
            darkLabel2.Text = "Tags:";
            // 
            // darkLabel1
            // 
            darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel1.AutoSize = true;
            darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(65, 194);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(71, 13);
            darkLabel1.TabIndex = 112;
            darkLabel1.Text = "Portal shade";
            // 
            // butSelectPreviousRoom
            // 
            butSelectPreviousRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSelectPreviousRoom.BackColor = System.Drawing.Color.FromArgb(128, 64, 64);
            butSelectPreviousRoom.Checked = false;
            butSelectPreviousRoom.Image = Properties.Resources.actions_back_16;
            butSelectPreviousRoom.Location = new System.Drawing.Point(228, 144);
            butSelectPreviousRoom.Name = "butSelectPreviousRoom";
            butSelectPreviousRoom.Size = new System.Drawing.Size(24, 22);
            butSelectPreviousRoom.TabIndex = 24;
            butSelectPreviousRoom.Tag = "SelectPreviousRoom";
            // 
            // butNewRoom
            // 
            butNewRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butNewRoom.Checked = false;
            butNewRoom.Image = Properties.Resources.general_plus_math_16;
            butNewRoom.Location = new System.Drawing.Point(258, 28);
            butNewRoom.Name = "butNewRoom";
            butNewRoom.Size = new System.Drawing.Size(24, 23);
            butNewRoom.TabIndex = 17;
            butNewRoom.Tag = "AddNewRoom";
            // 
            // butDeleteRoom
            // 
            butDeleteRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butDeleteRoom.Checked = false;
            butDeleteRoom.Image = Properties.Resources.general_trash_16;
            butDeleteRoom.Location = new System.Drawing.Point(258, 57);
            butDeleteRoom.Name = "butDeleteRoom";
            butDeleteRoom.Size = new System.Drawing.Size(24, 23);
            butDeleteRoom.TabIndex = 19;
            butDeleteRoom.Tag = "DeleteRooms";
            // 
            // butDublicateRoom
            // 
            butDublicateRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butDublicateRoom.Checked = false;
            butDublicateRoom.Image = Properties.Resources.general_copy_16;
            butDublicateRoom.Location = new System.Drawing.Point(228, 57);
            butDublicateRoom.Name = "butDublicateRoom";
            butDublicateRoom.Size = new System.Drawing.Size(24, 23);
            butDublicateRoom.TabIndex = 18;
            butDublicateRoom.Tag = "DuplicateRoom";
            // 
            // butLocked
            // 
            butLocked.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butLocked.Checked = false;
            butLocked.Image = Properties.Resources.general_Lock_16;
            butLocked.Location = new System.Drawing.Point(258, 144);
            butLocked.Name = "butLocked";
            butLocked.Size = new System.Drawing.Size(24, 22);
            butLocked.TabIndex = 25;
            butLocked.Tag = "LockRoom";
            // 
            // comboFlipMap
            // 
            comboFlipMap.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboFlipMap.Items.AddRange(new object[] { "None", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" });
            comboFlipMap.Location = new System.Drawing.Point(90, 100);
            comboFlipMap.Name = "comboFlipMap";
            comboFlipMap.Size = new System.Drawing.Size(55, 23);
            comboFlipMap.TabIndex = 4;
            comboFlipMap.SelectedIndexChanged += comboFlipMap_SelectedIndexChanged;
            // 
            // butRoomUp
            // 
            butRoomUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butRoomUp.Checked = false;
            butRoomUp.Image = Properties.Resources.general_ArrowUp_16;
            butRoomUp.Location = new System.Drawing.Point(258, 86);
            butRoomUp.Name = "butRoomUp";
            butRoomUp.Size = new System.Drawing.Size(24, 23);
            butRoomUp.TabIndex = 21;
            butRoomUp.Tag = "MoveRoomUp";
            // 
            // comboReverberation
            // 
            comboReverberation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboReverberation.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboReverberation.Items.AddRange(new object[] { "None", "Small", "Medium", "Large", "Pipe" });
            comboReverberation.Location = new System.Drawing.Point(151, 100);
            comboReverberation.Name = "comboReverberation";
            comboReverberation.Size = new System.Drawing.Size(71, 23);
            comboReverberation.TabIndex = 5;
            comboReverberation.SelectedIndexChanged += comboReverberation_SelectedIndexChanged;
            // 
            // comboRoomType
            // 
            comboRoomType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboRoomType.Location = new System.Drawing.Point(3, 100);
            comboRoomType.Name = "comboRoomType";
            comboRoomType.Size = new System.Drawing.Size(81, 23);
            comboRoomType.TabIndex = 3;
            comboRoomType.SelectedIndexChanged += comboRoomType_SelectedIndexChanged;
            // 
            // comboRoom
            // 
            comboRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboRoom.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            comboRoom.Location = new System.Drawing.Point(3, 28);
            comboRoom.Name = "comboRoom";
            comboRoom.Size = new System.Drawing.Size(219, 23);
            comboRoom.TabIndex = 0;
            comboRoom.SelectedIndexChanged += comboRoom_SelectedIndexChanged;
            // 
            // butRoomDown
            // 
            butRoomDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butRoomDown.Checked = false;
            butRoomDown.Image = Properties.Resources.general_ArrowDown_16;
            butRoomDown.Location = new System.Drawing.Point(258, 115);
            butRoomDown.Name = "butRoomDown";
            butRoomDown.Size = new System.Drawing.Size(24, 23);
            butRoomDown.TabIndex = 23;
            butRoomDown.Tag = "MoveRoomDown";
            // 
            // butEditRoomName
            // 
            butEditRoomName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butEditRoomName.Checked = false;
            butEditRoomName.Image = Properties.Resources.general_edit_16;
            butEditRoomName.Location = new System.Drawing.Point(228, 28);
            butEditRoomName.Name = "butEditRoomName";
            butEditRoomName.Size = new System.Drawing.Size(24, 23);
            butEditRoomName.TabIndex = 16;
            butEditRoomName.Tag = "EditRoomName";
            // 
            // butCropRoom
            // 
            butCropRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butCropRoom.Checked = false;
            butCropRoom.Image = Properties.Resources.general_crop_16;
            butCropRoom.Location = new System.Drawing.Point(228, 86);
            butCropRoom.Name = "butCropRoom";
            butCropRoom.Size = new System.Drawing.Size(24, 23);
            butCropRoom.TabIndex = 20;
            butCropRoom.Tag = "CropRoom";
            // 
            // butSplitRoom
            // 
            butSplitRoom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSplitRoom.Checked = false;
            butSplitRoom.Image = Properties.Resources.actions_Split_16;
            butSplitRoom.Location = new System.Drawing.Point(228, 115);
            butSplitRoom.Name = "butSplitRoom";
            butSplitRoom.Size = new System.Drawing.Size(24, 23);
            butSplitRoom.TabIndex = 22;
            butSplitRoom.Tag = "SplitRoom";
            // 
            // butHidden
            // 
            butHidden.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butHidden.Checked = false;
            butHidden.Image = Properties.Resources.toolbox_Invisible_16;
            butHidden.Location = new System.Drawing.Point(228, 172);
            butHidden.Name = "butHidden";
            butHidden.Size = new System.Drawing.Size(54, 22);
            butHidden.TabIndex = 26;
            butHidden.Tag = "HideRoom";
            // 
            // cbFlagNoCaustics
            // 
            cbFlagNoCaustics.AutoCheck = false;
            cbFlagNoCaustics.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbFlagNoCaustics.Location = new System.Drawing.Point(129, 165);
            cbFlagNoCaustics.Name = "cbFlagNoCaustics";
            cbFlagNoCaustics.Size = new System.Drawing.Size(81, 26);
            cbFlagNoCaustics.TabIndex = 113;
            cbFlagNoCaustics.Tag = "SetRoomNoCaustics";
            cbFlagNoCaustics.Text = "No caustics";
            // 
            // RoomOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(cbFlagCold);
            Controls.Add(cbFlagNoCaustics);
            Controls.Add(butHidden);
            Controls.Add(butSelectPreviousRoom);
            Controls.Add(comboPortalShade);
            Controls.Add(darkLabel1);
            Controls.Add(butNewRoom);
            Controls.Add(darkLabel2);
            Controls.Add(tbRoomTags);
            Controls.Add(numLightEffectStrength);
            Controls.Add(butDeleteRoom);
            Controls.Add(butDublicateRoom);
            Controls.Add(butLocked);
            Controls.Add(cbNoLensflare);
            Controls.Add(cbNoPathfinding);
            Controls.Add(cbHorizon);
            Controls.Add(comboFlipMap);
            Controls.Add(darkLabel19);
            Controls.Add(butRoomUp);
            Controls.Add(comboReverberation);
            Controls.Add(darkLabel18);
            Controls.Add(comboLightEffect);
            Controls.Add(darkLabel16);
            Controls.Add(cbFlagOutside);
            Controls.Add(cbFlagDamage);
            Controls.Add(comboRoomType);
            Controls.Add(darkLabel15);
            Controls.Add(comboRoom);
            Controls.Add(panelRoomAmbientLight);
            Controls.Add(darkLabel3);
            Controls.Add(butRoomDown);
            Controls.Add(butEditRoomName);
            Controls.Add(butCropRoom);
            Controls.Add(butSplitRoom);
            DockText = "Room Options";
            MinimumSize = new System.Drawing.Size(284, 236);
            Name = "RoomOptions";
            SerializationKey = "RoomOptions";
            Size = new System.Drawing.Size(284, 236);
            ((System.ComponentModel.ISupportInitialize)numLightEffectStrength).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkCheckBox cbNoPathfinding;
        private DarkUI.Controls.DarkCheckBox cbHorizon;
        private DarkUI.Controls.DarkComboBox comboFlipMap;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkButton butRoomUp;
        private DarkUI.Controls.DarkComboBox comboReverberation;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkComboBox comboLightEffect;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkCheckBox cbFlagOutside;
        private DarkUI.Controls.DarkCheckBox cbFlagCold;
        private DarkUI.Controls.DarkCheckBox cbFlagDamage;
        private DarkUI.Controls.DarkComboBox comboRoomType;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private TombLib.Controls.DarkSearchableComboBox comboRoom;
        private DarkUI.Controls.DarkPanel panelRoomAmbientLight;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkButton butRoomDown;
        private DarkUI.Controls.DarkButton butEditRoomName;
        private DarkUI.Controls.DarkButton butCropRoom;
        private DarkUI.Controls.DarkButton butSplitRoom;
        private DarkUI.Controls.DarkCheckBox cbNoLensflare;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butLocked;
        private DarkUI.Controls.DarkButton butDublicateRoom;
        private DarkUI.Controls.DarkButton butDeleteRoom;
        private DarkUI.Controls.DarkNumericUpDown numLightEffectStrength;
        private TombLib.Controls.DarkAutocompleteTextBox tbRoomTags;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butNewRoom;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboPortalShade;
        private DarkUI.Controls.DarkButton butSelectPreviousRoom;
        private DarkUI.Controls.DarkButton butHidden;
        private DarkUI.Controls.DarkCheckBox cbFlagNoCaustics;
    }
}

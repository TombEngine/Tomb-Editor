using System.Windows.Forms;
using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormEventSetEditor
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
            butCancel = new DarkButton();
            butOk = new DarkButton();
            cbActivatorLara = new DarkCheckBox();
            cbActivatorNPC = new DarkCheckBox();
            cbActivatorOtherMoveables = new DarkCheckBox();
            cbActivatorStatics = new DarkCheckBox();
            cbActivatorFlyBy = new DarkCheckBox();
            panelList = new DarkSectionPanel();
            treeEvents = new DarkTreeView();
            darkPanel1 = new DarkPanel();
            butSearch = new DarkButton();
            butUnassignEventSet = new DarkButton();
            butDeleteEventSet = new DarkButton();
            butCloneEventSet = new DarkButton();
			butNewFolder = new DarkButton();
            butNewEventSet = new DarkButton();
            triggerManager = new Controls.TriggerManager();
            lblActivators = new DarkLabel();
            toolTip = new ToolTip(components);
            cbEnableVolume = new DarkCheckBox();
            cbAdjacentRooms = new DarkCheckBox();
            darkLabel1 = new DarkLabel();
            tbName = new DarkTextBox();
            darkLabel2 = new DarkLabel();
            cbEvents = new DarkComboBox();
            panelEditor = new DarkSectionPanel();
            splitContainer = new SplitContainer();
            panelActivators = new DarkSectionPanel();
            panelList.SuspendLayout();
            darkPanel1.SuspendLayout();
            panelEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelActivators.SuspendLayout();
            SuspendLayout();
            // 
            // butCancel
            // 
            butCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(828, 391);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 2;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOk
            // 
            butOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            butOk.Checked = false;
            butOk.Location = new System.Drawing.Point(742, 391);
            butOk.Name = "butOk";
            butOk.Size = new System.Drawing.Size(80, 23);
            butOk.TabIndex = 1;
            butOk.Text = "OK";
            butOk.TextImageRelation = TextImageRelation.ImageBeforeText;
            butOk.Click += butOk_Click;
            // 
            // cbActivatorLara
            // 
            cbActivatorLara.AutoSize = true;
            cbActivatorLara.Location = new System.Drawing.Point(68, 7);
            cbActivatorLara.Name = "cbActivatorLara";
            cbActivatorLara.Size = new System.Drawing.Size(47, 17);
            cbActivatorLara.TabIndex = 10;
            cbActivatorLara.Text = "Lara";
            toolTip.SetToolTip(cbActivatorLara, "Can be activated by Lara");
            cbActivatorLara.CheckedChanged += cbActivators_CheckedChanged;
            // 
            // cbActivatorNPC
            // 
            cbActivatorNPC.AutoSize = true;
            cbActivatorNPC.Location = new System.Drawing.Point(120, 7);
            cbActivatorNPC.Name = "cbActivatorNPC";
            cbActivatorNPC.Size = new System.Drawing.Size(47, 17);
            cbActivatorNPC.TabIndex = 11;
            cbActivatorNPC.Text = "NPC";
            toolTip.SetToolTip(cbActivatorNPC, "Can be activated by creatures");
            cbActivatorNPC.CheckedChanged += cbActivators_CheckedChanged;
            // 
            // cbActivatorOtherMoveables
            // 
            cbActivatorOtherMoveables.AutoSize = true;
            cbActivatorOtherMoveables.Location = new System.Drawing.Point(172, 7);
            cbActivatorOtherMoveables.Name = "cbActivatorOtherMoveables";
            cbActivatorOtherMoveables.Size = new System.Drawing.Size(96, 17);
            cbActivatorOtherMoveables.TabIndex = 12;
            cbActivatorOtherMoveables.Text = "Other objects";
            toolTip.SetToolTip(cbActivatorOtherMoveables, "Can be activated by other moveables, such as rolling balls");
            cbActivatorOtherMoveables.CheckedChanged += cbActivators_CheckedChanged;
            // 
            // cbActivatorStatics
            // 
            cbActivatorStatics.AutoSize = true;
            cbActivatorStatics.Location = new System.Drawing.Point(271, 7);
            cbActivatorStatics.Name = "cbActivatorStatics";
            cbActivatorStatics.Size = new System.Drawing.Size(59, 17);
            cbActivatorStatics.TabIndex = 13;
            cbActivatorStatics.Text = "Statics";
            toolTip.SetToolTip(cbActivatorStatics, "Can be activated by shattering statics");
            cbActivatorStatics.CheckedChanged += cbActivators_CheckedChanged;
            // 
            // cbActivatorFlyBy
            // 
            cbActivatorFlyBy.Location = new System.Drawing.Point(336, 7);
            cbActivatorFlyBy.Name = "cbActivatorFlyBy";
            cbActivatorFlyBy.Size = new System.Drawing.Size(93, 17);
            cbActivatorFlyBy.TabIndex = 14;
            cbActivatorFlyBy.Text = "Flyby cameras";
            toolTip.SetToolTip(cbActivatorFlyBy, "Can be activated by flyby cameras");
            cbActivatorFlyBy.CheckedChanged += cbActivators_CheckedChanged;
            // 
            // panelList
            // 
            panelList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelList.Controls.Add(treeEvents);
            panelList.Controls.Add(darkPanel1);
            panelList.Location = new System.Drawing.Point(3, 3);
            panelList.Name = "panelList";
            panelList.SectionHeader = null;
            panelList.Size = new System.Drawing.Size(258, 377);
            panelList.TabIndex = 22;
            // 
            // treeEvents
            // 
            treeEvents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeEvents.Location = new System.Drawing.Point(4, 32);
            treeEvents.MultiSelect = false;
            treeEvents.Name = "treeEvents";
            treeEvents.AllowMoveNodes = true;
            treeEvents.Size = new System.Drawing.Size(250, 341);
            treeEvents.TabIndex = 0;
            treeEvents.SelectedNodesChanged += treeEvents_SelectedNodesChanged;
            treeEvents.DoubleClick += treeEvents_DoubleClick;
            // 
            // darkPanel1
            // 
            darkPanel1.Controls.Add(butSearch);
            darkPanel1.Controls.Add(butUnassignEventSet);
            darkPanel1.Controls.Add(butDeleteEventSet);
            darkPanel1.Controls.Add(butCloneEventSet);
            darkPanel1.Controls.Add(butNewFolder);
            darkPanel1.Controls.Add(butNewEventSet);
            darkPanel1.Dock = DockStyle.Top;
            darkPanel1.Location = new System.Drawing.Point(1, 1);
            darkPanel1.Name = "darkPanel1";
            darkPanel1.Size = new System.Drawing.Size(256, 30);
            darkPanel1.TabIndex = 1;
            // 
            // butSearch
            // 
            butSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            butSearch.Checked = false;
            butSearch.Image = Properties.Resources.general_search_16;
            butSearch.Location = new System.Drawing.Point(200, 3);
            butSearch.Name = "butSearch";
            butSearch.Size = new System.Drawing.Size(23, 23);
            butSearch.TabIndex = 26;
            toolTip.SetToolTip(butSearch, "Search for event set");
            butSearch.Click += butSearch_Click;
            // 
            // butUnassignEventSet
            // 
            butUnassignEventSet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            butUnassignEventSet.Checked = false;
            butUnassignEventSet.Image = Properties.Resources.actions_delete_16;
            butUnassignEventSet.Location = new System.Drawing.Point(229, 3);
            butUnassignEventSet.Name = "butUnassignEventSet";
            butUnassignEventSet.Size = new System.Drawing.Size(23, 23);
            butUnassignEventSet.TabIndex = 25;
            toolTip.SetToolTip(butUnassignEventSet, "Unassign event set from volume");
            butUnassignEventSet.Click += butUnassignEventSet_Click;
            // 
            // butDeleteEventSet
            // 
            butDeleteEventSet.Checked = false;
            butDeleteEventSet.Image = Properties.Resources.general_trash_16;
            butDeleteEventSet.Location = new System.Drawing.Point(91, 3);
            butDeleteEventSet.Name = "butDeleteEventSet";
            butDeleteEventSet.Size = new System.Drawing.Size(23, 23);
            butDeleteEventSet.TabIndex = 20;
            butDeleteEventSet.Tag = "AddNewRoom";
            toolTip.SetToolTip(butDeleteEventSet, "Delete selected event set or folder");
            butDeleteEventSet.Click += butDeleteEventSet_Click;
            // 
            // butCloneEventSet
            // 
            butCloneEventSet.Checked = false;
            butCloneEventSet.DialogResult = DialogResult.Cancel;
            butCloneEventSet.Image = Properties.Resources.general_copy_16;
            butCloneEventSet.Location = new System.Drawing.Point(62, 3);
            butCloneEventSet.Name = "butCloneEventSet";
            butCloneEventSet.Size = new System.Drawing.Size(23, 23);
            butCloneEventSet.TabIndex = 19;
            butCloneEventSet.Tag = "AddNewRoom";
            toolTip.SetToolTip(butCloneEventSet, "Copy selected event set");
            butCloneEventSet.Click += butCloneEventSet_Click;
			// 
			// butNewFolder
			// 
			butNewFolder.Checked = false;
            butNewFolder.Image = Properties.Resources.general_Open_16;
            butNewFolder.Location = new System.Drawing.Point(33, 3);
            butNewFolder.Name = "butNewFolder";
            butNewFolder.Size = new System.Drawing.Size(23, 23);
			butNewFolder.TabIndex = 27;
            toolTip.SetToolTip(butNewFolder, "Add new folder");
			butNewFolder.Click += butNewFolder_Click;
            // 
            // butNewEventSet
            // 
            butNewEventSet.Checked = false;
            butNewEventSet.Image = Properties.Resources.general_plus_math_16;
            butNewEventSet.Location = new System.Drawing.Point(4, 3);
            butNewEventSet.Name = "butNewEventSet";
            butNewEventSet.Size = new System.Drawing.Size(23, 23);
            butNewEventSet.TabIndex = 18;
            butNewEventSet.Tag = "EditRoomName";
            toolTip.SetToolTip(butNewEventSet, "Add new event set");
            butNewEventSet.Click += butNewEventSet_Click;
            // 
            // triggerManager
            // 
            triggerManager.Dock = DockStyle.Fill;
            triggerManager.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            triggerManager.Location = new System.Drawing.Point(1, 1);
            triggerManager.Name = "triggerManager";
            triggerManager.Padding = new Padding(2, 2, 2, 1);
            triggerManager.Size = new System.Drawing.Size(638, 310);
            triggerManager.TabIndex = 0;
            // 
            // lblActivators
            // 
            lblActivators.AutoSize = true;
            lblActivators.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblActivators.Location = new System.Drawing.Point(5, 8);
            lblActivators.Name = "lblActivators";
            lblActivators.Size = new System.Drawing.Size(60, 13);
            lblActivators.TabIndex = 26;
            lblActivators.Text = "Activators:";
            // 
            // cbEnableVolume
            // 
            cbEnableVolume.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cbEnableVolume.Location = new System.Drawing.Point(9, 391);
            cbEnableVolume.Name = "cbEnableVolume";
            cbEnableVolume.Size = new System.Drawing.Size(93, 17);
            cbEnableVolume.TabIndex = 33;
            cbEnableVolume.Text = "Enable volume";
            toolTip.SetToolTip(cbEnableVolume, "Indicates if selected volume is enabled by default");
            cbEnableVolume.CheckedChanged += cbEnableVolume_CheckedChanged;
            // 
            // cbAdjacentRooms
            // 
            cbAdjacentRooms.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cbAdjacentRooms.Location = new System.Drawing.Point(119, 391);
            cbAdjacentRooms.Name = "cbAdjacentRooms";
            cbAdjacentRooms.Size = new System.Drawing.Size(185, 17);
            cbAdjacentRooms.TabIndex = 34;
            cbAdjacentRooms.Text = "Detect in adjacent rooms";
            toolTip.SetToolTip(cbAdjacentRooms, "Detects volume interaction if activator resides in adjacent rooms");
            cbAdjacentRooms.CheckedChanged += cbAdjacentRooms_CheckedChanged;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(1, 10);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(87, 13);
            darkLabel1.TabIndex = 27;
            darkLabel1.Text = "Event set name:";
            // 
            // tbName
            // 
            tbName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(89, 7);
            tbName.Name = "tbName";
            tbName.SelectOnClick = true;
            tbName.Size = new System.Drawing.Size(349, 22);
            tbName.TabIndex = 28;
            tbName.Validated += tbName_Validated;
            // 
            // darkLabel2
            // 
            darkLabel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(442, 10);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(38, 13);
            darkLabel2.TabIndex = 29;
            darkLabel2.Text = "Event:";
            // 
            // cbEvents
            // 
            cbEvents.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbEvents.FormattingEnabled = true;
            cbEvents.Location = new System.Drawing.Point(480, 6);
            cbEvents.Name = "cbEvents";
            cbEvents.Size = new System.Drawing.Size(161, 23);
            cbEvents.TabIndex = 30;
            cbEvents.SelectedIndexChanged += cbEvents_SelectedIndexChanged;
            cbEvents.Format += cbEvents_Format;
            // 
            // panelEditor
            // 
            panelEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelEditor.Controls.Add(triggerManager);
            panelEditor.Location = new System.Drawing.Point(1, 35);
            panelEditor.Name = "panelEditor";
            panelEditor.SectionHeader = null;
            panelEditor.Size = new System.Drawing.Size(640, 312);
            panelEditor.TabIndex = 31;
            // 
            // splitContainer
            // 
            splitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer.Location = new System.Drawing.Point(1, 1);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(panelList);
            splitContainer.Panel1MinSize = 175;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(panelActivators);
            splitContainer.Panel2.Controls.Add(darkLabel1);
            splitContainer.Panel2.Controls.Add(panelEditor);
            splitContainer.Panel2.Controls.Add(cbEvents);
            splitContainer.Panel2.Controls.Add(tbName);
            splitContainer.Panel2.Controls.Add(darkLabel2);
            splitContainer.Panel2MinSize = 512;
            splitContainer.Size = new System.Drawing.Size(912, 387);
            splitContainer.SplitterDistance = 262;
            splitContainer.TabIndex = 32;
            splitContainer.SplitterMoved += splitContainer_SplitterMoved;
            // 
            // panelActivators
            // 
            panelActivators.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelActivators.Controls.Add(lblActivators);
            panelActivators.Controls.Add(cbActivatorNPC);
            panelActivators.Controls.Add(cbActivatorLara);
            panelActivators.Controls.Add(cbActivatorFlyBy);
            panelActivators.Controls.Add(cbActivatorStatics);
            panelActivators.Controls.Add(cbActivatorOtherMoveables);
            panelActivators.Location = new System.Drawing.Point(1, 350);
            panelActivators.Name = "panelActivators";
            panelActivators.SectionHeader = null;
            panelActivators.Size = new System.Drawing.Size(640, 30);
            panelActivators.TabIndex = 35;
            // 
            // FormEventSetEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(914, 421);
            Controls.Add(cbAdjacentRooms);
            Controls.Add(cbEnableVolume);
            Controls.Add(splitContainer);
            Controls.Add(butCancel);
            Controls.Add(butOk);
            KeyPreview = true;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(930, 460);
            Name = "FormEventSetEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            panelList.ResumeLayout(false);
            darkPanel1.ResumeLayout(false);
            panelEditor.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelActivators.ResumeLayout(false);
            panelActivators.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkCheckBox cbActivatorLara;
        private DarkUI.Controls.DarkCheckBox cbActivatorNPC;
        private DarkUI.Controls.DarkCheckBox cbActivatorOtherMoveables;
        private DarkUI.Controls.DarkCheckBox cbActivatorStatics;
        private DarkUI.Controls.DarkCheckBox cbActivatorFlyBy;
        private DarkSectionPanel panelList;
        private DarkTreeView treeEvents;
        private Controls.TriggerManager triggerManager;
        private DarkPanel darkPanel1;
        private DarkButton butDeleteEventSet;
        private DarkButton butCloneEventSet;
        private DarkButton butNewFolder;
        private DarkButton butNewEventSet;
        private DarkLabel lblActivators;
        private DarkButton butUnassignEventSet;
        private ToolTip toolTip;
        private DarkLabel darkLabel1;
        private DarkTextBox tbName;
        private DarkButton butSearch;
        private DarkLabel darkLabel2;
        private DarkComboBox cbEvents;
        private DarkSectionPanel panelEditor;
        private SplitContainer splitContainer;
        private DarkCheckBox cbEnableVolume;
        private DarkCheckBox cbAdjacentRooms;
        private DarkSectionPanel panelActivators;
    }
}
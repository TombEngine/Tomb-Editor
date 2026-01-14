
namespace TombEditor.Controls
{
    partial class TriggerManager
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
            darkPanel3 = new DarkUI.Controls.DarkPanel();
            rbLevelScript = new DarkUI.Controls.DarkRadioButton();
            lblNotify = new DarkUI.Controls.DarkLabel();
            butUnassign = new DarkUI.Controls.DarkButton();
            butSearch = new DarkUI.Controls.DarkButton();
            rbNodeEditor = new DarkUI.Controls.DarkRadioButton();
            toolTip = new System.Windows.Forms.ToolTip(components);
            nudCallCount = new DarkUI.Controls.DarkNumericUpDown();
            tbArgument = new DarkUI.Controls.DarkTextBox();
            butAddActionNode = new DarkUI.Controls.DarkButton();
            butAddConditionNode = new DarkUI.Controls.DarkButton();
            butDeleteNode = new DarkUI.Controls.DarkButton();
            nudCallCount2 = new DarkUI.Controls.DarkNumericUpDown();
            butClearNodes = new DarkUI.Controls.DarkButton();
            butRenameNode = new DarkUI.Controls.DarkButton();
            butChangeNodeColor = new DarkUI.Controls.DarkButton();
            butLinkSelectedNodes = new DarkUI.Controls.DarkButton();
            butExport = new DarkUI.Controls.DarkButton();
            butLockNodes = new DarkUI.Controls.DarkButton();
            cbEnableEvent = new DarkUI.Controls.DarkCheckBox();
            cbEnableEvent2 = new DarkUI.Controls.DarkCheckBox();
            tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            tabConstructor = new System.Windows.Forms.TabPage();
            panelNodeControls = new DarkUI.Controls.DarkPanel();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            nodeEditor = new TombLib.Controls.VisualScripting.NodeEditor();
            lblWait = new DarkUI.Controls.DarkLabel();
            tabLevelScript = new System.Windows.Forms.TabPage();
            lstFunctions = new DarkUI.Controls.DarkListView();
            panelFunctionControls = new DarkUI.Controls.DarkPanel();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            lblListNotify = new DarkUI.Controls.DarkLabel();
            darkPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudCallCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudCallCount2).BeginInit();
            tabbedContainer.SuspendLayout();
            tabConstructor.SuspendLayout();
            panelNodeControls.SuspendLayout();
            tabLevelScript.SuspendLayout();
            panelFunctionControls.SuspendLayout();
            SuspendLayout();
            // 
            // darkPanel3
            // 
            darkPanel3.Controls.Add(rbLevelScript);
            darkPanel3.Controls.Add(lblNotify);
            darkPanel3.Controls.Add(butUnassign);
            darkPanel3.Controls.Add(butSearch);
            darkPanel3.Controls.Add(rbNodeEditor);
            darkPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            darkPanel3.Location = new System.Drawing.Point(0, 0);
            darkPanel3.Name = "darkPanel3";
            darkPanel3.Size = new System.Drawing.Size(758, 25);
            darkPanel3.TabIndex = 21;
            // 
            // rbLevelScript
            // 
            rbLevelScript.AutoSize = true;
            rbLevelScript.Location = new System.Drawing.Point(103, 5);
            rbLevelScript.Name = "rbLevelScript";
            rbLevelScript.Size = new System.Drawing.Size(133, 17);
            rbLevelScript.TabIndex = 0;
            rbLevelScript.Text = "Level script functions";
            toolTip.SetToolTip(rbLevelScript, "Select from functions in level script file");
            rbLevelScript.CheckedChanged += rbLevelScript_CheckedChanged;
            // 
            // lblNotify
            // 
            lblNotify.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblNotify.AutoEllipsis = true;
            lblNotify.ForeColor = System.Drawing.Color.DarkGray;
            lblNotify.Location = new System.Drawing.Point(236, 6);
            lblNotify.Name = "lblNotify";
            lblNotify.Size = new System.Drawing.Size(461, 13);
            lblNotify.TabIndex = 4;
            lblNotify.Text = "Event notify";
            lblNotify.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            toolTip.SetToolTip(lblNotify, "A function which was linked to current event was not found in script file.\r\nPlease select another one or restore linked function in script.");
            lblNotify.Visible = false;
            // 
            // butUnassign
            // 
            butUnassign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butUnassign.Checked = false;
            butUnassign.Image = Properties.Resources.actions_delete_16;
            butUnassign.Location = new System.Drawing.Point(703, 2);
            butUnassign.Name = "butUnassign";
            butUnassign.Size = new System.Drawing.Size(23, 23);
            butUnassign.TabIndex = 3;
            toolTip.SetToolTip(butUnassign, "Unassign function from event");
            butUnassign.Click += butUnassign_Click;
            // 
            // butSearch
            // 
            butSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSearch.Checked = false;
            butSearch.Image = Properties.Resources.general_search_16;
            butSearch.Location = new System.Drawing.Point(732, 2);
            butSearch.Name = "butSearch";
            butSearch.Size = new System.Drawing.Size(23, 23);
            butSearch.TabIndex = 2;
            toolTip.SetToolTip(butSearch, "Search for item");
            butSearch.Click += butSearch_Click;
            // 
            // rbNodeEditor
            // 
            rbNodeEditor.AutoSize = true;
            rbNodeEditor.Checked = true;
            rbNodeEditor.Location = new System.Drawing.Point(5, 5);
            rbNodeEditor.Name = "rbNodeEditor";
            rbNodeEditor.Size = new System.Drawing.Size(87, 17);
            rbNodeEditor.TabIndex = 1;
            rbNodeEditor.TabStop = true;
            rbNodeEditor.Text = "Node editor";
            toolTip.SetToolTip(rbNodeEditor, "Visually construct an event");
            rbNodeEditor.CheckedChanged += rbNodeEditor_CheckedChanged;
            // 
            // nudCallCount
            // 
            nudCallCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudCallCount.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudCallCount.Location = new System.Drawing.Point(613, 2);
            nudCallCount.LoopValues = false;
            nudCallCount.Name = "nudCallCount";
            nudCallCount.Size = new System.Drawing.Size(63, 22);
            nudCallCount.TabIndex = 3;
            toolTip.SetToolTip(nudCallCount, "Determines how many times an event will be called.\r\nIf 0, it means event will be called infinitely.");
            nudCallCount.ValueChanged += nudCallCount_ValueChanged;
            // 
            // tbArgument
            // 
            tbArgument.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbArgument.Location = new System.Drawing.Point(63, 2);
            tbArgument.Name = "tbArgument";
            tbArgument.Size = new System.Drawing.Size(476, 22);
            tbArgument.TabIndex = 0;
            toolTip.SetToolTip(tbArgument, "Determine function argument, if function supports it.\r\nMust be formatted according to lua syntax.");
            tbArgument.Validated += tbArgument_Validated;
            // 
            // butAddActionNode
            // 
            butAddActionNode.Checked = false;
            butAddActionNode.Image = Properties.Resources.general_plus_math_16;
            butAddActionNode.Location = new System.Drawing.Point(0, 1);
            butAddActionNode.Name = "butAddActionNode";
            butAddActionNode.Size = new System.Drawing.Size(87, 23);
            butAddActionNode.TabIndex = 5;
            butAddActionNode.Text = "Action";
            butAddActionNode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butAddActionNode, "Add new action node");
            butAddActionNode.Click += butAddActionNode_Click;
            // 
            // butAddConditionNode
            // 
            butAddConditionNode.Checked = false;
            butAddConditionNode.Image = Properties.Resources.general_plus_math_16;
            butAddConditionNode.Location = new System.Drawing.Point(93, 1);
            butAddConditionNode.Name = "butAddConditionNode";
            butAddConditionNode.Size = new System.Drawing.Size(87, 23);
            butAddConditionNode.TabIndex = 6;
            butAddConditionNode.Text = "Condition";
            butAddConditionNode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            toolTip.SetToolTip(butAddConditionNode, "Add new condition node");
            butAddConditionNode.Click += butAddConditionNode_Click;
            // 
            // butDeleteNode
            // 
            butDeleteNode.Checked = false;
            butDeleteNode.Image = Properties.Resources.general_trash_16;
            butDeleteNode.Location = new System.Drawing.Point(302, 1);
            butDeleteNode.Name = "butDeleteNode";
            butDeleteNode.Size = new System.Drawing.Size(23, 23);
            butDeleteNode.TabIndex = 7;
            toolTip.SetToolTip(butDeleteNode, "Delete selected nodes");
            butDeleteNode.Click += butDeleteNode_Click;
            // 
            // nudCallCount2
            // 
            nudCallCount2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudCallCount2.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudCallCount2.Location = new System.Drawing.Point(613, 2);
            nudCallCount2.LoopValues = false;
            nudCallCount2.Name = "nudCallCount2";
            nudCallCount2.Size = new System.Drawing.Size(63, 22);
            nudCallCount2.TabIndex = 3;
            toolTip.SetToolTip(nudCallCount2, "Determines how many times an event will be called.\r\nIf 0, it means event will be called infinitely.");
            nudCallCount2.ValueChanged += nudCallCount_ValueChanged;
            // 
            // butClearNodes
            // 
            butClearNodes.Checked = false;
            butClearNodes.Image = Properties.Resources.actions_delete_16;
            butClearNodes.Location = new System.Drawing.Point(331, 1);
            butClearNodes.Name = "butClearNodes";
            butClearNodes.Size = new System.Drawing.Size(23, 23);
            butClearNodes.TabIndex = 8;
            toolTip.SetToolTip(butClearNodes, "Clear all nodes");
            butClearNodes.Click += butClearNodes_Click;
            // 
            // butRenameNode
            // 
            butRenameNode.Checked = false;
            butRenameNode.Image = Properties.Resources.general_edit_16;
            butRenameNode.Location = new System.Drawing.Point(215, 1);
            butRenameNode.Name = "butRenameNode";
            butRenameNode.Size = new System.Drawing.Size(23, 23);
            butRenameNode.TabIndex = 9;
            toolTip.SetToolTip(butRenameNode, "Rename last selected node");
            butRenameNode.Click += butRenameNode_Click;
            // 
            // butChangeNodeColor
            // 
            butChangeNodeColor.Checked = false;
            butChangeNodeColor.Image = Properties.Resources.actions_TextureMode_16;
            butChangeNodeColor.Location = new System.Drawing.Point(244, 1);
            butChangeNodeColor.Name = "butChangeNodeColor";
            butChangeNodeColor.Size = new System.Drawing.Size(23, 23);
            butChangeNodeColor.TabIndex = 10;
            toolTip.SetToolTip(butChangeNodeColor, "Change color for last selected node");
            butChangeNodeColor.Click += butChangeNodeColor_Click;
            // 
            // butLinkSelectedNodes
            // 
            butLinkSelectedNodes.Checked = false;
            butLinkSelectedNodes.Image = Properties.Resources.actions_Merge_16;
            butLinkSelectedNodes.Location = new System.Drawing.Point(186, 1);
            butLinkSelectedNodes.Name = "butLinkSelectedNodes";
            butLinkSelectedNodes.Size = new System.Drawing.Size(23, 23);
            butLinkSelectedNodes.TabIndex = 11;
            toolTip.SetToolTip(butLinkSelectedNodes, "Link selected nodes, if possible");
            butLinkSelectedNodes.Click += butLinkSelectedNodes_Click;
            // 
            // butExport
            // 
            butExport.Checked = false;
            butExport.Image = Properties.Resources.general_copy_16;
            butExport.Location = new System.Drawing.Point(360, 1);
            butExport.Name = "butExport";
            butExport.Size = new System.Drawing.Size(23, 23);
            butExport.TabIndex = 12;
            toolTip.SetToolTip(butExport, "Export lua script to clipboard");
            butExport.Click += butExport_Click;
            // 
            // butLockNodes
            // 
            butLockNodes.Checked = false;
            butLockNodes.Image = Properties.Resources.general_Lock_16;
            butLockNodes.Location = new System.Drawing.Point(273, 1);
            butLockNodes.Name = "butLockNodes";
            butLockNodes.Size = new System.Drawing.Size(23, 23);
            butLockNodes.TabIndex = 13;
            toolTip.SetToolTip(butLockNodes, "Lock selected nodes from modifying");
            butLockNodes.Click += butLockNodes_Click;
            // 
            // cbEnableEvent
            // 
            cbEnableEvent.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbEnableEvent.Location = new System.Drawing.Point(684, 4);
            cbEnableEvent.Name = "cbEnableEvent";
            cbEnableEvent.Size = new System.Drawing.Size(61, 17);
            cbEnableEvent.TabIndex = 34;
            cbEnableEvent.Text = "Enable";
            toolTip.SetToolTip(cbEnableEvent, "Indicates if selected event is enabled by default");
            cbEnableEvent.CheckedChanged += cbEnableEvent_CheckedChanged;
            // 
            // cbEnableEvent2
            // 
            cbEnableEvent2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cbEnableEvent2.Location = new System.Drawing.Point(684, 4);
            cbEnableEvent2.Name = "cbEnableEvent2";
            cbEnableEvent2.Size = new System.Drawing.Size(61, 17);
            cbEnableEvent2.TabIndex = 35;
            cbEnableEvent2.Text = "Enable";
            toolTip.SetToolTip(cbEnableEvent2, "Indicates if selected event is enabled by default");
            cbEnableEvent2.CheckedChanged += cbEnableEvent_CheckedChanged;
            // 
            // tabbedContainer
            // 
            tabbedContainer.Controls.Add(tabConstructor);
            tabbedContainer.Controls.Add(tabLevelScript);
            tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            tabbedContainer.Location = new System.Drawing.Point(0, 25);
            tabbedContainer.Name = "tabbedContainer";
            tabbedContainer.SelectedIndex = 0;
            tabbedContainer.Size = new System.Drawing.Size(758, 375);
            tabbedContainer.TabIndex = 1;
            // 
            // tabConstructor
            // 
            tabConstructor.Controls.Add(panelNodeControls);
            tabConstructor.Controls.Add(nodeEditor);
            tabConstructor.Controls.Add(lblWait);
            tabConstructor.Location = new System.Drawing.Point(4, 22);
            tabConstructor.Name = "tabConstructor";
            tabConstructor.Padding = new System.Windows.Forms.Padding(3);
            tabConstructor.Size = new System.Drawing.Size(750, 349);
            tabConstructor.TabIndex = 13;
            tabConstructor.Text = "Node editor";
            tabConstructor.UseVisualStyleBackColor = true;
            // 
            // panelNodeControls
            // 
            panelNodeControls.Controls.Add(cbEnableEvent);
            panelNodeControls.Controls.Add(butLockNodes);
            panelNodeControls.Controls.Add(butExport);
            panelNodeControls.Controls.Add(butLinkSelectedNodes);
            panelNodeControls.Controls.Add(butChangeNodeColor);
            panelNodeControls.Controls.Add(butRenameNode);
            panelNodeControls.Controls.Add(butClearNodes);
            panelNodeControls.Controls.Add(nudCallCount2);
            panelNodeControls.Controls.Add(butDeleteNode);
            panelNodeControls.Controls.Add(darkLabel1);
            panelNodeControls.Controls.Add(butAddActionNode);
            panelNodeControls.Controls.Add(butAddConditionNode);
            panelNodeControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelNodeControls.Location = new System.Drawing.Point(3, 321);
            panelNodeControls.Name = "panelNodeControls";
            panelNodeControls.Size = new System.Drawing.Size(744, 25);
            panelNodeControls.TabIndex = 8;
            // 
            // darkLabel1
            // 
            darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(545, 4);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(62, 13);
            darkLabel1.TabIndex = 2;
            darkLabel1.Text = "Call count:";
            // 
            // nodeEditor
            // 
            nodeEditor.AllowDrop = true;
            nodeEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            nodeEditor.DefaultNodeWidth = 400;
            nodeEditor.GridSize = 256;
            nodeEditor.GridStep = 8F;
            nodeEditor.LinksAsRopes = false;
            nodeEditor.Location = new System.Drawing.Point(3, 5);
            nodeEditor.Name = "nodeEditor";
            nodeEditor.SelectionColor = System.Drawing.Color.FromArgb(75, 110, 175);
            nodeEditor.ShowGrips = false;
            nodeEditor.Size = new System.Drawing.Size(744, 310);
            nodeEditor.TabIndex = 0;
            // 
            // lblWait
            // 
            lblWait.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblWait.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblWait.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblWait.Location = new System.Drawing.Point(3, 5);
            lblWait.Name = "lblWait";
            lblWait.Size = new System.Drawing.Size(744, 310);
            lblWait.TabIndex = 9;
            lblWait.Text = "Please wait...";
            lblWait.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabLevelScript
            // 
            tabLevelScript.Controls.Add(lstFunctions);
            tabLevelScript.Controls.Add(panelFunctionControls);
            tabLevelScript.Controls.Add(lblListNotify);
            tabLevelScript.Location = new System.Drawing.Point(4, 22);
            tabLevelScript.Name = "tabLevelScript";
            tabLevelScript.Padding = new System.Windows.Forms.Padding(3);
            tabLevelScript.Size = new System.Drawing.Size(750, 349);
            tabLevelScript.TabIndex = 12;
            tabLevelScript.Text = "Level script functions";
            tabLevelScript.UseVisualStyleBackColor = true;
            // 
            // lstFunctions
            // 
            lstFunctions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstFunctions.Location = new System.Drawing.Point(3, 5);
            lstFunctions.Name = "lstFunctions";
            lstFunctions.Size = new System.Drawing.Size(744, 312);
            lstFunctions.TabIndex = 0;
            lstFunctions.Text = "darkListView1";
            lstFunctions.SelectedIndicesChanged += lstFunctions_SelectedIndicesChanged;
            // 
            // panelFunctionControls
            // 
            panelFunctionControls.Controls.Add(cbEnableEvent2);
            panelFunctionControls.Controls.Add(nudCallCount);
            panelFunctionControls.Controls.Add(darkLabel3);
            panelFunctionControls.Controls.Add(darkLabel2);
            panelFunctionControls.Controls.Add(tbArgument);
            panelFunctionControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelFunctionControls.Location = new System.Drawing.Point(3, 321);
            panelFunctionControls.Name = "panelFunctionControls";
            panelFunctionControls.Size = new System.Drawing.Size(744, 25);
            panelFunctionControls.TabIndex = 1;
            // 
            // darkLabel3
            // 
            darkLabel3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel3.AutoSize = true;
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(545, 4);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(62, 13);
            darkLabel3.TabIndex = 2;
            darkLabel3.Text = "Call count:";
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(0, 4);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(61, 13);
            darkLabel2.TabIndex = 1;
            darkLabel2.Text = "Argument:";
            // 
            // lblListNotify
            // 
            lblListNotify.Dock = System.Windows.Forms.DockStyle.Fill;
            lblListNotify.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblListNotify.Location = new System.Drawing.Point(3, 3);
            lblListNotify.Name = "lblListNotify";
            lblListNotify.Size = new System.Drawing.Size(744, 343);
            lblListNotify.TabIndex = 2;
            lblListNotify.Text = "Function list notify";
            lblListNotify.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblListNotify.EnabledChanged += lblListNotify_EnabledChanged;
            lblListNotify.Click += lblListNotify_Click;
            // 
            // TriggerManager
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabbedContainer);
            Controls.Add(darkPanel3);
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Name = "TriggerManager";
            Size = new System.Drawing.Size(758, 400);
            darkPanel3.ResumeLayout(false);
            darkPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudCallCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudCallCount2).EndInit();
            tabbedContainer.ResumeLayout(false);
            tabConstructor.ResumeLayout(false);
            panelNodeControls.ResumeLayout(false);
            panelNodeControls.PerformLayout();
            tabLevelScript.ResumeLayout(false);
            panelFunctionControls.ResumeLayout(false);
            panelFunctionControls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private DarkUI.Controls.DarkPanel darkPanel3;
        private DarkUI.Controls.DarkRadioButton rbNodeEditor;
        private DarkUI.Controls.DarkRadioButton rbLevelScript;
        private DarkUI.Controls.DarkButton butSearch;
        private DarkUI.Controls.DarkButton butUnassign;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkLabel lblNotify;
        private System.Windows.Forms.TabPage tabLevelScript;
        private DarkUI.Controls.DarkListView lstFunctions;
        private DarkUI.Controls.DarkPanel panelFunctionControls;
        private DarkUI.Controls.DarkNumericUpDown nudCallCount;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbArgument;
        private System.Windows.Forms.TabPage tabConstructor;
        private DarkUI.Controls.DarkLabel lblListNotify;
        private TombLib.Controls.VisualScripting.NodeEditor nodeEditor;
        private DarkUI.Controls.DarkButton butDeleteNode;
        private DarkUI.Controls.DarkButton butAddConditionNode;
        private DarkUI.Controls.DarkButton butAddActionNode;
        private DarkUI.Controls.DarkPanel panelNodeControls;
        private DarkUI.Controls.DarkButton butClearNodes;
        private DarkUI.Controls.DarkNumericUpDown nudCallCount2;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butRenameNode;
        private DarkUI.Controls.DarkButton butChangeNodeColor;
        private DarkUI.Controls.DarkButton butLinkSelectedNodes;
        private DarkUI.Controls.DarkLabel lblWait;
        private DarkUI.Controls.DarkButton butExport;
        private DarkUI.Controls.DarkButton butLockNodes;
        private DarkUI.Controls.DarkCheckBox cbEnableEvent;
        private DarkUI.Controls.DarkCheckBox cbEnableEvent2;
    }
}

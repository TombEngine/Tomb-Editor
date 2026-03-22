namespace TombEditor.Features.DockableViews.TriggerList
{
    partial class TriggerList
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
            this.components = new System.ComponentModel.Container();
            this.panelTriggerTools = new System.Windows.Forms.Panel();
            this.butAddTrigger = new DarkUI.Controls.DarkButton();
            this.butEditTrigger = new DarkUI.Controls.DarkButton();
            this.butDeleteTrigger = new DarkUI.Controls.DarkButton();
            this.panelTriggerList = new System.Windows.Forms.Panel();
            this.lstTriggers = new DarkUI.Controls.DarkListView();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelTriggerTools.SuspendLayout();
            this.panelTriggerList.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTriggerTools
            // 
            this.panelTriggerTools.Controls.Add(this.butAddTrigger);
            this.panelTriggerTools.Controls.Add(this.butEditTrigger);
            this.panelTriggerTools.Controls.Add(this.butDeleteTrigger);
            this.panelTriggerTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTriggerTools.Location = new System.Drawing.Point(0, 143);
            this.panelTriggerTools.Name = "panelTriggerTools";
            this.panelTriggerTools.Size = new System.Drawing.Size(284, 31);
            this.panelTriggerTools.TabIndex = 57;
            // 
            // butAddTrigger
            // 
            this.butAddTrigger.Checked = false;
            this.butAddTrigger.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddTrigger.Location = new System.Drawing.Point(3, 4);
            this.butAddTrigger.Name = "butAddTrigger";
            this.butAddTrigger.Size = new System.Drawing.Size(24, 24);
            this.butAddTrigger.TabIndex = 1;
            this.butAddTrigger.Tag = "AddTrigger";
            this.butAddTrigger.MouseEnter += new System.EventHandler(this.butAddTrigger_MouseEnter);
            // 
            // butEditTrigger
            // 
            this.butEditTrigger.Checked = false;
            this.butEditTrigger.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditTrigger.Location = new System.Drawing.Point(33, 4);
            this.butEditTrigger.Name = "butEditTrigger";
            this.butEditTrigger.Size = new System.Drawing.Size(24, 24);
            this.butEditTrigger.TabIndex = 2;
            this.toolTip.SetToolTip(this.butEditTrigger, "Edit selected trigger");
            this.butEditTrigger.Click += new System.EventHandler(this.butEditTrigger_Click);
            // 
            // butDeleteTrigger
            // 
            this.butDeleteTrigger.Checked = false;
            this.butDeleteTrigger.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteTrigger.Location = new System.Drawing.Point(63, 4);
            this.butDeleteTrigger.Name = "butDeleteTrigger";
            this.butDeleteTrigger.Size = new System.Drawing.Size(24, 24);
            this.butDeleteTrigger.TabIndex = 3;
            this.toolTip.SetToolTip(this.butDeleteTrigger, "Delete trigger");
            this.butDeleteTrigger.Click += new System.EventHandler(this.butDeleteTrigger_Click);
            // 
            // panelTriggerList
            // 
            this.panelTriggerList.Controls.Add(this.lstTriggers);
            this.panelTriggerList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTriggerList.Location = new System.Drawing.Point(0, 25);
            this.panelTriggerList.Name = "panelTriggerList";
            this.panelTriggerList.Padding = new System.Windows.Forms.Padding(3, 2, 2, 2);
            this.panelTriggerList.Size = new System.Drawing.Size(284, 118);
            this.panelTriggerList.TabIndex = 58;
            // 
            // lstTriggers
            // 
            this.lstTriggers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTriggers.Location = new System.Drawing.Point(3, 2);
            this.lstTriggers.MouseWheelScrollSpeedV = 0.2F;
            this.lstTriggers.MultiSelect = true;
            this.lstTriggers.Name = "lstTriggers";
            this.lstTriggers.Size = new System.Drawing.Size(279, 114);
            this.lstTriggers.TabIndex = 0;
            this.lstTriggers.Text = "darkListView1";
            this.lstTriggers.SelectedIndicesChanged += new System.EventHandler(this.lstTriggers_SelectedIndicesChanged);
            this.lstTriggers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstTriggers_KeyDown);
            this.lstTriggers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstTriggers_MouseDoubleClick);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // TriggerList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.panelTriggerList);
            this.Controls.Add(this.panelTriggerTools);
            this.DockText = "Classic Triggers";
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "TriggerList";
            this.SerializationKey = "TriggerList";
            this.Size = new System.Drawing.Size(284, 174);
            this.panelTriggerTools.ResumeLayout(false);
            this.panelTriggerList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelTriggerTools;
        private DarkUI.Controls.DarkButton butAddTrigger;
        private DarkUI.Controls.DarkButton butEditTrigger;
        private DarkUI.Controls.DarkButton butDeleteTrigger;
        private System.Windows.Forms.Panel panelTriggerList;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkListView lstTriggers;
    }
}

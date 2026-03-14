namespace TombEditor.Features.DockableViews.ObjectList
{
    partial class ObjectList
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
            this.butEditObject = new DarkUI.Controls.DarkButton();
            this.butDeleteObject = new DarkUI.Controls.DarkButton();
            this.panelTriggerList = new System.Windows.Forms.Panel();
            this.lstObjects = new DarkUI.Controls.DarkListView();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelTriggerTools.SuspendLayout();
            this.panelTriggerList.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTriggerTools
            // 
            this.panelTriggerTools.Controls.Add(this.butEditObject);
            this.panelTriggerTools.Controls.Add(this.butDeleteObject);
            this.panelTriggerTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTriggerTools.Location = new System.Drawing.Point(0, 143);
            this.panelTriggerTools.Name = "panelTriggerTools";
            this.panelTriggerTools.Size = new System.Drawing.Size(284, 31);
            this.panelTriggerTools.TabIndex = 57;
            // 
            // butEditObject
            // 
            this.butEditObject.Checked = false;
            this.butEditObject.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditObject.Location = new System.Drawing.Point(3, 3);
            this.butEditObject.Name = "butEditObject";
            this.butEditObject.Size = new System.Drawing.Size(24, 24);
            this.butEditObject.TabIndex = 1;
            this.toolTip.SetToolTip(this.butEditObject, "Edit selected object");
            this.butEditObject.Click += new System.EventHandler(this.butEditObject_Click);
            // 
            // butDeleteObject
            // 
            this.butDeleteObject.Checked = false;
            this.butDeleteObject.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteObject.Location = new System.Drawing.Point(33, 3);
            this.butDeleteObject.Name = "butDeleteObject";
            this.butDeleteObject.Size = new System.Drawing.Size(24, 24);
            this.butDeleteObject.TabIndex = 2;
            this.toolTip.SetToolTip(this.butDeleteObject, "Delete selected object");
            this.butDeleteObject.Click += new System.EventHandler(this.butDeleteObject_Click);
            // 
            // panelTriggerList
            // 
            this.panelTriggerList.Controls.Add(this.lstObjects);
            this.panelTriggerList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTriggerList.Location = new System.Drawing.Point(0, 25);
            this.panelTriggerList.Name = "panelTriggerList";
            this.panelTriggerList.Padding = new System.Windows.Forms.Padding(3, 2, 2, 2);
            this.panelTriggerList.Size = new System.Drawing.Size(284, 118);
            this.panelTriggerList.TabIndex = 58;
            // 
            // lstObjects
            // 
            this.lstObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstObjects.Location = new System.Drawing.Point(3, 2);
            this.lstObjects.MouseWheelScrollSpeedV = 0.2F;
            this.lstObjects.MultiSelect = true;
            this.lstObjects.Name = "lstObjects";
            this.lstObjects.Size = new System.Drawing.Size(279, 114);
            this.lstObjects.TabIndex = 0;
            this.lstObjects.Text = "darkListView1";
            this.lstObjects.SelectedIndicesChanged += new System.EventHandler(this.lstObjects_SelectedIndicesChanged);
            this.lstObjects.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstObjects_MouseDoubleClick);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // ObjectList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.panelTriggerList);
            this.Controls.Add(this.panelTriggerTools);
            this.DockText = "Objects in Room";
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "ObjectList";
            this.SerializationKey = "ObjectList";
            this.Size = new System.Drawing.Size(284, 174);
            this.panelTriggerTools.ResumeLayout(false);
            this.panelTriggerList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTriggerTools;
        private System.Windows.Forms.Panel panelTriggerList;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butEditObject;
        private DarkUI.Controls.DarkButton butDeleteObject;
        private DarkUI.Controls.DarkListView lstObjects;
    }
}

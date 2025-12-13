namespace TombIDE.ScriptingStudio.Settings
{
	partial class FormTextEditorSettings
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.button_Apply = new DarkUI.Controls.DarkButton();
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.button_ResetDefault = new DarkUI.Controls.DarkButton();
			this.panel_Buttons = new System.Windows.Forms.Panel();
			this.panel_Main = new System.Windows.Forms.Panel();
			this.tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			this.tabPage_Global = new System.Windows.Forms.TabPage();
			this.tabPage_ClassicScript = new System.Windows.Forms.TabPage();
			this.settingsClassicScript = new TombIDE.ScriptingStudio.Settings.ClassicScriptSettingsControl();
			this.tabPage_GameFlow = new System.Windows.Forms.TabPage();
			this.settingsGameFlow = new TombIDE.ScriptingStudio.Settings.GameFlowSettingsControl();
			this.tabPage_Tomb1Main = new System.Windows.Forms.TabPage();
			this.settingsTomb1Main = new TombIDE.ScriptingStudio.Settings.Tomb1MainSettingsControl();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.tabPage_Lua = new System.Windows.Forms.TabPage();
			this.settingsLua = new TombIDE.ScriptingStudio.Settings.LuaSettingsControl();
			this.panel_Buttons.SuspendLayout();
			this.panel_Main.SuspendLayout();
			this.tablessTabControl.SuspendLayout();
			this.tabPage_ClassicScript.SuspendLayout();
			this.tabPage_GameFlow.SuspendLayout();
			this.tabPage_Tomb1Main.SuspendLayout();
			this.tabPage_Lua.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Apply.Checked = false;
			this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Apply.Location = new System.Drawing.Point(726, 8);
			this.button_Apply.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(75, 24);
			this.button_Apply.TabIndex = 1;
			this.button_Apply.Text = "Apply";
			this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(807, 8);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 24);
			this.button_Cancel.TabIndex = 0;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_ResetDefault
			// 
			this.button_ResetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button_ResetDefault.Checked = false;
			this.button_ResetDefault.Location = new System.Drawing.Point(6, 8);
			this.button_ResetDefault.Margin = new System.Windows.Forms.Padding(6, 6, 0, 6);
			this.button_ResetDefault.Name = "button_ResetDefault";
			this.button_ResetDefault.Size = new System.Drawing.Size(150, 24);
			this.button_ResetDefault.TabIndex = 2;
			this.button_ResetDefault.Text = "Reset settings to default";
			this.button_ResetDefault.Click += new System.EventHandler(this.button_ResetDefault_Click);
			// 
			// panel_Buttons
			// 
			this.panel_Buttons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Buttons.Controls.Add(this.button_ResetDefault);
			this.panel_Buttons.Controls.Add(this.button_Apply);
			this.panel_Buttons.Controls.Add(this.button_Cancel);
			this.panel_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_Buttons.Location = new System.Drawing.Point(0, 414);
			this.panel_Buttons.Name = "panel_Buttons";
			this.panel_Buttons.Size = new System.Drawing.Size(890, 40);
			this.panel_Buttons.TabIndex = 3;
			// 
			// panel_Main
			// 
			this.panel_Main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Main.Controls.Add(this.tablessTabControl);
			this.panel_Main.Controls.Add(this.treeView);
			this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Main.Location = new System.Drawing.Point(0, 0);
			this.panel_Main.Name = "panel_Main";
			this.panel_Main.Size = new System.Drawing.Size(890, 414);
			this.panel_Main.TabIndex = 4;
			// 
			// tablessTabControl
			// 
			this.tablessTabControl.Controls.Add(this.tabPage_Global);
			this.tablessTabControl.Controls.Add(this.tabPage_ClassicScript);
			this.tablessTabControl.Controls.Add(this.tabPage_GameFlow);
			this.tablessTabControl.Controls.Add(this.tabPage_Tomb1Main);
			this.tablessTabControl.Controls.Add(this.tabPage_Lua);
			this.tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tablessTabControl.Location = new System.Drawing.Point(168, 0);
			this.tablessTabControl.Name = "tablessTabControl";
			this.tablessTabControl.SelectedIndex = 0;
			this.tablessTabControl.Size = new System.Drawing.Size(720, 412);
			this.tablessTabControl.TabIndex = 4;
			// 
			// tabPage_Global
			// 
			this.tabPage_Global.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Global.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Global.Name = "tabPage_Global";
			this.tabPage_Global.Size = new System.Drawing.Size(712, 386);
			this.tabPage_Global.TabIndex = 0;
			this.tabPage_Global.Text = "Global";
			// 
			// tabPage_ClassicScript
			// 
			this.tabPage_ClassicScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_ClassicScript.Controls.Add(this.settingsClassicScript);
			this.tabPage_ClassicScript.Location = new System.Drawing.Point(4, 22);
			this.tabPage_ClassicScript.Name = "tabPage_ClassicScript";
			this.tabPage_ClassicScript.Size = new System.Drawing.Size(712, 386);
			this.tabPage_ClassicScript.TabIndex = 1;
			this.tabPage_ClassicScript.Text = "Classic Script";
			// 
			// settingsClassicScript
			// 
			this.settingsClassicScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.settingsClassicScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsClassicScript.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsClassicScript.Location = new System.Drawing.Point(0, 0);
			this.settingsClassicScript.MaximumSize = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.MinimumSize = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.Name = "settingsClassicScript";
			this.settingsClassicScript.Size = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.TabIndex = 0;
			// 
			// tabPage_GameFlow
			// 
			this.tabPage_GameFlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_GameFlow.Controls.Add(this.settingsGameFlow);
			this.tabPage_GameFlow.Location = new System.Drawing.Point(4, 22);
			this.tabPage_GameFlow.Name = "tabPage_GameFlow";
			this.tabPage_GameFlow.Size = new System.Drawing.Size(712, 386);
			this.tabPage_GameFlow.TabIndex = 2;
			this.tabPage_GameFlow.Text = "Game Flow";
			// 
			// settingsGameFlow
			// 
			this.settingsGameFlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.settingsGameFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsGameFlow.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsGameFlow.Location = new System.Drawing.Point(0, 0);
			this.settingsGameFlow.MaximumSize = new System.Drawing.Size(720, 412);
			this.settingsGameFlow.MinimumSize = new System.Drawing.Size(720, 412);
			this.settingsGameFlow.Name = "settingsGameFlow";
			this.settingsGameFlow.Size = new System.Drawing.Size(720, 412);
			this.settingsGameFlow.TabIndex = 0;
			// 
			// tabPage_Tomb1Main
			// 
			this.tabPage_Tomb1Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Tomb1Main.Controls.Add(this.settingsTomb1Main);
			this.tabPage_Tomb1Main.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Tomb1Main.Margin = new System.Windows.Forms.Padding(0);
			this.tabPage_Tomb1Main.Name = "tabPage_Tomb1Main";
			this.tabPage_Tomb1Main.Size = new System.Drawing.Size(712, 386);
			this.tabPage_Tomb1Main.TabIndex = 3;
			this.tabPage_Tomb1Main.Text = "TR1X / TR2X";
			// 
			// settingsTomb1Main
			// 
			this.settingsTomb1Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.settingsTomb1Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsTomb1Main.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsTomb1Main.Location = new System.Drawing.Point(0, 0);
			this.settingsTomb1Main.Margin = new System.Windows.Forms.Padding(0);
			this.settingsTomb1Main.MaximumSize = new System.Drawing.Size(720, 412);
			this.settingsTomb1Main.MinimumSize = new System.Drawing.Size(720, 412);
			this.settingsTomb1Main.Name = "settingsTomb1Main";
			this.settingsTomb1Main.Size = new System.Drawing.Size(720, 412);
			this.settingsTomb1Main.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 48;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.MaxDragChange = 48;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(168, 412);
			this.treeView.TabIndex = 3;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			// 
			// tabPage_Lua
			// 
			this.tabPage_Lua.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Lua.Controls.Add(this.settingsLua);
			this.tabPage_Lua.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Lua.Margin = new System.Windows.Forms.Padding(0);
			this.tabPage_Lua.Name = "tabPage_Lua";
			this.tabPage_Lua.Size = new System.Drawing.Size(712, 386);
			this.tabPage_Lua.TabIndex = 4;
			this.tabPage_Lua.Text = "Lua";
			// 
			// settingsLua
			// 
			this.settingsLua.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.settingsLua.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsLua.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.settingsLua.Location = new System.Drawing.Point(0, 0);
			this.settingsLua.Margin = new System.Windows.Forms.Padding(0);
			this.settingsLua.MaximumSize = new System.Drawing.Size(720, 412);
			this.settingsLua.MinimumSize = new System.Drawing.Size(720, 412);
			this.settingsLua.Name = "settingsLua";
			this.settingsLua.Size = new System.Drawing.Size(720, 412);
			this.settingsLua.TabIndex = 1;
			// 
			// FormTextEditorSettings
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(890, 454);
			this.Controls.Add(this.panel_Main);
			this.Controls.Add(this.panel_Buttons);
			this.FlatBorder = true;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormTextEditorSettings";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Text Editor Settings";
			this.panel_Buttons.ResumeLayout(false);
			this.panel_Main.ResumeLayout(false);
			this.tablessTabControl.ResumeLayout(false);
			this.tabPage_ClassicScript.ResumeLayout(false);
			this.tabPage_GameFlow.ResumeLayout(false);
			this.tabPage_Tomb1Main.ResumeLayout(false);
			this.tabPage_Lua.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ClassicScriptSettingsControl settingsClassicScript;
		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_ResetDefault;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_Buttons;
		private System.Windows.Forms.Panel panel_Main;
		private System.Windows.Forms.TabPage tabPage_ClassicScript;
		private System.Windows.Forms.TabPage tabPage_Global;
		private System.Windows.Forms.TabPage tabPage_GameFlow;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
		private GameFlowSettingsControl settingsGameFlow;
		private System.Windows.Forms.TabPage tabPage_Tomb1Main;
		private Tomb1MainSettingsControl settingsTomb1Main;
		private System.Windows.Forms.TabPage tabPage_Lua;
		private LuaSettingsControl settingsLua;
	}
}
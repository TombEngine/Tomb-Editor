using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Settings
{
	// TODO: Refactor

	public partial class FormTextEditorSettings : DarkForm
	{
		private ConfigurationCollection configs = new ConfigurationCollection();

		public FormTextEditorSettings(StudioMode studioMode)
		{
			InitializeComponent();

			settingsClassicScript.Initialize(configs.ClassicScript);
			settingsGameFlow.Initialize(configs.GameFlowScript);
			settingsTomb1Main.Initialize(configs.Tomb1Main);
			settingsLua.Initialize(configs.Lua);

			var classicScriptNode = new DarkTreeNode("TR4 / TRNG Script");
			var gameFlowNode = new DarkTreeNode("TR2 / TR3 Script");
			var tomb1MainNode = new DarkTreeNode("TR1X / TR2X Script");
			var luaNode = new DarkTreeNode("Lua");

			treeView.Nodes.Add(classicScriptNode);
			treeView.Nodes.Add(gameFlowNode);
			treeView.Nodes.Add(tomb1MainNode);
			treeView.Nodes.Add(luaNode);

			if (studioMode == StudioMode.ClassicScript)
				treeView.SelectNode(classicScriptNode);
			else if (studioMode == StudioMode.GameFlowScript)
				treeView.SelectNode(gameFlowNode);
			else if (studioMode == StudioMode.Tomb1Main)
				treeView.SelectNode(tomb1MainNode);
			else if (studioMode == StudioMode.Lua)
				treeView.SelectNode(luaNode);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (DialogResult == DialogResult.OK)
			{
				settingsClassicScript.ApplySettings(configs.ClassicScript);
				settingsGameFlow.ApplySettings(configs.GameFlowScript);
				settingsTomb1Main.ApplySettings(configs.Tomb1Main);
				settingsLua.ApplySettings(configs.Lua);
			}
			else
			{
				configs.SaveAllConfigs();
			}
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			configs.SaveAllConfigs();
		}

		private void button_ResetDefault_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to reset all settings for the selected language to default?", "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				if (treeView.SelectedNodes[0] == treeView.Nodes[0])
					settingsClassicScript.ResetToDefault();
				else if (treeView.SelectedNodes[0] == treeView.Nodes[1])
					settingsGameFlow.ResetToDefault();
				else if (treeView.SelectedNodes[0] == treeView.Nodes[2])
					settingsTomb1Main.ResetToDefault();
				else if (treeView.SelectedNodes[0] == treeView.Nodes[3])
					settingsLua.ResetToDefault();
			}
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (treeView.SelectedNodes[0] == treeView.Nodes[0])
			{
				tablessTabControl.SelectTab(1);
				settingsClassicScript.ForcePreviewUpdate();
			}
			else if (treeView.SelectedNodes[0] == treeView.Nodes[1])
			{
				tablessTabControl.SelectTab(2);
				settingsGameFlow.ForcePreviewUpdate();
			}
			else if (treeView.SelectedNodes[0] == treeView.Nodes[2])
			{
				tablessTabControl.SelectTab(3);
				settingsTomb1Main.ForcePreviewUpdate();
			}
			else if (treeView.SelectedNodes[0] == treeView.Nodes[3])
			{
				tablessTabControl.SelectTab(4);
				settingsLua.ForcePreviewUpdate();
			}
		}
	}
}

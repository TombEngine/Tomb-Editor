using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Controls
{
    public partial class TriggerManager : UserControl
    {
        public TriggerManager()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Event Event
        {
            get
            {
                return _event;
            }
            set
            {
                if (_event == value)
                    return;

                _event = value;
                UpdateNodes();
                UpdateUI();
            }
        }
        private Event _event = null;

        private Editor _editor;
        private bool _lockUI = false;
        private bool _nodeListRefreshPending = false;

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LoadedScriptsChangedEvent)
            {
                ReloadScriptFunctions();
                ReloadNodeFunctions();
                FindAndSelectFunction();
                UpdateNodeEditorOptions();
            }

            if (obj is Editor.RoomListChangedEvent ||
                obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.EventSetsChangedEvent ||
               (obj is Editor.ObjectChangedEvent && 
               (obj as Editor.ObjectChangedEvent).ChangeType != ObjectChangeType.Change))
            {
                if (!_nodeListRefreshPending && IsHandleCreated)
                {
                    _nodeListRefreshPending = true;
                    BeginInvoke((Action)(() =>
                    {
                        _nodeListRefreshPending = false;
                        nodeEditor.PopulateCachedNodeLists(_editor.Level);
                        nodeEditor.RefreshArgumentUI();
                    }));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WadSoundPlayer.StopSample();

                _editor.EditorEventRaised      -= EditorEventRaised;
                nodeEditor.ViewPositionChanged -= NodeEditor_ViewPostionChanged;
                nodeEditor.SelectionChanged    -= NodeEditor_SelectionChanged;
                nodeEditor.LocatedItemFound    -= NodeEditor_LocatedItemFound;
                nodeEditor.SoundEffectPlayed   -= NodeEditor_SoundEffectPlayed;
                nodeEditor.SoundtrackPlayed    -= NodeEditor_SoundtrackPlayed;

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        public void Initialize(Editor editor, List<NodeFunction> nodeFunctions, List<string> scriptFunctions)
        {
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            ReloadNodeFunctions(nodeFunctions);
            ReloadScriptFunctions(scriptFunctions);

            nodeEditor.Initialize(editor.Level, scriptFunctions);
            UpdateNodeEditorOptions();

            // HACK: Switch mode just for the display without actually loaded events
            rbLevelScript.Checked = _editor.Configuration.NodeEditor_DefaultEventMode == 0;

            nodeEditor.ViewPositionChanged += NodeEditor_ViewPostionChanged;
            nodeEditor.SelectionChanged  += NodeEditor_SelectionChanged;
            nodeEditor.LocatedItemFound  += NodeEditor_LocatedItemFound;
            nodeEditor.SoundEffectPlayed += NodeEditor_SoundEffectPlayed;
            nodeEditor.SoundtrackPlayed  += NodeEditor_SoundtrackPlayed;
        }

        private void NodeEditor_LocatedItemFound(object sender, EventArgs e)
        {
            if (sender is PositionBasedObjectInstance)
                _editor.ShowObject(sender as PositionBasedObjectInstance);
        }

        private void NodeEditor_SoundEffectPlayed(object sender, EventArgs e)
        {
            if (sender is string)
                WadSoundPlayer.PlaySoundInfo(_editor.Level, _editor.Level.Settings.GlobalSoundMap.FirstOrDefault(s => s.Name == (string)sender));
        }

        private void NodeEditor_SoundtrackPlayed(object sender, EventArgs e)
        {
            if (sender is string)
            {
                WadSoundPlayer.StopSample();
                WadSoundPlayer.PlaySoundtrack(_editor.Level, sender as string);
            }
        }

        private void NodeEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateNodeEditorControls();
        }

        private void NodeEditor_ViewPostionChanged(object sender, EventArgs e)
        {
            if (_event != null)
                _event.NodePosition = nodeEditor.ViewPosition;
        }

        private void UpdateNodeEditorOptions()
        {
            nodeEditor.GridSize = _editor.Configuration.NodeEditor_Size;
            nodeEditor.GridStep = _editor.Configuration.NodeEditor_GridStep;
            nodeEditor.DefaultNodeWidth = _editor.Configuration.NodeEditor_DefaultNodeWidth;
            nodeEditor.LinksAsRopes = _editor.Configuration.NodeEditor_LinksAsRopes;
            nodeEditor.ShowGrips = _editor.Configuration.NodeEditor_ShowGrips;
        }

        private void SelectTriggerMode()
        {
            tabbedContainer.SelectedIndex = rbLevelScript.Checked ? 1 : 0;

            UpdateNodeEditorControls();

            if (rbLevelScript.Checked)
                FindAndSelectFunction();

            if (!_lockUI && _event != null)
                _event.Mode = rbLevelScript.Checked ? EventSetMode.LevelScript : EventSetMode.NodeEditor;
        }

        private void UpdateNodes()
        {
            if (_event != null)
            {
                if (_event.NodePosition.X == float.MaxValue)
                    nodeEditor.ViewPosition = new Vector2(nodeEditor.GridSize / 2.0f);
                else
                    nodeEditor.ViewPosition = _event.NodePosition;
            }
            
            nodeEditor.Nodes = _event == null ? new List<TriggerNode>() : _event.Nodes;
        }

        private void UpdateUI()
        {
            tbArgument.Enabled   =
            nudCallCount.Enabled =
            nudCallCount2.Enabled =
            lstFunctions.Enabled = 
            nodeEditor.Enabled =
            rbLevelScript.Enabled =
            rbNodeEditor.Enabled =
            cbEnableEvent.Enabled =
            cbEnableEvent2.Enabled = _event != null;

            FindAndSelectFunction();

            if (_event == null)
                return;

            _lockUI = true;
            {
                rbLevelScript.Checked = _event.Mode == EventSetMode.LevelScript;
                rbNodeEditor.Checked = _event.Mode == EventSetMode.NodeEditor;
                tbArgument.Text = _event.Argument;
                nudCallCount.Value = _event.CallCounter;
                nudCallCount2.Value = _event.CallCounter;
                cbEnableEvent.Checked = _event.Enabled;
                cbEnableEvent2.Checked = _event.Enabled;

                UpdateNodeEditorControls();
            }
            _lockUI = false;
        }

        private void UpdateNodeEditorControls()
        {
            butChangeNodeColor.Enabled =
            butRenameNode.Enabled =
            butDeleteNode.Enabled = 
            butLockNodes.Enabled = nodeEditor.SelectedNodes.Count > 0;
            butClearNodes.Enabled = nodeEditor.LinearizedNodes().Count > 0;
            butLinkSelectedNodes.Enabled = nodeEditor.SelectedNodes.Count > 1;

            butExport.Enabled = nodeEditor.Nodes.Count > 0;

            butUnassign.Visible = rbLevelScript.Checked;

            // Stop any currently active sound previews on any UI update event
            WadSoundPlayer.StopSample();
        }

        private void ReloadScriptFunctions(List<string> scriptFunctions = null)
        {
            lstFunctions.Items.Clear();

            if (scriptFunctions != null && scriptFunctions.Count > 0)
            {
                scriptFunctions.ForEach(f => lstFunctions.Items.Add(new DarkUI.Controls.DarkListItem(f)));
            }
            else
            {
                lblListNotify.ForeColor = Colors.DisabledText;

                if (string.IsNullOrEmpty(_editor.Level.Settings.TenLuaScriptFile?.Trim() ?? string.Empty))
                {
                    lblListNotify.Tag = 1;
                    lblListNotify.Text = "Level script file is not specified." + "\n" +
                                         "Click here to load level script file.";
                }
                else
                {
                    string path = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile);

                    if (!File.Exists(path))
                    {
                        lblListNotify.Tag = 1;
                        lblListNotify.Text = "Level script file '" + Path.GetFileName(path) + "' not found." + "\n" +
                                             "Click here to choose replacement.";
                    }
                    else
                    {
                        scriptFunctions = ScriptingUtils.GetAllFunctionNames(path);
                        scriptFunctions.ForEach(f => lstFunctions.Items.Add(new DarkUI.Controls.DarkListItem(f)));

                        if (lstFunctions.Items.Count == 0)
                        {
                            lblListNotify.Tag = 0;
                            lblListNotify.Text = "Level script file does not have any level functions." + "\n" +
                                                 "They must have 'LevelFuncs.FuncName = function() ... end' format.";
                        }
                    }
                }
            }

            panelFunctionControls.Visible =
            lstFunctions.Visible =
            butUnassign.Enabled = lstFunctions.Items.Count > 0;
        }

        private void ReloadNodeFunctions(List<NodeFunction> nodeFunctions = null)
        {
            nodeEditor.NodeFunctions.Clear();

            if (nodeFunctions != null)
            {
                nodeEditor.NodeFunctions.AddRange(nodeFunctions);
                return;
            }

            nodeEditor.NodeFunctions.AddRange(ScriptingUtils.NodeFunctions);
        }

        private void FindAndSelectFunction()
        {
            if (lstFunctions.Items.Count == 0)
                return;

            if (_event == null || string.IsNullOrEmpty(_event.Function))
            {
                lblNotify.Visible = false;
                lstFunctions.ClearSelection();
                return;
            }

            for (int i = 0; i < lstFunctions.Items.Count; i++)
                if (lstFunctions.Items[i].Text == _event.Function)
                {
                    lstFunctions.ClearSelection();
                    lstFunctions.SelectItem(i);
                    return;
                }

            _lockUI = true;
            lstFunctions.ClearSelection();
            lblNotify.Text = "Not found: '" + _event.Function + "'";
            lblNotify.Visible = true;
            _lockUI = false;
        }

        private void rbLevelScript_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();
        private void rbNodeEditor_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();

        private void butSearch_Click(object sender, EventArgs e)
        {
            if (rbLevelScript.Checked)
            {
                var searchPopUp = new PopUpSearch(lstFunctions) { ShowAboveControl = true };
                searchPopUp.Show(this);
            }
            else if (rbNodeEditor.Checked)
            {
                using (var form = new FormInputBox("Find node", "Enter name of the node to find:", nodeEditor.SelectedNode?.Name ?? string.Empty))
                {
                    if (form.ShowDialog(FindForm()) == DialogResult.Cancel)
                        return;

                    nodeEditor.FindNodeByName(form.Result);
                }
            }
        }

        private void butUnassign_Click(object sender, EventArgs e)
        {
            lstFunctions.ClearSelection();
        }

        private void lstFunctions_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.Function = lstFunctions.SelectedItem?.Text ?? string.Empty;
            lblNotify.Visible = false;
        }

        private void nudCallCount_ValueChanged(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.CallCounter = (int)(sender as DarkNumericUpDown).Value;

            _lockUI = true;
            nudCallCount.Value = nudCallCount2.Value = _event.CallCounter;
            _lockUI = false;
        }

        private void tbArgument_Validated(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.Argument = tbArgument.Text;
        }

        private void lblListNotify_Click(object sender, EventArgs e)
        {
            if (lblListNotify.Tag == null || (int)lblListNotify.Tag == 0)
                return;

            string result = LevelFileDialog.BrowseFile(this, _editor.Level.Settings, _editor.Level.Settings.TenLuaScriptFile,
               "Select the LUA script file for this level", new[] { new FileFormat("LUA script file", "lua") },
               VariableType.LevelDirectory, false);

            if (result != null)
            {
                _editor.Level.Settings.TenLuaScriptFile = result;
                _editor.LoadedScriptsChange();
            }
        }

        private void lblListNotify_EnabledChanged(object sender, EventArgs e)
        {
            lblListNotify.Visible = lblListNotify.Enabled;
        }

        private void butAddConditionNode_Click(object sender, EventArgs e)
        {
            nodeEditor.AddConditionNode(Control.ModifierKeys != Keys.Control, Control.ModifierKeys == Keys.Alt);
        }

        private void butAddActionNode_Click(object sender, EventArgs e)
        {
            nodeEditor.AddActionNode(Control.ModifierKeys != Keys.Control, Control.ModifierKeys == Keys.Alt);
        }

        private void butClearNodes_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(FindForm(), "Do you really want to delete all nodes?", "Delete all nodes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            nodeEditor.ClearNodes();
        }

        private void butDeleteNode_Click(object sender, EventArgs e)
        {
            nodeEditor.DeleteNodes();
        }

        private void butRenameNode_Click(object sender, EventArgs e)
        {
            if (nodeEditor.SelectedNode == null)
                return;

            using (var form = new FormInputBox("Edit node name", "Enter new name for this node:", nodeEditor.SelectedNode.Name))
            {
                if (form.ShowDialog(FindForm()) == DialogResult.Cancel)
                    return;

                nodeEditor.SelectedNode.Name = form.Result;
                nodeEditor.Refresh();
            }
        }

        private void butChangeNodeColor_Click(object sender, EventArgs e)
        {
            if (nodeEditor.SelectedNode == null)
                return;

            nodeEditor.LockNodeChanges = true;

            using (var colorDialog = new RealtimeColorDialog(-1, -1, c =>
            {
                nodeEditor.SelectedNode.Color = c.ToFloat3Color();
                nodeEditor.Refresh();
            }))
            {
                var oldColor = nodeEditor.SelectedNode.Color.ToWinFormsColor();
                colorDialog.Color = oldColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                {
                    nodeEditor.SelectedNode.Color = oldColor.ToFloat3Color();
                    nodeEditor.Invalidate();
                }
                else if (oldColor != colorDialog.Color)
                {
                    nodeEditor.SelectedNode.Color = colorDialog.Color.ToFloat3Color();
                    nodeEditor.Invalidate();
                }
            }

            nodeEditor.LockNodeChanges = false;
        }

        private void butLinkSelectedNodes_Click(object sender, EventArgs e)
        {
            nodeEditor.LinkSelectedNodes();
        }

        public List<TriggerNode> CopyNodes(bool cut)
        {
            var result = new List<TriggerNode>();

            if (_event == null || _event.Mode == EventSetMode.LevelScript)
                return result;

            if (nodeEditor.SelectedNodes.Count == 0)
                return result;

            foreach (var node in nodeEditor.SelectedNodes.Where(n => !nodeEditor.SelectedNodes.Contains(n.Previous)))
                result.Add(node.Clone());

            if (cut)
                nodeEditor.DeleteNodes();

            return result;
        }

        public void PasteNodes(List<TriggerNode> nodes)
        {
            if (nodes == null || _event == null || _event.Mode == EventSetMode.LevelScript)
                return;

            if (nodes.Count == 0)
                return;

            foreach (var node in nodes)
            {
                if (nodeEditor.Nodes.Any(n => n.ScreenPosition == node.ScreenPosition))
                    node.ScreenPosition += new Vector2(nodeEditor.GridOffset, -nodeEditor.GridOffset);

                nodeEditor.Nodes.Add(node.Clone());
            }

            nodeEditor.UpdateVisibleNodes();
            nodeEditor.ShowNode(nodeEditor.Nodes.First());
        }

        public void ProcessKey(Keys keyCode)
        {
            if (_event == null)
                return;

            if (rbLevelScript.Checked)
            {
                switch (keyCode)
                {
                    case Keys.Delete:
                    case Keys.Back:
                        lstFunctions.ClearSelection();
                        break;
                }
            }
            else
            {
                switch (keyCode)
                {
                    case Keys.Delete:
                    case Keys.Back:
                        nodeEditor.DeleteNodes();
                        break;

                    case (Keys.Shift | Keys.Delete):
                    case (Keys.Shift | Keys.Back):
                        nodeEditor.ClearNodes();
                        break;

                    case Keys.N:
                    case Keys.A:
                        nodeEditor.AddActionNode(true, false);
                        break;

                    case Keys.C:
                        nodeEditor.AddConditionNode(true, false);
                        break;

                    case (Keys.Shift | Keys.N):
                    case (Keys.Shift | Keys.A):
                    case (Keys.Alt | Keys.N):
                    case (Keys.Alt | Keys.A):
                        nodeEditor.AddActionNode(true, true);
                        break;

                    case (Keys.Control | Keys.A):
                        nodeEditor.SelectAllNodes();
                        break;

                    case (Keys.Shift | Keys.C):
                    case (Keys.Alt | Keys.C):
                        nodeEditor.AddConditionNode(true, true);
                        break;

                    case Keys.L:
                        nodeEditor.LinkSelectedNodes();
                        break;

                    case Keys.Escape:
                        nodeEditor.ClearSelection();
                        break;
                }
            }
        }

        private void butExport_Click(object sender, EventArgs e)
        {
            var exportedNodes = ScriptingUtils.ParseNodes(nodeEditor.Nodes, "ExportedNodeFunction");

            if (string.IsNullOrEmpty(exportedNodes))
                return;

            Clipboard.SetText(exportedNodes);
            _editor.SendMessage("Node graph was successfully exported to Lua script\nand copied to clipboard.", PopupType.Info);
        }

        private void butLockNodes_Click(object sender, EventArgs e)
        {
            foreach (var node in nodeEditor.SelectedNodes)
                nodeEditor.LockNode(node, !node.Locked);
        }

        private void cbEnableEvent_CheckedChanged(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.Enabled = (sender as DarkCheckBox).Checked;

            _lockUI = true;
            cbEnableEvent.Checked = cbEnableEvent2.Checked = _event.Enabled;
            _lockUI = false;
        }
    }
}

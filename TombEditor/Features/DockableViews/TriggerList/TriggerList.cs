using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.Features.DockableViews.TriggerList
{
    public partial class TriggerList : DarkToolWindow
    {
        private readonly Editor _editor;

        public TriggerList()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update level-specific UI
            if (obj is Editor.InitEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                if (_editor.Level.IsTombEngine)
                    DockText = "Classic Triggers";
                else
                    DockText = "Triggers";
            }

            // Update the trigger control
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent)
                UpdateUI();

            // Update the trigger control selection
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent)
                UpdateSelection();

            // Update any modified trigger from area
            if (obj is Editor.ObjectChangedEvent)
            {
                var changedObject = ((Editor.ObjectChangedEvent)obj).Object;

                if (changedObject.Room == _editor.SelectedRoom &&
                    changedObject is TriggerInstance)
                {
                    UpdateUI();
                    UpdateSelection();
                }
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void DeleteTriggers()
        {
            if (_editor.SelectedRoom == null || lstTriggers.SelectedIndices.Count == 0)
                return;

            var triggersToRemove = new List<ObjectInstance>();
            foreach (var obj in lstTriggers.SelectedIndices)
            {
                var trigger = lstTriggers.Items[obj].Tag as ObjectInstance;
                if (trigger != null)
                    triggersToRemove.Add(trigger);
            }

            EditorActions.DeleteObjects(triggersToRemove, FindForm());
        }

        private void UpdateUI()
        {
            lstTriggers.Items.Clear();
            bool noSort = true;

            if (_editor.Level != null && _editor.SelectedSectors.Valid)
            {
                // Search for unique triggers inside the selected area
                var triggers = new List<TriggerInstance>();
                var area = _editor.SelectedSectors.Area;
                var origin = new VectorInt2(-1);

                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                        foreach (var trigger in _editor.SelectedRoom.GetSectorTry(x, z)?.Triggers ?? new List<TriggerInstance>())
                            if (!triggers.Contains(trigger))
                            {
                                // Look if incoming trigger doesn't belong to first sector in area.
                                // If that's the case, we're dealing with overlapping triggers and
                                // can't predict proper order for them, so we don't sort.

                                if (origin.X < 0) origin.X = x;
                                if (origin.Y < 0) origin.Y = z;
                                noSort = origin.X != x || origin.Y != z;
                                triggers.Add(trigger);
                            }

                if (triggers.Count == 1)
                    noSort = true; // Don't sort singular triggers

                if (triggers.Count > 0)
                {
                    // Sort triggers in same order as in compiled level, if area
                    if (!noSort)
                        TriggerInstance.SortTriggerList(ref triggers);

                    // Add triggers to listbox and highlight setup trigger if needed
                    for (int i = 0; i < triggers.Count; i++)
                    {
                        var trigger = triggers[i];
                        lstTriggers.Items.Add(new DarkListItem(trigger.ToShortString())
                        {
                            Tag = trigger,
                            TextColor = (!noSort && i == 0) ? Colors.BlueHighlight.Multiply(1.2f) : Colors.LightText
                        });
                    }
                }
            }
        }

        private void UpdateSelection()
        {
            if (_editor.SelectedObject is TriggerInstance)
            {
                var trigger = _editor.SelectedObject as TriggerInstance;
                var entry = lstTriggers.Items.FirstOrDefault(t => t.Tag == trigger);
                if (entry != null)
                {
                    lstTriggers.SelectItem(lstTriggers.Items.IndexOf(entry));
                    lstTriggers.EnsureVisible();
                }
                else
                    lstTriggers.ClearSelection();
            }
            else
                lstTriggers.ClearSelection();
        }

        private void lstTriggers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                DeleteTriggers();
        }

        private void butDeleteTrigger_Click(object sender, EventArgs e)
        {
            DeleteTriggers();
        }

        private void butEditTrigger_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.EditObject(_editor.SelectedObject, this);
        }

        private void lstTriggers_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || lstTriggers.SelectedItem == null)
                return;
            _editor.SelectedObject = (ObjectInstance)(lstTriggers.SelectedItem.Tag);
        }

        private void lstTriggers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstTriggers.SelectedIndices.Count == 0)
                return;

            var instance = lstTriggers.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }

        private void butAddTrigger_MouseEnter(object sender, EventArgs e)
        {
            if (_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
        }
    }
}

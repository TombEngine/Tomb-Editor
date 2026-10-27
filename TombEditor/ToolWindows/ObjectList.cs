using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class ObjectList : DarkToolWindow
    {
        private readonly Editor _editor;
        private bool _lockList = false;

        public ObjectList()
        {
            InitializeComponent();

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
            // Full rebuild when the displayed room or game version changes.
            if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.GameVersionChangedEvent)
            {
                RebuildObjectList();

                // Also sync selection state after a room switch.
                _lockList = true;
                lstObjects.ClearSelection();

                if (_editor.SelectedObject?.Room == _editor.SelectedRoom)
                    SelectObjectInList(_editor.SelectedObject);

                _lockList = false;
                return;
            }

            if (obj is Editor.ObjectChangedEvent)
            {
                var e = (Editor.ObjectChangedEvent)obj;

                // Events from other rooms are not visible in this panel.
                if (e.Room != _editor.SelectedRoom)
                    return;

                _lockList = true;

                switch (e.ChangeType)
                {
                    case ObjectChangeType.Add:
                        lstObjects.Items.Add(new DarkListItem(e.Object.ToShortString()) { Tag = e.Object });
                        break;

                    case ObjectChangeType.Remove:
                        var toRemove = lstObjects.Items.FirstOrDefault(i => i.Tag == e.Object);
                        if (toRemove != null)
                            lstObjects.Items.Remove(toRemove);
                        break;

                    case ObjectChangeType.Change:
                        var toUpdate = lstObjects.Items.FirstOrDefault(i => i.Tag == e.Object);
                        if (toUpdate != null)
                            toUpdate.Text = e.Object.ToShortString(); // TextChanged fires an incremental item update in DarkListView
                        else
                            RebuildObjectList(); // Item missing — fall back to full rebuild.
                        break;
                }

                _lockList = false;
                return;
            }

            // Update the object control selection
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                // Disable events
                _lockList = true;

                lstObjects.ClearSelection();

                if (_editor.SelectedObject?.Room == _editor.SelectedRoom)
                    SelectObjectInList(_editor.SelectedObject);

                _lockList = false;
            }
        }

        private void RebuildObjectList()
        {
            if (_editor.SelectedRoom == null)
                return;

            _lockList = true;

            var currentObject = lstObjects.SelectedItems.Count > 0 ? lstObjects.SelectedItem.Tag : null;

            // Build in a detached collection so DarkListView processes all items in one go.
            var allItems = new ObservableCollection<DarkListItem>();
            foreach (var o in _editor.SelectedRoom.Objects)
                allItems.Add(new DarkListItem(o.ToShortString()) { Tag = o });

            foreach (var o in _editor.SelectedRoom.GhostBlocks)
                allItems.Add(new DarkListItem(o.ToShortString()) { Tag = o });

            lstObjects.Items = allItems;

            // Restore selection.
            for (int i = 0; i < lstObjects.Items.Count; i++)
                if (lstObjects.Items[i].Tag == currentObject)
                {
                    lstObjects.SelectItem(i);
                    break;
                }

            _lockList = false;
        }

        private void SelectObjectInList(ObjectInstance obj)
        {
            if (obj is PositionBasedObjectInstance || obj is GhostBlockInstance)
            {
                var entry = lstObjects.Items.FirstOrDefault(t => t.Tag == obj);
                if (entry != null)
                {
                    lstObjects.SelectItem(lstObjects.Items.IndexOf(entry));
                    lstObjects.EnsureVisible();
                }
            }
        }

        private void lstObjects_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_lockList || lstObjects.SelectedItem == null ||
                _editor.SelectedRoom == null || _editor.SelectedObject is ObjectGroup)
                return;

            _editor.SelectedObject = (ObjectInstance)lstObjects.SelectedItem.Tag;
        }

        private void lstObjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var instances = lstObjects.SelectedItems.Select(o => o.Tag as ObjectInstance).ToList();
            EditorActions.DeleteObjects(instances, this);
        }

        private void butEditObject_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedItem == null)
                return;

            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }
    }
}

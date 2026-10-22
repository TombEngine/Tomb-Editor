using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Utils;
using System.ComponentModel;
using TombLib.LevelData;
using TombLib.Forms;

namespace TombEditor.Forms
{
    public partial class FormRoomProperties : DarkForm
    {
        private class RoomPropertyRow
        {
            public bool Replace { get; set; }
            public string DisplayName { get; set; }
            public string Name { get; set; }
        }

        private readonly List<RoomPropertyRow> _rows;
        private readonly Editor _editor;

        public FormRoomProperties(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Populate property list
            _rows = new List<RoomPropertyRow>();
            var names = TypeDescriptor.GetProperties(typeof(RoomProperties))
                                      .Cast<PropertyDescriptor>()
                                      .Select(p => new KeyValuePair<string, string>(p.Name, p.DisplayName));
            foreach (var n in names)
                if (!_rows.Any(r => r.DisplayName == n.Value))
                     _rows.Add(new RoomPropertyRow() { Replace = false, Name = n.Key, DisplayName = n.Value });

            dgvPropertyList.DataSource = new BindingList<RoomPropertyRow>(_rows);
            UpdateUI();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.SelectedRoomsChangedEvent ||
                obj is Editor.RoomListChangedEvent ||
                obj is Editor.LevelChangedEvent)
                UpdateUI();
        }

        private void UpdateUI() => butOk.Enabled = (_rows.Any(r => r.Replace) && _editor.SelectedRooms.Count > 1);

        private void butOk_Click(object sender, EventArgs e)
        {
            if (_rows.All(r => !r.Replace))
            {
                _editor.SendMessage("No properties were selected. Nothing was changed.", PopupType.Warning, true);
                return;
            }

            var undoList = new List<UndoRedoInstance>();
            var propInfo = typeof(RoomProperties).GetProperties();

            var curr = _editor.SelectedRoom;
            foreach (var r in _editor.SelectedRooms.Skip(1))
            {
                // Add this room to undo list
                undoList.Add(new RoomPropertyUndoInstance(_editor.UndoManager, r));

                // Clone current property so we don't reference same reference-type objects (e.g. room tags)
                var newProp = curr.Properties.Clone();

                // Scan through all collected properties and copy selected ones based on "DisplayName" attribute
                foreach (var row in _rows)
                {
                    if (!row.Replace) continue; // Don't copy properties not chosen to be replaced
                    foreach (var prop in propInfo)
                    {
                        var attribValue = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault();
                        if (attribValue is DisplayNameAttribute &&
                          ((attribValue as DisplayNameAttribute).DisplayName == row.DisplayName))
                        {
                            prop.SetValue(r.Properties, prop.GetValue(newProp));

                            // HACK: We need to rebuild lighting for rooms with changed AmbientLight property.
                            if (prop.Name == nameof(r.Properties.AmbientLight))
                                _editor.UpdateRoomLighting(r);
                        }
                    }
                }
                _editor.RoomPropertiesChange(r);
			}

            _editor.UndoManager.Push(undoList);
            _editor.SendMessage("Chosen room attributes were applied to selected rooms.", PopupType.Info, true);
        }

        private void dgvPropertyList_Click(object sender, EventArgs e) => UpdateUI();
        private void butCancel_Click(object sender, EventArgs e) => Close();

    }
}

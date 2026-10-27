using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using RateType = System.UInt64;
using ObjectType = System.Object;
using DarkUI.Config;
using DarkUI.Extensions;
using TombLib.Forms;

namespace TombEditor.Forms
{
    public partial class FormSearch : DarkForm
    {
        public enum ScopeMode
        {
            Everything,
            Rooms,
            AllObjects,
            ObjectsInSelectedRooms,
            ItemTypes,
            Triggers
        }

        private const int _trueRateBitCount = 16;
        private const int _matchBitShift = 64 - _trueRateBitCount;
        private static readonly Color _noTypoColor = Colors.LightText;
        private static readonly Color _smallTypoColor = Colors.LightText.Multiply(0.8f);
        private static readonly Color _bigTypoColor = Colors.LightText.Multiply(0.6f);
        private static readonly RateType _smallTypoThreshold = ((RateType)1 * 256) << _matchBitShift;
        private static readonly RateType _bigTypoThreshold = ((RateType)3 * 256) << _matchBitShift;

        private class RateTypeClass
        {
            public RateType _id = RateType.MaxValue;
        }

        private RateType _currentId;
        private readonly Editor _editor;
        private readonly ConditionalWeakTable<ObjectType, RateTypeClass> _baseObjectIdDictionary = new ConditionalWeakTable<ObjectType, RateTypeClass>();
        private Dictionary<ObjectType, RateType> _cachedRelevantObjects;
        private SortedDictionary<RateType, ObjectType> _cachedSortedObjects;
        private string _keyword = "";
        private bool _currentlyChangingRowCount;

        private readonly PopUpInfo popup = new PopUpInfo();

        public FormSearch(Editor editor)
        {
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;
            InitializeComponent();

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Populate scope combo box
            comboScope.Items.AddRange(Enum.GetValues(typeof(ScopeMode)).Cast<object>().ToArray());
            comboScope.SelectedItem = ScopeMode.Everything;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= _editor_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Rebuild object list if necessary
            if (_cachedRelevantObjects != null)
            {
                ScopeMode scope = (ScopeMode)comboScope.SelectedItem;
                if (obj is Editor.LoadedWadsChangedEvent && (scope == ScopeMode.ItemTypes || scope == ScopeMode.Everything))
                    ResetCompletely();
                if (obj is Editor.RoomListChangedEvent) // Always rebuild completely when rooms change for now.
                    ResetCompletely(); // We don't get precise object messages for objects removed with rooms otherwise.
                else if (obj is Editor.SelectedRoomChangedEvent && scope == ScopeMode.ObjectsInSelectedRooms)
                    ResetCompletely();
                else if (obj is IEditorObjectChangedEvent && (scope == ScopeMode.AllObjects || scope == ScopeMode.Everything ||
                    (scope == ScopeMode.ObjectsInSelectedRooms && _editor.SelectedRooms.Contains(((IEditorObjectChangedEvent)obj).Room)) ||
                    (scope == ScopeMode.Triggers && ((IEditorObjectChangedEvent)obj).Object is TriggerInstance)))
                {
                    var @event = (IEditorObjectChangedEvent)obj;
                    switch (@event.ChangeType)
                    {
                        case ObjectChangeType.Add:
                            AddObject(@event.Object);
                            break;
                        case ObjectChangeType.Change:
                            UpdateObject(@event.Object);
                            break;
                        case ObjectChangeType.Remove:
                            RemoveObject(@event.Object);
                            break;
                    }
                }
            }
        }

        private RateType RateObject(ObjectType obj)
        {
            // Get object identifier
            RateTypeClass idClass = _baseObjectIdDictionary.GetOrCreateValue(obj);
            if (idClass._id == RateType.MaxValue)
                idClass._id = _currentId++;

            // Get rate
            int startIndex;
            int levenshtein = Levenshtein.DistanceSubstring(obj.ToString().ToLower(), _keyword, out startIndex);
            RateType rate = (RateType)Math.Min(levenshtein * 256 + Math.Min(startIndex, 255), 1 << _trueRateBitCount);

            // Combine to result
            return idClass._id | (rate << _matchBitShift);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboScope_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                objectList.FirstDisplayedScrollingRowIndex = 0;
            }
            catch (Exception)
            {
                // ignored
            }

            ResetCompletely();
        }

        private void txtKeywords_TextChanged(object sender, EventArgs e)
        {
            _keyword = (txtKeywords.Text ?? "").ToLower();
            try
            {
                objectList.FirstDisplayedScrollingRowIndex = 0;
            }
            catch (Exception)
            {
                // ignored
            }

            ResetSort();
        }

        private void CurrentlyChangingRowCount(Action actionToPerform)
        {
            try
            {
                _currentlyChangingRowCount = true;
                actionToPerform();
            }
            finally
            {
                _currentlyChangingRowCount = false;
            }
        }

        private void AddObject(ObjectType obj)
        {
            RateType rate = RateObject(obj);
            _cachedRelevantObjects?.Add(obj, rate);
            _cachedSortedObjects?.Add(rate, obj);
            CurrentlyChangingRowCount(() => objectList.RowCount += 1);
            foreach (DataGridViewColumn column in objectList.Columns)
                objectList.InvalidateColumn(column.Index);
        }

        private void RemoveObject(ObjectType obj)
        {
            if (_cachedRelevantObjects == null)
                return;

            RateType rate;
            if (!_cachedRelevantObjects.TryGetValue(obj, out rate))
                return;

            _cachedRelevantObjects.Remove(obj);
            _cachedSortedObjects?.Remove(rate);

            CleanUp();

            foreach (DataGridViewColumn column in objectList.Columns)
                objectList.InvalidateColumn(column.Index);
            CurrentlyChangingRowCount(() => objectList.RowCount -= 1);
        }

        private void UpdateObject(ObjectType obj)
        {
            RemoveObject(obj);
            AddObject(obj);
        }

        private void CleanUp()
        {
            var deletedObjects = _cachedRelevantObjects.Keys.Where(o => o is ObjectInstance && ((ObjectInstance)o).Room == null).ToList();

            foreach (var obj in deletedObjects)
            {
                RateType rate;
                if (!_cachedRelevantObjects.TryGetValue(obj, out rate))
                    continue;
                _cachedRelevantObjects.Remove(obj);
                _cachedSortedObjects?.Remove(rate);
            }
        }

        private void ResetCompletely()
        {
            _cachedSortedObjects = null;
            _cachedRelevantObjects = null;
            foreach (DataGridViewColumn column in objectList.Columns)
                objectList.InvalidateColumn(column.Index);
        }

        private void ResetSort()
        {
            _cachedSortedObjects = null;
            foreach (DataGridViewColumn column in objectList.Columns)
                objectList.InvalidateColumn(column.Index);
        }

        private void BuildListNow()
        {
            // Collect relevant objects
            if (_cachedRelevantObjects == null)
            {
                _cachedRelevantObjects = new Dictionary<ObjectType, RateType>();
                _cachedSortedObjects = new SortedDictionary<RateType, ObjectType>();
                IEnumerable<ObjectType> relevantObjects = GetRelevantObjects((ScopeMode)comboScope.SelectedItem);
                foreach (ObjectType obj in relevantObjects)
                {
                    RateType rateType = RateObject(obj);
                    _cachedRelevantObjects.Add(obj, rateType);
                    _cachedSortedObjects.Add(rateType, obj);
                }

                CurrentlyChangingRowCount(() => objectList.RowCount = _cachedSortedObjects.Count);
                foreach (DataGridViewColumn column in objectList.Columns)
                    objectList.InvalidateColumn(column.Index);
            }

            // Update sorting
            if (_cachedSortedObjects == null)
            {
                _cachedSortedObjects = new SortedDictionary<RateType, ObjectType>();
                foreach (ObjectType obj in _cachedRelevantObjects.Keys.ToList())
                {
                    RateType rateType = RateObject(obj);
                    _cachedRelevantObjects[obj] = rateType;
                    _cachedSortedObjects.Add(rateType, obj);
                }
                CurrentlyChangingRowCount(() => objectList.RowCount = _cachedSortedObjects.Count);
                foreach (DataGridViewColumn column in objectList.Columns)
                    objectList.InvalidateColumn(column.Index);
            }
        }

        private void objectList_Paint(object sender, PaintEventArgs e)
        {
            BuildListNow();
        }

        private void objectList_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_cachedSortedObjects == null || e.RowIndex < 0 || e.RowIndex >= _cachedSortedObjects.Count)
                return;

            if (objectList.Columns[e.ColumnIndex].Name == objectListColumnName.Name)
            {
                KeyValuePair<RateType, ObjectType> entry = _cachedSortedObjects.ElementAt(e.RowIndex);
                if(entry.Value is Room)
                {
                    var room = (entry.Value as Room);
                    e.Value = room.Name;
                    if (room.Properties.Tags.Count > 0) e.Value += (" / " + string.Join(" ", room.Properties.Tags) + " /");
                }
                else
                {
                    e.Value = entry.Value.ToString();
                }
                
            }
            else if (objectList.Columns[e.ColumnIndex].Name == objectListColumnRoom.Name)
            {
                KeyValuePair<RateType, ObjectType> entry = _cachedSortedObjects.ElementAt(e.RowIndex);
                Room room = GetRoom(entry.Value);
                e.Value = room == null ? "<Unknown>" : _editor.Level.Rooms.ReferenceIndexOf(room) + ":   " + room.Name;
            }
            else if (objectList.Columns[e.ColumnIndex].Name == objectListColumnType.Name)
            {
                KeyValuePair<RateType, ObjectType> entry = _cachedSortedObjects.ElementAt(e.RowIndex);
                e.Value = GetType(entry.Value);
            }
        }

        private void objectList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_cachedSortedObjects == null || e.RowIndex < 0 || e.RowIndex >= _cachedSortedObjects.Count)
                return;

            KeyValuePair<RateType, ObjectType> entry = _cachedSortedObjects.ElementAt(e.RowIndex);
            if (entry.Key < _smallTypoThreshold)
            {
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = _noTypoColor;
                e.FormattingApplied = true;
            }
            else if (entry.Key < _bigTypoThreshold)
            {
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = _smallTypoColor;
                e.FormattingApplied = true;
            }
            else
            {
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = _bigTypoColor;
                e.FormattingApplied = true;
            }
        }

        private void objectList_SelectionChanged(object sender, EventArgs e)
        {
            if (objectList.SelectedRows.Count == 0 || _currentlyChangingRowCount)
                return;

            int rowIndex = objectList.SelectedRows[0].Index;
            if (_cachedSortedObjects == null || rowIndex < 0 || rowIndex >= _cachedSortedObjects.Count)
                return;

            KeyValuePair<RateType, ObjectType> entry = _cachedSortedObjects.ElementAt(rowIndex);
            SelectObject(entry.Value);
        }

        private Room GetRoom(ObjectType obj)
        {
            if (obj is Room)
                return (Room)obj;
            else if (obj is ObjectInstance)
                return ((ObjectInstance)obj).Room;
            else
                return null;
        }

        private string GetType(ObjectType obj)
        {
            string name = obj.GetType().Name;

            // Make name pretty
            if (name.EndsWith("Instance"))
                name = name.Substring(0, name.Length - "Instance".Length);
            return FormatProgrammingName(name);
        }

        private string FormatProgrammingName(string programmingName)
        {
            bool lastWasUpper = true;
            for (int i = 0; i < programmingName.Length; ++i)
            {
                bool isUpper = char.IsUpper(programmingName[i]);
                if (isUpper && !lastWasUpper)
                    programmingName = programmingName.Insert(i++, " ");
                lastWasUpper = isUpper;
            }

            return programmingName;
        }

        private void SelectObject(ObjectType obj)
        {
            if (obj is Room)
                _editor.SelectedRoom = (Room)obj;
            else if (obj is ObjectInstance)
                _editor.ShowObject((ObjectInstance)obj);
            else if (obj is ItemType itemType)
            {
                var wadObj = itemType.ToIWadObject(_editor.Level.Settings);
                if (wadObj != null)
                    _editor.ChosenItems = new[] { wadObj };
            }
        }

        private void DeleteObjects()
        {
            if (objectList.SelectedRows.Count == 0 || _currentlyChangingRowCount)
                return;

            var  roomsToDelete = new List<Room>();
            var  objectsToDelete = new List<ObjectInstance>();

            bool illegalOperation = false;

            for (int i = 0; i < objectList.SelectedRows.Count; i++)
            {
                int rowIndex = objectList.SelectedRows[i].Index;
                if (_cachedSortedObjects == null || rowIndex < 0 || rowIndex >= _cachedSortedObjects.Count)
                    continue;

                var entry = _cachedSortedObjects.ElementAt(rowIndex).Value;

                if (entry is Room)
                    roomsToDelete.Add((Room)entry);
                else if (entry is ISpatial || entry is TriggerInstance)
                    objectsToDelete.Add((ObjectInstance)entry);
                else
                    illegalOperation = true;
            }

            if ((objectsToDelete.Count > 0 || roomsToDelete.Count > 0) &&
                DarkMessageBox.Show(this, "Do you want to delete all selected entries? This action can't be undone.",
                                    "Delete selected entries", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                objectsToDelete = objectsToDelete.OrderBy(o => !(o is TriggerInstance)).ToList(); // Delete triggers first to avoid clashes
                EditorActions.DeleteObjects(objectsToDelete, null, false); // Delete objects first to avoid clashes
                EditorActions.DeleteRooms(roomsToDelete, null);

                objectList.ClearSelection();
            }

            if (illegalOperation)
                popup.ShowWarning(objectList, "Some objects weren't deleted because it's possible only from the map.");
        }

        private IEnumerable<ObjectType> GetRelevantObjects(ScopeMode scope)
        {
            // Rooms
            if (scope == ScopeMode.Everything || scope == ScopeMode.Rooms)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                    yield return room;
            }

            // Objects
            if (scope == ScopeMode.Everything || scope == ScopeMode.AllObjects)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                    foreach (var instance in room.AnyObjects)
                        yield return instance;
            }
            else if (scope == ScopeMode.ObjectsInSelectedRooms)
            {
                foreach (var instance in _editor.SelectedRooms.SelectMany(room => room.AnyObjects))
                    yield return instance;
            }

            // Item types
            if (scope == ScopeMode.Everything || scope == ScopeMode.ItemTypes)
            {
                foreach (var obj in _editor.Level.Settings.WadGetAllMoveables().Values)
                    yield return new ItemType(obj.Id, _editor.Level.Settings);
                foreach (var obj in _editor.Level.Settings.WadGetAllStatics().Values)
                    yield return new ItemType(obj.Id, _editor.Level.Settings);
            }

            // Triggers
            if (scope == ScopeMode.Triggers)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                    foreach (var instance in room.AnyObjects.Where(o => o is TriggerInstance))
                        yield return instance;
            }
        }

        private void butDelete_Click(object sender, EventArgs e) => DeleteObjects();

        private void FormSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteObjects();
        }
    }
}

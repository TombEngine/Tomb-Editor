using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Forms
{
    public partial class FormReplaceObject : DarkForm
    {
        private const string selectNewObjPrompt = " [ Select object in level or drag-n-drop it from item browser ]";

        private enum ObjectSelectionType
        {
            None,
            Source,
            Destination
        }

        private enum ObjectSearchType
        {
            PrimaryAttributeOnly,
            Full
        }

        private ObjectSelectionType SelectionType
        {
            get { return _selectionType; }
            set
            {
                // Block button and update labels to indicate that selection is in progress
                var selectDest = (value == ObjectSelectionType.Destination);
                butSelectDestObject.Enabled   = value == ObjectSelectionType.None || !selectDest;
                butSelectSourceObject.Enabled = value == ObjectSelectionType.None ||  selectDest;

                _selectionType = value;
                ToggleItem(_editor.SelectedObject as PositionBasedObjectInstance);
                UpdateLabels();
            }
        }
        private ObjectSelectionType _selectionType;

        private PositionBasedObjectInstance Source
        {
            get { return _source; }
            set
            {
                _source = (PositionBasedObjectInstance)value?.Clone();
                var newDesc = GetDescription((IReplaceable)_source);
                if (!string.IsNullOrEmpty(newDesc)) tbSourceObject.Text = newDesc;
                UpdateUI();
            }
        }
        private PositionBasedObjectInstance _source;

        private PositionBasedObjectInstance Dest
        {
            get { return _dest; }
            set
            {
                _dest = (PositionBasedObjectInstance)value?.Clone();
                var newDesc = GetDescription((IReplaceable)_dest);
                if (!string.IsNullOrEmpty(newDesc)) tbDestObject.Text = newDesc;
                UpdateUI();
            }
        }
        private PositionBasedObjectInstance _dest;

        private readonly Editor _editor;

        public FormReplaceObject(Editor editor, bool fromContext = false)
        {
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;
            InitializeComponent();

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Init UI
            InitializeNewSearch();

            // Switch to destination if we're in context mode
            if (fromContext)
            {
                _editor.SelectedObject = null;
                SelectionType = ObjectSelectionType.Destination;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= _editor_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Level has changed, reset everything
            if (obj is Editor.LevelChangedEvent)
                InitializeNewSearch();

            // Re-toggle current object on changed object selection
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                var evt  = (Editor.SelectedObjectChangedEvent)obj;
                if (evt.Current == null) return;
                ToggleItem((PositionBasedObjectInstance)evt.Current);
            }

            // Reset search if list of imported geos has changed and current search is imported geo
            if (obj is Editor.LoadedImportedGeometriesChangedEvent)
            {
                var evt = (Editor.LoadedImportedGeometriesChangedEvent)obj;

                if (Source != null && Source is ImportedGeometryInstance && !_editor.Level.Settings.ImportedGeometries.Contains(((ImportedGeometryInstance)Source).Model))
                    InitializeNewSearch();
                else if (Dest != null && Dest is ImportedGeometryInstance && !_editor.Level.Settings.ImportedGeometries.Contains(((ImportedGeometryInstance)Dest).Model))
                    InitializeNewSearch();
            }
        }

        private string GetDescription(IReplaceable instance)
        {
            if (instance == null) return string.Empty;

            string result;

            // HACK against TRTombalized ItemType madness!
            // ItemType automatically returns TR4 names by default, and it's impossible to fix without actually
            // recreating ItemType and derivative instance.

            if (instance is MoveableInstance)
                result = TrCatalog.GetMoveableName(_editor.Level.Settings.GameVersion, (instance as MoveableInstance).ItemType.MoveableId.TypeId);
            else if (instance is StaticInstance)
                result = TrCatalog.GetStaticName(_editor.Level.Settings.GameVersion, (instance as StaticInstance).ItemType.StaticId.TypeId);
            else
                result = ((ObjectInstance)instance).ToShortString();

            if (instance is MoveableInstance)
                result += " (" + instance.SecondaryAttribDesc + ": " + ((MoveableInstance)instance).Ocb + ")";
            else if (instance is StaticInstance && _editor.Level.IsNG)
                result += " (" + instance.SecondaryAttribDesc + ": " + ((StaticInstance)instance).Ocb + ")";
            else if (instance is SinkInstance)
                result += " (" + instance.PrimaryAttribDesc + ": " + (((SinkInstance)instance).Strength + 1) + ")";
            else if (instance is ImportedGeometryInstance)
                result += " (" + instance.SecondaryAttribDesc + ": " + ((ImportedGeometryInstance)instance).Scale.ToString(".0#") + ")";
            else if (instance is SoundSourceInstance)
                result += " (" + instance.PrimaryAttribDesc + ": " + ((SoundSourceInstance)instance).SoundId + ")";

            return result;
        }

        private void ToggleItem(PositionBasedObjectInstance item, ObjectSelectionType? selectionType = null)
        {
            var realSelectionType = selectionType.HasValue ? selectionType : SelectionType;
            var replItem = item as IReplaceable;
            if (replItem != null)
            {
                // Foolproofness to prevent selection of objects of different types.
                // Probably more smart approach is to reset opposite selection type, but IMO it's too intrusive. -- Lwmte
                
                switch (realSelectionType)
                {
                    case ObjectSelectionType.Destination:
                        if (Source != null && replItem != null && replItem.GetType() != Source.GetType())
                        {
                            InitializeNewSearch(false);
                            ToggleItem(item, realSelectionType);
                        }
                        else
                            Dest = item;
                        break;

                    case ObjectSelectionType.Source:
                        if (Dest != null && replItem != null && replItem.GetType() != Dest.GetType())
                        {
                            InitializeNewSearch(false);
                            ToggleItem(item, realSelectionType);
                            
                        }
                        else
                        {
                            var firstSearch = Source == null;
                            Source = item;
                            RepopulateUI();
                        }
                        break;
                }
            }
        }

        private void InitializeNewSearch(bool resetSelectionType = true)
        {
            Source = Dest = null;
            if (resetSelectionType) SelectionType = ObjectSelectionType.Source;
            RepopulateUI(true);
        }

        private void RepopulateUI(bool resetLabels = false)
        {
            lblResult.Text = string.Empty;
            if (resetLabels) UpdateLabels();
            cmbReplaceType.Items.Clear();
            cmbSearchType.Items.Clear();

            var primaryAttribDesc = ((IReplaceable)Source)?.PrimaryAttribDesc   ?? string.Empty;
            var secondAttribDesc  = ((IReplaceable)Source)?.SecondaryAttribDesc ?? string.Empty;

            var searchTypeList = new List<string>()
            {
                string.IsNullOrEmpty(primaryAttribDesc) ? string.Empty : "Only " + primaryAttribDesc,
                primaryAttribDesc + " and " + secondAttribDesc
            };

            // Populate search type combos
            foreach (var searchType in searchTypeList)
            {
                cmbReplaceType.Items.Add(searchType);
                cmbSearchType.Items.Add(searchType);
            }

            cmbReplaceType.SelectedIndex = cmbSearchType.SelectedIndex = 0;
        }

        private void UpdateUI()
        {
            butReplace.Enabled = (Source != null && Dest != null);

            // FIXME: These combo boxes should automatically enable or disable based on existence of secondary attrib,
            // but we have complications with light type (thanks TRTomb) and static type (thanks Paolone).
            // Both search / replacement type is disabled for statics in all game versions except TRNG as statics have OCBs there.
            // Sinks and sound sources have no additional parameters, therefore we block search type choice.
            cmbSearchType.Enabled  = !(Source == null ||
                                       Source is SinkInstance ||
                                       Source is SpriteInstance ||
                                       Source is SoundSourceInstance ||
                                       Source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG);

            // Additionally, light type can't be changed in runtime (thanks TRTomb?), so we block it as well for replace type choice.
            cmbReplaceType.Enabled = !(Source == null ||
                                       Source is SinkInstance ||
                                       Source is SpriteInstance ||
                                       Source is SoundSourceInstance ||
                                       Source is LightInstance ||
                                       Source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG);

            // Indicate source / dest light colour, if object type is light.

            if (Source is LightInstance)
            {
                colSource.BackColor = new Vector4(((LightInstance)Source).Color * 0.5f, 1.0f).ToWinFormsColor();
                colSource.Visible = true;
            }
            else
                colSource.Visible = false;

            if (Dest is LightInstance)
            {
                colDest.BackColor = new Vector4(((LightInstance)Dest).Color * 0.5f, 1.0f).ToWinFormsColor();
                colDest.Visible = true;
            }
            else
                colDest.Visible = false;
        }

        private void UpdateLabels()
        {
            if (SelectionType == ObjectSelectionType.Source)
            {
                if (_source == null) tbSourceObject.Text = selectNewObjPrompt;
                if (_dest == null) tbDestObject.Text = string.Empty;
            }
            else if (SelectionType == ObjectSelectionType.Destination)
            {
                if (_dest == null) tbDestObject.Text = selectNewObjPrompt;
                if (_source == null) tbSourceObject.Text = string.Empty;
            }
        }

        private void butSelectSourceObject_Click(object sender, EventArgs e) => SelectionType = ObjectSelectionType.Source;
        private void butSelectDestObject_Click(object sender, EventArgs e) => SelectionType = ObjectSelectionType.Destination;
        private void butCancel_Click(object sender, EventArgs e) => Close();
        private void butNewSearch_Click(object sender, EventArgs e) => InitializeNewSearch();

        private void butReplace_Click(object sender, EventArgs e)
        {
            int roomCount = 0;
            int replCount = 0;

            // Initialize source/dest replace objects
            var replSrc  = (IReplaceable)Source;
            var replDest = (IReplaceable)Dest;

            // Initialize undo list here not to clunk Undo.cs
            var undoList = new List<UndoRedoInstance>();

            foreach (var room in cbSelectedRooms.Checked ? _editor.SelectedRooms : _editor.Level.Rooms)
            {
                if (room == null) continue;
                bool anyObjectsChanged = false;

                var objectsToReplace = room.Objects.Where(item => item is IReplaceable && ((IReplaceable)item).ReplaceableEquals(replSrc, (cmbSearchType.SelectedIndex == (int)ObjectSearchType.Full)));
                foreach (IReplaceable obj in objectsToReplace)
                {
                    undoList.Add(new ChangeObjectPropertyUndoInstance(_editor.UndoManager, (PositionBasedObjectInstance)obj));

                    var objectChanged = obj.Replace(replDest, cmbReplaceType.SelectedIndex == (int)ObjectSearchType.Full);
                    if (objectChanged)
                    {
                        replCount++;
                        _editor.ObjectChange((ObjectInstance)obj, ObjectChangeType.Change);
                        anyObjectsChanged |= objectChanged;
                    }
                }

                if (anyObjectsChanged)
                {
                    roomCount++;
                    if (Source is LightInstance) _editor.UpdateRoomLighting(room);
                }
            }

            if (replCount > 0)
            {
                _editor.UndoManager.Push(undoList);
                lblResult.Text = "Replacement finished. Replaced " + replCount + " objects in " + roomCount + " room" + (roomCount > 1 ? "s" : string.Empty) + ".";
            }
            else
                lblResult.Text = "No matching objects found. No replacements were made.";
        }

        private void butSwapSrcDest_Click(object sender, EventArgs e)
        {
            var temp = Source;
            Source = Dest;
            Dest = temp;
        }

        // Convenient picker helper to replace color value right in window without selecting another light.
        private void colorPicker_Click(object sender, EventArgs e)
        {
            var panel = sender as DarkPanel;
            var light = panel == colSource ? (LightInstance)Source : (LightInstance)Dest;

            if (panel == null || light == null) return;

            using (var colorDialog = new RealtimeColorDialog(_editor.Configuration.UI_ColorScheme))
            {
                colorDialog.Color = panel.BackColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (panel.BackColor != colorDialog.Color)
                {
                    panel.BackColor = colorDialog.Color;
                    light.Color = colorDialog.Color.ToFloat3Color() * 2.0f;
                }
            }
        }

        private void FormReplaceObject_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as IWadObject) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void FormReplaceObject_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.Data.GetData(e.Data.GetFormats()[0]) as IWadObject) != null)
            {
                var item = e.Data.GetData(e.Data.GetFormats()[0]) as IWadObject;
                PositionBasedObjectInstance instance;

                if (item is WadStatic)
                    instance = new StaticInstance() { WadObjectId = ((WadStatic)item).Id };
                else if (item is WadMoveable)
                    instance = new MoveableInstance() { WadObjectId = ((WadMoveable)item).Id };
                else if (item is ImportedGeometry)
                    instance = new ImportedGeometryInstance() { Model = (ImportedGeometry)item };
                else
                    return;

                bool dest = e.Y >= PointToScreen(lblDest.Location).Y;
                ToggleItem(instance, dest ? ObjectSelectionType.Destination : ObjectSelectionType.Source);
            }
        }
    }
}
